using System.Collections.Generic;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Souls
{
    //[AutoloadEquip(EquipType.Back)]
    public class WorldShaperSoul : BaseSoul
    {
        public override List<AccessoryEffect> ActiveSkillTooltips => [AccessoryEffectLoader.GetEffect<DCUEffect>()];
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 36));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.value = 750000;
        }
        public static readonly Color ItemColor = new(255, 239, 2);
        protected override Color? nameColor => ItemColor;

        public override void UpdateInventory(Player player)
        {
            //hand of creation
            player.autoPaint = true;
            //spectre goggles
            player.CanSeeInvisibleBlocks = true;
            //fpv sight
            player.remoteVisionForDrone = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            AddEffects(player, Item, hideVisual);
        }
        public static void AddEffects(Player player, Item item, bool hideVisual)
        {
            player.FargoSouls().WorldShaperSoul = true;
            //mining speed, spelunker, dangersense, light, hunter
            MinerEnchant.AddEffects(player, .75f, item);
            //hand of creation
            player.autoPaint = true;
            player.equippedAnyWallSpeedAcc = true;
            player.equippedAnyTileSpeedAcc = true;
            player.autoPaint = true;
            player.equippedAnyTileRangeAcc = true;
            player.treasureMagnet = true;
            player.chiselSpeed = true;
            player.portableStoolInfo.SetStats(26, 26, 26);
            //placing speed up MORE
            player.tileSpeed += 0.5f;
            player.wallSpeed += 0.5f;
            //toolbox
            if (player.whoAmI == Main.myPlayer)
            {
                Player.tileRangeX += 10;
                Player.tileRangeY += 10;
            }
            //presserator
            player.autoActuator = true;
            //hand of creation
            player.autoPaint = true;
            //spectre goggles
            player.CanSeeInvisibleBlocks = true;
            //fpv sight
            player.remoteVisionForDrone = true;
            //dcu
            player.AddEffect<DCUEffect>(item);

            player.AddEffect<BuilderEffect>(item);
        }
        public override void UpdateItemDye(Player player, int dye, bool hideVisual)
        {
            player.cPortableStool = dye; //zero clue how this works but vanilla does this so may as well throw here
        }

        public override void AddRecipes()
        {
            CreateRecipe()

            .AddIngredient(null, "MinerEnchant")
            .AddIngredient(ItemID.HandOfCreation)
            .AddIngredient(ItemID.Toolbelt)
            .AddIngredient(ItemID.Toolbox)
            .AddIngredient(ItemID.ActuationAccessory)
            .AddIngredient(ItemID.SpectreGoggles)
            .AddIngredient(ItemID.JimsDroneVisor)
            .AddRecipeGroup("FargowiltasSouls:AnyDrax")
            .AddIngredient(ItemID.DrillContainmentUnit)
            //.AddRecipeGroup("FargowiltasSouls:AnyShellphone")
            //.AddIngredient(ItemID.ShroomiteDiggingClaw)

            .AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"))


            .Register();
        }
    }
    public class BuilderEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<WorldShaperHeader>();
        public override int ToggleItemType => ModContent.ItemType<WorldShaperSoul>();
        public override void PostUpdateEquips(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return;
            player.FargoSouls().BuilderMode = true;
            //if (Main.netMode == NetmodeID.MultiplayerClient) NetMessage.SendData(MessageID.SyncPlayer, number: Player.whoAmI);

            /*for (int i = 0; i < TileLoader.TileCount; i++)
            {
                player.adjTile[i] = true;
            }*/

            //toolbox
            if (player.HeldItem.createWall == 0) //tiles
            {
                Player.tileRangeX += 60;
                Player.tileRangeY += 60;
            }
            else //walls
            {
                Player.tileRangeX += 20;
                Player.tileRangeY += 20;
            }
        }
    }
    public class DCUEffect : AccessoryEffect
    {
        public override Header ToggleHeader => null;
        public override int ToggleItemType => ModContent.ItemType<WorldShaperSoul>();
        public override bool ActiveSkill => true;

        public override void PostUpdateEquips(Player player)
        {
            if (player.mount.Active && player.mount.Type == MountID.Drill && player.whoAmI == Main.myPlayer && player.HasEffect<DCUEffect>())
            {
                Mount.amountOfBeamsAtOnce = 5; //2.5x speed increase
            }
        }
        public override void ActiveSkillJustPressed(Player player, bool stunned)
        {
            if (stunned) return;

            if (player.mount.Active && player.mount.Type == MountID.Drill)
            {
                player.mount.Dismount(player);
            }
            else
            {
                player.mount.SetMount(MountID.Drill, player);
                if (!Main.dedServ) SoundEngine.PlaySound(SoundID.Item25, player.Center);
            }
        }
    }
}
