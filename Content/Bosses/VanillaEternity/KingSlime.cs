using Fargowiltas.Content.NPCs;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Content.Items.Armor;
using FargowiltasSouls.Content.NPCs;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.BossMinions;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.KingSlime;
using FargowiltasSouls.Core;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class KingSlime : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.KingSlime);

        public bool DroppedSummon;

        public int DeathTimer = -1;

        public NPC NPC;
        public Player Target => Main.player[NPC.target];
        public ref float Cycle => ref NPC.ai[0];
        public ref float State => ref NPC.ai[1];
        public ref float Phase => ref NPC.ai[2];
        public ref float Timer => ref NPC.ai[3];
        public int TeleportCD;
        public int ExplosionCD;
        public float Stretch;
        public bool CanFall;
        public float[] CustomAI = new float[3];
        public float PhaseTwoHP => 0.65f;
        public bool PhaseTwo => NPC.GetLifePercent() < PhaseTwoHP;
        public float P2Visuals = 0;

        public enum States
        {
            Spawn,
            JumpCycle,
            Burst,
            Teleport,
            Explosion
        };
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write7BitEncodedInt(DeathTimer);
            binaryWriter.Write7BitEncodedInt(TeleportCD);
            binaryWriter.Write7BitEncodedInt(ExplosionCD);
            for (int i = 0; i < CustomAI.Length; i++)
            {
                binaryWriter.Write(CustomAI[i]);
            }
            
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            DeathTimer = binaryReader.Read7BitEncodedInt();
            TeleportCD = binaryReader.Read7BitEncodedInt();
            ExplosionCD = binaryReader.Read7BitEncodedInt();
            for (int i = 0; i < CustomAI.Length; i++)
            {
                CustomAI[i] = binaryReader.ReadSingle();
            }
        }
        public override bool SafePreAI(NPC npc)
        {
            NPC = npc;
            NPC.noGravity = false;

            if (DeathTimer >= 0)
            {
                Stretch = 0;
                int scale = 1;
                if (scale != npc.scale)
                {
                    npc.position.X += npc.width / 2;
                    npc.position.Y += npc.height;
                    npc.scale = scale;
                    npc.width = Math.Clamp((int)(98f * npc.scale), 32, short.MaxValue);
                    npc.height = Math.Clamp((int)(92f * npc.scale), 32, short.MaxValue);
                    npc.position.X -= npc.width / 2;
                    npc.position.Y -= npc.height;
                }

                DeathAnimation(npc);
                if (++DeathTimer >= 300)
                {
                    npc.life = 0;
                    npc.dontTakeDamage = false;
                    npc.checkDead();
                }
                return false;
            }


            EModeGlobalNPC.slimeBoss = npc.whoAmI;
            EModeUtils.DropSummon(npc, ItemID.SlimeCrown, NPC.downedSlimeKing, ref DroppedSummon);
            // defaults
            CanFall = true;

            // despawn
            int maxDist = 3000;
            if (Main.player[npc.target].dead || Vector2.Distance(npc.Center, Main.player[npc.target].Center) > maxDist)
            {
                npc.TargetClosest();
                if (Main.player[npc.target].dead || Vector2.Distance(npc.Center, Main.player[npc.target].Center) > maxDist)
                {
                    return true; // run vanilla despawn code
                }
            }
            
            // Scaling (from vanilla)
            float num255 = (float)npc.life / (float)npc.lifeMax;
            num255 = num255 * 0.5f + 0.75f;
            float num236 = 1f;
            float num237 = 1f;
            float num238 = 2f;
            if (Main.getGoodWorld)
            {
                num238 -= 1f - (float)npc.life / (float)npc.lifeMax;
                num237 *= num238;
            }
            num255 *= num236;
            num255 *= num237;
            if (num255 != npc.scale)
            {
                npc.position.X += npc.width / 2;
                npc.position.Y += npc.height;
                var minScale =  33 / 92f;
                if (num255 < minScale)
                    num255 = minScale;
                npc.scale = num255;
                npc.width = Math.Clamp((int)MathF.Round(98f * npc.scale), 32, short.MaxValue);
                npc.height = Math.Clamp((int)MathF.Round(92f * npc.scale), 32, short.MaxValue);
                npc.position.X -= npc.width / 2;
                npc.position.Y -= npc.height;
            }

            // Anti cheese
            npc.noTileCollide = false;
            if (Math.Abs(Target.Center.X - NPC.Center.X) < npc.width / 2 && Target.Center.Y > NPC.Center.Y && !Collision.CanHitLine(NPC.position, npc.width, npc.height, Target.position, Target.width, Target.height))
                npc.noTileCollide = true;


            int state = (int)State;
            switch ((States)state)
            {
                case States.Spawn:
                    {
                        if (NPC.velocity.Y == 0)
                        {
                            Timer++;
                            if (Timer > 40)
                            {
                                ResetToJumps();
                            }
                        }
                    }
                    break;
                case States.JumpCycle:
                    {
                        if (Cycle > 2)
                        {
                            Cycle = 0;
                            NPC.netUpdate = true;
                        }
                        if (Cycle < 2) // normal jump
                        {
                            NormalJump();
                        }
                        else if (Cycle == 2) // big jump
                        {
                            BigJump();
                        }
                    }
                    break;
                case States.Teleport:
                    Teleport();
                    break;
                case States.Explosion:
                    Explosion();
                    break;
            }
            if (npc.velocity.Y < 0)
                npc.noTileCollide = true;
            return false;
        }
        public bool JumpWindup()
        {
            ref float timer = ref CustomAI[0];
            if (timer >= 0)
            {
                CanFall = false;
                if (NPC.velocity.Y == 0)
                    timer++;
                else
                    timer = 0;
                float delay = WorldSavingSystem.MasochistModeReal ? 22 : 25;
                float progress = timer / delay;
                Stretch = FargoSoulsUtil.SineInOut(2 * progress);
                Stretch = MathF.Pow(Stretch, 0.5f);
                Stretch = MathHelper.Clamp(Stretch, 0, 1);

                NPC.position.Y += NPC.height;
                float yScale = NPC.scale - Stretch * 0.45f;
                NPC.height = (int)(92f * yScale);
                NPC.position.Y -= NPC.height;

                if (timer >= delay - 3)
                {
                    timer = -1;
                }
                return true;
            }
            Stretch = 0;
            return false;
        }
        public void NormalJump()
        {
            if (JumpWindup())
                return;
            float xDir = NPC.HorizontalDirectionTo(Target.Center);
            if (Timer == 0) // before jump
            {
                if (NPC.velocity.Y == 0) // grounded
                {
                    float x = MathF.Abs(Target.Center.X - NPC.Center.X) / 1200;
                    if (x > 1)
                        x = 1;
                    int side = 14;
                    float up = 27;
                    if (Cycle == 0)
                    {
                        side = 12;
                        up = 23;
                    }
                    if (PhaseTwo)
                    {
                        side += 2;
                        up += 1;
                    }
                    float yDiff = NPC.Center.Y - Target.Center.Y;
                    if (yDiff > 400)
                        up *= yDiff / 400f;
                    NPC.velocity.X = xDir * side * x;
                    NPC.velocity.Y = -up;
                    Timer = 1;
                    NPC.netUpdate = true;
                }
            }
            else if (Timer == 1) // during jump
            {
                float threshold = 400;
                float distanceFactor = MathF.Abs(Target.Center.X - NPC.Center.X) / threshold;
                float accel = 0.4f;
                if (NPC.velocity.X.NonZeroSign() != xDir || distanceFactor > 1)
                    NPC.velocity.X += xDir * accel;
                else
                    NPC.velocity.X += xDir * accel * distanceFactor;
                int cap = 14;
                if (Cycle == 0)
                    cap = 12;
                if (PhaseTwo)
                    cap += 3;
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -cap, cap);
                NPC.noGravity = NPC.velocity.Y > 0;
                if (NPC.velocity.Y == 0)
                {
                    NPC.velocity.X = 0;
                    Timer = 2;
                    NPC.netUpdate = true;
                    // spike burst
                    if (FargoSoulsUtil.HostCheck)
                    {
                        for (int j = -1; j < 2; j += 2)
                        {
                            int count = WorldSavingSystem.MasochistModeReal ? 3 : 2;
                            for (int i = 0; i < count; i++)
                            {
                                float xS = (float)(i + 1) / count;
                                Vector2 vel = new(j * 6f * xS, -11);
                                Vector2 rand = Main.rand.NextFloat(-2, 2) * Vector2.UnitX + Main.rand.NextFloat(-1, 1) * Vector2.UnitY;
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + Vector2.UnitX * j * Main.rand.NextFloat(0.7f, 1.3f) * NPC.width * NPC.scale / 4, vel + rand,
                                    ModContent.ProjectileType<KingSlimeBall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer);
                            }
                        }
                    }
                    
                }
                else
                {
                    if (Math.Abs(NPC.velocity.Y) < 3)
                        NPC.velocity.Y += 0.4f;
                    else if (NPC.velocity.Y < 18)
                        NPC.velocity.Y += 1;
                }
                    
            }
            else // after jump
            {
                Timer++;
                float delay = 25;
                if (WorldSavingSystem.MasochistModeReal)
                {
                    delay -= 10;
                }
                delay += NPC.GetLifePercent() * 25;
                if (Timer == 5)
                    CheckExplosion();
                if (Timer > delay)
                {
                    IncrementCycle();
                    return;
                }
            }
        }
        public void BigJump()
        {
            if (JumpWindup())
                return;
            float xDir = NPC.HorizontalDirectionTo(Target.Center);
            if (Timer == 0) // before jump
            {
                if (NPC.velocity.Y == 0) // grounded
                {
                    float x = MathF.Abs(Target.Center.X - NPC.Center.X) / 350;
                    x = MathHelper.Clamp(x, 0.2f, 1f);
                    NPC.velocity.X = xDir * 24 * x;
                    NPC.velocity.Y = -38;
                    Timer = 1;
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/VanillaEternity/KingSlime/KSJump"), NPC.Center);
                    NPC.netUpdate = true;
                }
            }
            else if (Timer >= 1) // during jump
            {
                float threshold = 200;
                float xDist = MathF.Abs(Target.Center.X - NPC.Center.X);
                if (Timer == 1 && (xDist < threshold || NPC.velocity.Y > 0))
                    Timer = 2;
                if (Timer > 1)
                {
                    Timer++;
                    NPC.velocity.X *= 0.96f;
                }
                    
                NPC.noGravity = NPC.velocity.Y > 0;
                if (NPC.velocity.Y == 0)
                {
                    NPC.velocity.X = 0;
                    Timer = -1;
                    NPC.netUpdate = true;

                    // spike burst
                    SoundEngine.PlaySound(SoundID.Item154, NPC.Center);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        for (int j = -1; j < 2; j += 2)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                int x = (int)(NPC.position.X + Main.rand.NextFloat(NPC.width - 32));
                                int y = (int)(NPC.Bottom.Y - Main.rand.NextFloat(30));
                                int type = ModContent.NPCType<SlimeSwarm>();
                                int slime = NPC.NewNPC(NPC.GetSource_FromThis(), x, y, type);
                                if (slime.IsWithinBounds(Main.maxNPCs))
                                {
                                    Main.npc[slime].SetDefaults(type);
                                    Main.npc[slime].velocity.Y = Main.rand.NextFloat(-8, -10) * 0.3f;

                                    Main.npc[slime].ai[0] = Math.Sign(j);
                                    Main.npc[slime].velocity.X = Main.rand.NextFloat(9, 12) * 0.4f * j;

                                    //Main.npc[slime].ai[0] = -1000 * Main.rand.Next(3);
                                    //Main.npc[slime].ai[1] = 0f;
                                    if (Main.netMode == NetmodeID.Server)
                                    {
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, slime);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    float gravity = Timer == 1 ? 0.5f : 1.5f;
                    if (NPC.velocity.Y < 18)
                        NPC.velocity.Y += gravity;
                }

            }
            else // after jump
            {
                Timer--;
                float delay = 25;
                if (WorldSavingSystem.MasochistModeReal)
                {
                    delay -= 5;
                }
                delay += NPC.GetLifePercent() * 25;
                if (Timer == -5)
                    CheckExplosion();
                if (Timer < -delay)
                {
                    IncrementCycle();
                    return;
                }
            }
        }

        public void Teleport()
        {
            ref float hasTeleported = ref CustomAI[0];
            ref float teleX = ref CustomAI[1];
            ref float teleY = ref CustomAI[2];
            int fadeTime = 45;
            if ((int)Timer < fadeTime) // pre teleport
            {
                if (Timer < fadeTime / 6)
                {
                    teleX = Target.Bottom.X + NPC.HorizontalDirectionTo(Target.Center) * 400;
                    teleY = Target.Bottom.Y;
                    bool InTerrain(float x, float y) => Collision.SolidCollision(new Vector2(x - NPC.width / 2, y - NPC.height), NPC.width, NPC.height);
                    for (int i = 0; i < 40; i++)
                    {
                        if (InTerrain(teleX, teleY))
                            teleY -= 16;
                        else
                            break;
                    }
                    if (InTerrain(teleX, teleY) || !Collision.CanHitLine(NPC.Center, 0, 0, Main.player[NPC.target].Center, 0, 0) || !Collision.SolidCollision(new Vector2(teleX, teleY) - Vector2.One * 300, 600, 600, true)) // fall back to player center
                    {
                        teleX = Target.Bottom.X;
                        teleY = Target.Bottom.Y;
                    }
                        
                }
                NPC.scale *= 1f - Timer / fadeTime;

                // dust at npc position
                for (int num249 = 0; num249 < 10; num249++)
                {
                    float x = teleX - NPC.width / 2;
                    float y = teleY - NPC.height;
                    int num250 = Dust.NewDust(new Vector2(x, y) + Vector2.UnitX * -20f, NPC.width + 40, NPC.height, DustID.TintableDust, NPC.velocity.X, NPC.velocity.Y, 150, new Color(78, 136, 255, 80), 2f);
                    Main.dust[num250].noGravity = true;
                    Dust dust = Main.dust[num250];
                    dust.velocity *= 0.5f;
                }
            }
            else // post teleport
            {
                if (hasTeleported == 0) // teleport frame
                {
                    hasTeleported = 1;
                    NPC.Bottom = new(teleX, teleY);
                    NPC.netUpdate = true;
                }
                float lerp = (Timer - fadeTime) / fadeTime;
                NPC.scale *= lerp;
                if (Timer > fadeTime * 2)
                {
                    TeleportCD = 2;
                    ResetToJumps();
                    return;
                }
            }

            NPC.position.X += NPC.width / 2;
            NPC.position.Y += NPC.height;
            NPC.width = Math.Clamp((int)(98f * NPC.scale), 32, short.MaxValue);
            NPC.height = Math.Clamp((int)(92f * NPC.scale), 32, short.MaxValue);
            NPC.position.X -= NPC.width / 2;
            NPC.position.Y -= NPC.height;

            // dust at npc position
            for (int num249 = 0; num249 < 10; num249++)
            {
                int num250 = Dust.NewDust(NPC.position + Vector2.UnitX * -20f, NPC.width + 40, NPC.height, DustID.TintableDust, NPC.velocity.X, NPC.velocity.Y, 150, new Color(78, 136, 255, 80), 2f);
                Main.dust[num250].noGravity = true;
                Dust dust = Main.dust[num250];
                dust.velocity = (dust.position - NPC.Center).SafeNormalize(Main.rand.NextVector2CircularEdge(1, 1)) * Main.rand.NextFloat(4, 7);
            }

            float speedScale = 2f - NPC.GetLifePercent();
            Timer += speedScale;
        }
        public void Explosion()
        {
            int telegraphTime = 70;
            int explosionTime = 70;
            if (WorldSavingSystem.MasochistModeReal)
            {
                telegraphTime /= 2;
                explosionTime /= 2;
            }
            if (Timer == 0)
            {
                Particle p = new ExpandingBloomParticle(NPC.Center, Vector2.Zero, Color.Blue, Vector2.One, Vector2.One * 60, telegraphTime, true, Color.Transparent);
                p.Spawn();
                SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/VanillaEternity/KingSlime/KSCharge"), NPC.Center);
            }
            if (Timer == telegraphTime)
            {
                ExplosionProjectiles();
            }
            if (Timer >= telegraphTime + explosionTime)
            {
                ResetToJumps();
                ExplosionCD = 2;
                return;
            }
            Timer++;
        }
        public void ExplosionProjectiles()
        {
            SoundEngine.PlaySound(SoundID.Item167, NPC.Center);
            int halfBlobs = 6;
            if (FargoSoulsUtil.HostCheck)
            {
                for (int i = -halfBlobs; i <= halfBlobs; i++)
                {
                    Vector2 dir = -Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * 0.6f * i / halfBlobs);
                    dir = dir.RotatedByRandom(MathHelper.PiOver2 * 0.07f);

                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + dir * Main.rand.NextFloat(10f, 40f), dir * Main.rand.NextFloat(11f, 18f), ModContent.ProjectileType<KingSlimeBall>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer);
                }
            }
        }
        public void ResetState()
        {
            Timer = 0;
            for (int i = 0; i < CustomAI.Length; i++)
                CustomAI[i] = 0;
            NPC.netUpdate = true;
        }
        public void ResetToJumps()
        {
            State = (int)States.JumpCycle;
            Cycle = 0;
            ResetState();
        }
        public void IncrementCycle()
        {
            ResetState();
            Cycle++;
            if (NPC.HasPlayerTarget)
            {
                bool shouldTP = false;
                if (PhaseTwo && TeleportCD <= 0 && Main.rand.NextBool(3))
                    shouldTP = true;
                if (!Collision.CanHitLine(NPC.Center, 0, 0, Main.player[NPC.target].Center, 0, 0) || Math.Abs(NPC.Top.Y - Main.player[NPC.target].Bottom.Y) > 400)
                    shouldTP = true;
                if (TeleportCD >= 4) // even when forced, not TOO frequent
                    shouldTP = false;

                if (shouldTP)
                {
                    State = (int)States.Teleport;
                }
                else if (TeleportCD > 0)
                {
                    TeleportCD--;
                }
            }
            NPC.netUpdate = true;
        }
        public void CheckExplosion()
        {
            if (WorldSavingSystem.MasochistModeReal)
            {
                if (Cycle == 2) // after big jump
                {
                    ResetState();
                    State = (int)States.Explosion;
                }
                return;
            }
            if (ExplosionCD > 0)
            {
                ExplosionCD--;
            }
            else if (Main.rand.NextBool(3))
            {
                ResetState();
                State = (int)States.Explosion;
            }
        }
        public override bool? CanFallThroughPlatforms(NPC npc)
        {
            if (!CanFall)
                return false;
            if (!npc.HasPlayerTarget)
                return base.CanFallThroughPlatforms(npc);
            return (Main.player[npc.target].Top.Y > npc.Bottom.Y + 30) ? true : null;
        }

        public override bool CheckDead(NPC npc)
        {
            if (DeathTimer != -1)
                return true;

            // Dont do the anim if mutant already exists
            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss.MutantBoss>())
                || (ModContent.TryFind("Fargowiltas", "Mutant", out ModNPC mutant) && NPC.AnyNPCs(mutant.Type)) || !SoulConfig.Instance.BossRecolors)
            {
                return true;
            }

            if (WorldSavingSystem.HaveForcedMutantFromKS)
                return true;

            npc.life = 1;
            npc.active = true;

            // remove normal crown gore (manually spawned later)
            foreach (Gore gore in Main.gore.Where(g => g.active && g.type == GoreID.KingSlimeCrown))
                gore.active = false;
            DeathTimer++;
            npc.dontTakeDamage = true;
            FargoSoulsUtil.ClearHostileProjectiles(2, npc.whoAmI);
            npc.netUpdate = true;

            return false;
        }

        public override void OnKill(NPC npc)
        {
            base.OnKill(npc);

            if (FargoSoulsUtil.HostCheck
                && !FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss.MutantBoss>())
                && ModContent.TryFind("Fargowiltas", "Mutant", out ModNPC mutant) && !NPC.AnyNPCs(mutant.Type) && !NPC.AnyNPCs(ModContent.NPCType<ReleasedMutant>()))
            {   
                
                // manual gore spawn
                Gore.NewGore(npc.GetSource_FromThis(), npc.Center, -15 * Vector2.UnitY, GoreID.KingSlimeCrown);

                int n = NPC.NewNPC(npc.GetSource_FromThis(), (int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<ReleasedMutant>());
                if (n != Main.maxNPCs && Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.Slimed, 60);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadBossHeadSprite(recolor, 7);
            LoadGore(recolor, 734);
            LoadExtra(recolor, 39);

            LoadSpecial(recolor, ref TextureAssets.Ninja, ref FargowiltasSouls.TextureBuffer.Ninja, "Ninja");
        }
        public float StretchX = 0;
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            NPC = npc;
            bool resprite = WorldSavingSystem.EternityMode && SoulConfig.Instance.BossRecolors;

            // Draw the Ninja (Mutant).
            var ninjaOffset = new Vector2(-npc.velocity.X * 2f, -npc.velocity.Y);
            ninjaOffset /= 3;
            var ninjaRotation = npc.velocity.X * 0.05f;
            ninjaRotation /= 2;
            
            switch (npc.frame.Y)
            {
                case 120:
                    ninjaOffset.Y += 2f;
                    break;

                case 360:
                    ninjaOffset.Y -= 2f;
                    break;

                case 480:
                    ninjaOffset.Y -= 6f;
                    break;
            }
            
            spriteBatch.Draw(TextureAssets.Ninja.Value, new Vector2(npc.position.X - screenPos.X + npc.width / 2f + ninjaOffset.X, npc.position.Y - screenPos.Y + npc.height / 2f + ninjaOffset.Y), new Rectangle(0, 0, TextureAssets.Ninja.Width(), TextureAssets.Ninja.Height()), drawColor, ninjaRotation, TextureAssets.Ninja.Size() / 2f, 1f, SpriteEffects.None, 0f);
            
            // We have to manually set drawing to immediate mode when rendering
            // the NPC normally.  Bestiary icons don't have this requirement.
            if (!npc.IsABestiaryIconDummy && resprite)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.Transform);
            }

            // Can also use npc.type instead.
            var ksTexture = TextureAssets.Npc[NPCID.KingSlime].Value;
            
            // Render KS through DrawData since we need to apply a game shader.
            // We also want to stretch him.
            var frameCount = Main.npcFrameCount[npc.type];
            var frameVertical = npc.frame.Y / npc.frame.Height;
            var frame = ksTexture.Frame(1, frameCount, 0, frameVertical);
            frame.Inflate(0, -2);

            Vector2 scale = Vector2.One * npc.scale;
            if (Stretch > 0)
                scale.Y *= 1 - Stretch * 0.55f;

            float xStretch = MathF.Pow(Math.Abs(npc.velocity.Y) / 20f, 0.5f);
            StretchX = MathHelper.Lerp(StretchX, xStretch, 0.3f);
            if (StretchX > 0)
                scale.X *= 1 - StretchX * 0.42f;
            
            var drawData = new DrawData(ksTexture, npc.Bottom - screenPos + new Vector2(0f, 2f), frame, drawColor /*with { A = 200 }*/, npc.rotation, frame.Size() * new Vector2(0.5f, 1f), scale, npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            if (resprite)
                GameShaders.Misc["FargowiltasSouls:KingSlime"].Apply(drawData);
            drawData.Draw(spriteBatch);
            if (resprite)
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            
            // Render the crown normally without the shader.
            var crownTexture = TextureAssets.Extra[39].Value;
            var center = npc.Center;

            var yOffset = (npc.frame.Y / (ksTexture.Height / Main.npcFrameCount[NPCID.KingSlime])) switch
            {
                0 => 2f,
                1 => -6f,
                2 => 2f,
                3 => 10f,
                4 => 2f,
                5 => 0f,
                _ => 0f,
            };

            var spriteEffects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            center.Y += npc.gfxOffY - (70f - yOffset) * scale.Y;

            float crownUp = 30 * StretchX;
            spriteBatch.UseBlendState(BlendState.Additive);
            if (PhaseTwo && P2Visuals < 1)
                P2Visuals += 0.025f;
            if (P2Visuals > 0)
            {
                for (int j = 0; j < 12; j++)
                {
                    Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 2f * P2Visuals * npc.scale;
                    Color color = Color.Red;

                    spriteBatch.Draw(crownTexture, center + afterimageOffset- screenPos - Vector2.UnitY * crownUp, null, color, 0f, crownTexture.Size() / 2f, 1f, spriteEffects, 0f);
                }
            }
            spriteBatch.ResetToDefault();
            
            spriteBatch.Draw(crownTexture, center - screenPos - Vector2.UnitY * crownUp, null, drawColor, 0f, crownTexture.Size() / 2f, 1f, spriteEffects, 0f);
            return false;
        }

        public void DeathAnimation(NPC npc)
        {
            
            Particle p;
            float scaleMult;
            int screenshake = 3;
            npc.velocity.X *= 0.9f;
            // prevent king slime from flinging himself into heaven
            npc.noTileCollide = false;
            npc.velocity.Y = npc.velocity.ClampLength(0, 35).Y;

            Vector2 mutantEyePos = npc.Center + new Vector2(-5f, -12f);         

            // stall death animation whilst in the air
            if (npc.velocity.Y != 0)
            {
                DeathTimer--;
               
            }

            // Dust
            if (Main.rand.NextBool(5))
            {          
                SoundEngine.PlaySound(npc.HitSound, npc.Center);
            }
            bool recolor = SoulConfig.Instance.BossRecolors && WorldSavingSystem.EternityMode;
            Dust.NewDust(npc.TopLeft, npc.width, npc.height, DustID.t_Slime);

            if (DeathTimer == 100 || DeathTimer == 200 || DeathTimer == 250)
            {
                screenshake += 2;
                FargoSoulsUtil.ScreenshakeRumble(screenshake);
                SoundEngine.PlaySound(FargosSoundRegistry.MutantSword with { Volume = 0.6f}, npc.Center);
            }

            // initial charge up
            if (DeathTimer >= 180 && DeathTimer < 270)
            {
                Vector2 pos = npc.Center + 5 * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                scaleMult = (DeathTimer - 180) / 23f;
                p = new SparkParticle(pos, Vector2.UnitX.RotatedBy((pos - npc.Center).ToRotation()), Color.Teal, scaleMult * 0.1f, 10);
                p.Spawn();
            }

            if (DeathTimer >= 270)
            {
                // eye glow
                scaleMult = (DeathTimer - 270) / 14f;
                p = new SparkParticle(mutantEyePos, Vector2.UnitY, Color.Teal, 1.5f, 120);
                p.Scale *= scaleMult;
                p.Spawn();
                p = new SparkParticle(mutantEyePos, Vector2.UnitX, Color.Teal, 1.5f, 120);
                p.Scale *= scaleMult;
                p.Spawn();

                // explosions
                if (DeathTimer % 5 == 0)
                {
                    if (FargoSoulsUtil.HostCheck) 
                    {
                        Vector2 spawnPos = npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
                        int type = ModContent.ProjectileType<MutantBombSmall>();
                        Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), spawnPos, Vector2.Zero, type, 0, 0f, Main.myPlayer);
                        proj.scale *= 0.43f * scaleMult;
                    }
                    SoundEngine.PlaySound(SoundID.Item14, npc.Center);
                    FargoSoulsUtil.ScreenshakeRumble((DeathTimer - 270) / 15f);
                }
            }
            // grand finale
            if (DeathTimer == 298)
            {
                FargoSoulsUtil.ScreenshakeRumble(7f);
                SoundEngine.PlaySound(FargosSoundRegistry.MutantKSKill, npc.Center);
            }
        }
    }
}
