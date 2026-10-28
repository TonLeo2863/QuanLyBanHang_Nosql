using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyBanHang.Services
{
    public class CouchDbException : Exception
    {
        public CouchDbException(string message) : base(message) { }
    }

    public class CouchDbService
    {
        private static readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
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
            var url = server.AbsoluteUri.TrimEnd('/') + "/" + Uri.EscapeDataString(Setting("COUCHDB_DATABASE")) + path;
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
                                case 404: throw new CouchDbException("Không tìm thấy database hoặc tài liệu. Chạy script khởi tạo nếu database chưa tồn tại.");
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
        public Task<string> GetAllDocuments() { return Send(HttpMethod.Get, "/_all_docs?include_docs=true"); }
        public Task<string> GetDocument(string id) { ValidateId(id); return Send(HttpMethod.Get, "/" + Uri.EscapeDataString(id)); }
        private static void ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.StartsWith("_")) throw new CouchDbException("Mã tài liệu không hợp lệ hoặc thuộc vùng dành riêng.");
        }
        public Task<string> CreateDocument(string id, object document)
        {
            ValidateId(id);
            var data = JObject.FromObject(document); data.Remove("_rev"); data["_id"] = id;
            return Send(HttpMethod.Put, "/" + Uri.EscapeDataString(id), data);
        }
        public Task<string> UpdateDocument(string id, object document)
        {
            ValidateId(id);
            var data = JObject.FromObject(document);
            if (string.IsNullOrWhiteSpace((string)data["_rev"])) throw new CouchDbException("Thiếu phiên bản dữ liệu. Hãy tải lại trang sửa.");
            data["_id"] = id;
            return Send(HttpMethod.Put, "/" + Uri.EscapeDataString(id), data);
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
