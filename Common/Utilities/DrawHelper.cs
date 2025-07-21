namespace Stratum.Common.Utilities;

public static class DrawHelper
{
    public static SpriteEffects ToSpriteEffect(this int value, SpriteEffects spriteEffect = SpriteEffects.FlipHorizontally) => value > 0 ? SpriteEffects.None : spriteEffect;

    public static void WithShader(Effect shader, Action draw)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, shader, Main.GameViewMatrix.TransformationMatrix);

        draw();

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }
}