using System.ComponentModel.DataAnnotations;

namespace MARS_Project.Models.Citizen
{
    public class VerifiedEmailMobile
    {
        [Required]
       public string OTPEmailID { get; set; }

        [Required]
       public string OTPMobile {  get; set; }

        public bool EmailVerified { get; set; }
        public bool MobileVerified { get; set; }
    }
}
 