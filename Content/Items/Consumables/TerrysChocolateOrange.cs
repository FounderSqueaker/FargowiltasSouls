using FargowiltasSouls.Assets.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace FargowiltasSouls.Content.Items.Consumables
{
    public class TerrysChocolateOrange : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Consumables", Name);

        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
        }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 36;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.Orange;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useAnimation = 34;
            Item.useTime = 34;
            Item.consumable = true;
            Item.UseSound = new SoundStyle("FargowiltasSouls/Assets/Sounds/Chippy/chippy") with { Variants = [1, 2, 3, 4, 5], PitchVariance = 1f};
            Item.value = Item.sellPrice(0, 0, 10, 0);
        }

        public override bool? UseItem(Player player)
        {
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                player.AddBuff(BuffID.WellFed3, 36000);
            }
            return true;
        }
    }
}
