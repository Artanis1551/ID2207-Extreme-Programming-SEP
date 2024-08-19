using System.ComponentModel.DataAnnotations;
using WebAPI.EnumTypes;

namespace WebAPI.DataModels
{
    public class EventApplication
    {
        [Required]
        public string RecordNumber { get; set; }
        [Required]
        public string ClientName { get; set; }
        [Required]
        public string EventType { get; set; }
        [Required]
        public DateTime From { get; set; }
        [Required]
        public DateTime To { get; set; }
        [Required]
        public int Attendees { get; set; }
        [Required]
        public bool Decorations { get; set; }
        [Required]
        public bool Food { get; set; }
        [Required]
        public bool Parties { get; set; }
        [Required]
        public bool Drinks { get; set; }
        [Required]
        public bool PhotoVideo { get; set; }
        [Required]
        public int ExpectedBudget { get; set; }
        [Required]
        public UserRoles Assignee { get; set; }
        public int Id { get; set; }
        public EventApplicationState Status { get; set; }
    }
}
