using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnCSN.Controllers
{
    public class BaseController : Controller
    {
        // GET: Admin/BaseAdmin
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["ID"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "User", action = "DangNhap" }
                    )
                );
            }
            base.OnActionExecuting(filterContext);
        }
    }
}