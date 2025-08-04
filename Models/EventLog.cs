using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinBuddies.Models
{
    public class EventLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int RepId { get; set; }

        [Required]
        [StringLength(50)]
        public string EventType { get; set; } = string.Empty; // "Take Out" or "Bring In"

        [Required]
        public DateTime EventDate { get; set; }

        public bool Completed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Contact Customer { get; set; } = null!;

        [ForeignKey("RepId")]
        public virtual AccountRepresentative AccountRep { get; set; } = null!;
    }
}
