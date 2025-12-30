using MARS_Project.Models;
using MARS_Project.Models.SuperAdmin;

namespace MARS_Project.Repositories
{
    public interface IFair
    {
        public Task<string>CreateFair(Createfair createFair);
        public Task<string> UpdateFair();
        public Task<int> SetFairStatus(SetFairStatus status);
        public Task<int> CheckFairStatus(SetFairStatus status);


    }
}
