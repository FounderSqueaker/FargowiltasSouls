using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs;
using FargowiltasSouls.Content.Projectiles.Weapons.BossWeapons;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops
{
    public class HellBonez : Bonez
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Weapons/SwarmWeapons", Name);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.scale = 2f;
            Projectile.timeLeft = 120;
            Projectile.extraUpdates = 1;
            
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 8;
            Projectile.GetGlobalProjectile<AttackSpeedScalingGlobalProjectile>().UseAttackSpeedToDoubleHit = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.3f * Math.Sign(Projectile.velocity.X);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
          //target.immune[Projectile.owner] = 8;
            target.AddBuff(ModContent.BuffType<BlackInfernoBuff>(), 60);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width / 2, Projectile.height / 2, DustID.Hay, Projectile.velocity.X * 0.75f, Projectile.velocity.Y * 0.75f, 0, default, 2f);
                Main.dust[d].noGravity = true;
            }
        }
    }
}
