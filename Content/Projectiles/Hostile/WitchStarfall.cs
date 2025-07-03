using Microsoft.Xna.Framework.Graphics;
using Stratum.Content.Particles;
using Stratum.Core;
using Stratum.Core.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;

namespace Stratum.Content.Projectiles.Hostile
{
    public class WitchStarfall : ModProjectile
    {
        public override string Texture => "Stratum/Assets/Textures/Star";

        public override void SetDefaults()
        {
            Projectile.Size = new(64);
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, new Color(0, 255, 255).ToVector3() * 0.5f);
        }

        public override void Kill(int timeLeft)
        {
            CameraSystem.ScreenShake(20, 0.9f, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item62 with { MaxInstances = 5 }, Projectile.Center);

            for (int k = 0; k < 20; k++)
            {
                ParticleManager.NewParticle<StarParticle>(Projectile.Center, Main.rand.NextVector2Circular(1, 1) * 30, default, 1);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            float pulse = 1f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.5f);

            float t = (float)(Math.Sin(Main.GameUpdateCount * 0.5f) * 0.5f + 0.5f);
            Color interpolatedColor = Color.Lerp(new(0, 0, 255, 0), new(0, 255, 255, 0), t);

            Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle, Projectile.GetAlpha(interpolatedColor), Projectile.rotation, sourceRectangle.Size() * 0.5f, Projectile.scale * pulse, SpriteEffects.None, 0f);

            Color color = new(255, 255, 255, 0);
            Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle, Projectile.GetAlpha(color), Projectile.rotation, sourceRectangle.Size() * 0.5f, (Projectile.scale * pulse) * 0.9f, SpriteEffects.None, 0f);

            return false;
        }

        void DrawTrail()
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture + "Trail").Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int frameY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            float pulse = 1f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.5f);

            float t = (float)(Math.Sin(Main.GameUpdateCount * 0.5f) * 0.5f + 0.5f);
            Color interpolatedColor = Color.Lerp(new(0, 0, 255, 0), new(0, 255, 255, 0), t);

            Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle, Projectile.GetAlpha(interpolatedColor), Projectile.rotation, sourceRectangle.Size() * 0.5f, new Vector2(0.3f, 5) * (Projectile.scale * pulse), SpriteEffects.None, 0f);

            Color color = new(255, 255, 255, 0);
            Main.EntitySpriteDraw(texture, drawPosition, sourceRectangle, Projectile.GetAlpha(color), Projectile.rotation, sourceRectangle.Size() * 0.5f, new Vector2(0.3f, 5) * (Projectile.scale * pulse) * 0.9f, SpriteEffects.None, 0f);
        }
    }
}
