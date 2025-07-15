
namespace Stratum.Content.Particles;

public class Sparkle : Particle
{
    public override string Texture => Stratum.AssetPath + "/Textures/Light";

    public override void SetDefaults()
    {
        width = 1;
        height = 1;
        timeLeft = 100;
    }

    public override void AI()
    {
        velocity *= 0.9f;

        if (timeLeft > 80)
            Scale += 0.04f;
        else
            scale *= 0.95f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawPos, Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        float scaleMult = (MathF.Sin(Main.GameUpdateCount / 2) + 1.5f) * 1;
        Main.EntitySpriteDraw(texture, Center - Main.screenPosition, texture.Bounds, Color.Lerp(new Color(0, 0, 255, 0), new Color(128, 255, 255, 0), Scale), rotation + MathHelper.PiOver4, texture.Size() * 0.5f, new Vector2(0.5f, 1) * scale, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(texture, Center - Main.screenPosition, texture.Bounds, Color.Lerp(new Color(0, 0, 255, 0), new Color(128, 255, 255, 0), Scale), rotation - MathHelper.PiOver4, texture.Size() * 0.5f, new Vector2(0.5f, 1) * scale, SpriteEffects.None, 0f);

        return false;
    }
}
