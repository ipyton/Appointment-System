using System;
using System.Collections.Generic;

namespace SimpleTestsDemo
{
    public class AppointmentTests
    {
        // Simple model classes for testing
        public class User
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }
        
        public class Service
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int DurationMinutes { get; set; }
            public string ProviderId { get; set; }
            public bool IsActive { get; set; }
        }
        
        public class Appointment
        {
            public int Id { get; set; }
            public string UserId { get; set; }
            public User User { get; set; }
            public int ServiceId { get; set; }
            public Service Service { get; set; }
            public DateTime AppointmentDate { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Status { get; set; }
        }
        
        // Simple service class for testing
        public class AppointmentService
        {
            private List<Appointment> _appointments = new List<Appointment>();
            private List<Service> _services = new List<Service>();
            private List<User> _users = new List<User>();
            
            public AppointmentService()
            {
                // Initialize with some test data
                _users.Add(new User { Id = "user1", Name = "John Doe", Email = "john@example.com" });
                _users.Add(new User { Id = "user2", Name = "Jane Smith", Email = "jane@example.com" });
                _users.Add(new User { Id = "provider1", Name = "Dr. Provider", Email = "provider@example.com" });
                
                _services.Add(new Service 
                { 
                    Id = 1, 
                    Name = "Regular Checkup", 
                    Price = 100.00m, 
                    DurationMinutes = 30,
                    ProviderId = "provider1",
                    IsActive = true
                });
                
                _services.Add(new Service 
                { 
                    Id = 2, 
                    Name = "Specialist Consultation", 
                    Price = 200.00m, 
                    DurationMinutes = 60,
                    ProviderId = "provider1",
                    IsActive = true
                });
                
                _services.Add(new Service 
                { 
                    Id = 3, 
                    Name = "Inactive Service", 
                    Price = 150.00m, 
                    DurationMinutes = 45,
                    ProviderId = "provider1",
                    IsActive = false
                });
            }
            
            public List<Service> GetActiveServices()
            {
                return _services.FindAll(s => s.IsActive);
            }
            
            public Service GetServiceById(int id)
            {
                return _services.Find(s => s.Id == id);
            }
            
            public Appointment CreateAppointment(string userId, int serviceId, DateTime appointmentDate)
            {
                var user = _users.Find(u => u.Id == userId);
                if (user == null)
                    throw new ArgumentException("User not found");
                    
                var service = _services.Find(s => s.Id == serviceId);
                if (service == null)
                    throw new ArgumentException("Service not found");
                    
                if (!service.IsActive)
                    throw new ArgumentException("Service is not active");
                    
                var startTime = appointmentDate;
                var endTime = startTime.AddMinutes(service.DurationMinutes);
                
                // Check for conflicts
                bool hasConflict = _appointments.Exists(a => 
                    a.UserId == userId && 
                    ((startTime >= a.StartTime && startTime < a.EndTime) ||
                     (endTime > a.StartTime && endTime <= a.EndTime) ||
                     (startTime <= a.StartTime && endTime >= a.EndTime)));
                     
                if (hasConflict)
                    throw new InvalidOperationException("Appointment time conflicts with existing appointment");
                
                var appointment = new Appointment
                {
                    Id = _appointments.Count + 1,
                    UserId = userId,
                    User = user,
                    ServiceId = serviceId,
                    Service = service,
                    AppointmentDate = appointmentDate.Date,
                    StartTime = startTime,
                    EndTime = endTime,
                    Status = "Pending"
                };
                
                _appointments.Add(appointment);
                return appointment;
            }
            
            public List<Appointment> GetUserAppointments(string userId)
            {
                return _appointments.FindAll(a => a.UserId == userId);
            }
            
            public bool CancelAppointment(int appointmentId, string userId)
            {
                var appointment = _appointments.Find(a => a.Id == appointmentId && a.UserId == userId);
                if (appointment == null)
                    return false;
                    
                appointment.Status = "Cancelled";
                return true;
            }
        }
        
        // Test class fields
        private AppointmentService _service;
        
        public void Setup()
        {
            _service = new AppointmentService();
        }
        
        // Test methods
        public void TestGetActiveServices_ReturnsOnlyActiveServices()
        {
            // Arrange - done in Setup()
            
            // Act
            var services = _service.GetActiveServices();
            
            // Assert
            Assert.AreEqual(2, services.Count);
            Assert.IsTrue(services.TrueForAll(s => s.IsActive));
        }
        
        public void TestGetServiceById_WithValidId_ReturnsService()
        {
            // Act
            var service = _service.GetServiceById(1);
            
            // Assert
            Assert.IsNotNull(service);
            Assert.AreEqual(1, service.Id);
            Assert.AreEqual("Regular Checkup", service.Name);
        }
        
        public void TestGetServiceById_WithInvalidId_ReturnsNull()
        {
            // Act
            var service = _service.GetServiceById(999);
            
            // Assert
            Assert.IsNull(service);
        }
        
        public void TestCreateAppointment_WithValidData_CreatesAppointment()
        {
            // Arrange
            var userId = "user1";
            var serviceId = 1;
            var appointmentDate = DateTime.Now.AddDays(1);
            
            // Act
            var appointment = _service.CreateAppointment(userId, serviceId, appointmentDate);
            
            // Assert
            Assert.IsNotNull(appointment);
            Assert.AreEqual(userId, appointment.UserId);
            Assert.AreEqual(serviceId, appointment.ServiceId);
            Assert.AreEqual(appointmentDate, appointment.StartTime);
            Assert.AreEqual("Pending", appointment.Status);
        }
        
        public void TestCreateAppointment_WithInactiveService_ThrowsException()
        {
            // Arrange
            var userId = "user1";
            var serviceId = 3; // Inactive service
            var appointmentDate = DateTime.Now.AddDays(1);
            
            try
            {
                // Act
                _service.CreateAppointment(userId, serviceId, appointmentDate);
                
                // If we get here, the test should fail
                Assert.IsTrue(false, "Expected exception was not thrown");
            }
            catch (ArgumentException ex)
            {
                // Assert
                Assert.AreEqual("Service is not active", ex.Message);
            }
        }
        
        public void TestCreateAppointment_WithConflictingTime_ThrowsException()
        {
            // Arrange
            var userId = "user1";
            var serviceId = 1;
            var appointmentDate = DateTime.Now.AddDays(1);
            
            // Create first appointment
            _service.CreateAppointment(userId, serviceId, appointmentDate);
            
            try
            {
                // Act - try to create a conflicting appointment
                _service.CreateAppointment(userId, serviceId, appointmentDate);
                
                // If we get here, the test should fail
                Assert.IsTrue(false, "Expected exception was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                // Assert
                Assert.AreEqual("Appointment time conflicts with existing appointment", ex.Message);
            }
        }
        
        public void TestGetUserAppointments_ReturnsUserAppointments()
        {
            // Arrange
            var userId = "user1";
            var serviceId = 1;
            var appointmentDate = DateTime.Now.AddDays(1);
            
            // Create an appointment
            _service.CreateAppointment(userId, serviceId, appointmentDate);
            
            // Act
            var appointments = _service.GetUserAppointments(userId);
            
            // Assert
            Assert.IsNotNull(appointments);
            Assert.AreEqual(1, appointments.Count);
            Assert.AreEqual(userId, appointments[0].UserId);
        }
        
        public void TestCancelAppointment_WithValidId_CancelsAppointment()
        {
            // Arrange
            var userId = "user1";
            var serviceId = 1;
            var appointmentDate = DateTime.Now.AddDays(1);
            
            // Create an appointment
            var appointment = _service.CreateAppointment(userId, serviceId, appointmentDate);
            
            // Act
            var result = _service.CancelAppointment(appointment.Id, userId);
            
            // Assert
            Assert.IsTrue(result);
            
            // Verify the appointment status was updated
            var appointments = _service.GetUserAppointments(userId);
            Assert.AreEqual("Cancelled", appointments[0].Status);
        }
    }
} 