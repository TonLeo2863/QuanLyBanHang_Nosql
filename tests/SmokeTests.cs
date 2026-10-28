using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Services;
using QuanLyBanHang.Models;
using QuanLyBanHang.Controllers;

class Stub : HttpMessageHandler
{
    public Func<HttpRequestMessage, HttpResponseMessage> Reply;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken token)
    { return Task.FromResult(Reply(req)); }
}
class SmokeTests
{
    static int count;
    static bool Valid(object model) { return Validator.TryValidateObject(model, new ValidationContext(model), new List<ValidationResult>(), true); }
    static void Check(bool condition, string label) { if (!condition) throw new Exception(label); count++; }
    static HttpResponseMessage Response(int status, string json) { return new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(json) }; }
    static void Error(Action action, string label)
    {
        try { action(); throw new Exception("Expected controlled error: " + label); }
        catch (CouchDbException ex) { Check(!ex.Message.Contains("SENSITIVE"), label); }
    }
    static int Main() { try { Run(); return 0; } catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; } }
    static void Run()
    {
        // Chỉ cấu hình trong tiến trình kiểm thử; không dùng CouchDB thật.
        Environment.SetEnvironmentVariable("COUCHDB_URL", "http://127.0.0.1:5984");
        Environment.SetEnvironmentVariable("COUCHDB_DATABASE", "test-demo");
        Environment.SetEnvironmentVariable("COUCHDB_USERNAME", "fake-test-user");
        Environment.SetEnvironmentVariable("COUCHDB_PASSWORD", "fake-test-value");
        var stub = new Stub();
        using (var client = new HttpClient(stub))
        {
            var db = new CouchDbService(client);
            foreach (var code in new[] {401,403,404,409,500})
            {
                stub.Reply = r => Response(code, "SENSITIVE");
                Error(() => db.GetDocument("x").GetAwaiter().GetResult(), "HTTP " + code);
            }
            stub.Reply = r => { throw new TaskCanceledException(); };
            Error(() => db.GetDocument("x").GetAwaiter().GetResult(), "timeout");
            stub.Reply = r => { throw new HttpRequestException("SENSITIVE"); };
            Error(() => db.GetDocument("x").GetAwaiter().GetResult(), "connection");
            stub.Reply = r => Response(200, "{}");
            Error(() => db.FindDocuments<Product>("product").GetAwaiter().GetResult(), "missing docs");
            stub.Reply = r => Response(200, "not json");
            Error(() => db.FindDocuments<Product>("product").GetAwaiter().GetResult(), "invalid JSON");
            stub.Reply = r => Response(200, "{\"docs\":[],\"bookmark\":\"end\"}");
            Check(db.FindDocuments<Product>("product").GetAwaiter().GetResult().Count == 0, "empty database");
            int page = 0;
            stub.Reply = r => {
                var body = r.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Check(body.Contains("selector") && body.Contains("sort"), "Mango request");
                page++;
                if (page == 1) return Response(200, "{\"docs\":[{\"_id\":\"p1\"}],\"bookmark\":\"next\"}");
                Check(body.Contains("next"), "bookmark carried");
                return Response(200, "{\"docs\":[],\"bookmark\":\"end\"}");
            };
            Check(db.FindDocuments<Product>("product").GetAwaiter().GetResult().Count == 1 && page == 2, "pagination");
            stub.Reply = r => {
                Check(r.RequestUri.AbsoluteUri.ToLowerInvariant().Contains("a%2fb") && r.RequestUri.Query.ToLowerInvariant().Contains("1%2bx"), "encoded id and rev");
                return Response(200, "{\"ok\":true}");
            };
            Check(db.DeleteDocument("a/b", "1+x").GetAwaiter().GetResult(), "delete success");
            Error(() => db.UpdateDocument("x", new Product {_id="x"}).GetAwaiter().GetResult(), "missing rev");
            Error(() => db.CreateDocument("_design/x", new Product()).GetAwaiter().GetResult(), "reserved id");
            stub.Reply = r => {
                var body = r.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Check(!body.Contains("_rev"), "create removes rev");
                return Response(201, "{\"ok\":true}");
            };
            db.CreateDocument("x", new Product {_id="x", _rev="1-stale"}).GetAwaiter().GetResult();
            Error(() => db.GetDocument("..").GetAwaiter().GetResult(), "dot path id");
            stub.Reply = r => {
                if (r.Method == HttpMethod.Get) return Response(200, "{\"_id\":\"x\",\"_rev\":\"2-current\",\"type\":\"product\",\"custom\":42,\"tags\":[1,2]}");
                var body = r.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Check(body.Contains("custom") && body.Contains("42") && body.Contains("2-current"), "update preserves unknown fields and revision");
                return Response(201, "{\"ok\":true}");
            };
            db.UpdateDocument("x", new Product {_id="x", _rev="2-current", type="product", name="updated"}).GetAwaiter().GetResult();
            Error(() => db.UpdateDocument("x", new Product {_id="x", _rev="1-stale"}).GetAwaiter().GetResult(), "stale update");
            stub.Reply = r => Response(200, "{\"type\":\"customer\"}");
            Error(() => db.ReadDocument<Product>("x", "product").GetAwaiter().GetResult(), "wrong type");
        }
        foreach (var controller in new[] {typeof(CustomerController),typeof(ProductController),typeof(OrderController),typeof(DeliveryController)})
        foreach (var method in controller.GetMethods())
            if (Attribute.IsDefined(method, typeof(HttpPostAttribute)))
                Check(Attribute.IsDefined(method, typeof(ValidateAntiForgeryTokenAttribute)), "CSRF " + controller.Name + method.Name);
        Environment.SetEnvironmentVariable("COUCHDB_URL", null);
        using (var home = new HomeController())
        {
            Check(home.Index().GetAwaiter().GetResult() is ViewResult, "Home missing config returns view");
            Check(home.ViewBag.Error != null && (int)home.ViewBag.CustomerCount == 0, "Home warning and zero counts");
        }
        foreach (var controller in new Controller[] {new CustomerController(),new ProductController(),new OrderController(),new DeliveryController(),new ShippingController()})
        {
            var task = (Task<ActionResult>)controller.GetType().GetMethod("Index").Invoke(controller, null);
            Check(task.GetAwaiter().GetResult() is ViewResult && controller.ViewBag.Error != null, "list missing config " + controller.GetType().Name);
            controller.Dispose();
        }
        Check(!Valid(new Product {_id="x",name="x",price=-1,quantity=-1}), "negative product rejected");
        Check(!Valid(new Order {_id="x",customerId="c",orderDate="2026-10-28",status="unknown"}), "unknown status rejected");
        Console.WriteLine("PASS " + count + " assertions (HTTP gia lap; chua kiem thu CouchDB that).");
    }
}
