using MySql.Data.MySqlClient;

namespace UsedCarsPrice.WebScraper.Services.Utils
{
    public class DBUtils
    {
        public static MySqlConnection GetDBConnection(string connectionName)
        {
            return new MySqlConnection(CommonUtils.GetConfigItem(connectionName));
        }

        public static MySqlConnection GetDBConnection()
        {
            return new MySqlConnection(CommonUtils.GetConfigItem("ConnectionString"));
        }
    }
}
