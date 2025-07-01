using System;

namespace Appointment_System.Models
{
    public class Template
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        // slot model, and free-selection model
        public bool Type { get; set; }

        public string ProviderId { get; set; }

        public ICollection<Day> Days { get; set; }
        
    }
}
 