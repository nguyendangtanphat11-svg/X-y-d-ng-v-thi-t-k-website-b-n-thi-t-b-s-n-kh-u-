using System.Web;
using System.Web.Mvc;

namespace Wedside_TVS_SFX
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
