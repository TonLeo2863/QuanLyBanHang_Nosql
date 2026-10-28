using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly CouchDbService couchDbService = new CouchDbService();
        public async Task<ActionResult> Index()
        {
            var documents = new List<Product>();
            try { documents = await couchDbService.FindDocuments<Product>("product"); }
            catch (CouchDbException ex) { ViewBag.Error = ex.Message; }
            return View(documents);
        }
        [HttpGet]
        public ActionResult Create() { return View(new Product()); }
        [HttpGet]
        public async Task<ActionResult> Edit(string id) { return await Load(id, "Edit"); }
        [HttpGet]
        public async Task<ActionResult> Delete(string id) { return await Load(id, "Delete"); }
        private async Task<ActionResult> Load(string id, string view)
        {
            try { return View(view, await couchDbService.ReadDocument<Product>(id, "product")); }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; return RedirectToAction("Index"); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Product product) { return await Save(product, false); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Product product) { return await Save(product, true); }
        private async Task<ActionResult> Save(Product product, bool editing)
        {
            var view = editing ? "Edit" : "Create";
            if (!ModelState.IsValid) return View(view, product);
            try
            {
                product.type = "product";
                if (editing) await couchDbService.ReadDocument<Product>(product._id, "product");

                if (!ModelState.IsValid) return View(view, product);
                if (editing) await couchDbService.UpdateDocument(product._id, product);
                else await couchDbService.CreateDocument(product._id, product);
                return RedirectToAction("Index");
            }
            catch (CouchDbException ex) { ModelState.AddModelError("", ex.Message); return View(view, product); }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirm(string id, string rev)
        {
            try
            {
                await couchDbService.ReadDocument<Product>(id, "product");
                await couchDbService.DeleteDocument(id, rev);
            }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Index");
        }
    }
}
