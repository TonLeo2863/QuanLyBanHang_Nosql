using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CouchDbService couchDbService;

        public CustomerController()
        {
            couchDbService = new CouchDbService();
        }

        // =========================
        // DANH SÁCH KHÁCH HÀNG
        // =========================
        public async Task<ActionResult> Index()
        {
            string data =
                await couchDbService.GetAllDocuments();

            dynamic result =
                JsonConvert.DeserializeObject(data);

            List<Customer> customers =
                new List<Customer>();

            foreach (dynamic row in result.rows)
            {
                if (row.doc != null)
                {
                    string type =
                        row.doc.type;

                    if (type == "customer")
                    {
                        Customer customer =
                            JsonConvert.DeserializeObject<Customer>(
                                row.doc.ToString()
                            );

                        customers.Add(customer);
                    }
                }
            }

            return View(customers);
        }


        // =========================
        // GET: CREATE
        // =========================
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }


        // =========================
        // POST: CREATE
        // =========================
        [HttpPost]
        public async Task<ActionResult> Create(
            Customer customer)
        {
            // Kiểm tra mã khách hàng
            if (string.IsNullOrWhiteSpace(customer._id))
            {
                ViewBag.Error =
                    "Mã khách hàng không được để trống.";

                return View(customer);
            }

            // Kiểm tra họ tên
            if (string.IsNullOrWhiteSpace(customer.name))
            {
                ViewBag.Error =
                    "Họ tên không được để trống.";

                return View(customer);
            }

            // Gán loại document
            customer.type = "customer";

            // Thêm vào CouchDB
            string result =
                await couchDbService.CreateDocument(
                    customer._id,
                    customer
                );

            // Nếu thêm thành công
            if (result.Contains("\"ok\":true"))
            {
                return RedirectToAction("Index");
            }

            // Nếu lỗi thì hiện lỗi thật của CouchDB
            ViewBag.Error =
                "Không thể thêm khách hàng: "
                + result;

            return View(customer);
        }


        // =========================
        // GET: EDIT
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

            Customer customer =
                JsonConvert.DeserializeObject<Customer>(
                    data
                );

            if (customer == null)
            {
                return RedirectToAction("Index");
            }

            return View(customer);
        }


        // =========================
        // POST: EDIT
        // =========================
        [HttpPost]
        public async Task<ActionResult> Edit(
            Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer._id))
            {
                ViewBag.Error =
                    "Mã khách hàng không được để trống.";

                return View(customer);
            }

            if (string.IsNullOrWhiteSpace(customer.name))
            {
                ViewBag.Error =
                    "Họ tên không được để trống.";

                return View(customer);
            }

            customer.type = "customer";

            string result =
                await couchDbService.UpdateDocument(
                    customer._id,
                    customer
                );

            if (result.Contains("\"ok\":true"))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error =
                "Không thể cập nhật khách hàng: "
                + result;

            return View(customer);
        }


        // =========================
        // GET: DELETE
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

            Customer customer =
                JsonConvert.DeserializeObject<Customer>(
                    data
                );

            if (customer == null)
            {
                return RedirectToAction("Index");
            }

            return View(customer);
        }


        // =========================
        // POST: DELETE
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