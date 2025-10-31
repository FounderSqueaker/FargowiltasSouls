using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy
{
    public class BetsyFusedSigil : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_673";

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.scale = 0.5f;
            Projectile.Opacity = 0f;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            ref float kobold = ref Projectile.ai[0];
            ref float timer = ref Projectile.ai[1];
            Projectile.ai[2] = WorldSavingSystem.MasochistModeReal ? 90 : 120;

            if (kobold < 0 || kobold > Main.maxProjectiles)
            {
                int radius = 800;
                if (FargoSoulsUtil.HostCheck)
                    kobold = Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center - radius * Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)), Vector2.Zero, ModContent.ProjectileType<BetsySpawnPortal>(), Projectile.damage, 0f, ai0: NPCID.DD2KoboldFlyerT3, ai1: Projectile.whoAmI);
                Projectile.netUpdate = true;
                return;
            }

            Projectile.Opacity = (timer / Projectile.ai[2]);
            Projectile.scale = 0.5f * (timer / Projectile.ai[2]);
            timer++;

            if (timer >= Projectile.ai[2])
            {
                SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, Projectile.Center);
                Projectile.Kill();
                return;
            }
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
            for (int i = 0; i < 4; i++)
            {
                int goreId = Main.rand.NextFromCollection([GoreID.DD2KoboldFlyerT2_1, GoreID.DD2KoboldFlyerT2_2, GoreID.DD2KoboldFlyerT2_3, GoreID.DD2KoboldFlyerT2_4, GoreID.DD2KoboldFlyerT2_5]);
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitY, goreId);
            }
            if (FargoSoulsUtil.HostCheck)
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BetsyFusedSigilProj>(),
                    Projectile.damage, 0f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.ShadowFlame, 60);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            FargoSoulsUtil.GenericProjectileDraw(Projectile, Color.Orange);
            return false;
        }
    }

    internal class BetsyFusedSigilProj : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles", "Empty");
        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 5;
        }

        public override void AI()
        {
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
            }

            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.OnFire, 180);
            target.AddBuff(BuffID.WitheredArmor, 300);
            target.AddBuff(ModContent.BuffType<FusedBuff>(), 600);
        }
    }
}