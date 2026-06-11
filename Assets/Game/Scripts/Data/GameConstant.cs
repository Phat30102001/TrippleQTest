

    public enum UIEnum
    {
        WIN_UI, LOSE_UI, GAMEPLAY_UI
    }
    public enum GameSessionState { Loading, Playing, Won, Failed }

    public static class GameFailReason
    {
        public const string TimeExpired = "TimeExpired";
        public const string ConveyorOverflow = "ConveyorOverflow";
        public const string None = "None";
    }
