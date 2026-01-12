using MARS_Project.Models;
using MARS_Project.Models.Citizen;
using MARS_Project.Models.SuperAdmin;

namespace MARS_Project.Repositories
{
    public interface IFair
    {
        public Task<string>CreateFair(Createfair createFair);
        public Task<bool> UpdateFair(Createfair createfair);
        public Task<int> SetFairStatus(SetFairStatus status);
        public Task<int> CheckFairStatus(SetFairStatus status);
        public  Task<int> AddFairAdmin(SignUp signup);

        public Task<List<Createfair>> GetFairDetails(int FairID, string EmailID);
    }
}
