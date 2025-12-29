using MARS_Project.CreateFilters;
using MARS_Project.Models;
using MARS_Project.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MARS_Project.Controllers
{
    public class SuperAdminController : Controller

    {

        private readonly IUsers users;
        private readonly IFair fair;
        public SuperAdminController(IUsers user, IFair Fair)
        {
            users = user;
            fair = Fair;
        }

        [SessionAuthorize]
        public IActionResult Dashbord()
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
        }




        public IActionResult LogOut()
        {
            string email = HttpContext.Session.GetString("EmailID");
            // dal.setStatus(email, 0);
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "User");

        }


        [HttpGet]
        public async Task<IActionResult> CreateFair()
        {


            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateFair(Createfair model)
        {
            //if (!ModelState.IsValid)
            //    return View(model);

            try
            {
                // Handle File Upload
                if (model.FairLogoPath != null && model.FairLogoPath.Length > 0)
                {
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
                }
                else
                {
                    ModelState.AddModelError("FairLogoPath", "Logo is required");
                    return View(model);
                }

                // Default values
                model.CreatedAt = DateTime.UtcNow;
                model.Status = true;

                var result = await fair.CreateFair(model);

                if (!string.IsNullOrEmpty(result) && result == "SUCCESS")
                {
                    TempData["Success"] = "Fair created successfully!";
                    return RedirectToAction("Dashbord", "SuperAdmin");
                }

                TempData["Error"] = "Fair creation failed!";
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating fair: " + ex.Message);
                return View(model);
            }



        }
    }
}