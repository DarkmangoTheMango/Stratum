using Terraria.DataStructures;

namespace Stratum.Content.Projectiles.Witch;

public class DeadlyLaser : ModProjectile
{
    Vector2[] beamPoints;
    int pointCount = 20;
    float beamLength = 800f;

    public float pointSpacing => beamLength / pointCount;

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults()
    {
        Projectile.Size = new(64);
        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
    }

    public override void OnSpawn(IEntitySource source)
    {
        beamPoints = new Vector2[pointCount];
        for (int i = 0; i < beamPoints.Length; i++)
            beamPoints[i] = Projectile.Center;
    }

    public override void AI()
    {
        if (Projectile.ai[0] > 0)
        {
            Projectile.Center = Main.projectile[(int)Projectile.ai[0]].Center;
        }

        for (int k = 0; k < 20; k++)
        {
            Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY * (k * 10), DustID.Torch, Vector2.Zero, 100, default, 3f).noGravity = true;
        }
    }

    private void UpdateBeamPoints()
    {
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);

        beamPoints[0] = Projectile.Center;

        for (int i = 1; i < beamPoints.Length; i++)
        {
            float t = i / (float)(beamPoints.Length - 1); // t = 0 (close) to 1 (far)
            float followStrength = MathHelper.Lerp(0.9f, 0.5f, t); // Closer points follow tighter

            Vector2 target = beamPoints[i - 1] + direction * pointSpacing;
            beamPoints[i] = Vector2.Lerp(beamPoints[i], target, followStrength);
        }
    }

    private void DrawDustTrail()
    {
        for (int i = 0; i < beamPoints.Length - 1; i++)
        {
            Vector2 start = beamPoints[i];
            Vector2 end = beamPoints[i + 1];

            for (float j = 0; j < 1f; j += 0.2f)
            {
                Vector2 pos = Vector2.Lerp(start, end, j);
                Dust.NewDustPerfect(pos, DustID.Torch, Vector2.Zero, 100, default, 1f).noGravity = true;
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false; // For now, we only draw dusts
    }
}
