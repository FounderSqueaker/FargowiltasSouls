using FargowiltasSouls.Content.Bosses.AbomBoss;
using FargowiltasSouls.Content.Buffs.Eternity;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.Minions
{
    public class AbomMinionSickle : AbomSickle3
    {
        public override string Texture => "FargowiltasSouls/Content/Bosses/AbomBoss/AbomSickle";

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            ProjectileID.Sets.MinionShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;

            Projectile.timeLeft = 180;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 5;

            CooldownSlot = ImmunityCooldownID.General;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) { }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 600);
        }
    }
}
