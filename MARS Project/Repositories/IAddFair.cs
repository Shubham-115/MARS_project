using MARS_Project.Models;
using MARS_Project.Models.FairAdmin;

namespace MARS_Project.Repositories
{
    public interface IAddFair
    {
        public Task<int> AddSector(Sector sector);
        public Task<string> AddSubSector(Subsector subsector);
        public Task<string> AddBlock(Block block);
        public Task<long> GetFairID(string EmailID);

        public Task<Myprofile> profile(Myprofile myprofile);
    }
}

