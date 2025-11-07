using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Jungle;
using FargowiltasSouls.Content.Projectiles.Weapons.Minions;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
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

        public int DashTimer;
        public int BiteTimer;
        public int BittenPlayer = -1;
        public int ItemHeld = -1;
        public EatingStates EatingState = EatingStates.None;
        public int EatTimer = 0;

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

            binaryWriter.Write(DashTimer);
            binaryWriter.Write(BiteTimer);
            binaryWriter.Write(BittenPlayer);
            binaryWriter.Write(ItemHeld);
            binaryWriter.Write((int)EatingState);
            binaryWriter.Write(EatTimer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            DashTimer = binaryReader.ReadInt32();
            BiteTimer = binaryReader.ReadInt32();
            BittenPlayer = binaryReader.ReadInt32();
            ItemHeld = binaryReader.ReadInt32();
            EatingState = (EatingStates)binaryReader.ReadInt32();
            EatTimer = binaryReader.ReadInt32();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.damage = (int)(2.0 / 3.0 * npc.damage);
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.Poisoned] = true;
            npc.buffImmune[BuffID.Venom] = true;
        }

        public override bool SafePreAI(NPC npc)
        {
            switch (EatingState)
            {
                case EatingStates.None:
                    if (BittenPlayer == -1)
                        SearchForFood(npc);
                    return base.SafePreAI(npc);
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
            npc.spriteDirection = 1;
            return false;
        }

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
            npc.velocity += 0.03f * (dist * dist * dist / 500000f) * Vector2.UnitX.RotatedBy(rot);
            if (dist < 35f) // time to eat! :yummy:
            {
                rose.active = false;
                if (FargoSoulsUtil.HostCheck)
                    ItemHeld = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<RoseFruitTransform>(), 0, 0f, ai0: npc.whoAmI);
                npc.netUpdate = true;
                EatingState = EatingStates.Eating;
                DashTimer = 0; // disallow immediate dash out of eating
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
            if (EatingState == EatingStates.Digesting || EatingState == EatingStates.Vomiting)
            {
                int frame = 1;
                int window = 5;
                if (EatingState == EatingStates.Vomiting && (EatTimer % 60 < window || EatTimer % 60 > 60 - window))
                    frame = 2;
                npc.frame.Y = npc.frame.Height * frame;
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            int dashTime = npc.type == NPCID.AngryTrapper ? 120 : 360;

            if (BittenPlayer != -1)
            {
                DashTimer = 0;

                Player victim = Main.player[BittenPlayer];
                if (BiteTimer > 0 && victim.active && !victim.ghost && !victim.dead
                    && (npc.Distance(victim.Center) < 160 || victim.whoAmI != Main.myPlayer)
                    && victim.FargoSouls().MashCounter < 20)
                {
                    victim.AddBuff(ModContent.BuffType<GrabbedBuff>(), 2);
                    victim.velocity = Vector2.Zero;
                    npc.Center = victim.Center;
                }
                else
                {
                    BittenPlayer = -1;
                    BiteTimer = -90; //cooldown

                    //retract towards home
                    npc.velocity = 15f * npc.SafeDirectionTo(new Vector2(npc.ai[0] * 16, npc.ai[1] * 16));

                    npc.netUpdate = true;
                    NetSync(npc);
                }
            }
            else if (++DashTimer > dashTime && npc.Distance(new Vector2((int)npc.ai[0] * 16, (int)npc.ai[1] * 16)) < 1000 && npc.HasValidTarget)
            {
                DashTimer = 0;
                npc.velocity = 15f * Vector2.Normalize(Main.player[npc.target].Center - npc.Center);
            }

            if (DashTimer == dashTime - 30)
                NetSync(npc);

            if (BiteTimer < 0)
                BiteTimer++;
            if (BiteTimer > 0)
                BiteTimer--;
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

            if (BittenPlayer == -1 && BiteTimer == 0)
            {
                BittenPlayer = target.whoAmI;
                BiteTimer = 360;
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
