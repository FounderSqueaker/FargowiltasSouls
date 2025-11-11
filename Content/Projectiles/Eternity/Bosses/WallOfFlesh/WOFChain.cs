using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Projectiles.Masomode.Bosses.MechanicalBosses;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.WallOfFlesh
{
    public class WOFChain : ModProjectile
    {
        public int BittenPlayer = -1;
        Vector2 OriginPosition = Vector2.Zero;
        public Vector2 LockPosition => new(Projectile.ai[0], Projectile.ai[1]);
        public float Angle => Projectile.ai[2];
        public int Timer = 0;
        public bool Telegraphing => Timer < TelegraphTime;
        public int TelegraphTime => 130 * Projectile.MaxUpdates;
        public override string Texture => "Terraria/Images/NPC_115";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.npcFrameCount[NPCID.TheHungry];
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2400;
            /*ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;*/
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            //CooldownSlot = ImmunityCooldownID.Bosses;

            Projectile.extraUpdates = 2;
            Projectile.hide = true;
        }

        public override bool? CanDamage() => Telegraphing || (Projectile.timeLeft <= 30 || Projectile.localAI[2] == 1 || BittenPlayer != -1) ? false : base.CanDamage();
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(BittenPlayer);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            BittenPlayer = reader.ReadInt32();
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.1f, 0.5f, 0.7f);

            if (Projectile.timeLeft <= 30 || Projectile.localAI[2] == 1)
            {
                Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0f, 0.05f);
                if (Projectile.Opacity < 0.1f)
                {
                    Projectile.Kill();
                    return;
                }
            }

            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                OriginPosition = Projectile.Center;
                //Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value = Main.npcTexture[NPCID.TheHungry];
            }

            Timer++;
            if (Timer < TelegraphTime) // telegraphing
            {
                Projectile.rotation = Projectile.DirectionTo(LockPosition).ToRotation();
                Vector2 pos = LockPosition - Angle.ToRotationVector2() * 370;
                pos += Main.rand.NextVector2Circular(5, 5);
                Projectile.velocity = FargoSoulsUtil.SmartAccel(Projectile.Center, pos, Projectile.velocity, 0.5f, 0.07f);
            }
            else if (Timer == TelegraphTime) // telegraph exactly
            {
                Projectile.velocity = -Angle.ToRotationVector2() * 8;
            }
            else
            {
                if (Projectile.velocity != Vector2.Zero && Main.rand.NextBool(3))
                {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemSapphire, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 114, default, 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                }

                if (Projectile.velocity.LengthSquared() < 15 * 15) // accelerate
                    Projectile.velocity += Angle.ToRotationVector2() * 0.15f;

                //stop moving at vertical limits of underworld
                if (Projectile.velocity.Y > 0 && Projectile.Center.Y / 16 >= Main.maxTilesY
                    || Projectile.velocity.Y < 0 && Projectile.Center.Y / 16 <= Main.maxTilesY - 450)
                {
                    Projectile.position -= Projectile.velocity * 2f;
                    Projectile.velocity = Vector2.Zero;
                }

                if (BittenPlayer != -1)
                {

                    Player victim = Main.player[BittenPlayer];
                    if (victim.active && !victim.ghost && !victim.dead
                        && (Projectile.Distance(victim.Center) < 160 || victim.whoAmI != Main.myPlayer)
                        && victim.FargoSouls().MashCounter < 20)
                    {
                        victim.AddBuff(ModContent.BuffType<GrabbedBuff>(), 2);
                        victim.velocity = Vector2.Zero;
                        Projectile.Center = victim.Center;
                    }
                    else
                    {
                        BittenPlayer = -1;
                        Projectile.netUpdate = true;
                    }
                }

                if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.wallBoss, NPCID.WallofFlesh)
                    && Math.Abs(Projectile.Center.X - Main.npc[EModeGlobalNPC.wallBoss].Center.X) < 50)
                {
                    Projectile.localAI[2] = 1; //chain dies when wall moves over it
                }

                if (Projectile.velocity != Vector2.Zero)
                {
                    Projectile.rotation = Angle;

                    if (++Projectile.frameCounter > 6 * (Projectile.extraUpdates + 1))
                    {
                        Projectile.frameCounter = 0;
                        if (++Projectile.frame >= Main.projFrames[Projectile.type])
                            Projectile.frame = 0;
                    }
                }
            }
        }
        public override bool CanHitPlayer(Player target)
        {
            if (BittenPlayer != -1)
                return false;
            return base.CanHitPlayer(target);
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if (WorldSavingSystem.MasochistModeReal)
            {
                target.longInvince = true;
                modifiers.SourceDamage *= 0.25f;
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (WorldSavingSystem.MasochistModeReal && BittenPlayer == -1)
            {
                BittenPlayer = target.whoAmI;
                Projectile.netUpdate = true;
            }
            /*
            if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld) //if (Fargowiltas.Instance.MasomodeEXLoaded)
            {
                if (!target.tongued)
                    SoundEngine.PlaySound(SoundID.ForceRoarPitched, target.Center);
                target.AddBuff(BuffID.TheTongue, 10);
            }
            */
            target.AddBuff(BuffID.OnFire, 300);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            if (!TextureAssets.Chain12.IsLoaded)
                return false;

            if (Projectile.ai[0] != 0)
            {
                Texture2D texture = TextureAssets.Chain12.Value;
                Vector2 position = Projectile.Center;
                Vector2 mountedCenter = OriginPosition;
                Rectangle? sourceRectangle = new Rectangle?();
                Vector2 origin = new(texture.Width * 0.5f, texture.Height * 0.5f);
                float num1 = texture.Height;
                Vector2 vector24 = mountedCenter - position;
                float rotation = (float)Math.Atan2(vector24.Y, vector24.X) - 1.57f;
                bool flag = true;
                if (float.IsNaN(position.X) && float.IsNaN(position.Y))
                    flag = false;
                if (float.IsNaN(vector24.X) && float.IsNaN(vector24.Y))
                    flag = false;
                while (flag)
                    if (vector24.Length() < num1 + 1.0)
                    {
                        flag = false;
                    }
                    else
                    {
                        Vector2 vector21 = vector24;
                        vector21.Normalize();
                        position += vector21 * num1;
                        vector24 = mountedCenter - position;
                        Color color2 = Lighting.GetColor((int)position.X / 16, (int)(position.Y / 16.0));
                        color2 = Projectile.GetAlpha(color2);
                        Main.EntitySpriteDraw(texture, position - Main.screenPosition, sourceRectangle, color2, rotation, origin, 1f, SpriteEffects.None, 0);
                    }
            }

            if (!TextureAssets.Npc[NPCID.TheHungry].IsLoaded)
                return false;

            Texture2D texture2D13 = TextureAssets.Npc[NPCID.TheHungry].Value;
            int num156 = texture2D13.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = lightColor;
            color26 = Projectile.GetAlpha(color26);

            SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            /*for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i += 2)
            {
                Color color27 = color26 * Projectile.Opacity * 0.5f;
                color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Vector2 value4 = Projectile.oldPos[i];
                float num165 = Projectile.oldRot[i];
                Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, effects, 0);
            }*/

            /*
            if (Telegraphing)
            {
                Color color = Color.DeepPink;
                Asset<Texture2D> line = TextureAssets.Extra[178];
                float opacity = 0.9f;
                float rot = Projectile.rotation;
                Main.EntitySpriteDraw(line.Value, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, color * opacity, rot, new Vector2(0, line.Height() * 0.5f), new Vector2(0.47f, Projectile.scale * 7), SpriteEffects.None);
            }
            */

            if (CanDamage() != false)
            {
                Main.spriteBatch.UseBlendState(BlendState.Additive);
                for (int j = 0; j < 12; j++)
                {
                    Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 4f;
                    Color glowColor = Color.Purple;

                    Main.EntitySpriteDraw(texture2D13, Projectile.Center + afterimageOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
                }
                Main.spriteBatch.ResetToDefault();
            }

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);

            //spriteBatch.End(); spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.hide)
                overWiresUI.Add(index);
        }
    }
}