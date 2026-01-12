using System.ComponentModel.DataAnnotations;

namespace MARS_Project.Models.Citizen
{
    public class PasswordChange
    {

        [Required]
        [MinLength(6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9\W]).{6,}$",
            ErrorMessage = "Password must be at least 6 characters and include uppercase, lowercase, and number/special character.")]
        public string PasswordHash { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}


