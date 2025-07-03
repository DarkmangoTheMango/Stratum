using Microsoft.Xna.Framework.Graphics;
using Stratum.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stratum.Content.Particles
{
    public class StarParticle : Particle
    {
        public override string Texture => "Stratum/Assets/Textures/Star";

        public override void SetDefaults()
        {
            width = 1;
            height = 1;
            timeLeft = 100;
        }

        public override void AI()
        {
            velocity *= 0.9f;
            scale *= 0.9f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawPos, Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float pulse = 1f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.5f);

            float t = (float)(Math.Sin(Main.GameUpdateCount * 0.5f) * 0.5f + 0.5f);
            Color interpolatedColor = Color.Lerp(new(0, 0, 255, 0), new(0, 255, 255, 0), t);

            Main.EntitySpriteDraw(texture, drawPos, texture.Frame(), interpolatedColor, rotation, texture.Frame().Size() * 0.5f, scale * pulse, SpriteEffects.None, 0f);

            Color color = new(255, 255, 255, 0);
            Main.EntitySpriteDraw(texture, drawPos, texture.Frame(), color, rotation, texture.Frame().Size() * 0.5f, (scale * pulse) * 0.9f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
