using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Weapons.SwarmDrops
{
    public class TwilightTome : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Weapons/SwarmDrops", Name);
        public int timer;
        public bool lastItemUse;
        public override void SetDefaults()
        {
            Item.noMelee = true;
            Item.damage = 600;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 20;
            Item.rare = ItemRarityID.Purple;
            Item.width = 44;
            Item.height = 48;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item8;
            Item.shoot = ModContent.ProjectileType<TwilightStar>();
            Item.shootSpeed = 10f;
            Item.knockBack = 2f;
            Item.autoReuse = true;
            Item.channel = true;

            Item.value = Item.sellPrice(0, 2, 50, 0);
        }

        public override Vector2? HoldoutOffset()
        {
            return null;
        }

        public override bool CanUseItem(Player player)
        {
            bool starCount = Main.projectile.Where(p => p.active && p.type == Item.shoot && p.ai[1] < 4).Count() < 3;
            Item.UseSound = SoundID.Item8 with { Volume = starCount ? 1f : 0f };
            return base.CanUseItem(player) && timer == 0;
        }

        public override bool CanShoot(Player player)
        {
            return Main.projectile.Where(p => p.active && p.type == Item.shoot && p.ai[1] < 4).Count() < 3 && base.CanShoot(player);
        }



        public override void HoldItem(Player player)
        {
            if (timer > 0)
            {
                timer--;
            }
            if (!player.controlUseItem && lastItemUse && timer == 0)
            {
                timer = Item.useTime + 5;
            }
            lastItemUse = player.controlUseItem;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, Item.shoot, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(null, "DarkTome")
            .AddIngredient(ModContent.Find<ModItem>("Fargowiltas", "EnergizerDarkMage"))
            .AddIngredient(ItemID.LunarBar, 10)

            .AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"))

            .Register();
        }
    }
}
