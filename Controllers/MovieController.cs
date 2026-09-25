using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using OnlineMovieBooking.Models;

namespace OnlineMovieBooking.Controllers
{
    public class MovieController : Controller
    {
        // Helper: Get all categories for dropdown
        private List<MovieCategory> GetCategories()
        {
            List<MovieCategory> categories = new List<MovieCategory>();

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = "SELECT Cat_ID, Cat_Type FROM Tbl_Movie_Category ORDER BY Cat_Type ASC";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();

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
            }

            return categories;
        }

        // DISPLAY MOVIES (WITH CATEGORY SEARCH/FILTER)
        // Assignment Req 6: "User should able to search Movie based on selected category"
        public ActionResult Index(int? searchCatId)
        {
            List<Movie> movies = new List<Movie>();

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = @"
                    SELECT 
                        M.Movie_ID,
                        M.Movie_name,
                        M.Release_Date,
                        M.Cat_ID,
                        M.rate,
                        C.Cat_Type
                    FROM Tbl_Movie M
                    INNER JOIN Tbl_Movie_Category C ON M.Cat_ID = C.Cat_ID
                ";

                if (searchCatId.HasValue && searchCatId.Value > 0)
                {
                    query += " WHERE M.Cat_ID = @Cat_ID";
                }

                query += " ORDER BY M.Movie_name ASC";

                SqlCommand cmd = new SqlCommand(query, con);

                if (searchCatId.HasValue && searchCatId.Value > 0)
                {
                    cmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = searchCatId.Value;
                }

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        movies.Add(new Movie
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

            ViewBag.Categories = new SelectList(GetCategories(), "Cat_ID", "Cat_Type", searchCatId);
            ViewBag.SelectedCatId = searchCatId;

            return View(movies);
        }

        // CREATE - GET
        // Assignment Req 1: "User should able to insert new movie category and new movie"
        public ActionResult Create()
        {
            ViewBag.Categories = new SelectList(GetCategories(), "Cat_ID", "Cat_Type");
            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Movie movie)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(GetCategories(), "Cat_ID", "Cat_Type", movie.Cat_ID);
                return View(movie);
            }

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = @"
                    INSERT INTO Tbl_Movie (Movie_name, Release_Date, Cat_ID, rate)
                    VALUES (@Movie_name, @Release_Date, @Cat_ID, @rate)
                ";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Movie_name", SqlDbType.VarChar, 200).Value = movie.Movie_name.Trim();
                cmd.Parameters.Add("@Release_Date", SqlDbType.Date).Value = movie.Release_Date.Date;
                cmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = movie.Cat_ID;
                cmd.Parameters.Add("@rate", SqlDbType.Decimal).Value = movie.rate;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Movie '" + movie.Movie_name + "' added successfully!";
            return RedirectToAction("Index");
        }

        // EDIT - GET
        public ActionResult Edit(int id)
        {
            Movie movie = null;

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = @"
                    SELECT Movie_ID, Movie_name, Release_Date, Cat_ID, rate
                    FROM Tbl_Movie
                    WHERE Movie_ID = @Movie_ID
                ";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Movie_ID", SqlDbType.Int).Value = id;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        movie = new Movie
                        {
                            Movie_ID = Convert.ToInt32(reader["Movie_ID"]),
                            Movie_name = reader["Movie_name"].ToString(),
                            Release_Date = Convert.ToDateTime(reader["Release_Date"]),
                            Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                            rate = Convert.ToDecimal(reader["rate"])
                        };
                    }
                }
            }

            if (movie == null)
            {
                return HttpNotFound();
            }

            ViewBag.Categories = new SelectList(GetCategories(), "Cat_ID", "Cat_Type", movie.Cat_ID);
            return View(movie);
        }

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Movie movie)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(GetCategories(), "Cat_ID", "Cat_Type", movie.Cat_ID);
                return View(movie);
            }

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = @"
                    UPDATE Tbl_Movie
                    SET Movie_name = @Movie_name,
                        Release_Date = @Release_Date,
                        Cat_ID = @Cat_ID,
                        rate = @rate
                    WHERE Movie_ID = @Movie_ID
                ";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Movie_ID", SqlDbType.Int).Value = movie.Movie_ID;
                cmd.Parameters.Add("@Movie_name", SqlDbType.VarChar, 200).Value = movie.Movie_name.Trim();
                cmd.Parameters.Add("@Release_Date", SqlDbType.Date).Value = movie.Release_Date.Date;
                cmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = movie.Cat_ID;
                cmd.Parameters.Add("@rate", SqlDbType.Decimal).Value = movie.rate;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Movie updated successfully!";
            return RedirectToAction("Index");
        }

        // DELETE - GET
        // Assignment Req 5: "User should able to delete selected Movie"
        public ActionResult Delete(int id)
        {
            Movie movie = null;

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = @"
                    SELECT 
                        M.Movie_ID,
                        M.Movie_name,
                        M.Release_Date,
                        M.Cat_ID,
                        M.rate,
                        C.Cat_Type
                    FROM Tbl_Movie M
                    INNER JOIN Tbl_Movie_Category C ON M.Cat_ID = C.Cat_ID
                    WHERE M.Movie_ID = @Movie_ID
                ";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Movie_ID", SqlDbType.Int).Value = id;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        movie = new Movie
                        {
                            Movie_ID = Convert.ToInt32(reader["Movie_ID"]),
                            Movie_name = reader["Movie_name"].ToString(),
                            Release_Date = Convert.ToDateTime(reader["Release_Date"]),
                            Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                            rate = Convert.ToDecimal(reader["rate"]),
                            Cat_Type = reader["Cat_Type"].ToString()
                        };
                    }
                }
            }

            if (movie == null)
            {
                return HttpNotFound();
            }

            return View(movie);
        }

        // DELETE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int Movie_ID)
        {
            using (SqlConnection con = DBConnection.GetConnection())
            {
                con.Open();

                // Check if any bookings exist for this movie
                string checkQuery = "SELECT COUNT(*) FROM Tbl_Booking WHERE Movie_ID = @Movie_ID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.Add("@Movie_ID", SqlDbType.Int).Value = Movie_ID;
                    int bookingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (bookingCount > 0)
                    {
                        TempData["ErrorMessage"] = "Cannot delete this movie because " + bookingCount + " active booking(s) exist for it. Please delete the bookings first.";
                        return RedirectToAction("Index");
                    }
                }

                try
                {
                    string deleteQuery = "DELETE FROM Tbl_Movie WHERE Movie_ID = @Movie_ID";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
                    {
                        deleteCmd.Parameters.Add("@Movie_ID", SqlDbType.Int).Value = Movie_ID;
                        deleteCmd.ExecuteNonQuery();
                    }

                    TempData["SuccessMessage"] = "Movie deleted successfully!";
                }
                catch (SqlException ex)
                {
                    TempData["ErrorMessage"] = "Error deleting movie: " + ex.Message;
                }
            }

            return RedirectToAction("Index");
        }

        // AJAX ENDPOINT: Get Movies by Category (for cascading dropdown)
        // Assignment Note: "Movie should be filtered according to the category selection"
        [HttpGet]
        public JsonResult GetMoviesByCategory(int catId)
        {
            List<object> movies = new List<object>();

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = "SELECT Movie_ID, Movie_name, rate FROM Tbl_Movie WHERE Cat_ID = @Cat_ID ORDER BY Movie_name ASC";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = catId;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        movies.Add(new
                        {
                            Movie_ID = Convert.ToInt32(reader["Movie_ID"]),
                            Movie_name = reader["Movie_name"].ToString(),
                            Rate = Convert.ToDecimal(reader["rate"])
                        });
                    }
                }
            }

            return Json(movies, JsonRequestBehavior.AllowGet);
        }

        // AJAX ENDPOINT: Get Movie Rate
        // Assignment Note: "Calculate movie amount based on selected Movie"
        [HttpGet]
        public JsonResult GetMovieRate(int movieId)
        {
            decimal rate = 0;

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = "SELECT rate FROM Tbl_Movie WHERE Movie_ID = @Movie_ID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Movie_ID", SqlDbType.Int).Value = movieId;

                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    rate = Convert.ToDecimal(result);
                }
            }

            return Json(new { rate = rate }, JsonRequestBehavior.AllowGet);
        }
    }
}