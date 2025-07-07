namespace Stratum.Content.Particles;

public class Slash : Particle
{
    public override string Texture => "Stratum/Assets/Textures/Light";

    public override void SetDefaults()
    {
        width = 1;
        height = 1;
        timeLeft = 100;
    }

    private int timeAlive;

    public override void AI()
    {
        timeAlive++;

        if (timeAlive == 1)
        {
            velocity = velocity.SafeNormalize(Vector2.Zero) * (-Main.rand.NextFloat(8f, 12f) * 2);
        }

        velocity *= 0.94f;
        Scale *= 0.9f;

        // Always rotate to velocity
        rotation = velocity.ToRotation() + MathHelper.PiOver2;

        // Trail effect: shrink and fade
    }


    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawPos, Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        Vector2 drawPosition = Center - Main.screenPosition;

        float t = (float)(Math.Sin(Main.GameUpdateCount * 0.5f) * 0.5f + 0.5f);

        Main.EntitySpriteDraw(texture, drawPosition, texture.Frame(), new(255, 255, 255, 0), rotation, texture.Frame().Size() * 0.5f, scale * new Vector2(0.5f, 10), SpriteEffects.None, 0f);

        return false;
    }
}
