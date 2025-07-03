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
            if (bossHeadIndex != -1)
                return TextureAssets.NpcHeadBoss[bossHeadIndex];

            return null;
        }

        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC npc = Main.npc[info.npcIndexToAimAt];

            bossHeadIndex = npc.GetBossHeadTextureIndex();

            if (!npc.active)
                return false;

            life = npc.life;
            lifeMax = npc.lifeMax;

            return true;
        }
    }
}
