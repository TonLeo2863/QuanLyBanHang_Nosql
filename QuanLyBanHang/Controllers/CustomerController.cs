using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CouchDbService couchDbService = new CouchDbService();
        public async Task<ActionResult> Index()
        {
            var documents = new List<Customer>();
            try { documents = await couchDbService.FindDocuments<Customer>("customer"); }
            catch (CouchDbException ex) { ViewBag.Error = ex.Message; }
            return View(documents);
        }
        [HttpGet]
        public ActionResult Create() { return View(new Customer()); }
        [HttpGet]
        public async Task<ActionResult> Edit(string id) { return await Load(id, "Edit"); }
        [HttpGet]
        public async Task<ActionResult> Delete(string id) { return await Load(id, "Delete"); }
        private async Task<ActionResult> Load(string id, string view)
        {
            try { return View(view, await couchDbService.ReadDocument<Customer>(id, "customer")); }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; return RedirectToAction("Index"); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Customer customer) { return await Save(customer, false); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Customer customer) { return await Save(customer, true); }
        private async Task<ActionResult> Save(Customer customer, bool editing)
        {
            var view = editing ? "Edit" : "Create";
            if (!ModelState.IsValid) return View(view, customer);
            try
            {
                customer.type = "customer";
                if (editing) await couchDbService.ReadDocument<Customer>(customer._id, "customer");

                if (!ModelState.IsValid) return View(view, customer);
                if (editing) await couchDbService.UpdateDocument(customer._id, customer);
                else await couchDbService.CreateDocument(customer._id, customer);
                return RedirectToAction("Index");
            }
            catch (CouchDbException ex) { ModelState.AddModelError("", ex.Message); return View(view, customer); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirm(string id, string rev)
        {
            try
            {
                await couchDbService.ReadDocument<Customer>(id, "customer");
                await couchDbService.DeleteDocument(id, rev);
            }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Index");
        }
    }
}
