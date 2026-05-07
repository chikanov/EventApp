using System.ComponentModel.DataAnnotations;
using System;
using EventApp.Helper;

namespace EventApp.Models
{
    public class Event
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "The title should be between 2 and 50 characters long.")]
        public string Title { get; set; }
        [StringLength(1000, MinimumLength = 2,
            ErrorMessage = "The description should be between 2 and 1000 characters long.")]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DateGreaterThan("EndAt")]
        public DateTime StartAt { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndAt { get; set; }


    }



}
