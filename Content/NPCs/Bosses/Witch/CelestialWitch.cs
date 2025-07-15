using CalamityMod;
using Stratum.Content.Particles;
using System;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Stratum.Content.NPCs.Bosses.Witch;

[AutoloadBossHead]
public class CelestialWitch : ModNPC
{
    Player target => Main.player[NPC.target];

    enum AIState
    {
        Spawning,
        SolarHellstorm,
        CometAzure,
        Death
    }

    AIState aiState = AIState.Spawning;

    public override void SetStaticDefaults()
    {
        NPCID.Sets.MPAllowedEnemies[Type] = true;

        NPCID.Sets.BossBestiaryPriority.Add(Type);

        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.Size = new Vector2(34, 48);
        NPC.lifeMax = 1_000_000;
        NPC.damage = 777;
        NPC.defense = 777;
        NPC.knockBackResist = 0;
        NPC.takenDamageMultiplier = 0.75f;
        NPC.noGravity = true;
        NPC.aiStyle = -1;
        NPC.boss = true;
        NPC.HitSound = new SoundStyle(Stratum.SoundPath + "/NPCHit/CelestialWitch_Hit_") with { PitchVariance = 0.2f, MaxInstances = 5, Variants = [0, 1, 2] };

        if (!Main.dedServ)
            Music = MusicID.EmpressOfLight;

        if (ModLoader.HasMod("CalamityMod"))
        {
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToCold = false;
            NPC.Calamity().VulnerableToSickness = true;
        }
    }

    public override void OnSpawn(IEntitySource source)
    {
        SoundEngine.PlaySound(new SoundStyle(Stratum.SoundPath + "/Custom/Witch/WitchLaugh"));
        CameraSystem.ScreenShake(30, 0.9f, NPC.Center);

        for (int i = 0; i < 20; i++)
            ParticleManager.NewParticle<Sparkle>(NPC.Center, MathUtils.RandomVector2CircularEdge(30) * Main.rand.NextFloat(0.1f, 1f), default, 0);
    }

    public override void AI()
    {
        Lighting.AddLight(NPC.Center, new Color(0, 255, 255).ToVector3() * 0.5f);
        NPC.TargetClosest(true);
        NPC.spriteDirection = NPC.direction;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

        Color glowColor = new Color(255, 255, 255, 0);

        Main.EntitySpriteDraw(texture, NPC.Center - screenPos, texture.Bounds, drawColor, NPC.rotation, texture.Size() / 2, NPC.scale, NPC.spriteDirection.ToSpriteEffect(SpriteEffects.FlipHorizontally), 0);
        Main.EntitySpriteDraw(glowTexture, NPC.Center - screenPos, texture.Bounds, NPC.GetAlpha(glowColor), NPC.rotation, texture.Size() / 2, NPC.scale, NPC.spriteDirection.ToSpriteEffect(SpriteEffects.FlipHorizontally), 0);

        DrawFakeLighting(false, 1, new Color(255, 128, 0, 0));

        return false;
    }

    void DrawFakeLighting(bool active, float intensity, Color color)
    {
        if (!active)
            return;

        Texture2D texture = ModContent.Request<Texture2D>(Texture + "_Glow2").Value;

        Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, texture.Bounds, color, NPC.rotation, texture.Size() / 2, NPC.scale, NPC.spriteDirection.ToSpriteEffect(SpriteEffects.FlipHorizontally), 0);
    }
}