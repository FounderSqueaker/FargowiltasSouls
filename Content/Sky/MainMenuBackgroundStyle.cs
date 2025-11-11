using FargowiltasSouls.Assets.Textures;
using Humanizer;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using static FargowiltasSouls.Content.Sky.MutantSky;

namespace FargowiltasSouls.Content.Sky
{
    public class MainMenuBackgroundStyle : ModSurfaceBackgroundStyle
    {
        //theres some unnecessary clutter in here but oh well

        private float intensity = 1f;
        private float lifeIntensity = 1f;
        private float specialColorLerp = 0f;
        private Color? specialColor = null;
        private int delay = 0;
        private readonly int[] xPos = new int[50];
        private readonly int[] yPos = new int[50];
        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {
            for (int i = 0; i < fades.Length; i++)
            {
                if (i == Slot)
                {
                    fades[i] += transitionSpeed;
                    if (fades[i] > 1f)
                    {
                        fades[i] = 1f;
                    }
                }
                else
                {
                    fades[i] -= transitionSpeed;
                    if (fades[i] < 0f)
                    {
                        fades[i] = 0f;
                    }
                }
            }
        }

        public int fadeIn = 0;
        private Color ColorToUse(ref float opacity)
        {
            Color color = new(51, 255, 191);
            opacity = intensity * 0.5f + lifeIntensity * 0.5f;
            opacity *= Math.Min(fadeIn / 60f, 1);

            if (specialColorLerp > 0 && specialColor != null)
            {
                color = Color.Lerp(color, (Color)specialColor, specialColorLerp);
                if (specialColor == Color.Black)
                    opacity = Math.Min(1f, opacity + Math.Min(intensity, lifeIntensity) * 0.5f);
            }

            return color;
        }
        public List<LightRay> LightRays = [];
        public override bool PreDrawCloseBackground(SpriteBatch spriteBatch)
        {
            fadeIn++;
            float opacity = 0f;
            Color color = ColorToUse(ref opacity);

            Vector2 screenCenter = Main.screenPosition + Vector2.UnitX * Main.screenWidth / 2 + Vector2.UnitY * Main.screenHeight / 2;

            spriteBatch.Draw(ModContent.Request<Texture2D>($"FargowiltasSouls/Content/Sky/MutantSky{FargoSoulsUtil.TryAprilFoolsTexture}", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
                new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), color * opacity * 1.2f);

            var blackTile = TextureAssets.MagicPixel;
            var noise = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Sky/deepspace");
            var rayTexture = FargoAssets.LightRayTexture;
            //var noise2 = FargoAssets.SandyNoise;
            if (!blackTile.IsLoaded)
                return base.PreDrawCloseBackground(spriteBatch);
            if (!noise.IsLoaded)
                return base.PreDrawCloseBackground(spriteBatch);
            if (!rayTexture.IsLoaded)
                return base.PreDrawCloseBackground(spriteBatch);
            //if (!noise2.IsLoaded)
            //    return;

            ManagedShader blackShader = ShaderManager.GetShader("FargowiltasSouls.MutantNewBackgroundShader");
            blackShader.TrySetParameter("radius", Main.screenHeight * 1.6f);
            blackShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            blackShader.TrySetParameter("anchorPoint", screenCenter - Vector2.UnitY * Main.screenHeight * 2);
            blackShader.TrySetParameter("screenPosition", Main.screenPosition);
            blackShader.TrySetParameter("screenSize", Main.ScreenSize.ToVector2());
            blackShader.TrySetParameter("maxOpacity", opacity);

            Main.spriteBatch.GraphicsDevice.Textures[1] = noise.Value;
            //Main.spriteBatch.GraphicsDevice.Textures[2] = noise2.Value;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, blackShader.WrappedEffect, Main.GameViewMatrix.TransformationMatrix);
            Rectangle rekt = new(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth, Main.screenHeight);
            spriteBatch.Draw(blackTile.Value, rekt, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);

            // aurora


            Main.spriteBatch.UseBlendState(BlendState.Additive);

            int timer = (int)(Main.GlobalTimeWrappedHourly * 60f);
            if (timer % 4 == 0)
            {
                Vector2 rayPos = Vector2.UnitX * Main.rand.NextFloat(-Main.screenWidth * 1.3f, Main.screenWidth * 1.3f);
                Vector2 velocity = Vector2.UnitX * Main.rand.NextFloat(-16, 16);
                float maxRot = MathHelper.PiOver2 * 0.05f;
                float rayRot = Main.rand.NextFloat(-maxRot, maxRot);
                int rayTime = 320;
                float rayRotSpeed = Main.rand.NextFloat(0.25f * maxRot / rayTime, maxRot / rayTime);
                rayRotSpeed /= 14f;
                rayRotSpeed *= -rayRot.NonZeroSign();
                var ray = new LightRay(rayPos, velocity, rayRot, rayRotSpeed, rayTime);
                LightRays.Add(ray);
            }


            Vector2 lightRayOrigin = Vector2.UnitX * rayTexture.Width() / 2;
            List<LightRay> removeRays = [];
            for (int i = 0; i < LightRays.Count; i++)
            {
                var ray = LightRays[i];
                ray.TimeLeft--;
                ray.Rotation += ray.RotationSpeed;
                LightRays[i] = ray; // because it's a struct, non-reference type
                if (ray.TimeLeft <= 0)
                {
                    removeRays.Add(ray);
                    continue;
                }
                float rayOpacity = opacity;
                float fadeTime = 16;
                if (ray.TimeLeft <= fadeTime)
                {
                    rayOpacity *= ray.TimeLeft / fadeTime;
                }
                float fadeThreshold = ray.MaxTimeLeft - fadeTime;
                if (ray.TimeLeft >= fadeThreshold)
                {
                    rayOpacity *= 1 - (ray.TimeLeft - fadeThreshold) / fadeTime;
                }
                Vector2 pos = new(screenCenter.X + ray.Position.X, screenCenter.Y + 1362);
                float sin = MathF.Sin(MathF.PI * ray.TimeLeft / (float)ray.MaxTimeLeft);
                int amp = 60;
                pos.Y += amp - sin * amp * 2;
                spriteBatch.Draw(rayTexture.Value, pos - Main.screenPosition, rayTexture.Value.Bounds, Color.White * rayOpacity * 0.4f, ray.Rotation + MathHelper.Pi, lightRayOrigin, 0.59f, SpriteEffects.None, 0);
            }

            foreach (var ray in removeRays)
                LightRays.Remove(ray);

            spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


            //Main.spriteBatch.ResetToDefault();

            //DoTvBands(spriteBatch, opacity);



            float[] scalers = [0.1f, 0.15f, 0.2f];
            float[] yOffset = [0f, 80f, 240f];
            float[] colorLerps = [0.2f, 0.5f, 0.9f];

            float yLerp = 0.7f; // LumUtils.InverseLerp(0, (float)Main.worldSurface * 16, screenCenter.Y);

            var bg = ModContent.Request<Texture2D>($"FargowiltasSouls/Assets/Textures/Misc/MutantBackground").Value;

            for (int i = 0; i < 3; i++)
            {

                Color bgColor = color;
                bgColor = Color.Lerp(bgColor, Color.Black, colorLerps[i]);

                Rectangle frame = new Rectangle(0, i * bg.Height / 3, bg.Width, bg.Height / 3);

                Vector2 pos1 = new(
                    -screenCenter.X * scalers[i] % bg.Width,
                    MathHelper.Lerp(Main.screenHeight / 2 + yOffset[i], Main.screenHeight * 0.85f + yOffset[i] / 2, 1 - yLerp)
                    );

                int count = (int)Math.Ceiling(Main.screenWidth * 2f / bg.Width);
                for (int j = 0; j < count; j++)
                {
                    Vector2 pos = pos1;
                    pos.X += bg.Width * j;
                    spriteBatch.Draw(bg, pos, frame, bgColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
                }

                if (i == 2)
                {
                    pos1.Y += 1 * bg.Height / 3;
                    Texture2D texture = ModContent.Request<Texture2D>("Fargowiltas/Assets/InstaVisualSquare").Value;
                    spriteBatch.Draw(texture, pos1, frame, Color.Black, 0, Vector2.Zero, new Vector2(10000f, 10000f), SpriteEffects.None, 0);
                }
            }
            return base.PreDrawCloseBackground(spriteBatch);

            /*
            spriteBatch.Draw(ModContent.Request<Texture2D>("FargowiltasSouls/Content/Sky/MutantSky2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * 0.9f);
            return base.PreDrawCloseBackground(spriteBatch);
            */
        }

    }
}