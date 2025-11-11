using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies
{
    public class MeteorHead : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.MeteorHead);

        public int Counter;
        public int ParticleTimer;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(Counter);
            binaryWriter.Write(ParticleTimer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            Counter = binaryReader.ReadInt32();
            ParticleTimer = binaryReader.ReadInt32();
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            if (NPC.downedGolemBoss && Main.rand.NextBool(4))
                npc.Transform(NPCID.SolarCorite);
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (++Counter > 120)
            {
                Counter = 0;

                int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                if (t != -1 && npc.Distance(Main.player[t].Center) < 600 && FargoSoulsUtil.HostCheck)
                {
                    npc.velocity += npc.DirectionTo(Main.player[t].Center) * 5;
                    ParticleTimer = 20;
                    npc.netUpdate = true;
                    NetSync(npc);
                }
            }

            if (ParticleTimer > 0)
            {
                ParticleTimer--;
                for (int i = -1; i < 2; i += 2)
                {
                    Particle p = new SparkParticle(npc.Center - npc.velocity, -npc.velocity.RotatedBy(i * Math.PI / 20), Color.OrangeRed, 1f, 40);
                    if (ParticleTimer % 2 == 0)
                        p.Spawn();
                }
            }

            //EModeGlobalNPC.Aura(npc, 100, BuffID.OnFire, false, DustID.Torch);
        }
    }
}
