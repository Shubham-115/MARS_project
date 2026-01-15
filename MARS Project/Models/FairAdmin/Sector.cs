using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace MARS_Project.Models.FairAdmin
{
    public class Sector
    {
        [Required]
       public string EmailID { get; set; }
        [Required]
        public long FairID { get; set; }
        [Required]
        public string SectorName { get; set; }
        [Required]
        public string SectorGroup { get; set; }
        [Required]
        public string Area { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public long SectorID { get; set; }
    }
}
