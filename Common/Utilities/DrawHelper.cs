namespace Stratum.Common.Utilities;

public static class DrawHelper
{
    public static SpriteEffects ToSpriteEffect(this int value, SpriteEffects spriteEffect) => value > 0 ? SpriteEffects.None : spriteEffect;
}