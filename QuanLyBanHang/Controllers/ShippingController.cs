using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models;
using QuanLyBanHang.Services;
namespace QuanLyBanHang.Controllers
{
    public class ShippingController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var deliveries = new List<Delivery>();
            try { deliveries = await new CouchDbService().FindDocuments<Delivery>("delivery", "shipping"); }
            catch (CouchDbException ex) { ViewBag.Error = ex.Message; }
            return View(deliveries);
        }
    }
}
