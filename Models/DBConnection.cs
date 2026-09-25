using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace OnlineMovieBooking.Models
{
    public class DBConnection
    {
        public static SqlConnection GetConnection()
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings[
                    "MovieBookingConnection"
                ].ConnectionString;

            return new SqlConnection(connectionString);
        }
    }
}