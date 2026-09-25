using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using OnlineMovieBooking.Models;

namespace OnlineMovieBooking.Controllers
{
    public class BookingController : Controller
    {
        // Helper: Get Categories
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

        // Helper: Get Movies by Category
        private List<Movie> GetMoviesByCategory(int catId)
        {
            List<Movie> movies = new List<Movie>();
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
                        movies.Add(new Movie
                        {
                            Movie_ID = Convert.ToInt32(reader["Movie_ID"]),
                            Movie_name = reader["Movie_name"].ToString(),
                            rate = Convert.ToDecimal(reader["rate"])
                        });
                    }
                }
            }
            return movies;
        }

        // Helper: Get Movie Details (Rate & Cat_ID)
        private Movie GetMovieById(int movieId)
        {
            Movie movie = null;
            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = "SELECT Movie_ID, Movie_name, Cat_ID, rate FROM Tbl_Movie WHERE Movie_ID = @Movie_ID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Movie_ID", SqlDbType.Int).Value = movieId;
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        movie = new Movie
                        {
                            Movie_ID = Convert.ToInt32(reader["Movie_ID"]),
                            Movie_name = reader["Movie_name"].ToString(),
                            Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                            rate = Convert.ToDecimal(reader["rate"])
                        };
                    }
                }
            }
            return movie;
        }

        // GET: Booking/Book
        // Assignment Req 2: "user should able to book tickets of selected movie."
        // Assignment Note: "Use dropdown list box for category and movie selection (Movie should be filtered according to the category selection)"
        // Assignment Note: "Calculate movie amount based on selected Movie. (Note: amount = rate * no_of_tickets)"
        public ActionResult Book(int? movieId, int? catId)
        {
            if (Session["UserId"] == null)
            {
                string returnUrl = Url.Action("Book", "Booking", new { movieId = movieId, catId = catId });
                TempData["ErrorMessage"] = "Please log in to book movie tickets.";
                return RedirectToAction("Login", "User", new { returnUrl = returnUrl });
            }

            Booking booking = new Booking
            {
                no_of_Tickets = 1
            };

            int selectedCatId = catId ?? 0;
            int selectedMovieId = movieId ?? 0;
            decimal movieRate = 0;

            if (selectedMovieId > 0)
            {
                Movie movie = GetMovieById(selectedMovieId);
                if (movie != null)
                {
                    selectedCatId = movie.Cat_ID;
                    booking.Cat_ID = movie.Cat_ID;
                    booking.Movie_ID = movie.Movie_ID;
                    booking.Movie_Rate = movie.rate;
                    booking.amount = movie.rate * booking.no_of_Tickets;
                    movieRate = movie.rate;
                }
            }

            ViewBag.Categories = new SelectList(GetCategories(), "Cat_ID", "Cat_Type", selectedCatId > 0 ? (object)selectedCatId : null);

            List<Movie> movies = selectedCatId > 0 ? GetMoviesByCategory(selectedCatId) : new List<Movie>();
            ViewBag.Movies = new SelectList(movies, "Movie_ID", "Movie_name", selectedMovieId > 0 ? (object)selectedMovieId : null);
            ViewBag.CurrentRate = movieRate;

            return View(booking);
        }

        // POST: Booking/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Book(Booking model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            model.User_ID = userId;

            // Fetch actual movie rate from database to ensure correct amount calculation
            Movie movie = GetMovieById(model.Movie_ID);
            if (movie == null)
            {
                ModelState.AddModelError("Movie_ID", "Selected movie is invalid.");
            }
            else
            {
                model.Cat_ID = movie.Cat_ID;
                model.Movie_Rate = movie.rate;
                model.amount = movie.rate * model.no_of_Tickets;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(GetCategories(), "Cat_ID", "Cat_Type", model.Cat_ID);
                ViewBag.Movies = new SelectList(GetMoviesByCategory(model.Cat_ID), "Movie_ID", "Movie_name", model.Movie_ID);
                ViewBag.CurrentRate = model.Movie_Rate;
                return View(model);
            }

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string insertQuery = @"
                    INSERT INTO Tbl_Booking (User_ID, Cat_ID, Movie_ID, no_of_Tickets, amount)
                    VALUES (@User_ID, @Cat_ID, @Movie_ID, @no_of_Tickets, @amount)
                ";

                SqlCommand cmd = new SqlCommand(insertQuery, con);
                cmd.Parameters.Add("@User_ID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = model.Cat_ID;
                cmd.Parameters.Add("@Movie_ID", SqlDbType.Int).Value = model.Movie_ID;
                cmd.Parameters.Add("@no_of_Tickets", SqlDbType.Int).Value = model.no_of_Tickets;
                cmd.Parameters.Add("@amount", SqlDbType.Decimal).Value = model.amount;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Booking confirmed! You have successfully booked " + model.no_of_Tickets + " ticket(s) for ₹" + model.amount.ToString("F2") + ".";
            return RedirectToAction("MyBookings");
        }

        // GET: Booking/MyBookings
        // Assignment Req 3: "Authenticate User can able to login to update his/her Movie booking details."
        public ActionResult MyBookings()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "User", new { returnUrl = Url.Action("MyBookings", "Booking") });
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            List<Booking> bookings = new List<Booking>();

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = @"
                    SELECT 
                        B.booking_ID,
                        B.User_ID,
                        B.Cat_ID,
                        B.Movie_ID,
                        B.no_of_Tickets,
                        B.amount,
                        M.Movie_name,
                        M.rate AS Movie_Rate,
                        C.Cat_Type,
                        U.User_Name
                    FROM Tbl_Booking B
                    INNER JOIN Tbl_Movie M ON B.Movie_ID = M.Movie_ID
                    INNER JOIN Tbl_Movie_Category C ON B.Cat_ID = C.Cat_ID
                    INNER JOIN Tbl_User U ON B.User_ID = U.User_ID
                    WHERE B.User_ID = @User_ID
                    ORDER BY B.booking_ID DESC
                ";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@User_ID", SqlDbType.Int).Value = userId;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookings.Add(new Booking
                        {
                            booking_ID = Convert.ToInt32(reader["booking_ID"]),
                            User_ID = Convert.ToInt32(reader["User_ID"]),
                            Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                            Movie_ID = Convert.ToInt32(reader["Movie_ID"]),
                            no_of_Tickets = Convert.ToInt32(reader["no_of_Tickets"]),
                            amount = Convert.ToDecimal(reader["amount"]),
                            Movie_name = reader["Movie_name"].ToString(),
                            Movie_Rate = Convert.ToDecimal(reader["Movie_Rate"]),
                            Cat_Type = reader["Cat_Type"].ToString(),
                            User_Name = reader["User_Name"].ToString()
                        });
                    }
                }
            }

            return View(bookings);
        }

        // GET: Booking/Edit/5
        // Assignment Req 3: "Authenticate User can able to login to update his/her Movie booking details."
        public ActionResult Edit(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            Booking booking = null;

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = @"
                    SELECT 
                        B.booking_ID,
                        B.User_ID,
                        B.Cat_ID,
                        B.Movie_ID,
                        B.no_of_Tickets,
                        B.amount,
                        M.Movie_name,
                        M.rate AS Movie_Rate,
                        C.Cat_Type
                    FROM Tbl_Booking B
                    INNER JOIN Tbl_Movie M ON B.Movie_ID = M.Movie_ID
                    INNER JOIN Tbl_Movie_Category C ON B.Cat_ID = C.Cat_ID
                    WHERE B.booking_ID = @booking_ID AND B.User_ID = @User_ID
                ";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@booking_ID", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@User_ID", SqlDbType.Int).Value = userId;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        booking = new Booking
                        {
                            booking_ID = Convert.ToInt32(reader["booking_ID"]),
                            User_ID = Convert.ToInt32(reader["User_ID"]),
                            Cat_ID = Convert.ToInt32(reader["Cat_ID"]),
                            Movie_ID = Convert.ToInt32(reader["Movie_ID"]),
                            no_of_Tickets = Convert.ToInt32(reader["no_of_Tickets"]),
                            amount = Convert.ToDecimal(reader["amount"]),
                            Movie_name = reader["Movie_name"].ToString(),
                            Movie_Rate = Convert.ToDecimal(reader["Movie_Rate"]),
                            Cat_Type = reader["Cat_Type"].ToString()
                        };
                    }
                }
            }

            if (booking == null)
            {
                return HttpNotFound();
            }

            ViewBag.Categories = new SelectList(GetCategories(), "Cat_ID", "Cat_Type", booking.Cat_ID);
            ViewBag.Movies = new SelectList(GetMoviesByCategory(booking.Cat_ID), "Movie_ID", "Movie_name", booking.Movie_ID);
            ViewBag.CurrentRate = booking.Movie_Rate;

            return View(booking);
        }

        // POST: Booking/Edit
        // Assignment Req 3: "Authenticate User can able to login to update his/her Movie booking details."
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Booking model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            // Re-fetch movie rate to calculate correct amount
            Movie movie = GetMovieById(model.Movie_ID);
            if (movie == null)
            {
                ModelState.AddModelError("Movie_ID", "Selected movie is invalid.");
            }
            else
            {
                model.Cat_ID = movie.Cat_ID;
                model.Movie_Rate = movie.rate;
                model.amount = movie.rate * model.no_of_Tickets;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(GetCategories(), "Cat_ID", "Cat_Type", model.Cat_ID);
                ViewBag.Movies = new SelectList(GetMoviesByCategory(model.Cat_ID), "Movie_ID", "Movie_name", model.Movie_ID);
                ViewBag.CurrentRate = model.Movie_Rate;
                return View(model);
            }

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string updateQuery = @"
                    UPDATE Tbl_Booking
                    SET Cat_ID = @Cat_ID,
                        Movie_ID = @Movie_ID,
                        no_of_Tickets = @no_of_Tickets,
                        amount = @amount
                    WHERE booking_ID = @booking_ID AND User_ID = @User_ID
                ";

                SqlCommand cmd = new SqlCommand(updateQuery, con);
                cmd.Parameters.Add("@booking_ID", SqlDbType.Int).Value = model.booking_ID;
                cmd.Parameters.Add("@User_ID", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@Cat_ID", SqlDbType.Int).Value = model.Cat_ID;
                cmd.Parameters.Add("@Movie_ID", SqlDbType.Int).Value = model.Movie_ID;
                cmd.Parameters.Add("@no_of_Tickets", SqlDbType.Int).Value = model.no_of_Tickets;
                cmd.Parameters.Add("@amount", SqlDbType.Decimal).Value = model.amount;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Booking updated successfully! New total amount: ₹" + model.amount.ToString("F2");
            return RedirectToAction("MyBookings");
        }

        // GET: Booking/Delete/5
        public ActionResult Delete(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            Booking booking = null;

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = @"
                    SELECT 
                        B.booking_ID,
                        B.no_of_Tickets,
                        B.amount,
                        M.Movie_name,
                        M.rate AS Movie_Rate,
                        C.Cat_Type
                    FROM Tbl_Booking B
                    INNER JOIN Tbl_Movie M ON B.Movie_ID = M.Movie_ID
                    INNER JOIN Tbl_Movie_Category C ON B.Cat_ID = C.Cat_ID
                    WHERE B.booking_ID = @booking_ID AND B.User_ID = @User_ID
                ";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@booking_ID", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@User_ID", SqlDbType.Int).Value = userId;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        booking = new Booking
                        {
                            booking_ID = Convert.ToInt32(reader["booking_ID"]),
                            Movie_name = reader["Movie_name"].ToString(),
                            Movie_Rate = Convert.ToDecimal(reader["Movie_Rate"]),
                            Cat_Type = reader["Cat_Type"].ToString(),
                            no_of_Tickets = Convert.ToInt32(reader["no_of_Tickets"]),
                            amount = Convert.ToDecimal(reader["amount"])
                        };
                    }
                }
            }

            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking);
        }

        // POST: Booking/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int booking_ID)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string deleteQuery = "DELETE FROM Tbl_Booking WHERE booking_ID = @booking_ID AND User_ID = @User_ID";
                SqlCommand cmd = new SqlCommand(deleteQuery, con);
                cmd.Parameters.Add("@booking_ID", SqlDbType.Int).Value = booking_ID;
                cmd.Parameters.Add("@User_ID", SqlDbType.Int).Value = userId;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Booking cancelled successfully.";
            return RedirectToAction("MyBookings");
        }
    }
}
