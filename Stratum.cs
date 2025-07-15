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
using ReLogic.Content;
using Stratum.Content.NPCs.Bosses.Witch;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Stratum;

public class Stratum : Mod
{
    public static string AssetPath => "Stratum/Assets";

    public static string SoundPath => "Stratum/Assets/Sounds";

    public static string EffectPath => "Stratum/Assets/Effects";

    public override void Load()
    {
        LoadEffects();
    }

    public static Asset<Effect> BurningStarShader, Supernova, CelestialSky, DeadlyLaser;

    // TODO: Move this to a ModSystem for better organization
    static void LoadEffects()
    {
        if (!Main.dedServ)
        {
            BurningStarShader = ModContent.Request<Effect>(EffectPath + "/BurningStarShader");
            Supernova = ModContent.Request<Effect>(EffectPath + "/Supernova");
            CelestialSky = ModContent.Request<Effect>(EffectPath + "/CelestialSky");
            DeadlyLaser = ModContent.Request<Effect>(EffectPath + "/DeadlyLaser");
        }
    }
}

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

public class TestScene : ModSceneEffect
{
    public override void SpecialVisuals(Player player, bool isActive) => player.ManageSpecialBiomeVisuals("Stratum:CelestialSky", isActive);

    public override void Load()
    {
        Filters.Scene["Stratum:CelestialSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(Color.Transparent).UseOpacity(0f), EffectPriority.VeryHigh);
        SkyManager.Instance["Stratum:CelestialSky"] = new CelestialSky();
    }

    public override bool IsSceneEffectActive(Player player)
    {
        foreach (NPC npc in Main.ActiveNPCs)
            if (npc.ModNPC is CelestialWitch)
                return true;

        return false;
    }
}

public class CelestialSky : CustomSky
{
    bool isActive = false;
    Star[] stars;

    int StarCount = 400;
    float MinDepth = 0.1f;
    float MaxDepth = 0.8f;
    float StarSpeed = 3f;
    float TwinkleStrength = 0.25f;
    float TwinkleBase = 0.75f;
    int OffscreenBuffer = 100;
    float intensity;

    public struct Star
    {
        public Texture2D texture;
        public Vector2 pos;
        public float depth;
    }

    public override void Activate(Vector2 position, params object[] args)
    {
        isActive = true;
        stars = new Star[StarCount];

        for (int i = 0; i < StarCount; i++)
        {
            stars[i] = GenerateStar();
            stars[i].pos.X = Main.rand.NextFloat(Main.screenWidth);
        }
    }

    public override void Deactivate(params object[] args) => isActive = false;

    public override bool IsActive() => isActive || intensity > 0;

    public override void Reset() => isActive = false;

    public override void Update(GameTime gameTime)
    {
        if (Main.gamePaused || stars == null)
            return;

        for (int i = 0; i < StarCount; i++)
        {
            stars[i].pos.X += stars[i].depth * StarSpeed;

            if (stars[i].pos.X > Main.screenWidth + OffscreenBuffer)
            {
                stars[i] = GenerateStar();
                stars[i].pos.X = -OffscreenBuffer;
            }
        }

        if (isActive && intensity < 1f)
            intensity += 0.01f;
        else if (!isActive && intensity > 0)
            intensity -= 0.01f;
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.BackgroundViewMatrix.ZoomMatrix);

        if (maxDepth < 10000 || minDepth >= 10000 || stars == null)
            return;

        DrawBackground(shader =>
        {
            shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount / 60f);
            shader.Parameters["alpha"]?.SetValue(intensity);
            shader.CurrentTechnique.Passes[0].Apply();
        });

        DrawStars(spriteBatch);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.BackgroundViewMatrix.ZoomMatrix);
    }

    void DrawBackground(Action<Effect> configureShader)
    {
        Effect shader = (Effect)Stratum.CelestialSky;
        configureShader?.Invoke(shader);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, shader, Main.BackgroundViewMatrix.ZoomMatrix);

        Texture2D Texture = ModContent.Request<Texture2D>(Stratum.AssetPath + "/Textures/Noise/TurbulentNoise").Value;

        Main.spriteBatch.Draw(Texture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
    }

    void DrawStars(SpriteBatch spriteBatch)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.BackgroundViewMatrix.ZoomMatrix);

        for (int i = 0; i < StarCount; i++)
        {
            Star star = stars[i];

            float twinkle = TwinkleBase + TwinkleStrength * (float)Math.Sin(Main.GameUpdateCount * 0.05f + i);
            float scale = star.depth;
            Color starColor = Color.White * (2f * star.depth) * twinkle;

            spriteBatch.Draw(star.texture, star.pos, null, starColor * intensity, 0f, star.texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
    }

    Star GenerateStar()
    {
        int variant = Main.rand.Next(5);
        Texture2D texture = ModContent.Request<Texture2D>($"Terraria/Images/Star_{variant}").Value;

        return new Star
        {
            texture = texture,
            depth = Main.rand.NextFloat(MinDepth, MaxDepth),
            pos = new Vector2(0f, Main.rand.NextFloat(Main.screenHeight))
        };
    }

    public override float GetCloudAlpha() => MathHelper.Lerp(1, 0, intensity);

    public override Color OnTileColor(Color inColor) => inColor;
}