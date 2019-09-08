namespace Assets.Scripts
{
    public static class GameState
    {
        public enum CurrentState
        {
            Menu,
            Starting,
            Playing,
            Winning,
            Ending,
            LevelChange,
            BadGuyEnding,
            Paus
        }

        // Game Consts
        public const int firstLineX = -28;
        public const int lastLineX = 28;
        public const float PlayerSpeed = 50;

        // Game Variables
        public static int CurrentLevel;
        public static CurrentState currentState;
        public static float SpawnSpeed;
        public static bool GameInitialized;
        public static int TotalBricksDestroyed;
        public static int ActiveBricks;

        public static void SetSpawnSpeed()
        {
            SpawnSpeed = 2 - CurrentLevel * 0.2f;
            if (SpawnSpeed < 0.2f)
                SpawnSpeed = 0.2f;
        }

    }
}
