using System;

namespace Cybersecurity_Awareness_Bot
{
    
    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderAt { get; set; }
        public bool IsCompleted { get; set; }
        public bool ReminderNotified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string StatusText
        {
            get { return IsCompleted ? "Completed" : "Open"; }
        }

        public string ReminderText
        {
            get
            {
                return ReminderAt.HasValue
                    ? ReminderAt.Value.ToString("dd MMM yyyy, HH:mm")
                    : "No reminder";
            }
        }
    }
}
