using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stratum.Content.Projectiles.Witch;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Stratum.Content.Particles;

public class StarCollapseFlash : Particle
{
    const int Lifetime = 40;
    const float ScaleGrowthRate = 0.05f;
    const float OpacityFadeRate = 0.05f;
    const float DrawScale = 4f;

    public override string Texture => Stratum.AssetPath + "/Textures/Bloom";

    public override void SetDefaults()
    {
        width = 1;
        height = 1;
        timeLeft = Lifetime;
    }

    public override void AI()
    {
        layer = Layer.BeforeDust;

        Lighting.AddLight(Center, new Color(255, 0, 0).ToVector3() * Scale);

        Scale += ScaleGrowthRate * ai[0];
        opacity -= OpacityFadeRate;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 drawPos, Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        Vector2 origin = texture.Size() * 0.5f;
        Vector2 screenPos = Center - Main.screenPosition;

        float finalScale = Scale * DrawScale;

        Main.EntitySpriteDraw(texture, screenPos, texture.Bounds, new Color(255, 128, 0, 0) * opacity, rotation, origin, finalScale, SpriteEffects.None, 0f);

        Main.EntitySpriteDraw(texture, screenPos, texture.Bounds, new Color(255, 255, 128, 0) * opacity, rotation, origin, finalScale, SpriteEffects.None, 0f);

        return false;
    }
}