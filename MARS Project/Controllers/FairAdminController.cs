using MARS_Project.CreateFilters;
using MARS_Project.Filters;
using MARS_Project.Models;
using MARS_Project.Models.Citizen;
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
        public async Task<IActionResult> Dashbord()
        {
            // 1️⃣ Get email from session
            string email = HttpContext.Session.GetString("EmailID");

            // 2️⃣ Check if session exists
            if (string.IsNullOrEmpty(email))
            {
                TempData["Failed"] = "Please enter your Email";
                return RedirectToAction("Login", "User");
            }

            // 3️⃣ Check if user needs to change password
            if (!users.UserStatus(email))
            {
                TempData["ChangePassword"] = "Please change your password first";
                HttpContext.Session.SetString("EmailID", email);
                return RedirectToAction("PasswordChange", "User");
            }

            // 4️⃣ Store email in ViewBag and session
            ViewBag.EmailID = email;
            HttpContext.Session.SetString("EmailID", email);

            // 5️⃣ Fetch profile from DB (replace with your method)
            Myprofile profile = new Myprofile { Email = email };
            profile = await users.profile(profile); // async DB call returning Myprofile

            // 6️⃣ Pass profile model to view
            return View(profile);
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



        [HttpGet]
        public async Task<IActionResult> AddSubSector()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> AddSubSector(Subsector subsector)
        {
            if (!ModelState.IsValid)
            {
                return View(subsector);
            }
           string message = await addFair.AddSubSector(subsector);
            TempData["insertResult"] = message;
            return RedirectToAction("AddSubSector");
        }



        [HttpGet]
        public async Task<IActionResult> AddBlock()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult>AddBlock(Block block)
        {
            if(!ModelState.IsValid)
            return View(block);

            string result = await addFair.AddBlock(block);

            TempData["insertResult"] = result;
            return RedirectToAction("AddBlock");
        }

        [HttpGet]
        public IActionResult UpdateSector()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult>UpdateSector(Sector sector , string ActionType)
        {
            if(ActionType == "GetSector")
            {
               var sectorDetails = await addFair.getSetor(sector.SectorID,sector.FairID);
                if(sectorDetails == null)
                {
                    TempData["SectorUpdateMSG"] = "INvalid SectorID or FairID";
                    return RedirectToAction("UpdateSector");
                }
                return View("UpdateSector",sectorDetails);
            }

            if(ActionType == "UpdateSector")
            {

                int updateResutlt = await addFair.UpdateSector(sector);
                if (updateResutlt > 0)
                {
                    TempData["SectorUpdateMSG"] = "Sector Updated SuccessFully .....!";
                    return RedirectToAction("UpdateSector");
                }
                return View();
            }
            TempData["SectorUpdateMSG"] = "INvalid SectorID or FairID";
            return RedirectToAction("UpdateSector");
        }

        [HttpGet]
        public IActionResult UpdateSubSector()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSubSector(Subsector Subsector, string ActionType)
        {
            if (ActionType == "GetSubSector")
            {
                var sectorDetails = await addFair.getSubSetor(Subsector.SectorID, Subsector.SubSectorID);
                if (sectorDetails == null)
                {
                    TempData["SubSectorUpdateMSG"] = "INvalid SectorID or SubSectorID";
                    return RedirectToAction("UpdateSubSector");
                }
                return View("UpdateSubSector", sectorDetails);
            }

            if (ActionType == "UpdateSubSector")
            {

                int updateResutlt = await addFair.UpdateSubSector(Subsector);
                if (updateResutlt > 0)
                {
                    TempData["SubSectorUpdateMSG"] = "Sector Updated SuccessFully .....!";
                    return RedirectToAction("UpdateSubSector");
                }
                return View();
            }
            TempData["SubSectorUpdateMSG"] = "INvalid SectorID or FairID";
            return RedirectToAction("UpdateSubSector");
        }


    }
}
