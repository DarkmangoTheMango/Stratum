using Stratum.Common.Utilities;
using Stratum.Content.Particles;
using Stratum.Content.Projectiles.Hostile;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Stratum.Content.NPCs.Bosses.Witch;

[AutoloadBossHead]
public class Witch : ModNPC
{
    enum AIState
    {
        Spawn,
        Shield,
        Portal,
        Starshower,
        Shattered,
        Debris,
        Solaris,
        Astrea,
        PhaseCheckpoint,
        HarmonyCheckpoint,
        Death
    }

    AIState state = AIState.Spawn;

    Player player = null;

    public override void SetStaticDefaults()
    {
        NPCID.Sets.MPAllowedEnemies[Type] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        NPCID.Sets.BossBestiaryPriority.Add(Type);
    }

    public override void SetDefaults()
    {
        NPC.Size = new(34, 48);
        NPC.damage = 0;
        NPC.knockBackResist = 0;
        NPC.defense = 777;
        NPC.lifeMax = 2000000;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.npcSlots = 6;
        NPC.boss = true;
        NPC.BossBar = ModContent.GetInstance<WitchBossBar>();
        NPC.value = Item.buyPrice(0, 5, 0, 0);
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;

        if (!Main.dedServ)
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Witch");
    }

    public override void OnSpawn(IEntitySource source)
    {

    }

    public override void AI()
    {
        Lighting.AddLight(NPC.Center, Color.White.ToVector3() * 0.5f);
        NPC.TargetClosest(true);
        NPC.spriteDirection = NPC.direction;
        player = Main.player[NPC.target];

        switch (state)
        {
            case AIState.Spawn:
                Spawn();
                break;
            case AIState.Shield:
                Shield();
                break;
            case AIState.Portal:
                Portal();
                break;
            case AIState.Starshower:
                Starshower();
                break;
            case AIState.Shattered:
                Shattered();
                break;
            case AIState.Debris:
                Debris();
                break;
            case AIState.Solaris:
                Solaris();
                break;
            case AIState.Astrea:
                Astrea();
                break;
            case AIState.PhaseCheckpoint:
                PhaseCheckpoint();
                break;
            case AIState.HarmonyCheckpoint:
                HarmonyCheckpoint();
                break;
            case AIState.Death:
                Death();
                break;
            default:
                break;
        }
    }

    #region AIStates

    void Spawn()
    {
        NPC.dontTakeDamage = true;
        NPC.ai[0]++;

        Main.NewText("I'm in my spawn phase!");

        if (NPC.ai[0] >= MathUtils .SecondsToTicks(2))
        {
            state = AIState.Shield;
            NPC.dontTakeDamage = false;
            NPC.ai[0] = 0;
        }
    }

    void Shield()
    {
        if (NPC.ai[0] == MathUtils .SecondsToTicks(2))
        {
            for (int k = 0; k < 20; k++)
            {
                ParticleManager.NewParticle<StarParticle>(NPC.Center, Main.rand.NextVector2Circular(1, 1) * 30, default, 1);
            }

            CameraSystem.ScreenShake(30, 0.9f, NPC.Center);
            SoundEngine.PlaySound(new SoundStyle("Stratum/Assets/Sounds/Custom/Witch/Shield"), NPC.Center);

            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<WitchShield>(), 0, 0, 0, 0, 0);
        }

        NPC.velocity *= 0.95f;
        NPC.ai[0]++;

        Main.NewText("I'm in my shield phase!");

