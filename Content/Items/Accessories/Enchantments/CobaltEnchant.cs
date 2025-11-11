using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class CobaltEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(61, 164, 196);

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Pink;
            Item.value = 100000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.FargoSouls().CobaltEnchantActive = true;
            player.AddEffect<CobaltEffect>(Item);
            player.AddEffect<AncientCobaltFallEffect>(Item);
            player.AddEffect<AncientCobaltEffect>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("FargowiltasSouls:AnyCobaltHead")
                .AddIngredient(ItemID.CobaltBreastplate)
                .AddIngredient(ItemID.CobaltLeggings)
                .AddIngredient(null, "AncientCobaltEnchant")
                .AddIngredient(ItemID.ScarabBomb, 10)
                .AddIngredient(ItemID.ExplosivePowder, 50)

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Melee;
            tooltipColor = null;
            scaling = null;
            return (int)((Main.LocalPlayer.ForceEffect<AncientCobaltEffect>() ? 300 : 150) * Main.LocalPlayer.ActualClassDamage(DamageClass.Melee));
        }
    }

    public class CobaltEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<EarthHeader>();
        public override int ToggleItemType => ModContent.ItemType<CobaltEnchant>();
        public override void PostUpdateEquips(Player player)
        {
            var modPlayer = player.FargoSouls();
            if (modPlayer.CanCobaltJump || modPlayer.JustCobaltJumped && !player.ExtraJumps.ToArray().Any(j => j.Active))
            {
                
            }
            else
                player.jumpSpeedBoost += !player.controlDown ? 3.75f : 1f; //+75% / +20% when holding down
        }
        public override void PostUpdate(Player player)
        {
            //player.jump *= 2;
        }
    }
}
