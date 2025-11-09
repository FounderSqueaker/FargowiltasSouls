using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using FargowiltasSouls.Content.Items.Materials;
using Fargowiltas.Content.Items.Summons.SwarmSummons.Energizers;
using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Content.Items.Weapons.BossDrops;
using FargowiltasSouls.Content.Rarities;
using FargowiltasSouls.Content.Projectiles.Weapons.Minions;
using FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops;
using FargowiltasSouls.Assets.Textures;

namespace FargowiltasSouls.Content.Items.Weapons.SwarmDrops
{
    public class TheDestroyer : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Weapons/SwarmDrops", Name);
        public int Swings = 0;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<DestructionCannon>();
        }
        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<TheDestroyerProj>(), 1110, 20, 9, 60);
            Item.rare = ModContent.RarityType<AbominableRarity>();
            Item.value = Item.sellPrice(0, 25);
        }

        public override bool MeleePrefix()
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<DestroyerHeadWhip>()] > 0)
                return false;
            if (Swings >= 20)
            {
                Item.UseSound = SoundID.Item61 with { MaxInstances = 0, Pitch = -0.5f };
                return base.CanUseItem(player);
            }
            Item.UseSound = SoundID.Item152;
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Swings >= 20)
            {
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<DestroyerHeadWhip>(), damage * 3, knockback);
                Swings = 0;
                return false;
            }
            Swings++;
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient<ElectricWhip>()
            .AddIngredient<EnergizerDestroy>()
            .AddIngredient<AbomEnergy>(10)
            .AddTile<CrucibleCosmosSheet>()
            .Register();
        }
    }
}
