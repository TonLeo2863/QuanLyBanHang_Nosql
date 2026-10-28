using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;
namespace QuanLyBanHang.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.CustomerCount = ViewBag.ProductCount = ViewBag.OrderCount = ViewBag.DeliveryCount = ViewBag.ShippingCount = 0;
            try
            {
                var service = new CouchDbService();
                ViewBag.CustomerCount = (await service.FindDocuments<Customer>("customer")).Count;
                ViewBag.ProductCount = (await service.FindDocuments<Product>("product")).Count;
                ViewBag.OrderCount = (await service.FindDocuments<Order>("order")).Count;
                var deliveries = await service.FindDocuments<Delivery>("delivery");
                ViewBag.DeliveryCount = deliveries.Count;
                ViewBag.ShippingCount = deliveries.Count(x => x.status == "shipping");
            }
            catch (CouchDbException ex) { ViewBag.Error = ex.Message; }
            return View();
        }
    }
}
