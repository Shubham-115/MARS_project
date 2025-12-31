using MARS_Project.CreateFilters;
using MARS_Project.Filters;
using MARS_Project.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MARS_Project.Controllers
{

    [RoleAuthorize("SuperAdmin,FairAdmin")]
    
    public class FairAdminController : Controller
    {
        private readonly IUsers users;
        private readonly IFair fair;
        public FairAdminController(IUsers user, IFair Fair)
        {
            users = user;
            fair = Fair;
        }
        [SessionAuthorize]
        public IActionResult Dashbord()
        {

            string email = HttpContext.Session.GetString("EmailID");
            //if (email != null)
            //{
            //    return RedirectToAction("Dashbord", "SuperAdmin");
            //}

            if (HttpContext == null)
                return RedirectToAction("Login", "User");


            if (email == null)
            {
                TempData["Failed"] = " Please Enter Your Email ";
                return RedirectToAction("Login", "User");
            }

            if (!users.UserStatus(email))
            {

                TempData["ChangePassword"] = "Please Change your Password first";
                HttpContext.Session.SetString("EmailID", email);

                return RedirectToAction("PasswordChange", "User");
            }
            ViewBag.EmailID = email;
            HttpContext.Session.SetString("EmailID", email);
            return View();
     
        }
        public IActionResult LogOut()
        {
            string email = HttpContext.Session.GetString("EmailID");
            // dal.setStatus(email, 0);
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "User");

        }
    }
}
