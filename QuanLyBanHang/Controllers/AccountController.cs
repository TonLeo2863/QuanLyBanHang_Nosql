using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Security;
using QuanLyBanHang.Services;

namespace QuanLyBanHang.Controllers
{
    public class AccountController : Controller
    {
        private static readonly object loginLock = new object();
        [AllowAnonymous, HttpGet]
        public ActionResult Login() { return View(new LoginInput()); }
        [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginInput input)
        {
            if (!ModelState.IsValid) { ModelState.Remove("Password"); input.Password = null; return View(input); }
            var key = "login-attempt:" + input.Username;
            lock (loginLock) {
                int attempts = (int?)HttpRuntime.Cache[key] ?? 0;
                if (attempts >= 8) { ModelState.AddModelError("", "Quá nhiều lần đăng nhập. Thử lại sau 15 phút."); input.Password=null; ModelState.Remove("Password"); return View(input); }
                HttpRuntime.Cache.Insert(key, attempts + 1, null, DateTime.UtcNow.AddMinutes(15), Cache.NoSlidingExpiration);
            }
            try {
                var user = await new CouchDbService().ReadDocument<AppUser>("app-user-" + input.Username, "app_user");
                if (AppSecurity.Verify(user, input.Password)) {
                    HttpRuntime.Cache.Remove(key);
                    Session.Clear(); Session.Abandon();
                    FormsAuthentication.SetAuthCookie(user.username, false);
                    return RedirectToAction("Index", "Home");
                }
            } catch (CouchDbException) { /* Generic message prevents account enumeration and server disclosure. */ }
            input.Password=null; ModelState.Remove("Password");
            ModelState.AddModelError("", "Không thể đăng nhập. Kiểm tra thông tin hoặc liên hệ quản trị viên.");
            return View(input);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout() { FormsAuthentication.SignOut(); Session.Clear(); Session.Abandon(); return RedirectToAction("Login"); }
        [AppAuthorize(Roles="Administrator"), HttpGet]
        public async Task<ActionResult> Users()
        {
            try { ViewBag.Users = await new CouchDbService().FindDocuments<AppUser>("app_user"); }
            catch (CouchDbException ex) { ViewBag.Error = ex.Message; }
            return View(new CreateUserInput());
        }
        [AppAuthorize(Roles="Administrator"), HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Users(CreateUserInput input)
        {
            if (!AppSecurity.Strong(input.Password)) ModelState.AddModelError("Password", "Mật khẩu cần 12–128 ký tự.");
            if (ModelState.IsValid) try {
                await new CouchDbService().CreateDocument("app-user-"+input.Username, AppSecurity.NewUser(input.Username,input.Password,input.Role));
                return RedirectToAction("Users");
            } catch (CouchDbException ex) { ModelState.AddModelError("", ex.Message); }
            input.Password=null; ModelState.Remove("Password");
            return View(input);
        }
        [AppAuthorize(Roles="Administrator"), HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Disable(string id, string rev)
        {
            try {
                var db=new CouchDbService(); var user=await db.ReadDocument<AppUser>(id,"app_user");
                if(user.username==User.Identity.Name) throw new CouchDbException("Không thể khóa tài khoản đang đăng nhập.");
                if(user._rev!=rev) throw new CouchDbException("Tài khoản đã thay đổi. Tải lại danh sách.");
                user.enabled=false; await db.UpdateDocument(id,user);
            } catch(CouchDbException ex) { TempData["Error"]=ex.Message; }
            return RedirectToAction("Users");
        }
    }
}
