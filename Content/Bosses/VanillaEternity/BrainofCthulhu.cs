using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.BossMinions;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.BrainOfCthulhu;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.MechanicalBosses;
using FargowiltasSouls.Core;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public class BrainofCthulhu : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.BrainofCthulhu);

        public int ConfusionTimer;
        public int ConfusionIdleTimer;
        public int IllusionTimer;
        public int ForceDespawnTimer;

        public bool EnteredPhase2;

        public bool DroppedSummon;

        public int ClonefadeDashTimer;
        public float CloneFade = 0f;
        public bool ManuallyDrawing;
        public bool KnockbackImmune;

        public float GlowOpacity = 0f;

        public const int ConfusionIdleTime = 60;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(ConfusionTimer);
            binaryWriter.Write(ConfusionIdleTimer);
            binaryWriter.Write(IllusionTimer);
            binaryWriter.Write(ClonefadeDashTimer);
            binaryWriter.Write(CloneFade);
            bitWriter.WriteBit(EnteredPhase2);
            bitWriter.WriteBit(KnockbackImmune);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            ConfusionTimer = binaryReader.ReadInt32();
            ConfusionIdleTimer = binaryReader.ReadInt32();
            IllusionTimer = binaryReader.ReadInt32();
            ClonefadeDashTimer = binaryReader.ReadInt32();
            CloneFade = binaryReader.ReadSingle();
            EnteredPhase2 = bitReader.ReadBit();
            KnockbackImmune = bitReader.ReadBit();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.25);
            npc.scale += 0.25f;
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Ichor] = true;
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int CooldownSlot)
        {
            return npc.alpha == 0;
        }

        public override bool SafePreAI(NPC npc)
        {
            EModeGlobalNPC.brainBoss = npc.whoAmI;

            if (Main.LocalPlayer.active && Main.LocalPlayer.Eternity().ShorterDebuffsTimer < 2)
                Main.LocalPlayer.Eternity().ShorterDebuffsTimer = 2;

            if (!npc.HasValidTarget || npc.Distance(Main.player[npc.target].Center) > 3000)
            {
                if (++ForceDespawnTimer > 60)
                {
                    npc.velocity.Y += 0.75f;
                    if (npc.timeLeft > 60)
                        npc.timeLeft = 60;
                }
            }
            else
            {
                ForceDespawnTimer = 0;
            }

            if (npc.alpha > 0 && (npc.ai[0] == 2 || npc.ai[0] == -3) && npc.HasValidTarget) //stay at a minimum distance
            {
                const float safeRange = 360;
                /*Vector2 stayAwayFromHere = Main.player[npc.target].Center + Main.player[npc.target].velocity * 30f;
                if (npc.Distance(stayAwayFromHere) < safeRange)
                    npc.Center = stayAwayFromHere + npc.DirectionFrom(stayAwayFromHere) * safeRange;*/
                Vector2 stayAwayFromHere = Main.player[npc.target].Center;
                if (npc.Distance(stayAwayFromHere) < safeRange)
                    npc.Center = stayAwayFromHere + npc.DirectionFrom(stayAwayFromHere) * safeRange;
            }

            if (EnteredPhase2)
            {
                int confusionThreshold = 400;
                int confusionThreshold2 = confusionThreshold - 60;
                // Fade dash
                float cloneTime = WorldSavingSystem.MasochistModeReal ? 50 : 50;
                int dashTime = 60;
                ref float teleportTimer = ref npc.localAI[1];
                bool confused = npc.HasPlayerTarget && Main.player[npc.target].HasBuff(BuffID.Confused);
                bool noFadeDash = ConfusionTimer.IsWithinBounds(confusionThreshold2 - 60, confusionThreshold2);
                //if (npc.life > npc.lifeMax / 2 && !WorldSavingSystem.MasochistModeReal && !(npc.HasPlayerTarget && !Main.player[npc.target].HasBuff(BuffID.Confused)))
                //    noFadeDash = false;
                if (teleportTimer >= cloneTime - 25 && !noFadeDash)
                {
                    if (CloneFade == 0 && npc.HasPlayerTarget)
                    {
                        if (FargoSoulsUtil.HostCheck)
                        {
                            Player player = Main.player[npc.target];
                            if (WorldSavingSystem.MasochistModeReal)
                                FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), player.Center + (player.Center - npc.Center), ModContent.NPCType<BrainClone>(), npc.whoAmI);
                        }
                    }
                    if (CloneFade < 1)
                        CloneFade += 0.05f;
                }
                else
                {
                    CloneFade = 0;
                }
                if (teleportTimer >= cloneTime && teleportTimer <= 60 && !noFadeDash)
                {
                    if (!confused)
                    {
                        ConfusionTimer++;
                    }
                    if (ClonefadeDashTimer < dashTime && npc.HasPlayerTarget)
                    {
                        Player player = Main.player[npc.target];
                        if (ClonefadeDashTimer == 0)
                        {
                            npc.netUpdate = true;
                        }


                        KnockbackImmune = true;
                        ClonefadeDashTimer++;
                        teleportTimer = cloneTime + 5;
                        npc.velocity += npc.DirectionTo(player.Center) * 0.28f;
                    }
                    else
                    {
                        teleportTimer = 60;
                        npc.netUpdate = true;
                    }

                }
                if (teleportTimer < cloneTime)
                {
                    const float safeRange = 360;
                    Vector2 stayAwayFromHere = Main.player[npc.target].Center;
                    if (npc.Distance(stayAwayFromHere) < safeRange)
                        npc.Center = stayAwayFromHere + npc.DirectionFrom(stayAwayFromHere) * safeRange;

                    ClonefadeDashTimer = 0;
                    KnockbackImmune = false;
                }


                //debuff cleanse when tp'ing
                if (npc.alpha > 0 && npc.buffType[0] != 0)
                {
                    npc.DelBuff(0);
                }

                void TelegraphConfusion(Vector2 spawn)
                {
                    Projectile.NewProjectile(npc.GetSource_FromThis(), spawn, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 8, 180);
                    Projectile.NewProjectile(npc.GetSource_FromThis(), spawn, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 8, 200);
                    Projectile.NewProjectile(npc.GetSource_FromThis(), spawn, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 8, 220);
                }
                ;

                void LaserSpread(Vector2 spawn)
                {
                    if (npc.life > npc.lifeMax / 2 && !WorldSavingSystem.MasochistModeReal)
                        return;

                    if (npc.HasValidTarget && FargoSoulsUtil.HostCheck) //laser spreads from each illusion
                    {
                        int max = WorldSavingSystem.MasochistModeReal ? 7 : 3;
                        int degree = WorldSavingSystem.MasochistModeReal ? 2 : 3;
                        int laserDamage = FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 4f / 3);

                        Projectile.NewProjectile(npc.GetSource_FromThis(), spawn, new Vector2(0, -4), ModContent.ProjectileType<BrainofConfusion>(), 0, 0, Main.myPlayer);
                        for (int i = -max; i <= max; i++)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), spawn, 0.2f * Main.player[npc.target].DirectionFrom(spawn).RotatedBy(MathHelper.ToRadians(degree) * i), ModContent.ProjectileType<DestroyerLaser>(), laserDamage, 0f, Main.myPlayer);
                    }
                }
                ;

                if (--ConfusionTimer < 0)
                {
                    ConfusionTimer = confusionThreshold;

                    if (!Main.player[npc.target].HasBuff(BuffID.Confused))
                    {
                        SoundEngine.PlaySound(SoundID.Roar, npc.Center);

                        Vector2 offset = npc.Center - Main.player[npc.target].Center;
                        Vector2 spawnPos = Main.player[npc.target].Center;

                        TelegraphConfusion(new Vector2(spawnPos.X + offset.X, spawnPos.Y + offset.Y));
                        TelegraphConfusion(new Vector2(spawnPos.X + offset.X, spawnPos.Y - offset.Y));
                        TelegraphConfusion(new Vector2(spawnPos.X - offset.X, spawnPos.Y + offset.Y));
                        TelegraphConfusion(new Vector2(spawnPos.X - offset.X, spawnPos.Y - offset.Y));
                    }

                    npc.netUpdate = true;
                    NetSync(npc);
                }
                else if (ConfusionTimer > confusionThreshold2) // after telegraph
                {
                    KnockbackImmune = false;
                    // no teleporting
                    teleportTimer = 2;
                    if (!(npc.life > npc.lifeMax / 2 && !WorldSavingSystem.MasochistModeReal))
                    {
                        ClonefadeDashTimer = 0;
                        CloneFade = 0;
                    }

                    bool isConfused = Main.player[npc.target].HasBuff(BuffID.Confused);
                    bool shouldMove = ConfusionTimer == confusionThreshold2 + 1 ? confused : !confused;

                    if (shouldMove)
                    {
                        if (npc.HasPlayerTarget)
                        {
                            Player player = Main.player[npc.target];
                            Vector2 desiredPos = player.Center;
                            Vector2 toNPC = npc.Center - desiredPos;
                            desiredPos += Vector2.UnitX * MathF.Sign(toNPC.X) * 300f + Vector2.UnitY * MathF.Sign(toNPC.Y) * 300f;
                            npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(desiredPos) * Math.Min(10, npc.Distance(desiredPos)), 0.2f);
                            KnockbackImmune = true;
                        }
                    }
                    void TelegraphCircle()
                    {
                        if (FargoSoulsUtil.HostCheck)
                        {
                            float size = 20f + 180f * (ConfusionTimer - confusionThreshold2) / (confusionThreshold - confusionThreshold2);
                            foreach (Player p in Main.player.Where(p => p.Alive()))
                                Projectile.NewProjectile(npc.GetSource_FromThis(), p.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 15, size);
                        }
                    }
                    if (ConfusionTimer % 15 == 0 && !WorldSavingSystem.MasochistModeReal)
                        if (!Main.dedServ)
                        {
                            TelegraphCircle();
                            SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/ReticleBeep"), Main.LocalPlayer.Center);
                        }


                    if (ConfusionTimer == confusionThreshold2 + 2)
                    {
                        if (!WorldSavingSystem.MasochistModeReal)
                        {
                            ConfusionIdleTimer = ConfusionIdleTime;
                            npc.netUpdate = true;
                            if (!Main.dedServ)
                            {
                                TelegraphCircle();
                                SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/ReticleBeep") with { Pitch = -0.5f }, Main.LocalPlayer.Center);
                            }
                        }
                        if (npc.Distance(Main.LocalPlayer.Center) < 3000) //inflict confusion
                        {
                            if (!Main.LocalPlayer.HasBuff(BuffID.Confused))
                            {
                                int idle = WorldSavingSystem.MasochistModeReal ? 0 : ConfusionIdleTime;
                                FargoSoulsUtil.AddDebuffFixedDuration(Main.LocalPlayer, BuffID.Confused, confusionThreshold + 10 + idle, false);
                            }
                            else
                                Main.LocalPlayer.ClearBuff(BuffID.Confused);
                        }
                    }

                    if (ConfusionTimer == confusionThreshold2 + 1)
                    {
                        if (ConfusionIdleTimer > 0)
                        {
                            ConfusionIdleTimer--;
                            ConfusionTimer++; // stall
                        }
                    }
                }
                else if (ConfusionTimer == confusionThreshold2)
                {
                    //npc.netUpdate = true; //disabled because might be causing mp issues???
                    //NetSync(npc);

                    if (!Main.player[npc.target].HasBuff(BuffID.Confused))
                    {
                        SoundEngine.PlaySound(SoundID.ForceRoarPitched, npc.Center);
                        TelegraphConfusion(npc.Center);

                        IllusionTimer = 120 + 90;

                        if (FargoSoulsUtil.HostCheck)
                        {
                            int type = ModContent.ProjectileType<BrainIllusionProj>(); //make illusions attack
                            int alpha = (int)(255f * npc.life / npc.lifeMax);

                            void SpawnClone(Vector2 center)
                            {
                                int n = NPC.NewNPC(npc.GetSource_FromAI(), (int)center.X, (int)center.Y, ModContent.NPCType<BrainIllusionAttack>(), npc.whoAmI, npc.whoAmI, alpha);
                                if (n != Main.maxNPCs)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                            }

                            foreach (Projectile p in Main.projectile.Where(p => p.active && p.type == type && p.ai[0] == npc.whoAmI && p.ai[1] == 0f))
                            {
                                if (p.Distance(Main.player[npc.target].Center) < 1000)
                                {
                                    //p.ai[1] = 1f;
                                    //p.netUpdate = true;

                                    SpawnClone(p.Center);
                                }
                                p.Kill();
                            }

                            Vector2 offset = npc.Center - Main.player[npc.target].Center;
                            Vector2 spawnPos = Main.player[npc.target].Center;

                            SpawnClone(new Vector2(spawnPos.X + offset.X, spawnPos.Y + offset.Y));
                            SpawnClone(new Vector2(spawnPos.X + offset.X, spawnPos.Y - offset.Y));
                            SpawnClone(new Vector2(spawnPos.X - offset.X, spawnPos.Y + offset.Y));
                            SpawnClone(new Vector2(spawnPos.X - offset.X, spawnPos.Y - offset.Y));
                        }
                    }
                    else
                    {
                        Vector2 offset = npc.Center - Main.player[npc.target].Center;
                        Vector2 spawnPos = Main.player[npc.target].Center;

                        LaserSpread(new Vector2(spawnPos.X + offset.X, spawnPos.Y + offset.Y));
                        LaserSpread(new Vector2(spawnPos.X + offset.X, spawnPos.Y - offset.Y));
                        LaserSpread(new Vector2(spawnPos.X - offset.X, spawnPos.Y + offset.Y));
                        LaserSpread(new Vector2(spawnPos.X - offset.X, spawnPos.Y - offset.Y));
                    }
                }

                if (--IllusionTimer < 0) //spawn illusions
                {
                    IllusionTimer = Main.rand.Next(5, 11);
                    if (npc.life > npc.lifeMax / 2)
                        IllusionTimer += 5;
                    if (npc.life < npc.lifeMax / 10)
                        IllusionTimer -= 2;
                    if (WorldSavingSystem.MasochistModeReal)
                        IllusionTimer -= 2;
                    npc.netUpdate = true;

                    if (FargoSoulsUtil.HostCheck)
                    {
                        Vector2 spawn = Main.player[npc.target].Center + Main.rand.NextVector2CircularEdge(1200f, 1200f);
                        Vector2 speed = Main.player[npc.target].Center + Main.player[npc.target].velocity * 45f + Main.rand.NextVector2Circular(-600f, 600f) - spawn;
                        speed = Vector2.Normalize(speed) * Main.rand.NextFloat(12f, 48f);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), spawn, speed, ModContent.ProjectileType<BrainIllusionProj>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 4f / 3), 0f, Main.myPlayer, npc.whoAmI);
                    }
                }

                if (IllusionTimer > 60)
                {
                    if (npc.ai[0] == -1f && npc.localAI[1] < 80) //force a tp
                    {
                        npc.localAI[1] = 80f;
                    }
                    if (npc.ai[0] == -3f && npc.ai[3] > 200) //stay invis
                    {
                        npc.dontTakeDamage = true;
                        npc.ai[0] = -3f;
                        npc.ai[3] = 255;
                        npc.alpha = 255;
                        return false;
                    }
                }
                if (IllusionTimer == 60)
                {
                    npc.localAI[1] = 120;
                    npc.ai[0] = -1;
                }
            }
            else if (!npc.dontTakeDamage)
            {
                EnteredPhase2 = true;

                if (FargoSoulsUtil.HostCheck) //spawn illusions
                {
                    bool recolor = SoulConfig.Instance.BossRecolors && WorldSavingSystem.EternityMode;
                    int type = recolor ? ModContent.NPCType<BrainIllusion2>() : ModContent.NPCType<BrainIllusion>();

                    FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, type, npc.whoAmI, npc.whoAmI, -1, 1);
                    FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, type, npc.whoAmI, npc.whoAmI, 1, -1);
                    FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, type, npc.whoAmI, npc.whoAmI, 1, 1);

                    for (int i = 0; i < Main.maxProjectiles; i++) //clear old golden showers
                    {
                        if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<GoldenShowerHoming>())
                            Main.projectile[i].Kill();
                    }
                }
            }

            EModeUtils.DropSummon(npc, ItemID.BloodySpine, NPC.downedBoss2, ref DroppedSummon);

            npc.defense = 0;
            npc.defDefense = 0;

            return base.SafePreAI(npc);
        }
        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            if (!ManuallyDrawing)
                drawColor *= (1 - CloneFade);
            drawColor *= npc.Opacity;
            return drawColor;
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (CloneFade > 0)
            {
                Asset<Texture2D> texture = TextureAssets.Npc[npc.type];
                ManuallyDrawing = true;
                Color color = npc.GetAlpha(drawColor);
                SpriteEffects effects = npc.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                Vector2 halfSize = new(texture.Width() / 2, texture.Height() / Main.npcFrameCount[npc.type] / 2);
                float num35 = 50f * npc.scale;
                float num36 = Main.NPCAddHeight(npc);

                Vector2 drawPos = new(npc.position.X - screenPos.X + (float)(npc.width / 2) - (float)TextureAssets.Npc[npc.type].Width() * npc.scale / 2f + halfSize.X * npc.scale,
                    npc.position.Y - screenPos.Y + (float)npc.height - (float)texture.Height() * npc.scale / (float)Main.npcFrameCount[npc.type] + 4f + halfSize.Y * npc.scale + num36 + num35 + npc.gfxOffY);

                // glow
                Main.spriteBatch.UseBlendState(BlendState.Additive);
                if (GlowOpacity < 1)
                    GlowOpacity += 0.1f;
                Color glowColor = npc.GetAlpha(Color.DarkRed) * GlowOpacity;
                for (int i = 0; i < 12; i++)
                {
                    Vector2 afterimageOffset = (MathHelper.TwoPi * i / 12f).ToRotationVector2() * 6f;

                    spriteBatch.Draw(texture.Value, drawPos + afterimageOffset, npc.frame, glowColor, npc.rotation, halfSize, npc.scale, effects, 0f);
                }
                Main.spriteBatch.ResetToDefault();
                spriteBatch.Draw(texture.Value, drawPos, npc.frame, color, npc.rotation, halfSize, npc.scale, effects, 0f);
                ManuallyDrawing = false;
                //Main.EntitySpriteDraw(texture, npc.Center - screenPos + new Vector2(0f, npc.gfxOffY + Main.NPCAddHeight(npc)), npc.frame, color, npc.rotation, npc.frame.Size() / 2, npc.scale, effects, 0);
                return true;
            }
            else
            {
                GlowOpacity = 0;
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (npc.life > 0)
            {
                float resist = MathHelper.Lerp(1f, 0.3f, 1f - Math.Max(0.12f, (float)Math.Sqrt((double)npc.life / npc.lifeMax)));
                modifiers.FinalDamage *= resist;
            }


            if (KnockbackImmune)
            {
                modifiers.DisableKnockback();
            }


            base.ModifyIncomingHit(npc, ref modifiers);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            return;
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
            LoadBossHeadSprite(recolor, 23);
            LoadGoreRange(recolor, 392, 402);
        }
    }

    public class Creeper : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Creeper);

        public int IchorAttackTimer;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(IchorAttackTimer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            IchorAttackTimer = binaryReader.ReadInt32();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.2);

            IchorAttackTimer = Main.rand.Next(60 * NPC.CountNPCS(NPCID.Creeper)) + Main.rand.Next(61) + 60;
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Ichor] = true;
        }

        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            if (--IchorAttackTimer < 0)
            {
                IchorAttackTimer = 60 * NPC.CountNPCS(NPCID.Creeper) - 30;
                if (IchorAttackTimer >= 60)
                    IchorAttackTimer += Main.rand.Next(-30, 31);

                if (npc.HasPlayerTarget && FargoSoulsUtil.HostCheck)
                {
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, 10f * npc.DirectionFrom(Main.player[npc.target].Center).RotatedByRandom(Math.PI),
                        ModContent.ProjectileType<GoldenShowerHoming>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, npc.target, -60f);
                }

                npc.netUpdate = true;
                NetSync(npc);
            }

            if (IchorAttackTimer % 60 == 0) //update timer periodically for if player suddenly kills a lot of creepers at once
            {
                IchorAttackTimer = Math.Min(IchorAttackTimer, 60 * NPC.CountNPCS(npc.type));
            }

            return result;
        }
        public override void SafeModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.penetrate > 1 || projectile.penetrate < -1)
                modifiers.FinalDamage *= 0.75f;
        }
        public override void SafeModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0.75f;
        }
        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Ichor, 60 * 3 + 30);
        }

        public override bool CheckDead(NPC npc)
        {
            if (npc.DeathSound != null)
                SoundEngine.PlaySound(npc.DeathSound.Value, npc.Center);
            npc.active = false;
            return false;
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
        }
    }
}
