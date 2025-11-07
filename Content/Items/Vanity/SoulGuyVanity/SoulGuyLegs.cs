using FargowiltasSouls.Assets.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Vanity.SoulGuyVanity
{
    [AutoloadEquip(EquipType.Legs)]
    public class SoulGuyLegs : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Vanity/SoulGuyVanity", Name);
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ArmorIDs.Legs.Sets.HidesBottomSkin[EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs)] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 12;
            Item.rare = ItemRarityID.Cyan;
            Item.vanity = true;
        }
    }
}
