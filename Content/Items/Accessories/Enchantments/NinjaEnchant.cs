using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class NinjaEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(48, 49, 52);


        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Blue;
            Item.value = 30000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<NinjaEffect>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.NinjaHood)
                .AddIngredient(ItemID.NinjaShirt)
                .AddIngredient(ItemID.NinjaPants)
                .AddIngredient(ItemID.Gi)
                .AddIngredient(ItemID.Shuriken, 100)
                .AddIngredient(ItemID.ThrowingKnife, 100)

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
    }
    public class NinjaEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<ShadowHeader>();
        public override int ToggleItemType => ModContent.ItemType<NinjaEnchant>();
        public static bool PlayerCanHaveBuff(Player player)
        {
            int maxSpeedToAllow = player.ForceEffect<NinjaEffect>() ? 6 : 3;
            return player.velocity.Length() < maxSpeedToAllow;
        }
        public override void PostUpdateEquips(Player player)
        {
            if (PlayerCanHaveBuff(player))
            {
                var modPlayer = player.FargoSouls();
                if (modPlayer.NinjaCounter < 5)
                    modPlayer.NinjaCounter += 5f / 60;
                else
                    modPlayer.NinjaCounter = 5;
            }
            if (player.whoAmI == Main.myPlayer)
                CooldownBarManager.Activate("NinjaEnchantStealth", FargoAssets.GetTexture2D("Content/Items/Accessories/Enchantments", "NinjaEnchant").Value, Color.LightGray,
                    () => (int)(Main.LocalPlayer.FargoSouls().NinjaCounter) / 5f, true, activeFunction: player.HasEffect<NinjaEffect>);
        }
        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            var modPlayer = player.FargoSouls();
            if (modPlayer.NinjaCounter >= 1 && modPlayer.NinjaDecrementCD <= 0)
            {
                modPlayer.NinjaCounter--;
                modPlayer.NinjaDecrementCD = FargoSoulsPlayer.NinjaDecrementMaxCD;
            }
        }
        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            var modPlayer = player.FargoSouls();
            if (proj.FargoSouls().IsAHeldProj && modPlayer.NinjaCounter >= 1 && modPlayer.NinjaDecrementCD <= 0)
            {
                modPlayer.NinjaCounter--;
                modPlayer.NinjaDecrementCD = FargoSoulsPlayer.NinjaDecrementMaxCD;
            }
        }
    }
}
