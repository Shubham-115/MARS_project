using MARS_Project.Models;

namespace MARS_Project.Repositories
{
    public interface IFair
    {
        public Task<string>CreateFair(Createfair createFair);
        public Task<string> UpdateFair();
        public Task<string> DeleteFair();

    }
}
