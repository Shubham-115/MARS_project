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
        [Required(ErrorMessage = "Logo is required")]
        public IFormFile FairLogoPath { get; set; } // Uploaded file

        public string FairLogoPathString { get; set; } // Path saved in DB

        public string Division { get; set; }
        public string District { get; set; }
        public string Tehsil { get; set; }
        public string City { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ApplyStartDate { get; set; }
        public DateTime? ApplyEndDate { get; set; }

        public string ContactMobile1 { get; set; }
        public string ContactMobile2 { get; set; }
        public string ContactEmail { get; set; }

        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
