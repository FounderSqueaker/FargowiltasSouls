using Fargowiltas.Content.Items.Summons.SwarmSummons.Energizers;
using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Items.Materials;
using FargowiltasSouls.Content.Items.Weapons.BossDrops;
using FargowiltasSouls.Content.Projectiles.Weapons.Minions;
using FargowiltasSouls.Content.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Weapons.SwarmDrops
{
    [LegacyName("DestroyerGun2")]
    public class DestructionCannon : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Weapons/SwarmDrops", Name);
        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<TheDestroyer>();
        }

        public override void SetDefaults()
        {
            Item.damage = 275;
            Item.mana = 30;
            Item.DamageType = DamageClass.Summon;
            Item.width = 126;
            Item.height = 38;
            Item.useAnimation = 70;
            Item.useTime = 70;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.UseSound = SoundID.NPCDeath13;
            Item.value = Item.sellPrice(0, 25);
            Item.rare = ModContent.RarityType<AbominableRarity>();
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<DestroyerHead2>();
            Item.shootSpeed = 18f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            FargoSoulsUtil.NewSummonProjectile(source, position, velocity, type, Item.damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient<DestroyerGun>()
            .AddIngredient<EnergizerDestroy>()
            .AddIngredient<AbomEnergy>(10)
            .AddTile<CrucibleCosmosSheet>()
            .Register();
        }
    }
}