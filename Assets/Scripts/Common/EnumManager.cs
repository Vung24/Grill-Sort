public static class EnumManager
{
    public enum LevelState
    {
        None = 0,
        Playing = 1,
        RevivePanel = 2,
        Lose = 3,
        Win = 4
    }

    public enum LoseReason
    {
        None = 0,
        TimeUp = 1,
        OutOfSlot = 2,
        RevivePanel = 3
    }
}
