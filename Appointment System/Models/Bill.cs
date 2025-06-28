using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Appointment_System.Models
{
    public class Bill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public BillStatus Status { get; set; } = BillStatus.Pending;

        public PaymentMethod? PaymentMethod { get; set; }

        [StringLength(100)]
        public string TransactionId { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }

    public enum BillStatus
    {
        Pending,
        Paid,
        Refunded,
        Failed,
        Cancelled
    }

    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        Cash,
        BankTransfer,
        MobilePayment,
        Other
    }
} 