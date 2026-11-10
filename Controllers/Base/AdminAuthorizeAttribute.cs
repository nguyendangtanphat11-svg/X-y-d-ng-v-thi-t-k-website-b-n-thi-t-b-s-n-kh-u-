using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using Wedside_TVS_SFX.Models;

namespace STAGEFX_PRO.Controllers.Base
{
    public sealed class AdminAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            object sessionUserId = filterContext.HttpContext.Session["MaNguoiDung"];
            int userId;

            if (sessionUserId == null ||
                !int.TryParse(sessionUserId.ToString(), out userId))
            {
                filterContext.Result = RedirectToLogin(filterContext);
                return;
            }

            NguoiDung user;

            try
            {
                using (var db = new StageFxEntities())
                {
                    user = db.NguoiDung
                        .AsNoTracking()
                        .FirstOrDefault(x => x.MaNguoiDung == userId);
                }
            }
            catch (Exception)
            {
                filterContext.Result = new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden);
                return;
            }

            if (user == null || !user.TrangThai)
            {
                ClearSession(filterContext);
                filterContext.Result = RedirectToLogin(filterContext);
                return;
            }

            if (user.MaVaiTro != 1)
            {
                filterContext.Result = new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden);
            }
        }

        private static ActionResult RedirectToLogin(
            AuthorizationContext filterContext)
        {
            string returnUrl = filterContext.HttpContext.Request.RawUrl;

            return new RedirectToRouteResult(
                new RouteValueDictionary(new
                {
                    controller = "Account",
                    action = "Login",
                    returnUrl = returnUrl
                }));
        }

        private static void ClearSession(AuthorizationContext filterContext)
        {
            try
            {
                filterContext.HttpContext.Session.Clear();
                filterContext.HttpContext.Session.Abandon();
            }
            catch (Exception)
            {
                // The authorization failure must remain safe if session cleanup fails.
            }
        }
    }
}
