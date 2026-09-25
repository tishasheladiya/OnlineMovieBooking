using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineMovieBooking.Models
{
    public class MovieCategory
    {
        [Display(Name = "Category ID")]
        public int Cat_ID { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
        [Display(Name = "Category Name")]
        public string Cat_Type { get; set; }
    }
}