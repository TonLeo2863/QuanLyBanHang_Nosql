
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class ShippingController : Controller
    {
        private readonly CouchDbService couchDbService;

        public ShippingController()
        {
            couchDbService = new CouchDbService();
        }

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
                    string type = row.doc.type;

                    string status = row.doc.status;

                    if (type == "delivery" &&
                        status == "shipping")
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
    }
}