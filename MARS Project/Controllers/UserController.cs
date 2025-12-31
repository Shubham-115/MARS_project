using MARS_Project.Connection;
using MARS_Project.CreateFilters;
using MARS_Project.DataSecurity;
using MARS_Project.Models.Citizen;
using MARS_Project.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Protocol;
using PortalLib.Framework.Utilities;
using System.Threading.Tasks;
using ForgotPassword = MARS_Project.Models.Citizen.ForgotPassword;
using Login = MARS_Project.Models.Citizen.Login;

namespace MARS_Project.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UserController : Controller
    {
        private readonly IUsers users;

        private readonly SecureData secure;
        private readonly TokenGenerate TknG;
        private readonly PortalEncryption encryptdecpt;
        public UserController(IUsers user)
        {
            users = user;// object for Users 

            secure = new SecureData();
            TknG = new TokenGenerate();
            encryptdecpt = new PortalEncryption();
        }
        public IActionResult Index()
        {
            return View();
        }

        // Get singUp Action Method
        [HttpGet]
        public IActionResult SignUp()
        {
            if (HttpContext.Session.GetString("EmailID") != null)
            {
                return RedirectToAction("Dashbord");
            }
            return View();
        }


        // SignUp post Action method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUp signUp)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (users.IsexistEmail(signUp.EmailID))
            {

                if (!await users.isVerifiedEmail(signUp.EmailID))
                {
                    string token = users.UpdateToken(signUp.EmailID);
                    if (token != null)
                    {
                        token = PortalEncryption.EncryptPassword(token);
                        string Email = PortalEncryption.EncryptPassword(signUp.EmailID.ToString());
                        string link = Url.Action("VerifyEmail", "User", new { token = token, EmailID = Email }, Request.Scheme);

                        TempData["msg2"] = $@"
<div class='text-center'>
    <h5 class='mb-3 text-primary'>
        <i class='bi bi-envelope-check'></i> Email Verification Required
    </h5>

    <p class='mb-2'>
        You'r Already Registered With Us .........!
    </p>

    <p class='mb-2 text-muted'>
        Please verify your email address to activate your account.
    </p>

    <p class='fw-semibold'>
        ⏰ This verification link is valid for <strong>24 hours</strong>.
    </p>

    <div class='d-grid gap-2 col-8 mx-auto mt-3'>
        <a href='{link}' class='btn btn-primary btn-lg'>
            Verify Email
        </a>
    </div>
</div>"; return RedirectToAction("VerifyLink");

                    }
                    return RedirectToAction("VerifyEmail");
                }

                if (!await users.isVerifiedMobile(signUp.MobileNo))
                {
                    TempData["notVerify"] = "Mobile is not verified please verify";
                    return RedirectToAction("VerifyMobile");
                }

                if (users.IsMobileExist(signUp.MobileNo))
                {
                    if (!await users.isVerifiedMobile(signUp.MobileNo))
                    {
                        TempData["msg"] = "User Already Exist !";
                        TempData["notVerify"] = "Mobile is not verified please verify";
                        return RedirectToAction("VerifyMobile");
                    }
                    TempData["msg"] = "User Already Exist ! Please Login ";
                    return RedirectToAction("signUp");
                }


                TempData["msg"] = "User Already Exist ! Please Login ";
                return RedirectToAction("Login");



            }

            // string token = TknG.GenerateToken();

            int result = await users.UserSingUp(signUp);
            if (result != 0)
            {
                string token = users.UpdateToken(signUp.EmailID);
                if (token != null)
                {
                    token = PortalEncryption.EncryptPassword(token);
                    string Email = PortalEncryption.EncryptPassword(signUp.EmailID.ToString());
                    string link = Url.Action("VerifyEmail", "User", new { token = token, EmailID = Email }, Request.Scheme);

                    TempData["msg2"] = $@"
<div class='text-center'>
    <h5 class='mb-3 text-primary'>
        <i class='bi bi-envelope-check'></i> Email Verification Required
    </h5>

    <p class='mb-2'>
        Registration Success full .........!
    </p>

    <p class='mb-2 text-muted'>
        Please verify your email address to activate your account.
    </p>

    <p class='fw-semibold'>
        ⏰ This verification link is valid for <strong>24 hours</strong>.
    </p>

    <div class='d-grid gap-2 col-8 mx-auto mt-3'>
        <a href='{link}' class='btn btn-primary btn-lg'>
            Verify Email
        </a>
    </div>
</div>";
                    return RedirectToAction("VerifyLink");

                }
            }
            TempData["Error"] = result;
            return View();

        }





        public IActionResult GenereateToken(string EmailID)
        {
            if (EmailID == null)
            {
                return RedirectToAction("Index");
            }
            //string token = Guid.NewGuid().ToString();
            EmailID = PortalEncryption.DecryptPassword(EmailID.ToString());
            string token = users.UpdateToken(EmailID);
            if (token != null)
            {
                token = PortalEncryption.EncryptPassword(token);
                EmailID = PortalEncryption.EncryptPassword(EmailID.ToString());
                string link = Url.Action("VerifyEmail", "User", new { token = token, EmailID = EmailID }, Request.Scheme);

                TempData["msg2"] = $"Reset Verifyfication link is valid for 24 hours !<br/>Click below to verify:<br/><a href='{link}'>{link}</a>";
                return RedirectToAction("VerifyLink");
            }

            TempData["Error"] = "Invalid Email ";
            return View();
        }





        public IActionResult Verifylink()
        {
            return View();
        }





        public IActionResult VerifyEmail(string token, string EmailID)
        {
            if (EmailID == null)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid token.");


            token = PortalEncryption.DecryptPassword(token);
            EmailID = PortalEncryption.DecryptPassword(EmailID.ToString());
            DateTime tokenTime = users.VerifyUser(token, EmailID);

            if (tokenTime != DateTime.MinValue)   // Token valid
            {
                EmailID = PortalEncryption.EncryptPassword(EmailID.ToString());

                string link1 = Url.Action("VerifyMobile", "User", Request.Scheme);
                TempData["EmailSuccess"] = " Email Verification Successful <br/>";
                HttpContext.Session.SetString("EmailID", EmailID);

                return RedirectToAction("VerifyMobile");
            }
            else   // Token invalid or expired
            {
                EmailID = PortalEncryption.DecryptPassword(EmailID.ToString());

                if (users.resetToken(EmailID))
                {
                    // token = PortalEncryption.EncryptPassword(token);
                    EmailID = PortalEncryption.EncryptPassword(EmailID.ToString());
                    TempData["msg1"] = "Link expired or invalid token.";
                    string link1 = Url.Action("GenereateToken", "User", new { EmailID = EmailID }, Request.Scheme);
                    TempData["Regenerate"] = $"Invalid Verification Token or Time Out!<br/>Click below to Resend link :<br/><a href='{link1}'class ='btn btn-danger'>Resend</a>";
                    return RedirectToAction("ViewMessages");
                }
            }
            return View();
        }




        [SessionAuthorize]
        public IActionResult VerifyMobile()
        {
            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyMobile(VerifyMobile mob, string ActionType)
        {
            string EmailID = HttpContext.Session.GetString("EmailID");
            if (EmailID == null)
            {
                return RedirectToAction("VerifyEmail");
            }
            if (ActionType == "Send_OTP")
            {
                ModelState.Remove("OTP");
                if (!ModelState.IsValid)
                {
                    return View(mob);
                }
                EmailID = PortalEncryption.DecryptPassword(EmailID.ToString());
                if (users.IsValidEmailAndMobile(EmailID, mob.MobileNo))
                {
                    string otp = users.GetOTP(mob.MobileNo);

                    TempData["OTP"] = $"OTP is valid for 5 Minutes<br/>" + otp;
                    TempData["StartOTPTimer"] = "true";
                    TempData["msgOTP"] = "OTP sent successfully!";
                    TempData["HideMobile"] = "true";
                    return View();
                }
                else
                {

                    TempData["Error"] = "Mobile number is not Exist";
                    return View();
                }
            }

            if (ActionType == "Submit")
            {
                if (!ModelState.IsValid)
                {
                    return View(mob);
                }

                if (!users.IsMobileExist(mob.MobileNo))
                {
                    TempData["Error"] = "Mobile number is not Exist";
                    return View();
                }
                DateTime OTPTime = users.VerifyOTP(mob.OTP, mob.MobileNo);
                if (OTPTime != DateTime.MinValue)   // Token valid
                {
                    TempData["msg2"] = $" Mobile Verification Successful ";

                    EmailID = PortalEncryption.DecryptPassword(EmailID.ToString());
                    string password = users.GenerateRandomString(8);
                    if (users.SetPassWord(EmailID, password))
                    {

                        EmailID = PortalEncryption.EncryptPassword(EmailID.ToString());
                        password = PortalEncryption.EncryptPassword(password.ToString());
                        string link = Url.Action("VerifyPassword", "User", new { EmailID = EmailID, Password = password }, Request.Scheme);

                        TempData["Verify"] = $@"Click here to Change Password ! <br/>Verification link is valid for 24 hours.<br/> Click below to verify:<br/><br/><a href='{link}'>Change Password</a>";
                        return RedirectToAction("ViewMessages");
                    }

                }
                else
                {
                    TempData["msg2"] = "OTP Expired Click to Resend OTP ";
                    return View(mob);
                }
            }

            return View(mob);
        }





        public IActionResult VerifyPassword(string EmailID, string Password)
        {
            EmailID = PortalEncryption.DecryptPassword(EmailID.ToString());
            Password = PortalEncryption.DecryptPassword(Password.ToString());
            Password = users.ConvertHashPassword(Password);
            if (users.VerifyEmailPassword(EmailID, Password))
            {
                HttpContext.Session.SetString("EmailID", EmailID);
                return RedirectToAction("Passwordchange");

            }
            // return RedirectToAction("Verify");
            return View();
        }



        [AllowAnonymous]
        public IActionResult Login()
        {
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Session check (ASP.NET Core way)
            if (HttpContext.Session.GetString("EmailID") != null)
            {
                return RedirectToAction("Index");
            }

            return View();
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync(Login login)
        {
            if (ModelState.IsValid)
            {

                //login.EmailID = secure.Encrypt(login.EmailID);
                string Email = login.EmailID;

                if (!users.IsexistEmail(Email))
                {

                    TempData["NotFound"] = "User Not Registered ...  ! please Register";
                    return RedirectToAction("Login");
                }

                if (!await users.isVerifiedEmail(login.EmailID))
                {
                    string token = users.UpdateToken(Email);
                    if (token != null)
                    {
                        token = PortalEncryption.EncryptPassword(token);
                        Email = PortalEncryption.EncryptPassword(Email.ToString());
                        string link = Url.Action("VerifyEmail", "User", new { token = token, EmailID = Email }, Request.Scheme);

                        TempData["msg2"] = $@"Registration Successful! <br/>Verification link is valid for 24 hours.<br/> Click below to verify:<br/><br/><a href='{link}' class='btn btn-primary'>Verify Email</a>";
                        return RedirectToAction("VerifyLink");

                    }
                }

                // convert the plain password string to HashPassword string

                int loginUser = await users.UserLogin(login);

                switch (loginUser)
                {
                    case 1:

                        HttpContext.Session.SetString("EmailID", Email);
                        HttpContext.Session.SetString("Role", "User");
                        users.UpdateLoginTime(Email);
                        return RedirectToAction("Dashbord", "User");
                        break;
                    case 2:
                        HttpContext.Session.SetString("EmailID", Email);
                        HttpContext.Session.SetString("Role", "SuperAdmin");
                        users.UpdateLoginTime(Email);
                        return RedirectToAction("Dashbord", "SuperAdmin");
                        break;
                    case 3:
                        HttpContext.Session.SetString("EmailID", Email);
                        HttpContext.Session.SetString("Role", "FairAdmin");
                        users.UpdateLoginTime(Email);
                        return RedirectToAction("Dashbord", "FairAdmin");
                        break;

                    default:

                        TempData["Error"] = "Invalid Email and Password";
                        return RedirectToAction("Login");
                }
            }
            return View("login");
        }



        public IActionResult ViewMessages()
        {
            return View();
        }





        public IActionResult ForgotPassword()
        {
            // return RedirectToAction("Verify");
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult ForgotPassword(ForgotPassword fp)
        {
            if (ModelState.IsValid)
            {

                // fp.EmailID = secure.Encrypt(fp.EmailID);
                // fp.MobileNo = secure.Encrypt(fp.MobileNo);
                if (users.IsValidEmailAndMobile(fp.EmailID, fp.MobileNo))
                {
                    //HttpContext.Session.SetString("EmailID", fp.EmailID);
                    users.setStatus(fp.EmailID, 0);
                    string token = users.UpdateToken(fp.EmailID);
                    if (token != null)
                    {
                        token = PortalEncryption.EncryptPassword(token);
                        string Email = PortalEncryption.EncryptPassword(fp.EmailID.ToString());
                        string link = Url.Action("VerifyEmail", "User", new { token = token, EmailID = Email }, Request.Scheme);

                        TempData["msg2"] = $@"Forgot Password ! <br/>Verification link is valid for 24 hours.<br/> Click below to verify:<br/><br/><a href='{link}' class='btn btn-primary'>Verify Email</a>";
                        return RedirectToAction("VerifyLink");

                    }
                }
                else
                {
                    TempData["NotExist"] = "Please Enter Correct Email and Mobile Number";
                    return View();
                }
                //return View();
            }
            return View();
        }





        [SessionAuthorize]
        public IActionResult Passwordchange()
        {


            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize]
        public IActionResult Passwordchange(PasswordChange passChange)
        {
            if (ModelState.IsValid)
            {
                string EmailID = HttpContext.Session.GetString("EmailID");
                if (EmailID == null)
                {

                }
                if (passChange.PasswordHash != passChange.ConfirmPassword)
                {
                    ViewData["MissMatch"] = " Password does not match ";
                    return View();
                }

                string result = users.PassWordChange(EmailID, passChange.PasswordHash);
                TempData["SuccessChange"] = result;
                users.setStatus(EmailID, 1);
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            return View();

        }






        [SessionAuthorize]
        public IActionResult Dashbord()
        {
            // string Encryptemail = HttpContext.Session.GetString("EmailID");
            // string email = secure.Decript(Encryptemail);
            string email = HttpContext.Session.GetString("EmailID");
           



            if (email == null)
            {
                TempData["Failed"] = " Please Enter Your Email ";
                return RedirectToAction("Login");
            }

            if (!users.UserStatus(email))
            {

                TempData["ChangePassword"] = "Please Change your Password first";
                HttpContext.Session.SetString("EmailID", email);

                return RedirectToAction("PasswordChange");
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

            return RedirectToAction("Login");

        }
    }
}
