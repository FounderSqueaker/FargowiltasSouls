using FargowiltasSouls.Content.Bosses.CursedCoffin;
using FargowiltasSouls.Content.Items.Accessories;
using FargowiltasSouls.Content.Items.Accessories.Expert;
using FargowiltasSouls.Content.Items.Vanity.Masks;
using FargowiltasSouls.Content.Items.Weapons.Challengers;
using System.Data;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.BossBags
{
    public class CursedCoffinBag : BossBag
    {
        protected override bool IsPreHMBag => true; //so it doesn't drop dev sets
        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<CoffinMask>(), 7));
            //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<SoulLantern>()));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<AccursedAnkh>()));
            itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<CursedCoffin>()));

            itemLoot.Add(ItemDropRule.OneFromOptions(1, ModContent.ItemType<SisypheanFist>(), ModContent.ItemType<SpiritLongbow>(), ModContent.ItemType<GildedSceptre>(), ModContent.ItemType<EgyptianFlail>()));

            // gems
            itemLoot.Add(ItemDropRule.Common(ItemID.Amethyst, 3, 2, 4));
            itemLoot.Add(ItemDropRule.Common(ItemID.Topaz, 4, 2, 4));
            itemLoot.Add(ItemDropRule.Common(ItemID.Sapphire, 4, 2, 3));
            itemLoot.Add(ItemDropRule.Common(ItemID.Emerald, 5, 1, 3));
            itemLoot.Add(ItemDropRule.Common(ItemID.Ruby, 5, 1, 2));
            itemLoot.Add(ItemDropRule.Common(ItemID.Amber, 3, 2, 6));
            itemLoot.Add(ItemDropRule.Common(ItemID.Diamond, 7, 1, 1));
            itemLoot.Add(ItemDropRule.Common(ItemID.PharaohsMask, 4));
            itemLoot.Add(ItemDropRule.Common(ItemID.PharaohsRobe, 4));
        }
    }
}