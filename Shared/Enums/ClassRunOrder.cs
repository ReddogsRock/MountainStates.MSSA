namespace MountainStates.MSSA.Module.MSSA_Entries.Enums
{
    // Fixed class run order: Open runs first, then Nursery, Intermediate, Novice, Junior.
    // Used both to generate a trial's run order and to sort the Results grid by
    // Class + Placing after scoring. Anything not in this list (legacy/unusual classes)
    // sorts after, alphabetically, so nothing silently vanishes.
    public static class ClassRunOrder
    {
        public static readonly string[] Names = { "Open", "Nursery", "Intermediate", "Novice", "Junior" };

        public static int IndexOf(string className)
        {
            var index = System.Array.IndexOf(Names, className);
            return index >= 0 ? index : int.MaxValue;
        }
    }
}
