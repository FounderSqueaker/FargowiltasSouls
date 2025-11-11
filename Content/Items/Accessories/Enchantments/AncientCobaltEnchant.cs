using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class AncientCobaltEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(53, 76, 116);

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Blue;
            Item.value = 50000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<AncientCobaltEffect>(Item);
            player.AddEffect<AncientCobaltFallEffect>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()

                .AddIngredient(ItemID.AncientCobaltHelmet)
                .AddIngredient(ItemID.AncientCobaltBreastplate)
                .AddIngredient(ItemID.AncientCobaltLeggings)
                .AddIngredient(ItemID.Bomb, 10)
                .AddIngredient(ItemID.Dynamite, 10)
                .AddIngredient(ItemID.Grenade, 10)

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Melee;
            tooltipColor = null;
            scaling = null;
            return AncientCobaltEffect.BaseDamage(Main.LocalPlayer);
        }
    }

    public class AncientCobaltEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<EarthHeader>();
        public override int ToggleItemType => ModContent.ItemType<AncientCobaltEnchant>();
        public static int BaseDamage(Player player)
        {
            int dmg = 35;
            if (player.FargoSouls().CobaltEnchantActive || player.ForceEffect<AncientCobaltEffect>())
                dmg = 150;
            if (player.FargoSouls().CobaltEnchantActive && player.ForceEffect<AncientCobaltEffect>())
                dmg = 300;
            return (int)(dmg * player.ActualClassDamage(DamageClass.Melee));
        }
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (modPlayer.CobaltCooldownTimer > 0)
            {
                modPlayer.CobaltCooldownTimer--;
            }

            if (player.jump <= 0 && player.velocity.Y == 0f)
            {
                modPlayer.CanCobaltJump = true;
                modPlayer.JustCobaltJumped = false;
            }
            else
            {
                modPlayer.CanCobaltJump = false;
            }
            bool notAncient = EffectItem(player).type != ModContent.ItemType<AncientCobaltEnchant>();
            if (player.controlJump && player.releaseJump && modPlayer.CanCobaltJump && !modPlayer.JustCobaltJumped && modPlayer.CobaltCooldownTimer <= 0)
            {
                bool upgrade = notAncient || player.ForceEffect<AncientCobaltEffect>();

                float scale = 1.5f;
                int debuff = 1;
                if (upgrade)
                {
                    scale = 2f;
                    debuff = 2;
                }

                Projectile.NewProjectile(GetSource_EffectItem(player), player.Center, Vector2.Zero, ModContent.ProjectileType<CobaltExplosion>(), BaseDamage(player), 0, player.whoAmI, ai0: scale, ai1: debuff);

                modPlayer.JustCobaltJumped = true;

                if (modPlayer.CobaltCooldownTimer <= 30)
                    modPlayer.CobaltCooldownTimer = 30;
            }

            if (modPlayer.CanCobaltJump || modPlayer.JustCobaltJumped && !player.ExtraJumps.ToArray().Any(j => j.Active))
            {
                player.jumpBoost = true; //balloon effect
                if (notAncient || player.ForceEffect<AncientCobaltEffect>())
                {
                    player.jumpSpeedBoost += !player.controlDown ? 5f : 1f; //+100% / +20% when holding down
                }
                else
                {
                    player.jumpSpeedBoost += !player.controlDown ? 2.5f : 1f; //+50% / +20% when holding down
                }
            }
        }
    }

    public class AncientCobaltFallEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<EarthHeader>();
        public override int ToggleItemType => ModContent.ItemType<AncientCobaltEnchant>();
        public override void PostUpdateEquips(Player player)
        {
            if (player.controlDown)
            {
                player.maxFallSpeed *= 1.5f;

                bool holdingDown = player.controlDown && !player.controlJump;
                bool notInLiquid = !player.wet;
                bool notOnRope = !player.pulley && player.ropeCount == 0;
                bool notGrappling = player.grappling[0] == -1;
                bool airborne = player.velocity.Y != 0;
                if (holdingDown && notInLiquid && notOnRope && notGrappling && airborne)
                {
                    player.velocity.Y += player.gravity * player.gravDir * 0.5f;
                    if (player.velocity.Y * player.gravDir > player.maxFallSpeed)
                        player.velocity.Y = player.maxFallSpeed * player.gravDir;
                }
            }
        }

    }

}
