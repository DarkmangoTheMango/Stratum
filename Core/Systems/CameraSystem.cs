using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.UI;

namespace Stratum.Core.Systems;

public class CameraSystem : ModSystem
{
    static float ShakeIntensity = 0f;
    static float ShakeFade = 0.9f;
    static Vector2? ShakeSource = null;

    public override void ModifyScreenPosition()
    {
        if (ShakeIntensity > 0f && !Main.gamePaused)
        {
            float effectiveIntensity = ShakeIntensity;

            if (ShakeSource.HasValue && Main.LocalPlayer != null && Main.LocalPlayer.active)
            {
                float distance = Vector2.Distance(Main.LocalPlayer.Center, ShakeSource.Value);
                float maxDistance = 1000f;
                float falloff = 1f - MathHelper.Clamp(distance / maxDistance, 0f, 1f);

                effectiveIntensity *= falloff;
            }

            if (effectiveIntensity > 0f)
                Main.screenPosition += Main.rand.NextVector2Circular(effectiveIntensity, effectiveIntensity);

            ShakeIntensity *= ShakeFade;

            if (ShakeIntensity < 0.05f)
            {
                ShakeIntensity = 0f;
                ShakeSource = null;
            }
        }
    }

    /// <summary>
    /// Triggers a simple screen shake effect.
    /// </summary>
    /// <param name="intensity">Base intensity of the shake</param>
    /// <param name="fade">The amount of shake changed per frame</param>
    /// <param name="source">The world position that the shake comes from. Leave blank for global shake.</param>
    public static void ScreenShake(float intensity = 8f, float fade = 0.9f, Vector2? source = null)
    {
        ShakeIntensity = Math.Max(ShakeIntensity, intensity);
        ShakeFade = fade;
        ShakeSource = source;
    }
    
    static float FlashIntensity = 0f;
    static float FlashFade = 0f;

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Map / Minimap"));
        if (index != -1)
        {
            layers.Insert(index, new LegacyGameInterfaceLayer(
                "Stratum: Screen Flash",
                delegate
                {
                    if (FlashIntensity > 0f)
                    {
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * FlashIntensity);

                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
                    }
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }

    public override void PostUpdateEverything()
    {
        if (FlashIntensity > 0f)
        {
            FlashIntensity -= FlashFade;
            if (FlashIntensity < 0f)
                FlashIntensity = 0f;
        }
    }

    public static void ScreenFlash(float intensity = 1f, float fade = 0.01f)
    {
        FlashIntensity = Utils.Clamp(intensity, 0f, 1f);
        FlashFade = fade;
    }
}