using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Armor;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.BossMinions;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.FrostMoon;
using FargowiltasSouls.Content.Patreon.DanielTheRobot;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Core;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static tModPorter.ProgressUpdate;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class QueenSlime : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.QueenSlimeBoss);

        public int StompTimer;
        public int StompCounter;

        public float StompVelocityX;
        public float StompVelocityY;

        public int RainDirection;

        public bool DroppedSummon;

        public int WingFrameTimer;
        public int WingFrame;

        private const float StompTravelTime = 60;
        private const float StompGravity = 1.6f;

        public List<States> AvailableStates = [];

        // phase 1 cycles between attacks randomly, without direct repetition
        // mixes in superslam on consistent 10 second cooldown
        public enum States
        {
            // phase 1
            Hops = 0, // default state
            NormalSlam,
            MinionSlam,
            QuickHops,

            // both phases
            TripleSuperslam,

            // phase 2
            MinionChargeDirect,
            MinionChargeSide,
            Artillery,
            FlightExplosions,
            SpikeRain
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(StompTimer);
            binaryWriter.Write(StompCounter);

            binaryWriter.Write(StompVelocityX);
            binaryWriter.Write(StompVelocityY);

            binaryWriter.Write(WingFrameTimer);
            binaryWriter.Write(WingFrame);
            binaryWriter.Write(HopGravityTimer);

            binaryWriter.Write(State);
            binaryWriter.Write(Timer);
            binaryWriter.Write(NoContactDamage);
            binaryWriter.Write(Split);
            binaryWriter.Write(AI0);
            binaryWriter.Write(AI1);
            binaryWriter.Write(PreviousState);
    }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            StompTimer = binaryReader.ReadInt32();
            StompCounter = binaryReader.ReadInt32();

            StompVelocityX = binaryReader.ReadSingle();
            StompVelocityY = binaryReader.ReadSingle();

            WingFrameTimer = binaryReader.ReadInt32();
            WingFrame = binaryReader.ReadInt32();
            HopGravityTimer = binaryReader.ReadInt32();

            State = binaryReader.ReadSingle();
            Timer = binaryReader.ReadSingle();
            NoContactDamage = binaryReader.ReadSingle();
            Split = binaryReader.ReadSingle();
            AI0 = binaryReader.ReadSingle();
            AI1 = binaryReader.ReadSingle();
            PreviousState = binaryReader.ReadSingle();
    }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.25, MidpointRounding.ToEven);

            StompTimer = -360;
            NPC = npc;
        }
        public bool PhaseTwoHP => NPC.GetLifePercent() < 0.5f;
        public bool PhaseTwo => NPC.GetLifePercent() <= 0.5f && State >= (int)States.TripleSuperslam;
        public NPC NPC = null;
        public Player Target => Main.player[NPC.target];

        public float State;
        public float Timer;
        public float NoContactDamage;
        public float Split;
        public float AI0;
        public float AI1;
        public float PreviousState;

        public static int MaxSubjects => 9;

        public int HopGravityTimer = 0;

        public override bool SafePreAI(NPC npc)
        {
            NPC = npc;
            EModeGlobalNPC.queenSlimeBoss = npc.whoAmI;

            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(State.ToString()), Color.White);
            else
                Main.NewText("client: " + State.ToString());

            // despawn and targetting
            int num4 = 3000;
            if (Main.player[npc.target].dead || Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) / 16f > num4)
            {
                npc.TargetClosest();
                if (Main.player[npc.target].dead || Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) / 16f > num4)
                {
                    npc.EncourageDespawn(10);
                    return true;
                }
            }
            if (!NPC.HasValidTarget)
                return true;
            if (!PhaseTwo)
                NPC.noGravity = false;
            else
                NPC.noGravity = true;
            NPC.noTileCollide = false;

            NoContactDamage = 0;
            npc.ai[0] = 0; // set to 4 for afterimages

            float desiredScale = 1f;
            int subjects = NPC.CountNPCS(ModContent.NPCType<GelatinSubject>());
            float subjectScale = LumUtils.Saturate(subjects / MaxSubjects);
            if (subjects > 0)
            {
                desiredScale *= 1 - subjectScale;
            }
                
            NPC.dontTakeDamage = subjects > 0;

            float oldScale = NPC.scale;
            NPC.scale = MathHelper.Lerp(NPC.scale, desiredScale, 0.1f);

            if (NPC.scale != oldScale)
            {
                int baseWidth = 114;
                int baseHeight = 100;
                NPC.position = NPC.Center;
                float minScale = 0.2f;
                float scale = Math.Max(minScale, NPC.scale);
                NPC.width = (int)(baseWidth * scale);
                NPC.height = (int)(baseHeight * scale);
                NPC.Center = NPC.position;
            }
            // ai here
            switch ((States)State)
            {
                // p1
                case States.Hops:
                    Hops();
                    break;
                case States.NormalSlam:
                    NormalSlam();
                    break;
                case States.MinionSlam:
                    MinionSlam();
                    break;
                case States.QuickHops:
                    QuickHops();
                    break;
                // both
                case States.TripleSuperslam:
                    Stompy();
                    break;
                // p2
                case States.MinionChargeDirect:
                    MinionChargeDirect();
                    break;
                case States.MinionChargeSide:
                    MinionChargeSide();
                    break;
                case States.Artillery:
                    Artillery();
                    break;
                case States.FlightExplosions:
                    FlightExplosions();
                    break;
                case States.SpikeRain:
                    SpikeRain();
                    break;
            }
            if (StompTimer < 0)
                StompTimer++;

            EModeUtils.DropSummon(npc, ItemID.QueenSlimeCrystal, NPC.downedQueenSlime, ref DroppedSummon, Main.hardMode);
            return false;
        }
        #region States
        private void Hops()
        {
            int startup = 40;
            int hops = 1;
            if (PreviousState == 0) // start of fight
            {
                hops = 3;
                if (!Collision.CanHitLine(NPC.Center, 1, 1, Target.Center, 1, 1))
                    NPC.position += NPC.DirectionTo(Target.Center) * 20f;
            }
                
            if (Timer < startup)
            {
                Timer++;
                if (NPC.velocity.Y == 0)
                    NPC.velocity.X = 0;
                return;
            }
            int hopTimer = (int)Timer - startup;
            HopMovement(hopTimer, hops, ChooseAttackP1);
                

        }
        private void HopMovement(int hopTimer, int hops, Action endAction)
        {
            int endlag = 6;
            if (NPC.velocity.Y == 0)
            {
                HopGravityTimer = 0;
                if (hopTimer < hops) // start hop
                {
                    NPC.velocity.X = NPC.HorizontalDirectionTo(Target.Center) * 14;
                    float y = -12;
                    if (Target.Bottom.Y < NPC.Bottom.Y)
                    {
                        float bonus = MathF.Pow((Target.Bottom.Y - NPC.Bottom.Y) / 600, 2) * 11;
                        bonus = Math.Min(bonus, 22);
                        y -= bonus;
                    }
                        
                    NPC.velocity.Y = y;
                    Timer++;
                }
                else
                {
                    Timer++;
                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X = 0;
                    if (hopTimer > hops + endlag)
                    {
                        endAction.Invoke();
                        return;
                    }
                }
            }
            else
            {
                HopGravityTimer++;
                if (Target.Bottom.Y > NPC.Bottom.Y)
                {
                    NPC.velocity.Y += 0.25f;
                }
                float gravityMod = 1.75f * LumUtils.Saturate(HopGravityTimer / 220f);
                NPC.velocity.Y += gravityMod;

                float horDir = NPC.HorizontalDirectionTo(Target.Center);
                if (NPC.velocity.X.NonZeroSign() != horDir)
                    NPC.velocity.X += horDir * 0.5f;
            }
        }
        private void NormalSlam()
        {
            NPC.ai[0] = 4; // afterimages

            // go straight above player, then slam straight down 
            int slamPrepTime = WorldSavingSystem.MasochistModeReal ? 18 : 25;
            int endTime = WorldSavingSystem.MasochistModeReal ? 10 : 20;
            int abovePlayer = 250;
            if (Timer == 0) // moving to slam
            {
                NoContactDamage = 1;
                NPC.noTileCollide = true;
                Vector2 destination = new(Target.Center.X, Target.Center.Y - abovePlayer);
                NPC.velocity = FargoSoulsUtil.SmartAccel(NPC.Bottom, destination, NPC.velocity, 3f, 3f);

                if (NPC.Bottom.Distance(destination) < 80f)
                {
                    Timer = 1;
                    NPC.netUpdate = true;
                }
            }
            else if (Timer < slamPrepTime) // preparing slam
            {
                NoContactDamage = 1;
                if (Timer < slamPrepTime / 5)
                {
                    Vector2 destination = new(Target.Center.X, Target.Center.Y - abovePlayer);
                    NPC.velocity = FargoSoulsUtil.SmartAccel(NPC.Bottom, destination, NPC.velocity, 3f, 3f);
                }
                else
                {
                    NPC.velocity *= 0;
                }
                Timer++;
                if (Timer == slamPrepTime)
                {
                    NPC.velocity.Y = 18;
                    NPC.velocity.X *= 0;
                    NPC.noGravity = true;

                    if (FargoSoulsUtil.HostCheck)
                    {
                        for (int j = -1; j <= 1; j += 2)
                        {
                            int start = WorldSavingSystem.MasochistModeReal ? -3 : -1;
                            for (int i = start; i < 5; i++)
                            {
                                float angle = (i + 5) / 14f;
                                Vector2 dir = -Vector2.UnitY.RotatedBy(angle * MathHelper.PiOver2 * j);
                                dir.X *= 3f + (i * 0.6f);
                                dir.Y *= 2 - MathF.Pow(i * 0.3f, 2f);
                                dir *= 3;
                                dir += Main.rand.NextVector2Circular(0.5f, 0.5f);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + Vector2.Normalize(dir) * 24, dir, ProjectileID.QueenSlimeGelAttack, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 1f, Main.myPlayer);
                            }
                        }
                        
                    }
                }
                    
            }
            else if (Timer == slamPrepTime) // slamming
            {
                NPC.noGravity = true;

                for (int i = 0; i < 6; i++)
                {
                    QSDust(NPC.position, NPC.width, NPC.height, 0, 0);
                }
                if (NPC.Bottom.Y >= Target.Bottom.Y && NPCInAnyTiles(NPC)) // anti-clip technology
                {
                    NPC.velocity.Y = 0;
                    NPC.noGravity = false;
                    NPC.position.Y -= 16;
                }
                    
                if (NPC.velocity.Y == 0) // hit ground
                {
                    Timer = slamPrepTime + 1;
                    NPC.netUpdate = true;
                }
            }
            else
            {
                Timer++;
                if (NPC.Bottom.Y >= Target.Bottom.Y && NPCInAnyTiles(NPC)) // anti-clip technology
                {
                    NPC.velocity.Y = 0;
                    NPC.position.Y -= 16;
                }
                NPC.noGravity = false;
                if (Timer > slamPrepTime + endTime)
                    ResetToNeutral();
            }
        }
        private void MinionSlam()
        {
            NPC.ai[0] = 4; // afterimages

            // go straight above player, then slam straight down 
            int slamPrepTime = WorldSavingSystem.MasochistModeReal ? 18 : 25;
            int endTime = WorldSavingSystem.MasochistModeReal ? 10 : 20;
            int abovePlayer = 250;
            if (Timer == 0) // moving to slam
            {
                NoContactDamage = 1;
                NPC.noTileCollide = true;
                Vector2 destination = new(Target.Center.X, Target.Center.Y - abovePlayer);
                NPC.velocity = FargoSoulsUtil.SmartAccel(NPC.Bottom, destination, NPC.velocity, 3f, 3f);

                if (NPC.Bottom.Distance(destination) < 80f)
                {
                    Timer = 1;
                    NPC.netUpdate = true;
                }
            }
            else if (Timer < slamPrepTime) // preparing slam
            {
                NoContactDamage = 1;
                if (Timer < slamPrepTime / 5)
                {
                    Vector2 destination = new(Target.Center.X, Target.Center.Y - abovePlayer);
                    NPC.velocity = FargoSoulsUtil.SmartAccel(NPC.Bottom, destination, NPC.velocity, 3f, 3f);
                }
                else
                {
                    NPC.velocity *= 0;
                }
                Timer++;
                if (Timer == slamPrepTime)
                {
                    NPC.velocity.Y = 18;
                    NPC.velocity.X *= 0;
                    NPC.noGravity = true;
                }

            }
            else if (Timer == slamPrepTime) // slamming
            {
                NPC.noGravity = true;

                for (int i = 0; i < 6; i++)
                {
                    QSDust(NPC.position, NPC.width, NPC.height, 0, 0);
                }
                if (NPC.Bottom.Y >= Target.Bottom.Y && NPCInAnyTiles(NPC)) // anti-clip technology
                {
                    NPC.velocity.Y = 0;
                    NPC.noGravity = false;
                    NPC.position.Y -= 16;
                }
                    
                if (NPC.velocity.Y == 0) // hit ground
                {
                    Timer = slamPrepTime + 1;
                    NPC.netUpdate = true;

                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                    if (FargoSoulsUtil.HostCheck)
                    {
                        Vector2 focus = Main.player[NPC.target].Center;
                        for (int i = 0; i < 50; i++)
                        {
                            Tile tile = Framing.GetTileSafely(focus);
                            if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                                break;
                            focus.Y += 16f;
                        }
                        focus.Y -= Player.defaultHeight / 2f;

                        for (int i = -5; i <= 5; i++)
                        {
                            Vector2 targetPos = focus;
                            targetPos.X += 330 * i;

                            float minionTravelTime = StompTravelTime + Main.rand.Next(30);
                            float minionGravity = 0.4f;
                            Vector2 vel = targetPos - NPC.Center;
                            vel.X /= minionTravelTime;
                            vel.Y = vel.Y / minionTravelTime - 0.5f * minionGravity * minionTravelTime;

                            FargoSoulsUtil.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<GelatinSlime>(), NPC.whoAmI, minionTravelTime, minionGravity, vel.X, vel.Y, target: NPC.target);
                        }
                    }
                }
            }
            else if (Timer < slamPrepTime + endTime)
            {
                Timer++;
                NPC.noGravity = false;
                if (NPC.Bottom.Y >= Target.Bottom.Y && NPCInAnyTiles(NPC)) // anti-clip technology
                {
                    NPC.velocity.Y = 0;
                    NPC.position.Y -= 16;
                }
            }
            else
            {
                ResetToNeutral();
                //int hopTimer = (int)Timer - (slamPrepTime + endTime);
                //int hops = 2;
                //HopMovement(hopTimer, hops, ResetToNeutral);
            }
        }
        private void QuickHops()
        {
            ref float shotTimer = ref AI0;
            ref float minionDir = ref AI1;
            int startup = 15;
            int hops = 4;
            int endlag = 6;
            if (Timer < startup)
            {
                Timer++;
                if (NPC.velocity.Y == 0)
                    NPC.velocity.X = 0;
                return;
            }
            int hopTimer = (int)Timer - startup;
            if (NPC.velocity.Y == 0)
            {
                if (hopTimer < hops) // start hop
                {
                    float targetVel = Math.Abs(Target.Center.X - NPC.Center.X) / 28f;
                    if (Target.velocity.X.NonZeroSign() != Target.HorizontalDirectionTo(NPC.Center))
                        targetVel += Target.velocity.X / 3;
                    NPC.velocity.X = NPC.HorizontalDirectionTo(Target.Center) * Math.Min(21, Math.Max(10, targetVel));
                    int y = -18;
                    if (Target.Bottom.Y < NPC.Bottom.Y)
                        y = -18;
                    NPC.velocity.Y = y;
                    //if (Math.Abs(Target.Center.X - NPC.Center.X) < 240)
                        //NPC.velocity.X *= -0.7f;
                    Timer++;

                    if (!WorldSavingSystem.MasochistModeReal)
                        shotTimer *= -1;
                    shotTimer = shotTimer.NonZeroSign();
                    minionDir = NPC.HorizontalDirectionTo(Target.Center);
                }
                else
                {
                    Timer++;
                    if (NPC.velocity.Y == 0)
                        NPC.velocity.X = 0;
                    if (hopTimer > hops + endlag)
                    {
                        ResetToNeutral();
                        return;
                    }
                }
            }
            else
            {
                if (shotTimer > 0) // every other jump
                {
                    if (shotTimer == 22)
                    {
                        if (FargoSoulsUtil.HostCheck)
                        {
                            int projs = WorldSavingSystem.MasochistModeReal ? 7 : 7;
                            for (int i = 0; i < projs; i++)
                            {
                                float rot = -Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * (i - projs / 2) / (projs / 2)).RotatedByRandom(MathHelper.PiOver2 * 0.1f).ToRotation();

                                Vector2 dir = rot.ToRotationVector2();
                                dir.X *= 0.7f;
                                dir.Y /= 2;
                                float vel = 11f;
                                Vector2 offset = Main.rand.NextVector2Circular(NPC.width / 8, NPC.height / 8);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset + dir * 24, dir * vel, ProjectileID.QueenSlimeGelAttack, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 1f, Main.myPlayer);
                            }
                        }
                    }
                    shotTimer++;
                }
                
                NPC.velocity.Y += 0.4f;
                float horDir = NPC.HorizontalDirectionTo(Target.Center);
                if (NPC.velocity.X.NonZeroSign() != horDir)
                    NPC.velocity.X += horDir * 0.55f;
            }


        }
        private void Stompy()
        {
            NPC npc = NPC;
            NPC.noGravity = false;
            npc.ai[0] = 4; // afterimages

            if (StompTimer == 0) //ready to super stomp
            {
                StompTimer = 1;

                SoundEngine.PlaySound(SoundID.ForceRoarPitched, npc.Center);

                if (FargoSoulsUtil.HostCheck)
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, npc.whoAmI, NPCID.WallofFleshEye);

                if (PhaseTwo)
                    NPC.noGravity = true;

                npc.netUpdate = true;
                NetSync(npc);
                return;
            }
            else if (StompTimer > 0 && StompTimer < 30) //give time to react
            {
                if (PhaseTwo)
                    NPC.noGravity = true;
                StompTimer++;

                npc.rotation = 0;

                if (NPCInAnyTiles(npc))
                    npc.position.Y -= 16;

                return;
            }
            else if (StompTimer == 30)
            {
                if (!npc.HasValidTarget)
                    npc.TargetClosest(false);

                if (npc.HasValidTarget && StompCounter++ < 3)
                {
                    StompTimer++;

                    npc.ai[1] = 1f;

                    Vector2 target = Main.player[npc.target].Center;
                    for (int i = 0; i < 3; i++)
                    {
                        Tile tile = Framing.GetTileSafely(target);
                        if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                            break;
                        target.Y += 16f;
                    }
                    target.Y -= Player.defaultHeight;

                    Vector2 distance = target - npc.Bottom;
                    if (StompCounter == 1 || StompCounter == 2)
                        distance.X += 300f * Math.Sign(Main.player[npc.target].Center.X - npc.Center.X);
                    float time = StompTravelTime;
                    if (StompCounter < 0) //enraged
                        time /= 2;
                    distance.X /= time;
                    distance.Y = distance.Y / time - 0.5f * StompGravity * time;
                    StompVelocityX = distance.X;
                    StompVelocityY = distance.Y;

                    SoundEngine.PlaySound(SoundID.Item92, npc.Center);

                    npc.netUpdate = true;
                    NetSync(npc);
                    return;
                }
                else //done enough stomps
                {
                    StompCounter = 0;
                    StompTimer = -720;
                    if (PhaseTwo)
                        StompTimer *= 2;

                    npc.velocity.X = 0;

                    ResetToNeutral();
                    return;
                }
            }
            else if (StompTimer > 30)
            {
                npc.rotation = 0;
                npc.noTileCollide = true;

                float time = StompTravelTime;
                if (StompCounter < 0) //enraged
                    time /= 2;

                if (++StompTimer > time + 30)
                {
                    npc.noTileCollide = false;

                    //when landed on a surface
                    if (Grounded(npc) || StompTimer >= time * 2 + 25)
                    {
                        npc.velocity = Vector2.Zero;

                        if (WorldSavingSystem.MasochistModeReal)
                        {
                            StompTimer = 25;
                        }
                        else
                        {
                            StompTimer = /*NPC.AnyNPCs(ModContent.NPCType<GelatinSlime>()) ? 1 :*/ 15;
                        }

                        if (npc.DeathSound != null)
                            SoundEngine.PlaySound(npc.DeathSound.Value, npc.Center);

                        if (FargoSoulsUtil.HostCheck)
                        {
                            int smashDamage = WorldSavingSystem.MasochistModeReal && Main.getGoodWorld
                                ? FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage) : 0;
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ProjectileID.QueenSlimeSmash, smashDamage, 0f, Main.myPlayer);

                            for (int j = -1; j <= 1; j += 2) //spray spikes
                            {
                                Vector2 baseVel = Vector2.UnitX.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(10) * j));
                                const int max = 12;
                                for (int i = 0; i < max; i++)
                                {
                                    Vector2 vel = Main.rand.NextFloat(5f, 15f) * j * baseVel.RotatedBy(MathHelper.PiOver4 * 0.8f / max * i * -j);
                                    vel *= WorldSavingSystem.MasochistModeReal ? 2f : 1.5f;
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, vel, ProjectileID.QueenSlimeMinionBlueSpike, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer);
                                }
                            }
                        }

                        return;
                    }
                }

                //damn queen slime ai glitching out and not fastfalling properly sometimes
                float correction = StompVelocityY - npc.velocity.Y;
                if (correction > StompGravity)
                    npc.position.Y += correction;

                npc.velocity.X = StompVelocityX;
                npc.velocity.Y = StompVelocityY;
                StompVelocityY += StompGravity;

                return;
            }
        }
        private void MinionChargeDirect()
        {
            ref float lockonX = ref AI0;
            ref float lockonY = ref AI1;
            float startupTime = 60;
            float backdashTime = 15;
            float minionMinTime = 60;
            int endTime = 5;

            Timer++;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            if (Timer < startupTime)
            {
                NoContactDamage = 1;
                Vector2 desiredPos = Target.Center + Target.DirectionTo(NPC.Center) * 120;
                Movement(desiredPos, 1.1f);
                if (NPC.Distance(desiredPos) > 50 && Timer > startupTime - 5)
                    Timer--;
                if (NPC.Distance(Target.Center) < 75)
                    NPC.Center = Target.Center + Target.DirectionTo(NPC.Center) * 75;
            }
            else if (Timer < startupTime + backdashTime)
            {
                NPC.velocity *= 0.96f;
                NPC.velocity -= NPC.DirectionTo(Target.Center);
            }
            else
            {
                int distance = 600;
                Vector2 toTarget = NPC.DirectionTo(Target.Center);
                if (Timer == startupTime + backdashTime)
                {
                    SoundEngine.PlaySound(SoundID.Item155 with { Pitch = -0.5f}, NPC.Center);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        for (int i = 0; i < MaxSubjects; i++)
                        {
                            int j = i - (MaxSubjects / 2);
                            float vel = 20f;
                            vel += Math.Abs(j) * 4;
                            Vector2 dir = -toTarget.RotatedBy(j * MathHelper.PiOver2 * 4.7f / MaxSubjects);
                            Vector2 velocity = dir * vel;
                            velocity += -toTarget.RotatedBy(MathHelper.PiOver2 * Math.Sign(j)) * 9f * Math.Abs(j);
                            FargoSoulsUtil.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center + dir * 24f, ModContent.NPCType<GelatinSubject>(), NPC.whoAmI, target: NPC.target,
                                        velocity: velocity, ai0: 0);
                        }
                    }

                    for (int i = 0; i < 220; i++)
                    {
                        QSDust(NPC.position - NPC.Size, NPC.width * 2, NPC.height * 2, 0, 0);
                    }

                    Vector2 lockon = Target.Center + toTarget * distance;
                    lockonX = lockon.X;
                    lockonY = lockon.Y;
                    NPC.netUpdate = true;
                    Timer++;
                }
                if (Timer < startupTime + backdashTime + (minionMinTime * 0.4f))
                {
                    Vector2 lockon = Target.Center + toTarget * distance;
                    lockonX = lockon.X;
                    lockonY = lockon.Y;
                }
                bool shouldReposition = false;

                if (!NPC.AnyNPCs(ModContent.NPCType<GelatinSubject>()))
                {
                    shouldReposition = true;
                    if (Timer > startupTime + backdashTime + minionMinTime  + endTime)
                    {
                        ResetToNeutral();
                    }
                }
                else
                {
                    if (Timer > startupTime + backdashTime + minionMinTime)
                    {
                        Timer--;
                        //shouldReposition = true;
                    }
                        
                }
                Vector2 desiredPos = new(lockonX, lockonY);
                if (shouldReposition)
                    desiredPos = Target.Center + Target.DirectionTo(NPC.Center) * 250;
                Movement(desiredPos, 0.9f);
            }
        }
        private void MinionChargeSide()
        {

        }
        private void Artillery()
        {
            NPC.noTileCollide = true;
            int windup = 60;
            int attackLength = 60;
            int endlag = 30;
            float horDir = NPC.HorizontalDirectionTo(Target.Center);
            if (Timer < windup - 3)
            {
                Vector2 desiredPos = Target.Center + Vector2.UnitX * -horDir * 490;
                Movement(desiredPos, 1f);
            }
            else
            {
                NPC.velocity *= 0.93f;
            }

            Timer++;
            if (Timer < windup)
            {

            }
            else if (Timer < windup + attackLength)
            {
                float attackTimer = Timer - windup;
                float progress = attackTimer / attackLength;
                int freq = WorldSavingSystem.MasochistModeReal ? 5 : 6;
                if (attackTimer % freq == 0)
                {
                    if (FargoSoulsUtil.HostCheck)
                    {
                        float vel = 10f + 20f * MathF.Pow(progress, 0.8f);
                        float sqrt2 = MathF.Sqrt(2);
                        float verticalDir = MathHelper.Lerp(-0.6f, -0.8f, MathF.Pow(progress, 0.8f));
                        Vector2 dir = new(horDir / sqrt2, verticalDir / sqrt2);
                        Vector2 offset = Main.rand.NextVector2Circular(NPC.width / 8, NPC.height / 8);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset + dir * 24, dir * vel, ProjectileID.QueenSlimeGelAttack, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 1f, Main.myPlayer);
                    }
                }
            }
            else if (Timer > windup + attackLength + endlag)
            {
                ResetToNeutral();
            }
        }

        private void FlightExplosions()
        {
            int startup = 42;
            int explosions = 4;
            int windup = 80;
            int endlag = 30;
            int cycleTime = windup + endlag;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.ai[0] = 0;
            Timer++;
            int cycleTimer = Timer <= startup ? -1 : (int)(Timer - startup) % cycleTime;
            if (cycleTimer < 0)
            {
                NoContactDamage = 1;
                Vector2 desiredPos = Target.Center + new Vector2(Target.HorizontalDirectionTo(NPC.Center) * 350, -200);
                Movement(desiredPos, 1f);

                NPC.ai[1] = 0; // reset animation timer
            }
            else if (cycleTimer < windup)
            {
                float windupStart = (int)(windup * 0.35f);
                if (cycleTimer > windupStart)
                {
                    NPC.ai[0] = 5; // explosion animation
                    NPC.ai[1]++; // animation timer
                    for (int num27 = 0; num27 < 10; num27++)
                    {
                        Vector2 size = NPC.Size;
                        size *= 1 - ((float)(cycleTimer - windupStart) / (1 - windupStart)) * 0.8f;

                        QSDust(NPC.Center - size / 2, (int)size.X, (int)size.Y, NPC.velocity.X / 4, NPC.velocity.Y / 4);
                    }
                }
                
            }
            else if (cycleTimer == windup)
            {
                if (FargoSoulsUtil.HostCheck)
                {

                    // extra projectiles aimed at player
                    Vector2 baseOffset = Main.rand.NextVector2Circular(NPC.width / 8, NPC.height / 8);
                    Vector2 spawnPos = NPC.Center + baseOffset;
                    Vector2 baseDir = spawnPos.DirectionTo(Target.Center);
                    baseDir = Vector2.Lerp(baseDir, -Vector2.UnitY, 0.1f).SafeNormalize(baseDir);
                    for (int i = -1; i <= 1; i++)
                    {

                        Vector2 dir = baseDir.RotatedBy(i * MathHelper.PiOver2 * 0.09f).RotatedByRandom(MathHelper.PiOver2 * 0.04f);
                        float vel = 11f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos + dir * 24, dir * vel, ProjectileID.QueenSlimeGelAttack, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 1f, Main.myPlayer);
                    }
                    float projs = 10;
                    float baseInterval = MathHelper.TwoPi / (projs + 1);
                    float baseRot = baseDir.ToRotation() + baseInterval;
                    for (int i = 0; i < projs; i++)
                    {
                        float rot = baseRot + i * baseInterval;

                        Vector2 dir = rot.ToRotationVector2();
                        float vel = 11f;
                        Vector2 offset = Main.rand.NextVector2Circular(NPC.width / 8, NPC.height / 8);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset + dir * 24, dir * vel, ProjectileID.QueenSlimeGelAttack, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 1f, Main.myPlayer);
                    }

                }
            }
            else
            {
                NPC.ai[1] = 0; // reset animation timer
                if (WorldSavingSystem.MasochistModeReal)
                {
                    if (cycleTimer % 15 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item155, NPC.Center);
                        if (FargoSoulsUtil.HostCheck)
                        {
                            Vector2 dir = -NPC.DirectionTo(Target.Center).RotatedByRandom(MathHelper.PiOver2 * 0.6f);
                            Vector2 vel = 20f * dir;
                            FargoSoulsUtil.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<GelatinFlyer>(), NPC.whoAmI, target: NPC.target,
                                    velocity: vel, ai0: 0, ai2: 1);
                        }
                    }
                }
            }
            if (Timer > startup + cycleTime * explosions)
            {
                ResetToNeutral();
                return;
            }

            float desiredX = Target.Center.X;
            float desiredY = Target.Center.Y - 280 + 120 * MathF.Sin(Timer / (cycleTime * 0.3f));
            float xAccel = 0.4f;
            int xMax = 18;
            NPC.velocity.X += Math.Sign(desiredX - NPC.Center.X) * xAccel;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -xMax, xMax);
            NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, (desiredY - NPC.Center.Y) * 0.1f, 0.05f);
        }
        private void SpikeRain()
        {
            NPC.noTileCollide = true;
            int startup = 30;
            int rainDuration = 60 * 4;
            Timer++;
            if (Timer < startup)
            {
                Vector2 desiredPos = Target.Center + new Vector2(Target.HorizontalDirectionTo(NPC.Center) * 100, -400);
                Movement(desiredPos, 2f);
            }
            else if (Timer == startup)
            {
                SoundEngine.PlaySound(SoundID.Item155 with { Pitch = -0.5f }, NPC.Center);
                if (FargoSoulsUtil.HostCheck)
                {
                    for (int i = 0; i < MaxSubjects; i++)
                    {
                        int j = i - (MaxSubjects / 2);
                        float vel = 10f;
                        vel += Math.Abs(j) * 2;
                        Vector2 dir = new(-1, j * 2 / MaxSubjects);
                        Vector2 velocity = dir * vel;
                        FargoSoulsUtil.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center + dir * 24f, ModContent.NPCType<GelatinSubject>(), NPC.whoAmI, target: NPC.target,
                                    velocity: velocity, ai0: 1, ai2: j, ai3: Target.Center.Y - 500);
                    }
                }
            }
            else if (Timer < startup + rainDuration) // just chill
            {
                NoContactDamage = 1;
                if (Timer < startup + 50)
                {
                    Vector2 desiredPos = Target.Center - Vector2.UnitY * 350;
                    Movement(desiredPos, 1f);
                }
                NPC.velocity *= 0.9f;
            }
            else // end attack
            {
                Vector2 desiredPos = Target.Center - Vector2.UnitY * 350;
                Movement(desiredPos, 0.5f);

                if (Timer == startup + rainDuration)
                {
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (npc.TypeAlive<GelatinSubject>())
                            npc.ai[1] = -1;
                    }
                }
                if (!NPC.AnyNPCs(ModContent.NPCType<GelatinSubject>()))
                {
                    ResetToNeutral();
                    return;
                }
            }
        }
        #endregion
        #region Help Methods
        public void ResetState()
        {
            Timer = 0;
            AI0 = 0;
            AI1 = 0;
            NetSync(NPC);
            NPC.netUpdate = true;
        }
        public void ResetToNeutral()
        {
            if (!PhaseTwoHP)
            {
                PreviousState = State;
                State = (int)States.Hops;
            }
            else
            {
                if (FargoSoulsUtil.HostCheck)
                {
                    if (AvailableStates.Count == 0)
                    {
                        List<States> states = [
                            States.MinionChargeDirect, 
                        //States.MinionChargeSide, 
                        States.Artillery,
                        States.FlightExplosions,
                        States.SpikeRain
                            ];
                        AvailableStates.AddRange(states);
                    }
                    AvailableStates.Remove((States)State);
                    State = (int)Main.rand.NextFromCollection(AvailableStates);
                    AvailableStates.Remove((States)State);
                }

            }

            NetSync(NPC);
            NPC.netUpdate = true;
            ResetState();
            CheckStompy();
        }
        public void ChooseAttackP1()
        {
            ResetState();
            if (FargoSoulsUtil.HostCheck)
            {
                List<States> states;
                if (PreviousState != (int)States.MinionSlam && PreviousState != (int)States.QuickHops)
                    states = [States.MinionSlam, States.QuickHops];
                else
                    states = [States.NormalSlam];
                State = (int)Main.rand.NextFromCollection(states);
            }

            NetSync(NPC);
            NPC.netUpdate = true;

            // debug
            //State = (int)States.QuickHops;
            CheckStompy();
        }
        public void CheckStompy()
        {
            if (StompTimer < 0 && (!Collision.CanHitLine(NPC.position, NPC.width, NPC.height, Target.Center, 1, 1) || NPC.Distance(Target.Center) > 1200))
            {
                StompCounter = -3; // enrage
                StompTimer = 0;
            }
                
            if (StompTimer >= 0)
            {
                State = (int)States.TripleSuperslam;
                NetSync(NPC);
            }
        }
        public void Movement(Vector2 targetPos, float speedModifier)
        {
            float accel = 1f * speedModifier;
            float decel = 1.5f * speedModifier;
            float resistance = NPC.velocity.Length() * accel / (22f * speedModifier);
            NPC.velocity = FargoSoulsUtil.SmartAccel(NPC.Center, targetPos, NPC.velocity, accel - resistance, decel + resistance);
        }
        public static void QSDust(Vector2 pos, int width, int height, float velX, float velY, float velMult = 2f)
        {
            Color newColor2 = NPC.AI_121_QueenSlime_GetDustColor();
            newColor2.A = 150;
            int num28 = Dust.NewDust(pos + Vector2.UnitX * -20f, width + 40, height, DustID.TintableDust, velX, velY, 50, newColor2, 1.5f);
            Main.dust[num28].noGravity = true;
            Main.dust[num28].velocity *= velMult;
        }
        private static bool Grounded(NPC npc) => npc.velocity.Y == 0 || NPCInAnyTiles(npc);
        private static bool NPCInAnyTiles(NPC npc)
        {
            //WHERE'S TJHE FKC IJNGI METHOD FOR HTIS? ITS NOT COLLISION.SOLKIDCOLLIOSOM ITS NOPT COLLISON.SOLDITILES I HATE 1.4 IHATE TMODLAOREI I HATE THIS FUSPTID FUCKIGN GNAME SOFU KIGN MCUCH FUCK FUCK FUCK
            bool isInTilesIncludingPlatforms = false;
            for (int x = 0; x < npc.width; x += 16)
            {
                for (float y = npc.height / 2; y < npc.height; y += 16)
                {
                    Tile tile = Framing.GetTileSafely((int)(npc.position.X + x) / 16, (int)(npc.position.Y + y) / 16);
                    if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                    {
                        isInTilesIncludingPlatforms = true;
                        break;
                    }
                }
            }

            return isInTilesIncludingPlatforms;
        }
        #endregion
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (npc.lifeRegen >= 0)
                return;
            if (npc.life < npc.lifeMax / 2)
                modifiers.FinalDamage *= 0.8f;

            base.ModifyIncomingHit(npc, ref modifiers);
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.lifeRegen < 0)
            {
                if (npc.life < npc.lifeMax / 2)
                {
                    npc.lifeRegen = (int)Math.Round(npc.lifeRegen * 0.8f);
                    damage = (int)(Math.Round(damage *0.8f));
                }
            }
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (NoContactDamage > 0)
                return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }
        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.Slimed, 240);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadBossHeadSprite(recolor, 38);
            LoadGore(recolor, 1258);
            LoadGore(recolor, 1259);
            LoadExtra(recolor, 177);
            LoadExtra(recolor, 180);
            LoadExtra(recolor, 185);
            LoadExtra(recolor, 186);
            LoadProjectile(recolor, ProjectileID.QueenSlimeGelAttack);
            LoadProjectile(recolor, ProjectileID.QueenSlimeMinionPinkBall);
            LoadProjectile(recolor, ProjectileID.QueenSlimeMinionBlueSpike);

        }

        private static MiscShaderData queenSlimeBuffer;

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            bool resprite = WorldSavingSystem.EternityMode && SoulConfig.Instance.BossRecolors;
            if (resprite)
            {
                queenSlimeBuffer = GameShaders.Misc["QueenSlime"];
                GameShaders.Misc["QueenSlime"] = GameShaders.Misc["FargowiltasSouls:QueenSlime"];
            }
            Draw(npc, spriteBatch, screenPos, drawColor);
            if (resprite)
            {
                GameShaders.Misc["QueenSlime"] = queenSlimeBuffer;
            }
            return false;
        }
        public void Draw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // hellish modified vanilla drawcode
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (npc.spriteDirection == 1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Color npcColor = Lighting.GetColor((int)((double)npc.position.X + (double)npc.width * 0.5) / 16, (int)(((double)npc.position.Y + (double)npc.height * 0.5) / 16.0));
            if (npc.IsABestiaryIconDummy)
            {
                npcColor = npc.GetBestiaryEntryColor();
            }

            Texture2D value10 = TextureAssets.Npc[npc.type].Value;
            Vector2 position7 = npc.Bottom - screenPos;
            position7.Y += 2f;
            int num53 = Main.npcFrameCount[npc.type];
            int num54 = npc.frame.Y / npc.frame.Height;
            Rectangle rectangle3 = value10.Frame(2, 16, num54 / num53, num54 % num53);
            rectangle3.Inflate(0, -2);
            Vector2 origin4 = rectangle3.Size() * new Vector2(0.5f, 1f);
            Color originColor = Color.Lerp(Microsoft.Xna.Framework.Color.White, npcColor, 0.5f);
            if (npc.life <= npc.lifeMax / 2)
            {
                // wings !!

                if (++WingFrameTimer >= 6)
                {
                    WingFrame++;
                    WingFrameTimer = 0;
                    if (WingFrame >= 4)
                    {
                        WingFrame = 0;
                    }
                }

                if ((States)State == States.TripleSuperslam)
                {
                    WingFrame = 0;
                }

                Texture2D value = TextureAssets.Extra[185].Value;
                Rectangle rectangle = value.Frame(1, 4, 0, WingFrame);
                float scale = 0.8f * npc.scale;
                for (int i = 0; i < 2; i++)
                {
                    float x = 1f;
                    float num = 0f;
                    SpriteEffects effects = SpriteEffects.None;
                    if (i == 1)
                    {
                        x = 0f;
                        num = 0f - num + 2f;
                        effects = SpriteEffects.FlipHorizontally;
                    }
                    Vector2 origin = rectangle.Size() * new Vector2(x, 0.5f);
                    Vector2 vector = new Vector2(npc.Center.X + num, npc.Center.Y);
                    if (npc.rotation != 0f)
                    {
                        vector = vector.RotatedBy(npc.rotation, npc.Bottom);
                    }
                    vector -= screenPos;
                    float num2 = MathHelper.Clamp(npc.velocity.Y, -6f, 6f) * -0.07f;
                    if (i == 0)
                    {
                        num2 *= -1f;
                    }
                    spriteBatch.Draw(value, vector, rectangle, originColor, npc.rotation + num2, origin, scale, effects, 0f);
                }
            }
            Texture2D value11 = TextureAssets.Extra[186].Value; // gem?
            Microsoft.Xna.Framework.Rectangle rectangle4 = value11.Frame();
            Vector2 origin5 = rectangle4.Size() * new Vector2(0.5f, 0.5f);
            Vector2 vector16 = new Vector2(npc.Center.X, npc.Center.Y);
            float num55 = 0f;
            switch (num54)
            {
                case 1:
                case 6:
                    num55 -= 10f;
                    break;
                case 3:
                case 5:
                    num55 += 10f;
                    break;
                case 4:
                case 12:
                case 13:
                case 14:
                case 15:
                    num55 += 18f;
                    break;
                case 7:
                case 8:
                    num55 -= 14f;
                    break;
                case 9:
                    num55 -= 16f;
                    break;
                case 10:
                    num55 -= 18f;
                    break;
                case 11:
                    num55 += 20f;
                    break;
                case 20:
                    num55 -= 14f;
                    break;
                case 21:
                case 23:
                    num55 -= 18f;
                    break;
                case 22:
                    num55 -= 22f;
                    break;
            }
            vector16.Y += num55;
            if (npc.rotation != 0f)
            {
                vector16 = vector16.RotatedBy(npc.rotation, npc.Bottom);
            }
            vector16 -= screenPos;
            if (!npc.IsABestiaryIconDummy)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);
            }
            GameShaders.Misc["QueenSlime"].Apply();
            if (npc.ai[0] == 4f && npc.velocity.Y != 0f)
            {
                float num56 = 1f;
                if (npc.ai[2] == 1f)
                {
                    num56 = 6f;
                }
                for (int num57 = 7; num57 >= 0; num57--)
                {
                    float num58 = 1f - (float)num57 / 8f;
                    Vector2 vector17 = npc.oldPos[num57] + new Vector2((float)npc.width * 0.5f, npc.height);
                    vector17 -= (npc.Bottom - Vector2.Lerp(vector17, npc.Bottom, 0.75f)) * num56;
                    vector17 -= screenPos;
                    Microsoft.Xna.Framework.Color color16 = originColor * num58;
                    spriteBatch.Draw(value10, vector17, rectangle3, color16, npc.rotation, origin4, npc.scale, spriteEffects ^ SpriteEffects.FlipHorizontally, 0f);
                }
            }
            if (!npc.IsABestiaryIconDummy)
            {
                Main.spriteBatch.UseBlendState(BlendState.Additive);
                if (NPC.scale < 1)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 3f;
                        Color glowColor = Color.Purple;
                        spriteBatch.Draw(value11, vector16 + afterimageOffset, rectangle4, glowColor, npc.rotation, origin5, 1f, spriteEffects ^ SpriteEffects.FlipHorizontally, 0f);
                    }
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            }
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Draw(value11, vector16, rectangle4, originColor, npc.rotation, origin5, 1f, spriteEffects ^ SpriteEffects.FlipHorizontally, 0f);
            GameShaders.Misc["QueenSlime"].Apply();
            if (!npc.IsABestiaryIconDummy)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);
            }
            DrawData value12 = new DrawData(value10, position7, rectangle3, npc.GetAlpha(originColor), npc.rotation, origin4, npc.scale, spriteEffects ^ SpriteEffects.FlipHorizontally);
            GameShaders.Misc["QueenSlime"].Apply(value12);
            value12.Draw(spriteBatch);
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            if (!npc.IsABestiaryIconDummy)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            }
            Texture2D value13 = TextureAssets.Extra[177].Value; // crown
            rectangle3 = value13.Frame();
            origin4 = rectangle3.Size() * new Vector2(0.5f, 0.5f);
            position7 = new Vector2(npc.Center.X, npc.Top.Y - (float)rectangle3.Bottom + 44f);
            float num59 = 0f;
            switch (num54)
            {
                case 1:
                    num59 -= 10f;
                    break;
                case 3:
                case 5:
                case 6:
                    num59 += 10f;
                    break;
                case 4:
                case 12:
                case 13:
                case 14:
                case 15:
                    num59 += 18f;
                    break;
                case 7:
                case 8:
                    num59 -= 14f;
                    break;
                case 9:
                    num59 -= 16f;
                    break;
                case 10:
                    num59 -= 18f;
                    break;
                case 11:
                    num59 += 20f;
                    break;
                case 20:
                    num59 -= 14f;
                    break;
                case 21:
                case 23:
                    num59 -= 18f;
                    break;
                case 22:
                    num59 -= 22f;
                    break;
            }
            position7.Y += num59;
            if (npc.rotation != 0f)
            {
                position7 = position7.RotatedBy(npc.rotation, npc.Bottom);
            }
            position7 -= screenPos;
            spriteBatch.Draw(value13, position7, rectangle3, originColor, npc.rotation, origin4, npc.scale, spriteEffects ^ SpriteEffects.FlipHorizontally, 0f);
            return;
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            
        }

        public override void FindFrame(NPC npc, int frameHeight)
        {
            bool p2 = PhaseTwo;
            npc.frame.Width = 180;
            int num9 = npc.frame.Y / frameHeight;
            if ((States)State == States.FlightExplosions)
            {
                if (npc.ai[0] == 5f)
                {
                    npc.frameCounter = 0.0;
                    num9 = ((int)npc.ai[1] / 3 % 3) switch
                    {
                        1 => 14,
                        2 => 15,
                        _ => 13,
                    };
                    npc.frame.Y = num9 * frameHeight;
                }
            }

            /*
            if ((flag && noGravity) || velocity.Y < 0f)
            {
                if (num9 < 20 || num9 > 23)
                {
                    if (num9 < 4 || num9 > 7)
                    {
                        num9 = 4;
                        frameCounter = -1.0;
                    }
                    if ((frameCounter += 1.0) >= 4.0)
                    {
                        frameCounter = 0.0;
                        num9++;
                        if (num9 >= 7)
                        {
                            num9 = ((!flag) ? 7 : 22);
                        }
                    }
                }
                else if ((frameCounter += 1.0) >= 5.0)
                {
                    frameCounter = 0.0;
                    num9++;
                    if (num9 >= 24)
                    {
                        num9 = 20;
                    }
                }
                frame.Y = num9 * num;
            }
            else if (velocity.Y > 0f)
            {
                if (num9 < 8 || num9 > 10)
                {
                    num9 = 8;
                    frameCounter = -1.0;
                }
                if ((frameCounter += 1.0) >= 8.0)
                {
                    frameCounter = 0.0;
                    num9++;
                    if (num9 >= 10)
                    {
                        num9 = 10;
                    }
                }
                frame.Y = num9 * num;
            }
            else
            {
                if (velocity.Y != 0f)
                {
                    break;
                }
                if (ai[0] == 5f)
                {
                    frameCounter = 0.0;
                    num9 = ((int)ai[1] / 3 % 3) switch
                    {
                        1 => 14,
                        2 => 15,
                        _ => 13,
                    };
                }
                else if (ai[0] == 4f)
                {
                    frameCounter = 0.0;
                    switch ((int)ai[1] / 15)
                    {
                        default:
                            num9 = 12;
                            break;
                        case 1:
                            num9 = 11;
                            break;
                        case 2:
                        case 3:
                            num9 = 10;
                            break;
                    }
                }
                else
                {
                    bool flag2 = num9 >= 10 && num9 <= 12;
                    int num10 = 10;
                    if (flag2)
                    {
                        num10 = 6;
                    }
                    if (!flag2 && num9 >= 4)
                    {
                        num9 = 0;
                        frameCounter = -1.0;
                    }
                    if ((frameCounter += 1.0) >= (double)num10)
                    {
                        frameCounter = 0.0;
                        num9++;
                        if ((!flag2 || num9 == 13) && num9 >= 4)
                        {
                            num9 = 0;
                        }
                    }
                }
                frame.Y = num9 * num;
            }
            */
        }
    }

    public class QueenSlimeMinion : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.QueenSlimeMinionBlue,
            NPCID.QueenSlimeMinionPink,
            NPCID.QueenSlimeMinionPurple
        );

        public bool TimeToFly;
        public bool Landed;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            bitWriter.WriteBit(TimeToFly);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            TimeToFly = bitReader.ReadBit();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            if (WorldSavingSystem.MasochistModeReal)
                npc.knockBackResist = 0;
        }

        public override void OnFirstTick(NPC npc)
        {
            if (NPC.AnyNPCs(NPCID.QueenSlimeBoss))
            {
                npc.life = 0;
                npc.active = false;
                npc.timeLeft = 0;
                return;
            }
        }
        public override void AI(NPC npc)
        {
            if (NPC.AnyNPCs(NPCID.QueenSlimeBoss))
            {
                npc.life = 0;
                npc.active = false;
                npc.timeLeft = 0;
                return;
            }
            base.AI(npc);

            if (WorldSavingSystem.MasochistModeReal)
            {
                if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.queenSlimeBoss, NPCID.QueenSlimeBoss))
                {
                    Vector2 target = Main.player[Main.npc[EModeGlobalNPC.queenSlimeBoss].target].Top;
                    if (TimeToFly)
                    {

                        npc.velocity = Math.Min(npc.velocity.Length(), 20f) * npc.SafeDirectionTo(target);
                        npc.position += 8f * npc.SafeDirectionTo(target);

                        if (npc.Distance(target) < 300f)
                        {
                            TimeToFly = false;
                            NetSync(npc);

                            npc.velocity += 8f * npc.SafeDirectionTo(target).RotatedByRandom(MathHelper.PiOver4);
                            npc.netUpdate = true;
                        }
                    }
                    else if (npc.Distance(target) > 900f)
                    {
                        TimeToFly = true;
                        NetSync(npc);
                    }
                }
                else
                {
                    TimeToFly = false;
                }

                npc.noTileCollide = TimeToFly;
            }
            else
            {
                npc.localAI[0] = 30f; //prevent firing

                if (npc.type == NPCID.QueenSlimeMinionPurple)
                {
                    npc.position -= npc.velocity * 0.5f;
                }
                else
                {
                    if (!Landed) //tl;dr dont fall on the player
                    {
                        if (npc.velocity.Y == 0)
                            Landed = true;

                        Player p = FargoSoulsUtil.PlayerExists(Player.FindClosest(npc.Center, 0, 0));
                        if (p != null)
                        {
                            const float minDist = 16 * 8;
                            float dist = npc.Center.X - p.Center.X;
                            if (Math.Abs(dist) < minDist)
                            {
                                npc.velocity.X *= 0.95f;
                                npc.velocity.X += (minDist - Math.Abs(dist)) * Math.Sign(dist) * 0.05f;
                            }
                        }
                    }
                }
            }

            //if (npc.velocity.Y != 0)
            //    npc.localAI[0] = 25f;
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.Slimed, 180);
            target.AddBuff(ModContent.BuffType<SmiteBuff>(), 360);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadGore(recolor, 1260);
        }
    }
   
}
