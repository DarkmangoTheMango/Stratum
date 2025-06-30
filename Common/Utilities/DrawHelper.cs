using Microsoft.Xna.Framework.Graphics;

namespace Stratum.Common.Utilities
{
    public static class DrawHelper
    {
        public static SpriteEffects ToHorizontalFlip(this int value) => value == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
    }
}