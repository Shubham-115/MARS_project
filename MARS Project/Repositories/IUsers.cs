using MARS_Project.Models;

namespace MARS_Project.Repositories
{
    public interface IUsers
    {
        public bool IsexistEmail(string EmailID);
       public Task<int> UserSingUp(SignUp signup);
        Task<int> UserLogin(Login login);


        public Task<bool> isVerifiedMobile(string MobileNo);
        public  Task<bool> isVerifiedEmail(string EmailID);

        public bool VerifyEmailPassword(string EmailID, string Password);
        public bool IsValidEmailAndMobile(string EmailID, string MobileNo);
        public void UpdateLoginTime(string EmailID);
        public string GenerateRandomString(int length = 10);
        public string ConvertHashPassword(string password);
        public string PassWordChange(string EmailID, string Password);
        public bool UserStatus(string EmailID);
        public void setStatus(string EmailID, int Status);
        public DateTime VerifyUser(string token, string EmailID);
        public string UpdateToken(string EmailID);
        public bool resetToken(string EmailID);
        public bool IsMobileExist(string MobileNo);
        public string GetOTP(string MobileNo);
        public DateTime VerifyOTP(string OTPCode, string MobileNo);
        public bool SetPassWord(string EmailID, string Password);


    }
}
