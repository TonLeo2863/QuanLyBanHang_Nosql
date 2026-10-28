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
            try { var selector=new Newtonsoft.Json.Linq.JObject { ["type"]="delivery", ["status"]="shipping" };
                var actor=AppSecurity.Current(HttpContext);
                if(actor!=null && actor.role=="Shipper") selector["shipperId"]=actor.username;
                var page=await new CouchDbService().FindPage<Delivery>(selector,Request.QueryString["bookmark"],100);
                deliveries=page.docs; ViewBag.NextBookmark=page.docs.Count==100?page.bookmark:null; }
            catch (CouchDbException ex) { ViewBag.Error = ex.Message; }
            return View(deliveries);
        }
    }
}
