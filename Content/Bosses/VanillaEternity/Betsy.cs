using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA;
using FargowiltasSouls.Content.Patreon.DanielTheRobot;
using FargowiltasSouls.Content.Patreon.Duck;
using FargowiltasSouls.Content.Patreon.Volknet.Projectiles;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Deathrays;
using FargowiltasSouls.Content.Projectiles.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Hell;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using static FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA.DD2Ogre;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class Betsy : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.DD2Betsy);

        public int EntranceTimer = 0;

        public bool InPhase2 = false;
        public bool InPhase3 = false;

        public int wingFrame = 0;
        public int wingFrameCounter = 0;
        public int wingRate = 2;
        public int frameY = 0;

        public bool DroppedSummon;

        public bool TargetPlayer = false;
        public bool isStunning = false;

        private enum States
        {
            Spawn = 0,
            KillOOA,
            MiniOldOnesArmy,
            Idle,
            HarassCrystal,
            FireballRain,
            DirectDashes,
            WindVortex,
            FusedSigils


        }

        public bool ContactDamage = true;
        public int State = 0;
        public int PreviousState = 0;
        public int SubState = 0;
        public int heldProj = -1;
        public int Timer = 0;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            bitWriter.WriteBit(InPhase2);
            bitWriter.WriteBit(TargetPlayer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            InPhase2 = bitReader.ReadBit();
            TargetPlayer = bitReader.ReadBit();
        }

        public override void SetDefaults(NPC npc)
        {
            npc.boss = true;
            npc.lifeMax = (int)Math.Round(npc.lifeMax * 4.0 / 3.0);
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (!ContactDamage)
                return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }

        public override bool SafePreAI(NPC npc)
        {
            EModeGlobalNPC.betsyBoss = npc.whoAmI;
            EModeDD2Event.BetsyBlockSpawn = true;

            if (!DD2Event.Ongoing)
                npc.active = false;

            ContactDamage = true;
            Timer++;

            // wing animation
            if (wingFrameCounter++ > wingRate)
            {
                wingFrameCounter = 0;
                wingFrame++;
                if (wingFrame > 8)
                    wingFrame = 0;
            }
            wingRate = 2;

            if (isStunning)
            {
                foreach (var proj in Main.ActiveProjectiles)
                {
                    if (ProjectileID.Sets.IsADD2Turret[proj.type])
                        proj.Eternity().Jammed = true;
                }
            }
            else
            {
                foreach (var proj in Main.ActiveProjectiles)
                {
                    if (ProjectileID.Sets.IsADD2Turret[proj.type])
                        proj.Eternity().Jammed = false;
                }
            }

            if (InPhase3)
            {
                DeathAnim(npc);
                return false;
            }

            if (DD2Event.LostThisRun) // no crystal, betsy wins
            {
                npc.dontTakeDamage = true;
                npc.chaseable = false;
                return base.SafePreAI(npc);
            }

            if (!npc.HasValidTarget)
                npc.TargetClosest(false);

            if (!Main.player.Any(x => x.Alive() && x.Distance(npc.Center) < 3000) || !npc.HasValidTarget || !npc.HasPlayerTarget)
            {
                if (State != (int)States.HarassCrystal)
                {
                    ResetToIdle(npc);
                    State = (int)States.HarassCrystal;
                }
                HarassCrystal(npc);
                return false;
            }

            TargetPlayer = true;

            if (State == (int)States.HarassCrystal)
                ResetToIdle(npc);

            if (!InPhase2 && npc.life < npc.lifeMax / 2)
            {
                if (EModeDD2Event.BetsyOOAKills < 0)
                {
                    npc.dontTakeDamage = true;
                    npc.chaseable = false;

                    EModeDD2Event.BetsyOOAKills = 0;
                    SoundEngine.PlaySound(SoundID.Roar, npc.Center);
                }
                if (State == (int)States.Idle)
                {
                    isStunning = false;
                    MiniOldOnesArmy(npc);
                    return false;
                }
            }

            switch ((States)State)
            {
                case States.Spawn:
                    SpawnAnim(npc);
                    break;
                case States.Idle:
                    if (Timer > 30)
                    {
                        ChooseAttack(npc);
                    }
                    break;
                case States.FireballRain:
                    FireballRain(npc);
                    break;
                case States.DirectDashes:
                    DirectDashes(npc);
                    break;
                case States.WindVortex:
                    WindVortex(npc);
                    break;
                case States.FusedSigils:
                    FusedSigils(npc);
                    break;
                default:
                    break;
            }

            if (!DD2Event.Ongoing && npc.HasPlayerTarget && (!Main.player[npc.target].active || Main.player[npc.target].dead || npc.Distance(Main.player[npc.target].Center) > 3000))
            {
                int p = Player.FindClosest(npc.Center, 0, 0); //extra despawn code for when summoned outside event
                if (p < 0 || !Main.player[p].active || Main.player[p].dead || npc.Distance(Main.player[p].Center) > 3000)
                    npc.active = false;
            }

            if (DD2Event.Ongoing)
            {
                EModeUtils.DropSummon(npc, "DraconicCrystal", WorldSavingSystem.DownedBetsy, ref DroppedSummon, NPC.downedGolemBoss);
            }
            return false;
        }

        #region AI States
        private void SpawnAnim(NPC npc)
        {
            ContactDamage = false;
            npc.dontTakeDamage = true;
            npc.chaseable = false;
            NPC crystal = EModeDD2Event.GetEterniaCrystal();
            if (crystal == null)
            {
                npc.active = false;
                return;
            }

            Vector2 targetPos = crystal.Center - 250 * Vector2.UnitY;
            if (Timer == 1)
                SoundEngine.PlaySound(SoundID.DD2_BetsyScream, npc.Center);
            float dist = npc.Distance(targetPos);

            if (dist > 20)
            {
                if (targetPos.X == npc.Center.X) // gets stuck on perfect veritcals??
                    npc.Center += float.Epsilon * Vector2.UnitX;

                npc.direction = (int)npc.HorizontalDirectionTo(targetPos);
                Movement(npc, targetPos, traction: 0.6f, maxSpeed: 50);
                Timer = 1;
            }
            else
            {
                npc.velocity *= 0.8f;

                frameY = Math.Min(3, (int)Math.Floor(Timer / 6f));

                if (frameY == 3)
                {
                    isStunning = true;

                    if (Timer == 20)
                        SoundEngine.PlaySound(SoundID.DD2_BetsyScream, npc.Center);

                    if (Timer % 15 == 0)
                        SoundEngine.PlaySound(SoundID.Item94, npc.Center);

                    if (Timer >= 30 && !Main.projectile.Any(x => x.active && x.type == ModContent.ProjectileType<DD2CrystalAura>()))
                    {
                        SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -1f, Volume = 0.5f }, crystal.Center);
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(crystal.GetSource_FromThis(), crystal.Center, Vector2.Zero, ModContent.ProjectileType<DD2CrystalAura>(), 0, 0);
                    }

                    if (Timer % 2 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot with { Pitch = -0.2f, Volume = 2f }, npc.Center);
                        SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot with { Pitch = -0.2f, Volume = 2f }, npc.Center);
                        FargoSoulsUtil.ScreenshakeRumble(1.5f);
                    }

                    if (Timer % 6 == 0)
                    {
                        RoarParticles(npc, Color.Purple);
                        for (int i = 0; i < 10; i++)
                        {
                            float scale = Main.rand.NextFloat(1f, 3f);
                            float randRot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                            new ElectricSpark(npc.Center + 60 * Vector2.UnitX.RotatedBy(randRot), (4 - scale) * 15 * Vector2.UnitX.RotatedBy(randRot),
                                Color.Purple, scale, 35).Spawn();
                        }
                    }

                }

                if (Timer > 120)
                {
                    frameY = Math.Max(0, (int)Math.Floor((120 + 18 - Timer) / 6f));
                    if (frameY == 0)
                    {
                        isStunning = false;
                        npc.dontTakeDamage = false;
                        npc.chaseable = true;
                        ResetToIdle(npc);
                    }
                }
            }
        }

        private void FireballRain(NPC npc)
        {
            Player target = Main.player[npc.target];
            Vector2 targetPos = target.Center - 1200 * npc.HorizontalDirectionTo(target.Center) * Vector2.UnitX - 600 * Vector2.UnitY;
            float distToTarget = npc.Distance(targetPos);

            // done after 2 dashes; 1 in p2
            int substates = InPhase2 ? 4 : 8;

            if (SubState % 4 == 0) // get to position
            {
                npc.direction = npc.spriteDirection = (int)npc.HorizontalDirectionTo(target.Center);
                Movement(npc, targetPos);
                Timer = 0;
                if (distToTarget <= (SubState == 0 ? 40 : 80))
                {
                    SubState++;
                }
            }
            else if (SubState % 4 == 1) // prepare
            {
                npc.direction = npc.spriteDirection = (int)npc.HorizontalDirectionTo(target.Center);
                npc.velocity *= 0.9f;
                if (Timer % 3 == 0)
                {
                    frameY = 4 + (Timer / 3);
                }

                if (frameY == 9)
                {
                    Timer = 0;
                    SubState++;
                }
            }
            else if (SubState % 4 == 2) // dash
            {
                npc.velocity = npc.direction * 25 * Vector2.UnitX;
                if (Timer == 1) {
                    Projectile.NewProjectile(npc.GetSource_FromThis(), breathPos(npc), npc.direction * npc.velocity / 2, ProjectileID.DD2BetsyFlameBreath, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 1f, ai1: npc.whoAmI);
                }

                if (Timer % 5 == 1)
                {
                    if (FargoSoulsUtil.HostCheck)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), breathPos(npc), new(npc.direction * 2f, 0f), ProjectileID.DD2BetsyFireball, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 1f);
                    }
                }

                if (Timer >= 60 && Math.Abs(npc.Center.X - target.Center.X) > 550)
                {
                    Timer = 0;
                    SubState++;
                }
            }
            else if (SubState % 4 == 3)
            {
                frameY = 0;
                if (SubState >= substates - 1)
                {
                    Movement(npc, target.Center + Vector2.UnitX * target.HorizontalDirectionTo(npc.Center) * 600 - Vector2.UnitY * 250);
                    npc.direction = npc.spriteDirection = npc.velocity.X.NonZeroSign();
                }
                else
                {
                    npc.velocity *= 0.98f;
                }
                if (Timer > 20)
                {
                    Timer = 0;
                    SubState++;
                    if (target.Center.Y > npc.Center.Y) // only reposition if player is above
                        SubState++;
                }
            }

            if (SubState >= substates)
            {
                ResetToIdle(npc);
            }
        }

        private void DirectDashes(NPC npc)
        {
            Player target = Main.player[npc.target];
            Vector2 targetPos = target.Center - 750 * npc.HorizontalDirectionTo(target.Center) * Vector2.UnitX - 70 * Vector2.UnitY;
            float dist = npc.Distance(targetPos);

            if (SubState % 3 == 0) // get into position
            {
                Movement(npc, targetPos);
                npc.direction = npc.spriteDirection = (int)npc.HorizontalDirectionTo(target.Center);

                if (dist < 100)
                {
                    SubState++;
                    Timer = 0;
                }
            }
            else if (SubState % 3 == 1) // rev up
            {
                npc.velocity.Y *= 0.9f;
                if (Timer % 4 == 0)
                {
                    frameY = 4 + (Timer / 4);
                }
                if (frameY >= 9)
                {
                    SubState++;
                    Timer = 0;
                }
            }
            else if (SubState % 3 == 2) // dash
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), breathPos(npc), Vector2.Zero, ProjectileID.DD2BetsyFlameBreath, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 1f), 1f, ai1: npc.whoAmI);
                }

                const float startTime = 0;
                const float totalTime = 60;
                float progress = (Timer - startTime);
                float maxSpeed = 18f * 2 / (totalTime);
                float spd = progress * maxSpeed;

                npc.velocity = spd * npc.direction * Vector2.UnitX;

                bool condition = Timer == 3 || Timer == 58 || (WorldSavingSystem.MasochistModeReal && Timer == 30);

                if (condition)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsySummon, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            Projectile.NewProjectileDirect(npc.GetSource_FromThis(), npc.Center,
                            Vector2.Zero, ModContent.ProjectileType<BetsySpawnPortal>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 1f, ai0: NPCID.DD2WyvernT3, ai1: npc.target);
                        }
                    }
                }

                if (Timer > 60)
                {
                    frameY = 0;
                    SubState++;
                    Timer = 0;
                }
            }

            int end = InPhase2 ? 6 : 9;

            if (SubState >= end)
            {
                ResetToIdle(npc);
            }
        }

        private void WindVortex(NPC npc)
        {
            Player target = Main.player[npc.target];
            Vector2 targetPos = target.Center - 200 * npc.HorizontalDirectionTo(target.Center) * Vector2.UnitX - 250 * Vector2.UnitY;
            float dist = npc.Distance(targetPos);

            if (SubState == 0) // move to position
            {
                Movement(npc, targetPos);
                npc.direction = npc.spriteDirection = (int)npc.HorizontalDirectionTo(target.Center);

                if (dist < 100)
                {
                    SubState++;
                    Timer = 0;
                }
            }
            else if (SubState == 1) // raise head
            {
                npc.velocity *= 0f;
                wingRate = 1;
                if (Timer % 6 == 0)
                {
                    frameY++;
                    if (frameY > 3)
                    {
                        frameY = 3;
                        SubState++;
                        Timer = 0;
                    }
                }
            }
            else if (SubState == 2) // create vortex
            {
                wingRate = 1;
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlyingCircleAttack, npc.Center);
                    SoundEngine.PlaySound(SoundID.DD2_BetsyScream, npc.Center);
                    SoundEngine.PlaySound(SoundID.Item66, npc.Center);
                    for (int i = 0; i < 15; i++)
                        new SparkParticle(npc.Center, 3 * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), Color.Black, 0.3f, 45).Spawn();
                    if (FargoSoulsUtil.HostCheck)
                        heldProj = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<BetsyWindVortex>(), 0, 0, ai0: npc.whoAmI);
                    //npc.rotation = 0;
                    npc.direction = npc.spriteDirection = -1;
                    NetSync(npc);
                    if (heldProj == Main.maxProjectiles)
                    {
                        Timer = 0;
                        return;
                    }
                }
                else
                {
                    float maxRadius = 700;
                    float radius = MathHelper.Clamp(maxRadius * Timer / 120f, 0, maxRadius);
                    float rot = npc.direction * MathHelper.TwoPi * Timer / 120f;
                    npc.Center = Main.projectile[heldProj].Center + radius * Vector2.UnitX.RotatedBy(rot);
                    float rotLerp = LumUtils.Saturate(0.03f + 0.95f * Timer / 90f);
                    
                    npc.rotation = MathHelper.Lerp(npc.rotation, rot + MathHelper.PiOver2, rotLerp);

                    int frequency = 25;

                    if (radius == 700 && Timer % frequency == 0) // start shooting after vortex is finished
                    {
                        SoundEngine.PlaySound(SoundID.Item60, npc.Center);
                        SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash with { Volume = 2f }, npc.Center);
                        if (FargoSoulsUtil.HostCheck)
                        {
                            if (Timer % (2 * frequency) == 0)
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, 4 * Vector2.UnitX.RotatedBy((target.Center - npc.Center).ToRotation()), ModContent.ProjectileType<FlyingDragonHostile>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 0f);
                            else
                            {
                                for (int i = -1; i < 2; i+=2)
                                {
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, 4 * Vector2.UnitX.RotatedBy((target.Center - npc.Center).ToRotation() + MathHelper.Pi * i / 8.5f), 
                                        ModContent.ProjectileType<FlyingDragonHostile>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 0f);
                                }
                            }
                        }
                    }

                    bool endCondition = Timer > 400 && Timer % 120 == 30;
                    if (InPhase2)
                        endCondition = Timer > 400 - 240 && Timer % 120 == 30;

                    if (endCondition) // fly up from top
                    {
                        npc.velocity = -10 * Vector2.UnitY;
                        frameY = 0;
                        SubState = 3;
                        Timer = 0;
                    }
                }
            }
            else if (SubState == 3) // SLAM
            {
                if (Timer <= 30f)
                {
                    if (Timer == 1)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Pitch = -0.2f }, npc.Center);
                    }

                    npc.velocity *= 0.9f;
                    npc.rotation = (npc.Center - target.Center).ToRotation();
                }
                else
                {
                    frameY = Math.Min(9, 4 + (int)Math.Floor((Timer - 30) / 4f));
                    if (Timer == 50 && FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), breathPos(npc), Vector2.Zero,
                            ProjectileID.DD2BetsyFlameBreath, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 1f), 1f, ai1: npc.whoAmI);

                    npc.velocity = 30 * Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.Pi);


                    if (Collision.SolidCollision(npc.position + npc.velocity, npc.width, npc.height))
                    {
                        int x = (int)(npc.position.X / 16);
                        int y = (int)(npc.position.Y / 16);
                        for (int i = -5; i < 6; i++)
                        {
                            for (int j = -3; j < 1; j++)
                            {
                                WorldGen.KillTile(x + i, y + j, true, true);
                            }
                        }
                        FargoSoulsUtil.ScreenshakeRumble(6f);
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Volume = 3f }, npc.Center);
                        npc.velocity.Y *= -0.2f;
                        frameY = 4;
                        SubState = 4;
                        Timer = 0;
                        foreach (var p in Main.ActiveProjectiles)
                        {
                            if (p.type == ModContent.ProjectileType<BetsyWindVortex>())
                                p.ai[2] = 91; // fade out vortex
                            else if (p.type == ProjectileID.DD2BetsyFlameBreath)
                                p.Kill();
                        }
                    }
                }
            }
            else if (SubState == 4) // stun
            {
                int recoverTime = WorldSavingSystem.MasochistModeReal ? 120 : 240;
                if (InPhase2)
                    recoverTime = (int)(recoverTime * 0.3f);
                npc.velocity *= 0.9f;
                npc.rotation = 0;
                if (Timer < recoverTime - 60 && Timer % 60 == 1)
                {
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Debuffs/DizzyBird") with { Volume = 0.5f }, npc.Center);
                }

                if (Timer == recoverTime - 60)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Pitch = -0.2f }, npc.Center);
                }
                if (Timer > recoverTime - 60)
                {
                    if (Timer % 10 == 0)
                    {
                        frameY--;
                        if (frameY < 0)
                            frameY = 0;
                    }
                }

                if (Timer >= recoverTime)
                {
                    ResetToIdle(npc);
                }
            }
        }

        private void FusedSigils(NPC npc)
        {
            Player target = Main.player[npc.target];
            npc.direction = (int)npc.HorizontalDirectionTo(target.Center);
            Vector2 targetPos = target.Center + new Vector2(-npc.direction * 700, -100);

            Movement(npc, targetPos, 0.7f);

            if (SubState == 0 && npc.Distance(targetPos) < 70)
            {
                SubState = 1;
                Timer = 0;
            }
            if (SubState == 1)
            {
                frameY = Math.Min(9, 5 + Timer / 6);
                if (frameY == 9)
                {
                    if (Timer == 30)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyScream, npc.Center);
                    }

                    int timeToStop = InPhase2 ? 250 : 400;
                    if (Timer < timeToStop - 50 && Timer % 25 == 0)
                    {
                        if (FargoSoulsUtil.HostCheck)
                        {
                            float r = 300;
                            float minR = 40;
                            Vector2 offset = new Vector2((Main.rand.NextBool() ? -1 : 1) * Main.rand.NextFloat(minR, r), (Main.rand.NextBool() ? -1 : 1) * Main.rand.NextFloat(minR, r));
                            Projectile.NewProjectile(npc.GetSource_FromThis(), target.Center + offset, Vector2.Zero, ModContent.ProjectileType<BetsyFusedSigil>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 0f, ai0: -1);
                        }
                    }

                    if (Timer > 45 && Timer % 45 == 0)
                    {

                        npc.velocity.X = npc.direction * 3;
                        npc.velocity.Y = 1.5f;
                        if (FargoSoulsUtil.HostCheck)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                Vector2 vel = new Vector2(npc.direction, -1.7f) * 9;
                                vel.X += Main.rand.NextFloat(-1, 1) * 1.4f;
                                vel.Y -= Main.rand.NextFloat(0, 1) * 3.7f;
                                vel.X += (i - 2) * 5;
                                vel /= 1.6f;
                                int p = Projectile.NewProjectile(npc.GetSource_FromAI(), breathPos(npc), vel, ProjectileID.DD2BetsyFireball, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 1f);
                                if (p != Main.maxProjectiles)
                                {
                                    Main.projectile[p].extraUpdates = 1;
                                }
                            }
                        }
                        /*
                        if (FargoSoulsUtil.HostCheck)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                float xVel = Main.rand.NextFloat(1f, 10f);
                                int p = Projectile.NewProjectile(npc.GetSource_FromThis(), breathPos(npc), new(npc.direction * xVel, -10f), 
                                    ProjectileID.DD2BetsyFireball, npc.damage / 3, 1f);
                                if (p != Main.maxProjectiles)
                                {
                                    Main.projectile[p].extraUpdates = 1;
                                }

                            }
                        }
                        */
                    }

                    if (Timer > timeToStop)
                    {
                        frameY = 0;
                        ResetToIdle(npc);
                    }
                }
            }

        }

        private void HarassCrystal(NPC npc)
        {
            // no players around go beat up the crystal
            NPC target = EModeDD2Event.GetEterniaCrystal();
            if (target == null)
            {
                return;
            }
            Vector2 targetPos = target.Center - 650 * npc.HorizontalDirectionTo(target.Center) * Vector2.UnitX - 50 * Vector2.UnitY;
            float dist = npc.Distance(targetPos);
            npc.rotation = 0;

            if (SubState % 3 == 0)
            {
                Movement(npc, targetPos);
                npc.direction = npc.spriteDirection = (int)npc.HorizontalDirectionTo(target.Center);

                if (dist < 100)
                {
                    SubState++;
                    Timer = 0;
                }
            }
            else if (SubState % 3 == 1)
            {
                npc.velocity.Y *= 0.9f;
                if (Timer % 4 == 0)
                {
                    frameY = 4 + (Timer / 4);
                }
                if (frameY >= 9)
                {
                    SubState++;
                    Timer = 0;
                }
            }
            else if (SubState % 3 == 2)
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), breathPos(npc), Vector2.Zero, ProjectileID.DD2BetsyFlameBreath, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 1f), 1f, ai1: npc.whoAmI);
                }


                if (Timer <= 50)
                    npc.velocity = 24 * npc.direction * Vector2.UnitX;
                else
                    npc.velocity *= 0.98f;

                if (Timer > 60)
                {
                    frameY = 0;
                    SubState++;
                    Timer = 0;
                }
            }
        }

        private void MiniOldOnesArmy(NPC npc)
        {
            ContactDamage = false;
            NPC crystal = EModeDD2Event.GetEterniaCrystal();
            if (crystal == null) // skip if no event
            {
                InPhase2 = true;
                npc.dontTakeDamage = false;
                npc.chaseable = true;
                return;
            }
            Vector2 targetPos = crystal.Center;
            float dist = 0f;
            EModeDD2Event.BetsyOOAKillsNeeded = WorldSavingSystem.MasochistModeReal ? 100 : 50;

            if (EModeDD2Event.BetsyOOAKills >= EModeDD2Event.BetsyOOAKillsNeeded) // end event
            {
                EModeDD2Event.BetsyBlockSpawn = true;
                targetPos = crystal.Center + new Vector2(0, -200);
                dist = npc.Distance(targetPos);
                if (dist > 50)
                {
                    Movement(npc, targetPos);
                    Timer = 0;
                }
                else
                {
                    npc.velocity *= 0.9f;
                    frameY = Math.Min(3, (int)Math.Floor(Timer / 6f));

                    if (frameY == 3)
                    {
                        isStunning = true;
                        if (Timer == 30)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 0.5f }, npc.Center);
                        }
                        if (Timer % 15 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.Item94, npc.Center);
                            if (Timer < 120)
                                RoarParticles(npc, Color.Purple);
                        }

                        if (Timer % 2 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot with { Pitch = -0.2f, Volume = 2f }, npc.Center);
                            SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot with { Pitch = -0.2f, Volume = 2f }, npc.Center);
                            FargoSoulsUtil.ScreenshakeRumble(1.5f);
                        }

                        if (Timer % 6 == 0)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                float scale = Main.rand.NextFloat(1f, 3f);
                                float randRot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                                new ElectricSpark(npc.Center + 60 * Vector2.UnitX.RotatedBy(randRot), (4 - scale) * 15 * Vector2.UnitX.RotatedBy(randRot),
                                    Color.Purple, scale, 35).Spawn();
                            }

                            foreach (var n in Main.ActiveNPCs) // kill everything
                            {
                                if (NPCID.Sets.BelongsToInvasionOldOnesArmy[n.type] && n.type != NPCID.DD2Betsy && n.type != NPCID.DD2EterniaCrystal)
                                {
                                    SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Volume = 0.3f }, n.Center);
                                    n.SimpleStrikeNPC(n.lifeMax / 5, (int)npc.HorizontalDirectionTo(n.Center));
                                }
                            }
                        }

                        if (Timer > 300)
                        {
                            frameY = 0;
                            isStunning = false;
                            InPhase2 = true;
                            ContactDamage = true;
                            npc.dontTakeDamage = false;
                            npc.chaseable = true;
                            ResetToIdle(npc);
                        }
                    }
                }

                return;
            }
            else
            {
                EModeDD2Event.BetsyBlockSpawn = false;
            }

            if (SubState % 5 == 0) // buff portal and nearby enemies
            {
                NPC portal = FindClosestPortal(npc.Center);
                if (portal == null)
                {
                    frameY = 0;
                    InPhase2 = true;
                    ContactDamage = true;
                    npc.dontTakeDamage = false;
                    npc.chaseable = true;
                    ResetToIdle(npc);
                    return;
                }
                targetPos = portal.Center + new Vector2(150 * npc.HorizontalDirectionTo(crystal.Center), -150);
                dist = npc.Distance(targetPos);

                if (dist > 50)
                {
                    Movement(npc, targetPos);
                    Timer = 0;
                }
                else
                {
                    npc.direction = (int)npc.HorizontalDirectionTo(crystal.Center);
                    npc.velocity *= 0.9f;
                    frameY = Math.Min(3, (int)Math.Floor(Timer / 6f));

                    if (frameY == 3)
                    {
                        if (Timer % 120 == 20 && Timer < 220)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_WyvernScream with { Pitch = -0.7f, Variants = [0] }, npc.Center);
                        }

                        // make it spawn faster
                        portal.AI();

                        if (Timer % 15 == 0)
                            RoarParticles(npc, Color.White);

                        const float speedMult = 3f;
                        foreach (NPC n in Main.ActiveNPCs)
                        {
                            if (n.Distance(npc.Center) < 400 && EModeDD2GlobalNPC.IsInstance(n) && !EModeDD2Event.IsDD2Boss(n.type) && !n.EModeDD2().DrakinBuff && n.whoAmI != npc.whoAmI)
                            {
                                n.EModeDD2().DrakinBuff = true;
                                if (n.velocity.Length() == 0)
                                    continue;
                                if (Timer % 2 == 0)
                                    new SparkParticle(n.Top - (n.width * n.spriteDirection) * Vector2.UnitX + Main.rand.Next(0, n.height) * Vector2.UnitY, Main.rand.NextFloat(1, 3) * Vector2.UnitX.RotatedBy(n.rotation), Color.Purple, 0.5f, 14).Spawn();
                                n.position.X += (speedMult - 1) * n.velocity.X;
                                if (n.noGravity)
                                    n.position.Y += (speedMult - 1) * n.velocity.Y;
                            }
                        }

                        if (Timer > 220)
                        {
                            frameY = 0;
                            SubState++;
                            Timer = 0;
                        }
                    }

                }
            }
            else if (SubState % 5 == 1) // jam some sentries
            {
                int whoAmI = EModeDD2Event.FindClosestDD2Sentry(npc.Center);
                if (whoAmI < 0) // no sentries
                {
                    SubState++;
                    Timer = 0;
                }
                else
                {
                    targetPos = crystal.Center + new Vector2(-npc.direction * DD2Event.ArenaHitbox.Width / 4, -200);
                    dist = npc.Distance(targetPos);

                    if (dist > 50)
                    {
                        Movement(npc, targetPos);
                        Timer = 0;
                    }
                    else
                    {
                        npc.direction = (int)npc.HorizontalDirectionTo(crystal.Center);
                        npc.velocity *= 0.9f;
                        frameY = Math.Min(3, (int)Math.Floor(Timer / 6f));

                        if (frameY == 3)
                        {
                            if (Timer == 30)
                            {
                                SoundEngine.PlaySound(SoundID.DD2_BetsyScream with { Volume = 0.5f }, npc.Center);
                            }
                            if (Timer % 15 == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Item94, npc.Center);
                                if (Timer < 120)
                                    RoarParticles(npc, Color.Purple);
                            }

                            if (Timer % 6 == 0)
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    float scale = Main.rand.NextFloat(1f, 3f);
                                    float randRot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                                    new ElectricSpark(npc.Center + 60 * Vector2.UnitX.RotatedBy(randRot), (4 - scale) * 15 * Vector2.UnitX.RotatedBy(randRot),
                                        Color.Purple, scale, 35).Spawn();
                                }
                            }

                            int range = DD2Event.ArenaHitbox.Width / 4;
                            foreach (var proj in Main.ActiveProjectiles)
                            {
                                if (ProjectileID.Sets.IsADD2Turret[proj.type] && npc.Distance(proj.Center) < range)
                                    proj.Eternity().Jammed = true;
                            }

                            if (Timer > 220)
                            {
                                frameY = 0;
                                SubState++;
                                Timer = 0;
                                foreach (var proj in Main.ActiveProjectiles)
                                {
                                    if (ProjectileID.Sets.IsADD2Turret[proj.type] && npc.Distance(proj.Center) < range)
                                        proj.Eternity().Jammed = false;
                                }
                            }
                        }
                    }

                }
            }
            else // fireball rain
            {
                targetPos = crystal.Center + new Vector2(-1200 * npc.direction, -600);
                dist = npc.Distance(targetPos);

                if (dist > 50 && frameY != 9)
                {
                    Movement(npc, targetPos);
                    Timer = 0;
                }
                else
                {
                    float divisor = WorldSavingSystem.MasochistModeReal ? 9f : 6f;
                    frameY = Math.Min(9, 4 + (int)Math.Floor(Timer / divisor));

                    if (frameY == 9)
                    {
                        npc.velocity = 30 * npc.direction * Vector2.UnitX;

                        int freq = WorldSavingSystem.MasochistModeReal ? 7 : 10;
                        if (Timer % freq == 0)
                        {
                            if (FargoSoulsUtil.HostCheck)
                            {
                                Projectile.NewProjectile(npc.GetSource_FromThis(), breathPos(npc), new(npc.direction * 2f, 0f), ProjectileID.DD2BetsyFireball, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 1f);
                            }
                        }

                        if (Timer > 120)
                        {
                            if (SubState % 5 == 4)
                            {
                                SubState++;
                                Timer = 0;
                            }
                            else
                            {
                                SubState++;
                                Timer = 0;
                                frameY = 0;
                                npc.direction = (int)npc.HorizontalDirectionTo(crystal.Center);
                            }
                        }
                    }
                    else
                    {
                        npc.velocity *= 0.9f;
                        npc.direction = (int)npc.HorizontalDirectionTo(crystal.Center);
                    }
                }
            }
        }

        private void DeathAnim(NPC npc)
        {
            npc.rotation = 0;
            ContactDamage = false;
            NPC crystal = EModeDD2Event.GetEterniaCrystal();

            if (crystal == null)
            {
                SpecialOnKill(npc);
                OnKill(npc);
                return;
            }

            Vector2 targetPos = crystal.Center - 400 * Vector2.UnitY;
            float dist = npc.Distance(targetPos);

            if (dist > 10)
            {
                Movement(npc, targetPos);
                Timer = 0;
            }
            else
            {
                npc.velocity *= 0.2f;

                Vector2 cPos = crystal.Center - crystal.height / 2 * Vector2.UnitY;
                float cRot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                new SparkParticle(cPos + 100 * Vector2.UnitX.RotatedBy(cRot), -10 * Vector2.UnitX.RotatedBy(cRot), Color.White, 0.3f, 9).Spawn();
                frameY = Math.Min(3, (int)Math.Floor(Timer / 6f));

                if (frameY == 3)
                {
                    if (Timer % 60 == 20)
                    {
                        RoarParticles(npc, Color.White);
                        SoundEngine.PlaySound(SoundID.DD2_BetsyScream, npc.Center);
                    }

                    new AlphaBloomParticle(cPos, Vector2.Zero, Color.White, Vector2.One, (Timer / 5f) * Vector2.One, 5).Spawn();
                    if (Timer > 180)
                    {
                        if (FargoSoulsUtil.HostCheck)
                        {
                            Projectile.NewProjectile(crystal.GetSource_FromThis(), cPos, -Vector2.UnitY, ModContent.ProjectileType<DD2CrystalDeathray>(), 0, 0, ai0: crystal.whoAmI);
                            npc.SimpleStrikeNPC(npc.lifeMax, 1);
                        }
                    }
                }
            }
        }
        #endregion

        #region Helper Methods
        private void Movement(NPC npc, Vector2 target, float traction = 1.2f, float maxSpeed = 50)
        {
            float accel = traction;
            float decel = traction;
            float resistance = npc.velocity.Length() * accel / maxSpeed;
            npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, target, npc.velocity, accel - resistance, decel + resistance);
        }

        private NPC FindClosestPortal(Vector2 pos)
        {
            if (!DD2Event.Ongoing)
                return null;

            int whoAmI = -1;
            float dist = -1;
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.type != NPCID.DD2LanePortal)
                    continue;

                float nDist = n.Distance(pos);
                if (dist == -1 || nDist < dist)
                {
                    dist = nDist;
                    whoAmI = n.whoAmI;
                }
            }
            if (whoAmI == -1)
                return null;

            return Main.npc[whoAmI];
        }

        private List<States> AvailableStates = new List<States>();

        private void ChooseAttack(NPC npc)
        {
            ResetState(npc);
            if (AvailableStates.Count == 0)
            {
                for (int i = 5; i <= 8; i++)
                {
                    if (PreviousState != i)
                    {
                        AvailableStates.Add((States)i);
                    }
                }
            }
            if (FargoSoulsUtil.HostCheck)
            {
                State = (int)Main.rand.NextFromCollection(AvailableStates);
                AvailableStates.Remove((States)State);
            }
            NetSync(npc);
        }

        private void ResetState(NPC npc)
        {
            Timer = -1;
            SubState = 0;
            wingRate = 2;
            if (heldProj != -1)
            {
                Main.projectile[heldProj].Kill();
                heldProj = -1;
            }
            NetSync(npc);
            npc.netUpdate = true;
        }

        private void ResetToIdle(NPC npc)
        {
            PreviousState = State;
            State = (int)States.Idle;
            ResetState(npc);
        }

        private Vector2 breathPos(NPC npc) => npc.Center + new Vector2(npc.direction * npc.width / 1.75f, npc.height / 3);
        #endregion

        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            if (DD2Event.Ongoing && npc.life - hit.Damage < 0 && !InPhase3)
            {
                hit.Null();
                npc.life = 10;
                npc.dontTakeDamage = true;
                InPhase3 = true;
                ResetToIdle(npc);
                return;
            }
            base.HitEffect(npc, hit);
        }

        public override bool SpecialOnKill(NPC npc)
        {
            npc.boss = false;

            return base.SpecialOnKill(npc);
        }

        public override void OnKill(NPC npc)
        {
            base.OnKill(npc);

            WorldSavingSystem.DownedBetsy = true;
        }
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasNPCTarget && !InPhase3)
            {
                modifiers.Null();
                SoundEngine.PlaySound(SoundID.NPCHit4, npc.Center);
            }
            base.ModifyIncomingHit(npc, ref modifiers);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.WitheredArmor, 180);
            target.AddBuff(BuffID.WitheredWeapon, 180);
        }

        #region Visuals
        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, NPCID.DD2Betsy);
            LoadBossHeadSprite(recolor, 34);
            LoadGoreRange(recolor, 1079, 1086);
            LoadExtra(recolor, 81);
            LoadExtra(recolor, 82);
            LoadGlowMask(recolor, 226);
            LoadProjectile(recolor, ProjectileID.DD2BetsyFireball);
            LoadProjectile(recolor, ProjectileID.DD2BetsyFlameBreath);
        }

        public void RoarParticles(NPC npc, Color color)
        {
            Vector2 mouthPos = npc.Center + new Vector2(npc.direction * npc.width / 1.65f, -npc.height / 6);

            float opac = npc.dontTakeDamage && !InPhase3 && State != (int)States.Spawn ? 0.1f : 1f;
            float count = 120;
            for (int i = 0; i < count; i++)
            {
                //float rot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                float rot = MathHelper.TwoPi * (i / count) + Main.rand.NextFloat(-0.02f, 0.02f);
                SparkParticle p = new SparkParticle(mouthPos, 60 * Vector2.UnitX.RotatedBy(rot), color * opac * 0.5f, 0.5f, 8);
                p.Spawn();
            }

        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.IsABestiaryIconDummy)
                return base.PreDraw(npc, spriteBatch, screenPos, drawColor);

            npc.frame.Y = npc.frame.Height * frameY;
            float invOpac = npc.dontTakeDamage && !InPhase3 && State != (int)States.Spawn ? 0.1f : 1f;

            // evil ass vanilla draw code
            Texture2D bodyTexture = TextureAssets.Npc[npc.type].Value;
            Vector2 npcToScreen = npc.Center - screenPos;
            Rectangle frame = npc.frame;
            _ = frame.Size() / 2f;
            SpriteEffects spriteEffects2 = npc.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float rotation8 = npc.rotation;
            Color color9 = drawColor;
            Color color10 = Color.Lerp(color9, Color.White, 0.6f);
            color10.A = 66;
            Vector2 vector72 = new Vector2(171f, 44f);
            Vector2 vector11 = new Vector2(230f, 52f);
            Vector2 vector12 = Vector2.Lerp(vector72, vector11, 0.5f) + new Vector2(-50f, 30f);
            int num38 = (int)npc.localAI[0] / 4;
            Vector2 spinningpoint = vector72 - vector12;
            Vector2 spinningpoint2 = vector11 - vector12;
            Texture2D value3 = TextureAssets.Extra[ExtrasID.DD2BetsyWingBack].Value;
            if (spriteEffects2.HasFlag(SpriteEffects.FlipHorizontally))
            {
                spinningpoint2.X *= -1f;
            }
            Rectangle value4 = value3.Frame(2, 5, wingFrame / 5, wingFrame % 5);
            Vector2 origin = new Vector2(16f, 176f);
            if (spriteEffects2.HasFlag(SpriteEffects.FlipHorizontally))
            {
                origin.X = (float)value4.Width - origin.X;
            }
            if (spriteEffects2.HasFlag(SpriteEffects.FlipHorizontally))
            {
                vector12.X = (float)frame.Width - vector12.X;
            }
            Texture2D frontWingTexture = TextureAssets.Extra[ExtrasID.DD2BetsyWingFront].Value;
            if (spriteEffects2.HasFlag(SpriteEffects.FlipHorizontally))
            {
                spinningpoint.X *= -1f;
            }
            Rectangle fwFrame = frontWingTexture.Frame(2, 5, wingFrame / 5, wingFrame % 5);
            Vector2 origin2 = new Vector2(215f, 170f);
            if (spriteEffects2.HasFlag(SpriteEffects.FlipHorizontally))
            {
                origin2.X = (float)fwFrame.Width - origin2.X;
            }
            float lerpValue = Utils.GetLerpValue(0f, 30f, npc.localAI[1], clamped: true);
            if (lerpValue == 1f)
            {
                lerpValue = Utils.GetLerpValue(60f, 30f, npc.localAI[1], clamped: true);
            }
            lerpValue = 2f;
            Vector2 vector13 = npc.Size / 2f - screenPos;
            int num39 = -3;
            int num40 = 0;
            byte b2 = 2;
            for (int n = 9; n > num40; n += num39)
            {
                Vector2 vector14 = npc.oldPos[n] + vector13;
                float num41 = npc.oldRot[n];
                Color color11 = color9 * (1f - (float)n / 10f) * 0.35f;
                color11.A /= b2;
                Main.EntitySpriteDraw(value3, vector14 + spinningpoint2.RotatedBy(num41), value4, color11, num41, origin, 1f, spriteEffects2, 0f);
                Main.EntitySpriteDraw(bodyTexture, vector14, frame, color11, num41, vector12, 1f, spriteEffects2, 0f);
                Main.EntitySpriteDraw(frontWingTexture, vector14 + spinningpoint.RotatedBy(num41), fwFrame, color11, num41, origin2, 1f, spriteEffects2, 0f);
            }
            Main.EntitySpriteDraw(value3, npcToScreen + spinningpoint2.RotatedBy(rotation8), value4, color9 * invOpac, rotation8, origin, 1f, spriteEffects2, 0f);
            Main.EntitySpriteDraw(bodyTexture, npcToScreen, frame, color9 * invOpac, rotation8, vector12, 1f, spriteEffects2, 0f);
            Main.EntitySpriteDraw(TextureAssets.GlowMask[226].Value, npcToScreen, frame, color10 * (0.7f + 0.3f * lerpValue), rotation8, vector12, 1f, spriteEffects2, 0f);
            Main.EntitySpriteDraw(frontWingTexture, npcToScreen + spinningpoint.RotatedBy(rotation8), fwFrame, color9 * invOpac, rotation8, origin2, 1f, spriteEffects2, 0f);
            return false;
        }
        #endregion
    }
}
