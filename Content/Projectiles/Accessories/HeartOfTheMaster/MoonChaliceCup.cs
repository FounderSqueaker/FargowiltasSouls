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
        #region Buffs
        public static readonly List<int> ChaliceBuffsUse =
        [
            BuffID.AmmoReservation,
            BuffID.Archery,
            BuffID.Battle,
            BuffID.Builder,
            BuffID.BiomeSight,
            BuffID.Calm,
            BuffID.Crate,
            BuffID.Dangersense,
            BuffID.Endurance,
            BuffID.WellFed3,
            BuffID.Featherfall,
            BuffID.Fishing,
            BuffID.Flipper,
            BuffID.Gills,
            BuffID.Gravitation,
            BuffID.Heartreach,
            BuffID.Hunter,
            BuffID.Inferno,
            BuffID.Invisibility,
            BuffID.Ironskin,
            BuffID.Lifeforce,
            BuffID.Lucky,
            BuffID.MagicPower,
            BuffID.ManaRegeneration,
            BuffID.Mining,
            BuffID.NightOwl,
            BuffID.ObsidianSkin,
            BuffID.Rage,
            BuffID.Regeneration,
            BuffID.Shine,
            BuffID.Sonar,
            BuffID.Spelunker,
            BuffID.Summoning,
            BuffID.Swiftness,
            BuffID.Thorns,
            BuffID.Titan,
            BuffID.Warmth,
            BuffID.WaterWalking,
            BuffID.Wrath,
            BuffID.AmmoBox,
            BuffID.Bewitched,
            BuffID.Clairvoyance,
            BuffID.Sharpened,
            BuffID.WarTable,
        ];
        #endregion

        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Accessories/HeartOfTheMaster", Name);
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.scale = 0.8f;
        }

        public override bool? CanDamage() => false;

        public ref float rot => ref Projectile.ai[0];
        public ref float count => ref Projectile.ai[1];
        public override void AI()
        {

            Player player = Main.player[Projectile.owner];
            if (!player.Alive())
            {
                Projectile.Kill();
                return;
            }


            if (count == 0f)
            {
                Projectile.direction = player.direction;
                Projectile.spriteDirection = -Projectile.direction;
            }
            int spriteDir = Projectile.spriteDirection;
            
            player.ChangeDir(-spriteDir);

            float xOffset = spriteDir * -14f;
            Projectile.rotation = player.gravDir * float.Pi / 360f * rot;
            count++;

            if (count >= 180)
            {
                Projectile.Kill();
                return;
            }

            Vector2 holdOffset = new Vector2(xOffset, player.gravDir * 6f);
            player.heldProj = Projectile.whoAmI;
            Projectile.Center = player.Center + holdOffset;

            if (count <= 120f && (count % 60) > 30)
                rot += spriteDir;
            
            float handRotation = -spriteDir * 30 + (rot * MathHelper.Pi / 630);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, handRotation);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, handRotation);

            Projectile.position.Y += player.gravDir * spriteDir * (rot / -12f);

            Projectile.position.X -= rot / 14;

            if (count >= 150f)
            {
                rot -= 2f * spriteDir;

                if (-spriteDir * rot > 0f)
                    rot = 0f;
            }

            // gulp
            if (count % 60 == 1)
            {
                int gulpCount = (int)Math.Floor(count / 60f);
                float drinkPitch = -0.6f + 0.2f * gulpCount;
                for (int down = gulpCount; down < ChaliceBuffsUse.Count; down += 3)
                {
                    int buff = ChaliceBuffsUse[down];
                    int duration = 108000; //30 min
                    player.AddBuff(buff, duration);
                }
                SpilledDrink(player);
                SoundEngine.PlaySound(SoundID.Item3 with { Pitch = drinkPitch }, player.Center);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            if (!player.Alive())
            {
                Projectile.Kill();
                return false;
            }

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float rot = Projectile.rotation;
            SpriteEffects flip = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (player.gravDir == -1) 
                flip |= SpriteEffects.FlipVertically;
            Main.EntitySpriteDraw(texture, drawPos, texture.Frame(), lightColor, rot, new Vector2(texture.Width / 2, texture.Height / 2), Projectile.scale, flip);
            return false;
        }

        public void SpilledDrink(Player player)
        {
            player = Main.player[Projectile.owner];
            if (!player.Alive())
            {
                Projectile.Kill();
                return;
            }

            for (int i = 0; i < 50; i++)
            {
                float randomDust = Main.rand.Next(6, 10);
                float xPos = player.Center.X - Projectile.spriteDirection * randomDust;
                float ySpeed = 2 * Main.rand.NextFloat();
                float yPos = player.Center.Y - player.gravDir * ((player.height / 2f) - 15);
                Vector2 dustPosition = new Vector2(xPos, yPos);
                Color[] drink = new Color[]
                {
                    new(254, 126, 229), //nebula
                    new(254, 203, 58), //solar
                    new(0, 242, 170), //vortex
                    new(104, 214, 255) //stardust
                };
                Color drinkDust = Main.rand.NextFromList(drink);
                float dustRot = Main.rand.NextFloat(-0.35f, 0.35f);
                Dust.NewDustPerfect(dustPosition, DustID.FoodPiece, ySpeed * Vector2.UnitY.RotatedBy(dustRot), newColor: drinkDust);
            }
        }
    }
}