using ReLogic.Utilities;
using Stratum.Content.Particles;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Stratum.Content.Projectiles.Witch;

public class DeadlyLaser : ModProjectile
{
    SlotId soundSlot;

    public override string Texture => Stratum.AssetPath + "/Textures/Noise/TurbulentNoise";

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(480f);
        Projectile.scale = 1f;

        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
    }

    public override void OnSpawn(IEntitySource source)
    {

    }

    public override void AI()
    {
        PlaySoundLoop();
    }

    void PlaySoundLoop()
    {
        if (!SoundEngine.TryGetActiveSound(soundSlot, out _))
        {
            var tracker = new ProjectileAudioTracker(Projectile);

            soundSlot = SoundEngine.PlaySound(new SoundStyle(Stratum.SoundPath + "/Custom/Witch/SolarHellstorm_SolarWindsLoop") with { IsLooped = true, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest }, Projectile.Center, soundInstance => {
                soundInstance.Position = Projectile.Center;
                return tracker.IsActiveAndInGame();
            });
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, Stratum.DeadlyLaser.Value, Main.GameViewMatrix.TransformationMatrix);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Bounds, Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale * 0.94f, SpriteEffects.None, 0);

        Stratum.DeadlyLaser.Value.Parameters["uTime"].SetValue(Main.GameUpdateCount / 40f);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        return false;
    }
}
