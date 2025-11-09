using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Deathrays;
using FargowiltasSouls.Content.Projectiles.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.MechanicalBosses;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using FargowiltasSouls.Content.Projectiles.Masomode.Bosses.MechanicalBosses;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class Retinazer : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Retinazer);

        public int DeathrayState;
        public int LaserSide;
        public float LockedRotation;
        public int AuraRadiusCounter;
        public int MechElectricOrbTimer;

        public bool StoredDirectionToPlayer;

        public bool Phase2;
        public bool DroppedSummon;

        public bool HasSaidEndure;
        public bool Resist;
        public int RespawnTimer;

        public bool ShouldDrawAura = false;
        public float AuraOpacity = 0f;

        public float AuraRadius()
        {
            float radius = 2000 - 1200 * AuraRadiusCounter / 180f;
            if (WorldSavingSystem.MasochistModeReal)
                radius *= 0.75f;
            return radius;
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(LaserSide);
            binaryWriter.Write(DeathrayState);
            binaryWriter.Write(AuraRadiusCounter);
            binaryWriter.Write(MechElectricOrbTimer);
            bitWriter.WriteBit(StoredDirectionToPlayer);
            bitWriter.WriteBit(ShouldDrawAura);
            binaryWriter.Write(LockedRotation);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            LaserSide = binaryReader.ReadInt32();
            DeathrayState = binaryReader.ReadInt32();
            AuraRadiusCounter = binaryReader.ReadInt32();
            MechElectricOrbTimer = binaryReader.ReadInt32();
            StoredDirectionToPlayer = bitReader.ReadBit();
            ShouldDrawAura = bitReader.ReadBit();
            LockedRotation = binaryReader.ReadSingle();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax = (int)(npc.lifeMax * 1.2);
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Suffocation] = true;
        }
        public static void TwinDefaultMovement(NPC npc, float desiredX, float desiredY, float accel, float decelMult)
        {
            if (npc.velocity.X < desiredX)
            {
                npc.velocity.X += accel;
                if (npc.velocity.X < 0f && desiredX > 0f)
                {
                    npc.velocity.X += accel * decelMult;
                }
            }
            else if (npc.velocity.X > desiredX)
            {
                npc.velocity.X -= accel;
                if (npc.velocity.X > 0f && desiredX < 0f)
                {
                    npc.velocity.X -= accel * decelMult;
                }
            }
            if (npc.velocity.Y < desiredY)
            {
                npc.velocity.Y += accel;
                if (npc.velocity.Y < 0f && desiredY > 0f)
                {
                    npc.velocity.Y += accel * decelMult;
                }
            }
            else if (npc.velocity.Y > desiredY)
            {
                npc.velocity.Y -= accel;
                if (npc.velocity.Y > 0f && desiredY < 0f)
                {
                    npc.velocity.Y -= accel * decelMult;
                }
            }
        }
        public static void TwinManageRotation(NPC npc)
        {
            if (!npc.HasPlayerTarget)
                return;
            float num412 = npc.Center.X - Main.player[npc.target].Center.X;
            float num413 = npc.Center.Y - Main.player[npc.target].Center.Y;
            float num414 = (float)Math.Atan2(num413, num412) + 1.57f;
            if (num414 < 0f)
            {
                num414 += 6.283f;
            }
            else if ((double)num414 > 6.283)
            {
                num414 -= 6.283f;
            }
            float num415 = 0.1f;
            if (npc.rotation < num414)
            {
                if ((double)(num414 - npc.rotation) > 3.1415)
                {
                    npc.rotation -= num415;
                }
                else
                {
                    npc.rotation += num415;
                }
            }
            else if (npc.rotation > num414)
            {
                if ((double)(npc.rotation - num414) > 3.1415)
                {
                    npc.rotation += num415;
                }
                else
                {
                    npc.rotation -= num415;
                }
            }
            if (npc.rotation > num414 - num415 && npc.rotation < num414 + num415)
            {
                npc.rotation = num414;
            }
            if (npc.rotation < 0f)
            {
                npc.rotation += 6.283f;
            }
            else if ((double)npc.rotation > 6.283)
            {
                npc.rotation -= 6.283f;
            }
            if (npc.rotation > num414 - num415 && npc.rotation < num414 + num415)
            {
                npc.rotation = num414;
            }
        }
        public override bool SafePreAI(NPC npc)
        {
            EModeGlobalNPC.retiBoss = npc.whoAmI;

            Resist = false;

            if (!npc.HasValidTarget || !Main.player[npc.target].active || Main.player[npc.target].dead)
            {
                npc.TargetClosest();
                Player p = Main.player[npc.target];
                if (!npc.HasValidTarget || !p.active || p.dead)
                {
                    npc.noTileCollide = true;
                    if (npc.timeLeft > 30)
                        npc.timeLeft = 30;

                    if (npc.velocity.Y > 0)
                        npc.velocity.Y = 0;
                    npc.velocity.Y -= 0.5f;
                    return false;
                }
            }

            //have some dr during phase transition animation
            if (npc.ai[0] == 1 || npc.ai[0] == 2)
                Resist = true;

            NPC spazmatism = FargoSoulsUtil.NPCExists(EModeGlobalNPC.spazBoss, NPCID.Spazmatism);

            if (WorldSavingSystem.MasochistModeReal && spazmatism == null && npc.HasValidTarget && ++RespawnTimer > 1200)
            {
                RespawnTimer = 0;
                if (FargoSoulsUtil.HostCheck)
                {
                    int n = FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromThis(), npc.Center + new Vector2(Main.rand.NextFloat(-1000, 1000), Main.rand.NextFloat(-800, -600)), NPCID.Spazmatism, target: npc.target);
                    if (n != Main.maxNPCs)
                    {
                        Main.npc[n].life = Main.npc[n].lifeMax / 4;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, number: n);
                        FargoSoulsUtil.PrintLocalization($"Mods.{Mod.Name}.NPCs.EMode.TwinsRevive", new Color(175, 75, 255), Main.npc[n].FullName);
                    }
                }
            }
            
            if (!Phase2) //start phase 2
            {
                if (npc.GetLifePercent() < 0.66f || (spazmatism != null && spazmatism.GetLifePercent() < 0.66f))
                {
                    Phase2 = true;
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0.0f;
                    npc.ai[2] = 0.0f;
                    npc.ai[3] = 0.0f;
                    npc.netUpdate = true;
                }
            }

            if (npc.life <= npc.lifeMax / 2 || npc.dontTakeDamage)
            {
                npc.dontTakeDamage = npc.life == 1 || !npc.HasValidTarget;
                if (npc.life != 1 && npc.HasValidTarget)
                    npc.dontTakeDamage = false;
                //become vulnerable again when both twins at low life
                if (npc.dontTakeDamage && npc.HasValidTarget && (spazmatism == null || spazmatism.life == 1))
                    npc.dontTakeDamage = false;
            }

            if (Main.dayTime && !Main.remixWorld)
            {
                if (npc.velocity.Y > 0)
                    npc.velocity.Y = 0;

                npc.velocity.Y -= 0.5f;
                npc.dontTakeDamage = true;

                if (spazmatism != null)
                {
                    if (npc.timeLeft < 60)
                        npc.timeLeft = 60;

                    if (spazmatism.timeLeft < 60)
                        spazmatism.timeLeft = 60;

                    npc.TargetClosest(false);
                    spazmatism.TargetClosest(false);
                    if (npc.Distance(Main.player[npc.target].Center) > 2000 && spazmatism.Distance(Main.player[spazmatism.target].Center) > 2000)
                    {
                        if (FargoSoulsUtil.HostCheck)
                        {
                            npc.active = false;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                            spazmatism.active = false;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, EModeGlobalNPC.spazBoss);
                        }
                    }
                }

                return true;
            }
            if (!Phase2) // phase 1
            {
                npc.damage = npc.defDamage;
                ref float ai_State = ref npc.ai[1];
                ref float ai_StateTimer = ref npc.ai[2];
                ref float ai_ShotTimer = ref npc.ai[3];

                if (!npc.HasPlayerTarget || Main.IsItDay())
                    return true;
                Player player = Main.player[npc.target];

                switch (ai_State)
                {
                    case 0: //normal laser state
                        {
                            int stateTime = 600;
                            if (ai_StateTimer >= stateTime - 1)
                            {
                                ai_State = 1f;
                                ai_StateTimer = 0f;
                                ai_ShotTimer = 0f;
                                npc.netUpdate = true;
                                npc.TargetClosest();
                                goto case 1;
                            }

                            // movement
                            float accel = 0.22f;
                            int side = 1;
                            if (npc.position.X + (float)(npc.width / 2) < player.position.X + (float)player.width)
                            {
                                side = -1;
                            }

                            // up position, default retinazer
                            float desiredX = player.position.X + (float)(player.width / 2) + (float)(side * 350) - npc.Center.X;
                            float desiredY = player.position.Y + (float)(player.height / 2) - 350f - npc.Center.Y;
                            TwinDefaultMovement(npc, desiredX, desiredY, accel, 2);

                            // reworked p1 lasers
                            float delay = 55f; //LaserSide == 0 ? 50f : 20f;
                            ai_ShotTimer -= 0.2f;
                            if (ai_ShotTimer >= delay)
                            {
                                LaserSide++;
                                if (LaserSide > 1)
                                    LaserSide = 0;
                                ai_ShotTimer = 0f;
                                Vector2 shootPos = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
                                float targetX = player.Center.X - shootPos.X;
                                float targetY = player.Center.Y - shootPos.Y;

                                if (targetX * targetX + targetY * targetY > 800 * 800) // don't fire if too far
                                    break;
                                npc.netUpdate = true;
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    float num429 = 10.5f;
                                    int projDamage = npc.GetAttackDamage_ForProjectiles(20f, 19f);
                                    projDamage = (int)(projDamage * 1.25f);
                                    int num430 = 83;

                                    float angle = (float)Math.Sqrt(targetX * targetX + targetY * targetY);
                                    angle = num429 / angle;
                                    targetX *= angle;
                                    targetY *= angle;
                                    // terrible vanilla random
                                    //targetX += (float)Main.rand.Next(-40, 41) * 0.08f;
                                    //targetY += (float)Main.rand.Next(-40, 41) * 0.08f;

                                    targetX *= 0.6f;
                                    targetY *= 0.6f;
                                    Vector2 vel = new(targetX, targetY);
                                    shootPos.X += targetX * 10f;
                                    shootPos.Y += targetY * 10f;
                                    int spread = 1;
                                    if (LaserSide == 1) // every other shot is 1, every other is 2 (3 in maso)
                                        spread = 0;
                                    for (int i = -spread; i <= spread; i++)
                                    {
                                        if (spread != 0 && i == 0) // && !WorldSavingSystem.MasochistModeReal)
                                            continue;
                                        float offset = i;
                                        offset *= (int)npc.HorizontalDirectionTo(player.Center);
                                        Vector2 vel2 = vel.RotatedBy(MathHelper.PiOver2 * 0.15f * offset);
                                        //shootPos += vel2 * 6f;
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), shootPos.X, shootPos.Y, vel2.X, vel2.Y, num430, projDamage, 0f, Main.myPlayer);
                                    }
                                    npc.velocity -= vel;

                                }
                            }
                        }
                        break;
                    case 1: // dash frame
                        {
                            float prepTime = WorldSavingSystem.MasochistModeReal ? 95 : 105;

                            if (ai_StateTimer == 0)
                            {
                                LockedRotation = player.DirectionTo(npc.Center).ToRotation();
                                int dir = Main.rand.NextBool() ? 1 : -1;
                                // rotate away from spazmatism
                                if (spazmatism != null && spazmatism.TypeAlive(NPCID.Spazmatism)) 
                                {
                                    float spazmatism_Angle = player.DirectionTo(spazmatism.Center).ToRotation();
                                    dir = MathF.Sign(LockedRotation - spazmatism_Angle);
                                }
                                LockedRotation += dir * MathHelper.PiOver2 * (0.25f + Main.rand.NextFloat(0.2f));
                            }

                            // lock on to spot next to player
                            float distance = WorldSavingSystem.MasochistModeReal ? 380f : 450f;
                            Vector2 desiredPos = player.Center + LockedRotation.ToRotationVector2() * distance;
                            float desiredX = desiredPos.X - npc.Center.X;
                            float desiredY = desiredPos.Y - npc.Center.Y;

                            float accel = 0.6f;
                            TwinDefaultMovement(npc, desiredX, desiredY, accel, 4);
                            TwinManageRotation(npc);

                            bool waitForSpaz = spazmatism.ai[1] == npc.ai[1];
                            Vector2 spazDesiredPos = player.Center + spazmatism.GetGlobalNPC<Spazmatism>().LockedRotation.ToRotationVector2() * distance;
                            if (spazmatism.Distance(spazDesiredPos) <= 300)
                                waitForSpaz = false;

                            if (ai_StateTimer < prepTime - 30 && (npc.Distance(desiredPos) > 300 || waitForSpaz))
                                ai_StateTimer--;
                            int flashDelay = WorldSavingSystem.MasochistModeReal ? 35 : 45;
                            if (ai_StateTimer < prepTime - flashDelay && spazmatism.ai[2] < npc.ai[2])
                                npc.ai[2] = spazmatism.ai[2];

                            if (ai_StateTimer == prepTime - flashDelay)
                            {
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + TwinsEyeFlash.Offset(npc), Vector2.Zero, ModContent.ProjectileType<TwinsEyeFlash>(), 0, 0, Main.myPlayer, npc.whoAmI);
                                }
                            }

                            if (++ai_StateTimer >= prepTime)
                            {
                                npc.velocity = npc.DirectionTo(player.Center) * 28;
                                ai_StateTimer = 0;
                                LockedRotation = 0;
                                ai_State = 2;
                            }
                            return false;
                        }
                    case 2: // mid dash
                        {
                            npc.damage = (int)(npc.defDamage * 1.5f);
                            ai_StateTimer += 1f;
                            if (ai_StateTimer >= 25f)
                            {
                                ai_ShotTimer += 1f;
                                ai_StateTimer = 0f;
                                npc.TargetClosest();
                                if (ai_ShotTimer >= 4f)
                                {
                                    ai_State = 0f;
                                    ai_ShotTimer = 0f;
                                }
                                else
                                {
                                    ai_State = 1f;
                                }
                            }
                            else
                            {
                                npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) - 1.57f;
                                if (ai_StateTimer >= 20)
                                    npc.velocity *= 0.95f;
                            }
                            return false;
                        }
                }

            }
            else
            {
                npc.damage = (int)(npc.defDamage * 1.5f);

                if (npc.ai[1] != 0)
                    npc.localAI[1] -= 1f;
                npc.localAI[1] -= 0.2f;
                if (npc.localAI[1] >= (npc.ai[1] == 0 ? 170 : 50)) //hijacking vanilla laser code
                {
                    npc.localAI[1] = 0;
                    Vector2 vel = npc.SafeDirectionTo(Main.player[npc.target].Center);
                    Vector2 shotVel = vel;
                    shotVel *= 20;
                    int type = ModContent.ProjectileType<MechElectricOrb>();
                    bool middle = true;
                    float spreadAngle = 0.4f;
                    int spread = 0; // 1;
                    if (npc.ai[1] == 0 && Main.getGoodWorld)
                    {
                        spread = 1;
                    }
                        
                    for (int i = -spread; i <= spread; i++)
                    {
                        if (i == 0 && spread != 0 && !middle)
                            continue;
                        Vector2 shotVel2 = shotVel.RotatedBy(MathHelper.PiOver2 * spreadAngle * i);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + (npc.width - 24) * vel, shotVel2, type, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, npc.target, ai2: MechElectricOrb.Yellow);
                        for (int j = -3; j <= 3; j++)
                        {
                            Vector2 particleVel = (shotVel2 * 1.1f).RotatedBy(MathHelper.PiOver2 * 0.075f * j).RotatedByRandom(MathHelper.PiOver2 * 0.04f) * Main.rand.NextFloat(0.8f, 1.2f);
                            Particle p = new ElectricSpark(npc.Center + (npc.width - 24) * vel - shotVel2, particleVel, Color.Yellow, Main.rand.NextFloat(0.7f, 1f), 20);
                            p.Spawn();
                        }
                    }
                    if (npc.ai[1] == 0)
                        shotVel *= 2;
                    npc.velocity -= shotVel / 7f;
                    

                }

                if (npc.HasPlayerTarget && (DeathrayState == 0 || DeathrayState == 3)) // normal AI
                {
                    Player player = Main.player[npc.target];
                    if (npc.ai[1] == 0f) // normal slow lasers
                    {

                        // movement
                        float num438 = 12.5f;
                        float accel = 0.22f;

                        Vector2 vector45 = npc.Center;
                        float desiredX = player.Center.X - npc.Center.X;
                        float desiredY = player.Center.Y - npc.Center.Y - 450f;
                        float num442 = (float)Math.Sqrt(desiredX * desiredX + desiredY * desiredY);
                        num442 = num438 / num442;
                        desiredX *= num442;
                        desiredY *= num442;

                        TwinDefaultMovement(npc, desiredX, desiredY, accel, 2);
                    }
                }
                if (npc.ai[0] < 4f) //going to phase 3
                {
                    if (npc.life <= npc.lifeMax / 2 || (spazmatism != null && spazmatism.life <= spazmatism.lifeMax / 2))
                    {
                        //npc.ai[0] = 4f;
                        npc.ai[0] = 604f; //initiate spin immediately
                        npc.netUpdate = true;
                        //SoundEngine.PlaySound(SoundID.Roar, npc.Center);

                        ShouldDrawAura = true;

                        //if (FargoSoulsUtil.HostCheck)
                        //    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 11, npc.whoAmI);
                    }
                }
                else //in phase 3
                {

                    if (WorldSavingSystem.MasochistModeReal && spazmatism == null && --MechElectricOrbTimer < 0) //when twin dead, begin shooting Electric Orbs
                    {
                        MechElectricOrbTimer = 240;
                        if (FargoSoulsUtil.HostCheck && npc.HasPlayerTarget)
                        {
                            Vector2 distance = Main.player[npc.target].Center - npc.Center;
                            distance.Normalize();
                            distance *= 10f;
                            for (int i = 0; i < 12; i++)
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, distance.RotatedBy(2 * Math.PI / 12 * i),
                                    ModContent.ProjectileType<MechElectricOrb>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 0f, Main.myPlayer, ai2: MechElectricOrb.Yellow);
                        }
                    }

                    //dust code
                    /*
                    if (DeathrayState != 0 && DeathrayState != 3 && Main.rand.Next(4) < 3)
                    {
                        int dustID = DeathrayState != 0 ? DustID.GemRuby : DustID.GemAmber;
                        int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, dustID, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 3.5f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= 1.8f;
                        Main.dust[dust].velocity.Y -= 0.5f;
                        if (Main.rand.NextBool(4))
                        {
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].scale *= 0.5f;
                        }
                    }
                    */

                    if (DeathrayState == 0 || DeathrayState == 3) //not doing deathray, grow arena
                    {
                        AuraRadiusCounter--;
                        if (AuraRadiusCounter < 0)
                            AuraRadiusCounter = 0;
                    }
                    else //doing deathray, shrink arena
                    {
                        AuraRadiusCounter++;
                        if (AuraRadiusCounter > 180)
                            AuraRadiusCounter = 180;
                    }

                    float auraDistance = AuraRadius();
                    if (auraDistance < 2000 - 1)
                    {
                        if ((DeathrayState == 1 && npc.ai[0] >= 155f) || DeathrayState == 2)
                            EModeGlobalNPC.Aura(npc, auraDistance, true, -1, default, ModContent.BuffType<DaybrokenBuff>());
                            
                        float threshold = auraDistance;

                        Player localPlayer = Main.LocalPlayer;
                        float distance = localPlayer.Distance(npc.Center);
                        if (localPlayer.active && !localPlayer.dead && !localPlayer.ghost) //pull into arena
                        {
                            if (distance > threshold && distance < threshold * 4f)
                            {
                                if (distance > threshold * 2f)
                                {
                                    localPlayer.Incapacitate();
                                    localPlayer.velocity.X = 0f;
                                    localPlayer.velocity.Y = -0.4f;
                                }

                                Vector2 movement = npc.Center - localPlayer.Center;
                                float difference = movement.Length() - threshold;
                                movement.Normalize();
                                movement *= difference < 30f ? difference : 30f;
                                localPlayer.position += movement;
                            }
                        }
                    }

                    //2*pi * (# of full circles) / (seconds to finish rotation) / (ticks per sec)
                    float rotationInterval = 2f * (float)Math.PI * 1.2f / 4f / 60f;
                    if (WorldSavingSystem.MasochistModeReal)
                        rotationInterval *= 1.025f;

                    npc.ai[0]++; //base value is 4
                    switch (DeathrayState) //laser code idfk
                    {
                        case 0:
                            if (!npc.HasValidTarget)
                            {
                                npc.ai[0]--; //stop counting up if player is dead
                                if (spazmatism == null) //despawn REALLY fast
                                    npc.velocity.Y -= 0.5f;
                            }
                            if (npc.ai[0] > 604f)
                            {
                                npc.ai[0] = 4f;
                                if (npc.HasPlayerTarget)
                                {
                                    npc.rotation = npc.Center.X < Main.player[npc.target].Center.X ? 0 : (float)Math.PI;
                                    npc.rotation -= (float)Math.PI / 2;

                                    DeathrayState++;
                                    npc.ai[3] = -npc.rotation;
                                    if (--npc.ai[2] > 295f)
                                        npc.ai[2] = 295f;
                                    StoredDirectionToPlayer = Main.player[npc.target].Center.X - npc.Center.X < 0;

                                    if (FargoSoulsUtil.HostCheck)
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, npc.whoAmI, npc.type);

                                    SoundEngine.PlaySound(FargosSoundRegistry.TwinsWarning with {Volume = 4f }, npc.Center); 
                                }

                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                                NetSync(npc);
                            }
                            break;

                        case 1: //slowing down, beginning rotation
                            if (npc.HasPlayerTarget)
                            {
                                Player player = Main.player[npc.target];
                                Vector2 pos = player.Center + player.DirectionTo(npc.Center) * 250;
                                npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, pos, npc.velocity, 0.9f, 0.9f);
                            }
                            npc.velocity *= 1f - (npc.ai[0] - 4f) / 120f;
                            npc.localAI[1] = 0f;
                            //if (--npc.ai[2] > 295f) npc.ai[2] = 295f;
                            npc.ai[3] -= (npc.ai[0] - 4f) / 120f * rotationInterval * (StoredDirectionToPlayer ? 1f : -1f);
                            npc.rotation = -npc.ai[3];
                            

                            if (npc.ai[0] == 35f)
                            {
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowLine>(), 0, 0f, Main.myPlayer, 9f, npc.whoAmI);
                                }
                            }

                            if (npc.ai[0] >= 155f) //FIRE LASER
                            {
                                if (!Main.dedServ)
                                    SoundEngine.PlaySound(FargosSoundRegistry.TwinsDeathray with { Volume = 2f }, npc.Center);
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    Vector2 speed = Vector2.UnitX.RotatedBy(npc.rotation);
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, speed, ModContent.ProjectileType<RetinazerDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 4f / 3), 0f, Main.myPlayer, 0f, npc.whoAmI);
                                }
                                DeathrayState++;
                                npc.ai[0] = 4f;

                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                                NetSync(npc);
                            }
                            return false;

                        case 2: //spinning full speed
                            npc.velocity = Vector2.Zero;
                            npc.localAI[1] = 0f;
                            //if (--npc.ai[2] > 295f) npc.ai[2] = 295f;
                            npc.ai[3] -= rotationInterval * (StoredDirectionToPlayer ? 1f : -1f);
                            npc.rotation = -npc.ai[3];

                            if (npc.ai[0] >= 244f)
                            {
                                DeathrayState++;
                                npc.ai[0] = 4f;

                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                                NetSync(npc);
                            }
                            else if (!npc.HasValidTarget) //end spin immediately if player dead
                            {
                                npc.TargetClosest(false);
                                if (!npc.HasValidTarget)
                                    npc.ai[0] = 244f;
                            }
                            return false;

                        case 3: //laser done, slowing down spin, moving again
                            npc.velocity *= (npc.ai[0] - 4f) / 60f;
                            npc.localAI[1] = 0f;
                            //if (--npc.ai[2] > 295f) npc.ai[2] = 295f;
                            npc.ai[3] -= (1f - (npc.ai[0] - 4f) / 60f) * rotationInterval * (StoredDirectionToPlayer ? 1f : -1f);
                            npc.rotation = -npc.ai[3];

                            if (npc.ai[0] >= 64f)
                            {
                                DeathrayState = 0;
                                npc.ai[0] = 4f;

                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                                NetSync(npc);
                            }
                            return false;

                        default:
                            DeathrayState = 0;
                            npc.ai[0] = 4f;
                            npc.netUpdate = true;
                            NetSync(npc);
                            break;
                    }
                }
            }


            if (DeathrayState > 0)
                Resist = true;

            EModeUtils.DropSummon(npc, ItemID.MechanicalEye, NPC.downedMechBoss2, ref DroppedSummon, Main.hardMode);

            return true;
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            return Spazmatism.CircularHitbox(npc, target.position, target.Size);
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (Resist)
                modifiers.FinalDamage /= 2;

            base.ModifyIncomingHit(npc, ref modifiers);
        }

        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            //if (npc.ai[0] < 4 || DeathrayState == 0f || DeathrayState == 3f)
                return base.GetAlpha(npc, drawColor);
           // return new Color(255, drawColor.G / 2, drawColor.B / 2);
        }

        public override bool CheckDead(NPC npc)
        {
            if (WorldSavingSystem.MasochistModeReal)
                return base.CheckDead(npc);

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.spazBoss, NPCID.Spazmatism) && Main.npc[EModeGlobalNPC.spazBoss].life > 1) //spaz still active
            {
                npc.life = 1;
                npc.active = true;
                if (FargoSoulsUtil.HostCheck)
                    npc.netUpdate = true;

                if (!HasSaidEndure)
                {
                    HasSaidEndure = true;
                    FargoSoulsUtil.PrintLocalization($"Mods.{Mod.Name}.NPCs.EMode.TwinsEndure", new Color(175, 75, 255), npc.FullName);
                }
                return false;
            }

            return base.CheckDead(npc);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 AuraPosition = npc.Center;
            if (ShouldDrawAura)
                DrawAura(npc, spriteBatch, AuraPosition);

            Vector2 offset = new Vector2(npc.width - 24, 0).RotatedBy(npc.rotation + 1.57079637);
            Vector2 position = npc.Center + offset;
            Texture2D flare = MiscTexturesRegistry.BloomFlare.Value;
            float flarescale = Main.rand.NextFloat(0.1f, 0.15f);
            

            if (npc.GetGlobalNPC<Retinazer>().DeathrayState == 2)
            {
                Main.spriteBatch.Draw(flare, position - Main.screenPosition, null, Color.Red with {A= 0}, Main.GlobalTimeWrappedHourly * -2f, flare.Size() * 0.5f, flarescale, 0, 0f);
                Main.spriteBatch.Draw(flare, position - Main.screenPosition, null, Color.Red with {A = 0}, Main.GlobalTimeWrappedHourly * 2f, flare.Size() * 0.5f, flarescale, 0, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            }

            ref float ai_State = ref npc.ai[1];
            ref float ai_StateTimer = ref npc.ai[2];
            ref float ai_ShotTimer = ref npc.ai[3];

            float delay = 55;
            if (!Phase2 && ai_State == 0 && ai_ShotTimer < delay && LaserSide == 1)
            {
                Color color = Color.DarkMagenta;
                Asset<Texture2D> line = TextureAssets.Extra[178];
                float opacity = 0.6f * ai_ShotTimer / delay;
                for (int i = -1; i <= 1; i += 2)
                {
                    float rot = npc.rotation + MathHelper.PiOver2;
                    rot += MathHelper.PiOver2 * 0.15f * i;
                    Main.EntitySpriteDraw(line.Value, npc.Center + TwinsEyeFlash.Offset(npc) - Main.screenPosition + new Vector2(0f, npc.gfxOffY), null, color * opacity, rot, new Vector2(0, line.Height() * 0.5f), new Vector2(0.22f, npc.scale * 5), SpriteEffects.None);
                }
                
            }
                
            return true;
        }
        public void DrawAura(NPC npc, SpriteBatch spriteBatch, Vector2 position)
        {
            if (AuraOpacity < 1f)
                AuraOpacity += 0.05f;

            Color darkColor = Color.DarkRed;
            Color mediumColor = Color.Red;
            Color lightColor2 = Color.Lerp(Color.IndianRed, Color.White, 0.35f);
            Vector2 auraPos = position;
            float radius = AuraRadius();
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.Techno1Noise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return;
            var maxOpacity = npc.Opacity * AuraOpacity;

            ManagedShader borderShader = ShaderManager.GetShader("FargowiltasSouls.TwinsAuraShader");
            borderShader.TrySetParameter("colorMult", 7.35f); 
            borderShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            borderShader.TrySetParameter("radius", radius);
            borderShader.TrySetParameter("anchorPoint", auraPos);
            borderShader.TrySetParameter("screenPosition", Main.screenPosition);
            borderShader.TrySetParameter("screenSize", Main.ScreenSize.ToVector2());
            borderShader.TrySetParameter("maxOpacity", maxOpacity);
            borderShader.TrySetParameter("darkColor", darkColor.ToVector4());
            borderShader.TrySetParameter("midColor", mediumColor.ToVector4());
            borderShader.TrySetParameter("lightColor", lightColor2.ToVector4());

            spriteBatch.GraphicsDevice.Textures[1] = diagonalNoise.Value;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, borderShader.WrappedEffect, Main.GameViewMatrix.TransformationMatrix);
            Rectangle rekt = new(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth, Main.screenHeight);
            spriteBatch.Draw(blackTile.Value, rekt, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadBossHeadSprite(recolor, 15);
            LoadBossHeadSprite(recolor, 20);
            LoadGoreRange(recolor, 143, 146);

            LoadSpecial(recolor, ref TextureAssets.Chain12, ref FargowiltasSouls.TextureBuffer.Chain12, "Chain12");
            LoadSpecial(recolor, ref TextureAssets.EyeLaser, ref FargowiltasSouls.TextureBuffer.EyeLaser, "Eye_Laser");
        }
    }

    public class Spazmatism : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Spazmatism);

        public int ProjectileTimer;
        public float LockedRotation;
        public int FlameWheelSpreadTimer;
        public int FlameWheelCount;
        public int MechElectricOrbTimer;
        public int P3DashPhaseDelay;

        public bool Phase2;
        public bool HasSaidEndure;
        public bool Resist;
        public float RealRotation;
        public int RespawnTimer;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(ProjectileTimer);
            binaryWriter.Write(FlameWheelSpreadTimer);
            binaryWriter.Write(FlameWheelCount);
            binaryWriter.Write(MechElectricOrbTimer);
            binaryWriter.Write(P3DashPhaseDelay);
            binaryWriter.Write(LockedRotation);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            ProjectileTimer = binaryReader.ReadInt32();
            FlameWheelSpreadTimer = binaryReader.ReadInt32();
            FlameWheelCount = binaryReader.ReadInt32();
            MechElectricOrbTimer = binaryReader.ReadInt32();
            P3DashPhaseDelay = binaryReader.ReadInt32();
            LockedRotation = binaryReader.ReadSingle();
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Suffocation] = true;
        }
        public override bool SafePreAI(NPC npc)
        {
            EModeGlobalNPC.spazBoss = npc.whoAmI;

            Resist = false;

            if (!npc.HasValidTarget || !Main.player[npc.target].active || Main.player[npc.target].dead)
            {
                npc.TargetClosest();
                Player p = Main.player[npc.target];
                if (!npc.HasValidTarget || !p.active || p.dead)
                {
                    npc.noTileCollide = true;
                    if (npc.timeLeft > 30)
                        npc.timeLeft = 30;

                    if (npc.velocity.Y > 0)
                        npc.velocity.Y = 0;
                    npc.velocity.Y -= 0.5f;
                    return false;
                }
            }

            //have some dr during phase transition animation
            if (npc.ai[0] == 1 || npc.ai[0] == 2)
                Resist = true;

            NPC retinazer = FargoSoulsUtil.NPCExists(EModeGlobalNPC.retiBoss, NPCID.Retinazer);

            if (WorldSavingSystem.MasochistModeReal && retinazer == null && npc.HasValidTarget && ++RespawnTimer > 1200)
            {
                RespawnTimer = 0;
                if (FargoSoulsUtil.HostCheck)
                {
                    int n = FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromThis(), npc.Center + new Vector2(Main.rand.NextFloat(-1000, 1000), Main.rand.NextFloat(-800, -600)), NPCID.Retinazer, target: npc.target);
                    if (n != Main.maxNPCs)
                    {
                        Main.npc[n].life = Main.npc[n].lifeMax / 4;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, number: n);
                        FargoSoulsUtil.PrintLocalization($"Mods.{Mod.Name}.NPCs.EMode.TwinsRevive", new Color(175, 75, 255), Main.npc[n].FullName);
                    }
                }
            }

            float modifier = (float)npc.life / npc.lifeMax;
            if (WorldSavingSystem.MasochistModeReal)
                modifier *= modifier;

            
            if (!Phase2)
            {
                if (npc.GetLifePercent() < 0.66f || (retinazer != null && retinazer.GetLifePercent() < 0.66f))
                {
                    Phase2 = true;
                    npc.ai[0] = 1f;
                    npc.ai[1] = 0.0f;
                    npc.ai[2] = 0.0f;
                    npc.ai[3] = 0.0f;
                    npc.netUpdate = true;
                }
            }

            if (npc.life <= npc.lifeMax / 2 || npc.dontTakeDamage)
            {
                npc.dontTakeDamage = npc.life == 1 || !npc.HasValidTarget;
                if (npc.life != 1 && npc.HasValidTarget)
                    npc.dontTakeDamage = false;
                //become vulnerable again when both twins low
                if (npc.dontTakeDamage && npc.HasValidTarget && (retinazer == null || retinazer.life == 1))
                    npc.dontTakeDamage = false;
            }

            if (Main.dayTime && !Main.remixWorld)
            {
                if (npc.velocity.Y > 0)
                    npc.velocity.Y = 0;

                npc.velocity.Y -= 0.5f;
                npc.dontTakeDamage = true;

                if (retinazer != null)
                {
                    if (npc.timeLeft < 60)
                        npc.timeLeft = 60;

                    if (retinazer.timeLeft < 60)
                        retinazer.timeLeft = 60;

                    npc.TargetClosest(false);
                    retinazer.TargetClosest(false);
                    if (npc.Distance(Main.player[npc.target].Center) > 2000 && retinazer.Distance(Main.player[retinazer.target].Center) > 2000)
                    {
                        if (FargoSoulsUtil.HostCheck)
                        {
                            npc.active = false;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                            retinazer.active = false;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, EModeGlobalNPC.retiBoss);
                        }
                    }
                }

                return true;
            }

            const int P3DashDelayLength = 75;

            if (!Phase2) // phase 1
            {
                npc.damage = npc.defDamage;
                ref float ai_State = ref npc.ai[1];
                ref float ai_StateTimer = ref npc.ai[2];
                ref float ai_ShotTimer = ref npc.ai[3];

                if (!npc.HasPlayerTarget || Main.IsItDay())
                    return true;
                Player player = Main.player[npc.target];


                switch (ai_State)
                {
                    case 0: //normal fireball state
                        {
                            int num470 = 600;
                            if (ai_StateTimer >= (float)num470 - 1)
                            {
                                ai_State = 1f;
                                ai_StateTimer = 0f;
                                ai_ShotTimer = 0f;
                                npc.TargetClosest();
                                npc.netUpdate = true;
                                goto case 1;
                            }

                            // movement
                            float accel = 0.10f;
                            int side = 1;
                            if (npc.position.X + (float)(npc.width / 2) < player.position.X + (float)player.width)
                            {
                                side = -1;
                            }

                            // side position, default spazmatism
                            float desiredX = player.Center.X + side * 400 - npc.Center.X;
                            float desiredY = player.Center.Y - npc.Center.Y;

                            //Retinazer.TwinDefaultMovement(npc, desiredX, desiredY, accel, 2);

                            // reworked p1 fireballs
                            float delay = 58f;
                            if (WorldSavingSystem.MasochistModeReal)
                                ai_ShotTimer += 0.25f;
                            if (ai_ShotTimer < delay - 10)
                            {
                                float slowdownMod = 1f;
                                float distance = npc.Distance(player.Center);
                                if (distance > 600)
                                    slowdownMod *= MathHelper.Lerp(1, 0, (distance - 500) / 500);
                                slowdownMod = MathHelper.Clamp(slowdownMod, 0, 1);
                                npc.velocity *= 1f - 0.08f * slowdownMod;
                            }
                            if (ai_ShotTimer >= delay)
                            {
                                ai_ShotTimer = 0f;
                                Vector2 shootPos = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
                                float targetX = player.Center.X - shootPos.X;
                                float targetY = player.Center.Y - shootPos.Y;

                                if (targetX * targetX + targetY * targetY > 800 * 800) // don't fire if too far
                                    break;
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    float vel = 14f;
                                    int projDamage = npc.GetAttackDamage_ForProjectiles(25f, 22f);
                                    //projDamage = (int)(projDamage * 1.25f);
                                    float angle = (float)Math.Sqrt(targetX * targetX + targetY * targetY);
                                    angle = vel / angle;
                                    targetX *= angle;
                                    targetY *= angle;
                                    targetX += (float)Main.rand.Next(-40, 41) * 0.05f;
                                    targetY += (float)Main.rand.Next(-40, 41) * 0.05f;
                                    shootPos.X += targetX * 4f;
                                    shootPos.Y += targetY * 4f;
                                    int num473 = Projectile.NewProjectile(npc.GetSource_FromThis(), shootPos.X, shootPos.Y, targetX / 10f, targetY / 10f, ModContent.ProjectileType<MechElectricOrbSpaz>(),
                                        projDamage, 0f, Main.myPlayer, ai0: npc.target, ai2: MechElectricOrb.Green);

                                    Vector2 shotVel = new(targetX, targetY);
                                    npc.velocity -= shotVel / 2f;

                                    for (int i = -3; i <= 3; i++)
                                    {
                                        Vector2 particleVel = (shotVel * 1.2f).RotatedBy(MathHelper.PiOver2 * 0.1f * i).RotatedByRandom(MathHelper.PiOver2 * 0.04f) * Main.rand.NextFloat(0.8f, 1.2f);
                                        Particle p = new ElectricSpark(shootPos - shotVel, particleVel, Color.Green, Main.rand.NextFloat(0.7f, 1f), 20);
                                        p.Spawn();
                                    }

                                }
                            }
                        }
                        break;
                    case 1: // dash frame
                        {
                            float prepTime = WorldSavingSystem.MasochistModeReal ? 95 : 105;

                            if (ai_StateTimer == 0)
                            {
                                LockedRotation = player.DirectionTo(npc.Center).ToRotation();
                                int dir = Main.rand.NextBool() ? 1 : -1;
                                // rotate away from spazmatism
                                if (retinazer != null && retinazer.TypeAlive(NPCID.Spazmatism))
                                {
                                    float spazmatism_Angle = player.DirectionTo(retinazer.Center).ToRotation();
                                    dir = MathF.Sign(LockedRotation - spazmatism_Angle);
                                }
                                LockedRotation += dir * MathHelper.PiOver2 * (0.25f + Main.rand.NextFloat(0.2f));
                                npc.netUpdate = true;
                            }

                            // lock on to spot next to player
                            float distance = WorldSavingSystem.MasochistModeReal ? 380f : 450f;
                            Vector2 desiredPos = player.Center + LockedRotation.ToRotationVector2() * distance;
                            float desiredX = desiredPos.X - npc.Center.X;
                            float desiredY = desiredPos.Y - npc.Center.Y;

                            float accel = 0.6f;
                            Retinazer.TwinDefaultMovement(npc, desiredX, desiredY, accel, 4);
                            Retinazer.TwinManageRotation(npc);

                            bool waitForReti = false;
                            if (retinazer != null)
                            {
                                waitForReti = retinazer.ai[1] == npc.ai[1];

                                Vector2 spazDesiredPos = player.Center + retinazer.GetGlobalNPC<Retinazer>().LockedRotation.ToRotationVector2() * distance;
                                if (retinazer.Distance(spazDesiredPos) <= 300)
                                    waitForReti = false;
                            }

                            int flashDelay = WorldSavingSystem.MasochistModeReal ? 35 : 45;

                            if (ai_StateTimer < prepTime - flashDelay && (npc.Distance(desiredPos) > 300 || waitForReti))
                            {
                                ai_StateTimer--;
                                npc.netUpdate = true;
                            }
                                

                            if (ai_StateTimer < prepTime - flashDelay && retinazer != null && retinazer.ai[2] < npc.ai[2])
                            {
                                npc.ai[2] = retinazer.ai[2];
                                npc.netUpdate = true;
                            }
                                

                            if (ai_StateTimer == prepTime - 25)
                            {
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + TwinsEyeFlash.Offset(npc), Vector2.Zero, ModContent.ProjectileType<TwinsEyeFlash>(), 0, 0, Main.myPlayer, npc.whoAmI);
                                }
                            }
                            if (++ai_StateTimer >= prepTime)
                            {
                                npc.velocity = npc.DirectionTo(player.Center) * 28;
                                ai_StateTimer = 0;
                                LockedRotation = 0;
                                ai_State = 2;
                                npc.netUpdate = true;
                            }
                            return false;
                        }
                    case 2: // mid dash
                        {
                            npc.damage = (int)(npc.defDamage * 1.5f);

                            ai_StateTimer += 1f;
                            if (ai_StateTimer >= 25f)
                            {
                                ai_ShotTimer += 1f;
                                ai_StateTimer = 0f;
                                npc.TargetClosest();
                                if (ai_ShotTimer >= 7f)
                                {
                                    ai_State = 0f;
                                    ai_ShotTimer = 0f;
                                }
                                else
                                {
                                    ai_State = 1f;
                                }
                            }
                            else
                            {
                                npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) - 1.57f;
                                if (ai_StateTimer >= 20)
                                    npc.velocity *= 0.95f;
                            }
                            return false;
                        }
                }
            }
            else
            {
                npc.damage = (int)(npc.defDamage * 1.5f);
                if (npc.ai[1] == 0f) // not dashing
                {
                    if (retinazer != null && (retinazer.ai[0] < 4f || retinazer.GetGlobalNPC<Retinazer>().DeathrayState == 0
                        || retinazer.GetGlobalNPC<Retinazer>().DeathrayState == 3)) //reti is in normal AI
                    {
                        npc.ai[1] = 1; //switch to dashing
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;

                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                        NetSync(npc);
                        return false;
                    }
                }
                else if (npc.ai[0] > 2) //dashing
                {
                    if (FlameWheelSpreadTimer > 0) //cooldown before attacking again
                    {
                        P3DashPhaseDelay = Math.Min(FlameWheelSpreadTimer, 75);
                        FlameWheelSpreadTimer = 0;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                        NetSync(npc);
                    }

                    if (P3DashPhaseDelay > 0)
                    {
                        P3DashPhaseDelay--;
                        if (!TryWatchHarmlessly(npc))
                            return false;
                    }
                    if (npc.ai[2] == 0)
                    {
                        npc.velocity *= 1f;
                    }
                    if (npc.HasValidTarget)
                    {
                        Vector2 toTarget = npc.DirectionTo(Main.player[npc.target].Center);
                        npc.velocity += toTarget * 0.25f;
                        npc.velocity = npc.velocity.RotateTowards(toTarget.ToRotation(), 0.007f);
                    }
                    npc.ai[2] -= 0.5f;
                    if (npc.ai[2] > 50)
                    {
                        npc.ai[2] -= modifier;
                        npc.ai[2] -= 0.3f;
                    }
                    else
                    {
                        if (npc.HasValidTarget && ++ProjectileTimer > 3) //cursed flamethrower when dashing
                        {
                            float dashTime = 50f;
                            float extension = MathF.Sin(MathF.PI * npc.ai[2] / dashTime);
                            if (extension < 0)
                                extension = 0;
                            ProjectileTimer = 0;
                            if (FargoSoulsUtil.HostCheck)
                            {
                                float speed = extension * 0.55f;
                                float rotationVariance = 9f * extension * 0.75f;
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, speed * npc.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-rotationVariance, rotationVariance))), ProjectileID.EyeFire, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer);
                            }
                        }
                    }
                }

                if (npc.ai[0] < 4f)
                {
                    if (npc.life <= npc.lifeMax / 2 || (retinazer != null && retinazer.life <= retinazer.lifeMax / 2)) //going to phase 3
                    {
                        npc.ai[0] = 4f;
                        npc.netUpdate = true;
                        SoundEngine.PlaySound(SoundID.Roar, npc.Center);

                        if (!WorldSavingSystem.MasochistModeReal)
                            P3DashPhaseDelay = P3DashDelayLength;

                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, npc.whoAmI, NPCID.MoonLordCore);

                        int index = npc.FindBuffIndex(BuffID.CursedInferno);
                        if (index != -1)
                            npc.DelBuff(index); //remove cursed inferno debuff if i have it

                        npc.buffImmune[BuffID.CursedInferno] = true;
                        npc.buffImmune[BuffID.OnFire] = true;
                        npc.buffImmune[BuffID.OnFire3] = true;
                        npc.buffImmune[BuffID.ShadowFlame] = true;
                        npc.buffImmune[BuffID.Frostburn] = true;
                        npc.buffImmune[BuffID.Frostburn2] = true;
                    }

                    //reti is doing the spin
                    if (retinazer != null && retinazer.ai[0] >= 4f && retinazer.GetGlobalNPC<Retinazer>().DeathrayState != 0 && retinazer.GetGlobalNPC<Retinazer>().DeathrayState != 3)
                    {
                        if (!WorldSavingSystem.MasochistModeReal)
                        {
                            npc.velocity *= 0.98f;
                            if (!TryWatchHarmlessly(npc))
                                return false;
                        }
                    }
                }
                else //in phase 3
                {
                    //npc.position += npc.velocity / 10f;

                    //dust code
                    /*
                    if (npc.ai[1] == 0f && Main.rand.Next(4) < 3)
                    {
                        int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, DustID.GemEmerald, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 3.5f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= 1.8f;
                        Main.dust[dust].velocity.Y -= 0.5f;
                        if (Main.rand.NextBool(4))
                        {
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].scale *= 0.5f;
                        }
                    }
                    */

                    if (npc.ai[1] == 0f) //not dashing
                    {
                        Resist = true;
                        if (npc.HasValidTarget && retinazer != null)
                        {
                            Vector2 target = retinazer.Center + retinazer.SafeDirectionTo(npc.Center) * 100;
                            npc.velocity = (target - npc.Center) / 60;

                            float rotationInterval = 2f * (float)Math.PI * 1.2f / 4f / 60f * 0.65f;
                            rotationInterval *= retinazer.GetGlobalNPC<Retinazer>().StoredDirectionToPlayer ? 1f : -1f;
                            //if (WorldSavingSystem.MasochistModeReal)
                            rotationInterval *= -1f;

                            npc.rotation += rotationInterval * ProjectileTimer / 20f;
                            RealRotation += rotationInterval;

                            if (FlameWheelSpreadTimer < 0)
                                FlameWheelSpreadTimer = 0;

                            if (FlameWheelCount == 0) //i can't be bothered to figure out the formula for this rn
                            {
                                FlameWheelCount = 2;
                                if (modifier < 0.4f)
                                    FlameWheelCount = 3;
                                if (modifier < 0.2f)
                                    FlameWheelCount = 4;
                                if (WorldSavingSystem.MasochistModeReal && FlameWheelCount < 5)
                                    FlameWheelCount++;

                                ProjectileTimer = 0;
                            }

                            if (++FlameWheelSpreadTimer < 30) //snap to reti, don't do contact damage
                            {
                                npc.rotation = npc.SafeDirectionTo(retinazer.Center).ToRotation() - (float)Math.PI / 2;
                                RealRotation = npc.rotation;
                            }
                            else if (++ProjectileTimer % 35 == 0) //rings of stars
                            {
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    float speed = 12f * Math.Min((FlameWheelSpreadTimer - 30) / 120f, 1f); //fan out gradually
                                    int timeLeft = (int)(speed / 12f * 90f);
                                    float baseRotation = RealRotation + (float)Math.PI / 2;
                                    if (timeLeft > 5)
                                    {
                                        for (int i = 0; i < FlameWheelCount; i++)
                                        {
                                            int p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, speed * (baseRotation + MathHelper.TwoPi / FlameWheelCount * i).ToRotationVector2(),
                                                ModContent.ProjectileType<MechElectricOrbTelegraphed>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, ai2: MechElectricOrb.Green);
                                            if (p != Main.maxProjectiles)
                                                Main.projectile[p].timeLeft = timeLeft;
                                        }
                                    }
                                }
                            }

                            /*if (++Counter0 > 40)
                            {
                                Counter0 = 0;
                                if (FargoSoulsUtil.HostCheck && npc.HasPlayerTarget) //vanilla spaz p1 shoot fireball code
                                {
                                    Vector2 Speed = Main.player[npc.target].Center - npc.Center;
                                    Speed.Normalize();
                                    int Damage;
                                    if (Main.expertMode)
                                    {
                                        Speed *= 14f;
                                        Damage = 22;
                                    }
                                    else
                                    {
                                        Speed *= 12f;
                                        Damage = 25;
                                    }
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + Speed * 4f, Speed, ProjectileID.CursedFlameHostile, Damage, 0f, Main.myPlayer);
                                }
                            }*/

                            return false;
                        }
                        else // if reti dead, switch to dashing
                        {
                            npc.ai[1] = 1; 
                            npc.ai[2] = 0;
                            npc.ai[3] = 0;
                        }
                    }
                    else //dashing
                    {
                        if (retinazer != null && retinazer.ai[0] >= 4f && retinazer.GetGlobalNPC<Retinazer>().DeathrayState != 0 && retinazer.GetGlobalNPC<Retinazer>().DeathrayState != 3) //reti is doing the spin
                        {
                            npc.ai[1] = 0; //switch to not dashing

                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                            NetSync(npc);
                            return false;
                        }

                        FlameWheelCount = 0;
                    }

                    if (WorldSavingSystem.MasochistModeReal && retinazer == null && --MechElectricOrbTimer < 0) //when twin dead, begin shooting Electric Orbs
                    {
                        MechElectricOrbTimer = 150;
                        if (FargoSoulsUtil.HostCheck && npc.HasPlayerTarget)
                        {
                            Vector2 distance = Main.player[npc.target].Center - npc.Center;
                            distance.Normalize();
                            distance *= 14f;
                            for (int i = 0; i < 8; i++)
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, distance.RotatedBy(2 * Math.PI / 8 * i),
                                    ModContent.ProjectileType<MechElectricOrb>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 0f, Main.myPlayer, ai2: MechElectricOrb.Green);
                        }
                    }
                }
            }
            return true;
        }
        private bool TryWatchHarmlessly(NPC npc)
        {
            Resist = true;

            if (npc.HasValidTarget)
            {
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

                return false;
            }
            return true;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (Resist)
                modifiers.FinalDamage /= 2;

            base.ModifyIncomingHit(npc, ref modifiers);
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int CooldownSlot)
        {
            return !(npc.ai[1] == 0f && FlameWheelSpreadTimer < 30) && CircularHitbox(npc, target.position, target.Size);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            if (npc.ai[0] >= 4f)
                target.AddBuff(BuffID.CursedInferno, 240);
        }

        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            //if (npc.ai[0] < 4 || npc.ai[1] != 0f)
                return base.GetAlpha(npc, drawColor);
            //return new Color(drawColor.R / 2, 255, drawColor.B / 2);
        }

        public override bool CheckDead(NPC npc)
        {
            if ( WorldSavingSystem.MasochistModeReal)
                return base.CheckDead(npc);

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.retiBoss, NPCID.Retinazer) && Main.npc[EModeGlobalNPC.retiBoss].life > 1) //reti still active
            {
                npc.life = 1;
                npc.active = true;
                if (FargoSoulsUtil.HostCheck)
                    npc.netUpdate = true;

                if (!HasSaidEndure)
                {
                    HasSaidEndure = true;
                    FargoSoulsUtil.PrintLocalization($"Mods.{Mod.Name}.NPCs.EMode.TwinsEndure", new Color(175, 75, 255), npc.FullName);
                }
                return false;
            }

            return base.CheckDead(npc);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadBossHeadSprite(recolor, 16);
            LoadBossHeadSprite(recolor, 21);
            LoadGoreRange(recolor, 143, 146);

            LoadSpecial(recolor, ref TextureAssets.Chain12, ref FargowiltasSouls.TextureBuffer.Chain12, "Chain12");
        }

        public static bool CircularHitbox(NPC npc, Vector2 boxPos, Vector2 boxDim)
        {
            //circular hitbox-inator
            Vector2 ellipseDim = npc.Size;
            Vector2 ellipseCenter = npc.position + 0.5f * new Vector2(npc.width, npc.height);

            float x = 0f; //ellipse center
            float y = 0f; //ellipse center
            if (boxPos.X > ellipseCenter.X)
            {
                x = boxPos.X - ellipseCenter.X; //left corner
            }
            else if (boxPos.X + boxDim.X < ellipseCenter.X)
            {
                x = boxPos.X + boxDim.X - ellipseCenter.X; //right corner
            }
            if (boxPos.Y > ellipseCenter.Y)
            {
                y = boxPos.Y - ellipseCenter.Y; //top corner
            }
            else if (boxPos.Y + boxDim.Y < ellipseCenter.Y)
            {
                y = boxPos.Y + boxDim.Y - ellipseCenter.Y; //bottom corner
            }
            float a = ellipseDim.X / 2f;
            float b = ellipseDim.Y / 2f;

            return x * x / (a * a) + y * y / (b * b) < 1; //point collision detection
        }
    }
}
