namespace MountainStates.MSSA.Module.MSSA_Events.Enums
{
    public static class StockType
    {
        public const string Cattle = "Cattle";
        public const string Sheep = "Sheep";

        public static string[] All => new[] { Cattle, Sheep };
    }
}