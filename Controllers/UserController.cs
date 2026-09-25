using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using OnlineMovieBooking.Models;

namespace OnlineMovieBooking.Controllers
{
    public class UserController : Controller
    {
        // GET: User/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            using (SqlConnection con = DBConnection.GetConnection())
            {
                con.Open();

                // Check if email already registered
                string checkQuery = "SELECT COUNT(*) FROM Tbl_User WHERE Email_ID = @Email_ID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.Add("@Email_ID", SqlDbType.VarChar, 100).Value = user.Email_ID.Trim();
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        ModelState.AddModelError("Email_ID", "This email address is already registered.");
                        return View(user);
                    }
                }

                // Insert new user
                string insertQuery = @"
                    INSERT INTO Tbl_User (User_Name, Email_ID, User_password, City, PhoneNo)
                    VALUES (@User_Name, @Email_ID, @User_password, @City, @PhoneNo);
                    SELECT SCOPE_IDENTITY();
                ";

                using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                {
                    cmd.Parameters.Add("@User_Name", SqlDbType.VarChar, 100).Value = user.User_Name.Trim();
                    cmd.Parameters.Add("@Email_ID", SqlDbType.VarChar, 100).Value = user.Email_ID.Trim();
                    cmd.Parameters.Add("@User_password", SqlDbType.VarChar, 100).Value = user.User_password;
                    cmd.Parameters.Add("@City", SqlDbType.VarChar, 50).Value = (object)user.City ?? DBNull.Value;
                    cmd.Parameters.Add("@PhoneNo", SqlDbType.VarChar, 20).Value = (object)user.PhoneNo ?? DBNull.Value;

                    int newUserId = Convert.ToInt32(cmd.ExecuteScalar());

                    // Auto login after registration
                    Session["UserId"] = newUserId;
                    Session["UserName"] = user.User_Name.Trim();
                    Session["UserEmail"] = user.Email_ID.Trim();
                }
            }

            TempData["SuccessMessage"] = "Registration successful! Welcome, " + user.User_Name + "!";
            return RedirectToAction("Index", "Movie");
        }

        // GET: User/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: User/Login
        // Assignment Req 3: "Authenticate User can able to login to update his/her Movie booking details."
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = @"
                    SELECT User_ID, User_Name, Email_ID
                    FROM Tbl_User
                    WHERE Email_ID = @Email_ID AND User_password = @User_password
                ";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@Email_ID", SqlDbType.VarChar, 100).Value = model.Email_ID.Trim();
                cmd.Parameters.Add("@User_password", SqlDbType.VarChar, 100).Value = model.User_password;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Session["UserId"] = Convert.ToInt32(reader["User_ID"]);
                        Session["UserName"] = reader["User_Name"].ToString();
                        Session["UserEmail"] = reader["Email_ID"].ToString();

                        TempData["SuccessMessage"] = "Logged in successfully! Welcome back, " + Session["UserName"] + "!";

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("Index", "Movie");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid email address or password.");
                    }
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // GET: User/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // GET: User/Profile
        // Assignment Req 4: "User should able to update their profile details."
        public new ActionResult Profile()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile", "User") });
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            User user = null;

            using (SqlConnection con = DBConnection.GetConnection())
            {
                string query = "SELECT User_ID, User_Name, Email_ID, User_password, City, PhoneNo FROM Tbl_User WHERE User_ID = @User_ID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@User_ID", SqlDbType.Int).Value = userId;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            User_ID = Convert.ToInt32(reader["User_ID"]),
                            User_Name = reader["User_Name"].ToString(),
                            Email_ID = reader["Email_ID"].ToString(),
                            User_password = reader["User_password"].ToString(),
                            City = reader["City"] != DBNull.Value ? reader["City"].ToString() : "",
                            PhoneNo = reader["PhoneNo"] != DBNull.Value ? reader["PhoneNo"].ToString() : ""
                        };
                    }
                }
            }

            if (user == null)
            {
                Session.Clear();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // POST: User/Profile
        // Assignment Req 4: "User should able to update their profile details."
        [HttpPost]
        [ValidateAntiForgeryToken]
        public new ActionResult Profile(User model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            using (SqlConnection con = DBConnection.GetConnection())
            {
                con.Open();

                // Check if email changed and taken by someone else
                string checkQuery = "SELECT COUNT(*) FROM Tbl_User WHERE Email_ID = @Email_ID AND User_ID != @User_ID";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.Add("@Email_ID", SqlDbType.VarChar, 100).Value = model.Email_ID.Trim();
                    checkCmd.Parameters.Add("@User_ID", SqlDbType.Int).Value = userId;
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        ModelState.AddModelError("Email_ID", "This email address is already in use by another account.");
                        return View(model);
                    }
                }

                string updateQuery = @"
                    UPDATE Tbl_User
                    SET User_Name = @User_Name,
                        Email_ID = @Email_ID,
                        User_password = @User_password,
                        City = @City,
                        PhoneNo = @PhoneNo
                    WHERE User_ID = @User_ID
                ";

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.Add("@User_ID", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@User_Name", SqlDbType.VarChar, 100).Value = model.User_Name.Trim();
                    cmd.Parameters.Add("@Email_ID", SqlDbType.VarChar, 100).Value = model.Email_ID.Trim();
                    cmd.Parameters.Add("@User_password", SqlDbType.VarChar, 100).Value = model.User_password;
                    cmd.Parameters.Add("@City", SqlDbType.VarChar, 50).Value = (object)model.City ?? DBNull.Value;
                    cmd.Parameters.Add("@PhoneNo", SqlDbType.VarChar, 20).Value = (object)model.PhoneNo ?? DBNull.Value;

                    cmd.ExecuteNonQuery();
                }

                // Update session
                Session["UserName"] = model.User_Name.Trim();
                Session["UserEmail"] = model.Email_ID.Trim();
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}
