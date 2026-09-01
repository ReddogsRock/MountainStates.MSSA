namespace MountainStates.MSSA.Module.MSSA_Dogs.Enums
{
    // A Futurity nomination's payment status. One-way in the normal case:
    // PendingPayment -> Paid, once Stripe's webhook confirms the charge. A row can also
    // be marked Paid directly by an Admin recording an offline payment (check/cash).
    public static class FuturityPaymentStatus
    {
        public const string PendingPayment = "PendingPayment";
        public const string Paid = "Paid";
    }
}
