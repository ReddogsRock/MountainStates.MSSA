namespace MountainStates.MSSA.Module.MSSA_Handlers.Enums
{
    public static class HandlerLevel
    {
        public const string Junior = "Junior";
        public const string Novice = "Novice";
        public const string Intermediate = "Intermediate";
        public const string Open = "Open";

        public static string[] All => new[] { Junior, Novice, Intermediate, Open };
    }
}
