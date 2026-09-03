//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace OnlineMovieBooking.Controllers
//{
//    public class CategoryController : Controller
//    {
//        // GET: Category
//        public ActionResult Index()
//        {
//            return View();
//        }
//    }
//}

using OnlineMovieBooking.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

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
                string query = "SELECT Cat_ID, Cat_Type FROM Tbl_Movie_Category";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    categories.Add(new MovieCategory
                    {
                        Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                        Cat_Type = reader["Cat_Type"].ToString()
                    });
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
        public ActionResult Create(MovieCategory category)
        {
            if (string.IsNullOrWhiteSpace(category.Cat_Type))
            {
                ModelState.AddModelError(
                    "Cat_Type",
                    "Category name is required."
                );

                return View(category);
            }

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query =
                    "INSERT INTO Tbl_Movie_Category (Cat_Type) VALUES (@Cat_Type)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue(
                    "@Cat_Type",
                    category.Cat_Type
                );

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }


        // EDIT - GET
        public ActionResult Edit(int id)
        {
            MovieCategory category = null;

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query =
                    "SELECT Cat_ID, Cat_Type " +
                    "FROM Tbl_Movie_Category " +
                    "WHERE Cat_ID = @Cat_ID";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Cat_ID", id);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    category = new MovieCategory
                    {
                        Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                        Cat_Type = reader["Cat_Type"].ToString()
                    };
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
        public ActionResult Edit(MovieCategory category)
        {
            if (string.IsNullOrWhiteSpace(category.Cat_Type))
            {
                ModelState.AddModelError(
                    "Cat_Type",
                    "Category name is required."
                );

                return View(category);
            }

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query =
                    "UPDATE Tbl_Movie_Category " +
                    "SET Cat_Type = @Cat_Type " +
                    "WHERE Cat_ID = @Cat_ID";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue(
                    "@Cat_ID",
                    category.Cat_ID
                );

                cmd.Parameters.AddWithValue(
                    "@Cat_Type",
                    category.Cat_Type
                );

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }


        // DELETE
        //public ActionResult Delete(int id)
        //{
        //    using (SqlConnection con = DBConnection.GetConnection())
        //    {
        //        string query =
        //            "DELETE FROM Tbl_Movie_Category " +
        //            "WHERE Cat_ID = @Cat_ID";

        //        SqlCommand cmd = new SqlCommand(query, con);

        //        cmd.Parameters.AddWithValue("@Cat_ID", id);

        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //    }

        //    return RedirectToAction("Index");
        //}

        // GET: Category/Delete/5
        public ActionResult Delete(int id)
        {
            MovieCategory category = null;

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = "SELECT * FROM Tbl_Movie_Category WHERE Cat_ID = @Cat_ID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Cat_ID", id);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    category = new MovieCategory
                    {
                        Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                        Cat_Type = reader["Cat_Type"].ToString()
                    };
                }
            }

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }


        // POST: Category/Delete
        [HttpPost]
        public ActionResult DeleteConfirmed(int Cat_ID)
        {
            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query =
                    "DELETE FROM Tbl_Movie_Category WHERE Cat_ID = @Cat_ID";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Cat_ID", Cat_ID);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}