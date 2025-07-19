using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics;

namespace Stratum.Content.Projectiles.Witch;

public class SolarFlare : ModProjectile
{
    public override string Texture => AssetUtils.AssetPath + "/Textures/Empty";

    public bool NoHoming = false;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
    }

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(32f);
        Projectile.scale = 1f;

        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
        Projectile.hide = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        SoundEngine.PlaySound(new SoundStyle(AssetUtils.SoundPath + "/Custom/Witch/SolarHellstorm_SolarFlare") with { PitchVariance = 0.2f, MaxInstances = 5 }, Projectile.Center);
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();

        float MaxSpeed = 18f;
        int HomingDuration = 45;
        float HomingStrength = 0.1f;
        float AccelerationDuration = 30f; // frames over which it speeds up

        Projectile.ai[0]++;

        if (!NoHoming)
        {
            // Original homing logic here...
            if (Projectile.ai[0] < HomingDuration)
            {
                Player target = null;

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.dead && !player.ghost)
                        target = player;
                }

                if (target != null)
                {
                    Vector2 desiredVelocity = Projectile.DirectionTo(target.Center) * Projectile.velocity.Length();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, HomingStrength);
                }
            }
            else if (Projectile.ai[0] == HomingDuration)
            {
                if (Projectile.velocity != Vector2.Zero)
                {
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;
                }
            }
        }
        else
        {
            Projectile.ai[1]++;

            // Non-homing flare: start slow and accelerate straight
            float startSpeed = 4f; // initial slow speed
            float t = MathHelper.Clamp(Projectile.ai[1] / AccelerationDuration, 0f, 1f);
            float currentSpeed = MathHelper.Lerp(startSpeed, MaxSpeed, t);

            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * currentSpeed;
            }
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindProjectiles.Add(index);
    }

    Color LerpThreeColors(Color colorA, Color colorB, Color colorC, float progress)
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

        VertexStrip strip = new VertexStrip();

        strip.PrepareStripWithProceduralPadding(
            positions: Projectile.oldPos,
            rotations: Projectile.oldRot,
            colorFunction: progress =>
                LerpThreeColors(
                    new Color(255, 255, 128, 0) * 2,
                    new Color(255, 128, 0, 0) * 2,
                    new Color(255, 0, 0, 0) * 2,
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

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        int positionToUse = 5;

        hitbox = new Rectangle(
            (int)(Projectile.oldPos[positionToUse].X + Projectile.Size.X / 2f - Projectile.width / 2f),
            (int)(Projectile.oldPos[positionToUse].Y + Projectile.Size.Y / 2f - Projectile.height / 2f),
            Projectile.width,
            Projectile.height
        );
    }
}