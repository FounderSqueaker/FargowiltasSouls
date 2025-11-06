using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Core;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using UtfUnknown.Core.Probers;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Plantera
{
    public class DicerPlantera : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Eternity/Bosses/Plantera", Name);

        private const float range = 160f;

        public bool blingle;
        public int lastFrame;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1200;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
            writer.Write(lastFrame);
            writer.Write(blingle);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
            lastFrame = reader.ReadInt32();
            blingle = reader.ReadBoolean();
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override void AI()
        {
            bool recolor = SoulConfig.Instance.BossRecolors && WorldSavingSystem.EternityMode;
            if (recolor)
                Lighting.AddLight(Projectile.Center, 25f / 255, 47f / 255, 64f / 255);
            else
                Lighting.AddLight(Projectile.Center, .4f, 1.2f, .4f);


            if (Projectile.localAI[2] == 0) //random rotation direction
            {
                Projectile.localAI[2] = Main.rand.NextFloat(0.5f, 1f) * MathHelper.PiOver2;
                Projectile.localAI[2] *= Main.rand.NextBool() ? 1f : -1f;
            }

            if (Projectile.localAI[1] >= 0)
            {
                /*for (int i = 0; i < 4; i++)
                {
                    int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Main.rand.NextBool() ? 107 : 157);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 0.2f;
                    Main.dust[d].scale = 1.5f;
                }*/
                Projectile.rotation += 0.2f * Projectile.localAI[2] * (float)Math.Sin(Projectile.localAI[0] / 60 * MathHelper.Pi + Projectile.localAI[2]);
                if (++Projectile.localAI[1] > 25)
                {
                    Projectile.localAI[1] = -1;

                    if (Projectile.ai[1] > 0) //propagate
                    {
                        SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);
                        if (FargoSoulsUtil.HostCheck)
                        {
                            Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Projectile.velocity,
                                Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0], Projectile.ai[1] - 1);
                            if (Projectile.ai[0] == 1)
                            {
                                Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.ToRadians(120)),
                                  Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, 0, Projectile.ai[1] - 1);
                            }
                        }
                    }

                    for (int index1 = 0; index1 < 20; ++index1)
                    {
                        int dustID = recolor ?
                        Main.rand.NextBool() ? DustID.GlowingMushroom : DustID.MushroomTorch :
                        Main.rand.NextBool() ? DustID.TerraBlade : DustID.ChlorophyteWeapon;
                        Vector2 vel = Main.rand.NextVector2Circular(4, 4);
                        int index2 = Dust.NewDust(Projectile.Center - Projectile.Size * Projectile.scale / 2, (int)(Projectile.width * Projectile.scale), (int)(Projectile.height * Projectile.scale), dustID, vel.X, vel.Y, 0, new Color(), 2f);
                        Main.dust[index2].noGravity = true;
                        Main.dust[index2].velocity *= 5f;
                    }

                    Projectile.localAI[0] = 50 * (Projectile.ai[1] % 3);

                    Projectile.velocity = Vector2.Zero;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                Projectile.tileCollide = true;
                Projectile.localAI[0]--;

                if (lastFrame != Projectile.frame)
                {
                    blingle = false;
                    Projectile.ai[2] = 0;
                    lastFrame = Projectile.frame;
                }

                if (Projectile.localAI[0] < -30 && Projectile.localAI[0] > -120)
                {
                    if (++Projectile.frameCounter >= 22)
                    {
                        Projectile.frameCounter = 0;
                        if (++Projectile.frame >= Main.projFrames[Type])
                            Projectile.frame = Main.projFrames[Type] - 1;
                    }
                    //Projectile.rotation += 0.2f * Projectile.localAI[2] * (float)Math.Sin(Projectile.localAI[0] / 60 * MathHelper.Pi + Projectile.localAI[2]);
                }
                else if (Projectile.localAI[0] == -120)
                {
                    for (int index1 = 0; index1 < 20; ++index1)
                    {
                        int dustID = recolor ?
                        Main.rand.NextBool() ? DustID.GlowingMushroom : DustID.MushroomTorch :
                        Main.rand.NextBool() ? DustID.TerraBlade : DustID.ChlorophyteWeapon;
                        Vector2 vel = Main.rand.NextVector2Circular(4, 4);
                        int index2 = Dust.NewDust(Projectile.Center - Projectile.Size * Projectile.scale / 2, (int)(Projectile.width * Projectile.scale), (int)(Projectile.height * Projectile.scale), dustID, vel.X, vel.Y, 0, new Color(), 2f);
                        Main.dust[index2].noGravity = true;
                        Main.dust[index2].velocity *= 5f;
                    }

                    /*const int max = 30; //make some indicator dusts
                    for (int i = 0; i < max; i++)
                    {
                        Vector2 vector6 = Vector2.UnitY * 5f;
                        vector6 = vector6.RotatedBy((i - (max / 2 - 1)) * 6.28318548f / max) + Projectile.Center;
                        Vector2 vector7 = vector6 - Projectile.Center;
                        int d = Dust.NewDust(vector6 + vector7, 0, 0, 107, 0f, 0f, 0, default(Color), 2f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity = vector7;
                    }*/
                }
                else if (Projectile.localAI[0] < -150) //explode
                {
                    Projectile.localAI[0] = 0;
                    Projectile.netUpdate = true;

                    SoundEngine.PlaySound(SoundID.Item14, Projectile.Center); //spray
                 
                    if (FargoSoulsUtil.HostCheck)
                    {
                        bool planteraAlive = NPC.plantBoss > -1 && NPC.plantBoss < Main.maxNPCs && Main.npc[NPC.plantBoss].active && Main.npc[NPC.plantBoss].type == NPCID.Plantera;
                        //die after this many explosions, plantera is dead, or if i have no decent line of sight to plantera
                        if (!planteraAlive || !Collision.CanHitLine(Projectile.Center, 0, 0, Main.npc[NPC.plantBoss].Center + (Projectile.Center - Main.npc[NPC.plantBoss].Center) / 2, 0, 0))
                        {
                            Projectile.Kill();
                            return;
                        }
                        else //do the actual attack
                        {
                            const float time = 16f;
                            const float max = 16f;
                            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                            for (int i = 0; i < max; i++)
                            {
                                int type = ModContent.ProjectileType<DicerPlanteraSeed>();
                                int p = Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, range / time * Vector2.UnitX.RotatedBy(Math.PI * 2f / max * i + rotation),
                                    type, Projectile.damage, Projectile.knockBack, Projectile.owner);
                                if (p != Main.maxProjectiles)
                                    Main.projectile[p].timeLeft = (int)time;
                            }
                            Projectile.frame = 0;
                            Projectile.rotation = Main.rand.Next(360);
                        }

                        
                        if (Projectile.localAI[1]-- < -3)
                            Projectile.Kill();
                    }
                }
            }
        }

        /*public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(ModContent.BuffType<IvyVenom>(), 240);
        }*/

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
        }

        

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            SpriteEffects spriteEffects = SpriteEffects.None;

            Color color26 = Color.White;
            color26 = Projectile.GetAlpha(color26);

            float scale = Projectile.scale;
           
            if (lastFrame == Projectile.frame)
            {
                if (!Main.gamePaused)
                    ++Projectile.ai[2];
                if (!blingle)
                    scale = MathHelper.Lerp(Projectile.scale, 1.15f, Projectile.ai[2] * 0.3f);
                else
                    scale = MathHelper.Lerp(1.15f, Projectile.scale, Projectile.ai[2] * 0.3f);
                if (scale >= 1.15f)
                {
                    blingle = true;
                    Projectile.ai[2] = 0;
                    scale = 1.15f;

                }
                if (scale <= 1)
                    scale = 1;
            
            }
            
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle?(rectangle), color26, Projectile.rotation, origin2, scale, spriteEffects, 0);
            return false;
        }
    }
}