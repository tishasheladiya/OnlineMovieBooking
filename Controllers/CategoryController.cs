using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using OnlineMovieBooking.Models;

namespace OnlineMovieBooking.Controllers
{
    public class CategoryController : Controller
    {
        // DISPLAY ALL CATEGORIES
        public ActionResult Index()
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

            return View(categories);
        }

        // CREATE - GET
        public ActionResult Create()
        {
            return View();
        }

        // CREATE - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MovieCategory category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = "INSERT INTO Tbl_Movie_Category (Cat_Type) VALUES (@Cat_Type)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Cat_Type", SqlDbType.VarChar, 50).Value = category.Cat_Type.Trim();

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Category '" + category.Cat_Type + "' added successfully!";
            return RedirectToAction("Index");
        }

        // EDIT - GET
        public ActionResult Edit(int id)
        {
            MovieCategory category = null;

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = "SELECT Cat_ID, Cat_Type FROM Tbl_Movie_Category WHERE Cat_ID = @Cat_ID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = id;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        category = new MovieCategory
                        {
                            Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                            Cat_Type = reader["Cat_Type"].ToString()
                        };
                    }
                }
            }

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // EDIT - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MovieCategory category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = "UPDATE Tbl_Movie_Category SET Cat_Type = @Cat_Type WHERE Cat_ID = @Cat_ID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = category.Cat_ID;
                cmd.Parameters.Add("@Cat_Type", SqlDbType.VarChar, 50).Value = category.Cat_Type.Trim();

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Category updated successfully!";
            return RedirectToAction("Index");
        }

        // GET: Category/Delete/5
        public ActionResult Delete(int id)
        {
            MovieCategory category = null;

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = "SELECT Cat_ID, Cat_Type FROM Tbl_Movie_Category WHERE Cat_ID = @Cat_ID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = id;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        category = new MovieCategory
                        {
                            Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                            Cat_Type = reader["Cat_Type"].ToString()
                        };
                    }
                }
            }

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // POST: Category/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int Cat_ID)
        {
            using (SqlConnection con = DBConnection.GetConnection())
            {
                con.Open();

                // Check if any movies exist with this category
                string checkQuery = "SELECT COUNT(*) FROM Tbl_Movie WHERE Cat_ID = @Cat_ID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = Cat_ID;
                    int movieCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (movieCount > 0)
                    {
                        TempData["ErrorMessage"] = "Cannot delete this category because " + movieCount + " movie(s) are assigned to it. Please reassign or delete the movies first.";
                        return RedirectToAction("Index");
                    }
                }

                // Check if any bookings exist with this category
                string checkBookingQuery = "SELECT COUNT(*) FROM Tbl_Booking WHERE Cat_ID = @Cat_ID";
                using (SqlCommand checkBookingCmd = new SqlCommand(checkBookingQuery, con))
                {
                    checkBookingCmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = Cat_ID;
                    int bookingCount = Convert.ToInt32(checkBookingCmd.ExecuteScalar());
                    if (bookingCount > 0)
                    {
                        TempData["ErrorMessage"] = "Cannot delete this category because active bookings exist for it.";
                        return RedirectToAction("Index");
                    }
                }

                try
                {
                    string deleteQuery = "DELETE FROM Tbl_Movie_Category WHERE Cat_ID = @Cat_ID";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
                    {
                        deleteCmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = Cat_ID;
                        deleteCmd.ExecuteNonQuery();
                    }
                    TempData["SuccessMessage"] = "Category deleted successfully!";
                }
                catch (SqlException ex)
                {
                    TempData["ErrorMessage"] = "Error deleting category: " + ex.Message;
                }
            }

            return RedirectToAction("Index");
        }
    }
}