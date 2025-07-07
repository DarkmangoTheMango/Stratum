global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Stratum.Core;
global using Stratum.Core.Systems;
global using System;
global using Terraria;
global using Terraria.ID;
global using Terraria.ModLoader;

namespace Stratum;

public class Stratum : Mod
{
	public static string AssetPath => "Stratum/Assets";
    public static string SoundPath => "Stratum/Assets/Sounds";

    public override void Load()
    {
        LoadEffects();
    }

    void LoadEffects()
    {
        if (!Main.dedServ)
        {

        }
    }
}