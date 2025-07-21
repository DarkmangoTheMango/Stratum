using CalamityMod;
using CalamityMod.Items.Potions;
using CalamityMod.Particles;
using Microsoft.Build.Graph;
using Stratum.Content.Particles;
using Stratum.Content.Projectiles.Witch;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;

namespace Stratum.Content.NPCs.Bosses.Witch;

[AutoloadBossHead]
public class CelestialWitch : ModNPC
{
    ref Player Target => ref Main.player[NPC.target];

    enum AIState
    {
        Spawning,
        Shield,
        JudgementCut,
        Starfall,
        SolarHellstorm,
        CometAzure,
        Death
    }

    AIState state = AIState.Spawning;

    int _phase;

    public int Phase
    {
        get => _phase;
        set => _phase = Math.Clamp(value, 0, 3);
    }

    ref float PhaseTimer => ref NPC.ai[0];
    ref float AttackTimer => ref NPC.ai[1];
    ref float ExtraTimer => ref NPC.ai[2];

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {

    }

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 1;

        NPCID.Sets.MPAllowedEnemies[Type] = true;
        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;

        NPCID.Sets.BossBestiaryPriority.Add(Type);
    }

    public override void SetDefaults()
    {
        NPC.Size = new Vector2(34, 48);

        NPC.lifeMax = Main.expertMode ? Main.masterMode ? 3_000_000 : 2_000_000 : 1_000_000;
        NPC.defense = 100;
        NPC.takenDamageMultiplier = 0.75f;
        NPC.knockBackResist = 0;

        NPC.HitSound = new SoundStyle($"{AssetUtils.SoundPath}/NPCHit/CelestialWitch_Hit_", 3) { PitchVariance = 0.2f, MaxInstances = 5 };

        NPC.npcSlots = 50f;
        NPC.boss = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;

        //NPC.BossBar = ModContent.GetInstance<>();

        if (!Main.dedServ)
            Music = MusicID.EmpressOfLight;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment);
        NPC.damage = (int)(NPC.damage * NPC.GetExpertDamageMultiplier());
    }

    #region Behavior

    public override void OnSpawn(IEntitySource source)
    {
        SoundEngine.PlaySound(new SoundStyle($"{AssetUtils.SoundPath}/Custom/Witch/WitchLaugh"));
        CameraSystem.ScreenShake(30, 0.9f, NPC.Center);

        for (int i = 0; i < 20; i++)
            ParticleManager.NewParticle<Sparkle>(NPC.Center, MathUtils.RandomVector2CircularEdge(30) * Main.rand.NextFloat(0.1f, 1f), default, 0);
    }

    public override void AI()
    {
        Lighting.AddLight(NPC.Center, new Color(0, 255, 255).ToVector3() * 0.5f);
        NPC.TargetClosest(true);
        NPC.spriteDirection = NPC.direction;

        Main.NewText(state);

        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is CelestialShield)
                NPC.immortal = true;
            else
                NPC.immortal = false;
        }

        switch (state)
        {
            case AIState.Spawning:
                Spawning(120);
                break;
            case AIState.Shield:
                Shield(120);
                break;
            case AIState.JudgementCut:
                JudgementCut(400);
                break;
            case AIState.Starfall:
                Starfall(500);
                break;
            case AIState.SolarHellstorm:
                SolarHellstorm(120);
                break;
            case AIState.CometAzure:
                CometAzure(120);
                break;
            case AIState.Death:
                Death(120);
                break;
            default:
                break;
        }
    }

    void Spawning(int duration)
    {
        if (++PhaseTimer >= duration)
            SetState(AIState.Shield);
    }

    void Shield(int duration)
    {
        NPC.velocity *= 0.98f;

        if (PhaseTimer == 60)
        {
            bool shieldExists = false;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.type == ModContent.NPCType<CelestialShield>() && npc.ai[0] == NPC.whoAmI)
                {
                    shieldExists = true;
                    break;
                }
            }

            if (!shieldExists)
                NPC.NewNPCDirect(Entity.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CelestialShield>(), 0, NPC.whoAmI);
        }


        if (++PhaseTimer >= duration)
            SetState(AIState.JudgementCut);
    }

    void JudgementCut(int duration)
    {
        NPC.velocity *= 0.98f;

        if (AttackTimer == 60)
        {
            CameraSystem.ScreenShake(20, 0.9f, NPC.Center);
            SoundEngine.PlaySound(new SoundStyle($"{AssetUtils.SoundPath}/Custom/Witch/JudgementCut_Appear"), NPC.Center);
            Projectile.NewProjectile(Entity.GetSource_FromAI(), NPC.Center, default, ModContent.ProjectileType<JudgementCut>(), 200, 10, Main.myPlayer, NPC.whoAmI);
            NPC.velocity = NPC.DirectionTo(Target.Center) * 20;
            NPC.hide = false;
        }

        if (AttackTimer == 70)
        {
            AttackTimer = 0;
        }

        if (AttackTimer == 0)
        {
            SoundEngine.PlaySound(new SoundStyle($"{AssetUtils.SoundPath}/Custom/Witch/JudgementCut_Vanish"), NPC.Center);
            NPC.hide = true;
            NPC.dontTakeDamage = true;
        }

        if (AttackTimer == 30)
        {
            NPC.Center = Target.Center + (Target.velocity.SafeNormalize(Vector2.One.RotatedByRandom(MathHelper.PiOver2)) * 70);
            SoundEngine.PlaySound(new SoundStyle($"{AssetUtils.SoundPath}/Custom/Flash"), NPC.Center);

            for (int i = 0; i < 20; i++)
                ParticleManager.NewParticle<Sparkle>(NPC.Center, MathUtils.RandomVector2CircularEdge(30) * Main.rand.NextFloat(0.1f, 1f), default, 0);
        }

        AttackTimer++;

        if (++PhaseTimer >= duration)
        {
            NPC.hide = false;
            NPC.dontTakeDamage = false;
            SetState(AIState.Starfall);
        }
    }

    void Starfall(int duration)
    {
        float desiredSpeed = 6f;
        float inertia = 20f;

        Vector2 direction = Target.Center - NPC.Center;

        if (direction != Vector2.Zero)
            direction.Normalize();

        Vector2 desiredVelocity = direction * (desiredSpeed * NPC.Distance(Target.Center) * 0.005f);

        NPC.velocity = (NPC.velocity * (inertia - 1) + desiredVelocity) / inertia;

        if (++AttackTimer >= 40)
        {
            AttackTimer = 0;

            Vector2 Velocity = NPC.DirectionTo(Target.Center + Target.velocity * 10).RotatedByRandom(0.1f) * 12;

            Projectile.NewProjectile(Entity.GetSource_FromAI(), NPC.Center, Velocity, ModContent.ProjectileType<Comet>(), 100, 2, Main.myPlayer);
            SoundEngine.PlaySound(new SoundStyle($"{AssetUtils.SoundPath}/Custom/Witch/CometFire_", 2) { PitchVariance = 0.1f }, NPC.Center);
            NPC.velocity -= Velocity;
        }

        if (++ExtraTimer >= 10)
        {
            ExtraTimer = 0;

            float spawnX = Target.Center.X + (Main.rand.NextFloat(-Main.screenWidth, Main.screenWidth) / 2);
            float spawnY = Target.Center.Y - (Main.screenHeight / 2) - 600;

            Vector2 spawnPosition = new(spawnX, spawnY);

            Vector2 velocity = Vector2.UnitY * 10f;

            Projectile.NewProjectile(
                Entity.GetSource_FromAI(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<Comet>(),
                80,
                2f,
                Main.myPlayer
            );
        }


        if (++PhaseTimer >= duration)
            SetState(AIState.Shield);
    }

    void SolarHellstorm(int duration)
    {

    }

    void CometAzure(int duration)
    {

    }

    void Death(int duration)
    {

    }

    void SetState(AIState newState)
    {
        state = newState;
        PhaseTimer = 0;
        AttackTimer = 0;
        ExtraTimer = 0;

        NPC.netUpdate = true;
    }

    public override void OnKill()
    {
        //NPC.SetEventFlagCleared(ref DownedBossSystem.downedMinionBoss, -1);
    }

    #endregion

    #region Loot

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Placeable.Furniture.MinionBossTrophy>(), 10));

        LeadingConditionRule notExpertRule = new(new Conditions.NotExpert());

        //notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<MinionBossMask>(), 7));

        npcLoot.Add(notExpertRule);

        //npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<MinionBossBag>()));

        //npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Furniture.MinionBossRelic>()));

        //npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<MinionBossPetItem>(), 4));
    }

    public override void BossLoot(ref int potionType)
    {
        if (ModLoader.HasMod("CalamityMod"))
            potionType = ModContent.ItemType<OmegaHealingPotion>();
        else
            potionType = ItemID.SuperHealingPotion;
    }

    #endregion

    #region Drawing

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

        Color glowColor = new(255, 255, 255, 0);

        Main.EntitySpriteDraw(texture, NPC.Center - screenPos, texture.Bounds, NPC.GetAlpha(drawColor), NPC.rotation, texture.Size() / 2, NPC.scale, NPC.spriteDirection.ToSpriteEffect(), 0);
        Main.EntitySpriteDraw(glowTexture, NPC.Center - screenPos, texture.Bounds, NPC.GetAlpha(glowColor), NPC.rotation, texture.Size() / 2, NPC.scale, NPC.spriteDirection.ToSpriteEffect(), 0);

        return false;
    }

    #endregion

    #region Networking

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((int)state);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        state = (AIState)reader.ReadInt32();
    }

    #endregion
}