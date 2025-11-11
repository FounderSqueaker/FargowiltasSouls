using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Fargowiltas.Content.Items.Tiles;

namespace FargowiltasSouls.Content.Items.Accessories.Forces
{
    public class NatureForce : BaseForce
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Enchants[Type] =
            [
                ModContent.ItemType<CrimsonEnchant>(),
                ModContent.ItemType<MoltenEnchant>(),
                ModContent.ItemType<RainEnchant>(),
                ModContent.ItemType<FrostEnchant>(),
                ModContent.ItemType<ChlorophyteEnchant>(),
                ModContent.ItemType<ShroomiteEnchant>()
            ];
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            SetActive(player);
            player.AddEffect<NatureEffect>(Item);

            // crimson
            player.AddEffect<CrimsonEffect>(Item);
            // molten
            player.AddEffect<MoltenEffect>(Item);
            // rain
            //player.AddEffect<RainUmbrellaEffect>(Item);
            // frost
            player.AddEffect<FrostEffect>(Item);
            // chloro
            //player.AddEffect<ChloroMinion>(Item);
            player.AddEffect<JungleHerbEffect>(Item);
            // shroomite
            player.AddEffect<ShroomiteHealEffect>(Item);
            if (player.HasEffect<ShroomiteHealEffect>())
                player.AddEffect<ShroomiteMushroomPriority>(Item);
            player.AddEffect<ShroomiteShroomEffect>(Item);
        }
        public override void UpdateVanity(Player player)
        {
            player.AddEffect<LightningImmunity>(Item);
        }
        public override void UpdateInventory(Player player)
        {
            player.AddEffect<LightningImmunity>(Item);
        }
        public override void SafeModifyTooltips(List<TooltipLine> tooltips)
        {
            float bonus = 4f * Main.LocalPlayer.statLife / 50;
            int i = tooltips.FindIndex(line => line.Name == "Tooltip4");
            if (i != -1)
                tooltips[i].Text = string.Format(tooltips[i].Text, bonus);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            foreach (int ench in Enchants[Type])
                recipe.AddIngredient(ench);
            recipe.AddTile<CrucibleCosmosSheet>();
            recipe.Register();
        }
    }
    public class NatureEffect : AccessoryEffect
    {
        public override Header ToggleHeader => null;
        //public override int ToggleItemType => ModContent.ItemType<NatureForce>();
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (player.HasBuff<MushroomPowerBuff>())
            {
                modPlayer.AuraSizeBonus += 0.05f;
            }
            else
                modPlayer.AuraSizeBonus -= 0.05f;

            modPlayer.AuraSizeBonus = MathHelper.Clamp(modPlayer.AuraSizeBonus, 0, 0.2f);
        }

    }
}
