using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils.Cil;
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
    public class BetsyWyvernClone : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_560";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.Opacity = 0f;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            Player target = Main.player[(int)Projectile.ai[0]];
            if (target == null || !target.Alive())
            {
                Projectile.Kill();
                return;
            }

            Projectile.scale = LumUtils.Saturate(MathHelper.SmoothStep(0f, 1f, Projectile.ai[1] / 60));
            Projectile.Opacity = LumUtils.Saturate(MathHelper.Lerp(0f, 1f, Projectile.ai[1] / 60));

            Projectile.ai[1]++;
            Projectile.ai[2] = Projectile.ai[1] < 60 ? (Projectile.ai[1] / 6) % 4 : 4;

            if (Projectile.ai[1] < 60)
            {
                if (Projectile.ai[1] < 20)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                Projectile.rotation = MathHelper.Pi;
                Projectile.velocity = -Vector2.UnitY;
            }
            else if (Projectile.ai[1] == 60)
            {
                SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, Projectile.Center);
                Projectile.velocity = 12 * Vector2.UnitX.RotatedBy((target.Center - Projectile.Center).ToRotation());
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi * Projectile.direction;
            }
            else if (Projectile.ai[1] < 600)
            {
                float rotDiff = FargoSoulsUtil.RotationDifference(Projectile.velocity, Projectile.SafeDirectionTo(target.Center));
                Projectile.velocity = Projectile.velocity.RotatedBy(0.07f * rotDiff);
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi;
                if (Math.Abs(rotDiff) > 0.5f * MathHelper.PiOver2)
                {
                    Projectile.ai[1] = 600;
                }
            }
            else if (Projectile.ai[1] >= 600)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
                Projectile.Opacity -= 1 / 40f;
                if (Projectile.Opacity <= 0)
                {
                    Projectile.Kill();
                    return;
                }
            }

        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D text = TextureAssets.Npc[NPCID.DD2WyvernT3].Value;
            int frameHeight = text.Height / 5;
            Rectangle frame = text.Frame(1, 5, 0, (int)Projectile.ai[2]);
            Vector2 origin2 = frame.Size() / 2;
            SpriteEffects flip = Projectile.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition, frame, lightColor * Projectile.Opacity, Projectile.rotation, origin2, Projectile.scale, flip, 0);

            return false;
        }
    }
}
