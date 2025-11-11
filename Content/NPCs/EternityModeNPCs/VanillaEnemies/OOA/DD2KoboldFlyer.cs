using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.GoblinInvasion;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2KoboldFlyer : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.DD2KoboldFlyerT2,
            NPCID.DD2KoboldFlyerT3
        );

        public int Counter = 0;
        public override void SetDefaults(NPC npc)
        {
            npc.lifeMax /= 2;
        }
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
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, Scale: 4f);
                if (Counter > 90)
                {
                    npc.dontTakeDamage = false;
                    npc.ai[0] = 3;
                    npc.ai[1] = 1;
                    npc.ai[3] = 0;
                }
            }

            base.AI(npc);
        }

        public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (Counter > 0)
                return false;
            return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);
        }

        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            if (Counter == 0 && npc.life - hit.Damage < 0 && !npc.dontTakeDamage)
            {
                hit.Null();
                npc.life = 10;
                npc.dontTakeDamage = true;
                npc.ai[0] = 1;
                npc.noTileCollide = true;
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
