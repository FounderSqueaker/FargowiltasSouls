using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Bosses.Champions.Will;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Placables;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.BossMinions;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.QueenBee;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class QueenBee : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.QueenBee);

        public int HiveThrowTimer;
        public int StingerRingTimer;
        public int BeeSwarmTimer = 600;
        public int ForgorDeathrayTimer;
        public int EnrageFactor;

        public bool SpawnedRoyalSubjectWave1;
        public bool SpawnedRoyalSubjectWave2;
        public bool InPhase2;

        public bool DroppedSummon;
        public bool SubjectDR;

        public Vector2 LockVector1;
        public int ShotTimer = 0;
        public bool ChargePosition;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(HiveThrowTimer);
            binaryWriter.Write(StingerRingTimer);
            binaryWriter.Write(BeeSwarmTimer);
            binaryWriter.WriteVector2(LockVector1);
            bitWriter.WriteBit(SpawnedRoyalSubjectWave1);
            bitWriter.WriteBit(SpawnedRoyalSubjectWave2);
            bitWriter.WriteBit(InPhase2);
            bitWriter.WriteBit(ChargePosition);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            HiveThrowTimer = binaryReader.ReadInt32();
            StingerRingTimer = binaryReader.ReadInt32();
            BeeSwarmTimer = binaryReader.ReadInt32();
            LockVector1 = binaryReader.ReadVector2();
            SpawnedRoyalSubjectWave1 = bitReader.ReadBit();
            SpawnedRoyalSubjectWave2 = bitReader.ReadBit();
            InPhase2 = bitReader.ReadBit();
            ChargePosition = bitReader.ReadBit();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax = (int)Math.Round(npc.lifeMax * 1f);
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Poisoned] = true;
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);
            //FargoSoulsUtil.PrintAI(npc);

            EModeGlobalNPC.beeBoss = npc.whoAmI;
            Main.LocalPlayer.buffImmune[ModContent.BuffType<SwarmingBuff>()] = true;

            if (npc.ai[0] == 0) // sound fix
            {
                int oldDash = ShotTimer;
                ShotTimer = (int)npc.localAI[0];
                if (oldDash != ShotTimer && ShotTimer == 1)
                    SoundEngine.PlaySound(SoundID.Zombie125, npc.position);
            }

            if (npc.ai[0] == 2 && npc.HasValidTarget)
            {
                float lerp = Math.Min(++npc.ai[1] / 3000f, 1f);
                npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(Main.player[npc.target].Center) * npc.velocity.Length(), lerp);
            }


            if (npc.HasPlayerTarget && npc.HasValidTarget && (!Main.player[npc.target].ZoneJungle
                || Main.player[npc.target].position.Y < Main.worldSurface * 16))
            {
                if (++EnrageFactor == 300)
                {
                    FargoSoulsUtil.PrintLocalization($"Mods.{Mod.Name}.NPCs.EMode.QueenBeeEnrage", new Color(175, 75, 255));
                }

                if (EnrageFactor > 300)
                {
                    float rotation = Main.rand.NextFloat(0.03f, 0.18f);
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + new Vector2(3 * npc.direction, 15), Main.rand.NextFloat(8f, 24f) * Main.rand.NextVector2Unit(),
                        ModContent.ProjectileType<Bee>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 1.5f), 0f, Main.myPlayer, npc.target, Main.rand.NextBool() ? -rotation : rotation);
                }
            }
            else
            {
                EnrageFactor = 0;
            }


            if (!SpawnedRoyalSubjectWave1 && npc.life < npc.lifeMax / 3 * 2 && npc.HasPlayerTarget)
            {
                SpawnedRoyalSubjectWave1 = true;

                Vector2 vector72 = new(npc.position.X + npc.width / 2 + Main.rand.Next(20) * npc.direction, npc.position.Y + npc.height * 0.8f);

                int n = FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), vector72, ModContent.NPCType<RoyalSubject>(),
                    velocity: new Vector2(Main.rand.Next(-200, 201) * 0.1f, Main.rand.Next(-200, 201) * 0.1f));
                if (n != Main.maxNPCs)
                    Main.npc[n].localAI[0] = 60f;

                FargoSoulsUtil.PrintLocalization("Announcement.HasAwoken", new Color(175, 75, 255), Language.GetTextValue($"Mods.{Mod.Name}.NPCs.RoyalSubject.DisplayName"));

                npc.netUpdate = true;
                NetSync(npc);
            }

            if (!SpawnedRoyalSubjectWave2 && npc.life < npc.lifeMax / 3 && npc.HasPlayerTarget)
            {
                SpawnedRoyalSubjectWave2 = true;

                if (WorldSavingSystem.MasochistModeReal)
                    SpawnedRoyalSubjectWave1 = false; //do this again

                Vector2 vector72 = new(npc.position.X + npc.width / 2 + Main.rand.Next(20) * npc.direction, npc.position.Y + npc.height * 0.8f);

                int n = FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), vector72, ModContent.NPCType<RoyalSubject>(),
                    velocity: new Vector2(Main.rand.Next(-200, 201) * 0.1f, Main.rand.Next(-200, 201) * 0.1f));
                if (n != Main.maxNPCs)
                    Main.npc[n].localAI[0] = 60f;

                FargoSoulsUtil.PrintLocalization("Announcement.HasAwoken", new Color(175, 75, 255), Language.GetTextValue($"Mods.{Mod.Name}.NPCs.RoyalSubject.DisplayName"));

                NPC.SpawnOnPlayer(npc.target, ModContent.NPCType<RoyalSubject>()); //so that both dont stack for being spawned from qb

                npc.netUpdate = true;
                NetSync(npc);
            }


            if (!InPhase2 && npc.life < npc.lifeMax / 2) //enable new attack and roar below 50%
            {
                InPhase2 = true;
                SoundEngine.PlaySound(SoundID.Zombie125, npc.Center);

                if (WorldSavingSystem.MasochistModeReal)
                    SpawnedRoyalSubjectWave1 = false; //do this again

                npc.netUpdate = true;
                NetSync(npc);
            }

            SubjectDR = NPC.AnyNPCs(ModContent.NPCType<RoyalSubject>());
            if (SubjectDR)
            {
                npc.HitSound = SoundID.NPCHit4;
                npc.chaseable = false;

                int dustId = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone, 0f, 0f, 100, default, 2f);
                Main.dust[dustId].noGravity = true;
                int dustId3 = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone, 0f, 0f, 100, default, 2f);
                Main.dust[dustId3].noGravity = true;

                if (!Main.getGoodWorld)
                {
                    //if in dash mode, but not actually dashing right this second
                    if (npc.ai[0] == 0 && npc.ai[1] % 2 == 0)
                    {
                        npc.ai[0] = 3; //dont
                        npc.ai[1] = 0;
                        npc.netUpdate = true;
                    }

                    //shoot stingers mode
                    if (npc.ai[0] == 3)
                    {
                        if (npc.ai[1] > 1 && !WorldSavingSystem.MasochistModeReal)
                            npc.ai[1] -= 0.5f; //slower stingers
                    }
                }
            }
            else
            {
                npc.HitSound = SoundID.NPCHit1;
                npc.chaseable = true;

                if (InPhase2 && HiveThrowTimer % 2 == 0)
                    HiveThrowTimer++; //throw hives faster when no royal subjects alive
            }

            if (WorldSavingSystem.MasochistModeReal)
            {
                HiveThrowTimer++;

                if (ForgorDeathrayTimer > 0 && --ForgorDeathrayTimer % 10 == 0 && npc.HasValidTarget && FargoSoulsUtil.HostCheck)
                {
                    Projectile.NewProjectile(npc.GetSource_FromThis(),
                        Main.player[npc.target].Center - 2000 * Vector2.UnitY, Vector2.UnitY,
                        ModContent.ProjectileType<WillDeathraySmall>(),
                        (int)(npc.damage * .75), 0f, Main.myPlayer,
                        Main.player[npc.target].Center.X, npc.whoAmI, 1f);

                    for (int i = 0; i < 22; i++)
                    {
                        Vector2 rand = Vector2.UnitX * Main.rand.NextFloat(-100, 100) - Vector2.UnitY * 90 * i;
                        Vector2 spawnPos = Main.player[npc.target].Center - 22 * 90 * Vector2.UnitY + rand;
                        Vector2 speed = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), 22);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), spawnPos, speed, ModContent.ProjectileType<RoyalSubjectProjectile>(),
                            FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer);
                    }
                }
            }

            if (InPhase2)
            {
                if (++HiveThrowTimer > 570 && BeeSwarmTimer <= 600 && (npc.ai[0] == 3f || npc.ai[0] == 1f)) //lobs hives below 50%, not dashing
                {
                    HiveThrowTimer = 0;

                    npc.netUpdate = true;
                    NetSync(npc);

                    const float gravity = 0.25f;
                    float time = 75f;
                    Vector2 distance = Main.player[npc.target].Center - Vector2.UnitY * 16 - npc.Center + Main.player[npc.target].velocity * 30f;
                    distance.X /= time;
                    distance.Y = distance.Y / time - 0.5f * gravity * time;
                    if (FargoSoulsUtil.HostCheck)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, distance, ModContent.ProjectileType<Beehive>(),
                            FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, time - 5);
                    }
                }
                
                if (npc.ai[0] == 0 && npc.ai[1] == 1f) //if qb tries to start doing dashes of her own volition
                {
                    npc.ai[0] = 3f;
                    npc.ai[1] = 0f; //don't
                    npc.ai[2] = 0f;
                    npc.localAI[0] = 0f;
                    npc.netUpdate = true;
                }
            }

            //only while stationary mode
            if (npc.ai[0] == 3f || npc.ai[0] == 1f)
            {
                if (InPhase2 && ++BeeSwarmTimer > 600)
                {
                    if (BeeSwarmTimer < 720) //slow down
                    {
                        if (BeeSwarmTimer == 601)
                        {
                            npc.netUpdate = true;
                            NetSync(npc);

                            if (FargoSoulsUtil.HostCheck)
                            {
                                for (int j = -1; j <= 1; j += 2)
                                {
                                    for (int i = -1; i <= 1; i++)
                                    {
                                        Vector2 dir = j * 3 * Vector2.UnitX.RotatedBy(i * MathHelper.Pi / 7);
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + dir, Vector2.Zero, ModContent.ProjectileType<MutantGlowything>(), 0, 0f, Main.myPlayer, dir.ToRotation(), npc.whoAmI, 1f);
                                    }
                                }
                            }

                            if (npc.HasValidTarget)
                                SoundEngine.PlaySound(SoundID.Zombie125, Main.player[npc.target].Center); //eoc roar

                            if (WorldSavingSystem.MasochistModeReal)
                                BeeSwarmTimer += 30;
                        }

                        Player p = Main.player[npc.target];
                        if (npc.Distance(p.Center) > 500)
                        {
                            Movement(npc, p.Center + p.DirectionTo(npc.Center) * 500, 1f, 12);
                        }
                        if (Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
                        {
                            npc.velocity *= 0.975f;
                        }
                        else if (BeeSwarmTimer > 630)
                        {
                            BeeSwarmTimer--; //stall this section until has line of sight
                            return MovementRework(npc, true);
                        }
                    }
                    else if (BeeSwarmTimer < 840) //spray bees
                    {
                        npc.velocity = Vector2.Zero;

                        if (BeeSwarmTimer % 2 == 0 && FargoSoulsUtil.HostCheck)
                        {
                            const float rotation = 0.025f;
                            for (int i = -1; i <= 1; i += 2)
                            {
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + new Vector2(3 * npc.direction, 15), i * Main.rand.NextFloat(9f, 18f) * Vector2.UnitX.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-45, 45))),
                                    ModContent.ProjectileType<Bee>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, WorldSavingSystem.MasochistModeReal ? 4f / 3 : 1), 0f, Main.myPlayer, npc.target, Main.rand.NextBool() ? -rotation : rotation);
                            }
                        }
                    }
                    else if (BeeSwarmTimer > 870) //return to normal AI
                    {
                        BeeSwarmTimer = 0;
                        HiveThrowTimer -= 60;

                        npc.netUpdate = true;
                        NetSync(npc);

                        npc.ai[0] = 0f;
                        npc.ai[1] = 4f; //trigger dashes, but skip the first one
                        npc.ai[2] = -44f;
                        npc.ai[3] = 0f;
                    }

                    if (npc.netUpdate)
                    {
                        npc.netUpdate = false;

                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                    }
                    return MovementRework(npc, false);
                }
            }

            if (npc.ai[0] == 0 && npc.ai[1] == 4) //when about to do dashes triggered by royal subjects/bee swarm, telegraph and stall
            {
                if (npc.ai[2] < 0)
                {
                    if (npc.ai[2] == -44) //telegraph
                    {
                        /*
                        SoundEngine.PlaySound(SoundID.Item21, npc.Center);

                        for (int i = 0; i < 44; i++)
                        {
                            int d = Dust.NewDust(npc.position, npc.width, npc.height, Main.rand.NextBool() ? 152 : 153, npc.velocity.X * 0.2f, npc.velocity.Y * 0.2f);
                            Main.dust[d].scale = Main.rand.NextFloat(1f, 3f);
                            Main.dust[d].velocity *= Main.rand.NextFloat(4.4f);
                            Main.dust[d].noGravity = Main.rand.NextBool();
                            if (Main.dust[d].noGravity)
                            {
                                Main.dust[d].scale *= 2.2f;
                                Main.dust[d].velocity *= 4.4f;
                            }
                        }
                        */

                        if (WorldSavingSystem.MasochistModeReal)
                            npc.ai[2] = 0;

                        ForgorDeathrayTimer = 95;
                        if (Main.getGoodWorld)
                            ForgorDeathrayTimer += 60;
                    }

                    npc.velocity *= 0.95f;
                    npc.ai[2]++;

                    return MovementRework(npc, false);
                }
            }

            if (WorldSavingSystem.MasochistModeReal)
            {
                //if in dash mode, but not actually dashing right this second
                if (npc.ai[0] == 0 && npc.ai[1] % 2 == 0)
                {
                    if (npc.HasValidTarget && Math.Abs(Main.player[npc.target].Center.Y - npc.Center.Y) > npc.velocity.Y * 2)
                        npc.position.Y += npc.velocity.Y;
                }
            }

            EModeUtils.DropSummon(npc, ItemID.Abeemination, NPC.downedQueenBee, ref DroppedSummon);

            return MovementRework(npc, result);
        }
        public bool MovementRework(NPC npc, bool result)
        {
            if (!npc.HasPlayerTarget)
            {
                npc.TargetClosest();
                Player p = Main.player[npc.target];
                if (!p.active || p.dead || Vector2.Distance(npc.Center, p.Center) > 5000f)
                {
                    npc.noTileCollide = true;
                    if (npc.timeLeft > 30)
                        npc.timeLeft = 30;
                    npc.velocity.Y += 1f;
                    return false;
                }
            }
            if (result && npc.HasPlayerTarget) // ai rework normal flying ai rework
            {
                if (npc.ai[0] != 0) // reset charge memory
                    ChargePosition = false;

                Player player = Main.player[npc.target];
                if (npc.ai[0] == 2) // replaces preparing bees
                {
                    result = false;
                    ref float timer = ref npc.ai[1];
                    float speedMod = 1f;
                    npc.spriteDirection = npc.direction = (int)npc.HorizontalDirectionTo(player.Center);

                    // speed
                    timer++;
                    int duration = 60 * 7;
                    float durationDiv = 1f;
                    float lifeFraction = npc.GetLifePercent();
                    if (lifeFraction < 0.75)
                        durationDiv += 0.12f;
                    if (lifeFraction < 0.5)
                        durationDiv += 0.12f;
                    if (lifeFraction < 0.25)
                        durationDiv += 0.12f;
                    if (lifeFraction < 0.1)
                        durationDiv += 0.12f;
                    if (NPC.AnyNPCs(ModContent.NPCType<RoyalSubject>()))
                        durationDiv = 1f;
                    duration = (int)Math.Round(duration / durationDiv);

                    float progress = timer / duration;

                    float positionFrac = 0.25f;
                    float spreadFrac = 110f / duration;
                    float straightFrac = 1f - spreadFrac;
                    if (progress < positionFrac) // get in position
                    {
                        speedMod = 1.5f;
                    }
                    else if (progress < straightFrac) // straight shots
                    {
                        speedMod = 0.8f;
                        float spread = MathHelper.PiOver2 * 0.05f;

                        float baseFrequency = 13f;
                        float frequency = (int)Math.Round(baseFrequency / durationDiv);
                        if (timer % frequency == 0)
                        {
                            float angle = Main.rand.NextFloat(-spread, spread);
                            SoundEngine.PlaySound(SoundID.Item97, npc.position);
                            float speed = 7f;
                            speed += 5f * ((progress - positionFrac) / straightFrac);
                            Shot(angle, 0f, speed);
                        }
                    }
                    else if (progress < 1) // spread shots
                    {
                        speedMod = 0.05f;
                        float spreadProgress = (progress - straightFrac) / (1f - straightFrac);
                        float spread = MathHelper.PiOver2 * 1.4f * spreadProgress;

                        float baseFrequency = 5;
                        float frequency = (int)Math.Round(baseFrequency / durationDiv);
                        if (frequency < 4)
                            frequency = 4;
                        if (timer % frequency == 0)
                        {
                            float angleTimer = timer - straightFrac * duration;
                            float angle = spread * MathF.Sin(MathF.Tau * angleTimer / 127f);
                            SoundEngine.PlaySound(SoundID.Item97, npc.position);
                            float speed = 12f;
                            Shot(angle, 0f, speed);
                        }

                        // bees
                        if (timer % 10 == 0 && NPC.CountNPCS(NPCID.Bee) + NPC.CountNPCS(NPCID.BeeSmall) < 6f)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit1, npc.position);
                            if (FargoSoulsUtil.HostCheck)
                            {
                                Vector2 vector84 = new Vector2(npc.position.X + (float)(npc.width / 2) + (float)(Main.rand.Next(20) * npc.direction), npc.position.Y + (float)npc.height * 0.8f);
                                int num670 = Main.rand.Next(210, 212);
                                int num671 = NPC.NewNPC(npc.GetSource_FromAI(), (int)vector84.X, (int)vector84.Y, num670);
                                Main.npc[num671].velocity = player.Center - npc.Center;
                                Main.npc[num671].velocity.Normalize();
                                NPC nPC3 = Main.npc[num671];
                                nPC3.velocity *= 5f;
                                Main.npc[num671].CanBeReplacedByOtherNPCs = true;
                                Main.npc[num671].localAI[0] = 60f;
                                Main.npc[num671].netUpdate = true;
                            }
                        }
                    }

                    // exit as normal
                    if (timer > duration)
                    {
                        Vector2 desiredPos = player.Center + player.DirectionTo(npc.Center) * 300f;
                        Movement(npc, desiredPos, 0.5f, (int)(10 + player.velocity.Length()));
                        if (npc.Distance(player.Center) < 400f && (npc.velocity.Length() < 4f || timer > duration * 1.25f))
                        {
                            npc.ai[0] = -1f;
                            npc.ai[1] = 1f;
                            npc.netUpdate = true;
                            return result;
                        }
                    }
                    else
                    {
                        float rectDist = 500f;
                        int side = (int)player.HorizontalDirectionTo(npc.Center);
                        int ySide = (int)Math.Sign(npc.Center.Y - player.Center.Y);
                        Vector2 desiredPos = player.Center + Vector2.UnitX * side * rectDist + Vector2.UnitY * ySide * rectDist;
                        Vector2 chosenPos = player.Center;
                        for (float i = 0.2f; i <= 1f; i += 0.05f)
                        {
                            Vector2 newchosenPos = Vector2.Lerp(player.Center, desiredPos, i);
                            if (!Collision.CanHitLine(newchosenPos - npc.Size / 2, npc.width, npc.height, player.Center, 1, 1))
                                break;
                            chosenPos = newchosenPos;
                        }
                        Movement(npc, chosenPos, speedMod, 20);
                    }
                }
                else if (npc.ai[0] == 3) // normal flying ai with stingers
                {
                    result = false;
                    float enrageFactor = 0f;
                    if ((double)(npc.position.Y / 16f) < Main.worldSurface)
                    {
                        enrageFactor += 1f;
                    }
                    if (!Main.player[npc.target].ZoneJungle)
                    {
                        enrageFactor += 1f;
                    }
                    if (Main.getGoodWorld)
                    {
                        enrageFactor += 0.5f;
                    }
                    npc.ai[1] += 1f;
                    float lifeFraction = npc.GetLifePercent();
                    int shotFrequency = lifeFraction < 0.1f ? 30 : lifeFraction < (1f / 3) ? 40 : lifeFraction <= 2 ? 45 : 50;
                    shotFrequency -= (int)(5f * enrageFactor);

                    // real AI
                    ref float timer = ref npc.ai[1];
                    if (timer - (int)timer > 0 && !NPC.AnyNPCs(ModContent.NPCType<RoyalSubject>()))
                        timer = (int)timer;
                    int cycle = (int)(shotFrequency * 2);
                    if (cycle < 50)
                        cycle = 50;
                    float cycleProgress = (timer / cycle) % 1;
                    npc.spriteDirection = npc.direction = (int)npc.HorizontalDirectionTo(player.Center);
                    if (timer % cycle <= 1) // start of cycle, get position
                    {
                        int maxIter = 40;
                        for (int i = 0; i < maxIter; i++)
                        {
                            float randomMax = MathHelper.PiOver2 * 0.25f;
                            float randomRot = Main.rand.NextFloat(-randomMax, randomMax);
                            LockVector1 = player.Center + player.DirectionTo(npc.Center).RotatedBy(randomRot) * 240f;
                            if (Collision.CanHit(LockVector1 - npc.Size / 2, npc.width, npc.height, player.Center, 1, 1))
                                break;
                        }
                        npc.netUpdate = true;

                    }
                    if (timer % cycle > 5) // give time for sync
                    {
                        float speedMod = 1f;
                        int maxSpeed = 15;
                        if (enrageFactor > 0)
                        {
                            maxSpeed = 25;
                            speedMod += 1f * enrageFactor;
                        }
                        speedMod += ((70f - shotFrequency) / 70f) * 0.4f;
                        if (cycleProgress < 0.45f) // relock if too close
                        {
                            float threshold = 160;
                            if (LockVector1.Distance(player.Center) < threshold)
                                LockVector1 = player.Center + player.DirectionTo(LockVector1) * threshold;
                        }
                        Vector2 desiredPos = LockVector1;
                        Movement(npc, desiredPos, speedMod, maxSpeed);
                    }
                    if (cycleProgress < 0.5f) // movement part of cycle
                    {
                        ShotTimer = 0;
                    }
                    else // attack part of cycle
                    {
                        if (timer - (int)timer < 0.4f)
                            ShotTimer++;

                        int frequency = lifeFraction < 0.1f ? 10 : lifeFraction < (1f / 3) ? 10 : lifeFraction <= 2 ? 12 : 14;
                        //frequency += 2;
                        if (ShotTimer % frequency == 0 && timer - (int)timer < 0.4f)
                        {
                            float spreadFactor = (float)ShotTimer / frequency;
                            spreadFactor = (int)(spreadFactor);
                            spreadFactor -= 1;
                            float spread = spreadFactor * MathHelper.PiOver2 * 0.18f;
                            if (spreadFactor < 3)
                            {
                                SoundEngine.PlaySound(SoundID.Item17, npc.position);
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    int stop = 0;
                                    if (spreadFactor >= 1)
                                        stop = 1;
                                    else
                                        spread = 0;
                                    for (int i = -1; i <= stop; i += 2)
                                    {
                                        Shot(spread * i, enrageFactor);
                                    }
                                }
                            }
                        }
                    }
                    float durationScale = 20f;
                    durationScale -= 5f * enrageFactor;
                    if (npc.ai[1] > (float)shotFrequency * durationScale)
                    {
                        npc.ai[0] = -1f;
                        npc.ai[1] = 3f;
                        npc.netUpdate = true;
                    }
                }
                else if (npc.ai[0] == 0) // charging 
                {
                    if (!ChargePosition)
                    {
                        float distX = MathF.Abs(npc.Center.X - player.Center.X);
                        float desiredDist = 550f;
                        if (distX < desiredDist)
                        {
                            Vector2 desiredPos = player.Center + Vector2.UnitX * player.HorizontalDirectionTo(npc.Center) * (desiredDist + 50);
                            Movement(npc, desiredPos, 2f, (int)(10 + player.velocity.Length()));
                            result = false;
                        }
                        else
                        {
                            ChargePosition = true;
                            npc.netUpdate = true;
                        }
                    }
                }
            }
            void Shot(float angle, float enrageFactor, float speed = 10f)
            {
                if (FargoSoulsUtil.HostCheck)
                {
                    speed += 7f * enrageFactor;
                    Vector2 pos = new Vector2(npc.Center.X, npc.position.Y + npc.height * 0.8f);
                    int dmg = 11;
                    int type = ProjectileID.QueenBeeStinger;
                    Vector2 vel = npc.DirectionTo(Main.player[npc.target].Center) * speed;
                    vel = vel.RotatedBy(angle);
                    int num688 = Projectile.NewProjectile(npc.GetSource_FromAI(), pos, vel, type, dmg, 0f, Main.myPlayer);
                    Main.projectile[num688].timeLeft = 300;
                }
            }
            return result;
        }
        public void Movement(NPC npc, Vector2 desiredPos, float speedMod, int maxMovementSpeed)
        {
            float Acceleration = 0.25f;

            speedMod *= 1.6f;
            float accel = Acceleration * speedMod;
            float decel = Acceleration * 5 * speedMod;
            float max = maxMovementSpeed * speedMod;
            float resistance = npc.velocity.Length() * accel / max;
            npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, desiredPos, npc.velocity, accel - resistance, decel + resistance);
        }
        public override void SafePostAI(NPC npc)
        {
            base.SafePostAI(npc);

            if (!npc.HasValidTarget || npc.HasPlayerTarget && npc.Distance(Main.player[npc.target].Center) > 3000)
            {
                if (npc.timeLeft > 60)
                    npc.timeLeft = 60;
            }
        }

        public override void OnKill(NPC npc)
        {
            base.OnKill(npc);

            if ((int)(Main.time / 60 - 30) % 60 == 22) //COOMEDY
                Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ModContent.ItemType<TwentyTwoPainting>());
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<InfestedBuff>(), 300);
            //target.AddBuff(ModContent.BuffType<SwarmingBuff>(), 600);

            
            if (npc.ai[0] == 0 && WorldSavingSystem.MasochistModeReal) //in dash mode
            {
                target.AddBuff(BuffID.BrokenArmor, 60 * 5);
            }
            
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (SubjectDR)
                modifiers.FinalDamage /= 3;

            base.ModifyIncomingHit(npc, ref modifiers);
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.lifeRegen >= 0)
                return;
            if (SubjectDR)
            {
                npc.lifeRegen /= 2;
                damage /= 2;
            }
        }
        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadBossHeadSprite(recolor, 14);
            LoadGoreRange(recolor, 303, 308);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (SubjectDR && !npc.IsABestiaryIconDummy)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

                ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(ItemID.ReflectiveSilverDye);
                shader.Apply(npc, new Terraria.DataStructures.DrawData?());
            }
            return true;
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (SubjectDR && !npc.IsABestiaryIconDummy)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }
        }
    }
}