        if (NPC.ai[0] >= MathUtils .SecondsToTicks(4))
        {
            state = AIState.Portal;
            NPC.ai[0] = 0;
        }
    }

    void Portal()
    {
        NPC.dontTakeDamage = true;

        if (NPC.ai[0] == 0)
        {
            ParticleManager.NewParticle<Flash>(NPC.Center, Vector2.Zero, default, 0);
            ParticleManager.NewParticle<StarParticle>(NPC.Center, Vector2.Zero, default, 4);

            NPC.hide = true;

            CameraSystem.ScreenShake(30, 0.9f, NPC.Center);
            SoundEngine.PlaySound(new SoundStyle("Stratum/Assets/Sounds/Custom/Witch/Disappear"), NPC.Center);
        }

        if (NPC.ai[0] == 60)
        {
            NPC.Center = player.Center + (player.velocity * 20);
        }

        if (NPC.ai[0] >= 60)
        {
            if (NPC.ai[0] < 70)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(1f, 1f) * 100f;
                Vector2 spawnPos = NPC.Center + offset;
                Vector2 velocity = offset.SafeNormalize(Vector2.Zero) * 0.1f; // Start slow

                ParticleManager.NewParticle<Slash>(spawnPos, velocity, default, 1f);
                SoundEngine.PlaySound(SoundID.Item21 with { MaxInstances = -1 }, NPC.Center);
            }
        }

        if (NPC.ai[0] == 70)
        {
            if (NPC.Distance(player.Center) <= 100)
            {
                player.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), 100, 1);
            }
            NPC.hide = false;

            ParticleManager.NewParticle<Flash>(NPC.Center, Vector2.Zero, default, 0);
            ParticleManager.NewParticle<StarParticle>(NPC.Center, Vector2.Zero, default, 4);

            CameraSystem.ScreenShake(30, 0.9f, NPC.Center);
            SoundEngine.PlaySound(new SoundStyle("Stratum/Assets/Sounds/Custom/Witch/Reappear"), NPC.Center);
        }

        if (NPC.ai[0] >= MathUtils .SecondsToTicks(4))
        {
            state = AIState.Starshower;
            NPC.dontTakeDamage = false;
            NPC.ai[0] = 0;
        }

        NPC.ai[0]++;
    }

    void Starshower()
    {
        NPC.ai[0]++;

        Main.NewText("I'm in my starshower phase!");

        if (player != null)
        {
            Vector2 toPlayer = player.Center - NPC.Center;

            // Smooth follow direction
            float followSpeed = player.velocity.Length() * 0.9f; // Scales with player speed
            float inertia = 20f;

            Vector2 desiredVelocity = toPlayer.SafeNormalize(Vector2.Zero) * followSpeed;

            NPC.velocity = (NPC.velocity * (inertia - 1) + desiredVelocity) / inertia;
        }

        NPC.ai[1]++;

        if (NPC.ai[1] >= 20 - NPC.ai[2])
        {
            Projectile.NewProjectileDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(Main.rand.NextFloat(-Main.screenWidth, Main.screenWidth) * 0.5f, -Main.screenHeight), Vector2.UnitY * (8 + NPC.ai[2]), ModContent.ProjectileType<WitchStarfall>(), 100, 0, NPC.whoAmI);
            NPC.ai[1] = 0;
            NPC.ai[2] += 0.35f;
        }

        if (NPC.ai[0] >= MathUtils .SecondsToTicks(10))
        {
            state = AIState.Shattered;
            NPC.ai[0] = 0;
            NPC.ai[2] = 0;
        }
    }

    void Shattered()
    {
        NPC.velocity *= 0.95f;

        if (NPC.ai[0] == 0)
        {
            Projectile.NewProjectileDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(Main.rand.NextFloat(-Main.screenWidth, Main.screenWidth) * 0.5f, -Main.screenHeight), Vector2.UnitY * (8 + NPC.ai[2]), ModContent.ProjectileType<WitchMeteor>(), 100, 0, NPC.whoAmI);
        }

        if (NPC.ai[0] >= MathUtils .SecondsToTicks(5))
        {
            state = AIState.Shield;
            NPC.ai[0] = 0;
        }

        NPC.ai[0]++;
    }

    void Debris()
    {
        if (player != null)
        {
            Vector2 toPlayer = player.Center - NPC.Center;

            // Smooth follow direction
            float followSpeed = player.velocity.Length() * 0.9f; // Scales with player speed
            float inertia = 20f;

            Vector2 desiredVelocity = toPlayer.SafeNormalize(Vector2.Zero) * followSpeed;

            // Bobbing effect
            float bobSpeed = 0.15f; // Speed of bobbing
            float bobHeight = 1.5f; // Height of bobbing
            float bobOffset = (float)Math.Sin(Main.GameUpdateCount * bobSpeed) * bobHeight;

            desiredVelocity.Y += bobOffset; // Apply bobbing vertically

            NPC.velocity = (NPC.velocity * (inertia - 1) + desiredVelocity) / inertia;
        }
    }

    void Solaris()
    {
    }

    void Astrea()
    {
    }

    void PhaseCheckpoint()
    {
    }

    void HarmonyCheckpoint()
    {
    }

    void Death()
    {

    }

    #endregion

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPosition, Color lightColor)
    {
        // We don't need to define this, but I do it anyways.
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        spriteBatch.Draw(
            texture,
            NPC.Center - screenPosition,
            NPC.frame, NPC.GetAlpha(lightColor),
            NPC.rotation,
            NPC.frame.Size() * 0.5f, NPC.scale,
            NPC.spriteDirection.ToHorizontalFlip(),
            0f);

        DrawGlow(spriteBatch, screenPosition, Color.White);

        return false;
    }

    void DrawGlow(SpriteBatch spriteBatch, Vector2 screenPosition, Color glowColor)
    {
        // We don't need to define this, but I do it anyways.
        Texture2D texture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

        spriteBatch.Draw(
            texture,
            NPC.Center - screenPosition,
            NPC.frame, NPC.GetAlpha(glowColor),
            NPC.rotation, NPC.frame.Size() * 0.5f,
            NPC.scale,
            NPC.spriteDirection.ToHorizontalFlip(),
            0f);
    }
}

public class CelestialSkyPlayer : ModPlayer
{
    public static CelestialSkyPlayer Instance;

    public override void OnEnterWorld()
    {
        Instance = Player.GetModPlayer<CelestialSkyPlayer>();
    }

    public override void PostUpdateMiscEffects()
    {
        Player.ManageSpecialBiomeVisuals("Stratum:Conglomerate", true);
    }
}