using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly CouchDbService couchDbService;

        public ProductController()
        {
            couchDbService =
                new CouchDbService();
        }

        // =========================
        // DANH SÁCH SẢN PHẨM
        // =========================
        public async Task<ActionResult> Index()
        {
            string data =
                await couchDbService.GetAllDocuments();

            dynamic result =
                JsonConvert.DeserializeObject(data);

            List<Product> products =
                new List<Product>();

            foreach (dynamic row in result.rows)
            {
                if (row.doc != null)
                {
                    string type =
                        row.doc.type;

                    if (type == "product")
                    {
                        Product product =
                            JsonConvert.DeserializeObject<Product>(
                                row.doc.ToString()
                            );

                        products.Add(product);
                    }
                }
            }

            return View(products);
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
            Product product)
        {
            if (string.IsNullOrWhiteSpace(product._id))
            {
                ViewBag.Error =
                    "Mã sản phẩm không được để trống.";

                return View(product);
            }

            if (string.IsNullOrWhiteSpace(product.name))
            {
                ViewBag.Error =
                    "Tên sản phẩm không được để trống.";

                return View(product);
            }

            product.type = "product";

            string result =
                await couchDbService.CreateDocument(
                    product._id,
                    product
                );

            if (result.Contains("\"ok\":true"))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error =
                "Không thể thêm sản phẩm: "
                + result;

            return View(product);
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

            Product product =
                JsonConvert.DeserializeObject<Product>(
                    data
                );

            if (product == null)
            {
                return RedirectToAction("Index");
            }

            return View(product);
        }


        // =========================
        // POST: EDIT
        // =========================
        [HttpPost]
        public async Task<ActionResult> Edit(
            Product product)
        {
            if (string.IsNullOrWhiteSpace(product._id))
            {
                ViewBag.Error =
                    "Mã sản phẩm không được để trống.";

                return View(product);
            }

            if (string.IsNullOrWhiteSpace(product.name))
            {
                ViewBag.Error =
                    "Tên sản phẩm không được để trống.";

                return View(product);
            }

            product.type = "product";

            string result =
                await couchDbService.UpdateDocument(
                    product._id,
                    product
                );

            if (result.Contains("\"ok\":true"))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error =
                "Không thể cập nhật sản phẩm: "
                + result;

            return View(product);
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

            Product product =
                JsonConvert.DeserializeObject<Product>(
                    data
                );

            if (product == null)
            {
                return RedirectToAction("Index");
            }

            return View(product);
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