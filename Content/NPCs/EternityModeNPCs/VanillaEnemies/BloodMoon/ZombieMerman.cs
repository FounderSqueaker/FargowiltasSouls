using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.BloodMoon
{
    public class ZombieMerman : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.ZombieMerman);

        public int JumpTimer;
        public int AnchorSlamStartup;
        public int ShortHopAerialTimer;
        public bool Jumped;
        public bool AnchorSlam;
        public bool ShortHopping;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write7BitEncodedInt(JumpTimer);
            binaryWriter.Write7BitEncodedInt(AnchorSlamStartup);
            binaryWriter.Write7BitEncodedInt(ShortHopAerialTimer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            JumpTimer = binaryReader.Read7BitEncodedInt();
            AnchorSlamStartup = binaryReader.Read7BitEncodedInt();
            ShortHopAerialTimer = binaryReader.Read7BitEncodedInt();
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);
            if (FargoSoulsUtil.HostCheck)
            {
                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<MermanAnchor>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 2, Main.myPlayer, npc.whoAmI);
            }
            /*for (int i = 0; i < 9; i++)
            {
                FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, NPCID.Zombie, velocity: Main.rand.NextVector2Circular(8, 8));
            }*/
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);
            npc.waterMovementSpeed = 1;
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            const float gravity = 0.4f;
            DelegateMethods.v3_1 = new Vector3(0.8f, 0f, 0);
            Utils.PlotTileLine(npc.Center, npc.Center + npc.velocity, 10, DelegateMethods.CastLight);
            //Main.NewText($"{npc.ai[0]}, {npc.ai[1]}, {npc.ai[2]}, {npc.ai[3]}, ");

            if (npc.wet && npc.HasPlayerTarget) // water ai
            {
                npc.ai[0] = 3;
                Jumped = AnchorSlam = ShortHopping = false;
                JumpTimer = AnchorSlamStartup = ShortHopAerialTimer = 0;
            }
            else // regular ai
            {
                if (JumpTimer > 120) //initiate jump
                {
                    JumpTimer = 0;
                    Jumped = true;

                    int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                    if (t != -1 && FargoSoulsUtil.HostCheck)
                    {
                        const float time = 60;
                        Vector2 distance;
                        if (Main.player[t].Alive()) distance = (Main.player[t].Center + new Vector2(16 * 3 * Math.Sign(Main.player[t].Center.X - npc.Center.X), -16 * 6)) - npc.Bottom;
                        else distance = new Vector2(npc.Center.X < Main.player[t].Center.X ? -300 : 300, -100);
                        distance.X /= time;
                        distance.Y = distance.Y / time - 0.5f * gravity * time;
                        npc.ai[1] = time;
                        npc.ai[2] = distance.X;
                        npc.ai[3] = distance.Y;
                        npc.netUpdate = true;
                    }

                    return false;
                }

                if (npc.ai[1] > 0f) //while jumping
                {
                    npc.ai[1]--;
                    npc.noTileCollide = true;
                    if (!AnchorSlam)
                    {
                        npc.velocity.X = npc.ai[2] * 2;
                        npc.velocity.Y = npc.ai[3];
                        npc.ai[3] += gravity;
                        for (int i = 0; i < 2; ++i)
                        {
                            Vector2 vector2_2 = ((float)(Main.rand.NextDouble() * Math.PI) - (float)Math.PI / 2).ToRotationVector2() * Main.rand.Next(3, 8);
                            int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.BloodWater, vector2_2.X * 2f, vector2_2.Y * 2f, 100, Scale: 1.4f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].velocity /= 4f;
                            Main.dust[d].velocity -= npc.velocity;
                        }
                    }

                    int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                    if (npc.Center.X > Main.player[t].Center.X - 16 && npc.Center.X < Main.player[t].Center.X + 16 && !AnchorSlam && Collision.CanHitLine(npc.Center, 0, 0, Main.player[t].Center, 0, 0))
                    {
                        AnchorSlam = true;
                        AnchorSlamStartup = 10;
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastHurt with { Pitch = 0.5f }, npc.Center);
                    }
                    if (AnchorSlam)
                    {
                        if (AnchorSlamStartup > 0)
                        {
                            npc.ai[0] = 1;
                            npc.ai[1]++;
                            if (AnchorSlamStartup > 9)
                                npc.velocity.Y *= 0.01f;
                            npc.velocity.X *= 0.01f;
                            npc.velocity.Y -= 1.5f;
                            AnchorSlamStartup--;
                        }
                        else //initiate 
                        {
                            npc.noTileCollide = false;
                            if (npc.velocity.Y == 0)
                                npc.ai[1] = 0;
                            else
                            {
                                if (npc.velocity.Y < 20)
                                    npc.velocity.Y += 0.75f;
                                else
                                    npc.velocity.Y = 20;
                                npc.noGravity = true;
                                npc.MaxFallSpeedMultiplier *= 1f;
                                npc.ai[1]++;
                            }
                                
                        }
                    }
                    JumpTimer = 0;
                    JumpTimer++;
                    return false;
                }
                else
                {
                    if (ShortHopAerialTimer > 0)
                    {
                        if (ShortHopAerialTimer >= 30)
                        {
                            npc.velocity.Y += -6;
                            npc.velocity.X += 6 * npc.spriteDirection;
                            ShortHopping = true;
                            ShortHopAerialTimer = 0;
                            npc.ai[0] = 2;
                        }
                    }
                    if (npc.noTileCollide)
                    {
                        npc.direction = Math.Sign(npc.velocity.X);
                        JumpTimer = 0;
                        npc.noTileCollide = Collision.SolidCollision(npc.position, npc.width, npc.height);
                        return false;
                    }
                }

                if (npc.HasValidTarget && npc.velocity.Y == 0)
                {
                    JumpTimer++;
                    int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                    if ((npc.Distance(Main.player[t].Center) <= 16 * 12 && npc.Distance(Main.player[t].Center) > 16 * 3))
                        ShortHopAerialTimer++;
                    else ShortHopAerialTimer = 0;
                }

                if (npc.velocity.Y == 0f)
                {
                    ShortHopping = false;
                    if (Jumped)
                    {
                        if (AnchorSlam && FargoSoulsUtil.HostCheck)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit42 with { Pitch = -0.3f }, npc.Center);
                            for (int j = -1; j <= 1; j += 2)
                            {
                                for (int i = 0; i <= 3; i++)
                                {
                                    Vector2 vel = 16f * j * Vector2.UnitX.RotatedBy(MathHelper.PiOver4 / 3 * i * -j);
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, vel, ProjectileID.SharpTears, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, 0f, Main.rand.NextFloat(0.5f, 1f));
                                }
                            }
                        }
                        Jumped = false;
                        AnchorSlam = false;
                    }
                    npc.ai[0] = 0;
                    npc.ai[1] = 0;
                    npc.MaxFallSpeedMultiplier *= 1;
                }
            }
            return result;
        }
        public override bool? CanFallThroughPlatforms(NPC npc)
        {
            if (AnchorSlam && npc.HasPlayerTarget && Main.player[npc.target] is Player player && (player.Top.Y > npc.Bottom.Y + 30))
                return true;
            return base.CanFallThroughPlatforms(npc);
        }
        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);
            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 600);
        }
    }
}
