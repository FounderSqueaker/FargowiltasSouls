using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy
{
    public class BetsyDD2Shield : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Eternity/Bosses/Betsy", Name);

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 11;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public ref float Timer => ref Projectile.ai[1];
        public ref float BGFrame => ref Projectile.ai[2];

        public override void AI()
        {
            Projectile aura = FargoSoulsUtil.ProjectileExists(Projectile.ai[0], ModContent.ProjectileType<BetsyLightningAura>());
            if (aura == null)
            {
                Projectile.Kill();
                return;
            }

            Timer++;
            Projectile.rotation = Projectile.AngleTo(aura.Center);
            Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(aura.Center);


            if (Projectile.frameCounter++ > 8)
            {
                BGFrame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame < 9)
                {
                    Projectile.frame++;
                }
                if (BGFrame > 16)
                {
                    BGFrame = 9;
                }
            }

            float auraTimer = aura.ai[2];
            int timeToDet = WorldSavingSystem.MasochistModeReal ? 120 : 180;
            float timeSinceDet = auraTimer - timeToDet;

            if (timeSinceDet >= 0)
            {
                Projectile.velocity = Vector2.UnitX.RotatedBy(Projectile.rotation);
                if (timeSinceDet % 10 == 0)
                {
                    for (int i = 0; i < 2; i++)
                        SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
                    Projectile.Center -= Projectile.velocity * 10;
                }
                if (timeSinceDet == 90)
                {
                    for (int i = 0; i < 3; i++)
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Variants = [2], Pitch = 0.5f }, Projectile.Center);
                    Projectile.frame = 10;
                }

                if (timeSinceDet == 180)
                {
                    for (int i = 0; i < 5; i++)
                        SoundEngine.PlaySound(SoundID.NPCHit43, Projectile.Center);
                    Projectile.Kill();
                    return;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D shieldTexture = TextureAssets.Projectile[Type].Value;
            Rectangle shieldFrame = shieldTexture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 shieldOrigin2 = shieldFrame.Size() / 2;

            Texture2D bgTexture = ModContent.Request<Texture2D>(Texture + "_BG", AssetRequestMode.ImmediateLoad).Value;
            Rectangle bgFrame = bgTexture.Frame(1, 17, 0, (int)BGFrame);
            Vector2 origin2 = bgFrame.Size() / 2;
            SpriteEffects flip = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            Main.EntitySpriteDraw(bgTexture, Projectile.Center - (bgFrame.Width / 8) * Vector2.UnitX.RotatedBy(Projectile.rotation) - Main.screenPosition, bgFrame, Color.White * 0.9f, Projectile.rotation, origin2, Projectile.scale, flip);
            Main.EntitySpriteDraw(shieldTexture, Projectile.Center - Main.screenPosition, shieldFrame, Color.White, Projectile.rotation, shieldOrigin2, Projectile.scale, flip);
            return false;
        }
    }
}
