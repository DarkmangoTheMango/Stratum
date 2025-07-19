global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Stratum.Common.Utilities;
global using Stratum.Core;
global using Stratum.Core.Systems;
global using System;
global using Terraria;
global using Terraria.ID;
global using Terraria.ModLoader;
using CalamityMod;
using Stratum.Content.NPCs.Bosses.Witch;

namespace Stratum;

// This file kinda just sits here and looks pretty...
public class Stratum : Mod
{

}

// This is just a test to see if calamity compatibility works properly.
// PLEASE REMOVE THIS ONCE CALAMITY COMPATIBILITY IS PROPERLY IMPLEMENTED!
public class TestCompat : ModSystem
{
    public override void PostUpdateNPCs()
    {
        foreach (NPC npc in Main.ActiveNPCs)
            if (npc.ModNPC is CelestialWitch)
                GrantInfiniteFlight();
    }

    public static void GrantInfiniteFlight()
    {
        foreach (Player player in Main.ActivePlayers)
            if (ModLoader.HasMod("CalamityMod"))
            {
                player.Calamity().infiniteFlight = true;
            }
    }
}