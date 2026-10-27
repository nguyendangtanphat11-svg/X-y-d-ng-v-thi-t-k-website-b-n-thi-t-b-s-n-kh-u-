
using System.Web.Mvc;

namespace Wedside_TVS_SFX.Controllers
{
    public class ProductController : Controller
    {
        public ActionResult Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            ViewBag.ProductId = id;

            return View();
        }
    }
}
