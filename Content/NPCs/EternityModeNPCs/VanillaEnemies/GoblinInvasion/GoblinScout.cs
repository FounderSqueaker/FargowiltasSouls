using FargowiltasSouls.Core.NPCMatching;
using Terraria;
using Terraria.ID;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.GoblinInvasion
{
    public class GoblinScout : Shooters
    {
        public GoblinScout() : base(250, ProjectileID.ThrowingKnife, 14f, 1.25f, DustID.Grass, 1000, 30, true) { }
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.GoblinScout);
        public override void AI(NPC npc)
        {
            npc.dontTakeDamage = false;
            base.AI(npc);
            if (AttackTimer == AttackThreshold - Telegraph)
                npc.velocity.Y = -11f;
            if (AttackTimer > AttackThreshold - Telegraph)
            {
                npc.position.Y += npc.velocity.Y;
                if (npc.velocity.Y < 0)
                    npc.dontTakeDamage = true;
            }
            if (AttackTimer == AttackThreshold)
                npc.velocity.X -= npc.direction * 5;
        }
    }
}
