using Stratum.Common.Utilities;
using Terraria.DataStructures;

namespace Stratum.Content.NPCs.Bosses.Witch;

public class WitchShield : ModNPC
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
        NPC.Size = new(196);
        NPC.damage = 0;
        NPC.knockBackResist = 0;
        NPC.lifeMax = 200000;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.NPCHit54;
        NPC.DeathSound = SoundID.NPCDeath52;
    }

    private float scaleX = 0.8f;
    private float scaleY = 1.2f;
    private float scaleTimer = 0f;
    private float wobbleAmplitude = 0.3f; // Starts strong, shrinks over time
    private const float wobbleDecayRate = 0.98f; // Lower = faster decay

    public override void OnSpawn(IEntitySource source)
    {

    }

    public override void AI()
    {
        Lighting.AddLight(NPC.Center, Color.White.ToVector3() * 1f);

        // Decaying wobble effect
        scaleTimer += 0.1f;

        wobbleAmplitude *= wobbleDecayRate; // Slowly reduces the wobble over time

        scaleX = 1f + (float)Math.Sin(scaleTimer * 1.5f) * wobbleAmplitude;
        scaleY = 1f + (float)Math.Sin(scaleTimer * 2.0f + 1f) * wobbleAmplitude;

        for (int k = 0; k < Main.maxNPCs; k++)
        {
            if (Main.npc[k].ModNPC is Witch)
            {
                NPC.Center = Main.npc[k].Center;
                Main.npc[k].dontTakeDamage = true;
                NPC.hide = Main.npc[k].hide;
            }
        }
    }

    public override void OnKill()
    {
        for (int k = 0; k < Main.maxNPCs; k++)
        {
            if (Main.npc[k].ModNPC is Witch)
            {
                Main.npc[k].dontTakeDamage = false;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        Vector2 drawPosition = NPC.Center - screenPos;

        spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(lightColor), NPC.rotation, NPC.frame.Size() * 0.5f, new Vector2(scaleX, scaleY), NPC.spriteDirection.ToHorizontalFlip(), 0f);

        return false;
    }
}