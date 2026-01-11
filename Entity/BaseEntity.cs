using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentVerificationSystemApi.Entity
{
	public class BaseEntity
	{
		public Guid Id { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedDate { get; set; }
		public Guid TanetId { get; set; }

		[ForeignKey(nameof(TanetId))]
		public TanetEntity Tanet { get; set; }
	}
}
