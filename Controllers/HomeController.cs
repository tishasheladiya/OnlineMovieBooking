using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using OnlineMovieBooking.Models;

namespace OnlineMovieBooking.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<Movie> latestMovies = new List<Movie>();
            List<MovieCategory> categories = new List<MovieCategory>();

            using (SqlConnection con = DBConnection.GetConnection())
            {
                con.Open();

                // Get categories
                string catQuery = "SELECT Cat_ID, Cat_Type FROM Tbl_Movie_Category ORDER BY Cat_Type ASC";
                using (SqlCommand cmd = new SqlCommand(catQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new MovieCategory
                        {
                            Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                            Cat_Type = reader["Cat_Type"].ToString()
                        });
                    }
                }

                // Get latest movies
                string movieQuery = @"
                    SELECT TOP 6
                        M.Movie_ID,
                        M.Movie_name,
                        M.Release_Date,
                        M.Cat_ID,
                        M.rate,
                        C.Cat_Type
                    FROM Tbl_Movie M
                    INNER JOIN Tbl_Movie_Category C ON M.Cat_ID = C.Cat_ID
                    ORDER BY M.Movie_ID DESC
                ";

                using (SqlCommand cmd = new SqlCommand(movieQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        latestMovies.Add(new Movie
                        {
                            Movie_ID = Convert.ToInt32(reader["Movie_ID"]),
                            Movie_name = reader["Movie_name"].ToString(),
                            Release_Date = Convert.ToDateTime(reader["Release_Date"]),
                            Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                            rate = Convert.ToDecimal(reader["rate"]),
                            Cat_Type = reader["Cat_Type"].ToString()
                        });
                    }
                }
            }

            ViewBag.Categories = categories;
            return View(latestMovies);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Online Movie Booking Management System developed using ASP.NET MVC and ADO.NET.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Get in touch with customer support.";
            return View();
        }
    }
}