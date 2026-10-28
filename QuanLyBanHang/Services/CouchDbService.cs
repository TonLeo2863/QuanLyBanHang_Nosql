using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyBanHang.Services
{
    public class CouchDbService
    {
        private readonly HttpClient client;

        private readonly string couchDbUrl =
            "http://127.0.0.1:5984";

        private readonly string databaseName =
            "qlbanhang";

        private readonly string username =
            "admin123";

        private readonly string password =
            "123456";

        public CouchDbService()
        {
            client = new HttpClient();

            string login =
                username + ":" + password;

            string base64 =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(login)
                );

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    base64
                );
        }


        // ==========================================
        // LẤY TẤT CẢ DOCUMENT
        // ==========================================
        public async Task<string> GetAllDocuments()
        {
            string url =
                couchDbUrl
                + "/"
                + databaseName
                + "/_all_docs?include_docs=true";

            HttpResponseMessage response =
                await client.GetAsync(url);

            string result =
                await response.Content.ReadAsStringAsync();

            return result;
        }


        // ==========================================
        // LẤY 1 DOCUMENT
        // ==========================================
        public async Task<string> GetDocument(string id)
        {
            string url =
                couchDbUrl
                + "/"
                + databaseName
                + "/"
                + id;

            HttpResponseMessage response =
                await client.GetAsync(url);

            string result =
                await response.Content.ReadAsStringAsync();

            return result;
        }


        // ==========================================
        // THÊM DOCUMENT
        // ==========================================
        public async Task<string> CreateDocument(
            string id,
            object document)
        {
            string json =
                JsonConvert.SerializeObject(document);

            // Chuyển JSON thành JObject
            JObject jsonObject =
                JObject.Parse(json);

            // Khi tạo mới KHÔNG được gửi _rev
            jsonObject.Remove("_rev");

            json =
                jsonObject.ToString();

            StringContent content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

            string url =
                couchDbUrl
                + "/"
                + databaseName
                + "/"
                + id;

            HttpResponseMessage response =
                await client.PutAsync(
                    url,
                    content
                );

            string result =
                await response.Content.ReadAsStringAsync();

            return result;
        }


        // ==========================================
        // CẬP NHẬT DOCUMENT
        // ==========================================
        public async Task<string> UpdateDocument(
            string id,
            object document)
        {
            string json =
                JsonConvert.SerializeObject(document);

            StringContent content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

            string url =
                couchDbUrl
                + "/"
                + databaseName
                + "/"
                + id;

            HttpResponseMessage response =
                await client.PutAsync(
                    url,
                    content
                );

            string result =
                await response.Content.ReadAsStringAsync();

            return result;
        }


        // ==========================================
        // XÓA DOCUMENT
        // ==========================================
        public async Task<bool> DeleteDocument(
            string id,
            string rev)
        {
            string url =
                couchDbUrl
                + "/"
                + databaseName
                + "/"
                + id
                + "?rev="
                + rev;

            HttpResponseMessage response =
                await client.DeleteAsync(url);

            return response.IsSuccessStatusCode;
        }
    }
}