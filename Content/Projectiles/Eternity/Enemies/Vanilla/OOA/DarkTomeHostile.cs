using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Projectiles.Weapons.BossWeapons;
using FargowiltasSouls.Content.Projectiles.Weapons.Minions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static FargowiltasSouls.Content.Bosses.Magmaw.Magmaw;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA
{
    public class DarkTomeHostile : ModProjectile
    {
        
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Weapons/BossWeapons", "Tome");

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 8;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }

        public ref float Timer => ref Projectile.ai[0];

        public ref float State => ref Projectile.ai[1];

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 400;
            Projectile.hostile = true;
            Projectile.penetrate = -1;

            Projectile.Opacity = 0f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            //
        }

        public override void AI()
        {
            Projectile.Opacity += 1f / 20;
            Timer++;
            Frame();

            Player target = Main.player[(int)Projectile.ai[2]];
            float rot = (Projectile.Center - target.Center).ToRotation();

            if (State == 0)
            {
                Projectile.rotation = -MathHelper.PiOver2;
                float speed = (Timer * Timer) / 150f - 2f;
                if (Timer == 1)
                {
                    FargoSoulsUtil.DustRing(Projectile.Center, 30, DustID.PinkTorch, 2f, scale: 2f);
                    SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.5f }, Projectile.Center);
                }
                Projectile.velocity = speed * Vector2.UnitY;
                if (Projectile.Center.Y - target.Center.Y > 150)
                {
                    Projectile.velocity.Normalize();
                    State = 1;
                    Timer = 0;
                }
            }
            else
            {
                Projectile.rotation = rot;
            }

            if (State == 2)
            {
                if (Timer == 1 && FargoSoulsUtil.HostCheck && target.active && !target.dead)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath52 with { Volume = 0.5f }, Projectile.Center);
                    FargoSoulsUtil.DustRing(Projectile.Center, 30, DustID.PinkTorch, 2f, scale: 1f);
                    Vector2 vel = -2 * Vector2.UnitX.RotatedBy(rot);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        Projectile p = Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), Projectile.Center, vel, ModContent.ProjectileType<TomeShotHostile>(), Projectile.damage / 2, 0.1f);
                    }
                    Projectile.velocity = -0.5f * vel;

                }
                if (Timer > 20)
                {
                    Projectile.Kill();
                }
            }
        }

        void Frame()
        {
            switch (State)
            {
                case 0:
                    Projectile.frame = 0;
                    break;
                case 1:
                    if (Timer % 3 == 0)
                    {
                        if (Projectile.frame < 4)
                            Projectile.frame++;
                        else
                        {
                            State = 2;
                            Timer = 0;
                        }
                    }
                    break;
                case 2:
                    if (Timer % 4 == 0)
                    {
                        if (++Projectile.frame > 7)
                            Projectile.frame = 5;
                    }
                    break;
                case 3:
                    Projectile.frame = 4;
                    break;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;


            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            SpriteEffects spriteEffects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.spriteBatch.UseBlendState(BlendState.Additive);

            for (int k = Projectile.oldPos.Length - 1; k >= 0; k--)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + origin2 + new Vector2(0f, Projectile.gfxOffY);
                Color oldColor = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / ((float)Projectile.oldPos.Length * 2));
                oldColor *= 2.5f;
                Main.EntitySpriteDraw(texture2D13, drawPos, rectangle, oldColor, Projectile.rotation, origin2, Projectile.scale, spriteEffects, 0);
            }
            Color color = Projectile.GetAlpha(lightColor);
            Color glowColor = Projectile.GetAlpha(Color.Purple);
            if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height) && Projectile.Opacity > 0.7f)
                color = glowColor;

            Vector2 drawPosition = Projectile.position - Main.screenPosition + origin2 + new Vector2(0f, Projectile.gfxOffY);
            for (int j = 0; j < 12; j++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12).ToRotationVector2() * 4f * Projectile.scale;

                Main.EntitySpriteDraw(texture2D13, drawPosition + afterimageOffset, new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(glowColor), Projectile.rotation, origin2, Projectile.scale, spriteEffects, 0);
            }
            Main.spriteBatch.ResetToDefault();

            Main.EntitySpriteDraw(texture2D13, drawPosition, new Microsoft.Xna.Framework.Rectangle?(rectangle), color, Projectile.rotation, origin2, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}
