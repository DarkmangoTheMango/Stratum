namespace Stratum.Common.Utilities;

/// <summary>
/// A collection of mathematical utilities and constants.
/// </summary>
public class MathUtils
{
    public static float SecondsToTicks(float value) => value * 60;

    public static Vector2 RandomVector2Circular(float scale) => Main.rand.NextVector2Circular(1, 1) * scale;

    public static Vector2 RandomVector2CircularEdge(float scale) => Main.rand.NextVector2CircularEdge(1, 1) * scale;
}
