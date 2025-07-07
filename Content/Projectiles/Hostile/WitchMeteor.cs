using Stratum.Content.Particles;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Stratum.Content.Projectiles.Hostile;

public class WitchMeteor : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
    }

    public override void SetDefaults()
    {
        Projectile.Size = new(172);
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
    }

    public override void OnSpawn(IEntitySource source)
    {
        SoundEngine.PlaySound(new SoundStyle("Stratum/Assets/Sounds/Custom/Witch/MeteorFall"));
    }

    public override void AI()
    {
        Projectile.rotation += 0.01f;
        float t = (float)(Math.Sin(Main.GameUpdateCount * 0.5f) * 0.5f + 0.5f);
        Color interpolatedColor = Color.Lerp(new(0, 0, 255, 0), new(0, 255, 255, 0), t);

        Lighting.AddLight(Projectile.Center, interpolatedColor.ToVector3());
        ParticleManager.NewParticle<StarParticle>(Projectile.Center + (Main.rand.NextVector2Circular(1, 1) * (172 * 0.5f)), Projectile.velocity.RotatedByRandom(0.2f) * -Main.rand.Next(1, 2), default, 0.2f);
    }

    public override void OnKill(int timeLeft)
    {
        CameraSystem.ScreenShake(100, 0.95f, Projectile.Center);
        CameraSystem.ScreenFlash(1, 0.01f);
        SoundEngine.PlaySound(new SoundStyle("Stratum/Assets/Sounds/Custom/Witch/MeteorHit"), Projectile.Center);

        ParticleManager.NewParticle<StarParticle>(Projectile.Center, Vector2.Zero, default, 40);
        ParticleManager.NewParticle<Flash>(Projectile.Center, Vector2.Zero, default, 0);

        for (int k = 0; k < 30; k++)
        {
            ParticleManager.NewParticle<StarParticle>(Projectile.Center, Main.rand.NextVector2Unit(-(float)MathHelper.Pi, (float)MathHelper.Pi) * Main.rand.NextFloat() * 60, default, 1);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        int frameY = frameHeight * Projectile.frame;

        Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        float pulse = 1f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.5f);

        float t = (float)(Math.Sin(Main.GameUpdateCount * 0.5f) * 0.5f + 0.5f);
        Color interpolatedColor = Color.Lerp(new(0, 0, 255, 0), new(0, 255, 255, 0), t);

        Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle, Projectile.GetAlpha(interpolatedColor), Projectile.rotation, sourceRectangle.Size() * 0.5f, Projectile.scale * pulse, SpriteEffects.None, 0f);

        Color color = new(255, 255, 255, 255);
        Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle, Projectile.GetAlpha(color), Projectile.rotation, sourceRectangle.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0f);

        return false;
    }
}
