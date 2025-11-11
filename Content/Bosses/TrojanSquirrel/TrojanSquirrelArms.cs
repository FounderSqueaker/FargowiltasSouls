using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.TrojanSquirrel
{
    public class TrojanSquirrelArms : TrojanSquirrelLimb
    {
        public LoopedSoundInstance? Loop;
        int looptimer;
        public int ArmsAnimationType;
        public int ArmsAnimationAngle = 1;
        public int AltArmAnimationType = -1;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 450;
            NPC.DeathSound = FargosSoundRegistry.TrojanCannonDeath;
            NPC.width = baseWidth = 114;
            NPC.height = baseHeight = 64;
        }

        public override void AI()
        {
            //return;
            base.AI();

            if (body == null)
                return;

            NPC.velocity = Vector2.Zero;
            NPC.target = body.target;
            NPC.direction = NPC.spriteDirection = body.direction;
            NPC.Center = body.Bottom + new Vector2(18f * NPC.direction, -105f) * body.scale;
            
            if (NPC.ai[0] != 1 && Loop?.HasLoopSoundBeenStarted == true)
            {
                looptimer = 0;
                Loop?.Stop();
            }

            AltArmAnimationType = -1;

            ArmsAnimationAngle = (int)MathHelper.Clamp(ArmsAnimationAngle, -2, 2);

            switch ((int)NPC.ai[0])
            {
                case 0:
                    if (body.ai[0] == 0 && body.localAI[0] <= 0)
                    {
                        ArmsAnimationType = 0;
                        ArmsAnimationAngle = 1;

                        NPC.ai[1] += WorldSavingSystem.EternityMode ? 1.5f : 1f;

                        if (body.dontTakeDamage)
                            NPC.ai[1] += 1f;

                        int threshold = 360;

                        //structured like this so body gets priority first
                        int stallPoint = threshold - 30;
                        if (NPC.ai[1] > stallPoint)
                        {
                            TrojanSquirrel squirrel = body.As<TrojanSquirrel>();
                            if (squirrel.head != null && squirrel.head.ai[0] != 0f) //wait if other part is attacking
                                NPC.ai[1] = stallPoint;
                        }

                        if (NPC.ai[1] > threshold && Math.Abs(body.velocity.Y) < 0.05f)
                        {
                            //dont attack unless player is in 90 degree cone in front of squrrl
                            float baseAngle = NPC.direction > 0 ? 0f : MathHelper.Pi;
                            if (Math.Abs(MathHelper.WrapAngle(NPC.SafeDirectionTo(Main.player[NPC.target].Center).ToRotation() - baseAngle)) > MathHelper.PiOver4)
                            {
                                NPC.ai[1] = stallPoint;
                            }
                            else
                            {
                                NPC.ai[0] = 1 + NPC.ai[2];
                                NPC.ai[1] = 0;
                                if (Main.expertMode)
                                    NPC.ai[2] = NPC.ai[2] == 0 ? 1 : 0;
                                NPC.netUpdate = true;

                                body.localAI[3] = Math.Sign(body.SafeDirectionTo(Main.player[body.target].Center).X);
                                body.netUpdate = true;
                            }
                        }
                    }
                    break;

                case 1: //chains
                    {
                        if (++looptimer >= 90)
                        {   
                            
                            Loop ??= LoopedSoundManager.CreateNew(FargosSoundRegistry.TrojanHookLoop with { Volume = 0.2f }, () =>
                            {
                                return NPC == null || !NPC.active || NPC.ai[0] != 1;
                            });

                            Loop?.Update(NPC.Center);

                            if (Loop?.HasBeenStopped == true && Loop?.HasLoopSoundBeenStarted == true)
                            {
                                Loop?.Restart();
                            }
                        }

                        int start = 90;
                        if (WorldSavingSystem.EternityMode)
                            start -= 30;
                        if (WorldSavingSystem.MasochistModeReal)
                            start -= 30;
                        int end = 300;

                        int teabagInterval = start / (WorldSavingSystem.MasochistModeReal ? 3 : 2);

                        if (NPC.ai[1] < start) //better for animation
                        {
                            body.velocity.X *= 0.9f;
                            if (NPC.ai[1] <= 1)
                            {
                                SoundEngine.PlaySound(FargosSoundRegistry.TrojanHookTelegraph, NPC.Center);

                                // start shot prep animation
                            }
                                
                        }

                        NPC.ai[1]++;

                        Vector2 direction = GetNextShootPos().DirectionTo(Main.player[NPC.target].Center);
                        float offset = FargoSoulsUtil.RotationDifference(Math.Abs(direction.X) * Vector2.UnitX + direction.Y * Vector2.UnitY, Vector2.UnitX);
                        int aimAngle = (int)(-offset / (MathHelper.PiOver4 * 0.25f));
                        if (NPC.ai[1] <= start) // prep animation
                        {
                            float startAnimTime = start / 2;
                            if (NPC.ai[1] < startAnimTime) // starting animation
                            {
                                ArmsAnimationType = 1;
                                // this is here the Y-frame-variable straight up
                                ArmsAnimationAngle = (int)MathHelper.Lerp(0, 5.9f, NPC.ai[1] / startAnimTime);
                            }
                            else // aim
                            {
                                ArmsAnimationType = 2;
                                float lerper = (NPC.ai[1] - startAnimTime) / (start - startAnimTime);
                                lerper = Math.Clamp(lerper + 0.2f, 0, 1);
                                ArmsAnimationAngle = (int)MathHelper.Lerp(1, aimAngle, lerper);
                            }
                        }
                        else
                        {
                            ArmsAnimationType = 2;
                            if (NPC.ai[1] > end - 15 || Main.projectile.Any(p => p.TypeAlive<TrojanHook>() && p.ai[2] == -1))
                                ArmsAnimationType = 0;
                            AltArmAnimationType = 2;
                            if (NPC.ai[1] > end - 15 || Main.projectile.Any(p => p.TypeAlive<TrojanHook>() && p.ai[2] == 1))
                                AltArmAnimationType = 0;

                            //ArmsAnimationAngle = aimAngle;
                        }

                        //to help animate body
                        NPC.ai[3] = NPC.ai[1] < start && NPC.ai[1] % teabagInterval < teabagInterval / 2 ? 1 : 0;

                        int freq = body.dontTakeDamage || WorldSavingSystem.MasochistModeReal ? 40 : 70;
                        if (NPC.ai[1] > start && NPC.ai[1] < end && NPC.ai[1] % freq == 0)
                        {
                            if (NPC.ai[1] > freq && NPC.direction * NPC.HorizontalDirectionTo(Main.player[NPC.target].Center) < 0) // end early if facing the wrong direction after 2 hooks
                            {
                                NPC.ai[1] = end + 10;
                            }
                            else
                            {
                                Vector2 pos = GetShootPos();

                                float baseAngle = NPC.direction > 0 ? 0f : MathHelper.Pi;
                                float angle = NPC.SafeDirectionTo(Main.player[NPC.target].Center).ToRotation();
                                if (Math.Abs(MathHelper.WrapAngle(angle - baseAngle)) > MathHelper.PiOver2)
                                    angle = MathHelper.PiOver2 * Math.Sign(angle);

                                ArmsAnimationAngle = aimAngle;

                                if (FargoSoulsUtil.HostCheck)
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, angle.ToRotationVector2() * 8f, ModContent.ProjectileType<TrojanHook>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer, ai2: NPC.localAI[0] == 1 ? 1 : -1);
                            }
                                
                        }


                        if (NPC.ai[1] > end && FargoSoulsUtil.HostCheck && Main.LocalPlayer.ownedProjectileCounts[ModContent.ProjectileType<TrojanHook>()] <= 0)
                        {
                            ArmsAnimationType = 0;
                            ArmsAnimationAngle = 1;
                            NPC.ai[0] = 0;
                            NPC.ai[1] = 0;
                            NPC.netUpdate = true;

                            body.localAI[3] = 0;
                            body.netUpdate = true;
                        }
                    }
                    break;

                case 2: //snowballs
                    {
                        NPC.ai[1]++;

                        int start = 100;
                        int end = 370;
                        if (WorldSavingSystem.EternityMode)
                        {
                            start -= 30;
                            end -= 30;
                        }
                        if (WorldSavingSystem.MasochistModeReal)
                            end -= 60;

                        body.velocity.X *= 0.98f;

                        if (NPC.ai[1] == 10)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                Vector2 pos = GetShootPos();
                                SoundEngine.PlaySound(FargosSoundRegistry.TrojanGunStartup, pos);
                                /*
                                for (int j = 0; j < 20; j++)
                                {
                                    int d = Dust.NewDust(pos, 0, 0, DustID.SnowBlock, Scale: 3f);
                                    Main.dust[d].noGravity = true;
                                    Main.dust[d].velocity *= 4f;
                                    Main.dust[d].velocity.X += NPC.direction * Main.rand.NextFloat(6f, 24f);
                                }
                                */
                            }
                        }

                        void SnowDust()
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                int radius = 12;
                                Vector2 vel = Vector2.UnitX.RotatedBy(ArmsAnimationAngle * MathHelper.PiOver4 * 0.7f);
                                vel.X *= NPC.direction;
                                vel = vel.RotatedByRandom(MathHelper.PiOver4 * 0.15f);
                                int d = Dust.NewDust(GetNextShootPos(Main.rand.NextBool() ? 1 : -1) - Vector2.One * radius, radius * 2, radius * 2, DustID.Snow, Scale: 1f);
                                Main.dust[d].noGravity = true;
                                Main.dust[d].velocity = vel * Main.rand.NextFloat(3f, 8f);
                            }
                        }

                        // animation
                        ArmsAnimationType = 0;
                        if (NPC.ai[1] <= start)
                        {
                            ArmsAnimationAngle = (int)MathHelper.Lerp(1, -2.9f, NPC.ai[1] / start);
                            SnowDust();
                            
                        }




                        if (NPC.ai[1] > start && NPC.ai[1] % 4 == 0)
                        {
                            SoundEngine.PlaySound(FargosSoundRegistry.Minigun, GetShootPos());
                            if (NPC.ai[1] % 8 == 0)
                            {
                                Vector2 pos = GetShootPos();

                                SoundEngine.PlaySound(FargosSoundRegistry.TrojanSnowball, pos);

                                float ratio = (NPC.ai[1] - start) / (end - start);

                                Vector2 target = NPC.Center;
                                target.X += Math.Sign(NPC.direction) * (WorldSavingSystem.EternityMode ? 1800f : 1200f) * ratio; //gradually targets further and further
                                                                                                                                 //target.Y -= 8 * 16;
                                target += Main.rand.NextVector2Circular(16, 16);
                                const float gravity = 0.5f;
                                float time = 45f;
                                Vector2 distance = target - pos;
                                distance.X /= time;
                                distance.Y = distance.Y / time - 0.5f * gravity * time;

                                float offset = FargoSoulsUtil.RotationDifference(Math.Abs(distance.X) * Vector2.UnitX + distance.Y * Vector2.UnitY, Vector2.UnitX);
                                ArmsAnimationAngle = (int)(-offset / (MathHelper.PiOver4 * 0.7f));

                                if (FargoSoulsUtil.HostCheck)
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, distance, ModContent.ProjectileType<TrojanSnowball>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer, gravity);

                                SnowDust();
                            }
                            NPC.ai[1] += NPC.ai[1] > end / 3 ? NPC.ai[1] > end * (2 / 3) ? 3 : 1 : 0;
                        }

                        if (NPC.ai[1] > end)
                        {
                            ArmsAnimationAngle = 1;
                            NPC.ai[0] = 0;
                            NPC.ai[1] = 0;
                            NPC.netUpdate = true;

                            body.localAI[3] = 0;
                            body.netUpdate = true;
                        }
                    }
                    break;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame = GetFrame(ArmsAnimationType, frameHeight);
        }

        public Rectangle GetFrame(int animationType, int frameHeight)
        {
            var rectangle = NPC.frame;
            //74 is the width of each X Frame.
            switch (animationType)
            {
                case 1: // harpoon shot
                    rectangle.X = 74;
                    rectangle.Y = frameHeight * ArmsAnimationAngle;
                    break;
                case 2: // harpoon out
                    rectangle.X = 74 * 2;
                    rectangle.Y = frameHeight * AngleToFrame(ArmsAnimationAngle);
                    break;
                default: // normal
                    rectangle.X = 0;
                    rectangle.Y = frameHeight * AngleToFrame(ArmsAnimationAngle);
                    break;
            }


            if (rectangle.Y >= frameHeight * Main.npcFrameCount[Type])
                rectangle.Y = 0;
            return rectangle;
        }
        public int AngleToFrame(int angle)
        {
            return angle switch
            {
                
                1 => 0, // 1 down, default
                2 => 4, // 2 down
                0 => 1, // forward
                -1 => 2, // 1 up
                -2 => 3, // 2 up
                _ => 0, // default is 1 down
            };
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture2D13 = TextureAssets.Npc[NPC.type].Value;
            Rectangle rectangle = new Rectangle(NPC.frame.X, NPC.frame.Y, NPC.frame.Width / 3, NPC.frame.Height);
            Vector2 origin2 = rectangle.Size() / 2f;
            //NPC.direction = 1;


            Color color26 = drawColor;
            color26 = NPC.GetAlpha(color26);

            SpriteEffects effects = NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (body != null)
            {
                var trojan = body.As<TrojanSquirrel>();

                Main.EntitySpriteDraw(texture2D13, body.Center - screenPos + new Vector2(NPC.direction < 0 ? 20f : -2f, NPC.gfxOffY - 24 * NPC.scale) + trojan.bodyOffset, new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, NPC.rotation, origin2, NPC.scale, effects, 0);
            }

            return false;
        }

        public Vector2 GetShootPos()
        {
            Vector2 pos = GetNextShootPos();
            NPC.localAI[0] = NPC.localAI[0] == 0 ? 1 : 0;

            return pos;
        }

        public Vector2 GetNextShootPos(int arm = 0)
        {
            bool altArm = arm == 1 || (arm == 0 && NPC.localAI[0] != 0);
            Vector2 pos = NPC.Bottom;
            int angle = ArmsAnimationAngle;
            if (ArmsAnimationType == 1)
                angle = 0;
            switch (ArmsAnimationAngle)
            {
                case -2: // frame 4
                    pos.X += NPC.width / 2f * NPC.direction;
                    pos.Y -= 44 * NPC.scale;

                    pos.X -= (NPC.direction == -1 ? (altArm ? 36 : 86) : (altArm ? 20 : 64)) * NPC.direction * NPC.scale;
                    break;
                case -1: // frame 3
                    pos.X += NPC.width / 2f * NPC.direction;
                    pos.Y -= 36 * NPC.scale;

                    pos.X -= (NPC.direction == -1 ? (altArm ? 34 : 82) : (altArm ? 12 : 58)) * NPC.direction * NPC.scale;
                    break;
                case 0: // frame 2
                    pos.X += NPC.width / 2f * NPC.direction;
                    pos.Y -= 24 * NPC.scale;

                    pos.X -= (NPC.direction == -1 ? (altArm ? 34 : 82) : (altArm ? 12 : 58)) * NPC.direction * NPC.scale;
                    break;
                case 1: // default; frame 1
                    pos.X += NPC.width / 2f * NPC.direction;
                    pos.Y -= 16 * NPC.scale;

                    pos.X -= (NPC.direction == -1 ? (altArm ? 34 : 82) : (altArm ? 12 : 66)) * NPC.direction * NPC.scale;
                    break;
                case 2: // frame 5
                    pos.X += NPC.width / 2f * NPC.direction;
                    pos.Y -= 2 * NPC.scale;

                    pos.X -= (NPC.direction == -1 ? (altArm ? 34 : 82) : (altArm ? 16 : 58)) * NPC.direction * NPC.scale;
                    break;
            }
               

            return pos;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                Loop?.Stop();
                for (int i = 8; i <= 9; i++)
                {
                    Vector2 pos = Main.rand.NextVector2FromRectangle(NPC.Hitbox);
                    if (!Main.dedServ)
                        Gore.NewGore(NPC.GetSource_FromThis(), pos, NPC.velocity, ModContent.Find<ModGore>(Mod.Name, $"TrojanSquirrelGore{i}").Type, NPC.scale);
                }
            }
        }
    }
}
