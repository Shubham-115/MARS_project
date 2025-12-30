using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MARS_Project.Models
{
    public class Createfair
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Fair Name is required")]
        public string FairName { get; set; }

        [NotMapped]
        
        public IFormFile FairLogoPath { get; set; } // Uploaded file

        public string FairLogoPathString { get; set; } // Path saved in DB
        [Required]
        public string Division { get; set; }
        [Required]
        public string District { get; set; }
        [Required]
        public string Tehsil { get; set; }
        [Required(ErrorMessage ="Please Enter city or Place Name")]
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
        [Required]
        public string ContactMobile1 { get; set; }
        public string ContactMobile2 { get; set; }
        [Required]
        public string ContactEmail { get; set; }

        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        [Required]
        public string CreatedBy { get; set; }
    }
}
