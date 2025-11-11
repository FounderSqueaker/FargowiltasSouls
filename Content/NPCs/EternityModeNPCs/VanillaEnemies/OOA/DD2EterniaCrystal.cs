using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using System.Linq;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2EterniaCrystal : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.DD2EterniaCrystal);

        public int InvulTimer;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.ImmuneToAllBuffs[NPCID.DD2EterniaCrystal] = true;
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

        }

        public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
        {
            if (projectile.FargoSouls().Reflected || IsImmune(npc))
                return false;
            return base.CanBeHitByProjectile(npc, projectile);
        }

        public override bool CanBeHitByNPC(NPC npc, NPC attacker)
        {
            return !IsImmune(npc) && base.CanBeHitByNPC(npc, attacker);
        }

        private bool IsImmune(NPC npc)
        {
            if (!EModeDD2Event.BetsyBlockSpawn || !NPC.AnyNPCs(NPCID.DD2Betsy))
                return false;

            if (Main.player.Any(x => x.Alive() && x.Distance(npc.Center) < 2000)) // any alive players near betsy?
                return true;

            return false;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (NPC.AnyNPCs(NPCID.DD2Betsy) && EModeDD2Event.BetsyBlockSpawn) // no nearby players for betsy, make crystal take more damage
                modifiers.FinalDamage *= 3;

            base.ModifyIncomingHit(npc, ref modifiers);
        }
    }
}
