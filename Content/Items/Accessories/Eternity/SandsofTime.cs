using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Items.Accessories.Souls;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace FargowiltasSouls.Content.Items.Accessories.Eternity
{
    public class SandsofTime : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Accessories/Eternity", Name);
        public override bool Eternity => true;

        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 4);

            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.autoReuse = true;
            Item.channel = true;

            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTurn = true;
            Item.UseSound = SoundID.DD2_BetsyFlameBreath with { MaxInstances = 1, SoundLimitBehavior = Terraria.Audio.SoundLimitBehavior.IgnoreNew, Pitch = -1f, Volume = 2f };
        }

        public static void PassiveEffects(Player player)
        {
            player.buffImmune[BuffID.WindPushed] = true;
            player.FargoSouls().SandsofTime = true;
        }
        public static void ActiveEffects(Player player) => PassiveEffects(player);

        public override void UpdateInventory(Player player) => PassiveEffects(player);
        public override void UpdateVanity(Player player) => PassiveEffects(player);
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.04f;
            ActiveEffects(player);
        }
        public static float MaxTime(FargoSoulsPlayer modPlayer) => modPlayer.MasochistSoul ? 60 : 60 * 5;
        public static void Use(Player player)
        {
            var modPlayer = player.FargoSouls();
            modPlayer.SandsOfTimeChannel++;
            float maxTime = MaxTime(modPlayer);
            float progress = modPlayer.SandsOfTimeChannel / maxTime;
            float vel = 2f * progress;
            int dustAmt = (int)(10 * progress);
            if (modPlayer.SandsOfTimeChannel >= maxTime)
            {
                player.FargoSouls().SandsOfTimePosition = player.Center;
                SoundEngine.PlaySound(SoundID.Item4, player.Center);
                modPlayer.SandsOfTimeChannel = -120;
                vel = 8f;
                dustAmt = 70;

            }

            for (int index = 0; index < dustAmt; ++index)
            {
                int d = Dust.NewDust(player.position, player.width, player.height, DustID.GemTopaz, 0.0f, 0.0f, 150, new Color(), 1.5f);
                Main.dust[d].velocity *= vel;
                Main.dust[d].noGravity = true;
            }
        }
        public static bool CanUse(Player player)
        {
            var modPlayer = player.FargoSouls();
            if (modPlayer.SandsOfTimeChannel >= MaxTime(modPlayer))
            {
                player.FargoSouls().SandsOfTimeChannel = -120;
                return false;
            }
            return modPlayer.SandsOfTimeChannel >= 0;
        }
        public static readonly int[] DungeonWalls =
        [
            WallID.BlueDungeon, WallID.BlueDungeonSlab, WallID.BlueDungeonSlabUnsafe, WallID.BlueDungeonTile, WallID.BlueDungeonTileUnsafe, WallID.BlueDungeonUnsafe, WallID.GreenDungeon, WallID.GreenDungeonSlab, WallID.GreenDungeonSlabUnsafe,
            WallID.GreenDungeonTile, WallID.GreenDungeonTileUnsafe, WallID.GreenDungeonUnsafe, WallID.PinkDungeon, WallID.PinkDungeonSlab, WallID.PinkDungeonSlabUnsafe, WallID.PinkDungeonTile, WallID.PinkDungeonTileUnsafe, WallID.PinkDungeonUnsafe
        ];
        public override bool CanUseItem(Player player) => CanUse(player);
        public override bool? UseItem(Player player)
        {
            return base.UseItem(player);
        }
        public override void UseItemFrame(Player player) => Use(player);
    }
    public class SandsWarpMapMarker : ModMapLayer
    {
        public static bool MouseOver = false;

        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            if (!Main.mapFullscreen)
                return;
            var player = Main.LocalPlayer;
            var modPlayer = player.FargoSouls();
            if (modPlayer.SandsOfTimePosition == Vector2.Zero)
                return;

            float scaleIfNotSelected = 1f;
            float scaleIfSelected = 1.15f;

            var icon = FargoAssets.GetTexture2D("Content/Items/Accessories/Eternity", "SandsofTime").Value;
            if (context.Draw(icon, modPlayer.SandsOfTimePosition / 16, Color.White, new(1, 1, 0, 0), scaleIfNotSelected, scaleIfSelected, Alignment.Center).IsMouseOver)
            {
                if (!MouseOver)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    MouseOver = true;
                }

                text = Language.GetTextValue("Game.TeleportTo", ContentSamples.ItemsByType[ModContent.ItemType<SandsofTime>()].Name);
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.mouseLeftRelease = false;
                    Main.mapFullscreen = false;

                    Vector2 tpPos = player.FargoSouls().SandsOfTimePosition;

                    player.immune = true;
                    player.immuneTime = 20;
                    for (int index = 0; index < 70; ++index)
                    {
                        int d = Dust.NewDust(player.position, player.width, player.height, DustID.GemTopaz, player.velocity.X * 0.5f, player.velocity.Y * 0.5f, 150, new Color(), 1.5f);
                        Main.dust[d].velocity *= 4f;
                        Main.dust[d].noGravity = true;
                    }

                    player.grappling[0] = -1;
                    player.grapCount = 0;
                    for (int index = 0; index < Main.maxProjectiles; ++index)
                    {
                        if (Main.projectile[index].active && Main.projectile[index].owner == player.whoAmI && Main.projectile[index].aiStyle == ProjAIStyleID.Hook)
                            Main.projectile[index].Kill();
                    }

                    if (player.whoAmI == Main.myPlayer)
                    {
                        Vector2 teleport = tpPos;
                        if (SandsofTime.DungeonWalls.Contains(Framing.GetTileSafely(tpPos).WallType) && !NPC.downedBoss3)
                            teleport = new Vector2(Main.dungeonX * 16 + 8, Main.dungeonY * 16 - 16 * 3); //dungeon entrance

                        player.Teleport(teleport, 1);
                        player.velocity = Vector2.Zero;
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, tpPos.X, tpPos.Y, 1);
                    }

                    for (int index = 0; index < 70; ++index)
                    {
                        int d = Dust.NewDust(player.position, player.width, player.height, DustID.GemTopaz, 0.0f, 0.0f, 150, new Color(), 1.5f);
                        Main.dust[d].velocity *= 4f;
                        Main.dust[d].noGravity = true;
                    }
                }
            }

            else
                MouseOver = false;
        }
    }
}