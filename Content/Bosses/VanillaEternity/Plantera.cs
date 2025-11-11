using System.IO;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Common.Graphics.Particles;
using Microsoft.Xna.Framework.Graphics;
using FargowiltasSouls.Core;
using Luminance.Core.Graphics;
using Terraria.DataStructures;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Plantera;
using FargowiltasSouls.Content.Projectiles.Eternity;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.BossMinions;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public abstract class PlanteraPart : EModeNPCBehaviour
    {
        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Poisoned] = true;
            npc.buffImmune[BuffID.Venom] = true;
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<IvyVenomBuff>(), 240);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
        }
    }

    public class Plantera : PlanteraPart
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Plantera);

        // aiStyle = 51

        public int DicerTimer;
        public int RingTossTimer;
        public int TentacleTimer = 480; //line up first tentacles with ring toss lmao, 600

        public int CrystalRedirectTimer = 0;
        //public int TentacleTimerMaso;

        public float TentacleAttackAngleOffset;

        public bool IsVenomEnraged;
        public bool InPhase2;
        public bool EnteredPhase2;
        public bool EnteredPhase3;

        public bool DroppedSummon;

        public static readonly SoundStyle VineGrowth = new("FargowiltasSouls/Assets/Sounds/VanillaEternity/Plantera/PlanteraVineGrowth");

        public int DashTimer = 0;
        public bool Dashing = false;


        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(DicerTimer);
            binaryWriter.Write(RingTossTimer);
            binaryWriter.Write(TentacleTimer);
            binaryWriter.Write(CrystalRedirectTimer);
            //binaryWriter.Write(TentacleTimerMaso);
            binaryWriter.Write(npc.localAI[0]);
            binaryWriter.Write(npc.localAI[1]);
            binaryWriter.Write(npc.localAI[2]);
            bitWriter.WriteBit(IsVenomEnraged);
            bitWriter.WriteBit(InPhase2);
            bitWriter.WriteBit(EnteredPhase2);
            bitWriter.WriteBit(EnteredPhase3);

            binaryWriter.Write(DashTimer);
            binaryWriter.Write(Dashing);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            DicerTimer = binaryReader.ReadInt32();
            RingTossTimer = binaryReader.ReadInt32();
            TentacleTimer = binaryReader.ReadInt32();
            CrystalRedirectTimer = binaryReader.ReadInt32();
            //TentacleTimerMaso = binaryReader.ReadInt32();
            npc.localAI[0] = binaryReader.ReadSingle();
            npc.localAI[1] = binaryReader.ReadSingle();
            npc.localAI[2] = binaryReader.ReadSingle();
            IsVenomEnraged = bitReader.ReadBit();
            InPhase2 = bitReader.ReadBit();
            EnteredPhase2 = bitReader.ReadBit();
            EnteredPhase3 = bitReader.ReadBit();

            DashTimer = binaryReader.ReadInt32();
            Dashing = binaryReader.ReadBoolean();
        }
        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);
            npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.7f);

            if (WorldSavingSystem.SwarmActive)
                npc.lifeMax /= 3;
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            IsVenomEnraged = false;

            if (!EnteredPhase3 && (!npc.HasValidTarget || npc.Distance(Main.player[npc.target].Center) > 3000))
            {
                npc.velocity.Y++;
                npc.TargetClosest(false);
                if (!npc.HasValidTarget || npc.Distance(Main.player[npc.target].Center) > 3000)
                {
                    if (npc.timeLeft > 60)
                        npc.timeLeft = 60;
                }
            }

            Player player = Main.player[npc.target];

            const float innerRingDistance = 130f;
            const int delayForRingToss = 360 + 120;

            #region Phase 3
            if (!EnteredPhase3 && npc.GetLifePercent() < 0.25f)
            {
                EnteredPhase3 = true;
                SoundEngine.PlaySound(SoundID.Zombie21, npc.Center);


                npc.localAI[1] = 0;
                // these are unused but safeguarding anyway
                npc.ai[0] = 0;
                npc.ai[1] = 0;
                npc.ai[2] = 0;
                npc.ai[3] = 0;

                FargoSoulsUtil.ClearHostileProjectiles(2, npc.whoAmI);
                foreach (NPC n in Main.npc.Where(n => n.TypeAlive<CrystalLeaf>() && n.ai[0] == npc.whoAmI)) // delete crystal ring
                {
                    n.life = 0;
                    n.HitEffect();
                    n.checkDead();
                    n.active = false;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n.whoAmI);
                }

                const int halfAmt = 20;
                for (int i = -halfAmt; i <= halfAmt; i++)
                {
                    if (FargoSoulsUtil.HostCheck)
                    {
                        int type = Main.rand.NextFromList(ModContent.ProjectileType<PlanteraManEater>(), ModContent.ProjectileType<PlanteraSnatcher>(), ModContent.ProjectileType<PlanteraTrapper>());
                        float offset = Main.rand.NextFloat(MathHelper.Pi / halfAmt);
                        Vector2 dir = Vector2.UnitY.RotatedBy(offset + MathHelper.Pi * ((float)i / halfAmt));
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + dir * 1500, -dir * 6,
                            type, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, npc.whoAmI, dir.ToRotation());
                    }
                }
                //int ritual1 = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero,
                //ModContent.ProjectileType<PlanteraRitual>(), 0, 0f, Main.myPlayer, npc.lifeMax, npc.whoAmI);
            }
            if (EnteredPhase3)
            {

                ref float timer = ref npc.ai[0];
                ref float state = ref npc.ai[1];
                ref float movementTimer = ref npc.ai[2];
                ref float ai3 = ref npc.ai[3];


                //Targeting
                if (!player.active || player.dead || player.ghost || npc.Distance(player.Center) > 3000)
                {
                    npc.TargetClosest(false);
                    player = Main.player[npc.target];
                    if (!player.active || player.dead || player.ghost || npc.Distance(player.Center) > 3000)
                    {
                        if (npc.timeLeft > 60)
                            npc.timeLeft = 60;
                        npc.velocity.Y -= 0.4f;
                        return false;
                    }
                }
                else
                {
                    if (npc.timeLeft < 60)
                        npc.timeLeft = 60;
                }

                if (FargoSoulsUtil.HostCheck && !Main.npc.Any(n => n.active && n.type == ModContent.NPCType<CrystalLeaf>() && n.ai[0] == npc.whoAmI && n.ai[1] == innerRingDistance))
                {
                    const int max = 5;
                    float rotation = 2f * (float)Math.PI / max;
                    for (int i = 0; i < max; i++)
                    {
                        Vector2 spawnPos = npc.Center + new Vector2(innerRingDistance, 0f).RotatedBy(rotation * i);
                        FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), spawnPos, ModContent.NPCType<CrystalLeaf>(), 0, npc.whoAmI, innerRingDistance, 0, rotation * i);
                    }
                }

                EnsureInnerRingSpawned();

                npc.rotation = npc.SafeDirectionTo(player.Center).ToRotation() + MathHelper.PiOver2;

                #region Movement
                void Movement(Vector2 target, float speedMultiplier = 1f)
                {
                    float accel = 0.4f * speedMultiplier;
                    float decel = 0.7f * speedMultiplier;
                    float resistance = npc.velocity.Length() * accel / (35f * speedMultiplier);
                    npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, target, npc.velocity, accel - resistance, decel + resistance);
                }

                if (state == 0) // Phase transition movement
                {
                    Vector2 playerToNPC = (npc.Center - player.Center);
                    float distX;
                    if (Math.Sign(playerToNPC.Y) > -10)
                        distX = Utils.Clamp(Math.Abs(playerToNPC.X), 300, 500) * Math.Sign(playerToNPC.X);
                    else
                        distX = 0;
                    float targetX = player.Center.X + Math.Sign(playerToNPC.X) * distX;
                    float distY = 50;
                    float targetY = player.Center.Y - distY;
                    Vector2 targetPos = Vector2.UnitX * targetX + Vector2.UnitY * targetY;


                    Vector2 offset = player.DirectionTo(npc.Center) * 400f;
                    MovementAvoidWalls(player.Center + offset, speedMultiplier: 0.5f);

                    if (timer == 50)
                        Vineburst();
                    if (timer >= 46)
                        npc.velocity *= 0.8f;

                    if (timer > 140)
                    {
                        timer = 0;
                        state = 1;
                        npc.TargetClosest(false);
                    }
                }
                else
                {
                    movementTimer++;
                }

                bool MovementAvoidWalls(Vector2 target, int depth = 0, int direction = 0, float speedMultiplier = 1f)
                {
                    Vector2 dir = target.DirectionTo(npc.Center);
                    float distance = target.Distance(npc.Center);
                    int parts = 12;

                    for (int i = 0; i < parts; i++)
                    {
                        if (i > parts / 2) // too close, try a different direction, up to 5 times
                        {
                            if (depth > 5)
                            {
                                break;
                            }
                            if (direction != 1 && !MovementAvoidWalls(target + dir.RotatedBy(MathHelper.PiOver2 * 0.2f) * distance, depth + 1, -1))
                                if (direction != -1 && !MovementAvoidWalls(target + dir.RotatedBy(-MathHelper.PiOver2 * 0.2f) * distance, depth + 1, 1))
                                    if (direction == 0)
                                        Movement(target, speedMultiplier); // give up
                            break;
                        }
                        Vector2 pos = target + dir * i * (distance / parts);
                        if (Collision.SolidTiles(pos - npc.Size / 2, npc.width, npc.height))
                            continue;
                        // succeeded
                        Movement(pos, speedMultiplier);
                        break;
                    }
                    return true;
                }

                void Vineburst()
                {
                    SoundEngine.PlaySound(VineGrowth with { Volume = 3 }, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        for (int i = -2; i <= 2; i++)
                        {
                            if (i == 0)
                                continue;
                            Vector2 dir = npc.DirectionTo(player.Center);
                            dir = dir.RotatedBy(MathHelper.PiOver2 * 1.2f * MathF.Sign(i));
                            dir = dir.RotatedBy(MathHelper.PiOver2 * 0.3f * i);

                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + dir, dir * 14,
                                ModContent.ProjectileType<PlanteraSpikevine>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer);
                        }
                    }
                }
                #endregion

                #region Attacks
                switch (state) // ATTACKS
                {
                    case 1: // crystal madness
                        {
                            ref float shotTimer = ref npc.localAI[1];

                            Vector2 offset = player.DirectionTo(npc.Center) * 400f;
                            if (!Main.projectile.Any(p => p.TypeAlive<PlanteraSpikevine>() && p.ai[0] < 60))
                            {
                                if (timer < 60 * 6)
                                    MovementAvoidWalls(player.Center + offset, speedMultiplier: 0.3f);
                                else
                                    MovementAvoidWalls(player.Center + offset.RotateTowards((-Vector2.UnitY).ToRotation(), 0.1f), speedMultiplier: 0.2f);
                            }
                            else
                                npc.velocity *= 0.6f;

                            float attackDuration = LumUtils.SecondsToFrames(9);

                            float startMult = WorldSavingSystem.MasochistModeReal ? 2 : 3;
                            float progress = MathHelper.Clamp((timer * 2f) / attackDuration, 0, 1);
                            int shotTime = (int)(17f * MathHelper.Lerp(startMult, 1f, progress));
                            shotTimer++;
                            if (shotTimer >= shotTime && timer > 0 && timer < 60 * 6)
                            {
                                shotTimer = 0;
                                foreach (NPC leaf in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<CrystalLeaf>() && n.ai[0] == npc.whoAmI && n.ai[1] == innerRingDistance))
                                {
                                    SoundEngine.PlaySound(SoundID.Grass, leaf.Center);
                                    if (FargoSoulsUtil.HostCheck)
                                    {
                                        Vector2 dir = npc.SafeDirectionTo(leaf.Center);
                                        Projectile.NewProjectile(Entity.InheritSource(leaf), leaf.Center, 7f * dir, ModContent.ProjectileType<CrystalLeafShot>(),
                                            FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, ai0: npc.whoAmI);
                                    }
                                }
                            }

                            if (WorldSavingSystem.MasochistModeReal && timer == 60 * 3f)
                                Vineburst();

                            if (timer == 60 * 6) // redirect
                            {
                                SoundEngine.PlaySound(SoundID.Zombie21 with { Pitch = -0.3f }, npc.Center);
                                bool recolor = SoulConfig.Instance.BossRecolors && WorldSavingSystem.EternityMode;
                                Color color = recolor ? Color.DeepSkyBlue : Color.LimeGreen;
                                Color color2 = recolor ? Color.DarkBlue : Color.ForestGreen;
                                Particle particle = new ExpandingBloomParticle(npc.Center, Vector2.Zero, color, Vector2.Zero, Vector2.One * 100f, 20, true, color2);
                                particle.Spawn();

                                foreach (Projectile p in Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<CrystalLeafShot>() && p.ai[0] == npc.whoAmI)) //my crystal leaves
                                {
                                    p.ai[1] = 1;
                                    p.ai[2] = player.whoAmI;
                                    p.netUpdate = true;
                                }
                            }
                            if (timer >= attackDuration)
                            {
                                timer = 0;
                                shotTimer = 0;
                                state = 2;
                                npc.TargetClosest(false);
                                if (timer == 60 * 8 && WorldSavingSystem.MasochistModeReal)
                                    Vineburst();
                            }
                        }
                        break;
                    case 2:  // teeth spread
                        {
                            ref float repeatCheck = ref npc.localAI[1];

                            const int vineSpawnTime = 40;

                            int shotStartTime = vineSpawnTime + (WorldSavingSystem.MasochistModeReal ? -20 : 20);
                            float endTime = vineSpawnTime * (repeatCheck == 1 ? 5f : 4.5f);
                            if (timer >= shotStartTime)
                            {
                                Vector2 offset = player.DirectionTo(npc.Center) * 400f;
                                if (!Main.projectile.Any(p => p.TypeAlive<PlanteraSpikevine>() && p.ai[0] < 60))
                                    MovementAvoidWalls(player.Center + offset, speedMultiplier: 0.26f);
                                else
                                    npc.velocity *= 0.6f;

                                float midTime = MathHelper.Lerp(shotStartTime, endTime, 0.5f);
                                if (WorldSavingSystem.MasochistModeReal)
                                    midTime += 60;

                                int freq = 4;
                                if (!WorldSavingSystem.MasochistModeReal)
                                    freq = 6;
                                if (timer % freq == 0)
                                {
                                    SoundEngine.PlaySound(SoundID.NPCDeath13, npc.Center);
                                    if (FargoSoulsUtil.HostCheck)
                                    {
                                        float maxSpread = MathHelper.PiOver2 * 0.73f;
                                        float side = (repeatCheck != 0 ? -1 : 1);
                                        float angle = npc.DirectionTo(player.Center).RotatedBy(MathF.Sin(side * timer * MathHelper.TwoPi / 57f) * maxSpread).ToRotation();

                                        float speed = 1;
                                        Vector2 dir = angle.ToRotationVector2();
                                        int alt = Main.rand.NextFromList((int)PlanteraTooth.Alts.BigNormal, (int)(PlanteraTooth.Alts.BigAlt));
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + dir * npc.width / 2f, dir * speed,
                                            ModContent.ProjectileType<PlanteraTooth>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, 20, ai1: alt);
                                    }
                                }
                                /* ceroba
                                if (timer % 50 == 0)
                                {
                                    SoundEngine.PlaySound(SoundID.NPCDeath13, npc.Center);
                                    if (FargoSoulsUtil.HostCheck)
                                    {
                                        float spreadAngle = WorldSavingSystem.MasochistModeReal ? 0.18f : 0.27f;
                                        int spreadAmount = WorldSavingSystem.MasochistModeReal ? 3 : 2;
                                        for (int i = -spreadAmount; i <= spreadAmount; i++)
                                        {
                                            float angle = npc.SafeDirectionTo(player.Center).ToRotation();
                                            float speed = 1;
                                            angle += i * MathHelper.PiOver2 * spreadAngle;
                                            Vector2 dir = angle.ToRotationVector2();
                                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + dir * npc.width / 2f, dir * speed,
                                                ModContent.ProjectileType<PlanteraTooth>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, 4, ai1: (int)PlanteraTooth.Alts.Small);
                                        }
                                        for (int i = -spreadAmount; i <= spreadAmount + 1; i++)
                                        {
                                            float x = i - 0.5f;
                                            float angle = npc.SafeDirectionTo(player.Center).ToRotation();
                                            float speed = 2;
                                            angle += x * MathHelper.PiOver2 * spreadAngle;
                                            Vector2 dir = angle.ToRotationVector2();
                                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center + dir * npc.width / 2f, dir * speed,
                                                ModContent.ProjectileType<PlanteraTooth>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, 8);
                                        }
                                    }
                                }
                                */
                            }

                            if (timer < vineSpawnTime)
                            {
                                npc.velocity *= 0.96f;
                                if (timer == 5)
                                    Vineburst();
                            }
                            else
                            {
                                npc.velocity *= 0.96f;
                                if (timer > endTime)
                                {
                                    timer = 0;
                                    if (repeatCheck == 0)
                                    {
                                        repeatCheck = 1;
                                    }
                                    else
                                    {
                                        repeatCheck = 0;
                                        timer = -90;
                                        state = 3;
                                        npc.TargetClosest(false);
                                    }
                                }
                            }

                        }
                        break;
                    case 3: // cone shots
                        {
                            const int vineSpawnTime = 100;

                            if (timer < vineSpawnTime)
                            {

                                float vineProgress = timer / vineSpawnTime;

                                if (timer < vineSpawnTime * 0.7f)
                                {
                                    Vector2 offset = player.DirectionTo(npc.Center) * 450f;
                                    //if (offset.Y > 0)
                                    //    MovementAvoidWalls(player.Center + offset.RotateTowards(Vector2.UnitY.ToRotation(), 0.1f));
                                    //else
                                    float speed = 0.05f;
                                    if (timer < 0)
                                        speed /= 8;
                                    MovementAvoidWalls(player.Center + offset.RotateTowards((-Vector2.UnitY).ToRotation(), speed));
                                }
                                else
                                    npc.velocity *= 0.96f;

                                for (int i = -1; i <= 1; i += 2)
                                {
                                    float attackAngle = Vector2.Lerp(-Vector2.UnitY.RotatedBy(-i * MathHelper.PiOver2 * 0.1f), Vector2.UnitY.RotatedBy(i * MathHelper.PiOver2 * 0.3f), vineProgress).ToRotation();


                                    const int freq = 5;
                                    if (timer % freq == freq - 1)
                                    {
                                        if (FargoSoulsUtil.HostCheck)
                                        {
                                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, attackAngle.ToRotationVector2().RotatedByRandom(MathHelper.PiOver4) * 24,
                                                ModContent.ProjectileType<PlanteraTentacle>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, npc.whoAmI, attackAngle);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //WallHugMovement(true, 1, 1);
                                npc.velocity *= 0.96f;
                                /*
                                if (timer % 200 == 0)
                                {
                                    float attackAngle = npc.SafeDirectionTo(player.Center).ToRotation();
                                    if (FargoSoulsUtil.HostCheck)
                                    {
                                        int p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, attackAngle.ToRotationVector2().RotatedByRandom(MathHelper.PiOver4) * 24,
                                            ModContent.ProjectileType<PlanteraTentacle>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer, npc.whoAmI, attackAngle);
                                        if (p.IsWithinBounds(Main.maxProjectiles))
                                        {
                                            Main.projectile[p].extraUpdates += 1;
                                        }
                                    }
                                }
                                */
                                int freq = WorldSavingSystem.MasochistModeReal ? 9 : 14;
                                if (timer % freq == 0 && (timer > vineSpawnTime || WorldSavingSystem.MasochistModeReal) && timer < vineSpawnTime * 3.33f)
                                {
                                    if (timer % (freq * 4) <= freq * 2)
                                    {
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, npc.SafeDirectionTo(player.Center),
                                            ModContent.ProjectileType<PlanteraMushroomThing>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer);
                                    }
                                }
                                const int endTime = 550;
                                if (timer == endTime - 110)
                                    Vineburst();
                                if (timer == endTime - 60) // kill and instantly respawn ring, 1 second before attack1 starts
                                {
                                    for (int i = 0; i < Main.maxNPCs; i++)
                                    {
                                        if (Main.npc[i].TypeAlive<CrystalLeaf>() && Main.npc[i].ai[0] == npc.whoAmI)
                                        {
                                            Main.npc[i].StrikeInstantKill();
                                        }
                                    }


                                }
                                if (timer > endTime)
                                {
                                    timer = -90;
                                    state = 1;
                                    npc.TargetClosest(false);
                                }
                            }
                        }
                        break;
                }
                #endregion
                if (npc.Center.Y < player.Center.Y)
                {
                    float maxClimbSpeed = MathHelper.Lerp(-6f, -3f, Math.Clamp(player.Center.Y - npc.Center.Y, 0, 1000f) / 1000f);
                    if (npc.velocity.Y < maxClimbSpeed) // Cap climbing velocity
                        npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, maxClimbSpeed, 0.2f);
                }

                timer++;
                return false;
            }
            #endregion

            void EnsureInnerRingSpawned()
            {
                if (FargoSoulsUtil.HostCheck && !Main.npc.Any(n => n.active && n.type == ModContent.NPCType<CrystalLeaf>() && n.ai[0] == npc.whoAmI && n.ai[1] == innerRingDistance))
                {
                    const int max = 5;
                    float rotation = 2f * (float)Math.PI / max;
                    for (int i = 0; i < max; i++)
                    {
                        Vector2 spawnPos = npc.Center + new Vector2(innerRingDistance, 0f).RotatedBy(rotation * i);
                        FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), spawnPos, ModContent.NPCType<CrystalLeaf>(), 0, npc.whoAmI, innerRingDistance, 0, rotation * i);
                    }
                }
            }

            if (Dashing)
            {
                if (DashTimer == 0)
                {
                    SoundEngine.PlaySound(SoundID.Zombie21, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        int ai = SoulConfig.Instance.BossRecolors && WorldSavingSystem.EternityMode ? -12 : -11;
                        Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0, -1, npc.whoAmI, ai);
                    }
                }
                if (DashTimer < 0)
                {
                    npc.rotation = npc.AngleTo(player.Center);

                }
                int dashTelegraph = WorldSavingSystem.MasochistModeReal ? 30 : 45;
                int dashTime = 60;
                if (DashTimer == dashTelegraph)
                {
                    SoundEngine.PlaySound(SoundID.Roar, npc.Center);
                    npc.velocity = (player.Center - npc.Center).SafeNormalize(Vector2.Zero) * 15;
                    /*
                    if (FargoSoulsUtil.HostCheck)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, new Vector2(0, Main.rand.Next(5, 10)).RotatedBy(MathHelper.ToRadians(360 / 15f * i)), ModContent.ProjectileType<SporeGasPlantera>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage / 2), 0);
                        }
                    }
                    */
                }
                DashTimer++;
                if (DashTimer >= dashTelegraph + dashTime)
                {
                    Dashing = false;
                    DashTimer = 0;
                }
                return false;
            }
            if (TentacleTimer == -390)
            {
                Dashing = true;
            }

            if (--RingTossTimer < 0)
            {
                RingTossTimer = delayForRingToss;
                EnsureInnerRingSpawned();
            }
            else if (RingTossTimer == 120)
            {

                //if (WorldSavingSystem.MasochistModeReal)
                //    RingTossTimer = 0; //instantly spawn next set of crystals

                npc.netUpdate = true;
                NetSync(npc);

                if (FargoSoulsUtil.HostCheck) // do ring toss
                {
                    float speed = 8f;
                    Vector2 direction;
                    if (WorldSavingSystem.MasochistModeReal)
                    {
                        direction = FargoSoulsUtil.PredictiveAim(npc.Center, player.Center, player.velocity, speed * 2);
                        direction.Normalize();
                    }
                    else
                    {
                        direction = npc.SafeDirectionTo(player.Center);
                    }
                    int p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, speed * direction, ModContent.ProjectileType<MutantMark2>(), npc.defDamage / 4, 0f, Main.myPlayer);
                    if (p != Main.maxProjectiles)
                    {
                        Main.projectile[p].timeLeft -= 300;

                        foreach (NPC n in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<CrystalLeaf>() && n.ai[0] == npc.whoAmI && n.ai[1] == innerRingDistance)) //my crystal leaves
                        {
                            SoundEngine.PlaySound(SoundID.Grass, n.Center);
                            Projectile.NewProjectile(npc.GetSource_FromThis(), n.Center, Vector2.Zero, ModContent.ProjectileType<PlanteraCrystalLeafRing>(), npc.defDamage / 4, 0f, Main.myPlayer, Main.projectile[p].identity, n.ai[3]);

                            n.life = 0;
                            n.HitEffect();
                            n.checkDead();
                            n.active = false;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n.whoAmI);
                        }
                    }
                }

            }
            if (!InPhase2) // redirect attack
            {
                if (RingTossTimer == 360 + 60)
                {
                    if (CrystalRedirectTimer >= 2) // every 3 throws, redirect instead of throwing
                    {
                        SoundEngine.PlaySound(SoundID.Zombie21 with { Pitch = -0.3f }, npc.Center);
                        bool recolor = SoulConfig.Instance.BossRecolors && WorldSavingSystem.EternityMode;
                        Color color = recolor ? Color.DeepSkyBlue : Color.LimeGreen;
                        Color color2 = recolor ? Color.DarkBlue : Color.ForestGreen;
                        Particle particle = new ExpandingBloomParticle(npc.Center, Vector2.Zero, color, Vector2.Zero, Vector2.One * 100f, 20, true, color2);
                        particle.Spawn();

                        foreach (Projectile p in Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<CrystalLeafShot>() && p.ai[0] == npc.whoAmI)) //my crystal leaves
                        {
                            p.ai[1] = 1;
                            p.ai[2] = player.whoAmI;
                            p.netUpdate = true;
                        }
                        CrystalRedirectTimer = 0;
                        npc.netUpdate = true;
                    }
                    else
                    {
                        CrystalRedirectTimer++;
                        npc.netUpdate = true;
                    }

                }
                if (RingTossTimer.IsWithinBounds(360 - 60, 360 + 60) && CrystalRedirectTimer == 0 && !EnteredPhase2) // For 2 seconds after doing redirect
                {
                    npc.velocity *= 0.96f;
                    npc.localAI[1] = 0; // Don't fire vanilla projectiles
                }
            }

            if (npc.life > npc.lifeMax / 2)
            {
                if (--DicerTimer < 0)
                {
                    DicerTimer = 150 * 4 + 25;
                    if (WorldSavingSystem.MasochistModeReal && npc.HasValidTarget && FargoSoulsUtil.HostCheck)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), Main.player[npc.target].Center, Vector2.Zero, ModContent.ProjectileType<DicerPlantera>(), npc.defDamage / 4, 0f, Main.myPlayer, 0, 0);
                        for (int i = 0; i < 3; i++)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromThis(), Main.player[npc.target].Center, 30f * npc.SafeDirectionTo(Main.player[npc.target].Center).RotatedBy(2 * (float)Math.PI / 3 * i),
                              ModContent.ProjectileType<DicerPlantera>(), npc.defDamage / 4, 0f, Main.myPlayer, 1, 1);
                        }
                    }
                }
            }
            else
            {
                if (!InPhase2)
                {
                    InPhase2 = true;
                    DicerTimer = 0;
                }

                void SpawnOuterLeafRing()
                {
                    int max = WorldSavingSystem.MasochistModeReal ? 12 : 9;
                    const float distance = 250;
                    float rotation = 2f * (float)Math.PI / max;
                    for (int i = 0; i < max; i++)
                    {
                        Vector2 spawnPos = npc.Center + new Vector2(distance, 0f).RotatedBy(rotation * i);
                        FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), spawnPos, ModContent.NPCType<CrystalLeaf>(), 0, npc.whoAmI, distance, 0, rotation * i);
                    }
                }
                if (!EnteredPhase2)
                {
                    EnteredPhase2 = true;

                    if (FargoSoulsUtil.HostCheck)
                    {
                        if (!Main.npc.Any(n => n.active && n.type == ModContent.NPCType<CrystalLeaf>() && n.ai[0] == npc.whoAmI && n.ai[1] == innerRingDistance))
                        {
                            const int innerMax = 5;
                            float innerRotation = 2f * (float)Math.PI / innerMax;
                            for (int i = 0; i < innerMax; i++)
                            {
                                Vector2 spawnPos = npc.Center + new Vector2(innerRingDistance, 0f).RotatedBy(innerRotation * i);
                                FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), spawnPos, ModContent.NPCType<CrystalLeaf>(), 0, npc.whoAmI, innerRingDistance, 0, innerRotation * i);
                            }
                        }

                        SpawnOuterLeafRing();
                    }
                    DespawnProjs();
                }

                //explode time * explode repetitions + spread delay * propagations
                const int delayForDicers = 150 * 4 + 25 * 8;

                if (--DicerTimer < -120)
                {
                    DicerTimer = delayForDicers + delayForRingToss + 240;
                    //Counter3 = delayForDicers + 120; //extra compensation for the toss offset

                    npc.netUpdate = true;
                    NetSync(npc);

                    if (FargoSoulsUtil.HostCheck)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DicerPlantera>(), npc.defDamage / 4, 0f, Main.myPlayer);
                        for (int i = 0; i < 3; i++)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, 25f * npc.SafeDirectionTo(Main.player[npc.target].Center).RotatedBy(2 * (float)Math.PI / 3 * i),
                              ModContent.ProjectileType<DicerPlantera>(), npc.defDamage / 4, 0f, Main.myPlayer, 1, 8);
                        }
                    }
                }

                if (DicerTimer > delayForDicers || DicerTimer < 0)
                {
                    if (RingTossTimer > 120) //to still respawn the leaf ring if it's missing but disable throwing it
                        RingTossTimer = 120;
                }
                else if (DicerTimer < delayForDicers)
                {
                    RingTossTimer -= 1;

                    if (RingTossTimer % 2 == 0) //make sure plantera can get the timing for its check
                        RingTossTimer--;
                }
                else if (DicerTimer == delayForDicers)
                {
                    RingTossTimer = 121; //activate it immediately as the mines fade
                }

                IsVenomEnraged = npc.HasPlayerTarget && Main.player[npc.target].venom;

                if (--TentacleTimer <= 0)
                {
                    float slowdown = Math.Min(0.9f, -TentacleTimer / 60f);
                    //if (WorldSavingSystem.MasochistModeReal && slowdown > 0.75f)
                    //    slowdown = 0.75f;
                    npc.position -= npc.velocity * slowdown;

                    if (TentacleTimer == 0)
                    {
                        TentacleAttackAngleOffset = Main.rand.NextFloat(MathHelper.TwoPi);

                        SoundEngine.PlaySound(SoundID.Roar, npc.Center);

                        npc.netUpdate = true;
                        NetSync(npc);

                        foreach (NPC n in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<CrystalLeaf>() && n.ai[0] == npc.whoAmI && n.ai[1] > innerRingDistance)) //my crystal leaves
                        {
                            SoundEngine.PlaySound(SoundID.Grass, n.Center);

                            n.life = 0;
                            n.HitEffect();
                            n.checkDead();
                            n.active = false;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n.whoAmI);
                        }
                    }

                    const int maxTime = 30;
                    const int interval = 3;
                    float maxDegreeCoverage = 45f; //on either side of the middle, the full coverage of one side is x2 this
                    if (TentacleTimer >= -maxTime && TentacleTimer % interval == 0)
                    {
                        int tentacleSpawnOffset = Math.Abs(TentacleTimer) / interval;
                        for (int i = -tentacleSpawnOffset; i <= tentacleSpawnOffset; i += tentacleSpawnOffset * 2)
                        {
                            float attackAngle = MathHelper.WrapAngle(
                                TentacleAttackAngleOffset
                                + MathHelper.ToRadians(maxDegreeCoverage / (maxTime / interval)) * (i + Main.rand.NextFloat(-0.5f, 0.5f))
                            );

                            if (FargoSoulsUtil.HostCheck)
                            {
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Main.rand.NextVector2CircularEdge(24, 24),
                                    ModContent.ProjectileType<PlanteraTentacle>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, npc.whoAmI, attackAngle);
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Main.rand.NextVector2CircularEdge(24, 24),
                                    ModContent.ProjectileType<PlanteraTentacle>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, npc.whoAmI, attackAngle + MathHelper.Pi);
                            }

                            if (i == 0)
                                break;
                        }
                    }

                    if (TentacleTimer < -390)
                    {
                        TentacleTimer = 600 + Main.rand.Next(120);

                        if (!WorldSavingSystem.MasochistModeReal)
                            npc.velocity = Vector2.Zero;

                        npc.netUpdate = true;
                        NetSync(npc);

                        SpawnOuterLeafRing();
                    }
                }
                else
                {
                    npc.position -= npc.velocity * (IsVenomEnraged ? 0.1f : 0.2f);
                }

                //if (WorldSavingSystem.MasochistModeReal && --TentacleTimerMaso < 0)
                //{
                //    TentacleTimerMaso = 420;
                //    if (FargoSoulsUtil.HostCheck)
                //    {
                //        float angle = npc.SafeDirectionTo(Main.player[npc.target].Center).ToRotation();
                //        for (int i = -1; i <= 1; i++)
                //        {
                //            float offset = MathHelper.ToRadians(6) * i;
                //            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Main.rand.NextVector2CircularEdge(24, 24),
                //              ModContent.ProjectileType<PlanteraTentacle>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer, npc.whoAmI, angle + offset);
                //            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Main.rand.NextVector2CircularEdge(24, 24),
                //                ModContent.ProjectileType<PlanteraTentacle>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer, npc.whoAmI, -angle + offset);
                //        }
                //    }

                //}
            }

            EModeUtils.DropSummon(npc, "PlanterasFruit", NPC.downedPlantBoss, ref DroppedSummon);

            void DespawnProjs()
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].hostile &&
                        (Main.projectile[i].type == ProjectileID.ThornBall
                        || Main.projectile[i].type == ModContent.ProjectileType<DicerPlantera>()
                        || Main.projectile[i].type == ModContent.ProjectileType<PlanteraCrystalLeafRing>()
                        || Main.projectile[i].type == ModContent.ProjectileType<CrystalLeafShot>()))
                    {
                        Main.projectile[i].Kill();
                    }
                }
            }

            return result;
        }

        public override void SafePostAI(NPC npc)
        {
            base.SafePostAI(npc);

            npc.defense = Math.Max(npc.defense, npc.defDefense);
        }

        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            return !IsVenomEnraged ? base.GetAlpha(npc, drawColor) : new Color(255, drawColor.G / 2, drawColor.B / 2);
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            bool recolor = SoulConfig.Instance.BossRecolors && WorldSavingSystem.EternityMode;
            if (EnteredPhase3)
            {
                Vector2 drawPos = npc.Center - screenPos;

                Color glowColor = recolor ? Color.Blue : Color.Green;
                for (int j = 0; j < 12; j++)
                {
                    Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 1f;

                    spriteBatch.Draw(TextureAssets.Npc[npc.type].Value, drawPos + afterimageOffset, npc.frame, glowColor, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, SpriteEffects.None, 0f);
                }
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
        public static float DR(NPC npc) =>
            npc.GetLifePercent() < 0.25f ? 0.55f // phase 3
            : npc.GetLifePercent() < 0.5f ? 0.3f // phase 2
            : -0.1f; // phase 1
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 1 - DR(npc);
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.lifeRegen >= 0)
                return;
            float markiplier = 1 - DR(npc);
            npc.lifeRegen = (int)Math.Round(npc.lifeRegen * markiplier);
            damage = (int)Math.Round(damage * markiplier);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadBossHeadSprite(recolor, 11);
            LoadBossHeadSprite(recolor, 12);
            LoadGoreRange(recolor, 378, 391);
            LoadSpecial(recolor, ref TextureAssets.Chain26, ref FargowiltasSouls.TextureBuffer.Chain26, "Chain26");
            LoadSpecial(recolor, ref TextureAssets.Chain27, ref FargowiltasSouls.TextureBuffer.Chain27, "Chain27");
            LoadProjectile(recolor, ProjectileID.SeedPlantera);
            LoadProjectile(recolor, ProjectileID.PoisonSeedPlantera);
            LoadProjectile(recolor, ProjectileID.ThornBall);
        }
    }

    public class PlanterasHook : PlanteraPart
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.PlanterasHook);

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            npc.damage = 0;
            npc.defDamage = 0;

            NPC plantera = FargoSoulsUtil.NPCExists(NPC.plantBoss, NPCID.Plantera);
            if (plantera != null && plantera.life < plantera.lifeMax / 2 && plantera.HasValidTarget)
            {
                if (npc.Distance(Main.player[plantera.target].Center) > 600)
                {
                    Vector2 targetPos = Main.player[plantera.target].Center / 16; //pick a new target pos near player
                    targetPos.X += Main.rand.Next(-25, 26);
                    targetPos.Y += Main.rand.Next(-25, 26);

                    Tile tile = Framing.GetTileSafely((int)targetPos.X, (int)targetPos.Y);
                    npc.localAI[0] = 600; //reset vanilla timer for picking new block
                    if (FargoSoulsUtil.HostCheck)
                        npc.netUpdate = true;

                    npc.ai[0] = targetPos.X;
                    npc.ai[1] = targetPos.Y;
                }

                if (npc.Distance(new Vector2(npc.ai[0] * 16 + 8, npc.ai[1] * 16 + 8)) > 32)
                    npc.position += npc.velocity;
            }

            return result;
        }
    }

    public class PlanterasTentacle : PlanteraPart
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.PlanterasTentacle);

        public int ChangeDirectionTimer;
        public int RotationDirection;
        public int MaxDistanceFromPlantera;
        public int CanHitTimer;

        public bool DroppedSummon;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(ChangeDirectionTimer);
            binaryWriter.Write(RotationDirection);
            binaryWriter.Write(MaxDistanceFromPlantera);
            binaryWriter.Write(CanHitTimer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            ChangeDirectionTimer = binaryReader.ReadInt32();
            RotationDirection = binaryReader.ReadInt32();
            MaxDistanceFromPlantera = binaryReader.ReadInt32();
            CanHitTimer = binaryReader.ReadInt32();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            MaxDistanceFromPlantera = 200;
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int CooldownSlot)
        {
            return base.CanHitPlayer(npc, target, ref CooldownSlot) && CanHitTimer > 60;
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            NPC plantera = FargoSoulsUtil.NPCExists(NPC.plantBoss, NPCID.Plantera);
            if (plantera != null)
            {
                npc.position += plantera.velocity / 3;
                if (npc.Distance(plantera.Center) > MaxDistanceFromPlantera) //snap back in really fast if too far
                {
                    Vector2 vel = plantera.Center - npc.Center;
                    vel += MaxDistanceFromPlantera * plantera.DirectionFrom(npc.Center).RotatedBy(MathHelper.ToRadians(45) * RotationDirection);
                    npc.velocity = Vector2.Lerp(npc.velocity, vel / 15, 0.05f);
                }
            }

            if (++ChangeDirectionTimer > 120)
            {
                ChangeDirectionTimer = Main.rand.Next(30);
                if (FargoSoulsUtil.HostCheck)
                {
                    RotationDirection = Main.rand.NextBool() ? -1 : 1;
                    MaxDistanceFromPlantera = 50 + Main.rand.Next(150);
                    npc.netUpdate = true;
                    NetSync(npc);
                }
            }

            ++CanHitTimer;

            return result;
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
        }
    }

    public class Spore : PlanteraPart
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Spore);
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (NPC.AnyNPCs(NPCID.Plantera))
            {
                npc.life = 0;
                npc.checkDead();
                npc.active = false;
            }
        }
    }
}