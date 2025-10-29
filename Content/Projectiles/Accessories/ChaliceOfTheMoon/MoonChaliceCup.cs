using System;
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
namespace FargowiltasSouls.Content.Projectiles.Accessories.ChaliceOfTheMoon
{
    public class MoonChaliceCup : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Accessories/HeartOfTheMaster", "SqueakersWildNightDrink");
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 30;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.alpha = 255;
        }
        public ref float timer => ref Projectile.ai[0];
        public ref float rot => ref Projectile.ai[1];
        public ref float based => ref Projectile.ai[2];

        public override void AI()
        {

            Player player = Main.player[Projectile.owner];
            float bleep = (Main.MouseWorld.DirectionTo(player.Center).X <= 0 ? 10f : -10f);
                based++;
            Vector2 holdOffset = new Vector2(bleep, -20f).RotatedBy(rot);

                player.heldProj = Projectile.whoAmI;
                Projectile.Center = player.Center + holdOffset;

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, rot + MathHelper.Pi);
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, rot + MathHelper.Pi);
                Projectile.direction = Main.MouseWorld.DirectionTo(player.Center).X <= 0 ? 1 : -1;
                player.ChangeDir(Projectile.direction);
                Projectile.spriteDirection = -Projectile.direction;

                int down;
                /*if (player.itemAnimation >= 61)
                {
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, MathHelper.Pi);
                    player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, MathHelper.Pi);
                }
                else if (player.itemAnimation <= 30)
                {
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, MathHelper.Pi);
                    player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, MathHelper.Pi);
                }
                else
                {
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, MathHelper.Pi);
                    player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, MathHelper.Pi);
                }
                */
                if (based == 1)
                {
                    for (down = 0; down < ChalicePotionEffect.ChaliceBuffsUse.Count; down += 3)
                    {
                        int buff = ChalicePotionEffect.ChaliceBuffsUse[down];
                        int duration = 108000; //buff == BuffID.Lucky ? 60 * 60 * 15 : 60 * 60 * 8;
                        player.AddBuff(buff, duration);
                    }
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_3") { Pitch = -0.6f }, player.Center);
                }
                else if (based == 61)
                {
                    for (down = 1; down < ChalicePotionEffect.ChaliceBuffsUse.Count; down += 3)
                    {
                        int buff = ChalicePotionEffect.ChaliceBuffsUse[down];
                        int duration = 108000; //buff == BuffID.Lucky ? 60 * 60 * 15 : 60 * 60 * 8;
                        player.AddBuff(buff, duration);
                    }
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_3") { Pitch = -0.4f }, player.Center);
                }
                else if (based == 121)
                {
                    for (down = 2; down < ChalicePotionEffect.ChaliceBuffsUse.Count; down += 3)
                    {
                        int buff = ChalicePotionEffect.ChaliceBuffsUse[down];
                        int duration = 108000; //buff == BuffID.Lucky ? 60 * 60 * 15 : 60 * 60 * 8;
                        player.AddBuff(buff, duration);
                    }
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_3") { Pitch = -0.2f }, player.Center);
                }
            Projectile.timeLeft--;
            Main.NewText($"based: {based}");
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float rot = Projectile.rotation;
            SpriteEffects flip = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(texture, drawPos, texture.Frame(), lightColor, rot, new Vector2(texture.Width / 2, texture.Height / 2), Projectile.scale, flip);
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return base.CanHitNPC(target);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            for (int i = 0; i < 90; i++)
            {
                float face = Main.MouseWorld.DirectionTo(player.Center).X <= 0 ? 1 : -1;
                float xPos = player.Center.X + (face == 1 ? Main.rand.Next(0, 10) : Main.rand.Next(-10, 0));
                float ySpeed = 2 * Main.rand.NextFloat();
                Vector2 dustPosition = new Vector2(xPos, player.Top.Y + 10);
                Color[] drink = new Color[]
                {
                    new Color(r: 254, g: 126, b:229), //nebula
                    new Color(r: 254, g: 203, b: 58), //solar
                    new Color(r: 0, g: 242, b: 170), //vortex
                    new Color(r: 104, g: 214, b: 255) //stardust
                };
                Color drinkDust = drink[Main.rand.Next(drink.Length)];
                Dust.NewDustPerfect(dustPosition, DustID.WhiteTorch, new Vector2(player.velocity.X, player.velocity.Y + ySpeed), 0, drinkDust);
            }
            base.OnSpawn(source);
        }
    }
}