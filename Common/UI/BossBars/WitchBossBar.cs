using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Stratum.Content.NPCs.Bosses.Witch;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;

namespace Stratum.Common.UI.BossBars
{
    public class WitchBossBar : ModBossBar
    {
        private int bossHeadIndex = -1;

        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            // Display the previously assigned head index
            if (bossHeadIndex != -1)
                return TextureAssets.NpcHeadBoss[bossHeadIndex];

            return null;
        }

        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            // Here the game wants to know if to draw the boss bar or not. Return false whenever the conditions don't apply.
            // If there is no possibility of returning false (or null) the bar will get drawn at times when it shouldn't, so write defensive code!

            NPC npc = Main.npc[info.npcIndexToAimAt];

            bossHeadIndex = npc.GetBossHeadTextureIndex();

            if (!npc.active)
                return false;

            life = npc.life;
            lifeMax = npc.lifeMax;

            if (npc.ModNPC is Witch witch)
            {
                shield = witch.shield;
                shieldMax = witch.shieldMax;
            }

            return true;
        }
    }
}
