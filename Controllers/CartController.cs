using System.Web.Mvc;

namespace STAGEFX_PRO.Controllers
{
    public class CartController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int id, int quantity = 1)
        {
            if (id <= 0 || quantity <= 0)
                return new HttpStatusCodeResult(400);

            // TODO: Thêm sản phẩm vào giỏ hàng.
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            // TODO: Xóa sản phẩm khỏi giỏ hàng.
            return RedirectToAction("Index");
        }
    }
}