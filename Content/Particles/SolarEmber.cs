namespace Stratum.Content.Particles;

public class SolarEmber : Particle
{
    const int Lifetime = 100;
    const float VelocityDecay = 0.95f;
    const float ScaleDecay = 0.95f;
    const float ChaosFactor = 0.2f;
    const float MinVisibleScale = 0.1f;
    const float DrawScale = 4f;
    const float BloomScaleMultiplier = 0.05f;

    bool initialized = false;
    float initialSpeed = 0f;

    public override string Texture => AssetUtils.AssetPath + "/Textures/Pixel";

    public override void SetDefaults()
    {
        width = 1;
        height = 1;
        timeLeft = Lifetime;
    }

    public override void AI()
    {
        layer = Layer.BeforeProjectiles;

        Lighting.AddLight(Center, new Color(255, 0, 0).ToVector3() * Scale);

        if (!initialized)
        {
            initialSpeed = velocity.Length();
            initialized = true;
        }

        float chaosStrength = initialSpeed * ChaosFactor;
        float angle = Main.rand.NextFloat(MathHelper.TwoPi);

        velocity += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Main.rand.NextFloat(0f, chaosStrength);

        velocity *= VelocityDecay;
        scale *= ScaleDecay;

        float maxSpeed = initialSpeed * 1.5f;

        if (velocity.Length() > maxSpeed)
            velocity = Vector2.Normalize(velocity) * maxSpeed;

        if (Scale < MinVisibleScale)
            Scale = 0f;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawPos, Color lightColor)
    {
        DrawBloom();

        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        Main.EntitySpriteDraw(texture, Center - Main.screenPosition, texture.Bounds, new Color(255, 255, 128, 0), rotation, texture.Size() * 0.5f, Math.Clamp(Scale * 2, 0, 2), SpriteEffects.None, 0f);

        return false;
    }
    
    void DrawBloom()
    {
        Texture2D texture = ModContent.Request<Texture2D>(AssetUtils.AssetPath + "/Textures/Bloom").Value;

        Main.EntitySpriteDraw(texture, Center - Main.screenPosition, texture.Bounds, new Color(255, 128, 0, 0) * 0.5f, rotation, texture.Size() * 0.5f, scale * BloomScaleMultiplier, SpriteEffects.None, 0f);
    }
}