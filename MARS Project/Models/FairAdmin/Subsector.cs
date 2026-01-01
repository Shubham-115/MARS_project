namespace MARS_Project.Models.FairAdmin
{
    public class Subsector
    {
        public long SubSectorID { get; set; }
        public long SectorID { get; set; }
        public string SubSectorName { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
