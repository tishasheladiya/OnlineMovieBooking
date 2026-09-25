using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineMovieBooking.Models
{
    public class Movie
    {
        [Display(Name = "Movie ID")]
        public int Movie_ID { get; set; }

        [Required(ErrorMessage = "Movie name is required.")]
        [StringLength(200, ErrorMessage = "Movie name cannot exceed 200 characters.")]
        [Display(Name = "Movie Name")]
        public string Movie_name { get; set; }

        [Required(ErrorMessage = "Release date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Release Date")]
        public DateTime Release_Date { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int Cat_ID { get; set; }

        [Required(ErrorMessage = "Rate is required.")]
        [Range(1, 100000, ErrorMessage = "Rate must be greater than 0.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Ticket Rate (₹)")]
        public decimal rate { get; set; }

        // For displaying category name in views
        [Display(Name = "Category")]
        public string Cat_Type { get; set; }
    }
}