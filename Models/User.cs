using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineMovieBooking.Models
{
    public class User
    {
        [Display(Name = "User ID")]
        public int User_ID { get; set; }

        [Required(ErrorMessage = "User name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string User_Name { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Display(Name = "Email Address")]
        public string Email_ID { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Password must be at least 4 characters long.")]
        [Display(Name = "Password")]
        public string User_password { get; set; }

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Phone Number")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must be a 10-digit number.")]
        public string PhoneNo { get; set; }
    }
}
