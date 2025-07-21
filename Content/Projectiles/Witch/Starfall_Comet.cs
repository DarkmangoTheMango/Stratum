using CalamityMod.Particles;
using Stratum.Content.Particles;
using Terraria.Graphics;

namespace Stratum.Content.Projectiles.Witch;

public class Comet : ModProjectile
{
    public override string Texture => AssetUtils.Textures.Empty;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
    }

    public override void SetDefaults()
    {
        Projectile.Size = new(32);

        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;

        Projectile.netImportant = true;
        Projectile.aiStyle = -1;

        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, new Color(0, 255, 255).ToVector3() * 0.5f);
        Projectile.rotation = Projectile.velocity.ToRotation();

        if (Main.rand.NextBool(2))
        {
            if (Main.rand.NextBool(2))
                ParticleManager.NewParticle<Sparkle>(Projectile.Center, Main.rand.NextVector2Circular(1, 1) * 1, default, 0);

            Vector2 smokePos = Main.rand.NextVector2Circular(1, 1);
            Vector2 smokeSpeed = Vector2.Zero;
            CalamityMod.Particles.Particle smoke = new HeavySmokeParticle(Projectile.Center + smokePos, smokeSpeed, Color.Lerp(new(0, 0, 255), new(0, 0, 128), (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f)), 30, Main.rand.NextFloat(0.4f, 1f) * Projectile.scale, 0.8f, 0, false, 0, true);
            GeneralParticleHandler.SpawnParticle(smoke);

            CalamityMod.Particles.Particle smokeGlow = new HeavySmokeParticle(Projectile.Center + smokePos, smokeSpeed, new(0, 255, 255), 20, Main.rand.NextFloat(0.4f, 1f) * Projectile.scale, 0.8f, 0, true, 0.01f, true);
            GeneralParticleHandler.SpawnParticle(smokeGlow);
        }

        Projectile.velocity *= 1.01f;
    }

    static Color LerpThreeColors(Color colorA, Color colorB, Color colorC, float progress)
    {
        if (progress < 0.5f)
        {
            // Map 0..0.5 -> 0..1
            float t = progress / 0.5f;
            return Color.Lerp(colorA, colorB, t);
        }
        else
        {
            // Map 0.5..1 -> 0..1
            float t = (progress - 0.5f) / 0.5f;
            return Color.Lerp(colorB, colorC, t);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.oldPos[1] == Vector2.Zero)
            return false;

        Texture2D trailTexture = ModContent.Request<Texture2D>(AssetUtils.AssetPath + "/Textures/Trails/Ugly").Value;

        VertexStrip strip = new();

        strip.PrepareStripWithProceduralPadding(
            positions: Projectile.oldPos,
            rotations: Projectile.oldRot,
            colorFunction: progress =>
                LerpThreeColors(
                    new Color(128, 255, 255, 0) * 2,
                    new Color(0, 128, 255, 0) * 2,
                    new Color(0, 0, 255, 0) * 2,
                    progress
                    ),
            widthFunction: progress => 30,
            offsetForAllPositions: Projectile.Size / 2f - Main.screenPosition,
            includeBacksides: false,
            tryStoppingOddBug: true
        );

        Main.graphics.GraphicsDevice.Textures[0] = trailTexture;
        strip.DrawTrail();

        return false;
    }
}