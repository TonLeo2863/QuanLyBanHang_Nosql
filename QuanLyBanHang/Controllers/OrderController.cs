using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class OrderController : Controller
    {
        private readonly CouchDbService couchDbService = new CouchDbService();
        public async Task<ActionResult> Index()
        {
            var documents = new List<Order>();
            try { documents = await couchDbService.FindDocuments<Order>("order"); }
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
            try { return View(view, await couchDbService.ReadDocument<Order>(id, "order")); }
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
                order.type = "order";
                if (editing) await couchDbService.ReadDocument<Order>(order._id, "order");

            System.DateTime parsedDate;
            if (!System.DateTime.TryParseExact(order.orderDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))
                ModelState.AddModelError("orderDate", "Ngày đặt hàng phải có dạng yyyy-MM-dd hợp lệ.");
            order.items = (order.items ?? new List<OrderItem>()).Where(x => x != null && !(string.IsNullOrWhiteSpace(x.productId) && x.quantity == 0 && x.price == 0)).ToList();
            if (order.items.Count == 0) ModelState.AddModelError("", "Đơn hàng cần ít nhất một sản phẩm.");
            decimal total = 0;
            foreach (var item in order.items)
            {
                if (string.IsNullOrWhiteSpace(item.productId) || item.quantity <= 0 || item.price < 0)
                    ModelState.AddModelError("", "Sản phẩm cần mã hợp lệ, số lượng dương và đơn giá không âm.");
                else
                {
                    await couchDbService.ReadDocument<Product>(item.productId, "product");
                    try { total = checked(total + checked(item.quantity * item.price)); }
                    catch (System.OverflowException) { ModelState.AddModelError("", "Tổng tiền vượt giới hạn cho phép."); }
                }
            }
            await couchDbService.ReadDocument<Customer>(order.customerId, "customer");
            order.total = total;

                if (!ModelState.IsValid) return View(view, order);
                if (editing) await couchDbService.UpdateDocument(order._id, order);
                else await couchDbService.CreateDocument(order._id, order);
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
                await couchDbService.DeleteDocument(id, rev);
            }
            catch (CouchDbException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction("Index");
        }
    }
}
