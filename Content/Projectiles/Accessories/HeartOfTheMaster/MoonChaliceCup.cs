using System;
using System.Collections.Generic;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Projectiles.Weapons.ChallengerItems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace FargowiltasSouls.Content.Projectiles.Accessories.HeartOfTheMaster
{
    public class MoonChaliceCup : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Accessories/HeartOfTheMaster", "MoonChaliceCup");
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 30;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.alpha = 255;
            Projectile.scale = 0.8f;
        }
        public ref float rot => ref Projectile.ai[0];
        public ref float count => ref Projectile.ai[1];
        public override void AI()
        {

            Player player = Main.player[Projectile.owner];
            if (count == 0f)
            {
                Projectile.direction = Main.MouseWorld.DirectionTo(player.Center).X <= 0 ? 1 : -1;
                player.ChangeDir(Projectile.direction);
                Projectile.spriteDirection = -Projectile.direction;
            }
            float xOffset = Projectile.spriteDirection == -1 ? 14f : -14f;
            float rotate = (player.gravDir == 1) ? float.Pi / 360 * rot : float.Pi / -360 * rot;
            Projectile.rotation = rotate;
            int down;
            int dir = Projectile.spriteDirection; //i really don't want to write out "Projectile.spriteDirection" a trillion times tyvm
            count++;

            Vector2 holdOffset = new Vector2(xOffset, player.gravDir == -1 ? -6f : 6f);
            player.heldProj = Projectile.whoAmI;
            Projectile.Center = player.Center + holdOffset;

            if (count >= 30f && count < 61f || count >= 90f && count < 121f)
            {
                if (dir == -1)
                {
                    rot--;
                }
                else
                    rot++;
            }
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (dir == -1) ? 30 + rot * MathHelper.Pi / 630 : (-1) * (30 - rot * MathHelper.Pi / 630));
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, (dir == -1) ? 30 + rot * MathHelper.Pi / 630 : (-1) * (30 - rot * MathHelper.Pi / 630));
            if (player.gravDir == -1)
            {
                Projectile.position.Y -= (dir == -1) ? rot / 12 : (-1) * rot / 12;
            }
            else
            {
                Projectile.position.Y += (dir == -1) ? rot / 12 : (-1) * rot / 12;
            }
            Projectile.position.X -= (dir == -1) ? rot / 12 : rot / 10;

            if (count >= 150f)
            {
                if (dir == -1)
                {
                    rot += 2f;
                    if (rot > 0f)
                        rot = 0f;
                }
                else
                {
                    rot -= 2f;
                    if (rot < 0f)
                        rot = 0f;
                }
            }

            if (count == 1 || count == 61 || count == 121) //you don't gulp 180 times methinks
            {
                float drinkPitch = (count == 61) ? -0.4f : (count == 121) ? -0.2f : -0.6f;
                for (down = (count == 61) ? 1 : (count == 121) ? 2 : 0; down < ChalicePotionEffect.ChaliceBuffsUse.Count; down += 3)
                {
                    int buff = ChalicePotionEffect.ChaliceBuffsUse[down];
                    int duration = 108000; //30 min
                    player.AddBuff(buff, duration);
                }
                SpilledDrink(player);
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_3") { Pitch = drinkPitch }, player.Center);
            }
            Projectile.timeLeft--;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float rot = Projectile.rotation;
            SpriteEffects flip = SpriteEffects.None;
            if (player.gravDir == -1) 
                flip |= SpriteEffects.FlipVertically;
            if (Projectile.spriteDirection == -1) 
                flip |= SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(texture, drawPos, texture.Frame(), lightColor, rot, new Vector2(texture.Width / 2, texture.Height / 2), Projectile.scale, flip);
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return base.CanHitNPC(target);
        }
        public void SpilledDrink(Player player)
        {
            player = Main.player[Projectile.owner];
            for (int i = 0; i < 50; i++)
            {
                float randomDust = Main.rand.Next(6, 10);
                float xPos = player.Center.X + (Projectile.spriteDirection == -1 ? randomDust : (-1)*randomDust);
                float ySpeed = 2 * Main.rand.NextFloat();
                Vector2 dustPosition = new Vector2(xPos, player.gravDir == -1 ? player.Bottom.Y - 15 : player.Top.Y + 15);
                Color[] drink = new Color[]
                {
                    new Color(r: 254, g: 126, b: 229), //nebula
                    new Color(r: 254, g: 203, b: 58), //solar
                    new Color(r: 0, g: 242, b: 170), //vortex
                    new Color(r: 104, g: 214, b: 255) //stardust
                };
                Dust dust;
                Color drinkDust = drink[Main.rand.Next(drink.Length)];
                if (randomDust <= 8 && Projectile.spriteDirection == -1)
                    dust = Dust.NewDustPerfect(dustPosition, DustID.WhiteTorch, new Vector2(player.velocity.X, ySpeed), 0, drinkDust, 1.5f);
                else if ((-1)*randomDust >= -8 && Projectile.spriteDirection == 1)
                    dust = Dust.NewDustPerfect(dustPosition, DustID.WhiteTorch, new Vector2(player.velocity.X, ySpeed), 0, drinkDust, 1.5f);
                else if (randomDust > 8 || randomDust < -8)
                    dust = Dust.NewDustPerfect(dustPosition, DustID.WhiteTorch, new Vector2(player.velocity.X, ySpeed-1f), 0, drinkDust, 1.5f);
            }
        }
    }
}