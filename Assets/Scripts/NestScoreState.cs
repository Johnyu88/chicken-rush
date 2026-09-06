using System;

namespace ChickenRush
{
    /// <summary>滿窩時鎖定獎勵，退場後才入帳；不依賴 Unity，可單獨測試。</summary>
    public sealed class NestScoreState
    {
        public int Score { get; private set; }
        public int Combo { get; private set; }
        public int PendingPoints { get; private set; }
        public bool HasPending { get; private set; }

        public bool Begin(int basePoints, int maxMultiplier)
        {
            if (HasPending) return false;
            Combo++;
            PendingPoints = Math.Max(1, basePoints) * Math.Min(Combo, Math.Max(1, maxMultiplier));
            HasPending = true;
            return true;
        }

        public int Settle()
        {
            if (!HasPending) return 0;
            int awarded = PendingPoints;
            Score += awarded;
            PendingPoints = 0;
            HasPending = false;
            return awarded;
        }

        public void Miss() { Combo = 0; }
        public float Pitch(float step)
        {
            return Math.Min(2f, 1f + Math.Max(0, Combo - 1) * Math.Max(0f, step));
        }
    }
}
