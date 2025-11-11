using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Ocean;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Cavern
{
    public class Crawdad : Shooters
    {
        public Crawdad() : base(60 * 5, ModContent.ProjectileType<BubbleHostile>(), 14, 1, DustID.BubbleBlock, 500, 60) { }

        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.Crab,
            NPCID.Crawdad,
            NPCID.Crawdad2
        );
        public int Frame;
        public override bool SafePreAI(NPC npc)
        {
            if (AttackTimer == AttackThreshold - Telegraph)
            {
                npc.velocity.X = -npc.direction * 2;
                npc.velocity.Y = -4;
                Frame = 4;
            }
            if (AttackTimer > AttackThreshold - Telegraph && AttackTimer < AttackThreshold)
            {
                AttackTimer++;
                if (npc.velocity.Y == 0) npc.velocity.X *= 0.9f;
                int num = (Frame == 5 || Frame == 6) ? 25 : 15;
                if (AttackTimer % num == 0 && Frame < 7) Frame++;
                if (AttackTimer > AttackThreshold - Telegraph * 0.3f && AttackTimer % 6 == 0)
                {
                    if (npc.HasPlayerTarget && npc.Distance(Main.player[npc.target].Center) < Distance
                        && (!NeedLineOfSight || NeedLineOfSight && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
                        && FargoSoulsUtil.HostCheck)
                    {
                        int p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Speed * npc.direction * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(0, (-MathHelper.Pi * npc.spriteDirection) / 8)), ProjectileType, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, DamageMultiplier), 0, Main.myPlayer);
                        if (p.IsWithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[p].friendly = false;
                            Main.projectile[p].hostile = true;
                            Main.projectile[p].netUpdate = true;
                        }
                    }
                }
                return false;
            }
            return base.SafePreAI(npc);
        }
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (npc.type != NPCID.Crab && AttackTimer > AttackThreshold - Telegraph && AttackTimer < AttackThreshold) 
                npc.frame.Y = Frame * frameHeight;
        }
    }
}
