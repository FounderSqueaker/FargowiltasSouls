using Fargowiltas;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace FargowiltasSouls.Content.Bosses.TrojanSquirrel
{
    public class TrojanSquirrelHead : TrojanSquirrelLimb
    {
        public bool ReverseHeadAnimation = false;

        public int HeadAnimationType;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 600;
            NPC.DeathSound = FargosSoundRegistry.TrojanHeadDeath;
            NPC.width = baseWidth = 90;
            NPC.height = baseHeight = 78;
        }

        public override void AI()
        {
            //return;
            base.AI();

            if (body == null)
                return;

            //NPC.ai[0] = 1;

            NPC.velocity = Vector2.Zero;
            NPC.target = body.target;
            NPC.direction = NPC.spriteDirection = body.direction;
            NPC.Center = body.Bottom + new Vector2(NPC.direction < 0 ? -10 : 30f * NPC.direction, -163f) * body.scale;

            switch ((int)NPC.ai[0])
            {
                case 0:
                    if (body.ai[0] == 0 && body.localAI[0] <= 0)
                    {
                        NPC.ai[1] += WorldSavingSystem.EternityMode ? 1.5f : 1f;

                        //HeadAnimationType = 1;

                        if (body.dontTakeDamage)
                            NPC.ai[1] += 1f;

                        int threshold = 240;

                        //structured like this so body gets priority first
                        int stallPoint = threshold - 30;
                        if (NPC.ai[1] > stallPoint)
                        {
                            TrojanSquirrel squirrel = body.As<TrojanSquirrel>();
                            if (squirrel.arms != null && squirrel.arms.ai[0] != 0f) //wait if other part is attacking
                                NPC.ai[1] = stallPoint;
                        }

                        if (NPC.ai[1] > threshold && Math.Abs(body.velocity.Y) < 0.05f)
                        {
                            NPC.ai[0] = 1 + NPC.ai[2];
                            NPC.ai[1] = 0;
                            if (Main.expertMode)
                                NPC.ai[2] = NPC.ai[2] == 0 ? 1 : 0;
                            NPC.netUpdate = true;
                            
                        }
                    }
                    break;

                case 1: //acorn spray
                    if (NPC.ai[1] == 0 && !WorldSavingSystem.MasochistModeReal)
                    {
                        //telegraph
                        SoundEngine.PlaySound(FargosSoundRegistry.TrojanGunStartup, NPC.Center);
                        HeadAnimationType = 2;
                        

                        Vector2 pos = NPC.Center;
                        pos.X += 22 * NPC.direction; //FUCKING LAUGH
                        pos.Y += 22;

                        /*
                        for (int j = 0; j < 20; j++)
                        {
                            int d = Dust.NewDust(pos, 0, 0, DustID.GrassBlades, Scale: 3f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].velocity *= 4f;
                            Main.dust[d].velocity.X += NPC.direction * Main.rand.NextFloat(6f, 18f);
                        }
                        */
                    }

                    if (++NPC.ai[1] % (body.dontTakeDamage || WorldSavingSystem.MasochistModeReal ? 30 : 45) == 0)
                    {
                        bool doAttack = true;
                        if (!WorldSavingSystem.MasochistModeReal && NPC.localAI[1] == 0)
                        {
                            NPC.localAI[1] = 1;
                            doAttack = false; //skip the first normally
                        }

                        if (doAttack)
                        {
                            Vector2 pos = ShootCenter;

                            const float gravity = 0.2f;
                            float time = 80f;
                            if (body.dontTakeDamage)
                                time = 60f;
                            if (WorldSavingSystem.MasochistModeReal)
                                time = 45f;
                            Vector2 distance = Main.player[NPC.target].Center - pos;// + player.velocity * 30f;
                            distance.X /= time;
                            distance.Y = distance.Y / time - 0.5f * gravity * time;
                            for (int i = 0; i < 10; i++)
                            {
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    SoundEngine.PlaySound(FargosSoundRegistry.TrojanCannon, pos);
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, distance + Main.rand.NextVector2Square(-0.5f, 0.5f),
                                        ModContent.ProjectileType<TrojanAcorn>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer);
                                }
                            }
                        }
                    }

                    if (NPC.ai[1] > 210)
                    {
                        NPC.ai[0] = 0;
                        NPC.ai[1] = 0;
                        NPC.netUpdate = true;
                        //HeadAnimationType = 1;
                    }
                    break;

                case 2: //squirrel barrage
                    {
                        HeadAnimationType = 4;
                        if (WorldSavingSystem.MasochistModeReal && NPC.ai[1] == 90)
                        {
                            NPC arms = (body.ModNPC as TrojanSquirrel).arms;
                            if (arms != null && arms.ai[0] != 2)
                            {
                                arms.ai[0] = 2;
                                arms.ai[1] = 0;
                                arms.netUpdate = true;
                            }
                        }

                        NPC.ai[1]++;

                        int start = 60 + 30;
                        int end = 240 + 30;
                        if (WorldSavingSystem.MasochistModeReal)
                        {
                            start -= 30 - 30;
                            end -= 90 - 30;
                        }

                        body.velocity.X *= 0.99f;

                        if (NPC.ai[1] % 4 == 0)
                        {
                            ShootSquirrelAt(body.Center + Main.rand.NextVector2Circular(200, 200));

                            if (NPC.ai[1] > start)
                            {
                                float ratio = (NPC.ai[1] - start) / (end - start);
                                Vector2 target = new(NPC.Center.X, Main.player[NPC.target].Center.Y);
                                target.X += Math.Sign(NPC.direction) * (550f + (WorldSavingSystem.EternityMode ? 1800f : 1200f) * (1f - ratio));

                                ShootSquirrelAt(target);
                            }
                        }

                        if (NPC.ai[1] > end)
                        {
                            NPC.ai[0] = 0;
                            NPC.ai[1] = 0;
                            NPC.netUpdate = true;
                            //HeadAnimationType = 1;
                        }
                    }
                    break;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            //Main.NewText("X Frame:" + NPC.frame.X);
            //Main.NewText("Y Frame:" + NPC.frame.Y);
            //Main.NewText("AnimationType:" + HeadAnimationType);
            switch (HeadAnimationType)
            {   
                //Walking.
                case 1:
                    {
                        NPC.frame.X = 0;
                        if (++NPC.frameCounter >= 12)
                        {
                            NPC.frameCounter = 0;

                            if (NPC.frame.Y >= frameHeight * 2)
                                ReverseHeadAnimation = true;

                            if (NPC.frame.Y <= 0)
                                ReverseHeadAnimation = false;

                            if (ReverseHeadAnimation)
                            {
                                NPC.frame.Y -= frameHeight;
                            }
                            else
                                NPC.frame.Y += frameHeight;
                        }
                    }
                    break;
                
                //Acorns
                case 2:
                    {
                        if (NPC.frame.X == 0)
                            NPC.frame.X = 138;
                        if (NPC.frame.X == 138)
                        {
                            if (++NPC.frameCounter >= 12)
                            {
                                NPC.frameCounter = 0;
                                NPC.frame.Y += frameHeight;
                            }
                            if (NPC.frame.Y == frameHeight * 4)
                            {
                                NPC.frameCounter--;
                                NPC.frame.Y = 0;
                                NPC.frame.X += 138;
                            }
                        }
                        if (NPC.frame.X == 138 * 2)
                        {
                            if (++NPC.frameCounter >= 12)
                            {
                               
                                NPC.frameCounter = 0;
                                NPC.frame.Y += frameHeight;

                            }
                            if (NPC.frame.Y == frameHeight * 4)
                            {
                                NPC.frameCounter--;
                                NPC.frame.Y = 0;
                                NPC.frame.X += 138;
                            }
                        }
                        if (NPC.frame.X == 138 * 3)
                        {
                            if (++NPC.frameCounter >= 12)
                            {
                                if (NPC.frame.Y >= frameHeight * 3)
                                    NPC.frame.Y = 0;
                                NPC.frameCounter = 0;
                                NPC.frame.Y += frameHeight;

                            }
                        }

                        if (NPC.ai[0] != 1)
                            HeadAnimationType = 3;
                        
                    }
                    break;

                //Close head again.
                case 3:
                    {
                        if (NPC.frame.X > 138 * 3)
                            NPC.frame.X = 138 * 3;
                        if (NPC.frame.X == 138)
                        {
                            if (++NPC.frameCounter >= 6)
                            {
                                NPC.frameCounter = 0;
                                NPC.frame.Y -= frameHeight;
                            }
                            if (NPC.frame.Y == 0)
                            {
                                NPC.frameCounter--;
                                NPC.frame.Y = 0;
                                NPC.frame.X = 0;
                                HeadAnimationType = 1;
                            }
                        }
                        if (NPC.frame.X == 138 * 2)
                        {
                            if (++NPC.frameCounter >= 6)
                            {
                                NPC.frameCounter = 0;
                                NPC.frame.Y -= frameHeight;

                            }
                            if (NPC.frame.Y == 0)
                            {
                                NPC.frameCounter--;
                                NPC.frame.Y = frameHeight * 4;
                                NPC.frame.X -= 138;
                            }
                        }
                        if (NPC.frame.X == 138 * 3)
                        {
                            if (++NPC.frameCounter >= 6)
                            {
                                if (NPC.frame.Y == 0)
                                {
                                    NPC.frameCounter--;
                                    NPC.frame.Y = frameHeight * 4;
                                    NPC.frame.X -= 138;
                                }
                                    
                                NPC.frameCounter = 0;
                                NPC.frame.Y -= frameHeight;

                            }
                        }

                    }
                    break;
                
                //Squirrel Rain
                case 4:
                    {
                        if (NPC.frame.X == 0)
                            NPC.frame.X = 138;
                        if (NPC.frame.X == 138)
                        {
                            if (++NPC.frameCounter >= 3)
                            {
                                NPC.frameCounter = 0;
                                NPC.frame.Y += frameHeight;
                            }
                            if (NPC.frame.Y == frameHeight * 4)
                            {
                                NPC.frameCounter--;
                                NPC.frame.Y = 0;
                                NPC.frame.X += 138;
                            }
                        }
                        if (NPC.frame.X == 138 * 2)
                        {
                            if (++NPC.frameCounter >= 3)
                            {

                                NPC.frameCounter = 0;
                                NPC.frame.Y += frameHeight;

                            }
                            if (NPC.frame.Y == frameHeight * 4)
                            {
                                NPC.frameCounter--;
                                NPC.frame.Y = 0;
                                NPC.frame.X += 138;
                            }
                        }
                        if (NPC.frame.X == 138 * 3)
                        {
                            if (++NPC.frameCounter >= 3)
                            {
                                if (NPC.frame.Y >= frameHeight * 3)
                                    NPC.frame.Y = 0;
                                NPC.frameCounter = 0;
                                NPC.frame.Y += frameHeight;

                            }
                        }

                        if (NPC.ai[0] != 2)
                            HeadAnimationType = 3;
                    }
                    break;
                default:
                    goto case 1;
            }
           
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture2D13 = TextureAssets.Npc[NPC.type].Value;
            Rectangle rectangle = new Rectangle(NPC.frame.X, NPC.frame.Y, NPC.frame.Width / 4, NPC.frame.Height);
            Vector2 origin2 = rectangle.Size() / 2f;
            //NPC.direction = 1;


            Color color26 = drawColor;
            color26 = NPC.GetAlpha(color26);

            SpriteEffects effects = NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (body != null)
            {
                var trojan = body.As<TrojanSquirrel>();
                Main.EntitySpriteDraw(texture2D13, body.Top - screenPos + new Vector2(10f, NPC.gfxOffY - 30 * NPC.scale) + trojan.bodyOffset, new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, NPC.rotation, origin2, NPC.scale, effects, 0);
            }

            return false;
        }
        public Vector2 ShootCenter => NPC.Center + NPC.direction * Vector2.UnitX * NPC.scale * 30f;
        private void ShootSquirrelAt(Vector2 target)
        {
            float gravity = 0.6f;
            const float origTime = 75;
            float time = origTime;
            if (body.dontTakeDamage)
                time -= 15;
            if (WorldSavingSystem.MasochistModeReal)
                time -= 15;

            gravity *= origTime / time;

            Vector2 distance = target - ShootCenter;// + player.velocity * 30f;
            distance.X += Main.rand.NextFloat(-128, 128);
            distance.X /= time;
            distance.Y = distance.Y / time - 0.5f * gravity * time;

            distance.X += Math.Min(4f, Math.Abs(NPC.velocity.X)) * Math.Sign(NPC.velocity.X);

            SoundEngine.PlaySound(SoundID.Item1, ShootCenter);

            if (FargoSoulsUtil.HostCheck)
            {
                float ai1 = time + Main.rand.Next(-10, 11) - 1;
                Projectile.NewProjectile(NPC.GetSource_FromThis(), ShootCenter, distance,
                    ModContent.ProjectileType<TrojanSquirrelProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer, gravity, ai1);
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                Vector2 pos = NPC.Center;
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_FromThis(), pos, NPC.velocity, ModContent.Find<ModGore>(Mod.Name, $"TrojanSquirrelGore1").Type, NPC.scale);
                    Gore.NewGore(NPC.GetSource_FromThis(), pos, NPC.velocity, ModContent.Find<ModGore>("Fargowiltas", $"TophatSquirrelGore").Type, NPC.scale);
                }               
            }
        }
    }
}
