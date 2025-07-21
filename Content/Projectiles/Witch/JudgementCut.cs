using Terraria.DataStructures;

namespace Stratum.Content.Projectiles.Witch;

public class JudgementCut : ModProjectile
{
    NPC Parent => Main.npc[(int)Projectile.ai[0]];

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 6;
    }

    public override void SetDefaults()
    {
        Projectile.Size = new(384, 384);

        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;

        Projectile.netImportant = true;
        Projectile.aiStyle = -1;

        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void OnSpawn(IEntitySource source)
    {

    }

    public override void AI()
    {
        if (!Parent.active)
            Projectile.Kill();

        if (++Projectile.frameCounter >= 5)
        {
            Projectile.frameCounter = 0;
            Projectile.frame++;

            if (Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.Kill();
        }

        Projectile.Center = Parent.Center;
        Projectile.velocity = Parent.velocity.SafeNormalize(Vector2.One);
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
        Projectile.spriteDirection = Projectile.direction;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        Rectangle sourceRectangle = new(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + (Projectile.velocity * 100), sourceRectangle, Projectile.GetAlpha(Color.White), Projectile.rotation, sourceRectangle.Size() / 2f, Projectile.scale, Projectile.spriteDirection.ToSpriteEffect(SpriteEffects.FlipVertically), 0);

        return false;
    }
}