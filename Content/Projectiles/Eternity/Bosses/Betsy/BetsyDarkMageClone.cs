using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    internal class BetsyDarkMageClone : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_565";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.tileCollide = false;
            Projectile.light = 1f;
            Projectile.FargoSouls().DeletionImmuneRank = 1;
        }

        public override void AI()
        {
            ref float timer = ref Projectile.ai[1];
            timer++;
            
            if (!Main.projectile.Any(x => x.Alive() && x.type == ModContent.ProjectileType<BetsyLightningAura>()))
            {
                Projectile.Kill();
                return;
            }

            if (timer <= 60)
            {
                if (Projectile.frameCounter++ > 6)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame > 4)
                        Projectile.frame = 0;
                }
            }
            else
            {
                if (Projectile.frameCounter++ > 6)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = Projectile.frame == 6 ? 7 : 6;
                }

                Vector2 chargeCenter = Projectile.Center - 100 * Vector2.UnitY;
                float scale = MathHelper.Clamp(timer / 240f, 0f, 2f);
                if (timer % 60 == 1)
                {
                    SoundEngine.PlaySound(FargosSoundRegistry.CoffinHandCharge, Projectile.Center);
                    FargoSoulsUtil.DustRing(chargeCenter, 30, DustID.UltraBrightTorch, scale * 3f, scale: scale);
                }

                float r = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                new SparkParticle(chargeCenter + 100 * scale * Vector2.UnitX.RotatedBy(r), -10 * scale * Vector2.UnitX.RotatedBy(r), Color.RoyalBlue, scale, 10).Spawn();
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
                SoundEngine.PlaySound(SoundID.DD2_DarkMageDeath with { Volume = 2f }, Projectile.Center);
            for (int i = 0; i < 3; i++)
            {
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, 20 * Vector2.UnitY, Main.rand.Next(1009, 1020));
            }

            base.OnKill(timeLeft);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadNPC(NPCID.DD2DarkMageT3);
            Texture2D text = TextureAssets.Npc[NPCID.DD2DarkMageT3].Value;
            Rectangle frame = text.Frame(5, 9, 0, Projectile.frame);
            Vector2 origin2 = frame.Size() / 2;

            Color color = Color.Lerp(lightColor, Color.Purple, 0.5f);

            Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition, frame, color * 0.7f, 0, origin2, Projectile.scale, SpriteEffects.None);

            return false;
        }
    }
}
