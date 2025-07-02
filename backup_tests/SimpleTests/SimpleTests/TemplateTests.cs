using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTests
{
    [TestClass]
    public class TemplateTests
    {
        // Simple model classes for testing
        public class User
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public bool IsServiceProvider { get; set; }
        }
        
        public class Template
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ProviderId { get; set; }
            public User Provider { get; set; }
            public bool Type { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public List<Day> Days { get; set; } = new List<Day>();
        }
        
        public class Day
        {
            public int Id { get; set; }
            public int TemplateId { get; set; }
            public DayOfWeek DayOfWeek { get; set; }
            public bool IsActive { get; set; }
            public List<Segment> Segments { get; set; } = new List<Segment>();
        }
        
        public class Segment
        {
            public int Id { get; set; }
            public int DayId { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public List<Slot> Slots { get; set; } = new List<Slot>();
        }
        
        public class Slot
        {
            public int Id { get; set; }
            public int DayId { get; set; }
            public int SegmentId { get; set; }
            public TimeSpan Duration { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public bool IsAvailable { get; set; }
        }
        
        // Simple service class for testing
        public class TemplateService
        {
            private List<Template> _templates = new List<Template>();
            private List<User> _users = new List<User>();
            private int _nextTemplateId = 1;
            private int _nextDayId = 1;
            private int _nextSegmentId = 1;
            private int _nextSlotId = 1;
            
            public TemplateService()
            {
                // Initialize with some test data
                _users.Add(new User { Id = "provider1", Name = "Dr. Provider", Email = "provider@example.com", IsServiceProvider = true });
                _users.Add(new User { Id = "provider2", Name = "Dr. Smith", Email = "smith@example.com", IsServiceProvider = true });
                
                // Create a template for provider1
                var template1 = new Template
                {
                    Id = _nextTemplateId++,
                    Name = "Weekly Schedule",
                    Description = "Standard weekly schedule",
                    ProviderId = "provider1",
                    Provider = _users[0],
                    Type = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                };
                
                // Add days to template1
                var monday = new Day
                {
                    Id = _nextDayId++,
                    TemplateId = template1.Id,
                    DayOfWeek = DayOfWeek.Monday,
                    IsActive = true
                };
                
                var wednesday = new Day
                {
                    Id = _nextDayId++,
                    TemplateId = template1.Id,
                    DayOfWeek = DayOfWeek.Wednesday,
                    IsActive = true
                };
                
                template1.Days.Add(monday);
                template1.Days.Add(wednesday);
                
                // Add segments to days
                var morningSegment = new Segment
                {
                    Id = _nextSegmentId++,
                    DayId = monday.Id,
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(12)
                };
                
                var afternoonSegment = new Segment
                {
                    Id = _nextSegmentId++,
                    DayId = monday.Id,
                    StartTime = TimeSpan.FromHours(13),
                    EndTime = TimeSpan.FromHours(17)
                };
                
                monday.Segments.Add(morningSegment);
                monday.Segments.Add(afternoonSegment);
                
                // Create a template for provider2
                var template2 = new Template
                {
                    Id = _nextTemplateId++,
                    Name = "Weekend Schedule",
                    Description = "Weekend availability",
                    ProviderId = "provider2",
                    Provider = _users[1],
                    Type = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                };
                
                // Add days to template2
                var saturday = new Day
                {
                    Id = _nextDayId++,
                    TemplateId = template2.Id,
                    DayOfWeek = DayOfWeek.Saturday,
                    IsActive = true
                };
                
                template2.Days.Add(saturday);
                
                _templates.Add(template1);
                _templates.Add(template2);
            }
            
            public List<Template> GetTemplatesForProvider(string providerId)
            {
                return _templates.FindAll(t => t.ProviderId == providerId);
            }
            
            public Template GetTemplateWithDetails(int templateId)
            {
                return _templates.Find(t => t.Id == templateId);
            }
            
            public Template CreateTemplate(Template template)
            {
                // Validate
                if (string.IsNullOrEmpty(template.Name))
                    throw new ArgumentException("Template name is required");
                    
                if (string.IsNullOrEmpty(template.ProviderId))
                    throw new ArgumentException("Provider ID is required");
                    
                var provider = _users.Find(u => u.Id == template.ProviderId);
                if (provider == null)
                    throw new ArgumentException("Provider not found");
                    
                if (!provider.IsServiceProvider)
                    throw new ArgumentException("User is not a service provider");
                
                // Set properties
                template.Id = _nextTemplateId++;
                template.CreatedAt = DateTime.UtcNow;
                template.Provider = provider;
                
                // Create default days if none provided
                if (template.Days == null || !template.Days.Any())
                {
                    template.Days = CreateDefaultDays(template.Id);
                }
                
                _templates.Add(template);
                return template;
            }
            
            public bool UpdateTemplate(Template template)
            {
                var existingTemplate = _templates.Find(t => t.Id == template.Id);
                if (existingTemplate == null)
                    return false;
                    
                // Update properties
                existingTemplate.Name = template.Name;
                existingTemplate.Description = template.Description;
                existingTemplate.Type = template.Type;
                existingTemplate.IsActive = template.IsActive;
                existingTemplate.UpdatedAt = DateTime.UtcNow;
                
                return true;
            }
            
            public bool DeleteTemplate(int templateId)
            {
                var template = _templates.Find(t => t.Id == templateId);
                if (template == null)
                    return false;
                    
                _templates.Remove(template);
                return true;
            }
            
            private List<Day> CreateDefaultDays(int templateId)
            {
                var days = new List<Day>();
                
                // Create Monday to Friday by default
                for (int i = 1; i <= 5; i++)
                {
                    var day = new Day
                    {
                        Id = _nextDayId++,
                        TemplateId = templateId,
                        DayOfWeek = (DayOfWeek)i,
                        IsActive = true
                    };
                    
                    // Add default segments (9 AM - 5 PM)
                    var segment = new Segment
                    {
                        Id = _nextSegmentId++,
                        DayId = day.Id,
                        StartTime = TimeSpan.FromHours(9),
                        EndTime = TimeSpan.FromHours(17)
                    };
                    
                    day.Segments.Add(segment);
                    days.Add(day);
                }
                
                return days;
            }
        }
        
        // Test methods
        private TemplateService _service;
        
        public void Setup()
        {
            _service = new TemplateService();
        }
        
        [TestMethod]
        public void GetTemplatesForProvider_ReturnsProviderTemplates()
        {
            // Act
            var templates = _service.GetTemplatesForProvider("provider1");
            
            // Assert
            Assert.AreEqual(1, templates.Count);
            Assert.AreEqual("Weekly Schedule", templates[0].Name);
            Assert.AreEqual("provider1", templates[0].ProviderId);
        }
        
        [TestMethod]
        public void GetTemplateWithDetails_WithValidId_ReturnsTemplateWithDetails()
        {
            // Act
            var template = _service.GetTemplateWithDetails(1);
            
            // Assert
            Assert.IsNotNull(template);
            Assert.AreEqual(1, template.Id);
            Assert.AreEqual("Weekly Schedule", template.Name);
            Assert.IsNotNull(template.Days);
            Assert.AreEqual(2, template.Days.Count);
            
            // Verify days
            var monday = template.Days.Find(d => d.DayOfWeek == DayOfWeek.Monday);
            Assert.IsNotNull(monday);
            Assert.AreEqual(2, monday.Segments.Count);
        }
        
        [TestMethod]
        public void CreateTemplate_WithValidData_CreatesTemplate()
        {
            // Arrange
            var template = new Template
            {
                Name = "New Test Template",
                Description = "A test template",
                ProviderId = "provider1",
                Type = false,
                IsActive = true
            };
            
            // Act
            var createdTemplate = _service.CreateTemplate(template);
            
            // Assert
            Assert.IsNotNull(createdTemplate);
            Assert.AreEqual("New Test Template", createdTemplate.Name);
            Assert.AreEqual("provider1", createdTemplate.ProviderId);
            Assert.IsNotNull(createdTemplate.Days);
            Assert.AreEqual(5, createdTemplate.Days.Count); // Default days (Mon-Fri)
        }
        
        [TestMethod]
        public void CreateTemplate_WithInvalidProvider_ThrowsException()
        {
            // Arrange
            var template = new Template
            {
                Name = "Invalid Template",
                Description = "A template with invalid provider",
                ProviderId = "nonexistent",
                Type = false,
                IsActive = true
            };
            
            try
            {
                // Act
                var createdTemplate = _service.CreateTemplate(template);
                
                // If we get here, the test should fail
                Assert.IsTrue(false, "Expected exception was not thrown");
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.IsTrue(ex.Message.Contains("Provider not found"));
            }
        }
        
        [TestMethod]
        public void UpdateTemplate_WithValidData_UpdatesTemplate()
        {
            // Arrange
            var template = _service.GetTemplateWithDetails(1);
            template.Name = "Updated Template Name";
            template.Description = "Updated description";
            
            // Act
            bool result = _service.UpdateTemplate(template);
            
            // Assert
            Assert.IsTrue(result);
            var updatedTemplate = _service.GetTemplateWithDetails(1);
            Assert.AreEqual("Updated Template Name", updatedTemplate.Name);
            Assert.AreEqual("Updated description", updatedTemplate.Description);
            Assert.IsNotNull(updatedTemplate.UpdatedAt);
        }
        
        [TestMethod]
        public void DeleteTemplate_WithValidId_DeletesTemplate()
        {
            // Act
            bool result = _service.DeleteTemplate(2);
            
            // Assert
            Assert.IsTrue(result);
            var templates = _service.GetTemplatesForProvider("provider2");
            Assert.AreEqual(0, templates.Count);
        }
    }
} 