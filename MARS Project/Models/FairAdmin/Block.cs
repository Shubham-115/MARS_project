namespace MARS_Project.Models.FairAdmin
{
    public class Block
    {
        public long BlockID { get; set; }
        public long SubSectorID { get; set; }
        public string BlockName { get; set; }
        public string BlockGroup { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
