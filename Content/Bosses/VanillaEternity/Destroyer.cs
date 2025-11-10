using Fargowiltas;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.MechanicalBosses;
using FargowiltasSouls.Content.Projectiles.Weapons.ChallengerItems;
using FargowiltasSouls.Content.Projectiles.Weapons.FinalUpgrades;
using FargowiltasSouls.Core;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class Destroyer : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.TheDestroyer);

        public static readonly SoundStyle ScanSound = FargosSoundRegistry.DestroyerScan with { Volume = 5 };

        public int AttackModeTimer;
        public int CoilRadius;
        public int LaserTimer;
        public int SecondaryAttackTimer;
        public int RotationDirection = 1;
        public int LightshowSlowTimer;

        public static bool Phase2HP(NPC npc) => npc.life < (int)(npc.lifeMax * (WorldSavingSystem.MasochistModeReal ? 0.95 : .75));
        public bool InPhase2;
        public bool IsCoiling;
        public bool PrepareToCoil;

        public bool DroppedSummon;

        public const int P2_ATTACK_SPACING = 480;
        public const int P2_COIL_BEGIN_TIME = P2_ATTACK_SPACING * 4;

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (Main.getGoodWorld)
                cooldownSlot = ImmunityCooldownID.Bosses;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }


        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(AttackModeTimer);
            binaryWriter.Write(CoilRadius);
            binaryWriter.Write(LaserTimer);
            binaryWriter.Write(SecondaryAttackTimer);
            binaryWriter.Write(RotationDirection);
            binaryWriter.Write(LightshowSlowTimer);
            bitWriter.WriteBit(InPhase2);
            bitWriter.WriteBit(IsCoiling);
            bitWriter.WriteBit(PrepareToCoil);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            AttackModeTimer = binaryReader.ReadInt32();
            CoilRadius = binaryReader.ReadInt32();
            LaserTimer = binaryReader.ReadInt32();
            SecondaryAttackTimer = binaryReader.ReadInt32();
            RotationDirection = binaryReader.ReadInt32();
            LightshowSlowTimer = binaryReader.ReadInt32();
            InPhase2 = bitReader.ReadBit();
            IsCoiling = bitReader.ReadBit();
            PrepareToCoil = bitReader.ReadBit();
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Suffocation] = true;
            npc.buffImmune[BuffID.Chilled] = false;
            npc.buffImmune[ModContent.BuffType<TimeFrozenBuff>()] = false;
        }
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            modifiers.SourceDamage /= 3; // fuck you
        }
        private static int ProjectileDamage(NPC npc) => FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 4f / 10);

        private void CoilAI(NPC npc)
        {
            npc.buffImmune[ModContent.BuffType<TimeFrozenBuff>()] = true;

            npc.netUpdate = true;
            npc.velocity = Vector2.Normalize(npc.velocity) * 20f;
            npc.velocity += npc.velocity.RotatedBy(MathHelper.PiOver2 * RotationDirection) * npc.velocity.Length() / CoilRadius;
            npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;

            if (AttackModeTimer == 0)
                LaserTimer = 0;

            if (npc.life < npc.lifeMax / 10) //permanent coil phase 3
            {
                if (npc.localAI[2] >= 0)
                {
                    npc.localAI[2] = WorldSavingSystem.MasochistModeReal ? -60 : 0;
                    AttackModeTimer = 0; //for edge case where destroyer coils, then goes below 10% while coiling, make sure DR behaves right
                }

                if (--npc.localAI[2] < -120)
                {
                    npc.localAI[2] += WorldSavingSystem.MasochistModeReal ? 3 : 6;
                    if (FargoSoulsUtil.HostCheck)
                    {
                        Vector2 distance = npc.SafeDirectionTo(Main.player[npc.target].Center) * 14f;
                        int type = ModContent.ProjectileType<MechElectricOrbHoming>();
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, distance, type, ProjectileDamage(npc), 0f, Main.myPlayer, npc.target, ai2: MechElectricOrb.Blue);
                    }
                }

                if (!npc.HasValidTarget || npc.Distance(Main.player[npc.target].Center) > 3000)
                {
                    npc.TargetClosest(false);
                    if (!npc.HasValidTarget || npc.Distance(Main.player[npc.target].Center) > 3000)
                    {
                        AttackModeTimer = 0;
                        CoilRadius = 0;
                        IsCoiling = false;
                        PrepareToCoil = false;

                        NetSync(npc);
                    }
                }

                AttackModeTimer++;
            }
            else
            {
                if (--npc.localAI[2] < 0) //shoot star spreads into the circle
                {
                    npc.localAI[2] = Main.player[npc.target].HasBuff(ModContent.BuffType<LightningRodBuff>()) ? 110 : 65;
                    if (FargoSoulsUtil.HostCheck)
                    {
                        Vector2 distance = Main.player[npc.target].Center - npc.Center;
                        double angleModifier = MathHelper.ToRadians(30);
                        distance.Normalize();
                        distance *= 14f;
                        int type = ModContent.ProjectileType<MechElectricOrbHoming>();
                        int delay = -5;
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, distance.RotatedBy(-angleModifier), type, ProjectileDamage(npc), 0f, Main.myPlayer, npc.target, delay, ai2: MechElectricOrb.Blue);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, distance, type, ProjectileDamage(npc), 0f, Main.myPlayer, npc.target, delay, ai2: MechElectricOrb.Blue);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, distance.RotatedBy(angleModifier), type, ProjectileDamage(npc), 0f, Main.myPlayer, npc.target, delay, ai2: MechElectricOrb.Blue);
                    }
                }

                if (++AttackModeTimer > 300) //go back to normal AI
                {
                    AttackModeTimer = 0;
                    CoilRadius = 0;
                    IsCoiling = false;
                    PrepareToCoil = false;

                    NetSync(npc);
                }
            }

            Vector2 pivot = npc.Center;
            pivot += Vector2.Normalize(npc.velocity.RotatedBy(MathHelper.PiOver2 * RotationDirection)) * 600;

            if (++LaserTimer > 95)
            {
                LaserTimer = 0;
                if (FargoSoulsUtil.HostCheck)
                {
                    float ratio = (float)npc.life / npc.lifeMax;
                    if (WorldSavingSystem.MasochistModeReal)
                        ratio = 0;
                    float max = 12f;
                    float min = 4f;
                    if (WorldSavingSystem.MasochistModeReal)
                        max = 14f;
                    max = MathHelper.Lerp(max, min, ratio);
                    max = (int)max;
                    if (max % 2 != 0) //always shoot even number
                        max++;
                    if (max <= 4) // minimum of 6
                        max = 6;

                    for (int i = 0; i < max; i++)
                    {
                        Vector2 speed = npc.SafeDirectionTo(pivot).RotatedBy(2 * Math.PI / max * i);
                        Vector2 spawnPos = pivot - speed * 600;
                        Projectile.NewProjectile(npc.GetSource_FromThis(), spawnPos, 0.2f * speed, ModContent.ProjectileType<DestroyerLaser>(), ProjectileDamage(npc), 0f, Main.myPlayer, 1f, ai1: NPCID.TheDestroyer);
                    }
                }
            }

            for (int i = 0; i < 20; i++) //arena dust
            {
                Vector2 offset = new();
                double angle = Main.rand.NextDouble() * 2d * Math.PI;
                offset.X += (float)(Math.Sin(angle) * 600);
                offset.Y += (float)(Math.Cos(angle) * 600);
                Dust dust = Main.dust[Dust.NewDust(pivot + offset - new Vector2(4, 4), 0, 0, DustID.Clentaminator_Blue, 0, 0, 100, Color.White, 1f)];
                dust.velocity = Vector2.Zero;
                if (Main.rand.NextBool(3))
                    dust.velocity += Vector2.Normalize(offset) * 5f;
                dust.noGravity = true;
            }

            Player target = Main.player[npc.target];
            if (target.active && !target.dead) //arena effect
            {
                float distance = target.Distance(pivot);
                if (distance > 600 && distance < 3000)
                {
                    Vector2 movement = pivot - target.Center;
                    float difference = movement.Length() - 600;
                    movement.Normalize();
                    movement *= difference < 34f ? difference : 34f;
                    target.position += movement;

                    for (int i = 0; i < 20; i++)
                    {
                        int d = Dust.NewDust(target.position, target.width, target.height, DustID.Clentaminator_Blue, 0f, 0f, 0, default, 2f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity *= 5f;
                    }
                }
            }
        }

        private void NonCoilAI(NPC npc)
        {
            npc.buffImmune[ModContent.BuffType<TimeFrozenBuff>()] = false;

            npc.localAI[2] = 0;

            float maxSpeed = 16f;    //max speed?
            float num15 = 0.1f;   //turn speed?
            float num16 = 0.15f;   //acceleration?

            bool fastStart = AttackModeTimer < 120;

            float flySpeedModifierRatio = (float)npc.life / npc.lifeMax;
            if (flySpeedModifierRatio > 0.5f) //prevent it from subtracting speed
                flySpeedModifierRatio = 0.5f;
            if (fastStart) //if just entered this stage, max out ratio
                flySpeedModifierRatio = 0;

            if (npc.HasValidTarget)
            {
                if (!fastStart) //after fast start to uncoil
                {
                    float distance = npc.Distance(Main.player[npc.target].Center);
                    if (distance < 600) //slower nearby
                    {
                        maxSpeed *= 0.5f;
                    }
                    else if (distance > 900) //come at you really hard when out of range
                    {
                        num15 *= 2f;
                        num16 *= 2f;
                    }
                }

                float comparisonSpeed = Main.player[npc.target].velocity.Length() * 1.5f;
                float rotationDifference = MathHelper.WrapAngle(npc.velocity.ToRotation() - npc.SafeDirectionTo(Main.player[npc.target].Center).ToRotation());
                bool inFrontOfMe = Math.Abs(rotationDifference) < MathHelper.ToRadians(90 / 2);
                if (maxSpeed < comparisonSpeed && inFrontOfMe) //player is moving faster than my top speed
                {
                    maxSpeed = comparisonSpeed; //outspeed them
                }
            }

            Vector2 target = Main.player[npc.target].Center;
            if (PrepareToCoil) //move MUCH faster, approach a position nearby
            {
                num15 = 0.4f;
                num16 = 0.5f;

                target += Main.player[npc.target].SafeDirectionTo(npc.Center) * 600;

                if (++AttackModeTimer > 120) //move way faster if still not in range
                    maxSpeed *= 2f;

                if (npc.Distance(target) < 50)
                {
                    AttackModeTimer = 0;
                    CoilRadius = (int)npc.Distance(Main.player[npc.target].Center);
                    IsCoiling = true;

                    //angle difference from npc velocity, to angle towards player
                    float rotationDiff = MathHelper.WrapAngle(npc.SafeDirectionTo(Main.player[npc.target].Center).ToRotation() - npc.velocity.ToRotation());
                    RotationDirection = Math.Sign(rotationDiff);

                    npc.velocity = 20 * npc.SafeDirectionTo(Main.player[npc.target].Center).RotatedBy(-MathHelper.PiOver2 * RotationDirection);

                    npc.netUpdate = true;
                    NetSync(npc);

                    //SoundEngine.PlaySound(SoundID.Roar, Main.player[npc.target].Center);
                    SoundEngine.PlaySound(ScanSound with { Pitch = -0.5f, Volume = 2f }, Main.player[npc.target].Center);
                    if (npc.life < npc.lifeMax / 10)
                        SoundEngine.PlaySound(SoundID.ForceRoarPitched, Main.player[npc.target].Center); //eoc roar

                    if (FargoSoulsUtil.HostCheck)
                    {
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && (
                                Main.projectile[i].type == ModContent.ProjectileType<MechElectricOrbHoming>() ||
                                Main.projectile[i].type == ModContent.ProjectileType<MechElectricOrbDestroyer>() ||
                                Main.projectile[i].type == ModContent.ProjectileType<DestroyerLaser>() ||
                                Main.projectile[i].type == ProjectileID.DeathLaser))
                            {
                                Main.projectile[i].Kill();
                            }
                        }
                    }
                }
            }
            else
            {
                if (npc.life < npc.lifeMax / 10)
                {
                    if (AttackModeTimer < P2_COIL_BEGIN_TIME) //force begin desperation
                    {
                        AttackModeTimer = P2_COIL_BEGIN_TIME;
                        NetSync(npc);
                        SoundEngine.PlaySound(SoundID.ForceRoarPitched, Main.player[npc.target].Center); //eoc roar
                    }
                }
                else
                {
                    NonCoilAttacksAI(npc, ref num15, ref num16, ref target, ref maxSpeed, ref flySpeedModifierRatio);
                }

                if (++AttackModeTimer > P2_COIL_BEGIN_TIME) //change state
                {
                    AttackModeTimer = 0;
                    PrepareToCoil = true;

                    npc.netUpdate = true;
                    NetSync(npc);
                }
                else if (AttackModeTimer == P2_COIL_BEGIN_TIME - 120) //telegraph with roar
                {
                    SoundEngine.PlaySound(ScanSound with { Pitch = 0.5f, Volume = 2f }, Main.player[npc.target].Center);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 6, npc.whoAmI);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 6, npc.whoAmI);
                    }
                }
            }
            if (LightshowSlowTimer > 0)
            {
                if (maxSpeed > 4)
                maxSpeed = 4;
                LightshowSlowTimer--;
            }
                

            MovementAI(npc, target, num15, num16, maxSpeed);

            npc.position += npc.velocity * (.5f - flySpeedModifierRatio);
        }

        private void SpawnProbes(NPC npc, int probeCount)
        {
            List<NPC> segments = Main.npc.Where(n => n.active && n.realLife == npc.whoAmI && n.type == NPCID.TheDestroyerBody).ToList();
            Player player = Main.player[npc.target];

            // take 50 closest segments to target as applicable spawn locations
            segments = segments.OrderBy(n => n.DistanceSQ(player.Center)).ToList();
            segments = segments.Take(50).ToList();

            // randomize list and spawn some probes from the first few randomly selected segments
            segments = segments.OrderBy(_ => Main.rand.NextFloat()).ToList();
            for (int i = 0; i < probeCount; i++)
            {
                if (i >= segments.Count - 1)
                    break;
                NPC segment = segments[i];

                segment.HitEffect();
                SoundEngine.PlaySound(SoundID.Item14, segment.Center);
                segment.GetGlobalNPC<DestroyerSegment>().DisabledTime = 60 * 6;
                segment.netUpdate = true;
                if (FargoSoulsUtil.HostCheck)
                {
                    FargoSoulsUtil.NewNPCEasy(segment.GetSource_FromAI(), segment.Center, NPCID.Probe);
                }

                for (int j = 0; j < 11; j++)
                {
                    SparkParticle p = new SparkParticle(Main.rand.NextVector2FromRectangle(segment.Hitbox), Main.rand.NextVector2Circular(8, 8), Color.Cyan, 1f, 20, true, Color.DarkBlue);
                    p.Spawn();
                }
            }
        }

        private void ProbeLasers(int count)
        {
            List<NPC> probes = Main.npc.Where(n => n.active && n.type == NPCID.Probe).ToList();

            int max = probes.Count;
            int attempt = Main.rand.Next(max);
            int probesActivated = 0;
            for (int i = 0; i < max; i++)
            {
                NPC probe = probes[attempt];
                if (!probe.GetGlobalNPC<Probe>().ShootLaser)
                {
                    probe.GetGlobalNPC<Probe>().ShootLaser = true;
                    probe.GetGlobalNPC<Probe>().AttackTimer = 0;

                    if (++probesActivated >= count)
                        break;

                    attempt += Main.rand.Next(max);
                }

                attempt = (attempt + 1) % max; //start from a random point in the probe collection and try to make one shoot a laser, looping around at end of list
            }
        }

        private void NonCoilAttacksAI(NPC npc, ref float num15, ref float num16, ref Vector2 target, ref float maxSpeed, ref float flySpeedModifierRatio)
        {
            int MechElectricOrbThreshold = P2_ATTACK_SPACING * 3;
            int laserThreshold = P2_ATTACK_SPACING * 2;
            if (AttackModeTimer == P2_ATTACK_SPACING - 120) // spawn probes
            {
                int probeCount = WorldSavingSystem.MasochistModeReal ? 7 : 5;
                probeCount -= NPC.CountNPCS(NPCID.Probe);
                if (probeCount > 0)
                {
                    SpawnProbes(npc, probeCount);
                }
            }
            if (FargoSoulsUtil.HostCheck && AttackModeTimer > P2_ATTACK_SPACING - 120 && AttackModeTimer < P2_ATTACK_SPACING * 2 - 60)
            {
                int interval = WorldSavingSystem.MasochistModeReal ? 40 : 120;
                if (AttackModeTimer % interval == 12) //make a probe shoot
                {
                    ProbeLasers(2);
                }
            }

            int maxMechElectricOrbIntervals = 4;
            if (npc.life < npc.lifeMax * 0.75)
                maxMechElectricOrbIntervals = 5;
            if (npc.life < npc.lifeMax * 0.5)
                maxMechElectricOrbIntervals = 6;
            if (npc.life < npc.lifeMax * 0.25)
                maxMechElectricOrbIntervals = 7;

            const int MechElectricOrbPause = 50;
            int upperMechElectricOrbTime = MechElectricOrbThreshold + maxMechElectricOrbIntervals * MechElectricOrbPause;
            if (AttackModeTimer == MechElectricOrbThreshold)
                SecondaryAttackTimer = 0;
            if (AttackModeTimer >= MechElectricOrbThreshold && AttackModeTimer <= upperMechElectricOrbTime + 90) //spaced star spread attack
            {
                if (WorldSavingSystem.MasochistModeReal)
                {
                    num15 *= 0.75f;
                    num16 *= 0.75f;
                }
                else
                {
                    if (npc.Distance(target) < 600) //get away from player at high speed
                    {
                        target += npc.DirectionFrom(target) * 1000;

                        num15 = 0.4f;
                        num16 = 0.5f;
                    }
                    else
                    {
                        target += npc.SafeDirectionTo(target).RotatedBy(MathHelper.PiOver2) * 1200;

                        if (npc.Distance(target) < 1200)
                        {
                            maxSpeed *= 0.5f;
                        }
                        else //stop running
                        {
                            num15 *= 2f;
                            num16 *= 2f;
                        }
                    }
                }

                if (AttackModeTimer < upperMechElectricOrbTime && AttackModeTimer % MechElectricOrbPause == 0)
                {
                    Vector2 targetPos = Main.player[npc.target].Center;

                    List<int> segments = [];
                    foreach (NPC n in Main.npc.Where(n => n.active && n.realLife == npc.whoAmI && n.Distance(targetPos) < 1600))
                        segments.Add(n.whoAmI);

                    NPC segment = segments.Count > 0 ? Main.npc[Main.rand.Next(segments)] : npc;

                    targetPos += segment.DirectionFrom(targetPos) * Math.Min(300, segment.Distance(targetPos)); //slightly between player and npc

                    float accelerationAngle = segment.SafeDirectionTo(targetPos).ToRotation();

                    double maxStarModifier = 0.5 + 0.5 * Math.Sin(MathHelper.Pi / (maxMechElectricOrbIntervals - 1) * SecondaryAttackTimer++);
                    int maxStarsInOneWave = (int)(maxStarModifier * (8.0 - 7.0 * npc.life / npc.lifeMax));
                    if (maxStarsInOneWave > 6)
                        maxStarsInOneWave = 6;
                    //Main.NewText($"{Counter3} {maxStarModifier} {maxStarsInOneWave} {maxMechElectricOrbIntervals}");
                    SoundEngine.PlaySound(MechElectricOrb.ShotSound with { Volume = 0.5f, MaxInstances = 4 }, Main.player[npc.target].position);
                    for (int i = -maxStarsInOneWave; i <= maxStarsInOneWave; i++)
                    {
                        Vector2 offset = segment.SafeDirectionTo(targetPos).RotatedBy(MathHelper.PiOver2);
                        float offsetLength = 1000 / Math.Max(maxStarsInOneWave, 1) * i;
                        int travelTime = 30 + Math.Abs(i) * 5;
                        Vector2 individualTarget = targetPos + offset * offsetLength;
                        Vector2 vel = (individualTarget - segment.Center) / travelTime;
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), segment.Center, vel * 2, ModContent.ProjectileType<MechElectricOrbDestroyer>(), ProjectileDamage(npc), 0f, Main.myPlayer, accelerationAngle, -travelTime, ai2: MechElectricOrb.Blue);
                        //Main.NewText($"{segment.Center} to {individualTarget}, dist {segment.Distance(individualTarget)}");
                        //Main.NewText($"vel: {vel * 2} for {travelTime} ticks");
                        //Main.NewText($"sanity: {targetPos}");
                    }
                    //Main.NewText($"targetpos dist to player: {Main.player[npc.target].Distance(targetPos)}");
                }
            }
            int telegraphTime = 120;

            if (AttackModeTimer == laserThreshold - telegraphTime) //tell for hyper dash for light show
            {
                SecondaryAttackTimer = 0;
                SoundEngine.PlaySound(ScanSound with { Volume = 2f }, npc.Center);
                if (FargoSoulsUtil.HostCheck)
                {
                    float angle = MathHelper.Pi * 0.7f;
                    int p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, npc.velocity, ModContent.ProjectileType<DestroyerScanTelegraph>(), 0, 0f, Main.myPlayer, 500, angle, 0);
                    if (p != Main.maxProjectiles)
                        Main.projectile[p].timeLeft = telegraphTime;
                    //Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 9, npc.whoAmI);
                    //Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 9, npc.whoAmI);
                }
            }
            if (AttackModeTimer.IsWithinBounds(laserThreshold - telegraphTime, laserThreshold) && npc.HasPlayerTarget) // while telegraphing light show
            {
                npc.velocity = Vector2.Lerp(npc.velocity, (Main.player[npc.target].Center - npc.Center) / 80, 0.05f);
            }
            if (AttackModeTimer > laserThreshold && AttackModeTimer < laserThreshold + 420)
            {
                flySpeedModifierRatio /= 2;

                if (SecondaryAttackTimer == 0) //fly at player
                {
                    if (maxSpeed < 16)
                        maxSpeed = 16;
                    maxSpeed *= 1.5f;

                    num15 *= 10;
                    num16 *= 10;

                    if (npc.Distance(target) < 400)
                    {
                        SecondaryAttackTimer = 1;
                        npc.velocity = 20f * npc.SafeDirectionTo(target);//.RotatedBy(MathHelper.ToRadians(30) * (Main.rand.NextBool() ? -1 : 1));

                        if (!WorldSavingSystem.MasochistModeReal) //deflect away at the last second
                        {
                            float targetSpeedDirection = MathHelper.WrapAngle(Main.player[npc.target].velocity.ToRotation() - npc.velocity.ToRotation());
                            npc.velocity = npc.velocity.RotatedBy(MathHelper.ToRadians(30) * -Math.Sign(targetSpeedDirection));
                        }

                        npc.netUpdate = true;
                        NetSync(npc);
                    }
                }
                else
                {
                    double angle = npc.SafeDirectionTo(target).ToRotation() - npc.velocity.ToRotation();
                    while (angle > Math.PI)
                        angle -= 2.0 * Math.PI;
                    while (angle < -Math.PI)
                        angle += 2.0 * Math.PI;
                    int rotationTowardsPlayer = Math.Sign(angle);

                    bool playerIsInFront = Math.Abs(angle) < MathHelper.ToRadians(45);
                    if (!playerIsInFront)
                    {
                        if (WorldSavingSystem.MasochistModeReal)
                            maxSpeed /= 2;
                        else if (maxSpeed > 4)
                            maxSpeed = 4;

                        if (npc.velocity.Length() > maxSpeed)
                            npc.velocity *= 0.986f;

                        float turnModifier = 15f;
                        num15 /= turnModifier; //garbage turning
                        num16 /= turnModifier;
                    }

                    //curve very slightly towards player
                    npc.velocity = npc.velocity.RotatedBy(MathHelper.ToRadians(0.3f) * rotationTowardsPlayer);

                    if (AttackModeTimer < laserThreshold + 300 && ++SecondaryAttackTimer % 90 == 20)
                    {
                        LightshowSlowTimer = 120;
                        bool flip = Main.rand.NextBool();
                        bool spawn = true;
                        foreach (NPC n in Main.npc.Where(n => n.active && n.realLife == npc.whoAmI))
                        {
                            spawn = !spawn;
                            if (!spawn)
                                continue;

                            if (FargoSoulsUtil.HostCheck)
                            {
                                if (Main.rand.NextFloat() > npc.life / npc.lifeMax)
                                {
                                    float range = MathHelper.ToRadians(10);
                                    float ai1 = n.rotation + (flip ? 0 : MathHelper.Pi) + Main.rand.NextFloat(-range, range);
                                    int p = Projectile.NewProjectile(npc.GetSource_FromThis(), n.Center, Vector2.Zero, ModContent.ProjectileType<GlowLine>(), ProjectileDamage(npc), 0f, Main.myPlayer, 11, n.whoAmI);
                                    if (p != Main.maxProjectiles)
                                    {
                                        Main.projectile[p].localAI[1] = ai1;
                                        if (Main.netMode == NetmodeID.Server)
                                            NetMessage.SendData(MessageID.SyncProjectile, number: p);
                                    }
                                }
                            }

                            flip = !flip;
                            if (Main.rand.NextBool(5))
                                flip = !flip;
                        }
                    }
                }
            }
        }

        private static void MovementAI(NPC npc, Vector2 target, float num15, float num16, float maxSpeed)
        {
            float num17 = target.X;
            float num18 = target.Y;

            float num21 = num17 - npc.Center.X;
            float num22 = num18 - npc.Center.Y;
            float num23 = (float)Math.Sqrt((double)num21 * (double)num21 + (double)num22 * (double)num22);

            //ground movement code but it always runs
            float num2 = (float)Math.Sqrt(num21 * num21 + num22 * num22);
            float num3 = Math.Abs(num21);
            float num4 = Math.Abs(num22);
            float num5 = maxSpeed / num2;
            float num6 = num21 * num5;
            float num7 = num22 * num5;
            if ((npc.velocity.X > 0f && num6 > 0f || npc.velocity.X < 0f && num6 < 0f) && (npc.velocity.Y > 0f && num7 > 0f || npc.velocity.Y < 0f && num7 < 0f))
            {
                if (npc.velocity.X < num6)
                    npc.velocity.X += num16;
                else if (npc.velocity.X > num6)
                    npc.velocity.X -= num16;
                if (npc.velocity.Y < num7)
                    npc.velocity.Y += num16;
                else if (npc.velocity.Y > num7)
                    npc.velocity.Y -= num16;
            }
            if (npc.velocity.X > 0f && num6 > 0f || npc.velocity.X < 0f && num6 < 0f || npc.velocity.Y > 0f && num7 > 0f || npc.velocity.Y < 0f && num7 < 0f)
            {
                if (npc.velocity.X < num6)
                    npc.velocity.X += num15;
                else if (npc.velocity.X > num6)
                    npc.velocity.X -= num15;
                if (npc.velocity.Y < num7)
                    npc.velocity.Y += num15;
                else if (npc.velocity.Y > num7)
                    npc.velocity.Y -= num15;

                if (Math.Abs(num7) < maxSpeed * 0.2f && (npc.velocity.X > 0f && num6 < 0f || npc.velocity.X < 0f && num6 > 0f))
                {
                    if (npc.velocity.Y > 0f)
                        npc.velocity.Y += num15 * 2f;
                    else
                        npc.velocity.Y -= num15 * 2f;
                }
                if (Math.Abs(num6) < maxSpeed * 0.2f && (npc.velocity.Y > 0f && num7 < 0f || npc.velocity.Y < 0f && num7 > 0f))
                {
                    if (npc.velocity.X > 0f)
                        npc.velocity.X += num15 * 2f;
                    else
                        npc.velocity.X -= num15 * 2f;
                }
            }
            else if (num3 > num4)
            {
                if (npc.velocity.X < num6)
                    npc.velocity.X += num15 * 1.1f;
                else if (npc.velocity.X > num6)
                    npc.velocity.X -= num15 * 1.1f;

                if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < maxSpeed * 0.5f)
                {
                    if (npc.velocity.Y > 0f)
                        npc.velocity.Y += num15;
                    else
                        npc.velocity.Y -= num15;
                }
            }
            else
            {
                if (npc.velocity.Y < num7)
                    npc.velocity.Y += num15 * 1.1f;
                else if (npc.velocity.Y > num7)
                    npc.velocity.Y -= num15 * 1.1f;

                if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < maxSpeed * 0.5f)
                {
                    if (npc.velocity.X > 0f)
                        npc.velocity.X += num15;
                    else
                        npc.velocity.X -= num15;
                }
            }
            npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) + 1.57f;
            npc.netUpdate = true;
            npc.localAI[0] = 1f;
        }

        public override bool SafePreAI(NPC npc)
        {
            EModeGlobalNPC.destroyBoss = npc.whoAmI;


            if (!InPhase2)
            {
                AttackModeTimer++;
                if (NPC.CountNPCS(NPCID.Probe) < 2 && AttackModeTimer % 120 == 0)
                {
                    SpawnProbes(npc, 1);
                }
                if (AttackModeTimer % 120 == 80) // make a probe shoot
                {
                    ProbeLasers(1);
                }
                if (Phase2HP(npc))
                {
                    InPhase2 = true;
                    AttackModeTimer = P2_COIL_BEGIN_TIME;
                    npc.netUpdate = true;
                    if (npc.HasPlayerTarget)
                        SoundEngine.PlaySound(ScanSound with { Pitch = 1f, Volume = 2f }, Main.player[npc.target].Center);
                }
            }
            else
            {
                if (npc.HasValidTarget && (!Main.dayTime || Main.remixWorld))
                {
                    npc.timeLeft = 600; //don't despawn

                    if (IsCoiling) //spinning
                    {
                        CoilAI(npc);
                    }
                    else
                    {
                        NonCoilAI(npc);
                    }

                    if (Main.netMode == NetmodeID.Server && npc.netUpdate && --npc.netSpam < 0) //manual mp sync control
                    {
                        npc.netUpdate = false;
                        npc.netSpam = 5;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                    }
                    return false;
                }
                else
                {
                    npc.velocity.Y++;
                    if (npc.timeLeft > 60)
                        npc.timeLeft = 60;
                    return true;
                }
            }

            EModeUtils.DropSummon(npc, ItemID.MechanicalWorm, NPC.downedMechBoss1, ref DroppedSummon, Main.hardMode);

            return true;
        }

        public override void ModifyHitByAnything(NPC npc, Player player, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitByAnything(npc, player, ref modifiers);

            if (IsCoiling)
            {
                if (npc.life < npc.lifeMax / 10)
                {
                    float modifier = Math.Min(1f, AttackModeTimer / 480f);
                    modifiers.FinalDamage *= modifier;
                }
                else
                {
                    modifiers.FinalDamage *= 0.6f;
                }
            }
            else if (npc.life < npc.lifeMax / 10)
            {
                modifiers.FinalDamage *= 0.1f;
            }
            else if (Phase2HP(npc) && (PrepareToCoil || AttackModeTimer >= P2_COIL_BEGIN_TIME - 120))
            {
                modifiers.FinalDamage *= 0.6f;
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.Electrified, 60);
            //target.AddBuff(ModContent.BuffType<LightningRodBuff>(), 600);
        }


        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);

            LoadBossHeadSprite(recolor, 25);
            LoadGore(recolor, 156);
            for (int i = 1; i <= 3; i++)
                LoadDest(recolor, i - 1);
        }
    }

    public class DestroyerSegment : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(NPCID.TheDestroyerBody, NPCID.TheDestroyerTail);

        public int ProjectileCooldownTimer;
        public int AttackTimer;
        public int ProbeReleaseTimer;
        public int IntangibleTimer;
        public bool GoIntangibleAfterCoil;
        public int DisabledTime;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(ProjectileCooldownTimer);
            binaryWriter.Write(AttackTimer);
            binaryWriter.Write(ProbeReleaseTimer);
            binaryWriter.Write(DisabledTime);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            ProjectileCooldownTimer = binaryReader.ReadInt32();
            AttackTimer = binaryReader.ReadInt32();
            ProbeReleaseTimer = binaryReader.ReadInt32();
            DisabledTime = binaryReader.ReadInt32();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            ProbeReleaseTimer = -Main.rand.Next(360);
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Suffocation] = true;
            npc.buffImmune[BuffID.Chilled] = false;
            npc.buffImmune[ModContent.BuffType<TimeFrozenBuff>()] = false;
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            if (npc.type == NPCID.TheDestroyerBody || npc.type == NPCID.TheDestroyerTail)
            {
                npc.ai[2] = 1; // set to "probe deployed" state; no natural probes
                if (DisabledTime > 0)
                    DisabledTime--;
            }

            if (IntangibleTimer > 0)
                IntangibleTimer--;

            if (IntangibleTimer > 20)
            {
                npc.alpha += 12;
                if (npc.alpha > 255)
                    npc.alpha = 255;
            }
            else
            {
                npc.alpha -= 12;
                if (npc.alpha < 0)
                    npc.alpha = 0;
            }

            //if (npc.whoAmI == NPC.FindFirstNPC(npc.type))
                //Main.NewText($"{IntangibleTimer} {npc.alpha}");

            NPC destroyer = FargoSoulsUtil.NPCExists(npc.realLife, NPCID.TheDestroyer);

            if (destroyer == null || npc.life <= 0 || !destroyer.active || destroyer.life <= 0)
            {
                if (FargoSoulsUtil.HostCheck)
                {
                    npc.life = 0;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                    npc.active = false;
                }
                return result;
            }

            Destroyer destroyerEmode = destroyer.GetGlobalNPC<Destroyer>();

            npc.defense = npc.defDefense;
            npc.localAI[0] = 0f; //disable vanilla lasers
            npc.buffImmune[ModContent.BuffType<TimeFrozenBuff>()] = destroyer.buffImmune[ModContent.BuffType<TimeFrozenBuff>()];

            if (destroyerEmode.IsCoiling) //spinning
            {
                npc.defense = 0;

                ProjectileCooldownTimer = 180;
                if (AttackTimer < 0)
                    AttackTimer = 0; //cancel startup on any imminent projectiles
                Vector2 pivot = Main.npc[npc.realLife].Center;
                pivot += Vector2.Normalize(Main.npc[npc.realLife].velocity.RotatedBy(MathHelper.PiOver2 * destroyerEmode.RotationDirection)) * 600;
                if (npc.Distance(pivot) < 600) //make sure body doesnt coil into the circling zone
                    npc.Center = pivot + npc.DirectionFrom(pivot) * 600;

                if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld)
                    GoIntangibleAfterCoil = true;
            }
            else //not spinning
            {
                if (GoIntangibleAfterCoil)
                {
                    IntangibleTimer = 120;
                    GoIntangibleAfterCoil = false;
                }
            }

            if (destroyerEmode.InPhase2)
                AttackTimer = 0; //just shut it off, fuck it

            if (ProjectileCooldownTimer > 0) //no lasers or stars while or shortly after spinning
            {
                ProjectileCooldownTimer--;
                if (AttackTimer > 1000)
                    AttackTimer = 1000;
            }
            int rarity = 5200;
            if (AttackTimer == 0)
                AttackTimer = Main.rand.Next(rarity - 60);
            AttackTimer += Main.rand.Next(3);
            if (AttackTimer >= Main.rand.Next(rarity, 36000))
            {
                AttackTimer = 1;
                npc.TargetClosest();
                if (FargoSoulsUtil.HostCheck)
                {
                    Vector2 distance = Main.player[npc.target].Center - npc.Center;

                    float modifier = 28f * (1f - (float)Main.npc[npc.realLife].life / Main.npc[npc.realLife].lifeMax);
                    if (modifier < 12)
                        modifier = 12;

                    int delay = (int)(distance.Length() / modifier) / 2;
                    if (delay < 0)
                        delay = 0;

                    int type = ModContent.ProjectileType<MechElectricOrbHoming>();
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Normalize(distance) * modifier, type, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 0f, Main.myPlayer, npc.target, -delay, ai2: MechElectricOrb.Blue);
                }
            }

            if (npc.buffType[0] != 0 && npc.buffType[0] != BuffID.Chilled && npc.buffType[0] != ModContent.BuffType<TimeFrozenBuff>())
                npc.DelBuff(0);

            return result;
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int CooldownSlot)
        {
            NPC destroyer = FargoSoulsUtil.NPCExists(npc.realLife, NPCID.TheDestroyer);

            if (destroyer == null)
                return base.CanHitPlayer(npc, target, ref CooldownSlot);

            if (IntangibleTimer > 0)
                return false;

            Destroyer destroyerEmode = destroyer.GetGlobalNPC<Destroyer>(); //basically, don't hit player right around when a coil begins, segments inside radius may move eratically
            if (destroyerEmode.IsCoiling)
            {
                if (destroyerEmode.AttackModeTimer < 15)
                    return false;
            }
            else if (destroyerEmode.PrepareToCoil)
            {
                return false;
            }

            return true;
        }

        public override void ModifyHitByAnything(NPC npc, Player player, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitByAnything(npc, player, ref modifiers);

            NPC destroyer = FargoSoulsUtil.NPCExists(npc.realLife, NPCID.TheDestroyer);

            if (destroyer == null)
                return;

            Destroyer destroyerEmode = destroyer.GetGlobalNPC<Destroyer>();

            if (destroyerEmode.IsCoiling)
            {
                if (npc.life < npc.lifeMax / 10)
                {
                    float modifier = Math.Min(1f, destroyerEmode.AttackModeTimer / 480f);
                    modifiers.FinalDamage *= modifier;
                }
                else
                {
                    modifiers.FinalDamage *= 0.6f;
                }
            }
            else if (destroyer.life < destroyer.lifeMax / 10)
            {
                modifiers.FinalDamage *= 0.1f;
            }
            else if (Destroyer.Phase2HP(npc) && (destroyerEmode.PrepareToCoil || destroyerEmode.AttackModeTimer >= Destroyer.P2_COIL_BEGIN_TIME - 120 || destroyer.life < destroyer.lifeMax / 10))
            {
                modifiers.FinalDamage *= 0.6f;
            }

            if (Main.npc.Count(n => n.active && n.type == npc.type && n.Distance(npc.Center) < npc.width * 0.75) > 4)
            {
                modifiers.Null();
            }
        }


        public override void SafeModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            base.SafeModifyHitByProjectile(npc, projectile, ref modifiers);

            if (projectile.type == ProjectileID.RainFriendly)
                modifiers.FinalDamage /= 2;

            if (projectile.type == ProjectileID.MonkStaffT1 || projectile.type == ProjectileID.MonkStaffT1Explosion) // sleepy
                modifiers.FinalDamage *= 0.3f;

            if (WorldSavingSystem.SwarmActive)
                if (projectile.type == ModContent.ProjectileType<StyxGazer>() || projectile.type == ModContent.ProjectileType<StyxSickle>())
                    modifiers.FinalDamage *= 0.001f;

            PierceResistance(projectile, ref modifiers);
        }
        public override void SafeModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0.7f;
        }
        public static void PierceResistance(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!FargoSoulsUtil.IsSummonDamage(projectile)
            && !FargoSoulsSets.Projectiles.PierceResistImmune[projectile.type]
            && !EModeGlobalProjectile.PierceResistImmuneAiStyles.Contains(projectile.aiStyle)
            && !projectile.FargoSouls().IsAHeldProj)
            {
                if (projectile.numHits > 0)
                    modifiers.FinalDamage *= 1f / MathF.Pow(1.75f, projectile.numHits);
            }
            else if (projectile.maxPenetrate >= 20 || projectile.maxPenetrate <= -1)
            { 
                //generic damage decrease so projectiles that needed to be exempted dont overpower
                //only affects projs of the type that are effectively infinite pierce
                modifiers.FinalDamage *= 0.7f;
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.Electrified, 60);
            //target.AddBuff(ModContent.BuffType<LightningRodBuff>(), 600);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
        }
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (npc.type == NPCID.TheDestroyerBody ||npc.type == NPCID.TheDestroyerTail) // segments always active
            {
                if (DisabledTime > 0)
                {
                    npc.frame.Y = frameHeight;
                    int[] blinkTimes = [10, 20, 40, 60, 90, 130, 170, 220];
                    foreach (int i in blinkTimes)
                    {
                        if (Math.Abs(DisabledTime - i) < 5)
                            npc.frame.Y = 0;
                    }
                }
                else
                    npc.frame.Y = 0;
            }
            base.FindFrame(npc, frameHeight);
        }
    }

    public class Probe : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Probe);

        public int OrbitChangeTimer;
        public int OrbitDirection;
        public int AttackTimer;
        public int GlowmaskFadeTimer;

        public float TargetOrbitRotation;

        public bool ShootLaser;

        public int FallingState;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(OrbitChangeTimer);
            binaryWriter.Write(OrbitDirection);
            binaryWriter.Write(AttackTimer);
            binaryWriter.Write(GlowmaskFadeTimer);
            binaryWriter.Write(TargetOrbitRotation);
            bitWriter.WriteBit(ShootLaser);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            OrbitChangeTimer = binaryReader.ReadInt32();
            OrbitDirection = binaryReader.ReadInt32();
            AttackTimer = binaryReader.ReadInt32();
            GlowmaskFadeTimer = binaryReader.ReadInt32();
            TargetOrbitRotation = binaryReader.ReadSingle();
            ShootLaser = bitReader.ReadBit();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.destroyBoss, NPCID.TheDestroyer))
                npc.lifeMax = (int)(npc.lifeMax * 1.5);
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int CooldownSlot)
        {
            return !FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.destroyBoss, NPCID.TheDestroyer)
                || (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld);
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Suffocation] = true;

            if (Main.rand.NextBool(4) && !LumUtils.AnyBosses() && npc.FargoSouls().CanHordeSplit)
                EModeGlobalNPC.Horde(npc, 8);
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            if (FallingState > 0)
            {
                npc.dontTakeDamage = true;
                npc.velocity.X *= 0.96f;
                npc.velocity.Y += 0.7f;
                npc.rotation += npc.velocity.X / 30;
                if (Collision.SolidCollision(npc.position, npc.width, npc.height))
                {
                    FallingState = 2;
                    npc.life = 0;
                    npc.HitEffect();
                    npc.checkDead();
                }
                    

                if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.destroyBoss, NPCID.TheDestroyer))
                {
                    List<NPC> segments = Main.npc.Where(n => n.type is NPCID.TheDestroyer or NPCID.TheDestroyerBody or NPCID.TheDestroyerTail).ToList();
                    segments = segments.OrderBy(n => n.DistanceSQ(npc.Center)).ToList();
                    NPC closestSegment = segments[0];
                    if (closestSegment.Hitbox.Intersects(npc.Hitbox))
                    {
                        FallingState = 2;
                        closestSegment.SimpleStrikeNPC(npc.lifeMax, (int)npc.HorizontalDirectionTo(closestSegment.Center));
                        npc.life = 0;
                        npc.HitEffect();
                        npc.checkDead();
                    }
                }
                return false;
            }

            if (!FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.destroyBoss, NPCID.TheDestroyer))
                return result;

            //bool isCoiling = Main.npc[EModeGlobalNPC.destroyBoss].GetGlobalNPC<Destroyer>().IsCoiling;

            if (!(WorldSavingSystem.MasochistModeReal && Main.getGoodWorld))
            {
                if (npc.localAI[0] > 30) //disable vanilla lasers unless maso ftw
                    npc.localAI[0] = -30;
            }

            if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld) //use vanilla movement unless shooting laser
            {
                if (!ShootLaser)
                    return result;
            }

            if (++OrbitChangeTimer > 120) //choose a direction to orbit in
            {
                OrbitChangeTimer = 0;
                OrbitDirection = Main.rand.NextBool() ? 1 : -1;

                npc.netUpdate = true;
                NetSync(npc);
            }

            if (Main.npc[EModeGlobalNPC.destroyBoss].GetGlobalNPC<Destroyer>().IsCoiling)
                ShootLaser = false;

            if (npc.HasValidTarget)
            {
                if (ShootLaser)
                {
                    Vector2 towardsPlayer = 6f * npc.SafeDirectionTo(Main.player[npc.target].Center);
                    const int attackTime = 110;

                    if (AttackTimer == 0)
                    {
                        TargetOrbitRotation = Main.player[npc.target].SafeDirectionTo(npc.Center).ToRotation(); //when shooting laser, stop orbiting
                        
                        npc.netUpdate = true;
                        NetSync(npc);

                        float angle = MathHelper.Pi * 0.3f;
                        if (FargoSoulsUtil.HostCheck)
                        {
                            int p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, npc.velocity, ModContent.ProjectileType<DestroyerScanTelegraph>(), 0, 0f, Main.myPlayer, 100, angle, 0);
                            if (p != Main.maxProjectiles)
                                Main.projectile[p].timeLeft = attackTime;
                        }
                            
                    }
                       

                    /*
                    int dustID = WorldSavingSystem.EternityMode && SoulConfig.Instance.BossRecolors ? DustID.GemSapphire : DustID.GemRuby;
                    float dustScale = 0.5f + 2.5f * AttackTimer / attackTime;
                    int d = Dust.NewDust(npc.position, npc.width, npc.height, dustID, 2f * towardsPlayer.X, 2f * towardsPlayer.Y, 0, default, dustScale);
                    Main.dust[d].noGravity = true;
                    */

                    if (++AttackTimer > attackTime)
                    {
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, towardsPlayer, ProjectileID.DeathLaser, (int)(1.1 * FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage)), 0f, Main.myPlayer);

                        npc.velocity -= towardsPlayer * 2.5f;
                        AttackTimer = 0;
                        ShootLaser = false;

                        npc.netUpdate = true;
                        NetSync(npc);
                    }
                }

                float orbitDistance = ShootLaser ? 440 : 220; //LAUGH
                Vector2 vel = Main.player[npc.target].Center - npc.Center;
                vel += orbitDistance * (ShootLaser ? Vector2.UnitX.RotatedBy(TargetOrbitRotation) : Main.player[npc.target].SafeDirectionTo(npc.Center).RotatedBy(MathHelper.ToRadians(22) * OrbitDirection));

                npc.position += (Main.player[npc.target].position - Main.player[npc.target].oldPosition) / 3;

                if (npc.Distance(Main.player[npc.target].Center) < 150) //snap away really fast if too close
                {
                    npc.velocity = vel / 20;
                }
                else //regular movement
                {
                    vel.Normalize();
                    vel *= 12f;

                    float moveSpeed = 0.7f;

                    if (ShootLaser)
                    {
                        vel *= 1.5f;
                        moveSpeed *= 1.5f;
                    }

                    if (npc.velocity.X < vel.X)
                    {
                        npc.velocity.X += moveSpeed;
                        if (npc.velocity.X < 0 && vel.X > 0)
                            npc.velocity.X += moveSpeed;
                    }
                    else if (npc.velocity.X > vel.X)
                    {
                        npc.velocity.X -= moveSpeed;
                        if (npc.velocity.X > 0 && vel.X < 0)
                            npc.velocity.X -= moveSpeed;
                    }
                    if (npc.velocity.Y < vel.Y)
                    {
                        npc.velocity.Y += moveSpeed;
                        if (npc.velocity.Y < 0 && vel.Y > 0)
                            npc.velocity.Y += moveSpeed;
                    }
                    else if (npc.velocity.Y > vel.Y)
                    {
                        npc.velocity.Y -= moveSpeed;
                        if (npc.velocity.Y > 0 && vel.Y < 0)
                            npc.velocity.Y -= moveSpeed;
                    }
                }
            }

            return result;
        }

        public override bool CheckDead(NPC npc)
        {
            if (FallingState < 2)
            {
                if (FallingState == 0)
                    npc.velocity *= 0.7f;
                npc.dontTakeDamage = true;
                FallingState = 1;
                npc.life = 1;
                npc.active = true;
                return false;
            }
            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.destroyBoss, NPCID.TheDestroyer))
            {
                npc.active = false;
                if (npc.DeathSound != null)
                    SoundEngine.PlaySound(npc.DeathSound.Value, npc.Center);
                return false;
            }

            return base.CheckDead(npc);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadSpecial(recolor, ref TextureAssets.Probe, ref FargowiltasSouls.TextureBuffer.Probe, "Probe");
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float opacity = 1;
            if (FallingState > 0)
                opacity = MathHelper.Lerp(1, 0, ++GlowmaskFadeTimer * 0.04f);
            if (opacity < 0)
                opacity = 0;
            Rectangle rectangle = npc.frame;
            Vector2 origin2 = rectangle.Size() / 2f;



            Main.EntitySpriteDraw(TextureAssets.Npc[npc.type].Value, npc.Center - Main.screenPosition, new Rectangle?(rectangle), drawColor, npc.rotation, rectangle.Size() / 2, npc.scale, npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

            Main.EntitySpriteDraw(TextureAssets.Probe.Value, npc.Center - Main.screenPosition, new Rectangle?(rectangle), Color.White * opacity, npc.rotation, rectangle.Size() / 2, npc.scale, npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
    }
}
