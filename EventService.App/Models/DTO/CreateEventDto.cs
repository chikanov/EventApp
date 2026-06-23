using EventApp.Helper;
using System.ComponentModel.DataAnnotations;

namespace EventApp.Models.DTO
{
    public class CreateEventDto
    {
        /// Title
        [Required]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "The title should be between 2 and 50 characters long.")]
        public string Title { get; set; } = string.Empty;
        /// Description
        [StringLength(1000, MinimumLength = 2,
            ErrorMessage = "The description should be between 2 and 1000 characters long.")]
        public string Description { get; set; } = string.Empty;
        /// StartAt
        [Required]
        [DataType(DataType.Date)]
        [DateGreaterThan("EndAt")]
        public DateTime? StartAt { get; set; }
        /// EndAt
        [Required]
        [DataType(DataType.Date)]
        public DateTime? EndAt { get; set; }
        [Required]
        public int TotalSeats { get; set; }
    }
}
