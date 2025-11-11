using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Hell;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Hell
{
    public class FireImp : Teleporters
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.FireImp);

        public override void SetDefaults(NPC npc)
        {
            if (NPC.downedMechBossAny && npc.lifeMax < 650)
            {
                npc.lifeMax = 650;
            }
        }
        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            if (Main.rand.NextBool(5))
            {
                npc.TargetClosest(false);
                if (npc.HasValidTarget && Main.player[npc.target].ZoneUnderworldHeight && npc.FargoSouls().CanHordeSplit)
                    EModeGlobalNPC.Horde(npc, Main.rand.Next(3) + 1);
            }
        }
        public int Timer = 0;

        public override bool SafePreAI(NPC npc)
        {
            if (true)
            {
                if (npc.ai[1] == 24 && npc.HasPlayerTarget && npc.Distance(Main.player[npc.target].Center) < 800) // frame before it should fire fireball
                {
                    npc.velocity *= 0f;
                    if (Timer == 0)
                    {
                        if (FargoSoulsUtil.HostCheck)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Top - Vector2.UnitY * 30, Vector2.Zero, ModContent.ProjectileType<ImpFireOrb>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 3f, ai0: npc.target, ai2: npc.whoAmI);
                        }
                    }
                    Timer++;
                    if (Timer > 60 * 4)
                    {
                        return true;
                    }
                    return false;
                }
                else
                    Timer = 0;
            }
            return base.SafePreAI(npc);
        }
    }
}
