using Microsoft.Xna.Framework.Graphics;
using Stratum.Common.UI.BossBars;
using Stratum.Common.Utilities;
using Stratum.Core.Systems;
using System.Data;
using Terraria.DataStructures;

namespace Stratum.Content.NPCs.Bosses.Witch
{
    [AutoloadBossHead]
    public class Witch : ModNPC
    {
        public int shield;
        public int shieldMax;

        /// <summary>
        /// Represents all possible AI states of the Witch.
        /// </summary>
        public enum AIState
        {
            // Spawn
            Spawn,

            // Phase 1
            Shield,
            Portal,
            Starshower,
            Shattered,
            Debris,

            // Phase 2
            Solaris,

            // Phase 3
            Astrea,

            // Anti-cheat logic
            PhaseCheckpoint,
            HarmonyCheckpoint,

            // Death
            Death
        }

        /// <summary>
        /// The current active behavior/attack pattern.
        /// </summary>
        public AIState State = AIState.Spawn;

        /// <summary>
        /// The internal timer for the boss, used to control the timing of attacks and behaviors.
        /// </summary>
        public int bossTimer
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        /// <summary>
        /// Sets the boss' internal timer to the specified value. returns 0 by default.
        /// </summary>
        /// <param name="value">The value to set the timer to.</param>
        void SetBossTimer(int value = 0) => bossTimer = value;

        /// <summary>
        /// Checks if the boss' internal timer is equal to the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        bool CheckBossTimer(int value = 0) => bossTimer == value;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetDefaults()
        {
            // Size and stats
            NPC.Size = new(32);
            NPC.damage = 0;
            NPC.knockBackResist = 0;
            NPC.defense = 777;
            NPC.lifeMax = 1000000;
            shieldMax = 100;

            // Behavior flags
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1;
            NPC.npcSlots = 6;

            // Boss properties
            NPC.boss = true;
            NPC.BossBar = ModContent.GetInstance<WitchBossBar>();
            NPC.value = Item.buyPrice(0, 5, 0, 0);

            // Sounds
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            // Music
            if (!Main.dedServ)
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Witch");
        }

        public override void OnSpawn(IEntitySource source)
        {
            shield = shieldMax;

            Main.musicFade[MusicLoader.GetMusicSlot(Mod, "Assets/Music/Witch")] = 1;
            CameraSystem.ScreenFlash(1, 0.01f);
        }

        public override void AI()
        {
            //Boss Behavior
            Behavior();

            //AI State switching
            switch (State)
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
            }
        }

        /// <summary>
        /// ADD COMMENT
        /// </summary>
        void Behavior()
        {
            Lighting.AddLight(NPC.Center, new Vector3(0, 1, 1) * 1f);
            UpdateShield();
        }

        void UpdateShield()
        {
            if (shield > 0)
            {
                NPC.immortal = true;
                NPC.dontTakeDamage = true;
            }
            else
            {
                NPC.immortal = false;
                NPC.dontTakeDamage = false;
            }
        }

        /// <summary>
        /// Plays spawn animation and logic.
        /// </summary>
        void Spawn()
        {
            if (CheckBossTimer())
            {
                Main.NewText("I have spawned, bitch!");
            }

            if (++bossTimer >= Mathematics.SecondsToTicks(2))
            {
                State = AIState.Shield;
                SetBossTimer();
            }
        }

        /// <summary>
        /// Summons minions that protect the Witch.
        /// </summary>
        /// <param name="minionCount">ADD COMMENT</param>
        void Shield(int minionCount = 1)
        {
            if (CheckBossTimer())
            {
                Main.NewText("I have a shield, bitch!");
            }

            if (++bossTimer >= Mathematics.SecondsToTicks(2))
            {
                State = AIState.Portal;
                SetBossTimer();
            }
        }

        /// <summary>
        /// ADD COMMENT
        /// </summary>
        /// <param name="cycles">ADD COMMENT</param>
        /// <param name="predictionTime">ADD COMMENT</param>
        /// <param name="delay">ADD COMMENT</param>
        void Portal(int cycles = 1, float predictionTime = 120, float delay = 120)
        {
            if (CheckBossTimer())
            {
                Main.NewText("I'm teleporting, bitch!");
            }

            if (++bossTimer >= Mathematics.SecondsToTicks(2))
            {
                State = AIState.Starshower;
                SetBossTimer();
            }
        }

        /// <summary>
        /// ADD COMMENT
        /// </summary>
        void Starshower()
        {
            if (CheckBossTimer())
            {
                Main.NewText("I have a starshower, bitch!");
            }

            if (++bossTimer >= Mathematics.SecondsToTicks(2))
            {
                State = AIState.Shattered;
                SetBossTimer();
            }
        }

        /// <summary>
        /// ADD COMMENT
        /// </summary>
        void Shattered()
        {
            if (CheckBossTimer())
            {
                Main.NewText("I'm shattered, bitch!");
            }

            if (++bossTimer >= Mathematics.SecondsToTicks(2))
            {
                State = AIState.Debris;
                SetBossTimer();
            }
        }

        /// <summary>
        /// ADD COMMENT
        /// </summary>
        void Debris()
        {
            if (CheckBossTimer())
            {
                Main.NewText("I have a debris, bitch!");
            }

            if (++bossTimer >= Mathematics.SecondsToTicks(2))
            {
                State = AIState.Solaris;
                SetBossTimer();
            }
        }

        /// <summary>
        /// ADD COMMENT
        /// </summary>
        void Solaris()
        {
            if (CheckBossTimer())
            {
                Main.NewText("I have a solaris, bitch!");
            }

            if (++bossTimer >= Mathematics.SecondsToTicks(2))
            {
                State = AIState.Astrea;
                SetBossTimer();
            }
        }

        /// <summary>
        /// ADD COMMENT
        /// </summary>
        void Astrea()
        {
            if (CheckBossTimer())
            {
                Main.NewText("I have a astrea, bitch!");
            } 

            if (++bossTimer >= Mathematics.SecondsToTicks(2))
            {
                State = AIState.Shield;
                SetBossTimer();
            }
        }

        /// <summary>
        /// ADD COMMENT
        /// </summary>
        void PhaseCheckpoint()
        {

        }

        /// <summary>
        /// ADD COMMENT
        /// </summary>
        void HarmonyCheckpoint()
        {

        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

            Vector2 drawPosition = NPC.Center - screenPos;

            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(lightColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection.ToHorizontalFlip(), 0f);

            Color glowColor = new(255, 255, 255, 0);

            spriteBatch.Draw(glowTexture, drawPosition, NPC.frame, NPC.GetAlpha(glowColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection.ToHorizontalFlip(), 0f);

            return false;
        }
    }
}