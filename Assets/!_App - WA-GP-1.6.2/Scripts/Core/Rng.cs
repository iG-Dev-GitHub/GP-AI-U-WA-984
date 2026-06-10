namespace WorkoutDrop.Core
{
    /// <summary>
    /// Thin wrapper over UnityEngine.Random so the game logic mirrors the web app's
    /// <c>Math.random()</c> usage while remaining swappable (e.g. for deterministic tests).
    /// </summary>
    public interface IRng
    {
        /// <summary>Uniform value in [0, 1). Equivalent to JS <c>Math.random()</c>.</summary>
        float Value01();
    }

    /// <summary>Default RNG backed by UnityEngine.Random.</summary>
    public sealed class UnityRng : IRng
    {
        public static readonly UnityRng Shared = new UnityRng();

        public float Value01()
        {
            // UnityEngine.Random.value is inclusive of 1; nudge to keep it in [0,1) like Math.random().
            float v = UnityEngine.Random.value;
            return v >= 1f ? 0.9999999f : v;
        }
    }
}
