using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Terraria;
using Terraria.ID;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Hell
{
    public class BoneSerpent : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.BoneSerpentHead);

        public int Counter;
        public override void SetDefaults(NPC npc)
        {
            if (NPC.downedMechBossAny)
            {
                npc.lifeMax *= 3; 
            }
        }
        public override void AI(NPC npc)
        {
            base.AI(npc);

            //lava wet doesnt work because ??
            if (Collision.LavaCollision(npc.position, npc.width, npc.height))
            {
                if (++Counter < 150 && Counter % 30 == 0)
                {
                    int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                    if (t != -1 && npc.Distance(Main.player[t].Center) < 800 && FargoSoulsUtil.HostCheck)
                        FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, NPCID.BurningSphere);
                }
            }
            else
            {
                Counter = 0;
            }
        }
    }
}
