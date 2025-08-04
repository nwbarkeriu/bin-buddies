using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinBuddies.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContactId { get; set; }

        [StringLength(50)]
        public string? TrashDay { get; set; }

        [StringLength(255)]
        public string? AccountRepresentative { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ContactId")]
        public virtual Contact Contact { get; set; } = null!;
        
        public virtual AccountRepresentative? AccountRep { get; set; }
        public virtual ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();
    }
}
