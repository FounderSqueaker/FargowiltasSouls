using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Eternity
{
    [AutoloadEquip(EquipType.Shield)]
    public class DreadShell : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Accessories/Eternity", Name);
        public override bool Eternity => true;
        public override List<AccessoryEffect> ActiveSkillTooltips =>
            [AccessoryEffectLoader.GetEffect<DreadShellEffect>()];
        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(0, 4);
            Item.defense = 2;
        }
        public static void ActiveEffects(Player player, Item item)
        {
            player.noKnockback = true;
            player.buffImmune[ModContent.BuffType<AnticoagulationBuff>()] = true;
            player.AddEffect<DreadShellEffect>(item);
            player.AddEffect<ParryEffect>(item);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ActiveEffects(player, Item);
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Melee;
            tooltipColor = null;
            scaling = null;
            return DreadShellEffect.BaseDamage(Main.LocalPlayer) * 2; //guaranteed crit so displays dmg as double
        }
    }
    public class DreadShellEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<LithosphericHeader>();
        public override int ToggleItemType => ModContent.ItemType<DreadShell>();
        public static int BaseDamage(Player player) => (int)(300 * player.ActualClassDamage(DamageClass.Melee));
    }
}