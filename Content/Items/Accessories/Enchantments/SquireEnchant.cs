using Fargowiltas.Common.Configs;
using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class SquireEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(148, 143, 140);

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.LightPurple;
            Item.value = 150000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            SquireEffect(player, Item);
        }

        public static void SquireEffect(Player player, Item item)
        {
            player.AddEffect<SquireMountSpeed>(item);
            player.AddEffect<SquireMountJump>(item);
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            modPlayer.SquireEnchantItem = item;

            //player.buffImmune[BuffID.BallistaPanic] = true;

            Mount mount = player.mount;

            if (mount.Active)
            {
                if (modPlayer.BaseMountType != mount.Type)
                {
                    //we want to reset the prev mount first or all hell breaks loose
                    if (modPlayer.BaseMountType != -1)
                    {
                        ResetMountStats(modPlayer);
                    }

                    Mount.MountData original = Mount.mounts[mount.Type];
                    //copy over ANYTHING that will be changed
                    modPlayer.BaseSquireMountData = new Mount.MountData
                    {
                        acceleration = original.acceleration,
                        dashSpeed = original.dashSpeed,
                        fallDamage = original.fallDamage,

                        jumpSpeed = original.jumpSpeed,
                        //usesHover = original.usesHover
                    };

                    modPlayer.BaseMountType = mount.Type;
                }
                float accelBoost;
                float speedBoost;

                if (modPlayer.ValhallaEnchantActive && modPlayer.ForceEffect<ValhallaKnightEnchant>())
                {
                    accelBoost = 2f;
                    speedBoost = 1.5f;
                }
                else if (modPlayer.ValhallaEnchantActive || modPlayer.ForceEffect<SquireEnchant>())
                {
                    accelBoost = 1.5f;
                    speedBoost = 1.5f;
                }
                else
                {
                    accelBoost = 1.25f;
                    speedBoost = 1.25f;
                }


                if (!player.HasEffect<SquireMountSpeed>())
                {
                    accelBoost = 1;
                    speedBoost = 1;
                }

                if (player.HasEffect<SquireMountJump>())
                {
                    player.GetJumpState(ExtraJump.BasiliskMount).Enable();
                }

                mount._data.acceleration = modPlayer.BaseSquireMountData.acceleration * accelBoost;
                mount._data.dashSpeed = modPlayer.BaseSquireMountData.dashSpeed * speedBoost;
                mount._data.jumpSpeed = modPlayer.BaseSquireMountData.jumpSpeed * speedBoost;
                mount._data.fallDamage = 0;
                player.noFallDmg = true;

                //Main.NewText(mount.DashSpeed);
            }
        }

        public static void ResetMountStats(FargoSoulsPlayer modPlayer)
        {
            if (modPlayer.BaseSquireMountData == null || modPlayer.BaseMountType < 0 || modPlayer.BaseMountType >= Mount.mounts.Length)
            {
                return;
            }

            Mount.mounts[modPlayer.BaseMountType].acceleration = modPlayer.BaseSquireMountData.acceleration;
            Mount.mounts[modPlayer.BaseMountType].dashSpeed = modPlayer.BaseSquireMountData.dashSpeed;
            Mount.mounts[modPlayer.BaseMountType].fallDamage = modPlayer.BaseSquireMountData.fallDamage;

            Mount.mounts[modPlayer.BaseMountType].jumpSpeed = modPlayer.BaseSquireMountData.jumpSpeed;
            //Mount.mounts[modPlayer.BaseMountType].usesHover = modPlayer.BaseSquireMountData.usesHover;
            modPlayer.BaseMountType = -1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.SquireGreatHelm)
            .AddIngredient(ItemID.SquirePlating)
            .AddIngredient(ItemID.SquireGreaves)
            .AddIngredient(ItemID.JoustingLance)
            .AddIngredient(ItemID.HamBat)
            .AddIngredient(ItemID.MajesticHorseSaddle)

            .AddTile<EnchantedTreeSheet>()
            .Register();
        }
    }
    public class SquireMountSpeed : AccessoryEffect
    {

        public override Header ToggleHeader => Header.GetHeader<WillHeader>();

        public override int ToggleItemType => ModContent.ItemType<SquireEnchant>();
    }
    public class SquireMountJump : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<WillHeader>();
        public override int ToggleItemType => ModContent.ItemType<SquireEnchant>();
    }
}
