namespace ChickenRush
{
    /// <summary>不依賴 Unity 的按住生成計時器，供生成器使用並可獨立驗證。</summary>
    public sealed class HoldSpawnInput
    {
        private bool wasHeld;
        private float anchorX;
        private double spawnCredit;
        public float HorizontalSpeed { get; private set; }
        public void Reset()
        {
            wasHeld = false;
            spawnCredit = 0;
            HorizontalSpeed = 0f;
        }

        /// <summary>
        /// normalizedX 是指標位置除以螢幕寬；dragWidth 是達到最大偏速所需的拖動比例。
        /// 按下立即生成一隻；之後累積 rate × deltaTime，保留餘數以避免幀率影響。
        /// 回傳本幀生成數。呼叫端應傳遊戲時間，暫停或停用時呼叫 Reset。
        /// </summary>
        public int Step(bool held, float normalizedX, float deltaTime, float rate, float dragWidth, float maxSpeed)
        {
            if (!held || rate <= 0f) { Reset(); return 0; }
            if (!wasHeld)
            {
                wasHeld = true;
                anchorX = normalizedX;
                HorizontalSpeed = 0f;
                return 1; // 不計入按下前的該幀時間。
            }
            float aim = (normalizedX - anchorX) / System.Math.Max(0.001f, dragWidth);
            HorizontalSpeed = System.Math.Max(-1f, System.Math.Min(1f, aim)) * System.Math.Max(0f, maxSpeed);
            spawnCredit += System.Math.Max(0f, deltaTime) * (double)rate;
            int count = (int)System.Math.Floor(spawnCredit + 0.000001);
            spawnCredit = System.Math.Max(0, spawnCredit - count);
            return count;
        }
    }
}
