using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Items.BossBags;
using FargowiltasSouls.Content.Projectiles.Weapons.ChallengerItems;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Weapons.Challengers
{
    public class SpiritLongbow : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Weapons/Challengers", Name);
        private int delay = 0;
        private bool lastLMouse = false;

        public static readonly SoundStyle PullSound = new SoundStyle($"FargowiltasSouls/Assets/Sounds/Weapons/BowPull") with { Volume = 1f };
        public static readonly SoundStyle ReleaseSound = new SoundStyle($"FargowiltasSouls/Assets/Sounds/Weapons/BowRelease") with { Volume = 1f };
        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 23;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 66;
            Item.height = 30;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 0.5f;
            Item.value = Item.sellPrice(0, 2);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = PullSound with { Pitch = -0.5f };
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SpiritBow>();
            Item.useAmmo = AmmoID.Arrow;
            Item.noMelee = true;
        }

        private float Charge = 1;
        
        public override void HoldItem(Player player)
        {
            if (lastLMouse && !Main.mouseLeft && CanUseItem(player) && FargoSoulsUtil.ActuallyClickingInGameplay(player))
            {
                delay = (int)MathF.Ceiling(30f / player.FargoSouls().AttackSpeed);
            }
            if (delay-- < 0)
            {
                delay = 0;
            }
            if (player.channel)
            {
                if (Charge < 4) {
                    Charge += 1 / 30f;
                }
            }
            else
            {
                Charge = 1;
            }
            lastLMouse = Main.mouseLeft;

            if (player.channel && player.ownedProjectileCounts[Item.shoot] < 1 && delay == 0)
            {
                Projectile.NewProjectile(player.GetSource_ItemUse_WithPotentialAmmo(Item, -1), player.Center, Vector2.Zero, Item.shoot, (int)(player.ActualClassDamage(DamageClass.Ranged) * Item.damage), Item.knockBack, player.whoAmI);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false; // projectile is manually spawned in HoldItem
        }

        public override bool CanUseItem(Player player)
        {
            return delay <= 0 && base.CanUseItem(player);

        }
       
        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient<CursedCoffinBag>(2).AddTile(TileID.Solidifier).DisableDecraft().Register();
        }
    }
}