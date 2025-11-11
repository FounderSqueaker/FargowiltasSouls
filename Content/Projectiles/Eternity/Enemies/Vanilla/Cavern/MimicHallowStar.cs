using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Cavern
{
    public class MimicHallowStar : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_92";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Hallow Star");
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.HallowStar);
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.light = 1f;
            //CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void PostAI()
        {
            Projectile.tileCollide = false;
            Projectile.light = 1f;
        }
    }
}
