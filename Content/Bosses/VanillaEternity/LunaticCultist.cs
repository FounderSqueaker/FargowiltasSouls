using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Patreon.DanielTheRobot;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.LunaticCultist;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class LunaticCultist : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.CultistBoss);

        public float RitualRotation;

        public int MagicDamageCounter;
        public int MeleeDamageCounter;
        public int RangedDamageCounter;
        public int MinionDamageCounter;

        public bool EnteredPhase2;
        public bool DroppedSummon;

        public int Timer;
        public int State = -1; // -1 (intro) for real cultist; 0 for clones
        public int OldAttack;
        public int AnimationState;

        public int Phase = 1;

        public static int LastAttack;
        public static int AttackRepeat;

        public static bool Alone(NPC npc) => npc.type == NPCID.CultistBoss && !NPC.AnyNPCs(NPCID.CultistBossClone);

        public enum States
        {
            Intro = -1,
            Reposition = 0,
            SpawnClones,
            SolarCircle,
            IceShatter,
            Lightning,
            Shadow,
            AncientLight
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(RitualRotation);
            binaryWriter.Write(MeleeDamageCounter);
            binaryWriter.Write(RangedDamageCounter);
            binaryWriter.Write(MagicDamageCounter);
            binaryWriter.Write(MinionDamageCounter);
            bitWriter.WriteBit(EnteredPhase2);

            binaryWriter.Write(Timer);
            binaryWriter.Write(State);
            binaryWriter.Write(OldAttack);
            binaryWriter.Write(AnimationState);
            binaryWriter.Write(Phase);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            RitualRotation = binaryReader.ReadSingle();
            MeleeDamageCounter = binaryReader.ReadInt32();
            RangedDamageCounter = binaryReader.ReadInt32();
            MagicDamageCounter = binaryReader.ReadInt32();
            MinionDamageCounter = binaryReader.ReadInt32();
            EnteredPhase2 = bitReader.ReadBit();

            Timer = binaryReader.ReadInt32();
            State = binaryReader.ReadInt32();
            OldAttack = binaryReader.ReadInt32();
            AnimationState = binaryReader.ReadInt32();
            Phase = binaryReader.ReadInt32();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax = (int)Math.Round(npc.lifeMax * 5f / 3f + 1);
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Suffocation] = true;
        }

        public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
        {
            if (npc.ai[3] == -1f && FargoSoulsUtil.IsSummonDamage(projectile) && !ProjectileID.Sets.IsAWhip[projectile.type])
                return false;
            if (npc.ai[3] == -1f && Main.netMode != NetmodeID.SinglePlayer) // during ritual, only nearby players can hit
            {
                if (!projectile.owner.IsWithinBounds(Main.maxPlayers))
                    return null;
                Player player = Main.player[projectile.owner];
                if (!player.Alive())
                    return false;
                if (player.Distance(npc.Center) > 300)
                    return false;
            }
            return base.CanBeHitByProjectile(npc, projectile);
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            EModeGlobalNPC.cultBoss = npc.whoAmI;

            Main.LocalPlayer.buffImmune[BuffID.Frozen] = true;

            if (Phase == 1 && npc.GetLifePercent() < 0.7f && State == (int)States.Reposition && Timer == 5)
            {
                Timer = 0;
                State = (int)States.SpawnClones;
                Phase = 2;
                npc.netUpdate = true;
            }

            if (Phase == 2 && npc.GetLifePercent() < 0.35f && State == (int)States.Reposition && Timer == 5)
            {
                Timer = 0;
                State = (int)States.SpawnClones;
                Phase = 3;
                npc.netUpdate = true;
            }

            npc.dontTakeDamage = false;

            AttacksAI(npc, ref Timer, ref State, ref OldAttack, ref AnimationState);

            npc.defense = npc.defDefense; //prevent vanilla p2 from lowering defense!
            Lighting.AddLight(npc.Center, 1f, 1f, 1f);

            EModeUtils.DropSummon(npc, "CultistSummon", NPC.downedAncientCultist, ref DroppedSummon, NPC.downedGolemBoss);

            if (!SkyManager.Instance["FargowiltasSouls:CultistSky"].IsActive())
                SkyManager.Instance.Activate("FargowiltasSouls:CultistSky");
            return false;
        }
        public static int AttackDuration => 210;
        public static void AttacksAI(NPC npc, ref int timer, ref int state, ref int oldAttack, ref int animation)
        {
            int damage = 30;
            //Targeting
            int maxDist = 4000;
            if (!npc.HasPlayerTarget || !Main.player[npc.target].active || Main.player[npc.target].dead || Main.player[npc.target].ghost || npc.Distance(Main.player[npc.target].Center) > maxDist)
            {
                npc.TargetClosest(false);
                Player newPlayer = Main.player[npc.target];
                if (!newPlayer.active || newPlayer.dead || newPlayer.ghost || npc.Distance(newPlayer.Center) > maxDist)
                {
                    if (npc.timeLeft > 60)
                        npc.timeLeft = 60;
                    npc.velocity.Y -= 0.4f;
                    if (npc.position.Y < 0)
                        npc.active = false;
                    return;
                }
            }
            npc.timeLeft = 180;
            Player player = Main.player[npc.target];
            bool alone = Alone(npc);
            switch ((States)state)
            {
                case States.Intro:
                    {
                        animation = (int)Animation.Laugh;
                        npc.dontTakeDamage = true;
                        if (timer == 1)
                        {
                            SoundEngine.PlaySound(SoundID.Zombie105, npc.Center);
                        }
                        //do 2sec spawn anim normally, or wait until tablet breaks if it's there. have a longer timer failsafe
                        if (timer >= 120 && (timer > 480 || !Main.npc.Any(otherNpc => otherNpc.active && otherNpc.type == NPCID.CultistTablet && npc.Distance(otherNpc.Center) < 600)))
                        {
                            EndAttack(npc, ref timer, ref state, ref oldAttack);
                        }
                    }
                    break;
                case States.Reposition:
                    {
                        int duration = WorldSavingSystem.MasochistModeReal ? 16 : 30;
                        if (EModeGlobalNPC.cultBoss.IsWithinBounds(Main.maxNPCs) && FargoSoulsUtil.NPCExists(EModeGlobalNPC.cultBoss, NPCID.CultistBoss) is NPC cultist && cultist.GetGlobalNPC<LunaticCultist>().Phase == 3)
                            duration = WorldSavingSystem.MasochistModeReal ? 50 : 65;
                        if (alone)
                            duration = WorldSavingSystem.MasochistModeReal ? 8 : 30;
                        animation = (int)Animation.Float;
                        Vector2 desiredPos = player.Center + player.DirectionTo(npc.Center) * 420;

                        npc.velocity = (Vector2.Lerp(npc.Center, desiredPos, 0.3f * timer / (float)duration) - npc.Center) * 0.25f;

                        npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(player.Center).NonZeroSign();
                        if (timer >= duration - 7)
                        {
                            npc.velocity *= 0.7f;
                        }
                        if (timer >= duration)
                        {
                            GetAttack(npc, ref timer, ref state, ref oldAttack);
                            return;
                        }
                    }
                    break;
                case States.SpawnClones:
                    {
                        animation = (int)Animation.Laugh;
                        Vector2 desiredPos = player.Center + player.DirectionTo(npc.Center) * 420;
                        Movement(npc, desiredPos, 0.8f, 0.8f);
                        npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(player.Center).NonZeroSign();

                        if (timer == 1 && npc.type == NPCID.CultistBoss)
                        {
                            SoundEngine.PlaySound(SoundID.Zombie105, npc.Center);

                            float ai0 = Main.rand.Next(4);

                            LunaticCultist cultistData = npc.GetGlobalNPC<LunaticCultist>();
                            int[] weight =
                            [
                                cultistData.MagicDamageCounter,
                                cultistData.MeleeDamageCounter,
                                cultistData.RangedDamageCounter,
                                cultistData.MinionDamageCounter,
                            ];
                            cultistData.MeleeDamageCounter = 0;
                            cultistData.RangedDamageCounter = 0;
                            cultistData.MagicDamageCounter = 0;
                            cultistData.MinionDamageCounter = 0;

                            npc.netUpdate = true;

                            int max = 0;
                            for (int i = 1; i < 4; i++)
                                if (weight[max] < weight[i])
                                    max = i;
                            if (weight[max] > 0)
                                ai0 = max;
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.UnitY * -10f, ModContent.ProjectileType<CelestialPillar>(),
                                Math.Max(75, FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 4)), 0f, Main.myPlayer, ai0);
                        }
                        if (timer == 70 * 2 && npc.type == NPCID.CultistBoss)
                        {
                            if (FargoSoulsUtil.HostCheck)
                            {
                                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.CultistBossClone, ai3: npc.whoAmI);
                            }
                        }
                        if (timer > 70 * 3)
                        {
                            GetAttack(npc, ref timer, ref state, ref oldAttack);
                            return;
                        }
                    }
                    break;
                case States.SolarCircle:
                    {
                        //if (alone && timer > 1)
                        //    timer++;
                        animation = (int)Animation.Float;
                        if (timer == 1)
                        {
                            SoundEngine.PlaySound(SoundID.Zombie89 with { Volume = 2 }, npc.Center);
                            int fireballs = alone ? 6 : 4;
                            if (WorldSavingSystem.MasochistModeReal)
                                fireballs += 1;
                            if (FargoSoulsUtil.HostCheck)
                            {
                                float rotDir = Main.rand.NextBool() ? 1 : -1;
                                for (int i = 0; i < fireballs; i++)
                                {
                                    float rot = i * MathF.Tau / fireballs;
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<CultistFireballSpin>(), damage, 1f, ai0: rot, ai1: npc.whoAmI, ai2: rotDir);
                                }
                            }
                        }
                        float start = 30;
                        if (timer < start)
                        {
                            npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(player.Center).NonZeroSign();
                            if (npc.Distance(player.Center) < 420)
                            {
                                npc.velocity -= npc.DirectionTo(player.Center) * 1.1f;
                            }
                            npc.velocity *= 0.93f;
                        }
                        else
                        {
                            npc.direction = npc.spriteDirection = npc.velocity.X.NonZeroSign();
                            float progress = (float)(timer - start) / (AttackDuration - start);
                            float speedupTime = 0.25f;
                            float accel = alone ? 0.45f : 0.35f;
                            float distance = npc.Distance(player.Center);
                            int anticheeseStart = 500;
                            if (npc.Distance(player.Center) > anticheeseStart)
                            {
                                accel += 2f * (distance - anticheeseStart) / 800f;
                            }
                            if (progress < speedupTime)
                                accel = MathHelper.SmoothStep(0, accel, progress / speedupTime);

                            npc.velocity += npc.DirectionTo(player.Center) * accel;
                            npc.velocity = npc.velocity.ClampLength(0, alone ? 22 : 22);
                        }

                        if (timer >= AttackDuration)
                        {
                            EndAttack(npc, ref timer, ref state, ref oldAttack);
                            return;
                        }
                    }
                    break;
                case States.IceShatter:
                    {
                        animation = (int)Animation.HoldForward;
                        npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(player.Center).NonZeroSign();
                        int positionTime = 5;
                        int shatterTime = alone ? 110 : 130;
                        if (WorldSavingSystem.MasochistModeReal)
                            shatterTime -= 15;
                        Vector2 desiredPos = player.Center + player.DirectionTo(npc.Center) * 400;
                        Movement(npc, desiredPos, 1.2f, 1.2f);
                        if (npc.Distance(player.Center) < 650)
                            npc.velocity *= 0.9f;
                        if (timer < positionTime)
                        {
                            
                        }
                        else if (timer == positionTime)
                        {
                            SoundEngine.PlaySound(SoundID.Zombie91 with { Volume = 2 }, npc.Center);
                            if (FargoSoulsUtil.HostCheck)
                            {
                                Vector2 vel = npc.DirectionTo(player.Center) * 5;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, vel, ModContent.ProjectileType<CultistIceBall>(), damage, 1f, ai0: vel.ToRotation(), ai1: shatterTime, ai2: npc.whoAmI);
                            }
                        }
                        else
                        {
                            npc.velocity *= 0.97f;
                        }
                        int duration = AttackDuration;
                        if (alone)
                            duration = positionTime + shatterTime + 25;
                        if (timer >= duration)
                        {
                            EndAttack(npc, ref timer, ref state, ref oldAttack);
                            return;
                        }
                    }
                    break;
                case States.Lightning:
                    {
                        animation = (int)Animation.HoldUp;
                        npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(player.Center).NonZeroSign();
                        Vector2 dir = player.DirectionTo(npc.Center);
                        dir = dir.RotateTowards(player.velocity.ToRotation(), 0.1f);
                        Vector2 desiredPos = player.Center + dir * 400;

                        Movement(npc, desiredPos, 1.6f, 1.6f);
                        if (timer == 2)
                        {
                            SoundEngine.PlaySound(SoundID.Zombie90 with { Volume = 2 }, npc.Center);
                            if (FargoSoulsUtil.HostCheck)
                            {
                                int bolts = alone ? 5 : 4;
                                for (int i = 0; i < bolts; i++)
                                {
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Main.rand.NextVector2Square(-15, 15), ModContent.ProjectileType<CultistVortex>(), damage, 0, Main.myPlayer, 0f, i);
                                }
                            }
                        }
                        
                        int duration = AttackDuration;
                        if (alone)
                            duration = 60;
                        if (timer >= duration)
                        {
                            EndAttack(npc, ref timer, ref state, ref oldAttack);
                            return;
                        }
                    }
                    break;
                case States.Shadow:
                    {
                        animation = alone ? (int)Animation.HoldForward : (int)Animation.HoldUp;
                        npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(player.Center).NonZeroSign();
                        Vector2 dir = player.DirectionTo(npc.Center);
                        if (alone)
                        {
                            npc.velocity *= 0.92f;
                        }
                        else
                        {
                            dir = dir.RotateTowards(player.velocity.ToRotation(), 0.1f);
                            Vector2 desiredPos = player.Center + dir * 400;
                            Movement(npc, desiredPos, 1.6f, 1.6f);
                        }
                            
                        void Circle()
                        {
                            if (FargoSoulsUtil.HostCheck)
                            {
                                int max = alone ? 14 : 10;
                                if (WorldSavingSystem.MasochistModeReal)
                                    max += 3;

                                Vector2 baseOffset = npc.SafeDirectionTo(player.Center);
                                const float spawnOffset = 1200f;
                                const float speed = 7f;
                                const float ai0 = spawnOffset / speed;
                                float rand = Main.rand.NextFloat(MathF.Tau);
                                for (int i = 0; i < max; i++)
                                {
                                    int p = Projectile.NewProjectile(npc.GetSource_FromAI(), player.Center + spawnOffset * baseOffset.RotatedBy(rand + 2 * Math.PI / max * i), -speed * baseOffset.RotatedBy(rand + 2 * Math.PI / max * i),
                                        ModContent.ProjectileType<CultistFireball>(), damage, 0f, Main.myPlayer, ai0);

                                }
                            }
                        }
                        if (timer == 10)
                        {
                            SoundEngine.PlaySound(SoundID.Zombie91 with { Volume = 2}, npc.Center);
                            Circle();
                        }
                        if (WorldSavingSystem.MasochistModeReal && timer == 130)
                        {
                            Circle();
                        }
                        if (alone)
                        {
                            if (timer > 10 && timer % 10 == 0 && timer < 50)
                            {
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    Vector2 vec = Vector2.Normalize(player.Center - npc.Center + player.velocity * 20f);
                                    if (vec.HasNaNs())
                                    {
                                        vec = new Vector2(npc.direction, 0f);
                                    }
                                    Vector2 vector3 = npc.Center + new Vector2(npc.direction * 30, 12f);
                                    for (int m = 0; m < 1; m++)
                                    {
                                        Vector2 spinninpoint = vec * (6f + (float)Main.rand.NextDouble() * 4f);
                                        spinninpoint = spinninpoint.RotatedByRandom(0.5235987901687622);
                                        Projectile.NewProjectile(npc.GetSource_FromAI(), vector3.X, vector3.Y, spinninpoint.X, spinninpoint.Y, ProjectileID.CultistBossFireBallClone, damage, 0f, Main.myPlayer);
                                    }
                                }
                            }
                        }
                        int duration = AttackDuration;
                        if (alone)
                            duration = 120;
                        if (timer >= duration)
                        {
                            EndAttack(npc, ref timer, ref state, ref oldAttack);
                            return;
                        }
                    }
                    break;
                case States.AncientLight:
                    {
                        animation = (int)Animation.HoldForward;
                        int windup = 45;
                        int swingTime = 40;

                        npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(player.Center).NonZeroSign();

                        Vector2 desiredPos = player.Center + player.HorizontalDirectionTo(npc.Center) * Vector2.UnitX * 450;
                        if (timer < windup)
                        {
                            
                            Movement(npc, desiredPos, 1.2f, 1.2f);

                            npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(player.Center).NonZeroSign();
                            if (npc.Distance(player.Center) < 420)
                            {
                                npc.velocity -= npc.DirectionTo(player.Center) * 1.8f;
                            }
                            npc.velocity *= 0.98f;
                        }
                        else
                        {
                            npc.velocity *= 0.89f;
                            Movement(npc, desiredPos, 0.6f, 0.6f);
                        }
                        if (timer == windup)
                            SoundEngine.PlaySound(SoundID.Zombie88 with { Volume = 2 }, npc.Center);
                        if (timer > windup && timer <= windup + swingTime)
                        {
                            float progress = (float)(timer - windup) / swingTime;
                            int div = alone ? 4 : 5;
                            if (WorldSavingSystem.MasochistModeReal)
                                div -= 1;
                            if (timer % div == 0)
                            {
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    Vector2 pos = npc.Center + new Vector2(npc.direction * 30, 12f);
                                    float dir = npc.HorizontalDirectionTo(player.Center);
                                    float speed = 8f + 6f * progress;
                                    Vector2 vel = (dir * Vector2.UnitX * speed).RotatedBy(dir * MathHelper.Lerp(-MathHelper.PiOver2 * 0.9f, MathHelper.PiOver2 * 1.2f, progress));
                                    float ai = (Main.rand.NextFloat() - 0.5f) * 0.3f * ((float)Math.PI * 2f) / 60f;
                                    int n = NPC.NewNPC(npc.GetSource_FromAI(), (int)pos.X, (int)pos.Y + 7, NPCID.AncientLight, 0, 0f, ai, vel.X, vel.Y);
                                    Main.npc[n].velocity = vel;
                                    Main.npc[n].netUpdate = true;
                                }
                            }
                        }
                        int duration = AttackDuration;
                        if (alone)
                            duration = 120;
                        if (timer >= duration)
                        {
                            EndAttack(npc, ref timer, ref state, ref oldAttack);
                            return;
                        }
                    }
                    break;
            }
            timer++;
        }
        #region Help Methods
        public static void EndAttack(NPC npc, ref int timer, ref int state, ref int oldAttack)
        {
            oldAttack = state;
            state = (int)States.Reposition;
            timer = 0;
            npc.netUpdate = true;
        }
        public static void GetAttack(NPC npc, ref int timer, ref int state, ref int oldAttack)
        {
            List<States> randAttacks = [States.SolarCircle, States.IceShatter, /*States.Lightning, */States.Shadow, States.AncientLight];
            randAttacks.Remove((States)oldAttack);
            // no repeat my own attack
            if (AttackRepeat >= 1)
                randAttacks.Remove((States)LastAttack);
            // no repeat solar attack
            if (LastAttack == (int)States.SolarCircle)
                randAttacks.Remove(States.SolarCircle);

            state = (int)Main.rand.NextFromCollection(randAttacks);
            timer = 0;
            npc.netUpdate = true;
            if (LastAttack == state)
                AttackRepeat++;
            else
                AttackRepeat = 0;
            LastAttack = state;
            // debug
            //state = (int)States.IceShatter;
        }
        public static void Movement(NPC npc, Vector2 desiredPos, float accel, float decel)
        {
            accel *= 0.8f;
            decel *= 0.8f;
            RepulseOthers(npc, ref desiredPos);
            float resistance = npc.velocity.Length() * accel / 35f;
            npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, desiredPos, npc.velocity, accel - resistance, decel + resistance);
        }
        public static void RepulseOthers(NPC npc, ref Vector2 desiredPos)
        {
            NPC[] others = Main.npc.Where(n => n.whoAmI != npc.whoAmI && n.Alive() && (n.type == NPCID.CultistBoss || n.type == NPCID.CultistBossClone)).ToArray();

            for (int i = 0; i < others.Length; i++)
            {
                if (others[i] == null) continue;
                int minDistance = 500;
                if (desiredPos.Distance(others[i].Center) < minDistance)
                    desiredPos = others[i].Center + others[i].DirectionTo(desiredPos) * minDistance;
            }
        }
        #endregion
        public enum Animation
        {
            Float,
            HoldUp,
            HoldForward,
            Laugh
        }
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (npc.IsABestiaryIconDummy)
            {
                if (npc.frameCounter > 5.0)
                {
                    npc.frameCounter = 0.0;
                    npc.frame.Y += frameHeight;
                }
                if (npc.frame.Y < frameHeight * 4 || npc.frame.Y > frameHeight * 6)
                {
                    npc.frame.Y = frameHeight * 4;
                }
            }
            else
            {
                switch ((Animation)AnimationState)
                {
                    case Animation.Float:
                        if (npc.frameCounter >= 15.0)
                        {
                            npc.frameCounter = 0.0;
                        }
                        npc.frame.Y = ((int)npc.frameCounter / 5 + 4) * frameHeight;
                        break;

                    case Animation.HoldUp:
                        if (npc.frameCounter >= 15.0)
                        {
                            npc.frameCounter = 0.0;
                        }
                        npc.frame.Y = ((int)npc.frameCounter / 5 + 7) * frameHeight;
                        break;
                    case Animation.HoldForward:
                        if (npc.frameCounter >= 15.0)
                        {
                            npc.frameCounter = 0.0;
                        }
                        npc.frame.Y = ((int)npc.frameCounter / 5 + 10) * frameHeight;
                        break;

                    case Animation.Laugh:
                        if (npc.frameCounter >= 15.0)
                        {
                            npc.frameCounter = 0.0;
                        }
                        npc.frame.Y = ((int)npc.frameCounter / 5 + 13) * frameHeight;
                        break;
                }
            }
            npc.frameCounter += 1.0;
        }
        public override void ModifyHitByAnything(NPC npc, Player player, ref NPC.HitModifiers modifiers)
        {
            if (State == (int)States.Intro || State == (int)States.SpawnClones)
            {
                modifiers.FinalDamage *= 0.25f;
            }
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int CooldownSlot)
        {
            return false;
        }

        public override void SafeOnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            base.SafeOnHitByItem(npc, player, item, hit, damageDone);

            if (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.Throwing))
                MeleeDamageCounter += hit.Damage;
            if (item.CountsAsClass(DamageClass.Ranged))
                RangedDamageCounter += hit.Damage;
            if (item.CountsAsClass(DamageClass.Magic))
                MagicDamageCounter += hit.Damage;
            if (item.CountsAsClass(DamageClass.Summon))
                MinionDamageCounter += hit.Damage;
        }

        public override void SafeModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            base.SafeModifyHitByProjectile(npc, projectile, ref modifiers);

            if (ProjectileID.Sets.CultistIsResistantTo[projectile.type])
                modifiers.FinalDamage *= 1 / 1.3f;
        }

        public override void SafeOnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            base.SafeOnHitByProjectile(npc, projectile, hit, damageDone);

            if (projectile.CountsAsClass(DamageClass.Melee) || projectile.CountsAsClass(DamageClass.Throwing))
                MeleeDamageCounter += hit.Damage;
            if (projectile.CountsAsClass(DamageClass.Ranged))
                RangedDamageCounter += hit.Damage;
            if (projectile.CountsAsClass(DamageClass.Magic))
                MagicDamageCounter += hit.Damage;
            if (FargoSoulsUtil.IsSummonDamage(projectile))
                MinionDamageCounter += hit.Damage;
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadBossHeadSprite(recolor, 24);
            LoadBossHeadSprite(recolor, 31);
            LoadGoreRange(recolor, 902, 903);
            LoadExtra(recolor, 30);
        }
    }

    public class LunaticCultistClone : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.CultistBossClone);

        public int TotalCultistCount;
        public int MyRitualPosition;

        public int Timer;
        public int State;
        public int OldAttack;
        public int AnimationState;
        public int Phase;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(TotalCultistCount);
            binaryWriter.Write(MyRitualPosition);

            binaryWriter.Write(Timer);
            binaryWriter.Write(State);
            binaryWriter.Write(OldAttack);
            binaryWriter.Write(AnimationState);
            binaryWriter.Write(Phase);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            TotalCultistCount = binaryReader.ReadInt32();
            MyRitualPosition = binaryReader.ReadInt32();

            Timer = binaryReader.ReadInt32();
            State = binaryReader.ReadInt32();
            OldAttack = binaryReader.ReadInt32();
            AnimationState = binaryReader.ReadInt32();
            Phase = binaryReader.ReadInt32();
        }
        public override bool CheckActive(NPC npc)
        {
            return false;
        }
        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[ModContent.BuffType<LethargicBuff>()] = true;
            npc.buffImmune[BuffID.Suffocation] = true;
        }

        public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
        {
            if (FargoSoulsUtil.IsSummonDamage(projectile) && !ProjectileID.Sets.IsAWhip[projectile.type])
                return false;
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                if (!projectile.owner.IsWithinBounds(Main.maxPlayers))
                    return null;
                Player player = Main.player[projectile.owner];
                if (!player.Alive())
                    return false;
                if (player.Distance(npc.Center) > 300)
                    return false;
            }
            return null;
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            NPC cultist = FargoSoulsUtil.NPCExists(npc.ai[3], NPCID.CultistBoss);

            if (cultist != null && cultist.TypeAlive(NPCID.CultistBoss))
            {
                npc.target = cultist.target;
                npc.realLife = cultist.whoAmI;
                npc.HitSound = cultist.HitSound;
                npc.defense = 9999;
                //npc.dontTakeDamage = true;
                var cultistEmode = cultist.GetGlobalNPC<LunaticCultist>();
                if (cultistEmode.State == (int)LunaticCultist.States.SpawnClones && Phase < cultistEmode.Phase)
                {
                    Phase = cultistEmode.Phase;
                    State = (int)LunaticCultist.States.SpawnClones;
                    Timer = 70;
                    npc.netUpdate = true;
                }
                LunaticCultist.AttacksAI(npc, ref Timer, ref State, ref OldAttack, ref AnimationState);
                Lighting.AddLight(npc.Center, 1f, 1f, 1f);
                return false;
            }
            npc.active = false;
            return false;
        }
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (npc.IsABestiaryIconDummy)
            {
                if (npc.frameCounter > 5.0)
                {
                    npc.frameCounter = 0.0;
                    npc.frame.Y += frameHeight;
                }
                if (npc.frame.Y < frameHeight * 4 || npc.frame.Y > frameHeight * 6)
                {
                    npc.frame.Y = frameHeight * 4;
                }
            }
            else
            {
                switch ((LunaticCultist.Animation)AnimationState)
                {
                    case LunaticCultist.Animation.Float:
                        if (npc.frameCounter >= 15.0)
                        {
                            npc.frameCounter = 0.0;
                        }
                        npc.frame.Y = ((int)npc.frameCounter / 5 + 4) * frameHeight;
                        break;

                    case LunaticCultist.Animation.HoldUp:
                        if (npc.frameCounter >= 15.0)
                        {
                            npc.frameCounter = 0.0;
                        }
                        npc.frame.Y = ((int)npc.frameCounter / 5 + 7) * frameHeight;
                        break;
                    case LunaticCultist.Animation.HoldForward:
                        if (npc.frameCounter >= 15.0)
                        {
                            npc.frameCounter = 0.0;
                        }
                        npc.frame.Y = ((int)npc.frameCounter / 5 + 10) * frameHeight;
                        break;

                    case LunaticCultist.Animation.Laugh:
                        if (npc.frameCounter >= 15.0)
                        {
                            npc.frameCounter = 0.0;
                        }
                        npc.frame.Y = ((int)npc.frameCounter / 5 + 13) * frameHeight;
                        break;
                }
            }
            npc.frameCounter += 1.0;
        }
        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            base.HitEffect(npc, hit);

            NPC cultist = FargoSoulsUtil.NPCExists(npc.ai[3], NPCID.CultistBoss);

            //yes, this spawns two clones without the check
            if (cultist != null && NPC.CountNPCS(npc.type) < (WorldSavingSystem.MasochistModeReal ? Math.Min(TotalCultistCount + 1, 12) : TotalCultistCount))
            {
                if (FargoSoulsUtil.HostCheck)
                {
                    FargoSoulsUtil.NewNPCEasy(cultist.GetSource_FromAI(), npc.Center, NPCID.CultistBossClone, 0, npc.ai[0], npc.ai[1], npc.ai[2], npc.ai[3], npc.target);
                }
            }
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
        }
    }

    public class AncientDoom : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.AncientDoom);

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax *= 4;
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Suffocation] = true;
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int CooldownSlot)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.CanHitPlayer(npc, target, ref CooldownSlot);
            CooldownSlot = ImmunityCooldownID.Bosses;
            return base.CanHitPlayer(npc, target, ref CooldownSlot) && npc.localAI[3] > 120;
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            if (npc.localAI[3] == 0f)
            {
                if (FargoSoulsUtil.HostCheck)
                {
                    Vector2 pivot = npc.Center + new Vector2(250f, 0f).RotatedByRandom(2 * Math.PI);
                    npc.ai[2] = pivot.X;
                    npc.ai[3] = pivot.Y;
                    npc.netUpdate = true;
                }
            }

            npc.localAI[3]++;

            if (npc.ai[2] > 0f && npc.ai[3] > 0f)
            {
                Vector2 pivot = new(npc.ai[2], npc.ai[3]);
                npc.velocity = Vector2.Normalize(pivot - npc.Center).RotatedBy(Math.PI / 2) * 6f;
            }

            return result;
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<MarkedforDeathBuff>(), 300);
            target.AddBuff(ModContent.BuffType<ShadowflameBuff>(), 300);
        }
    }

    public class AncientLight : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.AncientLight);

        public int Timer;

        public int SourceNPCType;
        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lavaImmune = true;
            //MoonLordAlive = FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.moonBoss, NPCID.MoonLordCore);

            npc.dontTakeDamage = true;
            npc.immortal = true;
            npc.chaseable = false;
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Suffocation] = true;
            npc.buffImmune[BuffID.OnFire] = true;
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            npc.dontTakeDamage = true;
            npc.immortal = true;
            npc.chaseable = false;

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.moonBoss, NPCID.MoonLordCore))
            {
                if (npc.HasPlayerTarget)
                {
                    Vector2 speed = Main.player[npc.target].Center - npc.Center;
                    speed.Normalize();
                    speed *= 9f;

                    npc.ai[2] += speed.X / 100f;
                    if (npc.ai[2] > 9f)
                        npc.ai[2] = 9f;
                    if (npc.ai[2] < -9f)
                        npc.ai[2] = -9f;
                    npc.ai[3] += speed.Y / 100f;
                    if (npc.ai[3] > 9f)
                        npc.ai[3] = 9f;
                    if (npc.ai[3] < -9f)
                        npc.ai[3] = -9f;
                }
                else
                {
                    npc.TargetClosest(false);
                }

                Timer++;
                if (Timer > 240)
                {
                    npc.HitEffect(0, 9999);
                    npc.active = false;
                }

                npc.velocity.X = npc.ai[2];
                npc.velocity.Y = npc.ai[3];
            }
            else if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.cultBoss, NPCID.CultistBoss))
            {
                if (++Timer < 40)
                {
                    npc.position -= npc.velocity * (Timer / 40f);
                    return false;
                }

                if (Timer > 180)
                {
                    npc.dontTakeDamage = false;
                    npc.immortal = false;
                }
            }

            return result;
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (!WorldSavingSystem.EternityMode)
                base.OnSpawn(npc, source);
            else if (source is EntitySource_Parent parent && parent.Entity is NPC sourceNPC)
            {
                SourceNPCType = sourceNPC.type;
            }
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.CanHitPlayer(npc, target, ref cooldownSlot);

            if (SourceNPCType is NPCID.CultistBoss or NPCID.CultistBossClone or NPCID.MoonLordCore or NPCID.MoonLordHead or NPCID.MoonLordHand)
                cooldownSlot = ImmunityCooldownID.Bosses;

            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);
        }
    }

    public class CultistDragon : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.CultistDragonBody1,
            NPCID.CultistDragonBody2,
            NPCID.CultistDragonBody3,
            NPCID.CultistDragonBody4,
            NPCID.CultistDragonHead,
            NPCID.CultistDragonTail
        );

        public int DamageReductionTimer;

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Suffocation] = true;

            if (npc.type == NPCID.CultistDragonHead)
            {
                if (WorldSavingSystem.MasochistModeReal && FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.cultBoss, NPCID.CultistBoss))
                    npc.Center = Main.npc[EModeGlobalNPC.cultBoss].Center;
            }
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            DamageReductionTimer++;

            return result;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= Math.Min(1.0f, DamageReductionTimer / 300.0f);

            base.ModifyIncomingHit(npc, ref modifiers);
        }

        public override void SafeModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            base.SafeModifyHitByProjectile(npc, projectile, ref modifiers);

            if (projectile.maxPenetrate > 1)
                modifiers.FinalDamage /= projectile.maxPenetrate;
            else if (projectile.maxPenetrate < 0)
                modifiers.FinalDamage /= 4;
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 360);
        }
    }

    public class AncientVision : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.AncientCultistSquidhead);

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            if (!WorldSavingSystem.MasochistModeReal)
                npc.lifeMax /= 2;
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Suffocation] = true;
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 360);
        }
    }
}
