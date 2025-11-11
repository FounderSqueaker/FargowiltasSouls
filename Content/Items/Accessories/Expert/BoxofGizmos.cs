using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Items.Accessories.Souls;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.Items.Accessories.Expert
{
    public class BoxofGizmos : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Accessories/Expert", Name);
        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        static (int, string) GetItemTuple(int itemType) => (itemType, ModContent.GetModItem(itemType).Name);

        // only put modded items in this
        public static (int, string)[] GetStorableItems()
        {
            return new (int, string)[]
            {
                GetItemTuple(ModContent.ItemType<IronEnchant>()),
                GetItemTuple(ModContent.ItemType<RainEnchant>()),
                GetItemTuple(ModContent.ItemType<WoodEnchant>()),
                GetItemTuple(ModContent.ItemType<TerraForce>()),
                GetItemTuple(ModContent.ItemType<TimberForce>()),
                GetItemTuple(ModContent.ItemType<LifeForce>()),
                GetItemTuple(ModContent.ItemType<NatureForce>()),
                GetItemTuple(ModContent.ItemType<BionomicCluster>()),
                GetItemTuple(ModContent.ItemType<LithosphericCluster>()),
                GetItemTuple(ModContent.ItemType<FrigidGrasp>()),
                GetItemTuple(ModContent.ItemType<ChaliceofTheMoon>()),
                GetItemTuple(ModContent.ItemType<HeartoftheMasochist>()),
                GetItemTuple(ModContent.ItemType<MysticSkull>()),
                GetItemTuple(ModContent.ItemType<NymphsPerfume>()),
                GetItemTuple(ModContent.ItemType<SandsofTime>()),
                GetItemTuple(ModContent.ItemType<SaucerControlConsole>()),
                GetItemTuple(ModContent.ItemType<SecurityWallet>()),
                GetItemTuple(ModContent.ItemType<WyvernFeather>()),
                GetItemTuple(ModContent.ItemType<MasochistSoul>()),
                GetItemTuple(ModContent.ItemType<SquirrelCharm>()),
                GetItemTuple(ModContent.Find<ModItem>("Fargowiltas", "AutoSummoner").Type),
            };
        }

        public List<Item> storedItems = new List<Item>();

        public override void SaveData(TagCompound tag)
        {
            foreach (Item storedItem in storedItems)
            {
                List<int> data = new List<int>();
                data.Add(storedItem.stack);
                data.Add(storedItem.prefix);
                tag.Add(storedItem.ModItem.Name, data);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            foreach ((int itemType, string itemName) in GetStorableItems())
            {
                if (tag.ContainsKey(itemName))
                {
                    var data = tag.GetList<int>(itemName);
                    if (data.Count < 2)
                        continue;
                    int stack = data[0];
                    int prefix = data[1];
                    Item item = new Item(itemType, stack, prefix);
                    storedItems.Add(item);
                }
            }
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            //Item.accessory = true;
            Item.rare = ItemRarityID.Expert;
            Item.value = Item.sellPrice(0, 1);

            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.useTurn = true;

            Item.expert = true;
        }
        

        public override bool AltFunctionUse(Player player) => true;

        void BoxItems(Player player)
        {
            var storableItems = GetStorableItems();
            foreach (Item item in player.inventory)
            {
                foreach ((int itemType, string itemName) in storableItems)
                {
                    if (itemType == item.type && !item.favorited && !storedItems.Any(storedItem => storedItem.type == item.type))
                    {
                        storedItems.Add(item.Clone());
                        item.TurnToAir(true);
                        break;
                    }
                }
            }
        }

        void UnboxItems(Player player)
        {
            if (storedItems.Count > 0)
            {
                foreach (Item item in storedItems)
                    Item.NewItem(player.GetSource_FromThis(), player.Center, item);
                storedItems.Clear();
            }
        }
        
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
                UnboxItems(player);
            else
                BoxItems(player);

            return true;
        }

        public override bool CanRightClick() => true;

        public override bool ConsumeItem(Player player) => false;

        public override void RightClick(Player player)
        {
            UnboxItems(player);
        }

        public override void SafeModifyTooltips(List<TooltipLine> tooltips)
        {
            base.SafeModifyTooltips(tooltips);

            if (storedItems.Count > 0)
            {
                TooltipLine line = new(Mod, "tooltip", Language.GetTextValue($"Mods.{Mod.Name}.Items.{Name}.Contains"));
                tooltips.Add(line);

                foreach (Item storedItem in storedItems)
                {
                    TooltipLine storedItemLine = new(Mod, $"tooltip{storedItem.ModItem.Name}", $"    {storedItem.Name}");
                    tooltips.Add(storedItemLine);
                }
            }
        }

        void PassiveEffect(Player player)
        {
            foreach (Item storedItem in storedItems)
            {
                if (storedItem.ModItem != null)
                    storedItem.ModItem.UpdateInventory(player);
            }
        }

        public override void UpdateInventory(Player player) => PassiveEffect(player);
        public override void UpdateVanity(Player player) => PassiveEffect(player);
        public override void UpdateAccessory(Player player, bool hideVisual) => PassiveEffect(player);
    }

    public class GizmoGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => false;

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(item, tooltips);

            foreach ((int itemType, string itemName) in BoxofGizmos.GetStorableItems())
            {
                if (itemType == item.type)
                {
                    TooltipLine line = new(Mod, "gizmo", Language.GetTextValue($"Mods.{Mod.Name}.Items.BoxofGizmos.Storable"));
                    tooltips.Add(line);
                    break;
                }
            }
        }
    }
}
