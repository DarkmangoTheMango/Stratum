using Stratum.Content.Particles;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Stratum.Content.Projectiles.Witch;

public class Supernova : ModProjectile
{
    const int MaxTime = 40;
    const int EaseInDuration = 20;
    const int FadeInDelay = 10;
    const int FadeOutStart = 20;
    const float MaxScale = 3f;
    const float BaseRadius = 240f;
    
    float intensity;

    public override string Texture => AssetUtils.AssetPath + "/Textures/Noise/TurbulentNoise";

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(BaseRadius * 2f);
        Projectile.scale = 1f;
        Projectile.timeLeft = MaxTime;

        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
    }

    public override void OnSpawn(IEntitySource source)
    {
        ParticleManager.NewParticle<StarCollapseFlash>(Projectile.Center, Vector2.Zero, default, 0, 1, 1);

        CameraSystem.ScreenShake(60, 0.95f, Projectile.Center);
        CameraSystem.ScreenFlash(1f);

        SoundEngine.PlaySound(new SoundStyle(AssetUtils.SoundPath + "/Custom/Witch/SolarHellstorm_Supernova"), Projectile.Center);

        SpawnParticles();

        Projectile.scale = 0f;
    }
    
    void SpawnParticles()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(20f, 40f);
            float scale = Main.rand.NextFloat(3f, 4f);
            ParticleManager.NewParticle<SolarEmber>(Projectile.Center, velocity, default, scale);
        }
    }

    public override void AI()
    {
        float scaleLerp = MathHelper.Clamp((MaxTime - Projectile.timeLeft) / (float)(MaxTime - EaseInDuration), 0f, 1f);

        Projectile.scale = MathF.Sin(scaleLerp * MathHelper.PiOver2) * MaxScale;

        int fadeElapsed = Math.Max(0, MaxTime - Projectile.timeLeft - FadeInDelay);
        float fadeProgress = MathHelper.Clamp(fadeElapsed / (float)(MaxTime - FadeInDelay), 0f, 1f);

        intensity = MathHelper.Lerp(0.1f, 1f, fadeProgress);

        float fadeOutProgress = MathHelper.Clamp((Projectile.timeLeft - FadeOutStart) / (float)(MaxTime - FadeOutStart), 0f, 1f);
        float easedFade = MathF.Sin(fadeOutProgress * MathHelper.PiOver2);

        Lighting.AddLight(Projectile.Center, new Color(255, 0, 0).ToVector3() * 5f * easedFade * Projectile.scale);
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        float radius = Projectile.scale * BaseRadius;
        hitbox = new Rectangle((int)(Projectile.Center.X - radius), (int)(Projectile.Center.Y - radius), (int)(radius * 2f), (int)(radius * 2f));
    }

    public override bool CanHitPlayer(Player target)
    {
        if (intensity >= 0.5f)
            return false;

        float radius = Projectile.scale * BaseRadius;
        float distance = Vector2.Distance(target.Center, Projectile.Center);

        return distance <= radius;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, ShaderManager.Supernova.Value, Main.GameViewMatrix.TransformationMatrix);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Bounds, Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale * 0.94f, SpriteEffects.None, 0);

        ShaderManager.Supernova.Value.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
        ShaderManager.Supernova.Value.Parameters["intensity"].SetValue(intensity);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        return false;
    }
}