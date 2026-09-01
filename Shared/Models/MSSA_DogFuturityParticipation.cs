using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MountainStates.MSSA.Module.MSSA_Dogs.Enums;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Models
{
    public class MSSA_DogFuturityParticipation
    {
        [Key]
        public int ParticipationId { get; set; }

        [Required]
        public int DogId { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        // Age-proof documentation (optional; can be added at enrollment or later)
        [StringLength(500)]
        public string DocumentFileName { get; set; }

        [StringLength(500)]
        public string DocumentPath { get; set; }

        public DateTime? DocumentUploadedDate { get; set; }

        // Payment (see FuturityPaymentStatus). Set to PendingPayment on creation and
        // moved to Paid either by the Stripe webhook once checkout completes, or by an
        // Admin recording an offline payment (check/cash) directly.
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = FuturityPaymentStatus.PendingPayment;

        // "Stripe", "Check", "Cash", etc. Null until a payment is actually recorded.
        [StringLength(20)]
        public string PaymentMethod { get; set; }

        public decimal? Amount { get; set; }

        [StringLength(100)]
        public string PaidBy { get; set; }

        public DateTime? DateReceived { get; set; }

        [StringLength(255)]
        public string StripePaymentIntentId { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Transient carriers used only for the upload round-trip (not persisted).
        // Kept on this model, rather than a separate DTO, so the client can post
        // and receive the same type T through ServiceBase's PostJsonAsync<T>.
        [NotMapped]
        public string UploadFileName { get; set; }

        [NotMapped]
        public string UploadContentBase64 { get; set; }
    }
}
