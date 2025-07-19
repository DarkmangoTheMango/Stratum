using ReLogic.Content;

namespace Stratum.Core.Systems;

// I should probably make this more intuitive, but eh, whatever...
public class ShaderManager : ModSystem
{
    const string EffectPath = "Stratum/Assets/Effects";

    public override void Load()
    {
        if (!Main.dedServ)
            LoadEffects();
    }

    internal static Asset<Effect>
        BurningStarShader,
        Supernova,
        CelestialSky,
        DeadlyLaser
        ;

    static void LoadEffects()
    {
        BurningStarShader = ModContent.Request<Effect>($"{EffectPath}/BurningStarShader");
        Supernova = ModContent.Request<Effect>($"{EffectPath}/Supernova");
        CelestialSky = ModContent.Request<Effect>($"{EffectPath}/CelestialSky");
        DeadlyLaser = ModContent.Request<Effect>($"{EffectPath}/DeadlyLaser");
    }
}