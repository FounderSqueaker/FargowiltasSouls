using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Jungle;
using FargowiltasSouls.Content.Projectiles.Weapons.Minions;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Steamworks;
using System;
using System.IO;
using System.Runtime.Intrinsics.X86;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Jungle
{
    public class Snatchers : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.Snatcher,
            NPCID.ManEater,
            NPCID.AngryTrapper
        );

        public int BiteTimer;
        public int BittenPlayer = -1;
        public int ItemHeld = -1;

        float JawOpenness;
        int AttackTimer;
        Vector2 OriginPoint;
        //im sorry
        Vector2[] IdleVinePoints = [
            new(0, 0),
            new(0, -5),
            new(1, -6),
            new(2.5f, -7),
            new(4, -8.5f),
            new(5, -10),
            new(5.5f, -12),
            new(5.5f, -14),
            new(5, -16),
            new(4, -17),
            new(2, -17.5f),
            new(1, -17.5f),
            new(0, -17),
            new(-1, -16),
            ];
        Vector2[] IdleVinePointsReal;
        float sizeFactor = 10;
        float VineSideSwitcherTimer = 0;
        float VineSide = -1;
        public AttackStates AttackState = 0;
        public EatingStates EatingState = EatingStates.None;
        public int EatTimer = 0;

        public enum AttackStates
        {
            Hiding = 0,
            Launching,
            Idle,
            Snapping,
            BigSnapping,
            Grabbing
        }
        public enum EatingStates
        {
            None = 0,
            Grabbing,
            Eating,
            Digesting,
            Vomiting
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(BiteTimer);
            binaryWriter.Write(BittenPlayer);
            binaryWriter.Write(ItemHeld);
            binaryWriter.Write((int)EatingState);
            binaryWriter.Write(EatTimer);

            binaryWriter.Write(AttackTimer);
            binaryWriter.Write((int)AttackState);
            binaryWriter.Write(VineSide);
            binaryWriter.WriteVector2(OriginPoint);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            BiteTimer = binaryReader.ReadInt32();
            BittenPlayer = binaryReader.ReadInt32();
            ItemHeld = binaryReader.ReadInt32();
            EatingState = (EatingStates)binaryReader.ReadInt32();
            EatTimer = binaryReader.ReadInt32();

            AttackTimer = binaryReader.ReadInt32();
            AttackState = (AttackStates)binaryReader.ReadInt32();
            VineSide = binaryReader.ReadInt32();
            OriginPoint = binaryReader.ReadVector2();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.damage = (int)(2.0 / 3.0 * npc.damage);
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);
            npc.Center += new Vector2(0, 8);
            IdleVinePointsReal = (Vector2[])IdleVinePoints.Clone();
            OriginPoint = npc.Center;
            if (npc.type == NPCID.Snatcher) sizeFactor = 5;
            if (npc.type == NPCID.AngryTrapper) sizeFactor = 15;
            for (int i = 0; i < IdleVinePointsReal.Length; i++)
            {
                IdleVinePointsReal[i] *= sizeFactor;
                IdleVinePointsReal[i] += npc.Center;
            }
            npc.buffImmune[BuffID.Poisoned] = true;
            npc.buffImmune[BuffID.Venom] = true;
        }

        public override bool SafePreAI(NPC npc)
        {
            if (!npc.HasValidTarget)
            {
                npc.TargetClosest(false);
            }
            bool eating = true;
            switch (EatingState)
            {
                case EatingStates.None:
                    if (BittenPlayer == -1)
                        SearchForFood(npc);
                    eating = false;
                    break;
                case EatingStates.Grabbing:
                    ApproachFood(npc);
                    
                    break;
                case EatingStates.Eating:
                    EatFood(npc);
                    break;
                case EatingStates.Digesting:
                case EatingStates.Vomiting:
                    SpitOutFruit(npc);
                    break;
            }
            
            if (VineSide == 1 && VineSideSwitcherTimer < 1)
            {
                VineSideSwitcherTimer += 0.05f;
            }
            if (VineSide == -1 && VineSideSwitcherTimer > 0)
            {
                VineSideSwitcherTimer -= 0.05f;
            }
            if (npc.HasValidTarget)
            {
                if (VineSide == 1 && npc.Center.X > Main.player[npc.target].Center.X)
                {
                    VineSide = -1;
                }
                if (VineSide == -1 && npc.Center.X < Main.player[npc.target].Center.X)
                {
                    VineSide = 1;
                }
            }
            for (int i = 0; i < IdleVinePointsReal.Length; i++)
            {
                IdleVinePointsReal[i] = Vector2.Lerp(IdleVinePoints[i], new Vector2(-IdleVinePoints[i].X, IdleVinePoints[i].Y), VineSideSwitcherTimer)*sizeFactor;
                IdleVinePointsReal[i] += OriginPoint;
            }
            npc.ai[0] = npc.Center.X/16;
            npc.ai[1] = npc.Center.Y / 16;
            if (eating)
            {
                return false;
            }
            switch (AttackState)
            {
                case AttackStates.Hiding:
                    Hiding(npc);
                    break;
                case AttackStates.Launching:
                    Launching(npc);
                    break;
                case AttackStates.Idle:
                    Idle(npc);
                    break;
                case AttackStates.Snapping:
                    Snapping(npc);
                    break;
                case AttackStates.BigSnapping:
                    BigSnapping(npc);
                    break;
                case AttackStates.Grabbing:
                    Grabbing(npc);
                    break;

            }
            if (BiteTimer < 0)
                BiteTimer++;
            if (BiteTimer > 0)
                BiteTimer--;
            npc.spriteDirection = 1;
            return false;
        }
        #region attacking Methods
        private void Grabbing(NPC npc)
        {
            AttackTimer++;
            npc.velocity = Vector2.Lerp(npc.velocity, new Vector2(0, -2), 0.03f);
            npc.rotation = new Vector2(0, -1).ToRotation() + MathF.PI;
            if (AttackTimer < 30)
            {
                float x = AttackTimer / 30f;
                JawOpenness = MathHelper.Lerp(0f, 0.8f, MathF.Sin((x * MathF.PI) / 2));
            }
            else if (AttackTimer < 40)
            {
                float x = (AttackTimer - 30) / 10f;
                JawOpenness = MathHelper.Lerp(0.8f, 0f, x * x * x * x * x);
            }
            if (AttackTimer >= 40)
            {
                SoundEngine.PlaySound(SoundID.Item17, npc.Center);
                npc.velocity = new Vector2(npc.velocity.X, 1);
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDustDirect(npc.Center, 1, 1, DustID.Blood);
                }
                AttackTimer = 0;
            }
            Player victim = Main.player[BittenPlayer];
            if (BiteTimer > 0 && victim.active && !victim.ghost && !victim.dead
                && (npc.Distance(victim.Center) < 160 || victim.whoAmI != Main.myPlayer)
                && victim.FargoSouls().MashCounter < 20)
            {
                victim.AddBuff(ModContent.BuffType<GrabbedBuff>(), 2);
                victim.velocity = Vector2.Zero;
                victim.Center = npc.Center - new Vector2(0, 20);
            }
            else
            {
                BittenPlayer = -1;
                BiteTimer = -90; //cooldown
            }
            if (BittenPlayer == -1)
            {
                
                AttackTimer = 0;
                AttackState = AttackStates.Idle;
                NetSync(npc);
            }

        }
        private void BigSnapping(NPC npc)
        {
            if (!npc.HasValidTarget)
            {
                AttackState = AttackStates.Idle;
                AttackTimer = 0;
            }
            Player target = Main.player[npc.target];
            AttackTimer++;
            if (AttackTimer < 60)
            {
                npc.rotation = npc.AngleTo(target.Center) - MathF.PI;
                JawOpenness = MathHelper.Lerp(0.1f, 0.8f, AttackTimer / 60f);
            }
            if (AttackTimer == 60)
            {
                npc.velocity = npc.DirectionTo(target.Center) * sizeFactor * 1.8f;
            }
            if (AttackTimer > 70 && AttackTimer <= 90)
            {
                float x = (AttackTimer - 70) / 20f;
                JawOpenness = MathHelper.Lerp(0.8f, -0.1f, x*x*x);
                npc.velocity *= 0.95f;
            }
            if (AttackTimer == 90)
            {
                SoundEngine.PlaySound(SoundID.Item17, npc.Center);
                AttackState = AttackStates.Idle;
                AttackTimer = 0;
            }
        }
        private void Snapping(NPC npc)
        {
            if (!npc.HasValidTarget)
            {
                AttackState = AttackStates.Idle;
                AttackTimer = 0;
            }
            Player target = Main.player[npc.target];
            AttackTimer++;
            if (AttackTimer < 30)
            {
                npc.rotation = npc.AngleTo(target.Center) + MathHelper.Pi;
                JawOpenness = MathHelper.Lerp(0, 0.8f, AttackTimer / 30f);
            }
            if (AttackTimer == 30)
            {
                npc.velocity = npc.DirectionTo(target.Center) * sizeFactor * 1.3f;
            }
            if (AttackTimer > 40 && AttackTimer <= 60)
            {
                float x = (AttackTimer - 40) / 20f;
                //Main.NewText(x);
                JawOpenness = MathHelper.Lerp(0.8f, -0.1f, x*x*x);
                npc.velocity *= 0.95f;
            }    
            if (AttackTimer == 60)
            {
                SoundEngine.PlaySound(SoundID.Item17, npc.Center);
                AttackState = AttackStates.Idle;
                AttackTimer = 0;
            }
        }
        private void Idle(NPC npc)
        {
            Vector2 targetpos = IdleVinePointsReal[IdleVinePointsReal.Length - 1];
            if (npc.HasValidTarget)
            {
                Player target = Main.player[npc.target];
                targetpos += targetpos.DirectionTo(target.Center) * 10;
                if (npc.Distance(targetpos) < 200)
                {
                    npc.rotation =  Utils.AngleLerp(npc.rotation, npc.AngleTo(target.Center) + MathHelper.Pi, 0.08f);
                }
            }
            float targetopenness = MathHelper.Lerp(0.05f, 0.1f, MathF.Sin(AttackTimer / 10f));
            JawOpenness = MathHelper.Lerp(JawOpenness, targetopenness, 0.08f);
            npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(targetpos) * npc.Distance(targetpos)/20, 0.05f);
            
            AttackTimer++;
            if (AttackTimer >= 150)
            {
                
                AttackState = Main.rand.NextBool(3) ? AttackStates.BigSnapping  : AttackStates.Snapping;
                AttackTimer = 0;
                NetSync(npc);
            }
        }
        private void Hiding(NPC npc)
        {
            npc.velocity = Vector2.Zero;
            JawOpenness = 0.8f;
            npc.rotation = MathHelper.PiOver2;
            if (npc.HasValidTarget)
            {
                Player target = Main.player[npc.target];
                if (target.Distance(npc.Center) < 300)
                {
                    AttackState = AttackStates.Launching;
                }
            }
        }
        private void Launching(NPC npc)
        {
            AttackTimer++;
            int timeToComplete = 30;
            int point = (int)MathHelper.Lerp(1, IdleVinePointsReal.Length - 1, (float)AttackTimer / timeToComplete);
            npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(IdleVinePointsReal[point]) * sizeFactor, 0.2f);
            npc.rotation = npc.velocity.ToRotation() + MathF.PI;
            float x = (float)AttackTimer / timeToComplete;
            JawOpenness = MathHelper.Lerp(0.8f, 0.1f, x );
            if (AttackTimer >= timeToComplete)
            {
                AttackTimer = 0;
                AttackState = AttackStates.BigSnapping;
                AttackTimer = 55;
            }
        }
        #endregion
        #region Eat State Methods
        private void ResetEating(NPC npc)
        {
            EatingState = EatingStates.None;
            EatTimer = 0;
            ItemHeld = -1;
        }

        private void SearchForFood(NPC npc)
        {
            const float range = 200f;
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (!item.active || item.type != ItemID.JungleRose || npc.Center.Distance(item.Center) > range)
                    continue;
                ItemHeld = i;
                EatingState = EatingStates.Grabbing;
                break;
            }
        }

        private void ApproachFood(NPC npc)
        {
            Item rose = Main.item[ItemHeld];

            if (!rose.active || rose.beingGrabbed)
            {
                ResetEating(npc);
                return;
            }

            Vector2 posDiff = rose.Center - npc.Center;
            float dist = posDiff.Length();
            float rot = posDiff.ToRotation();
            npc.rotation = rot;
            if (npc.velocity.Length() > 1f)
            {
                npc.velocity *= 0.9f;
            }
            npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(rose.Center) * 5, 0.03f);
            //npc.velocity += 0.03f * (dist * dist * dist / 500000f) * Vector2.UnitX.RotatedBy(rot);
            if (dist < 35f) // time to eat! :yummy:
            {
                rose.active = false;
                if (FargoSoulsUtil.HostCheck)
                    ItemHeld = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<RoseFruitTransform>(), 0, 0f, ai0: npc.whoAmI);
                npc.netUpdate = true;
                EatingState = EatingStates.Eating;
                NetSync(npc);
            }
            return;
        }

        private void EatFood(NPC npc)
        {
            const int chompCount = 3;

            Projectile rose = Main.projectile[ItemHeld];
            if (!rose.active)
            {
                ResetEating(npc);
            }

            Vector2 basePos = new Vector2(16 * npc.ai[0], 16 * npc.ai[1]); // position of the root
            Vector2 posToRoot = basePos - npc.position;
            float dist = posToRoot.Length();
            npc.velocity = ((dist * dist / 50000f)) * Vector2.UnitX.RotatedBy(posToRoot.ToRotation());
            npc.rotation = posToRoot.ToRotation() + MathHelper.Pi;

            EatTimer++;
            if (EatTimer % 60 == 0)
            {
                SpawnDustFromMouthItem(npc, rose, DustID.Grass, 10);
                rose.scale -= 0.5f/(chompCount);
                SoundEngine.PlaySound(SoundID.Item2, npc.Center);
                SoundEngine.PlaySound(SoundID.NPCHit1, npc.Center);
            }
            if (EatTimer > chompCount * 60f)
            {
                // finished eating
                EatTimer = 0;
                rose.ai[1] = 1;
                EatingState = EatingStates.Digesting;
            }
        }

        private void SpitOutFruit(NPC npc)
        {
            const int gagCount = 2;
            if (EatingState == EatingStates.Digesting)
            {
                if (EatTimer++ <= 30f)
                {
                    npc.velocity *= 0.9f;
                    return;
                }
                EatTimer = 0;
                EatingState = EatingStates.Vomiting;
            }

            Projectile fruit = Main.projectile[ItemHeld];
            fruit.ai[1] = 2;
            if (fruit.scale < 0.5f) // fruit appears
            {
                FargoSoulsUtil.DustRing(fruit.Center, 10, DustID.GemAmethyst, 1.5f);
                fruit.scale = 0.5f;
            }

            float x = EatTimer % 60;
            npc.velocity = 2 * (x/30 - 1) * Vector2.UnitX.RotatedBy(npc.rotation);
            if (EatTimer % 60 == 0) // gag
            {
                SoundEngine.PlaySound(SoundID.NPCDeath1, npc.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath13 with { Pitch = 0.3f, Volume = 0.5f }, npc.Center);
                fruit.scale += 0.5f/(gagCount);
                SpawnDustFromMouthItem(npc, fruit, DustID.Plantera_Pink, 10);
            }
            if (EatTimer >= 60 * gagCount) // throw up fruit
            {
                SoundEngine.PlaySound(SoundID.NPCDeath1, npc.Center);
                SoundEngine.PlaySound(SoundID.ChesterOpen with { Pitch = -0.8f }, npc.Center);
                SpawnDustFromMouthItem(npc, fruit, DustID.Plantera_Pink, 20);
                fruit.ai[1] = 3;
                ResetEating(npc);
            }
            EatTimer++;
        }

        private void SpawnDustFromMouthItem(NPC npc, Projectile item, int type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustDirect(item.position, item.width, item.height, type);
                d.velocity += 3 * Vector2.UnitX.RotatedBy(npc.rotation);
            }
        }
        #endregion

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> vine = npc.type == NPCID.AngryTrapper ? TextureAssets.Chain14 : npc.type == NPCID.Snatcher ? TextureAssets.Chain5 : TextureAssets.Chain4;
            LazyAsset<Texture2D> t = FargoAssets.GetTexture2D("Content/NPCs/EternityModeNPCs/VanillaEnemies/Jungle", "Snatchers");
            Rectangle JawLeft = new();
            Vector2 OriginLeft = new();
            Rectangle JawRight = new();
            Vector2 OriginRight = new();
            Rectangle Eye = new();
            Vector2 OriginEye = new();
            Rectangle bush = new(56, 110, 48, 20);

            if (npc.type == NPCID.Snatcher)
            {
                JawLeft = new Rectangle(0, 60, 42, 18);
                OriginLeft = new Vector2(36, 0);
                JawRight = new Rectangle(0, 40, 42, 18);
                OriginRight = new Vector2(36, 17);
                Eye = new Rectangle(44, 54, 14, 10);
                OriginEye = new Vector2(6, 5);
            }
            else if (npc.type == NPCID.ManEater)
            {
                JawLeft = new Rectangle(0, 20, 42, 18);
                OriginLeft = new Vector2(36, 0);
                JawRight = new Rectangle(0, 0, 42, 18);
                OriginRight = new Vector2(36, 17);
                Eye = new Rectangle(44, 14, 14, 10);
                OriginEye = new Vector2(6, 5);
            }
            else if (npc.type == NPCID.AngryTrapper)
            {
                JawLeft = new Rectangle(0, 106, 54, 24);
                OriginLeft = new Vector2(42, 2);
                JawRight = new Rectangle(0, 80, 54, 24);
                OriginRight = new Vector2(42, 20);
            }

            if (EatingState == EatingStates.Digesting || EatingState == EatingStates.Vomiting)
            {
                int frame = 1;
                int window = 5;
                if (EatingState == EatingStates.Vomiting && (EatTimer % 60 < window || EatTimer % 60 > 60 - window))
                    frame = 2;
                npc.frame.Y = npc.frame.Height * frame;
            }


            if (IdleVinePointsReal != null)
            {
                Vector2[] drawPoints = (Vector2[])IdleVinePointsReal.Clone();
                float lerpFactor = Math.Clamp(npc.Distance(OriginPoint) / (sizeFactor * 30f), 0, 1);

                for (int i = 0; i < IdleVinePointsReal.Length; i++)
                {

                    drawPoints[i] = Vector2.Lerp(IdleVinePointsReal[i], npc.Center + npc.DirectionTo(OriginPoint) * (npc.Distance(OriginPoint) / IdleVinePointsReal.Length * (IdleVinePointsReal.Length - i)), lerpFactor);
                    //Dust.NewDustPerfect(drawPoints[i], DustID.Terra).velocity *= Vector2.Zero;
                    if (npc.Distance(OriginPoint) < IdleVinePointsReal[i].Distance(OriginPoint) && AttackState == AttackStates.Launching)
                    {
                        drawPoints[i] = npc.Center;
                    }
                }

                for (int i = 0; i < drawPoints.Length; i++)
                {
                    Vector2 nextPoint = npc.Center;
                    if (i < drawPoints.Length - 1) nextPoint = drawPoints[i + 1];
                    for (int k = 0; k < drawPoints[i].Distance(nextPoint); k += vine.Height() / 3)
                    {
                        Rectangle vource = new Rectangle(0, vine.Height() / 3 * (k / 8 % 3), vine.Width(), vine.Height() / 3);
                        spriteBatch.Draw(vine.Value, drawPoints[i] + drawPoints[i].DirectionTo(nextPoint) * k - Main.screenPosition, vource, drawColor, drawPoints[i].AngleTo(nextPoint) + MathHelper.PiOver2, vource.Size() / 2, 1, SpriteEffects.None, 1);
                    }
                }
            }
            
            spriteBatch.Draw(t.Value, OriginPoint - Main.screenPosition, bush, drawColor, 0, bush.Size() / 2, 1, SpriteEffects.None, 1);
            if (EatingState != EatingStates.None)
            {
                return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
            }
            if (npc.type != NPCID.AngryTrapper)
            {
                spriteBatch.Draw(t.Value, npc.Center - Main.screenPosition + new Vector2(6, 0).RotatedBy(npc.rotation + Math.PI), Eye, drawColor, npc.rotation, OriginEye, 1, SpriteEffects.None, 1);
            }
            spriteBatch.Draw(t.Value, npc.Center - Main.screenPosition, JawLeft, drawColor, npc.rotation - MathHelper.Lerp(0, MathHelper.PiOver2, JawOpenness), OriginLeft, 1, SpriteEffects.None, 1);
            spriteBatch.Draw(t.Value, npc.Center - Main.screenPosition, JawRight, drawColor, npc.rotation - MathHelper.Lerp(0, -MathHelper.PiOver2, JawOpenness), OriginRight, 1, SpriteEffects.None, 1);

            return false;
            
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (BittenPlayer != -1)
                return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            base.ModifyHitPlayer(npc, target, ref modifiers);

            target.longInvince = true;
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.Bleeding, 300);

            if (BittenPlayer == -1 && BiteTimer == 0 && AttackState == AttackStates.BigSnapping && AttackTimer >= 60)
            {
                AttackTimer = 0;
                AttackState = AttackStates.Grabbing;
                BittenPlayer = target.whoAmI;
                BiteTimer = 360;
                npc.velocity *= 0.1f;
                //NetSync(npc, false);

                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    // remember that this is target client side; we sync to server
                    var netMessage = Mod.GetPacket();
                    netMessage.Write((byte)FargowiltasSouls.PacketID.SyncSnatcherGrab);
                    netMessage.Write((byte)npc.whoAmI);
                    netMessage.Write((byte)BittenPlayer);
                    netMessage.Write(BiteTimer);
                    netMessage.Send();
                }
                NetSync(npc);
            }

            LocalizedText DeathText = Language.GetText("Mods.FargowiltasSouls.DeathMessage.Snatchers");
            if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld && npc.type == NPCID.ManEater && target.Male)
            {
                target.KillMe(PlayerDeathReason.ByCustomReason(DeathText.ToNetworkText(target.name)), 999999, 0);
            }
        }

        public override void OnKill(NPC npc)
        {
            //Player player = FargoSoulsUtil.PlayerExists(npc.lastInteraction);
            //int chance = player != null && player.FargoSouls().HasJungleRose ? 5 : 200;
            //if (Main.rand.NextBool(chance))
            //{
            //    Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ModContent.Find<ModItem>("Fargowiltas", "PlanterasFruit").Type);
            //}
        }
    }
}
