using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Stratum.Content.NPCs.Bosses.Witch;

public class CelestialShield : ModNPC
{
    ref float Parent => ref NPC.ai[0];

    public override string Texture => AssetUtils.AssetPath + "/Textures/Noise/TurbulentNoise";

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 1;

        NPCID.Sets.MPAllowedEnemies[Type] = true;
        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.Size = new Vector2(512);
        NPC.scale = 0.5f;

        NPC.lifeMax = 100_000;
        NPC.takenDamageMultiplier = 0.75f;
        NPC.knockBackResist = 0;

        NPC.HitSound = new SoundStyle($"{AssetUtils.SoundPath}/NPCHit/CelestialWitch_Hit_", 3) { PitchVariance = 0.2f, MaxInstances = 5 };

        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment);
    }

    #region Behavior

    public override void OnSpawn(IEntitySource source)
    {

    }

    public override void AI()
    {
        NPC.hide = Main.npc[(int)Parent].hide;
        Lighting.AddLight(NPC.Center, new Color(0, 255, 255).ToVector3() * 0.5f);
        NPC.Center = Main.npc[(int)Parent].Center;
    }

    #endregion

    #region Drawing

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Effect shader = ShaderManager.Shield.Value;

        DrawHelper.WithShader(shader, () =>
        {
            Main.EntitySpriteDraw(texture, NPC.Center - screenPos, texture.Bounds, Color.White, NPC.rotation, texture.Size() * 0.5f, NPC.scale, SpriteEffects.None, 0);
            shader.Parameters["uTime"].SetValue(Main.GameUpdateCount / 60f);
        });

        return false;
    }

    #endregion

    #region Networking

    public override void SendExtraAI(BinaryWriter writer)
    {

    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {

    }

    #endregion
}