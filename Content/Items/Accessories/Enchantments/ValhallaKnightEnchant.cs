using Fargowiltas.Common.Configs;
using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core.Toggler.Content;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class ValhallaKnightEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(147, 101, 30);

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Yellow;
            Item.value = 250000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.FargoSouls().ValhallaEnchantActive = true;
            player.AddEffect<ValhallaDashEffect>(Item);
            SquireEnchant.SquireEffect(player, Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.SquireAltHead)
            .AddIngredient(ItemID.SquireAltShirt)
            .AddIngredient(ItemID.SquireAltPants)
            .AddIngredient(null, "SquireEnchant")
            .AddIngredient(ItemID.DD2BallistraTowerT3Popper)
            //.AddIngredient(ItemID.VikingHelmet)
            .AddIngredient(ItemID.ShadowJoustingLance)

                .AddTile<EnchantedTreeSheet>()
            .Register();
        }
    }

    public class ValhallaDashEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<WillHeader>();
        public override int ToggleItemType => ModContent.ItemType<ValhallaKnightEnchant>();
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (player.mount.Active)
            {
                if (player.FargoSouls().IsDashingTimer % 2 == 0 && player.FargoSouls().IsDashingTimer != 0)
                {
                    DashParticles(player);
                }
                if (modPlayer.ValhallaVerticalDashing != 0)
                {
                    if (modPlayer.ValhallaVerticalDashing % 2 == 0)
                        DashParticles(player);
                    player.eocDash = Math.Abs(modPlayer.ValhallaVerticalDashing);
                    player.armorEffectDrawShadowEOCShield = true;
                    if (modPlayer.ValhallaVerticalDashing > 0)
                        modPlayer.ValhallaVerticalDashing--;
                    else modPlayer.ValhallaVerticalDashing++;
                } //i made the int count like this so movement stats could be edited when dashing down specifically but that didnt work out so its here now
            }     //so TODO: make downwards dash ignore mounts max fall speed
        }
        public static void AddDash(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (player.mount.Active)
            {
                modPlayer.FargoDash = DashManager.DashType.Valhalla;
                modPlayer.HasDash = true;
                if (player.dashDelay == 0 && modPlayer.DashCD == 0 && modPlayer.IsDashingTimer == 0)
                {
                    if (Fargowiltas.Fargowiltas.DashKey.Current)
                    {
                        if (player.controlUp)
                        {
                            VerticalDash(player, -1);
                        }
                        else if (player.controlDown)
                        {
                            VerticalDash(player, 1);
                        }
                    }
                    else if (!ModContent.GetInstance<FargoClientConfig>().DoubleTapDashDisabled)
                    {
                        if (player.controlUp && player.releaseUp)
                        {
                            if (player.doubleTapCardinalTimer[1] > 0 && player.doubleTapCardinalTimer[1] != 15)
                            {
                                VerticalDash(player, -1);
                            }
                        }
                        else if (player.controlDown && player.releaseDown)
                        {
                            if (player.doubleTapCardinalTimer[0] > 0 && player.doubleTapCardinalTimer[0] != 15)
                            {
                                VerticalDash(player, 1);
                            }
                        }
                    }
                    if (modPlayer.FallthroughTimer < player.FargoSouls().IsDashingTimer)
                        modPlayer.FallthroughTimer = player.FargoSouls().IsDashingTimer;

                    if (modPlayer.FallthroughTimer < player.FargoSouls().ValhallaVerticalDashing)
                        modPlayer.FallthroughTimer = player.FargoSouls().ValhallaVerticalDashing;
                }

                if (player.FargoSouls().ValhallaVerticalDashing != 0)
                    player.maxFallSpeed = 16;
            }
        }
        public static void ValhallaDash(Player player, int direction)
        {
            player.velocity.X = 16 * direction;
            if (player.FargoSouls().IsDashingTimer < 20)
                player.FargoSouls().IsDashingTimer = 20;
            player.dashDelay = 60;
            player.FargoSouls().DashCD = 60;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);
        }
        public static void VerticalDash(Player player, int direction)
        {
            player.velocity = Vector2.Zero;
            player.velocity.Y = 16 * direction;
            player.FargoSouls().ValhallaVerticalDashing = 20 * direction;
            player.dashDelay = 60;
            player.FargoSouls().DashCD = 60;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);
        }

        public static void DashParticles(Player player)
        {
            //TODO: make particle spawn rectangle encompass mount size instead of player size
            if (player.velocity != Vector2.Zero)
            {
                //Vector2 mountsize = new Vector2(Main.rand.Next(0, player.mount._data.textureWidth + player.mount._data.xOffset), Main.rand.Next(0, player.mount._data.textureHeight + player.mount._data.yOffset))
                Vector2 spawnrectangle = new Vector2(Main.rand.Next(0, player.width), Main.rand.Next(0, player.height));
                Particle spark = new SparkParticle(player.position + spawnrectangle, -player.velocity * 0.6f, Color.Black, 1, 100);
                spark.Spawn();
            }
        }
    }
}
