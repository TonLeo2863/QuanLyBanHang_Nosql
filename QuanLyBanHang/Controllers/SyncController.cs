using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using QuanLyBanHang.Services;
namespace QuanLyBanHang.Controllers
{
    [AppAuthorize(Roles="Administrator")]
    public class SyncController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var docs=new List<JObject>();
            try { var page=await new CouchDbService().FindPage<JObject>(new JObject { ["type"]="order", ["pendingDelivery"]=new JObject { ["$type"]="object" } },Request.QueryString["bookmark"],20); docs=page.docs; ViewBag.NextBookmark=docs.Count==20?page.bookmark:null; }
            catch(CouchDbException ex) { ViewBag.Error=ex.Message; }
            return View(docs);
        }
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<ActionResult> Retry(string id)
        {
            if(string.IsNullOrWhiteSpace(id)) { TempData["Error"]="Thiếu mã đơn."; return RedirectToAction("Index"); }
            try { await new WorkflowService().Repair(id); }
            catch(CouchDbException ex) { TempData["Error"]=ex.Message; }
            return RedirectToAction("Index");
        }
    }
}
