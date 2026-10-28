using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly CouchDbService couchDbService;

        public HomeController()
        {
            couchDbService =
                new CouchDbService();
        }

        public async Task<ActionResult> Index()
        {
            string data =
                await couchDbService.GetAllDocuments();

            dynamic result =
                JsonConvert.DeserializeObject(data);

            int customerCount = 0;
            int productCount = 0;
            int orderCount = 0;
            int deliveryCount = 0;
            int shippingCount = 0;

            foreach (dynamic row in result.rows)
            {
                if (row.doc != null)
                {
                    string type =
                        row.doc.type;

                    if (type == "customer")
                    {
                        customerCount++;
                    }

                    if (type == "product")
                    {
                        productCount++;
                    }

                    if (type == "order")
                    {
                        orderCount++;
                    }

                    if (type == "delivery")
                    {
                        deliveryCount++;

                        string status =
                            row.doc.status;

                        if (status == "shipping")
                        {
                            shippingCount++;
                        }
                    }
                }
            }

            ViewBag.CustomerCount =
                customerCount;

            ViewBag.ProductCount =
                productCount;

            ViewBag.OrderCount =
                orderCount;

            ViewBag.DeliveryCount =
                deliveryCount;

            ViewBag.ShippingCount =
                shippingCount;

            return View();
        }
    }
}