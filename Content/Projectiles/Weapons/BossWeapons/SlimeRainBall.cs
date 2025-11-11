using FargowiltasSouls.Assets.Textures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.BossWeapons
{
    public class SlimeRainBall : Bosses.MutantBoss.MutantSlimeBall
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Accessories", "SlimeBall");

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            CooldownSlot = ImmunityCooldownID.General;
        }

        public override void AI()
        {
            Projectile.tileCollide = --Projectile.ai[1] < 0;
            base.AI();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slimed, 240);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Slimed, 240);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(lightColor.R, lightColor.G, lightColor.B, lightColor.A);
        }
    }
}