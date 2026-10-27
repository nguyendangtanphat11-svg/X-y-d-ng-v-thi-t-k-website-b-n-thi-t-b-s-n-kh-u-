
using System.Web.Mvc;

namespace STAGEFX_PRO.Controllers
{
    public class CheckoutController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder()
        {
            // TODO:
            // 1. Kiểm tra giỏ hàng.
            // 2. Kiểm tra thông tin khách hàng.
            // 3. Kiểm tra tồn kho và giá.
            // 4. Lưu đơn hàng vào database.
            // 5. Chuyển đến trang xác nhận khi thành công.

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Confirmation(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(400);

            // TODO: Tải đơn hàng từ database
            // và kiểm tra quyền truy cập của khách hàng.

            return View();
        }
    }
}
