using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2KoboldWalker : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.DD2KoboldWalkerT2,
            NPCID.DD2KoboldWalkerT3
        );

        public int Counter = 0;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(Counter);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            Counter = binaryReader.ReadInt32();
        }

        public override void AI(NPC npc)
        {
            if (npc.dontTakeDamage && npc.life < npc.lifeMax)
            {
                npc.TargetClosest();
                Counter++;
                npc.position.X += 1.5f * npc.velocity.X;
                if (Counter > 40)
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, Scale: 4f);
                if (Counter > 120)
                {
                    npc.ai[1] = 2;
                    npc.HitEffect();
                }
            }

            base.AI(npc);
        }

        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            if (Counter == 0 && npc.life - hit.Damage < 0 && !npc.dontTakeDamage)
            {
                hit.Null();
                npc.life = 10;
                npc.dontTakeDamage = true;
                npc.ai[0] = 39;
            }
            base.HitEffect(npc, hit);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.OnFire, 180);
            //target.AddBuff(ModContent.BuffType<FusedBuff>(), 1800);
        }
    }
}
