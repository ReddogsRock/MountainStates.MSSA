namespace MountainStates.MSSA.Module.MSSA_Dogs.Enums
{
    // A Futurity nomination's payment status. Normally PendingPayment -> Paid, once
    // Stripe's webhook confirms the charge. A row can also be marked Paid directly by
    // an Admin recording an offline payment (check/cash), or marked Failed by an Admin
    // when a Stripe attempt didn't go through - the handler then retries via the "Pay
    // Now" action on the dog's Detail page, which re-fires a fresh checkout for the
    // same participation.
    public static class FuturityPaymentStatus
    {
        public const string PendingPayment = "PendingPayment";
        public const string Paid = "Paid";
        public const string Failed = "Failed";
    }
}
