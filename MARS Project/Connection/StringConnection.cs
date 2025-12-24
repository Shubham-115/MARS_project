namespace MARS_Project.Connection
{
    public class StringConnection
    {
        private readonly IConfiguration _config;

        public StringConnection(IConfiguration config)
        {
            _config = config;
        }

        public string Dbcs =>
            _config.GetConnectionString("DefaultConnection");

    }
}
