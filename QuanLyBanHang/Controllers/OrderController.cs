using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    [AppAuthorize(Roles="Administrator,OrderManager")]
    public class OrderController : Controller
    {
        private readonly CouchDbService couchDbService = new CouchDbService();
        public async Task<ActionResult> Index()
        {
            var documents = new List<Order>();
            try { documents = await QueryService.Load<Order>(this, couchDbService, "order"); }
            catch (CouchDbException ex) { ViewBag.Error = ex.Message; }
            return View(documents);
        }
        [HttpGet]
        public ActionResult Create() { return View(new Order()); }
        [HttpGet]
        public async Task<ActionResult> Edit(string id) { return await Load(id, "Edit"); }
        [HttpGet]
        public async Task<ActionResult> Delete(string id) { return await Load(id, "Delete"); }
        private async Task<ActionResult> Load(string id, string view)
        {
            try { var doc=await couchDbService.ReadDocument<Newtonsoft.Json.Linq.JObject>(id,"order"); ViewBag.History=doc["history"]; return View(view,doc.ToObject<Order>()); }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; return RedirectToAction("Index"); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Order order) { return await Save(order, false); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Order order) { return await Save(order, true); }
        private async Task<ActionResult> Save(Order order, bool editing)
        {
            var view = editing ? "Edit" : "Create";
            if (!ModelState.IsValid) return View(view, order);
            try
            {
                await new WorkflowService().SaveOrder(order, editing, User.Identity.Name);
                return RedirectToAction("Index");
            }
            catch (CouchDbException ex) { ModelState.AddModelError("", ex.Message); return View(view, order); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirm(string id, string rev)
        {
            try
            {
                await couchDbService.ReadDocument<Order>(id, "order");
                if (await couchDbService.HasReference("delivery", "orderId", id)) throw new CouchDbException("Đơn đã có phiếu giao; không thể xóa.");
                var current = await couchDbService.ReadDocument<Newtonsoft.Json.Linq.JObject>(id, "order");
                if (current["pendingDelivery"] is Newtonsoft.Json.Linq.JObject) throw new CouchDbException("Đơn đang chờ đồng bộ; không thể xóa.");
                await couchDbService.DeleteDocument(id, rev);
            }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Index");
        }
    }
}
