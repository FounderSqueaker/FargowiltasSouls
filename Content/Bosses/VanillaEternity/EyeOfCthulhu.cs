using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.EyeOfCthulhu;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.MoonLord;
using FargowiltasSouls.Core;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.Metrics;
using System.IO;
using System.Reflection.Metadata;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static System.TimeZoneInfo;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class EyeofCthulhu : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.EyeofCthulhu);

        public static bool recolor => SoulConfig.Instance.BossRecolors && WorldSavingSystem.EternityMode;

        public bool DroppedSummon;

        public int ServantAttackCounter;

        public bool FinalPhaseBerserkDashesComplete;
        public bool FinalPhaseDashHorizSpeedSet;
        public int FinalPhaseDashCD;
        public int FinalPhaseDashStageDuration;
        public int FinalPhaseAttackCounter;
        public int ScytheSpawnTimer;
        Vector2 targetCenter = Vector2.Zero;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(ServantAttackCounter);

            binaryWriter.Write(FinalPhaseBerserkDashesComplete);
            binaryWriter.Write(FinalPhaseDashHorizSpeedSet);
            binaryWriter.Write(FinalPhaseDashCD);
            binaryWriter.Write(FinalPhaseDashStageDuration);
            binaryWriter.Write(FinalPhaseAttackCounter);
            binaryWriter.Write(ScytheSpawnTimer);

            binaryWriter.WriteVector2(targetCenter);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            ServantAttackCounter = binaryReader.ReadInt32();

            FinalPhaseBerserkDashesComplete = binaryReader.ReadBoolean();
            FinalPhaseDashHorizSpeedSet = binaryReader.ReadBoolean();
            FinalPhaseDashCD = binaryReader.ReadInt32();
            FinalPhaseDashStageDuration = binaryReader.ReadInt32();
            FinalPhaseAttackCounter = binaryReader.ReadInt32();
            ScytheSpawnTimer = binaryReader.ReadInt32();
            targetCenter = binaryReader.ReadVector2();
    }
        public enum States
        {
            EyeShots,
            EyeShots2,
            Dashes,
            Servants,
            // p2
            SpinScytheDashes,
            HorizontalDashes,
            ServantSpin,
            ZigZag
        }
        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);
            if (npc.damage < 28)
                npc.damage = 28;
        }
        public NPC NPC;
        public Player Target => Main.player[NPC.target];
        public ref float Phase => ref NPC.ai[0];
        public ref float AttackState => ref NPC.ai[1];
        public ref float Timer => ref NPC.ai[2];
        public ref float AI3 => ref NPC.ai[3];
        public void Movement(Vector2 target, float maxSpeed = 20f, float speedMultiplier = 1f)
        {
            float accel = 0.4f * speedMultiplier;
            float decel = 0.7f * speedMultiplier;
            float resistance = NPC.velocity.Length() * accel / (maxSpeed * speedMultiplier);
            NPC.velocity = FargoSoulsUtil.SmartAccel(NPC.Center, target, NPC.velocity, accel - resistance, decel + resistance);
        }
        public void GoToState(States state)
        {
            if (FargoSoulsUtil.HostCheck)
            {
                AttackState = (int)state;
                if (Phase == 0 && NPC.GetLifePercent() <= 0.6f)
                {
                    AttackState = (int)States.SpinScytheDashes;
                }
                if (Phase > 0)
                {
                    if (ServantAttackCounter <= 0)
                    {
                        AttackState = (int)States.ServantSpin;
                    }
                    FinalPhaseAttackCounter++;
                    int counterMax = (int)MathF.Round(MathHelper.Lerp(2f, 5, NPC.GetLifePercent() / 0.6f));
                    if (FinalPhaseAttackCounter > counterMax)
                    {
                        AttackState = (int)States.ZigZag;
                    }
                }
            }
                
            Timer = 0;
            AI3 = 0;
            NPC.TargetClosest(false);
            NPC.netUpdate = true;
            NetSync(NPC);
        }
        public Vector2 Forward => (NPC.rotation + MathHelper.PiOver2).ToRotationVector2();
        public Vector2 EyeCenter => NPC.Center + Forward * NPC.height * 0.5f;
        public static int XAmount => WorldSavingSystem.MasochistModeReal ? 8 : 8;
        public static int DustType => recolor ? DustID.Vortex : DustID.BloodWater;
        public void DefaultRotation(Vector2? direction = null, float rotLerp = 0.05f)
        {
            if (direction == null)
                direction = NPC.DirectionTo(Target.Center);
            NPC.rotation = NPC.rotation.ToRotationVector2().RotateTowards(direction.Value.ToRotation() - MathHelper.PiOver2, rotLerp).ToRotation();
        }
        public override bool SafePreAI(NPC npc)
        {
            EModeGlobalNPC.eyeBoss = npc.whoAmI;
            NPC = npc;

            // Get a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Target.dead || !Target.active)
                NPC.TargetClosest();


            // Despawn
            if (!Target.active || Target.dead || Main.IsItDay())
            {
                NPC.TargetClosest(false);
                if (!Target.active || Target.dead || Main.IsItDay())
                {
                    NPC.velocity *= 0.95f;
                    NPC.velocity.Y -= 1f;

                    if (NPC.timeLeft > 60)
                        NPC.timeLeft = 60;

                    return false;
                }
            }
            else if (NPC.timeLeft < 1800)
                NPC.timeLeft = 1800;
            switch ((States)AttackState)
            {
                case States.EyeShots:
                    EyeShots();
                    break;
                case States.EyeShots2:
                    EyeShots2();
                    break;
                case States.Dashes:
                    Dashes();
                    break;
                case States.Servants:
                    Servants();
                    break;
                // p2
                case States.SpinScytheDashes:
                    SpinScytheDashes();
                    break;
                case States.HorizontalDashes:
                    HorizontalDashes();
                    break;
                case States.ServantSpin:
                    ServantSpin();
                    break;
                case States.ZigZag:
                    ZigZag();
                    break;
            }


            Lighting.AddLight(npc.Center, 0.75f / 3, 1.35f / 3, 1.5f / 3);

            EModeUtils.DropSummon(npc, ItemID.SuspiciousLookingEye, NPC.downedBoss1, ref DroppedSummon);

            return false;
        }
        public void EyeShots()
        {
            DefaultRotation();

            float distance = 380;


            Vector2 pos = Target.Center;
            float offsetDir = Target.DirectionTo(NPC.Center).ToRotation();
            Vector2 offset = offsetDir.ToRotationVector2() * distance;

            if (Math.Abs(FargoSoulsUtil.RotationDifference(offset, -Vector2.UnitY)) > MathHelper.PiOver2 * 0.7f)
                offset = offset.RotateTowards(-Vector2.UnitY.ToRotation(), 0.07f);
            pos += offset;
            float speed = 0.55f;
            if (Target.Distance(NPC.Center) < distance)
                speed /= 3;
            Movement(pos, maxSpeed: 90, speedMultiplier: speed);



            float shotTime = WorldSavingSystem.MasochistModeReal ? 60 * 1f : 60 * 2.5f;
            if (NPC.Distance(Target.Center) > distance + 400 && Timer < shotTime)
                Timer--;
            if (Timer >= shotTime - 60 && Timer < shotTime)
            {
                int index = Dust.NewDust(EyeCenter, 0, 0, DustType, 0.0f, 0.0f, 100, new Color(), 1f);
                Main.dust[index].noLight = true;
                Main.dust[index].noGravity = true;
                Main.dust[index].velocity = Forward.RotatedByRandom(MathHelper.PiOver2 * 0.3f) * Main.rand.NextFloat(5, 10);
            }
            float spread = WorldSavingSystem.MasochistModeReal ? 1f : 1.2f;
            if (Timer == (int)shotTime)
            {
                Vector2 projCenter = EyeCenter;
                SoundEngine.PlaySound(SoundID.Item124, projCenter);
                if (FargoSoulsUtil.HostCheck)
                {
                    Vector2 vel = Forward;
                    for (int i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), projCenter,
                            1.5f * vel.RotatedBy(spread * Math.PI / 2 / 2 * i),
                            ModContent.ProjectileType<PhantasmalBoltEoC>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer);
                    }
                }
            }
            if (Timer == (int)shotTime + 4)
            {
                Vector2 projCenter = EyeCenter;
                SoundEngine.PlaySound(SoundID.Item125, projCenter);
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 vel = Forward;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), projCenter,
                        1.25f * vel.RotatedBy(spread * 0.5f * Math.PI / 2 / 2 * i),
                        ModContent.ProjectileType<PhantasmalBoltEoC>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer);
                }
            }
            float endTime = WorldSavingSystem.MasochistModeReal ? 60 * 2f : 60 * 3.5f;
            if (++Timer > (int)endTime)
            {
                GoToState(States.Dashes);
                return;
            }
        }
        public void EyeShots2()
        {
            float shotTime = 60 * 1.5f;
            if (Timer == (int)shotTime)
            {
                Vector2 projCenter = EyeCenter;
                SoundEngine.PlaySound(SoundID.Item124, projCenter);


            }
        }
        public void Dashes()
        {
            DefaultRotation(rotLerp: 0.2f);

            int dashTime = WorldSavingSystem.MasochistModeReal ? 48 : 60;
            int dashes = WorldSavingSystem.MasochistModeReal ? 4 : 3;
            float endTime = dashTime * dashes - 2;

            if (Timer % dashTime == 0)
            {
                int counter = (int)(Timer / dashTime);
                counter = (int)MathHelper.Clamp(counter, 0, 2);
                NPC.velocity = Forward * (13 + 6 * counter);
            }
            else
            {
                if (Timer % dashTime <= 25)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int index = Dust.NewDust(NPC.Center, 0, 0, DustType, 0.0f, 0.0f, 100, new Color(), 1f);
                        Main.dust[index].position += Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2);
                        Main.dust[index].noLight = true;
                        Main.dust[index].noGravity = true;
                        Main.dust[index].velocity = -NPC.velocity / 2;
                    }
                }
                NPC.velocity *= 0.975f;
            }

            if (++Timer > endTime)
            {
                States state = States.EyeShots;
                if (++ServantAttackCounter >= 2)
                {
                    state = States.Servants;
                    ServantAttackCounter = 0;
                }
                GoToState(state);
            }
        }
        public void Servants()
        {
            DefaultRotation();
            if (AI3 == 0)
            {
                AI3 = Main.rand.NextBool() ? 1 : -1;
                NPC.netUpdate = true;
                NetSync(NPC);
            }
                

            float distance = 380;

            Vector2 pos = Target.Center;
            Vector2 offset = Target.DirectionTo(NPC.Center) * distance;
            offset = offset.RotatedBy(MathF.Tau * 0.05f * AI3);
            pos += offset;
            float speed = 0.2f;
            Movement(pos, maxSpeed: 32, speedMultiplier: speed);

            int freq = WorldSavingSystem.MasochistModeReal ? 52 : 65;
            if (Timer % freq == freq - 5)
            {
                if (FargoSoulsUtil.HostCheck)
                {
                    int n = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.ServantofCthulhu);
                    if (n != Main.maxNPCs)
                    {
                        Main.npc[n].velocity = Forward.RotatedByRandom(MathHelper.PiOver2 * 0.8f);
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                    }
                }
            }


            float endTime = 60 * 4.5f;
            if (++Timer > endTime)
            {
                States state = States.EyeShots;
                GoToState(state);
                return;
            }
        }
        // p2
        public void SpinScytheDashes()
        {
            // phase transition spin
            int transitionTime = 60 * 3;
            if (Timer < transitionTime)
            {
                for (int i = 0; i < 3; i++)
                {
                    int index = Dust.NewDust(NPC.Center, 0, 0, DustType, 0.0f, 0.0f, 100, new Color(), 1f);
                    Main.dust[index].position += Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2);
                    Main.dust[index].noLight = true;
                    Main.dust[index].noGravity = true;
                    Main.dust[index].velocity = (Main.dust[index].position - NPC.Center) / 8;
                }

                float distance = 420;

                Vector2 pos = Target.Center;
                float offsetDir = Target.DirectionTo(NPC.Center).ToRotation();
                Vector2 offset = offsetDir.ToRotationVector2() * distance;

                offset = offset.RotateTowards(-Vector2.UnitY.ToRotation(), 0.09f);
                pos += offset;
                float speed = 1.3f;

                Movement(pos, maxSpeed: 35, speedMultiplier: speed);

                float rotationSpeed = MathHelper.Lerp(0, 0.8f, Timer / transitionTime);
                int phaser = 90;
                if (WorldSavingSystem.MasochistModeReal)
                    phaser = 60;
                if (Timer % phaser == phaser - 1)
                {
                    if (Phase < 2)
                        Phase++;

                    if (FargoSoulsUtil.HostCheck)
                    {
                        Projectile[] projs = FargoSoulsUtil.XWay(XAmount, NPC.GetSource_FromThis(), NPC.Center, ModContent.ProjectileType<BloodScythe>(), 1.5f, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0);
                        float rot = Main.rand.NextFloat(MathF.Tau);
                        foreach (var p in projs)
                            p.velocity = p.velocity.RotatedBy(rot);
                    }
                        
                }
                NPC.rotation += rotationSpeed;
            }
            else
            {
                int dashTime = WorldSavingSystem.MasochistModeReal ? 65 : 82;
                int dashes = WorldSavingSystem.MasochistModeReal ? 4 : 3;
                int endlag = 32;

                int endlagStart = transitionTime + dashTime * dashes - 2;
                if (Timer < endlagStart)
                {
                    if ((Timer - transitionTime) % dashTime == 0)
                    {
                        int counter = (int)((Timer - transitionTime) / dashTime);
                        counter = (int)MathHelper.Clamp(counter, 0, 2);
                        NPC.velocity = NPC.DirectionTo(Target.Center) * (16 + 5 * counter);
                        SoundEngine.PlaySound(SoundID.ForceRoar);
                    }
                    DefaultRotation(rotLerp: 0.2f);
                    NPC.velocity *= 0.98f;
                    if ((Timer - transitionTime) % dashTime < 25)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int index = Dust.NewDust(NPC.Center, 0, 0, DustType, 0.0f, 0.0f, 100, new Color(), 1f);
                            Main.dust[index].position += Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2);
                            Main.dust[index].noLight = true;
                            Main.dust[index].noGravity = true;
                            Main.dust[index].velocity = -NPC.velocity / 2;
                        }
                    }
                }
                else
                {
                    NPC.velocity *= 0.96f;
                    if (Timer > endlagStart + endlag)
                    {
                        ServantAttackCounter--;
                        GoToState(States.HorizontalDashes);
                        return;
                    }
                }
            }
            Timer++;
        }
        public void HorizontalDashes()
        {
            int dashStart = 60;
            int dashDuration = 40;
            int dashTime = dashStart + dashDuration;
            int extraStartup = 15;
            int extraEndlag = 32;
            int finalDashTime = dashTime + extraStartup + extraEndlag;
            DefaultRotation(rotLerp: 0.2f);

            int distance = 500;
            if (AI3 == 0)
            {
                AI3 = Main.rand.Next(2, 5);
                NPC.netUpdate = true;
                NetSync(NPC);
            }
            int dashes = (int)AI3;
            if (Timer < dashTime * dashes)
            {
                int subTimer = (int)(Timer % dashTime);
                if (subTimer < dashStart)
                {
                    Vector2 pos = Target.Center;
                    Vector2 offset = Target.HorizontalDirectionTo(NPC.Center) * Vector2.UnitX * distance;
                    float speed = 2f;
                    Movement(pos + offset, maxSpeed: 32, speedMultiplier: speed);
                }
                else if (subTimer == dashStart)
                {
                    NPC.velocity = Forward * 18;
                    SoundEngine.PlaySound(SoundID.ForceRoarPitched);

                    if (NPC.CountNPCS(NPCID.ServantofCthulhu) < 4 && FargoSoulsUtil.HostCheck)
                    {
                        int n = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.ServantofCthulhu);
                        if (n != Main.maxNPCs)
                        {
                            Main.npc[n].velocity = -NPC.DirectionTo(Target.Center).RotatedByRandom(MathHelper.PiOver2 * 0.8f) * 1;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                        }
                    }

                    if (WorldSavingSystem.MasochistModeReal && FargoSoulsUtil.HostCheck)
                    {
                        Projectile[] projs = FargoSoulsUtil.XWay(XAmount, NPC.GetSource_FromThis(), NPC.Center, ModContent.ProjectileType<BloodScythe>(), 1.5f, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0);
                        float rot = Main.rand.NextFloat(MathF.Tau);
                        foreach (var p in projs)
                            p.velocity = p.velocity.RotatedBy(rot);
                    }
                }
                else if (subTimer <= dashStart + 25)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int index = Dust.NewDust(NPC.Center, 0, 0, DustType, 0.0f, 0.0f, 100, new Color(), 1f);
                        Main.dust[index].position += Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2);
                        Main.dust[index].noLight = true;
                        Main.dust[index].noGravity = true;
                        Main.dust[index].velocity = -NPC.velocity / 2;
                    }
                }

                if (subTimer > dashTime - 5 && Math.Abs(FargoSoulsUtil.RotationDifference(NPC.velocity, NPC.DirectionTo(Target.Center))) < MathHelper.PiOver2)
                {
                    Timer--;
                    if (NPC.velocity.LengthSquared() < 35 * 35)
                        NPC.velocity *= 1.03f;
                }
            }
            else // final dash
            {
                int subTimer = (int)(Timer - (dashTime * dashes));
                if (subTimer < dashStart + extraStartup)
                {
                    Vector2 pos = Target.Center;
                    Vector2 offset = -Vector2.UnitY * distance;
                    float speed = 3f;
                    Movement(pos + offset, maxSpeed: 36, speedMultiplier: speed);
                }
                else if (subTimer == dashStart + extraStartup)
                {
                    NPC.velocity = Forward * 18;
                    SoundEngine.PlaySound(SoundID.ForceRoarPitched);

                    for (int i = -1; i <= 1; i += 2)
                    {
                        int n = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.ServantofCthulhu);
                        if (n != Main.maxNPCs)
                        {
                            Main.npc[n].velocity = NPC.DirectionTo(Target.Center).RotatedBy(MathHelper.PiOver2 * 0.8f * i) * 5;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                        }
                    }

                    if (WorldSavingSystem.MasochistModeReal && FargoSoulsUtil.HostCheck)
                    {
                        Projectile[] projs = FargoSoulsUtil.XWay(XAmount, NPC.GetSource_FromThis(), NPC.Center, ModContent.ProjectileType<BloodScythe>(), 1.5f, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0);
                        float rot = Main.rand.NextFloat(MathF.Tau);
                        foreach (var p in projs)
                            p.velocity = p.velocity.RotatedBy(rot);
                    }
                }
                else
                {
                    if (subTimer <= dashStart + extraStartup + 25)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int index = Dust.NewDust(NPC.Center, 0, 0, DustType, 0.0f, 0.0f, 100, new Color(), 1f);
                            Main.dust[index].position += Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2);
                            Main.dust[index].noLight = true;
                            Main.dust[index].noGravity = true;
                            Main.dust[index].velocity = -NPC.velocity / 2;
                        }
                    }
                    NPC.velocity *= 0.99f;
                }
                if (subTimer > finalDashTime - extraEndlag)
                    NPC.velocity *= 0.96f;
                if (subTimer > finalDashTime)
                {
                    ServantAttackCounter--;
                    GoToState(States.SpinScytheDashes);
                    return;
                }
            }
            Timer++;
        }
        public void ServantSpin()
        {
            if (AI3 == 0)
            {
                AI3 = Main.rand.NextBool() ? 1 : -1;
                NPC.netUpdate = true;
                NetSync(NPC);
            }
            Vector2 dir = Vector2.Lerp(Forward, NPC.velocity, LumUtils.Saturate(Timer / 120));
            DefaultRotation(dir, 0.2f);

            float spd = MathHelper.Lerp(0.5f, 12f, LumUtils.Saturate(Timer / 70));
            if (Timer == 70)
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            Vector2 pos = Target.Center;
            Vector2 offset = Target.DirectionTo(NPC.Center) * 450;
            offset = offset.RotatedBy(MathF.Tau * 0.05f * AI3);
            pos += offset;

            int maxTime = 60 * 4;
            int endTime = 60;
            int endStart = maxTime - endTime;
            if (Timer > endStart)
            {
                spd *= MathHelper.Lerp(1, 0, (Timer - endStart) / endTime);
            }
            if (spd > 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    int index = Dust.NewDust(NPC.Center, 0, 0, DustType, 0.0f, 0.0f, 100, new Color(), 1f);
                    Main.dust[index].position += Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2);
                    Main.dust[index].noLight = true;
                    Main.dust[index].noGravity = true;
                    Main.dust[index].velocity = -NPC.velocity / 2;
                }
            }
            if (Timer < maxTime)
            {
                Movement(pos, maxSpeed: 32, speedMultiplier: spd);

                int freq = WorldSavingSystem.MasochistModeReal ? 38 : 48;
                if (Timer % freq == freq - 3)
                {
                    if (FargoSoulsUtil.HostCheck)
                    {
                        int n = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.ServantofCthulhu);
                        if (n != Main.maxNPCs)
                        {
                            Main.npc[n].velocity = -NPC.DirectionTo(Target.Center).RotatedByRandom(MathHelper.PiOver2 * 0.8f) * 3;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                        }
                    }
                }
            }
            else
            {
                ServantAttackCounter = Main.rand.Next(2, 5);
                GoToState(States.HorizontalDashes);
                return;
            }

            Timer++;
        }
        public void ZigZag()
        {
            NPC npc = NPC;

            if (AI3 == 0) // startup
            {
                npc.velocity *= 0.98f;
                npc.alpha += 8;
                for (int i = 0; i < 3; i++)
                {
                    int d = Dust.NewDust(npc.position, npc.width, npc.height, DustType, 0f, 0f, 0, default, 1.5f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                    Main.dust[d].velocity *= 4f;
                }
                if (npc.alpha > 255)
                {
                    npc.alpha = 255;
                    AI3 = 1;

                    SoundEngine.PlaySound(SoundID.Roar, npc.HasValidTarget ? Main.player[npc.target].Center : npc.Center);

                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, npc.whoAmI, npc.type);
                }
                return;
            }

            if (WorldSavingSystem.MasochistModeReal && ScytheSpawnTimer > 0)
            {
                if (ScytheSpawnTimer % 2 == 0 && FargoSoulsUtil.HostCheck)
                {
                    int p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<BloodScythe>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 1f, Main.myPlayer);
                    if (p != Main.maxProjectiles)
                        Main.projectile[p].timeLeft = 120;
                }
                ScytheSpawnTimer--;
            }

            const float speedModifier = 0.3f;

            if (npc.HasValidTarget && !Main.IsItDay())
            {
                if (npc.timeLeft < 300)
                    npc.timeLeft = 300;
            }
            else //despawn and retarget
            {
                npc.TargetClosest(false);
                npc.velocity.X *= 0.98f;
                npc.velocity.Y -= npc.velocity.Y > 0 ? 1f : 0.25f;

                if (npc.timeLeft > 30)
                    npc.timeLeft = 30;

                Timer = 90;
                FinalPhaseDashCD = 0;
                FinalPhaseBerserkDashesComplete = true;
                FinalPhaseDashHorizSpeedSet = false;
                FinalPhaseAttackCounter = 0;

                npc.alpha = 0;

                const float PI = (float)Math.PI;
                if (npc.rotation > PI)
                    npc.rotation -= 2 * PI;
                if (npc.rotation < -PI)
                    npc.rotation += 2 * PI;

                float targetRotation = npc.SafeDirectionTo(Main.player[npc.target].Center).ToRotation() - PI / 2;
                if (targetRotation > PI)
                    targetRotation -= 2 * PI;
                if (targetRotation < -PI)
                    targetRotation += 2 * PI;
                npc.rotation = MathHelper.Lerp(npc.rotation, targetRotation, 0.07f);
            }

            if (++Timer == 1) //teleport to random position
            {
                if (FargoSoulsUtil.HostCheck)
                {
                    npc.Center = Main.player[npc.target].Center;
                    npc.position.X += Main.rand.NextBool() ? -600 : 600;
                    npc.position.Y += Main.rand.NextBool() ? -400 : 400;

                    if (WorldSavingSystem.MasochistModeReal)
                        npc.position.X += Main.rand.Next(-100, 100); //1.6.1 change: random offset

                    npc.TargetClosest(false);
                    npc.netUpdate = true;
                    NetSync(npc);

                    Timer = 50; //1.6.1 change: skip most of windup

                    if (npc.HasValidTarget) //1.6.1 change: telegraph with spectral EoC clone
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<SpectralEoC>(), 0, 0, Main.myPlayer, Timer + 20, npc.target);
                }

                if (npc.HasValidTarget)
                    targetCenter = Main.player[npc.target].Center;
                else
                    targetCenter = npc.Center;
            }
            else if (Timer < 90) //fade in
            {

                npc.alpha -= WorldSavingSystem.MasochistModeReal ? 30 : 25;
                if (npc.alpha < 0)
                {
                    npc.alpha = 0;
                }

                const float PI = (float)Math.PI;
                if (npc.rotation > PI)
                    npc.rotation -= 2 * PI;
                if (npc.rotation < -PI)
                    npc.rotation += 2 * PI;

                float targetRotation = npc.SafeDirectionTo(targetCenter).ToRotation() - PI / 2;
                if (targetRotation > PI)
                    targetRotation -= 2 * PI;
                if (targetRotation < -PI)
                    targetRotation += 2 * PI;
                npc.rotation = MathHelper.Lerp(npc.rotation, targetRotation, 0.3f);

                for (int i = 0; i < 3; i++)
                {
                    int d = Dust.NewDust(npc.position, npc.width, npc.height, DustType, 0f, 0f, 0, default, 1.5f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                    Main.dust[d].velocity *= 4f;
                }

                Vector2 target = targetCenter;
                target.X += npc.Center.X < target.X ? -600 : 600;
                target.Y += npc.Center.Y < target.Y ? -400 : 400;

                npc.velocity = Vector2.Zero;
            }
            else if (!FinalPhaseBerserkDashesComplete) //berserk dashing phase
            {
                Timer = 90;

                const float xSpeed = 18f;
                const float ySpeed = 40f;

                if (++FinalPhaseDashCD == 1)
                {
                    SoundEngine.PlaySound(SoundID.ForceRoarPitched, targetCenter);

                    if (!FinalPhaseDashHorizSpeedSet) //only set this on the first dash of each set
                    {
                        FinalPhaseDashHorizSpeedSet = true;
                        npc.velocity.X = npc.Center.X < targetCenter.X ? xSpeed : -xSpeed;
                    }

                    npc.velocity.Y = npc.Center.Y < targetCenter.Y ? ySpeed : -ySpeed; //alternate this every dash

                    ScytheSpawnTimer = 30;
                    //if (WorldSavingSystem.MasochistModeReal)
                    //    SpawnServants();
                    if (FargoSoulsUtil.HostCheck)
                        FargoSoulsUtil.XWay(XAmount, npc.GetSource_FromThis(), npc.Center, ModContent.ProjectileType<BloodScythe>(), 1f, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0);

                    npc.netUpdate = true;
                }
                else if (FinalPhaseDashCD > 20)
                {
                    FinalPhaseDashCD = 0;
                }


                if (++FinalPhaseDashStageDuration > 600 * 3 / xSpeed + 5) //proceed
                {
                    ScytheSpawnTimer = 0;
                    FinalPhaseDashStageDuration = 0;
                    FinalPhaseBerserkDashesComplete = true;
                    npc.velocity *= 0.75f;
                    npc.netUpdate = true;
                }

                const float PI = (float)Math.PI;
                npc.rotation = npc.velocity.ToRotation() - PI / 2;
                if (npc.rotation > PI)
                    npc.rotation -= 2 * PI;
                if (npc.rotation < -PI)
                    npc.rotation += 2 * PI;
            }
            else
            {
                int threshold = 180;
                if (Timer < threshold / 3)
                    NPC.velocity *= 0.98f;
                else
                    Movement(Target.Center + Target.DirectionTo(NPC.Center) * 400, speedMultiplier: 1.4f);

                const float PI = (float)Math.PI;
                float targetRotation = MathHelper.WrapAngle(npc.SafeDirectionTo(Main.player[npc.target].Center).ToRotation() - PI / 2);
                npc.rotation = MathHelper.WrapAngle(MathHelper.Lerp(npc.rotation, targetRotation, 0.07f));

                if (npc.alpha > 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int d = Dust.NewDust(npc.position, npc.width, npc.height, DustType, 0f, 0f, 0, default, 1.5f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].noLight = true;
                        Main.dust[d].velocity *= 4f;
                    }
                }

                if (Timer > threshold) //reset
                {
                    Timer = 0;
                    FinalPhaseDashCD = 0;
                    FinalPhaseBerserkDashesComplete = false;
                    FinalPhaseDashHorizSpeedSet = false;
                    npc.velocity = Vector2.Zero;
                    npc.netUpdate = true;

                    FinalPhaseAttackCounter = 0;
                    GoToState(States.SpinScytheDashes);
                }
            }

            if (npc.netUpdate)
            {
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                    NetSync(npc);
                }
                npc.netUpdate = false;
            }
        }
        /*
        public override bool SafePreAI(NPC npc)
        {
            ref float ai_Phase = ref npc.ai[0];
            ref float ai_AttackState = ref npc.ai[1];
            ref float ai_Timer = ref npc.ai[2];
            EModeGlobalNPC.eyeBoss = npc.whoAmI;

            void SpawnServants()
            {
                if (npc.life <= npc.lifeMax * 0.65 && NPC.CountNPCS(NPCID.ServantofCthulhu) < 7 && FargoSoulsUtil.HostCheck)
                {
                    Vector2 vel = new(3, 3);
                    for (int i = 0; i < 3; i++)
                    {
                        int n = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.ServantofCthulhu);
                        if (n != Main.maxNPCs)
                        {
                            Main.npc[n].velocity = vel.RotatedBy(Math.PI / 2 * i);
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                        }
                    }
                }
            }

            //npc.dontTakeDamage = npc.alpha > 50;
            if (npc.alpha > 50)
                Lighting.AddLight(npc.Center, 0.75f, 1.35f, 1.5f);

            if (ScytheSpawnTimer > 0)
            {
                if (ScytheSpawnTimer % (IsInFinalPhase ? 2 : 6) == 0 && FargoSoulsUtil.HostCheck)
                {
                    if (IsInFinalPhase && !WorldSavingSystem.MasochistModeReal)
                    {
                        int p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<BloodScythe>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 1f, Main.myPlayer);
                        if (p != Main.maxProjectiles)
                            Main.projectile[p].timeLeft = 75;
                    }
                    else
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Normalize(npc.velocity), ModContent.ProjectileType<BloodScythe>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 1f, Main.myPlayer);
                    }
                }
                ScytheSpawnTimer--;
            }

            if (ai_Phase == 0f) //p1
            {
                //Faster speed, even faster when far
                float modifier = 0.15f;
                if (npc.HasValidTarget)
                    modifier = MathHelper.Lerp(0.15f, 0.5f, Math.Clamp(npc.Distance(Main.player[npc.target].Center) / 1000f, 0, 1));
                npc.position += npc.velocity * modifier;

                //Faster consecutive dashes
                if (ai_AttackState == 2f)
                    npc.position += npc.ai[3] * 0.3f * npc.velocity;
            }
            if ((ai_Phase == 0f || ai_Phase == 3f) && ai_AttackState == 2f && !IsInFinalPhase) // Faster consecutive dashes in p1 and p2
            {
                float modifier = ai_Phase == 0 ? 0.25f : 0.5f; // more increase in p2
                npc.position += npc.ai[3] * modifier * npc.velocity;
            }


            if (ai_Phase == 0f && ai_AttackState == 2f && npc.HasValidTarget && WorldSavingSystem.MasochistModeReal) // Dashes curve in phase 1 done
            {
                float speed = npc.velocity.Length();
                float modifier = 0.25f;
                npc.velocity += npc.SafeDirectionTo(Main.player[npc.target].Center) * modifier;
                npc.velocity = Vector2.Normalize(npc.velocity) * speed;
            }

            if (ai_Phase == 0f && ai_AttackState == 2f && ai_Timer == 0f)
            {
                ScytheSpawnTimer = 30;

            }

            if (ai_AttackState == 3f && !IsInFinalPhase) //during dashes in phase 2
            {
                if (WorldSavingSystem.MasochistModeReal)
                {
                    ScytheSpawnTimer = 30;
                    SpawnServants();
                }

                if (!ScytheRingIsOnCD)
                {
                    ScytheRingIsOnCD = true;
                    if (FargoSoulsUtil.HostCheck)
                        FargoSoulsUtil.XWay(XAmount, npc.GetSource_FromThis(), npc.Center, ModContent.ProjectileType<BloodScythe>(), 1.5f, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0);
                }
            }
            else
            {
                ScytheRingIsOnCD = false; //hacky fix for scythe spam during p2 transition
            }

            if (npc.life < npc.lifeMax / 2)
            {
                if (IsInFinalPhase) //final phase
                {
                    const float speedModifier = 0.3f;

                    if (npc.HasValidTarget && (!Main.dayTime || Main.zenithWorld || Main.remixWorld))
                    {
                        if (npc.timeLeft < 300)
                            npc.timeLeft = 300;
                    }
                    else //despawn and retarget
                    {
                        npc.TargetClosest(false);
                        npc.velocity.X *= 0.98f;
                        npc.velocity.Y -= npc.velocity.Y > 0 ? 1f : 0.25f;

                        if (npc.timeLeft > 30)
                            npc.timeLeft = 30;

                        AITimer = 90;
                        FinalPhaseDashCD = 0;
                        FinalPhaseBerserkDashesComplete = true;
                        FinalPhaseDashHorizSpeedSet = false;
                        FinalPhaseAttackCounter = 0;

                        npc.alpha = 0;

                        const float PI = (float)Math.PI;
                        if (npc.rotation > PI)
                            npc.rotation -= 2 * PI;
                        if (npc.rotation < -PI)
                            npc.rotation += 2 * PI;

                        float targetRotation = npc.SafeDirectionTo(Main.player[npc.target].Center).ToRotation() - PI / 2;
                        if (targetRotation > PI)
                            targetRotation -= 2 * PI;
                        if (targetRotation < -PI)
                            targetRotation += 2 * PI;
                        npc.rotation = MathHelper.Lerp(npc.rotation, targetRotation, 0.07f);
                    }

                    if (++AITimer == 1) //teleport to random position
                    {
                        if (FargoSoulsUtil.HostCheck)
                        {
                            npc.Center = Main.player[npc.target].Center;
                            npc.position.X += Main.rand.NextBool() ? -600 : 600;
                            npc.position.Y += Main.rand.NextBool() ? -400 : 400;

                            if (WorldSavingSystem.MasochistModeReal)
                                npc.position.X += Main.rand.Next(-100, 100); //1.6.1 change: random offset

                            npc.TargetClosest(false);
                            npc.netUpdate = true;
                            NetSync(npc);

                            AITimer = 40; //1.6.1 change: skip most of windup

                            if (npc.HasValidTarget) //1.6.1 change: telegraph with spectral EoC clone
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<SpectralEoC>(), 0, 0, Main.myPlayer, AITimer + 20, npc.target);
                        }

                        if (npc.HasValidTarget)
                            targetCenter = Main.player[npc.target].Center;
                        else
                            targetCenter = npc.Center;
                    }
                    else if (AITimer < 90) //fade in
                    {

                        npc.alpha -= WorldSavingSystem.MasochistModeReal ? 30 : 25;
                        if (npc.alpha < 0)
                        {
                            npc.alpha = 0;
                        }

                        const float PI = (float)Math.PI;
                        if (npc.rotation > PI)
                            npc.rotation -= 2 * PI;
                        if (npc.rotation < -PI)
                            npc.rotation += 2 * PI;

                        float targetRotation = npc.SafeDirectionTo(targetCenter).ToRotation() - PI / 2;
                        if (targetRotation > PI)
                            targetRotation -= 2 * PI;
                        if (targetRotation < -PI)
                            targetRotation += 2 * PI;
                        npc.rotation = MathHelper.Lerp(npc.rotation, targetRotation, 0.3f);

                        for (int i = 0; i < 3; i++)
                        {
                            int d = Dust.NewDust(npc.position, npc.width, npc.height, recolor? DustID.Vortex : DustID.BloodWater, 0f, 0f, 0, default, 1.5f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].noLight = true;
                            Main.dust[d].velocity *= 4f;
                        }

                        Vector2 target = targetCenter;
                        target.X += npc.Center.X < target.X ? -600 : 600;
                        target.Y += npc.Center.Y < target.Y ? -400 : 400;

                        npc.velocity = Vector2.Zero;
                    }
                    else if (!FinalPhaseBerserkDashesComplete) //berserk dashing phase
                    {
                        AITimer = 90;

                        const float xSpeed = 18f;
                        const float ySpeed = 40f;

                        if (++FinalPhaseDashCD == 1)
                        {
                            SoundEngine.PlaySound(SoundID.ForceRoarPitched, targetCenter);

                            if (!FinalPhaseDashHorizSpeedSet) //only set this on the first dash of each set
                            {
                                FinalPhaseDashHorizSpeedSet = true;
                                npc.velocity.X = npc.Center.X < targetCenter.X ? xSpeed : -xSpeed;
                            }

                            npc.velocity.Y = npc.Center.Y < targetCenter.Y ? ySpeed : -ySpeed; //alternate this every dash

                            ScytheSpawnTimer = 30;
                            //if (WorldSavingSystem.MasochistModeReal)
                            //    SpawnServants();
                            if (FargoSoulsUtil.HostCheck)
                                FargoSoulsUtil.XWay(XAmount, npc.GetSource_FromThis(), npc.Center, ModContent.ProjectileType<BloodScythe>(), 1f, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0);

                            npc.netUpdate = true;
                        }
                        else if (FinalPhaseDashCD > 20)
                        {
                            FinalPhaseDashCD = 0;
                        }


                        if (++FinalPhaseDashStageDuration > 600 * 3 / xSpeed + 5) //proceed
                        {
                            ScytheSpawnTimer = 0;
                            FinalPhaseDashStageDuration = 0;
                            FinalPhaseBerserkDashesComplete = true;
                            if (!WorldSavingSystem.MasochistModeReal)
                                FinalPhaseAttackCounter++;
                            npc.velocity *= 0.75f;
                            npc.netUpdate = true;
                        }

                        const float PI = (float)Math.PI;
                        npc.rotation = npc.velocity.ToRotation() - PI / 2;
                        if (npc.rotation > PI)
                            npc.rotation -= 2 * PI;
                        if (npc.rotation < -PI)
                            npc.rotation += 2 * PI;
                    }
                    else
                    {
                        bool mustRest = FinalPhaseAttackCounter >= 3;

                        const int restingTime = 240;

                        int threshold = 180;
                        if (mustRest)
                            threshold += restingTime;

                        if (mustRest && AITimer < restingTime + 90)
                        {
                            if (AITimer == 91)
                                npc.velocity = npc.SafeDirectionTo(Main.player[npc.target].Center) * npc.velocity.Length() * 0.75f;

                            npc.velocity.X *= 0.98f;
                            if (Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) < 300)
                                npc.velocity.X *= 0.9f;

                            bool floatUp = Collision.SolidCollision(npc.position, npc.width, npc.height);
                            if (!floatUp && npc.Bottom.X > 0 && npc.Bottom.X < Main.maxTilesX * 16 && npc.Bottom.Y > 0 && npc.Bottom.Y < Main.maxTilesY * 16)
                            {
                                Tile tile = Framing.GetTileSafely(npc.Bottom);
                                if (tile != null && tile.HasUnactuatedTile)
                                    floatUp = Main.tileSolid[tile.TileType];
                            }

                            if (floatUp)
                            {
                                npc.velocity.X *= 0.95f;

                                npc.velocity.Y -= speedModifier;
                                if (npc.velocity.Y > 0)
                                    npc.velocity.Y = 0;
                                if (Math.Abs(npc.velocity.Y) > 24)
                                    npc.velocity.Y = 24 * Math.Sign(npc.velocity.Y);
                            }
                            else
                            {
                                npc.velocity.Y += speedModifier;
                                if (npc.velocity.Y < 0)
                                    npc.velocity.Y += speedModifier * 2;
                                if (npc.velocity.Y > 15)
                                    npc.velocity.Y = 15;
                            }
                        }
                        else
                        {
                            npc.alpha += WorldSavingSystem.MasochistModeReal ? 16 : 4;
                            if (npc.alpha > 255)
                            {
                                npc.alpha = 255;
                                if (WorldSavingSystem.MasochistModeReal && AITimer < threshold)
                                    AITimer = threshold;
                            }

                            if (mustRest)
                            {
                                npc.velocity.Y -= speedModifier * 0.5f;
                                if (npc.velocity.Y > 0)
                                    npc.velocity.Y = 0;
                                if (Math.Abs(npc.velocity.Y) > 24)
                                    npc.velocity.Y = 24 * Math.Sign(npc.velocity.Y);
                            }
                            else
                            {
                                npc.velocity *= 0.98f;
                            }
                        }

                        const float PI = (float)Math.PI;
                        float targetRotation = MathHelper.WrapAngle(npc.SafeDirectionTo(Main.player[npc.target].Center).ToRotation() - PI / 2);
                        npc.rotation = MathHelper.WrapAngle(MathHelper.Lerp(npc.rotation, targetRotation, 0.07f));

                        if (npc.alpha > 0)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                int d = Dust.NewDust(npc.position, npc.width, npc.height, recolor ? DustID.Vortex : DustID.BloodWater, 0f, 0f, 0, default, 1.5f);
                                Main.dust[d].noGravity = true;
                                Main.dust[d].noLight = true;
                                Main.dust[d].velocity *= 4f;
                            }
                        }

                        if (AITimer > threshold) //reset
                        {
                            AITimer = 0;
                            FinalPhaseDashCD = 0;
                            FinalPhaseBerserkDashesComplete = false;
                            FinalPhaseDashHorizSpeedSet = false;
                            if (mustRest)
                                FinalPhaseAttackCounter = 0;
                            npc.velocity = Vector2.Zero;
                            npc.netUpdate = true;
                        }
                    }

                    if (npc.netUpdate)
                    {
                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                            NetSync(npc);
                        }
                        npc.netUpdate = false;
                    }
                    return false;
                }
                else if (!IsInFinalPhase && npc.life <= npc.lifeMax * 0.1) //go into final phase
                {
                    npc.velocity *= 0.98f;
                    npc.alpha += 4;
                    for (int i = 0; i < 3; i++)
                    {
                        int d = Dust.NewDust(npc.position, npc.width, npc.height, recolor ? DustID.Vortex : DustID.BloodWater, 0f, 0f, 0, default, 1.5f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].noLight = true;
                        Main.dust[d].velocity *= 4f;
                    }
                    if (npc.alpha > 255)
                    {
                        npc.alpha = 255;
                        IsInFinalPhase = true;

                        SoundEngine.PlaySound(SoundID.Roar, npc.HasValidTarget ? Main.player[npc.target].Center : npc.Center);

                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, npc.whoAmI, npc.type);
                    }
                    return false;
                }
                else if (ai_Phase == 3 && (ai_AttackState == 0 || ai_AttackState == 5))
                {
                    if (ai_Timer < 2)
                    {
                        ai_Timer--;
                        npc.alpha += 4;
                        for (int i = 0; i < 3; i++)
                        {
                            int d = Dust.NewDust(npc.position, npc.width, npc.height, recolor ? DustID.Vortex : DustID.BloodWater, 0f, 0f, 0, default, 1.5f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].noLight = true;
                            Main.dust[d].velocity *= 4f;
                        }
                        if (npc.alpha > 255)
                        {
                            npc.alpha = 255;
                            if (FargoSoulsUtil.HostCheck && npc.HasPlayerTarget)
                            {
                                ai_Timer = 60;
                                ai_AttackState = 5f;

                                Vector2 distance = Main.player[npc.target].Center - npc.Center;
                                if (distance.X == 0) //never zero side
                                    distance.X = 1;
                                const int Xmax = 1200; //1.6.1 note: was 1200 before
                                const int Xmin = 1100; //1.6.1 note: was 600 before
                                if (Math.Abs(distance.X) > Xmax)
                                    distance.X = Xmax * Math.Sign(distance.X);
                                else if (Math.Abs(distance.X) < Xmin)
                                    distance.X = Xmin * Math.Sign(distance.X);

                                if (TeleportDirection == 0)
                                    TeleportDirection = Math.Sign(distance.X); //first dash picks side towards player
                                else
                                    TeleportDirection *= -1; //switch side

                                distance.X = Math.Abs(distance.X) * TeleportDirection;

                                if (distance.Y > 0) //ensure to teleport above
                                    distance.Y *= -1;

                                const int Ymax = 300; // 1.6.1 note: was 450 before
                                const int Ymin = 150; // 1.6.1 note: was 150 before
                                if (Math.Abs(distance.Y) > Ymax)
                                    distance.Y = Ymax * Math.Sign(distance.Y);
                                if (Math.Abs(distance.Y) < Ymin)
                                    distance.Y = Ymin * Math.Sign(distance.Y);

                                distance.X += Main.rand.NextFloat(-50, 50);
                                distance.Y += Main.rand.NextFloat(-200, 200); //randomness otherwise pattern basically becomes static

                                npc.Center = Main.player[npc.target].Center + distance;

                                npc.netUpdate = true;
                            }
                        }
                    }
                    else
                    {
                        const int aDif = 2;
                        npc.alpha -= aDif;
                        const int delay = 30;
                        if (Math.Abs(npc.alpha - (245 - delay)) <= aDif)
                        {
                            SoundEngine.PlaySound(SoundID.Roar, npc.Center);

                        }
                        if (npc.alpha < 245 - delay && npc.alpha > 212 - delay) //latter value calibrates dash distance, basically
                        {
                            if (npc.HasValidTarget)
                                npc.velocity = npc.SafeDirectionTo(Main.player[npc.target].Center) * 50;


                        }
                        if (npc.alpha < 245 - delay && npc.alpha > 120 - delay) //scythes
                        {
                            if (npc.alpha % (aDif * 10) <= aDif && FargoSoulsUtil.HostCheck)
                            {
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Normalize(npc.velocity), ModContent.ProjectileType<BloodScythe>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 1f, Main.myPlayer);
                            }
                        }
                        if (npc.alpha < 245 - delay && npc.alpha > 30) //curve towards player
                        {
                            float speed = npc.velocity.Length();
                            float modifier = 1f;
                            npc.velocity += npc.SafeDirectionTo(Main.player[npc.target].Center) * modifier;
                            npc.velocity = Vector2.Normalize(npc.velocity) * speed;


                        }
                        if (npc.alpha < 0)
                        {
                            npc.alpha = 0;
                        }
                        else
                        {
                            ai_Timer--;
                            npc.position -= npc.velocity / 2;
                            for (int i = 0; i < 3; i++)
                            {
                                int d = Dust.NewDust(npc.position, npc.width, npc.height, recolor ? DustID.Vortex : DustID.BloodWater, 0f, 0f, 0, default, 1.5f);
                                Main.dust[d].noGravity = true;
                                Main.dust[d].noLight = true;
                                Main.dust[d].velocity *= 4f;
                            }
                        }
                    }
                }
            }
            else
            {
                npc.alpha = 0;
            }
            EModeUtils.DropSummon(npc, ItemID.SuspiciousLookingEye, NPC.downedBoss1, ref DroppedSummon);


            return true;
        }
        */

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            if (WorldSavingSystem.MasochistModeReal)
            {
                target.AddBuff(BuffID.Bleeding, 180);
            }

            target.AddBuff(ModContent.BuffType<BerserkedBuff>(), 120);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadBossHeadSprite(recolor, 0);
            LoadBossHeadSprite(recolor, 1);
            LoadGoreRange(recolor, 6, 10);
        }
    }
    /*
    public class Servants : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.ServantofCthulhu);

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            base.OnSpawn(npc, source);
            npc.life = npc.lifeMax = 6;
        }
    }
    */
}
