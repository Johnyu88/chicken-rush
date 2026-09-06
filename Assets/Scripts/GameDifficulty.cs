using System;

namespace ChickenRush
{
    public enum GameDifficulty { Relaxed, Hell, Demon, Extreme }

    public struct GameDifficultySettings
    {
        public int ObstacleCount { get; private set; }
        public float NestSpeed { get; private set; }
        public float BaseBounciness { get; private set; }

        private GameDifficultySettings(int count, float speed, float bounce)
        { ObstacleCount = count; NestSpeed = speed; BaseBounciness = bounce; }

        public static GameDifficultySettings For(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Relaxed: return new GameDifficultySettings(0, 0f, 0.4f);
                case GameDifficulty.Hell: return new GameDifficultySettings(1, 1.5f, 0.4f);
                case GameDifficulty.Demon: return new GameDifficultySettings(2, 2.5f, 0.55f);
                case GameDifficulty.Extreme: return new GameDifficultySettings(3, 4f, 0.7f);
                default: throw new ArgumentOutOfRangeException(nameof(difficulty));
            }
        }
    }
}
