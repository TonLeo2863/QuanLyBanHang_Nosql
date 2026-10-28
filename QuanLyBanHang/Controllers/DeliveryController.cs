using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class DeliveryController : Controller
    {
        private readonly CouchDbService couchDbService = new CouchDbService();
        public async Task<ActionResult> Index()
        {
            var documents = new List<Delivery>();
            try { documents = await QueryService.Load<Delivery>(this, couchDbService, "delivery"); }
            catch (CouchDbException ex) { ViewBag.Error = ex.Message; }
            return View(documents);
        }
        [HttpGet]
        [AppAuthorize(Roles="Administrator,OrderManager")]
        public ActionResult Create() { return View(new Delivery()); }
        [HttpGet]
        public async Task<ActionResult> Edit(string id) { return await Load(id, "Edit"); }
        [HttpGet]
        [AppAuthorize(Roles="Administrator,OrderManager")]
        public async Task<ActionResult> Delete(string id) { return await Load(id, "Delete"); }
        private async Task<ActionResult> Load(string id, string view)
        {
            try { var doc=await couchDbService.ReadDocument<Delivery>(id, "delivery");
                var actor=AppSecurity.Current(HttpContext);
                if (actor.role=="Shipper" && doc.shipperId!=actor.username) return new HttpStatusCodeResult(403);
                return View(view,doc); }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; return RedirectToAction("Index"); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        [AppAuthorize(Roles="Administrator,OrderManager")]
        public async Task<ActionResult> Create(Delivery delivery) { return await Save(delivery, false); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Delivery delivery) { return await Save(delivery, true); }
        private async Task<ActionResult> Save(Delivery delivery, bool editing)
        {
            var view = editing ? "Edit" : "Create";
            if (!ModelState.IsValid) return View(view, delivery);
            try
            {
                await new WorkflowService().SaveDelivery(delivery, editing, AppSecurity.Current(HttpContext));
                return RedirectToAction("Index");
            }
            catch (CouchDbException ex) { ModelState.AddModelError("", ex.Message); return View(view, delivery); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        [AppAuthorize(Roles="Administrator,OrderManager")]
        public async Task<ActionResult> DeleteConfirm(string id, string rev)
        {
            try
            {
                await couchDbService.ReadDocument<Delivery>(id, "delivery");
                await new WorkflowService().DeleteDelivery(id, rev, User.Identity.Name);
            }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Index");
        }
    }
}
