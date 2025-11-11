using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Consumables;
using FargowiltasSouls.Content.Items.Materials;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Eternity
{
    public class BionomicCluster : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Accessories/Eternity", Name);
        public override bool Eternity => true;
        public override List<AccessoryEffect> ActiveSkillTooltips => 
            [AccessoryEffectLoader.GetEffect<FrigidGraspKeyEffect>(), AccessoryEffectLoader.GetEffect<TimsInspectEffect>()];

        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 6);
            Item.defense = 3;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.autoReuse = true;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTurn = true;
            Item.UseSound = SoundID.DD2_BetsyFlameBreath with { MaxInstances = 1, SoundLimitBehavior = Terraria.Audio.SoundLimitBehavior.IgnoreNew, Pitch = -1f, Volume = 2f };
        }

        public static void PassiveEffect(Player player, Item item)
        {
            //FrigidGrasp.PassiveEffects(player, item);
            SandsofTime.PassiveEffects(player);
            //SqueakyToy.PassiveEffects(player, item);
            NymphsPerfume.PassiveEffects(player, item);
            //TimsConcoction.PassiveEffects(player, item);
        }
        public static void ActiveEffect(Player player, Item item)
        {
            PassiveEffect(player, item);
            FrigidGrasp.ActiveEffects(player, item);
            //SandsofTime.ActiveEffects(player, Item);
            SqueakyToy.ActiveEffects(player, item);
            NymphsPerfume.ActiveEffects(player, item);
            TimsConcoction.ActiveEffects(player, item);
        }

        public override void UpdateInventory(Player player) => PassiveEffect(player, Item);
        public override void UpdateVanity(Player player) => PassiveEffect(player, Item);

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ActiveEffect(player, Item);
            player.AddEffect<FrigidGraspKeyEffect>(Item);
        }
        public override bool CanUseItem(Player player) => SandsofTime.CanUse(player);
        public override bool? UseItem(Player player) => true;
        public override void UseItemFrame(Player player) => SandsofTime.Use(player);

        public override void AddRecipes()
        {
            CreateRecipe()

            .AddIngredient<FrigidGrasp>()
            .AddIngredient<SandsofTime>()
            .AddIngredient<SqueakyToy>()
            .AddIngredient<NymphsPerfume>()
            .AddIngredient<TimsConcoction>()
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.Glass, 10)
            .AddIngredient<DeviatingEnergy>(10)

            .AddTile(TileID.Anvils)
            .DisableDecraft()
            .Register();
        }
    }
}
