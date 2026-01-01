namespace MARS_Project.Models.FairAdmin
{
    public class Sector
    {
        public long SectorID { get; set; }
        public long FairID { get; set; }
        public string SectorName { get; set; }
        public string SectorGroup { get; set; }
        public string Area { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
