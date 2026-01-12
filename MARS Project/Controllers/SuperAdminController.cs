using MARS_Project.CreateFilters;
using MARS_Project.Filters;
using MARS_Project.Models;
using MARS_Project.Models.Citizen;
using MARS_Project.Models.SuperAdmin;
using MARS_Project.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace MARS_Project.Controllers
{
    [RoleAuthorize("SuperAdmin")]
    public class SuperAdminController : Controller

    {

        private readonly IUsers users;
        private readonly IFair fair;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SuperAdminController(IUsers user, IFair Fair, IWebHostEnvironment webHostEnvironment)
        {
            users = user;
            fair = Fair;
            _webHostEnvironment = webHostEnvironment;
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

        [SessionAuthorize]
        [HttpGet]
        public async Task<IActionResult> CreateFair()
        {
            string email = HttpContext.Session.GetString("EmailID");

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

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> CreateFair(Createfair model)
        {
            ModelState.Remove("FairLogoPathString");

            // File validation (manual)
            if (model.FairLogoPath == null || model.FairLogoPath.Length == 0)
            {
                ModelState.AddModelError("FairLogoPath", "Logo is required");
            }

            if (!ModelState.IsValid)
            {
                return View(model); // ⚠️ MUST pass model
            }

            try
            {
                // Upload file
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.FairLogoPath.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.FairLogoPath.CopyToAsync(stream);
                }

                model.FairLogoPathString = "/uploads/" + fileName;

                model.CreatedAt = DateTime.Now;
                model.Status = true;

                var result = await fair.CreateFair(model);

                if (result == "SUCCESS")
                {
                    TempData["Success"] = "Fair created successfully!";
                    return RedirectToAction("Dashbord", "SuperAdmin");
                }

                TempData["Error"] = "Fair creation failed!";
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }



        // write a Action to sate the fair status means the fair is Active or not
        [HttpGet]
        [SessionAuthorize]
        public IActionResult FairStatus()
        {
            return View();
        }
        [HttpPost]
        [SessionAuthorize]
        public async Task<IActionResult> FairStatusAsync(SetFairStatus status, string ActionType)
        {
            if (status == null)
                status = new SetFairStatus(); // safety

            if (ActionType == "CheckStatus")
            {
                ModelState.Remove("Status");
                if (!ModelState.IsValid)
                    return View(status);

                int result = await fair.CheckFairStatus(status);
                if (result < 0)
                {
                    TempData["Update"] = "Invlaid Crediantails !";
                    return RedirectToAction("FairStatus");
                }

                status.Status = result;

                return View(status);
            }

            if (ActionType == "Active" || ActionType == "Deactivate")
            {
                if (!ModelState.IsValid)
                    return View(status); // return model to avoid NullReference

                if ((status.FairId) < 0)
                {
                    TempData["Update"] = "Fair ID is required!";
                    return View(status);
                }

                // Set status based on ActionType
                status.Status = ActionType == "Active" ? 1 : 0;

                int result = await fair.SetFairStatus(status); // repository method
                TempData["Update"] = result == 0 ? "Please check your credentials" : "Status updated successfully!";

                return RedirectToAction("FairStatus");
            }

            // Default return to avoid null model
            return View();
        }


        public IActionResult FairAdmin()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> FairAdmin(SignUp signUp)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            int result = await fair.AddFairAdmin(signUp);
            if (result != 0)
            {
                TempData["Success"] = "Fair Admin Inserted Successfully........!";
                return RedirectToAction("Dashbord");
            }

            return View();
        }
      
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> UpdateFairSearch(int FairID, string EmailID)
        {
            if (FairID <= 0 || string.IsNullOrEmpty(EmailID))
            {
                TempData["Message"] = "Please provide valid Fair ID and Email ID.";
                return View("UpdateFair", new Createfair());
            }

            var fairDetails = await fair.GetFairDetails(FairID, EmailID);

            if (fairDetails == null || fairDetails.Count == 0)
            {
                TempData["Message"] = "No fair found with this ID and Email.";
                return View("UpdateFair", new Createfair());
            }

            return View("UpdateFair", fairDetails.First());
        }

        public async Task<IActionResult> UpdateFair()
        {
            return View();
        }
        // POST: update fair


        [HttpPost]
        public async Task<IActionResult> UpdateFair(Createfair model)
        {
            ModelState.Remove("FairLogoPathString");
            ModelState.Remove("FairLogoPath");

            if (!ModelState.IsValid)
                return View(model);

            // 🔹 If new logo uploaded → replace
            if (model.FairLogoPath != null && model.FairLogoPath.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/fairlogos");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(model.FairLogoPath.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.FairLogoPath.CopyToAsync(stream);
                }

                model.FairLogoPathString = "/uploads/fairlogos/" + fileName;
            }
            // ❗ ELSE DO NOTHING
            // Hidden field already contains old FairLogoPathString

            bool result = await fair.UpdateFair(model);

            TempData["Message"] = result
                ? "Fair updated successfully!"
                : "Something went wrong.";

            return RedirectToAction("UpdateFair");
        }

        public async Task<IActionResult> UpdateFairAdmin()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateFairAdmin(SignUp signUp)
        {
            return View();
        }
    }
}