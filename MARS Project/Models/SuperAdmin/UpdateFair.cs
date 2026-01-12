using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MARS_Project.Models.SuperAdmin
{
    public class UpdateFair
    {
        
        public string FairName { get; set; }

        [NotMapped]

        public IFormFile FairLogoPath { get; set; } // Uploaded file

        public string FairLogoPathString { get; set; } // Path saved in DB
       
        public string Division { get; set; }
   
        public string District { get; set; }
 
        public string Tehsil { get; set; }
    
        public string City { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? ApplyStartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? ApplyEndDate { get; set; }
      
        public string ContactMobile1 { get; set; }
        public string ContactMobile2 { get; set; }
        
        public string ContactEmail { get; set; }

        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; }
    }
}
