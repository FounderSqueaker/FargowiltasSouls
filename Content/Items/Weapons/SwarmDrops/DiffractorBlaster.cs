using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops;
using FargowiltasSouls.Content.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Weapons.SwarmDrops
{
    [LegacyName("RefractorBlaster2")]     
    public class DiffractorBlaster : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Weapons/SwarmDrops", Name);
        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.LaserRifle);
            Item.width = 98;
            Item.height = 38;
            Item.damage = 580;
            Item.channel = true;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.reuseDelay = 20;
            Item.UseSound = null;
            Item.shootSpeed = 15f;
            Item.value = Item.sellPrice(0, 10);
            Item.rare = ModContent.RarityType<AbominableRarity>();
            Item.shoot = ModContent.ProjectileType<DiffractorBlasterHeld>();
            Item.noUseGraphic = true;
            Item.mana = 18;
            Item.knockBack = 0.5f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(null, "RefractorBlaster")
            .AddIngredient(ModContent.Find<ModItem>("Fargowiltas", "EnergizerPrime"))
            .AddIngredient(null, "AbomEnergy", 10)
            .AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"))

            .Register();
        }
    }
}