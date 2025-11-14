using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Threading.Channels;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using static FargowiltasSouls.FargowiltasSouls;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2Goblin : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.DD2GoblinT1,
            NPCID.DD2GoblinT2,
            NPCID.DD2GoblinT3
        );

        public override void SetDefaults(NPC entity)
        {
            base.SetDefaults(entity);
            entity.damage = (int)(entity.damage * 0.8f);
        }

        public int Timer = -60;
        public int State;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(Timer);
            binaryWriter.Write(State);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            Timer = binaryReader.ReadInt32();
            State = binaryReader.ReadInt32();
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (State != 0)
            {
                modifiers.Null();
                return;
            }

            if (State == 0 && TryDodgeRoll(npc))
            {
                DodgeRoll(npc);
                modifiers.Null();
                return;
            }
            else
            {
                npc.HideStrikeDamage = false;
            }
            base.ModifyIncomingHit(npc, ref modifiers);
        }

        private bool TryDodgeRoll(NPC npc)
        {
            if (npc.dontTakeDamage)
                return false;

            int chance = Math.Max(15 - (int)(Timer / 60), 8);
            return State == 0 && Main.rand.NextBool(15);
        }

        public void DodgeRoll(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                var netMessage = FargowiltasSouls.Instance.GetPacket();
                netMessage.Write((byte)PacketID.GoblinDodgeRoll);
                netMessage.Write((byte)npc.whoAmI);
                netMessage.Send();
            }
            else
            {
                npc.HideStrikeDamage = true;
                State = -1;
                Timer = -1;
            }
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (State != 0)
                return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }

        public override bool SafePreAI(NPC npc)
        {
            Timer++;

            if (State == 1)
            {
                
                float rollTime = 30;
                npc.dontTakeDamage = true;
                npc.rotation = Timer * npc.spriteDirection * (2f/rollTime) * MathHelper.TwoPi;
                Dust d = Dust.NewDustDirect(npc.Bottom, 1, 1, DustID.Smoke, Scale: 2f);
                d.noGravity = true;
                d.velocity *= 0.7f;
                if (Timer >= rollTime)
                {
                    npc.rotation = 0;
                    npc.dontTakeDamage = false;
                    State = 0;
                    Timer = 0;
                    npc.netUpdate = true;
                }
                return false;
            }

            if (State == -1)
            {
                npc.dontTakeDamage = true;
                npc.velocity.X = 6 * npc.spriteDirection;
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -1 }, npc.Center);
                SoundEngine.PlaySound(SoundID.DD2_GoblinScream, npc.Center);
                for (int i = 0; i < 20; i++)
                {
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Smoke, Scale: 2f);
                    d.noGravity = true;
                    d.velocity *= 0.7f;
                }
                State = 1;
                npc.netUpdate = true;
                return false;
            }

            return base.SafePreAI(npc);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            //target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(BuffID.Bleeding, 300);
        }
    }
}
