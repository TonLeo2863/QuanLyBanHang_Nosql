using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class DeliveryController : Controller
    {
        private readonly CouchDbService couchDbService;

        public DeliveryController()
        {
            couchDbService =
                new CouchDbService();
        }

        // =========================
        // DANH SACH GIAO HANG
        // =========================

        public async Task<ActionResult> Index()
        {
            string data =
                await couchDbService.GetAllDocuments();

            dynamic result =
                JsonConvert.DeserializeObject(data);

            List<Delivery> deliveries =
                new List<Delivery>();

            foreach (dynamic row in result.rows)
            {
                if (row.doc != null)
                {
                    string type =
                        row.doc.type;

                    if (type == "delivery")
                    {
                        Delivery delivery =
                            JsonConvert.DeserializeObject<Delivery>(
                                row.doc.ToString()
                            );

                        deliveries.Add(delivery);
                    }
                }
            }

            return View(deliveries);
        }

        // =========================
        // THEM GIAO HANG - GET
        // =========================

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // =========================
        // THEM GIAO HANG - POST
        // =========================

        [HttpPost]
        public async Task<ActionResult> Create(
            Delivery delivery)
        {
            if (string.IsNullOrWhiteSpace(delivery._id))
            {
                ViewBag.Error =
                    "Mã giao hàng không được để trống.";

                return View(delivery);
            }

            if (string.IsNullOrWhiteSpace(delivery.orderId))
            {
                ViewBag.Error =
                    "Mã đơn hàng không được để trống.";

                return View(delivery);
            }

            delivery.type =
                "delivery";

            string result =
                await couchDbService.CreateDocument(
                    delivery._id,
                    delivery
                );

            if (result.Contains("\"ok\":true"))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error =
                "Không thể thêm giao hàng: "
                + result;

            return View(delivery);
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

            Delivery delivery =
                JsonConvert.DeserializeObject<Delivery>(
                    data
                );

            if (delivery == null)
            {
                return RedirectToAction("Index");
            }

            return View(delivery);
        }

        // =========================
        // SUA - POST
        // =========================

        [HttpPost]
        public async Task<ActionResult> Edit(
            Delivery delivery)
        {
            if (string.IsNullOrWhiteSpace(delivery._id))
            {
                ViewBag.Error =
                    "Mã giao hàng không được để trống.";

                return View(delivery);
            }

            if (string.IsNullOrWhiteSpace(delivery.orderId))
            {
                ViewBag.Error =
                    "Mã đơn hàng không được để trống.";

                return View(delivery);
            }

            delivery.type =
                "delivery";

            string result =
                await couchDbService.UpdateDocument(
                    delivery._id,
                    delivery
                );

            if (result.Contains("\"ok\":true"))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error =
                "Không thể cập nhật giao hàng: "
                + result;

            return View(delivery);
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

            Delivery delivery =
                JsonConvert.DeserializeObject<Delivery>(
                    data
                );

            if (delivery == null)
            {
                return RedirectToAction("Index");
            }

            return View(delivery);
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