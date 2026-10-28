using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyBanHang.Services
{
    public class CouchDbException : Exception
    {
        public int StatusCode { get; private set; }
        public CouchDbException(string message, int statusCode = 0) : base(message) { StatusCode = statusCode; }
    }

    public class CouchDbService
    {
        private static readonly HttpClient sharedClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        private readonly HttpClient client;
        public CouchDbService() : this(sharedClient) { }
        public CouchDbService(HttpClient httpClient) { client = httpClient ?? throw new ArgumentNullException("httpClient"); }
        private static string Setting(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value)) throw new CouchDbException("Thiếu cấu hình CouchDB. Xem README để thiết lập biến môi trường.");
            return value;
        }
        private async Task<string> Send(HttpMethod method, string path, object body = null)
        {
            Uri server;
            if (!Uri.TryCreate(Setting("COUCHDB_URL"), UriKind.Absolute, out server) ||
                (server.Scheme != "http" && server.Scheme != "https") || !string.IsNullOrEmpty(server.UserInfo) ||
                server.AbsolutePath != "/" || !string.IsNullOrEmpty(server.Query) || !string.IsNullOrEmpty(server.Fragment))
                throw new CouchDbException("COUCHDB_URL phải là địa chỉ máy chủ HTTP/HTTPS không chứa thông tin đăng nhập.");
            if (server.Scheme == "http" && !server.IsLoopback)
                throw new CouchDbException("Kết nối CouchDB từ xa phải dùng HTTPS.");
            var database = Setting("COUCHDB_DATABASE");
            if (!Regex.IsMatch(database, @"^[a-z][a-z0-9_$()+/\-]*$"))
                throw new CouchDbException("Tên database CouchDB không hợp lệ.");
            var url = server.AbsoluteUri.TrimEnd('/') + "/" + Uri.EscapeDataString(database) + path;
            using (var request = new HttpRequestMessage(method, url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(Setting("COUCHDB_USERNAME") + ":" + Setting("COUCHDB_PASSWORD"))));
                if (body != null) request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                try
                {
                    using (var response = await client.SendAsync(request))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            switch ((int)response.StatusCode)
                            {
                                case 401: throw new CouchDbException("CouchDB từ chối đăng nhập. Kiểm tra tài khoản và mật khẩu cấu hình.");
                                case 403: throw new CouchDbException("Tài khoản CouchDB không có quyền thực hiện thao tác.");
                                case 404: throw new CouchDbException("Không tìm thấy database hoặc tài liệu. Chạy script khởi tạo nếu database chưa tồn tại.", 404);
                                case 409: throw new CouchDbException("Mã đã tồn tại hoặc dữ liệu đã được sửa bởi người khác. Tải lại dữ liệu trước khi thử lại.");
                                default: throw new CouchDbException("CouchDB không thể xử lý yêu cầu. Vui lòng thử lại sau.");
                            }
                        }
                        return await response.Content.ReadAsStringAsync();
                    }
                }
                catch (TaskCanceledException) { throw new CouchDbException("Kết nối CouchDB quá thời gian chờ."); }
                catch (HttpRequestException) { throw new CouchDbException("Không kết nối được CouchDB. Kiểm tra dịch vụ và địa chỉ cấu hình."); }
            }
        }
        public async Task<List<T>> FindDocuments<T>(string type, string status = null)
        {
            var selector = new JObject { ["type"] = type };
            if (status != null) selector["status"] = status;
            var documents = new List<T>();
            string bookmark = null;
            while (true)
            {
                var query = new JObject { ["selector"] = selector, ["limit"] = 200, ["sort"] = new JArray(new JObject { ["type"] = "asc" }, new JObject { ["_id"] = "asc" }) };
                if (bookmark != null) query["bookmark"] = bookmark;
                var json = await Send(HttpMethod.Post, "/_find", query);
                try
                {
                    var result = JsonConvert.DeserializeObject<FindResponse<T>>(json);
                    if (result == null || result.docs == null || result.docs.Exists(x => object.ReferenceEquals(x, null))) throw new JsonException();
                    documents.AddRange(result.docs);
                    if (result.docs.Count == 0) break;
                    if (string.IsNullOrEmpty(result.bookmark) || result.bookmark == bookmark)
                        throw new CouchDbException("CouchDB trả về phân trang không hợp lệ.");
                    bookmark = result.bookmark;
                }
                catch (JsonException) { throw new CouchDbException("Dữ liệu CouchDB trả về không hợp lệ."); }
            }
            return documents;
        }
        public class FindResponse<T>
        {
            public List<T> docs { get; set; }
            public string bookmark { get; set; }
        }
        public async Task<FindResponse<T>> FindPage<T>(JObject selector, string bookmark = null, int limit = 20)
        {
            if (limit < 1 || limit > 100) throw new CouchDbException("Kích thước trang không hợp lệ.");
            var query = new JObject { ["selector"] = selector, ["limit"] = limit,
                ["sort"] = new JArray(new JObject { ["type"] = "asc" }, new JObject { ["_id"] = "asc" }), ["use_index"] = "idx-type-id" };
            if (!string.IsNullOrWhiteSpace(bookmark)) query["bookmark"] = bookmark;
            try {
                var result = JsonConvert.DeserializeObject<FindResponse<T>>(await Send(HttpMethod.Post, "/_find", query));
                if (result == null || result.docs == null) throw new JsonException();
                return result;
            } catch (JsonException) { throw new CouchDbException("Dữ liệu phân trang không hợp lệ."); }
        }
        public async Task<bool> HasReference(string type, string field, string id, bool array = false)
        {
            var selector = new JObject { ["type"] = type };
            selector[array ? "items" : field] = array ? (JToken)new JObject { ["$elemMatch"] = new JObject { [field] = id } } : new JValue(id);
            return (await FindPage<JObject>(selector, null, 1)).docs.Count != 0;
        }
        public async Task<T> ReadDocument<T>(string id, string type)
        {
            try
            {
                var data = JObject.Parse(await GetDocument(id));
                if ((string)data["type"] != type) throw new CouchDbException("Tài liệu không thuộc loại dữ liệu cần thao tác.");
                return data.ToObject<T>();
            }
            catch (JsonException) { throw new CouchDbException("Nội dung tài liệu không hợp lệ."); }
        }
        public Task<string> GetDocument(string id) { ValidateId(id); return Send(HttpMethod.Get, "/" + Uri.EscapeDataString(id)); }
        private static void ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.StartsWith("_") || id == "." || id == "..") throw new CouchDbException("Mã tài liệu không hợp lệ hoặc thuộc vùng dành riêng.");
        }
        public Task<string> CreateDocument(string id, object document)
        {
            ValidateId(id);
            var data = JObject.FromObject(document); data.Remove("_rev"); data["_id"] = id;
            return Send(HttpMethod.Put, "/" + Uri.EscapeDataString(id), data);
        }
        public async Task<string> UpdateDocument(string id, object document)
        {
            ValidateId(id);
            var data = JObject.FromObject(document);
            if (string.IsNullOrWhiteSpace((string)data["_rev"])) throw new CouchDbException("Thiếu phiên bản dữ liệu. Hãy tải lại trang sửa.");
            JObject existing;
            try { existing = JObject.Parse(await GetDocument(id)); }
            catch (JsonException) { throw new CouchDbException("Nội dung tài liệu không hợp lệ."); }
            if ((string)existing["_rev"] != (string)data["_rev"])
                throw new CouchDbException("Dữ liệu đã thay đổi. Hãy tải lại trang sửa.");
            existing.Merge(data, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Merge });
            existing["_id"] = id;
            return await Send(HttpMethod.Put, "/" + Uri.EscapeDataString(id), existing);
        }
        public async Task<bool> DeleteDocument(string id, string rev)
        {
            ValidateId(id);
            if (string.IsNullOrWhiteSpace(rev)) throw new CouchDbException("Thiếu phiên bản dữ liệu. Hãy tải lại trang xóa.");
            await Send(HttpMethod.Delete, "/" + Uri.EscapeDataString(id) + "?rev=" + Uri.EscapeDataString(rev));
            return true;
        }
    }
}
