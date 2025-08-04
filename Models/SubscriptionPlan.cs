using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinBuddies.Models
{
    public class SubscriptionPlan
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MonthlyPrice { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal YearlyPrice { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal SetupFee { get; set; } = 0;
        
        public int BinsIncluded { get; set; }
        public int PickupsPerWeek { get; set; }
        
        public bool IncludesHolidayAdjustment { get; set; }
        public bool IncludesGPSTracking { get; set; }
        public bool IncludesSMSNotifications { get; set; }
        public bool IncludesEmailNotifications { get; set; }
        public bool IncludesMobileApp { get; set; }
        public bool IncludesPrioritySupport { get; set; }
        public bool IncludesAccountManager { get; set; }
        public bool IncludesServiceReports { get; set; }
        
        [StringLength(50)]
        public string SupportLevel { get; set; } = "Email"; // Email, Priority, 24/7
        
        public bool IsActive { get; set; } = true;
        public bool IsPopular { get; set; } = false;
        public bool IsEnterprise { get; set; } = false;
        
        [StringLength(7)]
        public string BadgeColor { get; set; } = "#007bff"; // Bootstrap color
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
    
    public class CustomerProfile
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? ServiceAddress { get; set; }
        
        [StringLength(100)]
        public string? ServiceCity { get; set; }
        
        [StringLength(20)]
        public string? ServiceState { get; set; }
        
        [StringLength(10)]
        public string? ServiceZipCode { get; set; }
        
        [StringLength(50)]
        public string? PreferredServiceDay { get; set; } // Monday, Tuesday, etc.
        
        [StringLength(50)]
        public string? PreferredServiceTime { get; set; } // Morning, Afternoon, Evening
        
        [StringLength(500)]
        public string? SpecialInstructions { get; set; }
        
        public bool AllowWeekendService { get; set; } = false;
        public bool AllowHolidayService { get; set; } = true;
        
        [StringLength(20)]
        public string NotificationPreference { get; set; } = "Both"; // SMS, Email, Both
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
    
    public class ServiceArea
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string City { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string State { get; set; } = string.Empty;
        
        [StringLength(10)]
        public string ZipCode { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal ServiceMultiplier { get; set; } = 1.0m; // Price adjustment for area
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
