using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core.Toggler.Content;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Forces
{
    [LegacyName("ShadowForce")]
    public class DeathForce : BaseForce
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Enchants[Type] =
            [
                ModContent.ItemType<NinjaEnchant>(),
                ModContent.ItemType<ShadowEnchant>(),
                ModContent.ItemType<AncientShadowEnchant>(),
                ModContent.ItemType<CrystalAssassinEnchant>(),
                ModContent.ItemType<SpookyEnchant>(),
                ModContent.ItemType<ShinobiEnchant>(),
                ModContent.ItemType<DarkArtistEnchant>(),
                ModContent.ItemType<NecroEnchant>()
            ];
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            SetActive(player);
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            player.AddEffect<ShadowForceEffect>(Item);
            // Ninja
            player.AddEffect<NinjaEffect>(Item);
            // Ashadow
            player.AddEffect<ShadowBalls>(Item);
            // CrystalAssassin-Shinobi
            player.AddEffect<CrystalDiagonalDash>(Item);
            player.AddEffect<MonkDashEffect>(Item);
            if (player.HasEffect<ShadowForceEffect>())
                player.AddEffect<ShadowForceDashEffect>(Item);
            // Spooky
            player.AddEffect<SpookyEffect>(Item);
            // Dartist
            modPlayer.ApprenticeEnchantActive = true;
            modPlayer.DarkArtistEnchantActive = true;
            player.AddEffect<ApprenticeSupport>(Item);

            // Necro
            player.AddEffect<NecroEffect>(Item);

            if (!player.HasEffect<ShadowForceEffect>())
            {
                player.AddEffect<DarkArtistMinion>(Item);
                player.AddEffect<NecroEffect>(Item);
                //shadow orbs
                player.AddEffect<AncientShadowBolt>(Item);
                //shinobi and monk effects
                ShinobiEnchant.AddEffects(player, Item);
                //smoke bomb nonsense
                CrystalAssassinEnchant.AddEffects(player, Item);

            }


        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            foreach (int ench in Enchants[Type])
                recipe.AddIngredient(ench);
            recipe.AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"));
            recipe.Register();
        }
    }
    public class ShadowForceEffect : AccessoryEffect
    {
        public override Header ToggleHeader => null;
        //public override int ToggleItemType => ModContent.ItemType<ShadowForce>();
    }
    public class ShadowForceDashEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<ShadowHeader>();
        public override int ToggleItemType => ModContent.ItemType<DeathForce>();
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer farg = player.FargoSouls();
            if (farg.IFrameDashTimer > 0)
                farg.IFrameDashTimer--;
            if (player.whoAmI == Main.myPlayer)
                CooldownBarManager.Activate("DeathForceIframeCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Enchantments", "ShinobiEnchant").Value, new(147, 91, 24),
                    () => 1 - farg.IFrameDashTimer / 250f, activeFunction: () => player.HasEffect<ShadowForceDashEffect>());
            if (farg.IFrameDashTimer == 1 && player.HasEffect<MonkDashEffect>())
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = -0.5f });
                Particle boom = new AlphaBloomParticle(player.Center, Vector2.Zero, Color.Black, Vector2.One * 8, Vector2.One * 1, 40, false);
                boom.Spawn();
                for (int i = 0; i < 10; i++)
                {
                    Vector2 pos = player.Center + new Vector2(0, Main.rand.NextFloat(20, 100)).RotatedByRandom(MathHelper.TwoPi);
                    Particle boom2 = new AlphaBloomParticle(pos, (player.Center - pos).SafeNormalize(Vector2.Zero)*2, Color.Black, Vector2.One * 1, Vector2.One * 0.1f, 50, false);
                    boom2.Spawn();
                }
            }
            if (farg.IsDashingTimer <= 0)
                farg.IFrameDash = false;
            if (farg.ShadowDashTimer > 0)
            {
                Color color = Color.Purple;
                if (farg.IFrameDash)
                {
                    player.immuneNoBlink = true;
                    player.immuneTime = 5;
                    player.immune = true;
                    player.AddImmuneTime(ImmunityCooldownID.Bosses, 1);
                    color = Color.Black;
                    Particle part = new AlphaSparkParticle(player.oldPosition + player.Size * 0.5f, player.velocity * 0.4f, Color.Black, 1, 10, false);
                    part.Spawn();
                    player.Yoraiz0rEye();
                }
                farg.ShadowDashTimer--;
                
                Particle bub = new AlphaSparkParticle(player.position + new Vector2(Main.rand.Next(0, player.width), Main.rand.Next(0, player.height)), -(player.velocity * 0.1f) , color, 0.5f, 10, false);
                bub.Spawn();
            }
        }
        public override void DrawEffects(Player player, PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            FargoSoulsPlayer farg = player.FargoSouls();
            if (farg.IFrameDash)
            {
                float jerp = farg.ShadowDashTimer / 10f;
                jerp = MathHelper.Clamp(jerp, 0f, 1f);
                r = MathHelper.Lerp(r, 0, jerp);
                g = MathHelper.Lerp(g, 0, jerp);
                b = MathHelper.Lerp(b, 0, jerp);
            }
        }
        public static void AddDash(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            modPlayer.HasDash = true;
            modPlayer.FargoDash = DashManager.DashType.Shadow;
        }
        public static void ShadowDash(Player player, int dir)
        {
            player.FargoSouls().ShadowDashTimer = 30;
            

            FargoSoulsPlayer modPlayer = player.FargoSouls();
            float dashSpeed = 25f;
            if (player.HasEffect<CrystalDiagonalDash>())
            {
                player.velocity.Y *= 0;
                if (player.controlUp)
                    player.velocity.Y = dashSpeed * -0.5f;
                else if (player.controlDown)
                    player.velocity.Y = dashSpeed * 0.7f;
                else
                    player.velocity.Y = float.Epsilon;
            }
            player.velocity.X = dashSpeed * dir;
            if (modPlayer.IsDashingTimer < 30)
                modPlayer.IsDashingTimer = 30;
            player.dashDelay = 60;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);

            if (player.FargoSouls().IFrameDashTimer <= 0 && player.HasEffect<MonkDashEffect>())
            {
                player.FargoSouls().IFrameDash = true;
                player.FargoSouls().IFrameDashTimer = 250;
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = -0.5f });

                Particle boom = new AlphaBloomParticle(player.Center, Vector2.Zero, Color.Black, Vector2.One * 1, Vector2.One * 10, 20, false);
                boom.Spawn();
                for (int i = 0; i < 10; i++)
                {
                    Particle boom2 = new AlphaBloomParticle(player.Center, -player.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-30, 30))) * Main.rand.NextFloat(0.1f, 0.4f), Color.Black, Vector2.One * 1, Vector2.One * 0.1f, 100, false);
                    boom2.Spawn();
                }

                //int proj = Projectile.NewProjectile(player.GetSource_EffectItem<ShadowForceDashEffect>(), player.Center, Vector2.Zero, ModContent.ProjectileType<ShadowDash>(), (int)(3600 * player.ActualClassDamage(DamageClass.Melee)), 3, player.whoAmI);
                //if (Main.netMode == NetmodeID.MultiplayerClient)
                //    NetMessage.SendData(MessageID.SyncProjectile, number: proj);
                float melee = player.ActualClassDamage(DamageClass.Melee);
                float summon = player.ActualClassDamage(DamageClass.Summon);
                int scytheDmg = (int)(2800 * (((melee + summon - 2f) / 2f) + 1f)); // melee-summon 50-50 damage scaling
                int proj = Projectile.NewProjectile(player.GetSource_EffectItem<ShadowForceDashEffect>(), player.Center, Vector2.Zero, ModContent.ProjectileType<SpookySpinScythe>(), scytheDmg, 3, player.whoAmI, ai2: 10);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncProjectile, number: proj);

                if (player.HasEffect<ShadowBalls>())
                {
                    int numBalls = 5;
                    int dmg = 400;

                    int damage = FargoSoulsUtil.HighestDamageTypeScaling(player, dmg);
                    Projectile[] balls = FargoSoulsUtil.XWay(numBalls, player.GetSource_EffectItem<ShadowForceDashEffect>(), player.Center, ModContent.ProjectileType<ShadowBall>(), 6, damage, 0);

                    foreach (Projectile ball in balls)
                    {
                        ball.originalDamage = damage;
                    }
                }
            }
        }
    }
}
