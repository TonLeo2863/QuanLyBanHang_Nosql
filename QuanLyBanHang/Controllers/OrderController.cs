using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class OrderController : Controller
    {
        private readonly CouchDbService couchDbService;

        public OrderController()
        {
            couchDbService =
                new CouchDbService();
        }

        // =========================
        // DANH SACH DON HANG
        // =========================

        public async Task<ActionResult> Index()
        {
            string data =
                await couchDbService.GetAllDocuments();

            dynamic result =
                JsonConvert.DeserializeObject(data);

            List<Order> orders =
                new List<Order>();

            foreach (dynamic row in result.rows)
            {
                if (row.doc != null)
                {
                    string type =
                        row.doc.type;

                    if (type == "order")
                    {
                        Order order =
                            JsonConvert.DeserializeObject<Order>(
                                row.doc.ToString()
                            );

                        orders.Add(order);
                    }
                }
            }

            return View(orders);
        }

        // =========================
        // THEM DON HANG - GET
        // =========================

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // =========================
        // THEM DON HANG - POST
        // =========================

        [HttpPost]
        public async Task<ActionResult> Create(
            Order order)
        {
            if (string.IsNullOrWhiteSpace(order._id))
            {
                ViewBag.Error =
                    "Mã đơn hàng không được để trống.";

                return View(order);
            }

            if (string.IsNullOrWhiteSpace(order.customerId))
            {
                ViewBag.Error =
                    "Mã khách hàng không được để trống.";

                return View(order);
            }

            if (order.items == null)
            {
                order.items =
                    new List<OrderItem>();
            }

            decimal total =
                0;

            foreach (OrderItem item in order.items)
            {
                total =
                    total
                    + (item.quantity * item.price);
            }

            order.total =
                total;

            order.type =
                "order";

            string result =
                await couchDbService.CreateDocument(
                    order._id,
                    order
                );

            if (result.Contains("\"ok\":true"))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error =
                "Không thể thêm đơn hàng: "
                + result;

            return View(order);
        }

        // =========================
        // SUA - GET
        // =========================

        [HttpGet]
        public async Task<ActionResult> Edit(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index");
            }

            string data =
                await couchDbService.GetDocument(id);

            Order order =
                JsonConvert.DeserializeObject<Order>(
                    data
                );

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // =========================
        // SUA - POST
        // =========================

        [HttpPost]
        public async Task<ActionResult> Edit(
            Order order)
        {
            if (string.IsNullOrWhiteSpace(order._id))
            {
                ViewBag.Error =
                    "Mã đơn hàng không được để trống.";

                return View(order);
            }

            if (string.IsNullOrWhiteSpace(order.customerId))
            {
                ViewBag.Error =
                    "Mã khách hàng không được để trống.";

                return View(order);
            }

            if (order.items == null)
            {
                order.items =
                    new List<OrderItem>();
            }

            decimal total =
                0;

            foreach (OrderItem item in order.items)
            {
                total =
                    total
                    + (item.quantity * item.price);
            }

            order.total =
                total;

            order.type =
                "order";

            string result =
                await couchDbService.UpdateDocument(
                    order._id,
                    order
                );

            if (result.Contains("\"ok\":true"))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error =
                "Không thể cập nhật đơn hàng: "
                + result;

            return View(order);
        }

        // =========================
        // XOA - GET
        // =========================

        [HttpGet]
        public async Task<ActionResult> Delete(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index");
            }

            string data =
                await couchDbService.GetDocument(id);

            Order order =
                JsonConvert.DeserializeObject<Order>(
                    data
                );

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // =========================
        // XOA - POST
        // =========================

        [HttpPost]
        public async Task<ActionResult> DeleteConfirm(
            string id,
            string rev)
        {
            if (!string.IsNullOrWhiteSpace(id) &&
                !string.IsNullOrWhiteSpace(rev))
            {
                await couchDbService.DeleteDocument(
                    id,
                    rev
                );
            }

            return RedirectToAction("Index");
        }

    }
}