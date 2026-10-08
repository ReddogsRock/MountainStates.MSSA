using System;

namespace MountainStates.MSSA.Module.MSSA_Dogs.Models
{
    // Admin recording an offline Futurity payment (check/cash/etc.) directly, bypassing
    // Stripe entirely.
    public class MarkFuturityPaymentManualDto
    {
        public int ParticipationId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime DateReceived { get; set; }
    }
}
