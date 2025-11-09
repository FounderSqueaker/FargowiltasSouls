using System.IO;
using Terraria.ModLoader.IO;
using FargowiltasSouls.Content.Projectiles.Deathrays;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core;
using Terraria.Graphics.CameraModifiers;
using FargowiltasSouls.Content.Projectiles.Eternity.Environment;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Deerclops;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class Deerclops : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Deerclops);

        public int BerserkSpeedupTimer;
        public int TeleportTimer;
        public int WalkingSpeedUpTimer;

        public bool EnteredPhase2;
        public bool EnteredPhase3;
        public bool DoLaserAttack;

        public bool DroppedSummon;

        public int ForceDespawnTimer;
        public int LockDirection;
        public bool FirstFrameOfRubble;

        public int HandsCooldown = 2;
        public bool HandsCheck;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(BerserkSpeedupTimer);
            binaryWriter.Write(TeleportTimer);
            binaryWriter.Write(WalkingSpeedUpTimer);
            binaryWriter.Write(HandsCooldown);
            bitWriter.WriteBit(EnteredPhase2);
            bitWriter.WriteBit(EnteredPhase3);
            bitWriter.WriteBit(DoLaserAttack);
            bitWriter.WriteBit(FirstFrameOfRubble);
            bitWriter.WriteBit(HandsCheck);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            BerserkSpeedupTimer = binaryReader.ReadInt32();
            TeleportTimer = binaryReader.ReadInt32();
            WalkingSpeedUpTimer = binaryReader.ReadInt32();
            HandsCooldown = binaryReader.ReadInt32();
            EnteredPhase2 = bitReader.ReadBit();
            EnteredPhase3 = bitReader.ReadBit();
            DoLaserAttack = bitReader.ReadBit();
            FirstFrameOfRubble = bitReader.ReadBit();
            HandsCheck = bitReader.ReadBit();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.1, MidpointRounding.ToEven);
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Frostburn] = true;
            npc.buffImmune[BuffID.Frostburn2] = true;
            npc.buffImmune[BuffID.Chilled] = true;
            npc.buffImmune[BuffID.Frozen] = true;

            if (npc.HasPlayerTarget && npc.Bottom.Y > Main.player[npc.target].Bottom.Y)
                npc.position.Y = Main.player[npc.target].Bottom.Y - 100 - npc.height;
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int CooldownSlot)
        {
            if (npc.alpha > 0)
                return false;
            if (npc.frame.Y == 17)
                return true;
            return false;
            /*
            if (npc.alpha > 0)
                return false;

            return base.CanHitPlayer(npc, target, ref CooldownSlot);
            */
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            EModeGlobalNPC.deerBoss = npc.whoAmI;

            const int MaxBerserkTime = 600;

            BerserkSpeedupTimer -= 1;

            if (npc.localAI[3] > 0 || EnteredPhase3)
                npc.localAI[2]++; //cry about it

            const int TeleportThreshold = 800;
            
            if (npc.ai[0] != 0)
            {
                npc.alpha -= 10;
                if (npc.alpha < 0)
                    npc.alpha = 0;

                if (EnteredPhase3)
                    npc.localAI[2]++;
            }

            TeleportTimer++;
            if (EnteredPhase3)
                TeleportTimer++;

            if (!Main.IsItDay())
                Lighting.AddLight(npc.Center, SoulConfig.Instance.BossRecolors ? TorchID.Red : TorchID.Blue);

            if (Main.LocalPlayer.active && !Main.LocalPlayer.ghost && !Main.LocalPlayer.dead && npc.Distance(Main.LocalPlayer.Center) < 1000)
            {
                Main.LocalPlayer.AddBuff(ModContent.BuffType<LowGroundBuff>(), 2);
            }
            if (npc.ai[0] != 1)
                HandsCheck = true;
            if (npc.ai[0] != 2)
                FirstFrameOfRubble = true;

            switch ((int)npc.ai[0])
            {
                case 0: //walking at player
                    if (++WalkingSpeedUpTimer > 900) //scaling capped for edge case sanity
                        WalkingSpeedUpTimer = 900;
                    //after walking for a bit, begin walking faster to catch up to outrunning player
                    npc.position.X += npc.velocity.X * Math.Max(0, WalkingSpeedUpTimer - 90) / 90f;

                    if (TeleportTimer < TeleportThreshold)
                    {
                        if (EnteredPhase3)
                            npc.position.X += npc.velocity.X;

                        if (npc.velocity.Y == 0)
                        {
                            if (EnteredPhase2)
                                npc.position.X += npc.velocity.X;
                            if (BerserkSpeedupTimer > 0)
                                npc.position.X += npc.velocity.X * 4f * BerserkSpeedupTimer / MaxBerserkTime;
                        }
                    }

                    if (EnteredPhase2)
                    {
                        if (!EnteredPhase3 && npc.life < npc.lifeMax * .33)
                        {
                            npc.ai[0] = 3;
                            npc.ai[1] = 0;
                            npc.netUpdate = true;
                            break;
                        }

                        if (TeleportTimer > TeleportThreshold)
                        {
                            WalkingSpeedUpTimer = 0;

                            npc.velocity.X *= 0.9f;
                            npc.dontTakeDamage = true;
                            npc.localAI[1] = 0; //reset walls attack counter

                            if (EnteredPhase2 && Main.LocalPlayer.active && !Main.LocalPlayer.ghost && !Main.LocalPlayer.dead && npc.Distance(Main.LocalPlayer.Center) < 1600)
                            {
                                FargoSoulsUtil.AddDebuffFixedDuration(Main.LocalPlayer, BuffID.Darkness, 2);
                                FargoSoulsUtil.AddDebuffFixedDuration(Main.LocalPlayer, BuffID.Blackout, 2);
                            }

                            if (npc.alpha == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Roar, npc.Center);
                                foreach (Projectile p in Main.ActiveProjectiles)
                                {
                                    if (p.TypeAlive(ProjectileID.DeerclopsRangedProjectile))
                                        p.Kill();
                                }
                                SpawnFreezeHands(npc, Main.player[npc.target]);
                            }

                            npc.alpha += 5;
                            if (WorldSavingSystem.SwarmActive)
                                npc.alpha -= 2;
                            if (npc.alpha > 255)
                            {
                                npc.alpha = 255;

                                npc.localAI[3] = 30;

                                if (npc.HasPlayerTarget) //teleport
                                {
                                    float distance = 16 * 14 * Math.Sign(npc.Center.X - Main.player[npc.target].Center.X);
                                    distance *= -1f; //alternate back and forth

                                    if (TeleportTimer == TeleportThreshold + 10) //introduce randomness
                                    {
                                        if (Main.rand.NextBool())
                                            distance *= -1f;

                                        if (Main.netMode == NetmodeID.Server)
                                            NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);

                                        DoLaserAttack = !DoLaserAttack; //guarantee he alternates wall attacks at some point in the fight
                                        NetSync(npc);
                                    }

                                    npc.Bottom = Main.player[npc.target].Bottom + distance * Vector2.UnitX;

                                    npc.direction = Math.Sign(Main.player[npc.target].Center.X - npc.Center.X);
                                    npc.velocity.X = 3.4f * npc.direction;
                                    npc.velocity.Y = 0;

                                    int addedThreshold = 180;
                                    if (EnteredPhase3)
                                        addedThreshold -= 30;
                                    if (WorldSavingSystem.MasochistModeReal)
                                        addedThreshold -= 30;

                                    if (TeleportTimer > TeleportThreshold + addedThreshold)
                                    {
                                        TeleportTimer = 0;
                                        npc.velocity.X = 0;
                                        npc.ai[0] = 4;
                                        npc.ai[1] = 0;
                                        NetSync(npc);
                                        if (Main.netMode == NetmodeID.Server)
                                            NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
                                    }
                                }
                            }
                            else
                            {
                                TeleportTimer = TeleportThreshold;

                                if (npc.localAI[3] > 0)
                                    npc.localAI[3] -= 3; //remove visual effect
                            }

                            return false;
                        }
                    }
                    else if (npc.life < npc.lifeMax * .66)
                    {
                        npc.ai[0] = 3;
                        npc.ai[1] = 0;
                        npc.netUpdate = true;
                    }

                    break;

                case 1: //ice wave, npc.localai[1] counts them, attacks at ai1=30, last spike 52, ends at ai1=80

                    if (HandsCheck)
                    {
                        if (!npc.HasPlayerTarget)
                        {
                            HandsCooldown = 3;
                            HandsCheck = false;
                            npc.netUpdate = true;
                        }
                        else
                        {
                            if (HandsCooldown <= 0 && npc.ai[1] == 0 && Main.rand.NextBool(2))
                            {
                                HandsCooldown = 1;
                            }
                            if (HandsCooldown <= 0)
                            {
                                npc.velocity.X = 0;
                                int attackDuration = WorldSavingSystem.MasochistModeReal ? 125 : 140;
                                int firstVolleyTime = 10;
                                int secondVolleyTime = WorldSavingSystem.MasochistModeReal ? 60 : 85;
                                float distance = 300;
                                void SpawnHand(Vector2 dir)
                                {
                                    if (!FargoSoulsUtil.HostCheck)
                                        return;
                                    Vector2 spawnposition = Main.player[npc.target].Center + dir.SafeNormalize(-Vector2.UnitY) * distance;
                                    float angle = -dir.ToRotation();
                                    Vector2 spawnvelocity = Vector2.Zero; // angle.ToRotationVector2() * velocity;
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), spawnposition, spawnvelocity, ModContent.ProjectileType<DeerclopsDarknessHand>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 1f), 0f, Main.myPlayer, ai2: 1);
                                }
                                if (npc.ai[1] == firstVolleyTime) // first volley
                                {
                                    npc.TargetClosest();
                                    for (int i = -1; i <= 1; i += 2)
                                    {
                                        SpawnHand(Vector2.UnitX * i);
                                    }
                                    if (EnteredPhase2)
                                        SpawnHand(-Vector2.UnitY);

                                }
                                if (npc.ai[1] == secondVolleyTime) // second volley
                                {
                                    npc.TargetClosest();
                                    if (EnteredPhase2)
                                    {
                                        for (int i = -1; i <= 1; i += 2)
                                        {
                                            SpawnHand(Vector2.UnitX * i - Vector2.UnitY);
                                        }
                                    }
                                    else
                                    {
                                        SpawnHand(-Vector2.UnitY);
                                    }
                                }
                                npc.ai[1]++;
                                if (npc.ai[1] > attackDuration)
                                {
                                    HandsCheck = false;
                                    HandsCooldown = 3;
                                    npc.ai[1] = 0;
                                    npc.netUpdate = true;
                                }
                                result = false;
                                break;
                            }
                            else
                            {
                                HandsCheck = false;
                                HandsCooldown--;
                                npc.netUpdate = true;
                            }
                        }
                    }
                    WalkingSpeedUpTimer = 0;

                    if (npc.ai[1] < 30)
                    {
                        if (WorldSavingSystem.MasochistModeReal)
                        {
                            npc.ai[1] += 0.5f;
                            npc.frameCounter += 0.5;
                            
                            
                        }
                    }
                    break;

                case 2: //debris attack
                    int num8 = 4;
                    int telegraphTime = 8 * num8;
                    ref float timer = ref npc.ai[1];
                    if (FirstFrameOfRubble)
                    {
                        FirstFrameOfRubble = false;
                        float mult = Main.getGoodWorld ? WorldSavingSystem.MasochistModeReal ? 0f : 0f : 0.3f;
                        npc.ai[1] = (int)(-telegraphTime * mult);
                    }
                    if (timer < telegraphTime)
                    {
                        // vanilla code for rubble positions
                        Point sourceTileCoords = npc.Top.ToTileCoordinates();
                        int distancedByThisManyTiles = 1;
                        sourceTileCoords.X += npc.direction * 3;
                        sourceTileCoords.Y -= 10;

                        for (int count = 0; count < 2; count++)
                        {
                            int num10 = 20;
                            int num11 = Main.rand.Next(1, 60 - telegraphTime);
                            int num12 = 1;
                            int num13 = num11 / num12 * num12;
                            int num14 = num13 + num12;
                            if (num11 % num12 != 0)
                            {
                                num14 = num13;
                            }
                            int max = Math.Min(num14, num10);
                            int whichOne = Main.rand.Next(Math.Min(num13, max - 1), max);
                            int num2 = whichOne * distancedByThisManyTiles;
                            int i = Main.rand.Next(0, 35);
                            int num3 = sourceTileCoords.X + num2 * npc.direction;
                            int num4 = sourceTileCoords.Y + i;
                            Vector2 pos = new(num3 * 16 + 8, num4 * 16 - 8);
                            pos.Y -= 200;
                            pos.Y = LumUtils.FindGroundVertical(pos.ToTileCoordinates()).ToWorldCoordinates().Y;
                            pos.X += Main.rand.NextFloat(-30, 30);
                            int d = Dust.NewDust(pos, 0, 0, DustID.SnowSpray);
                            Main.dust[d].velocity = new(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-2, -1));
                            Main.dust[d].noGravity = false;
                        }
                    }
                        

                    break;

                case 3: //roar at 30, ends at ai1=60
                    WalkingSpeedUpTimer = 0;

                    if (!WorldSavingSystem.MasochistModeReal && npc.ai[1] < 30)
                    {
                        npc.ai[1] -= 0.5f;
                        npc.frameCounter -= 0.5;
                    }

                    if (EnteredPhase2)
                    {
                        npc.localAI[1] = 0; //ensure this is always the same
                        npc.localAI[3] = 30; //go invul

                        if (npc.ai[1] > 30)
                        {
                            Main.dayTime = false;
                            Main.time = 16200; //midnight, to help teleport visual
                        }
                    }
                    else if (npc.life < npc.lifeMax * .66)
                    {
                        EnteredPhase2 = true;
                        NetSync(npc);
                    }

                    if (EnteredPhase3)
                    {
                        if (!Main.dedServ)
                            FargoSoulsUtil.ScreenshakeRumble(6);

                        if (npc.ai[1] > 30) //roaring
                        {
                            if (npc.HasValidTarget) //fly over player
                                npc.position = Vector2.Lerp(npc.position, Main.player[npc.target].Center - 450 * Vector2.UnitY, 0.2f);
                        }
                    }
                    else if (npc.life < npc.lifeMax * .33)
                    {
                        EnteredPhase3 = true;
                        NetSync(npc);
                    }

                    if (EnteredPhase3 || WorldSavingSystem.MasochistModeReal)
                        BerserkSpeedupTimer = MaxBerserkTime;
                    break;

                case 4: //both sides ice wave, attacks at ai1=50, last spike 70, ends at ai1=90
                    {
                        WalkingSpeedUpTimer = 0;

                        int cooldown = 100; //stops deerclops from teleporting while old ice walls are still there
                        if (EnteredPhase3)
                            cooldown *= 2;
                        if (TeleportTimer > TeleportThreshold - cooldown)
                            TeleportTimer = TeleportThreshold - cooldown;

                        if (npc.ai[1] == 0)
                        {
                            LockDirection = npc.direction;
                            if (EnteredPhase2)
                            {
                                if (npc.alpha == 0) //i.e. dont randomize when coming out of tp
                                    DoLaserAttack = Main.rand.NextBool();
                                NetSync(npc);

                                if (FargoSoulsUtil.HostCheck)
                                {
                                    if (DoLaserAttack)
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, npc.whoAmI, npc.type);
                                }
                            }

                            if (FargoSoulsUtil.HostCheck && !DoLaserAttack)
                            {
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ProjectileID.DD2OgreSmash, 0, 0f, Main.myPlayer);
                            }
                        }
                        npc.direction = LockDirection;
                        Vector2 eye = npc.Center + new Vector2(64 * npc.direction, -24f) * npc.scale;

                        if (WorldSavingSystem.MasochistModeReal)
                        {
                            const int desiredStartup = 30; //effectively changes startup from 50 to this value
                            const int threshold = 50 - desiredStartup / 2;
                            if (npc.ai[1] < threshold)
                                npc.ai[1]++;
                        }

                        if (DoLaserAttack && npc.ai[1] >= 70)
                        {
                            if (EnteredPhase3)
                            {
                                const float baseIncrement = 0.33f;
                                float increment = baseIncrement;
                                //if (WorldSavingSystem.MasochistModeReal) increment *= 2;

                                if (npc.ai[1] == 70) //shoot laser
                                {
                                    float time = (90 - 70) / baseIncrement - 5;
                                    time *= 5; //account for the ray having extra updates
                                    float rotation = MathHelper.Pi * (WorldSavingSystem.MasochistModeReal ? 1f : 0.8f) / time * -npc.direction;

                                    if (FargoSoulsUtil.HostCheck)
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), eye, Vector2.UnitY, ModContent.ProjectileType<DeerclopsDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 2f), 0f, Main.myPlayer, rotation, time);
                                }

                                npc.ai[1] += increment; //more endlag than normal

                                if (npc.ai[1] < 90)
                                    return false; //stop deerclops from turning around
                            }
                            else
                            {
                                npc.ai[1] += 0.33f; //more endlag than normal

                                if (npc.ai[1] >= 89)
                                {
                                    npc.ai[0] = 2; //force debris attack instead
                                    npc.ai[1] = 0;
                                    npc.frameCounter = 0;
                                    npc.netUpdate = true;
                                    break;
                                }
                            }

                            if (npc.ai[1] < 90)
                                return false; //stop deerclops from turning around
                        }
                    }
                    break;

                case 5: //another roar?
                    if (npc.ai[1] == 30)
                    {
                        //if player is somehow directly above deerclops at moment of roar
                        if (npc.HasValidTarget && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) < 16 * 3
                            && Main.player[npc.target].Bottom.Y < npc.Top.Y - 16 * 5)
                        {
                            //freeze them and drag them down
                            SpawnFreezeHands(npc, Main.player[npc.target]);
                        }
                    }
                    break;

                case 6: //trying to return home
                    if (++ForceDespawnTimer > 180)
                    {
                        if (FargoSoulsUtil.HostCheck) //force despawn
                        {
                            npc.ai[0] = 8f;
                            npc.ai[1] = 0.0f;
                            npc.localAI[1] = 0.0f;
                            npc.netUpdate = true;
                        }
                    }
                    break;

                default:
                    break;
            }

            //FargoSoulsUtil.PrintAI(npc);

            if (EnteredPhase3 && !(npc.ai[0] == 0 && npc.alpha > 0))
            {
                npc.localAI[3] += 3;
                if (npc.localAI[3] > 30)
                    npc.localAI[3] = 30;
            }

            //FargoSoulsUtil.PrintAI(npc);

            EModeUtils.DropSummon(npc, ItemID.DeerThing, NPC.downedDeerclops, ref DroppedSummon);

            return result;
        }
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (npc.ai[0] == 1)
            {
                if (HandsCheck && npc.HasPlayerTarget && HandsCooldown <= 0)
                {
                    if (npc.frameCounter > 4)
                        npc.frameCounter = 4;
                }
            }
            if (npc.ai[0] == 2)
            {
                if (npc.frameCounter < 8)
                    npc.frameCounter = 8;
                if (npc.ai[1] < 0 && npc.frameCounter > 8)
                {
                    npc.frameCounter = 8;
                }
                    
            }
        }
        public static void SpawnFreezeHands(Entity source, Player targetPlayer)
        {
            if (FargoSoulsUtil.HostCheck)
            {
                const int max = 12;
                for (int i = 0; i < 12; i++)
                {
                    Vector2 spawnPos = targetPlayer.Center + 16 * Main.rand.NextFloat(6, 36) * Vector2.UnitX.RotatedBy(MathHelper.TwoPi / max * (i + Main.rand.NextFloat()));
                    Projectile.NewProjectile(source.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<DeerclopsHand>(), 0, 0f, Main.myPlayer, targetPlayer.whoAmI);
                }
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.Frostburn, 90);
            target.AddBuff(BuffID.BrokenArmor, 90);
            if (WorldSavingSystem.MasochistModeReal)
                target.AddBuff(ModContent.BuffType<MarkedforDeathBuff>(), 600);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadBossHeadSprite(recolor, 39);
            LoadGore(recolor, 1270);
            LoadGore(recolor, 1271);
            LoadGore(recolor, 1272);
            LoadGore(recolor, 1273);
            LoadGore(recolor, 1274);
        }
    }
}
