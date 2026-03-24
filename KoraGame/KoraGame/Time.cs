
namespace KoraGame
{
    public static class Time
    {
        // Private
        private const int fpsAverageSamples = 24; // Average over 60 frames

        private static float timeScale = 1f;
        private static float elapsedTime = 0f;
        private static float elapsedFixedTime = 0f;
        private static float deltaTime = 0f;
        private static float fixedDeltaTime = 0f;
        private static int frame = 0;

        private static readonly float[] fpsHistory = new float[fpsAverageSamples];
        private static int fpsHistoryIndex = 0;
        private static float fps = 0f;

        // Properties
        public static float TimeScale => timeScale;
        public static float ElapsedTime => elapsedTime;
        public static float FixedTime => elapsedFixedTime;
        public static float DeltaTime => deltaTime;
        public static float FixedDeltaTime => fixedDeltaTime;
        public static int Frame => frame;
        public static float FPS => fps;

        // Methods
        internal static void UpdateTime(float frameDelta)
        {
            // Apply time scale to delta time
            deltaTime = frameDelta * timeScale;

            // Accumulate elapsed time
            elapsedTime += deltaTime;

            // Increment frame counter
            frame++;

            // Update FPS calculation
            UpdateFPS(frameDelta);
        }

        internal static void UpdateFixedTime(float fixedFrameDelta)
        {
            // Apply time scale to fixed delta time
            fixedDeltaTime = fixedFrameDelta * timeScale;

            // Accumulate elapsed fixed time
            elapsedFixedTime += fixedDeltaTime;
        }

        private static void UpdateFPS(float frameDelta)
        {
            // Store frame time in circular buffer
            fpsHistory[fpsHistoryIndex] = frameDelta;
            fpsHistoryIndex = (fpsHistoryIndex + 1) % fpsAverageSamples;

            // Calculate average frame time
            float totalTime = 0f;
            int validSamples = Math.Min(frame, fpsAverageSamples);

            for (int i = 0; i < validSamples; i++)
            {
                totalTime += fpsHistory[i];
            }

            // Calculate FPS from average frame time
            if (totalTime > 0f && validSamples > 0)
            {
                fps = validSamples / totalTime;
            }
        }
    }
}
