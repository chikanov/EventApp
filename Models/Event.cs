using System.ComponentModel.DataAnnotations;
using EventApp.Helper;

namespace EventApp.Models
{
    /// <summary>
    /// Event
    /// </summary>
    public class Event
    {
        /// Id
        public int Id { get; set; }
        /// Title
        public string Title { get; set; } = string.Empty;
        /// Description
        public string Description { get; set; } = string.Empty;
        /// StartAt
        public DateTime? StartAt { get; set; }
        /// EndAt
        public DateTime? EndAt { get; set; }
    }



}
