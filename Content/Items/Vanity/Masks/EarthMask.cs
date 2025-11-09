using FargowiltasSouls.Assets.Textures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Vanity.Masks
{
    [AutoloadEquip(EquipType.Head)]
    public class EarthMask : ModItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Vanity/Masks", Name);
        public override void SetStaticDefaults()
        {     
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.rare = ItemRarityID.Blue;
            Item.vanity = true;
        }
    }
}