using FargowiltasSouls.Assets.Textures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Accessories.Souls
{
    public class SuperBlood : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Accessories/Souls", Name);
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.aiStyle = 2;
            Projectile.timeLeft = 300;
            AIType = 48;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 120;

            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            //dust!
            int dustId = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + 2f), Projectile.width, Projectile.height + 5, DustID.Blood, Projectile.velocity.X * 0.2f,
                Projectile.velocity.Y * 0.2f, 100);
            Main.dust[dustId].noGravity = true;
            int dustId3 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + 2f), Projectile.width, Projectile.height + 5, DustID.Blood, Projectile.velocity.X * 0.2f,
                Projectile.velocity.Y * 0.2f, 100);
            Main.dust[dustId3].noGravity = true;
        }
    }
}