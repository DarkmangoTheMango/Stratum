﻿using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;

namespace Stratum.Core.Systems
{
    public class CameraSystem : ModSystem
    {
        public static float ShakeIntensity = 0f;
        public static float ShakeFade = 0.9f;
        public static Vector2? ShakeSource = null; // Nullable, used only when relevant

        public override void ModifyScreenPosition()
        {
            if (ShakeIntensity > 0f && !Main.gamePaused)
            {
                float effectiveIntensity = ShakeIntensity;

                if (ShakeSource.HasValue && Main.LocalPlayer != null && Main.LocalPlayer.active)
                {
                    float distance = Vector2.Distance(Main.LocalPlayer.Center, ShakeSource.Value);
                    float maxDistance = 1000f; // beyond this, no shake
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
        /// Call this to trigger a screen shake.
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

        public static float FlashIntensity = 0f;
        public static float FlashFade = 0f;

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (FlashIntensity > 0f)
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * FlashIntensity);
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
}