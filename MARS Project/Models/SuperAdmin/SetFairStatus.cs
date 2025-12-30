using System.ComponentModel.DataAnnotations;

namespace MARS_Project.Models.SuperAdmin
{
    public class SetFairStatus
    {
        [Required (ErrorMessage ="FairId is Must ")]        
        public int FairId { get; set; }
        [Required(ErrorMessage = "Fair EmailID is Must ")]
        public string FairEmailId { get; set; }
        [Required]
        public int Status { get; set; }

    }
}
