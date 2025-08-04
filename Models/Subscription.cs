using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinBuddies.Models
{
    public class Subscription
    {
        [Key] 
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string PlanName { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MonthlyPrice { get; set; }
        
        [Required]
        public SubscriptionStatus Status { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime NextBillingDate { get; set; }
        
        [StringLength(100)]
        public string? StripeSubscriptionId { get; set; }
        
        [StringLength(100)] 
        public string? StripeCustomerId { get; set; }
        
        [StringLength(50)]
        public string BillingCycle { get; set; } = "monthly"; // monthly, yearly
        
        public int BinsIncluded { get; set; } = 1;
        public int PickupsPerWeek { get; set; } = 1;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }

    public class Payment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int SubscriptionId { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        public PaymentStatus Status { get; set; }
        
        [StringLength(100)]
        public string? StripePaymentIntentId { get; set; }
        
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Navigation properties
        [ForeignKey("SubscriptionId")]
        public virtual Subscription Subscription { get; set; } = null!;
    }

    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public int? SubscriptionId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; } = string.Empty; // "pickup", "delivery", "maintenance"
        
        [Required]
        public ServiceRequestStatus Status { get; set; }
        
        public DateTime RequestedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        [StringLength(450)]
        public string? AssignedEmployeeId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [ForeignKey("SubscriptionId")]
        public virtual Subscription? Subscription { get; set; }
        
        [ForeignKey("AssignedEmployeeId")]
        public virtual ApplicationUser? AssignedEmployee { get; set; }
    }

    public enum SubscriptionStatus
    {
        Active,
        Paused,
        Cancelled,
        PastDue,
        Trial
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public enum ServiceRequestStatus
    {
        Pending,
        Scheduled,
        Assigned,
        InProgress,
        Completed,
        Cancelled
    }
}
