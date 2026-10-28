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
            try { documents = await couchDbService.FindDocuments<Delivery>("delivery"); }
            catch (CouchDbException ex) { ViewBag.Error = ex.Message; }
            return View(documents);
        }
        [HttpGet]
        public ActionResult Create() { return View(new Delivery()); }
        [HttpGet]
        public async Task<ActionResult> Edit(string id) { return await Load(id, "Edit"); }
        [HttpGet]
        public async Task<ActionResult> Delete(string id) { return await Load(id, "Delete"); }
        private async Task<ActionResult> Load(string id, string view)
        {
            try { return View(view, await couchDbService.ReadDocument<Delivery>(id, "delivery")); }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; return RedirectToAction("Index"); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Delivery delivery) { return await Save(delivery, false); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Delivery delivery) { return await Save(delivery, true); }
        private async Task<ActionResult> Save(Delivery delivery, bool editing)
        {
            var view = editing ? "Edit" : "Create";
            if (!ModelState.IsValid) return View(view, delivery);
            try
            {
                delivery.type = "delivery";
                if (editing) await couchDbService.ReadDocument<Delivery>(delivery._id, "delivery");

            await couchDbService.ReadDocument<Order>(delivery.orderId, "order");

                if (!ModelState.IsValid) return View(view, delivery);
                if (editing) await couchDbService.UpdateDocument(delivery._id, delivery);
                else await couchDbService.CreateDocument(delivery._id, delivery);
                return RedirectToAction("Index");
            }
            catch (CouchDbException ex) { ModelState.AddModelError("", ex.Message); return View(view, delivery); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirm(string id, string rev)
        {
            try
            {
                await couchDbService.ReadDocument<Delivery>(id, "delivery");
                await couchDbService.DeleteDocument(id, rev);
            }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Index");
        }
    }
}
