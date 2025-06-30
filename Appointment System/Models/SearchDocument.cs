using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace Appointment_System.Models
{
    public class SearchDocument
    {
        [SimpleField(IsKey = true, IsFilterable = true)]
        public string Id { get; set; }
        
        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string Type { get; set; } // "User" or "Service"
        
        // Common fields
        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string Name { get; set; }
        
        [SearchableField]
        public string Description { get; set; }
        
        [SimpleField(IsFilterable = true)]
        public bool IsActive { get; set; }
        
        [SimpleField(IsFilterable = true, IsSortable = true)]
        public DateTime CreatedAt { get; set; }
        
        // User specific fields
        [SearchableField(IsFilterable = true)]
        public string Email { get; set; }
        
        [SearchableField]
        public string Address { get; set; }
        
        [SimpleField(IsFilterable = true)]
        public bool IsServiceProvider { get; set; }
        
        [SearchableField]
        public string BusinessName { get; set; }
        
        // Service specific fields
        [SimpleField(IsFilterable = true, IsSortable = true)]
        public decimal? Price { get; set; }
        
        [SimpleField(IsFilterable = true)]
        public int? DurationMinutes { get; set; }
        
        [SimpleField(IsFilterable = true)]
        public string ProviderId { get; set; }
        
        // Additional fields for faceting and filtering
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public List<string> Tags { get; set; } = new List<string>();
    }
}
