using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops;
using FargowiltasSouls.Content.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Weapons.SwarmDrops
{
    public class GeminiGlaives : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Weapons/SwarmDrops", Name);
        private int lastThrown = 0;

        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<OmniscienceStaff>();
        }

        public override void SetDefaults()
        {
            Item.damage = 395;
            Item.DamageType = DamageClass.Melee;
            Item.width = 30;
            Item.height = 30;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.reuseDelay = 25;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3;
            Item.value = Item.sellPrice(0, 25);
            Item.rare = ModContent.RarityType<AbominableRarity>();
            Item.shootSpeed = 20;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.UseSound = null;
            Item.autoReuse = true;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            //if (player.ownedProjectileCounts[ModContent.ProjectileType<Retiglaive>()] > 3)
             //  return false;

            //if (player.ownedProjectileCounts[ModContent.ProjectileType<Spazmaglaive>()] > 3)
           // {
           //     return false;
           // }

            if (player.altFunctionUse == 2)
            {
                Item.shoot = ModContent.ProjectileType<Retiglaive>();
                Item.shootSpeed = 15f;
                Item.UseSound = FargosSoundRegistry.GeminiReti with { Volume = 0.8f };
            }
            else 
            {
                Item.shoot = ModContent.ProjectileType<Spazmaglaive>();
                Item.shootSpeed = 45f;
                Item.UseSound = FargosSoundRegistry.GeminiSpaz with {Volume = 0.8f};
            }
            return true;
        }

        public override bool CanShoot(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<Retiglaive>()] <= 0 || player.ownedProjectileCounts[ModContent.ProjectileType<Spazmaglaive>()] <= 0;

        /*public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (lastThrown != type)
                damage = (int)(damage * 1.5); //additional damage boost for switching
        }*/

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = -1; i <= 1; i++)
            {
                Projectile.NewProjectile(source, position, velocity.RotatedBy(MathHelper.ToRadians(30) * i), type, damage, knockback, player.whoAmI, lastThrown);
            }

            lastThrown = type;
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(null, "TwinRangs")         
            .AddIngredient(ModContent.Find<ModItem>("Fargowiltas", "EnergizerTwins"))
            .AddIngredient(null, "AbomEnergy", 10)
            .AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"))

            .Register();
        }
    }
}