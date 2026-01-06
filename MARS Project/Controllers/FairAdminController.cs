using MARS_Project.CreateFilters;
using MARS_Project.Filters;
using MARS_Project.Models.FairAdmin;
using MARS_Project.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Threading.Tasks;

namespace MARS_Project.Controllers
{

    [RoleAuthorize("SuperAdmin,FairAdmin")]
    
    public class FairAdminController : Controller
    {
        private readonly IUsers users;
        private readonly IFair fair;
        private readonly IAddFair addFair;
        public FairAdminController(IUsers user, IFair Fair,IAddFair addfair)
        {
            users = user;
            fair = Fair;
            addFair = addfair;
        }
        [SessionAuthorize]
        public IActionResult Dashbord()
        {

            string email = HttpContext.Session.GetString("EmailID");
            //if (email != null)
            //{
            //    return RedirectToAction("Dashbord");
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

        public async Task<IActionResult> CreateSector()
        {
            string email = HttpContext.Session.GetString("EmailID");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("login", "User");
            }          

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSector(Sector sector)
        {
            if (!ModelState.IsValid)
            {
                return View(sector);
            }
            long fairId = await addFair.GetFairID(sector.EmailID);
            if (sector.FairID != fairId)
            {
                TempData["Invalid"] = "Fair not Exist";
                
                return View(sector);
            }
            int sectorID = await addFair.AddSector(sector);
            if (sectorID != 0)
            {
                TempData["Sector"] = "Sector inserted SuccessFully ......!";
                return RedirectToAction("CreateSector");
            }

            TempData["Sector"] = "Sector already Exist ......!";
            return View(sector);
            
        }
        [HttpPost]
        public async Task<IActionResult> ForgotFairID(string EmailID)
        {
            // 1. Email validation
            if (string.IsNullOrEmpty(EmailID))
            {
                TempData["Invalid"] = "Please enter Email ID";
                return RedirectToAction("CreateSector");
            }

            // 2. Get FairID from DB
            long fairId = await addFair.GetFairID(EmailID);

            // 3. Check if Fair exists
            if (fairId <= 0)
            {
                TempData["Invalid"] = "Fair does not exist";
                return RedirectToAction("CreateSector");
            }

            // 4. Success
            TempData["Sector"] = $"Your Fair ID is {fairId}";
            TempData["FairID"] = fairId;

            return RedirectToAction("CreateSector");
        }


    }
}
