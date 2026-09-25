using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineMovieBooking.Models
{
    public class Booking
    {
        [Display(Name = "Booking ID")]
        public int booking_ID { get; set; }

        [Display(Name = "User")]
        public int User_ID { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int Cat_ID { get; set; }

        [Required(ErrorMessage = "Please select a movie.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a movie.")]
        [Display(Name = "Movie")]
        public int Movie_ID { get; set; }

        [Required(ErrorMessage = "Number of tickets is required.")]
        [Range(1, 50, ErrorMessage = "Number of tickets must be between 1 and 50.")]
        [Display(Name = "Number of Tickets")]
        public int no_of_Tickets { get; set; }

        [Display(Name = "Total Amount (₹)")]
        [DataType(DataType.Currency)]
        public decimal amount { get; set; }

        // Additional display properties for joins
        [Display(Name = "Customer Name")]
        public string User_Name { get; set; }

        [Display(Name = "Category")]
        public string Cat_Type { get; set; }

        [Display(Name = "Movie Name")]
        public string Movie_name { get; set; }

        [Display(Name = "Ticket Rate (₹)")]
        public decimal Movie_Rate { get; set; }
    }
}
