using System;
using System.Collections.Generic;
using System.IO;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.BloodMoon
{
    public class HemogoblinShark : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.GoblinShark);

        public int AttackTimer;
        public int VanillaAttackCycles;
        public int State;
        public int Frame;
        public int Jumps;
        public bool Jumped;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write7BitEncodedInt(AttackTimer);
            binaryWriter.Write7BitEncodedInt(VanillaAttackCycles);
            binaryWriter.Write7BitEncodedInt(State);
            binaryWriter.Write7BitEncodedInt(Frame);
            binaryWriter.Write7BitEncodedInt(Jumps);
            binaryWriter.Write(Jumped);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            AttackTimer = binaryReader.Read7BitEncodedInt();
            VanillaAttackCycles = binaryReader.Read7BitEncodedInt();
            State = binaryReader.Read7BitEncodedInt();
            Frame = binaryReader.Read7BitEncodedInt();
            Jumps = binaryReader.Read7BitEncodedInt();
            Jumped = binaryReader.ReadBoolean();
        }

        public override bool SafePreAI(NPC npc)
        {
            Player target = Main.player[npc.target];
            DelegateMethods.v3_1 = new Vector3(0.8f, 0f, 0);
            Utils.PlotTileLine(npc.Center, npc.Center + npc.velocity, 10, DelegateMethods.CastLight);
            if (npc.wet)
            {
                State = 0;
                AttackTimer = 0;
                Jumps = 0;
                npc.position += npc.velocity; //faster
            }
            if (State != 0)
            {
                AttackTimer++;
                if (npc.HasPlayerTarget && State != 2 || (npc.HasPlayerTarget && AttackTimer > 90 && Math.Abs(npc.velocity.X) >= 20)) npc.direction = Math.Sign(target.Center.X - npc.Center.X);
            }
            if (State == 1)
            {
                npc.GravityMultiplier *= 5;
                if (AttackTimer >= 30 && (npc.velocity.Y != 0 || (AttackTimer % 8 == 0 && npc.velocity.Y == 0))) Frame++;
                if (Frame >= 8 && AttackTimer >= 30) Frame = 4;
                if (AttackTimer == 18 && FargoSoulsUtil.HostCheck) // roar
                {
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 8, 180);
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 8, 200);
                    SoundEngine.PlaySound(SoundID.DD2_DrakinShot with { Pitch = 0.5f }, npc.Center);
                }
                if (AttackTimer < 30)
                {
                    if (AttackTimer % 6 == 0) Frame++;
                    if (Frame >= 19) Frame = 19;
                }

                if (AttackTimer % 30 == 0 && npc.velocity.Y == 0 && npc.collideY) // hop
                {
                    npc.velocity.Y -= 15;
                    Jumped = true;
                }
                if (npc.velocity.Y == 0 && npc.collideY && Jumped) // landing
                {
                    Jumps++;
                    Jumped = false;
                    for (int j = -1; j <= 1; j += 2)
                    {
                        for (int i = 0; i <= 4; i++)
                        {
                            Vector2 vel = Main.rand.NextFloat(8f, 16f) * j * Vector2.UnitX.RotatedBy(MathHelper.PiOver4 / 2 * i * -j).RotatedByRandom(MathHelper.PiOver4);
                            Particle c = new RectangleParticle(npc.Bottom, vel, Color.DarkRed, 0.25f, 40, true);
                            c.Spawn();
                            c.Velocity.Y -= 5;
                        }
                    }
                    if (FargoSoulsUtil.HostCheck && npc.HasValidTarget)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, npc.Bottom);
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 targ = target.Center + Main.rand.NextVector2Circular(8, 8);
                            Vector2 spawnPos = FindSharpTearsSpot(Collision.CanHitLine(npc.Center, npc.width, npc.height, target.Center, target.width, target.height) ? npc.Center : target.Center, targ).ToWorldCoordinates(Main.rand.Next(17), Main.rand.Next(17));
                            Projectile.NewProjectile(npc.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<BloodThornWarning>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, (16f * Vector2.Normalize(targ - spawnPos)).X, (16f * Vector2.Normalize(targ - spawnPos)).Y);
                        }
                        if (Jumps == 1)
                        {
                            for (int i = -1; i < 2; i++)
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Bottom - new Vector2(i * 36, 2), Vector2.Zero, ModContent.ProjectileType<BloodTrail>(), 0, 0, Main.myPlayer, npc.whoAmI, ai1: 1);
                        }
                    }
                }
                if (Jumps >= 4)
                {
                    Jumps = State = AttackTimer = 0;
                    npc.ai[0] = 0;
                    npc.ai[1] = 150;
                }
                return false;
            }
            if (State == 2)
            {
                AttackTimer++;
                bool charging = AttackTimer >= 150;
                if (AttackTimer % 8 == 0) Frame++;
                if (Frame >= (AttackTimer >= 180 ? 14 : 8)) Frame = AttackTimer >= 180 ? 10 : 2;
                if (AttackTimer == 180) Frame = 10;

                if (AttackTimer >= 30 && !charging)
                {
                    npc.velocity.X = 1f * -npc.direction;
                    if (AttackTimer % 30 == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 vel = Main.rand.NextFloat(8f, 16f) * -npc.direction * Vector2.UnitX.RotatedBy(MathHelper.PiOver4 * npc.direction).RotatedByRandom(MathHelper.PiOver4);
                            Particle c = new RectangleParticle(npc.direction == 1 ? npc.BottomLeft : npc.BottomRight, vel, Color.DarkRed, 0.25f, 40, true);
                            c.Spawn();
                        }
                    }
                }
                if (charging)
                {
                    npc.TargetClosest(false);
                    npc.velocity.X += npc.direction * 0.5f;
                    if (Math.Abs(npc.velocity.X) > 25)
                        npc.velocity.X = 25 * npc.direction;

                    if (AttackTimer % 3 == 0 && npc.velocity.Y == 0 && Math.Abs(npc.velocity.X) >= 10)
                    {
                        if (AttackTimer % 30 == 0) SoundEngine.PlaySound(SoundID.Item22 with {Pitch = 0}, npc.Bottom);
                        Vector2 vel = 16f * -Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(30));
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Bottom, vel, ProjectileID.SharpTears, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, 0, 1f);

                        for (int i = 0; i < 2; i++)
                        {
                            int dir = Math.Sign(npc.velocity.X);
                            Vector2 evel = 3f * npc.velocity.X * Vector2.UnitX.RotatedBy(-MathHelper.PiOver4 * dir).RotatedByRandom(MathHelper.PiOver4);
                            Particle e = new RectangleParticle(dir == 1 ? npc.BottomRight : npc.BottomLeft, evel, Color.DarkRed, 0.25f, 40, true);
                            e.Spawn();
                        }
                    }
                    if (npc.direction != npc.oldDirection) Jumps++;

                    if (npc.collideX) //wall reposition? weird with steep inclines
                    {
                        npc.position.Y -= 16;
                        npc.velocity.X = npc.oldVelocity.X;
                    }
                }

                if (Jumps >= 3 || AttackTimer > 1200)
                {
                    npc.velocity.X *= 0.2f;
                    Jumps = State = AttackTimer = 0;
                    npc.ai[0] = 0;
                    npc.ai[1] = 150;
                }
                return false;
            }
            return base.SafePreAI(npc);
        }

        public override void AI(NPC npc)
        {
            npc.TargetClosest(); // fix weird thing where it just runs away for no reason?
            Player target = Main.player[npc.target];
            npc.GravityMultiplier *= 1;

            if (!Collision.CanHitLine(npc.Center, 0, 0, target.Center, 0, 0))
            { // force vanilla attack because dude is stuck in a wall and this game sucks
                if (AttackTimer++ >= 120 && npc.ai[0] == 0 && npc.velocity.Y == 0)
                {
                    npc.ai[0] = 1;
                    npc.ai[1] = 60;
                    AttackTimer = 0;
                }
            }
            else if (Math.Abs(target.Center.X - npc.Center.X) >= 16 * 30 && Math.Abs(target.Center.Y - npc.Center.Y) <= 16 * 20 && VanillaAttackCycles < 2)
            { // force charge if rungodding
                if (AttackTimer++ >= 240 && npc.ai[0] == 0)
                {
                    State = 2;
                    VanillaAttackCycles = 0;
                    AttackTimer = 0;
                    Frame = 2;
                }
            }
            else AttackTimer = 0;

            if (npc.ai[0] != 0 && npc.ai[1] == 60)
                VanillaAttackCycles++;

            if (VanillaAttackCycles >= 2 && npc.ai[0] == 0)
            {
                if (Main.rand.NextBool())
                {
                    State = 1; // blood stomp temper tantrum
                    VanillaAttackCycles = 0;
                    AttackTimer = 0;
                    npc.velocity = Vector2.Zero;
                    npc.frameCounter = 0;
                    Frame = 14;
                }
                else
                {
                    State = 2; // blood spike charge
                    VanillaAttackCycles = 0;
                    AttackTimer = 0;
                    npc.velocity = Vector2.Zero;
                    Frame = 2;
                }
            }
        }
        public override bool? CanFallThroughPlatforms(NPC npc)
        {
            if (State == 1) return false;
            return base.CanFallThroughPlatforms(npc);
        }

        public override void OnHitByAnything(NPC npc, Player player, NPC.HitInfo hit, int damageDone)
        {
            npc.justHit = false; // dont pause vanilla attacks on hit
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 600);
            if (State == 2)
            {
                State = 0;
                Jumps = 0;
                npc.velocity.X = -npc.velocity.X * 0.5f;
                npc.velocity.Y -= 4;
                if (FargoSoulsUtil.HostCheck)
                {
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 8, 180);
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 8, 200);
                }
            }
        }

        public override void FindFrame(NPC npc, int frameHeight)
        {
            float offset = (State == 2 && Frame >= 10) ? frameHeight / 5 : 0;
            if (State != 0)
                npc.frame.Y = (int)((Frame * frameHeight) - offset);
        }

        private static Point FindSharpTearsSpot(Vector2 origin, Vector2 targetSpot)
        {
            targetSpot.ToTileCoordinates();
            Vector2 center = origin;
            Vector2 endPoint = targetSpot;
            int samplesToTake = 3;
            float samplingWidth = 4f;
            Collision.AimingLaserScan(center, endPoint, samplingWidth, samplesToTake, out Vector2 vectorTowardsTarget, out float[] samples);
            float num = float.PositiveInfinity;
            for (int index = 0; index < samples.Length; ++index)
            {
                if ((double)samples[index] < (double)num)
                    num = samples[index];
            }
            targetSpot = center + vectorTowardsTarget.SafeNormalize(Vector2.Zero) * num;
            Point tileCoordinates = targetSpot.ToTileCoordinates();
            Microsoft.Xna.Framework.Rectangle rectangle1 = new(tileCoordinates.X, tileCoordinates.Y, 1, 1);
            rectangle1.Inflate(6, 16);
            Microsoft.Xna.Framework.Rectangle rectangle2 = new(0, 0, Main.maxTilesX, Main.maxTilesY);
            rectangle2.Inflate(-40, -40);
            rectangle1 = Rectangle.Intersect(rectangle1, rectangle2);
            List<Point> pointList1 = [];
            List<Point> pointList2 = [];
            for (int left = rectangle1.Left; left <= rectangle1.Right; ++left)
            {
                for (int top = rectangle1.Top; top <= rectangle1.Bottom; ++top)
                {
                    if (WorldGen.SolidTile2(left, top))
                    {
                        Vector2 vector2 = new((float)(left * 16 + 8), (float)(top * 16 + 8));
                        if ((double)Vector2.Distance(targetSpot, vector2) <= 200.0)
                        {
                            if (FindSharpTearsOpening(left, top, left > tileCoordinates.X, left < tileCoordinates.X, top > tileCoordinates.Y, top < tileCoordinates.Y))
                                pointList1.Add(new Point(left, top));
                            else pointList2.Add(new Point(left, top));
                        }
                    }
                }
            }
            if (pointList1.Count == 0 && pointList2.Count == 0)
                pointList1.Add((origin.ToTileCoordinates().ToVector2() + Main.rand.NextVector2Square(-2f, 2f)).ToPoint());
            List<Point> pointList3 = pointList1;
            if (pointList3.Count == 0)
                pointList3 = pointList2;
            int index1 = Main.rand.Next(pointList3.Count);
            return pointList3[index1];
        }

        private static bool FindSharpTearsOpening(int x, int y, bool acceptLeft, bool acceptRight, bool acceptUp, bool acceptDown)
        {
            return acceptLeft && !WorldGen.SolidTile(x - 1, y) || acceptRight && !WorldGen.SolidTile(x + 1, y) || acceptUp && !WorldGen.SolidTile(x, y - 1) || acceptDown && !WorldGen.SolidTile(x, y + 1);
        }
    }
}
