using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace QuanLyBanHang.Services
{
    public class AppUser
    {
        public string _id { get; set; }
        public string _rev { get; set; }
        public string type { get; set; }
        public string username { get; set; }
        public string role { get; set; }
        public bool enabled { get; set; }
        public string salt { get; set; }
        public string passwordHash { get; set; }
        public int iterations { get; set; }
    }
    public class LoginInput
    {
        [Required, RegularExpression("^[a-z0-9][a-z0-9._-]{2,49}$")]
        public string Username { get; set; }
        [Required, DataType(DataType.Password), StringLength(128)]
        public string Password { get; set; }
    }
    public class CreateUserInput : LoginInput
    {
        [Required, RegularExpression("^(Administrator|OrderManager|Shipper)$")]
        public string Role { get; set; }
    }
    public static class AppSecurity
    {
        public const int Iterations = 600000;
        public static bool Strong(string password) { return password != null && password.Length >= 12 && password.Length <= 128; }
        public static AppUser NewUser(string name, string password, string role)
        {
            if (!Strong(password)) throw new CouchDbException("Mật khẩu phải có từ 12 đến 128 ký tự.");
            var salt = new byte[16]; using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            using (var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                return new AppUser { _id = "app-user-" + name, type = "app_user", username = name, role = role, enabled = true,
                    iterations = Iterations, salt = Convert.ToBase64String(salt), passwordHash = Convert.ToBase64String(derive.GetBytes(32)) };
        }
        public static bool Verify(AppUser user, string password)
        {
            if (user == null || !user.enabled || user.iterations < Iterations || user.iterations > 1000000 || password == null || password.Length > 128) return false;
            try {
                var salt = Convert.FromBase64String(user.salt); var expected = Convert.FromBase64String(user.passwordHash);
                if (salt.Length < 16 || expected.Length != 32) return false;
                using (var derive = new Rfc2898DeriveBytes(password, salt, user.iterations, HashAlgorithmName.SHA256)) {
                    var actual = derive.GetBytes(32); int difference = 0;
                    for (int i=0; i<32; i++) difference |= actual[i] ^ expected[i];
                    return difference == 0;
                }
            } catch (FormatException) { return false; }
        }
        public static AppUser Current(HttpContextBase context)
        {
            var user = context.Items["AppUser"] as AppUser;
            if (user != null) return user;
            if (!context.User.Identity.IsAuthenticated) return null;
            user = Task.Run(() => new CouchDbService().ReadDocument<AppUser>("app-user-" + context.User.Identity.Name, "app_user")).GetAwaiter().GetResult();
            if (!user.enabled) return null;
            context.Items["AppUser"] = user;
            return user;
        }
    }
    public class AppAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext context)
        {
            if (context.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) || context.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)) return;
            if (!context.HttpContext.User.Identity.IsAuthenticated) {
                context.Result = new RedirectResult("~/Account/Login"); return;
            }
            try {
                var user = AppSecurity.Current(context.HttpContext);
                if (user == null || (!string.IsNullOrEmpty(Roles) && !Roles.Split(',').Contains(user.role))) {
                    context.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
                    context.Result = new HttpStatusCodeResult(403, "Không đủ quyền");
                }
            } catch (CouchDbException) { context.Result = new HttpStatusCodeResult(503, "Không thể xác thực quyền truy cập"); }
        }
    }
}
