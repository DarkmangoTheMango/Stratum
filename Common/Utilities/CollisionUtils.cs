namespace Stratum.Common.Utilities;

public static class CollisionUtils
{
    /// <summary>
    /// Checks if a circle intersects a given rectangle.
    /// </summary>
    /// <param name="center">Center of the circle in world space.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="hitbox">The rectangle to check against.</param>
    public static bool CircleAABBIntersect(Vector2 center, float radius, Rectangle hitbox) =>
        Vector2.DistanceSquared(center, new Vector2(MathHelper.Clamp(center.X, hitbox.Left, hitbox.Right), MathHelper.Clamp(center.Y, hitbox.Top, hitbox.Bottom))) < radius * radius;
}