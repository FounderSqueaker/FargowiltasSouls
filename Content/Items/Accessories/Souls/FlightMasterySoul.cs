using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Expert;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.ModPlayers;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Souls
{
    [AutoloadEquip(EquipType.Wings)]
    public class FlightMasterySoul : FlightMasteryWings
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 45));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }


        public static readonly Color ItemColor = new(56, 134, 255);
        protected override Color? nameColor => ItemColor;

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            AddEffects(player, Item);
        }
        public static void AddEffects(Player player, Item item)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();

            modPlayer.FlightMasterySoul = true;
            player.wingTimeMax = 999999;
            player.wingTime = player.wingTimeMax;
            player.ignoreWater = true;

            player.AddEffect<FlightMasteryInsignia>(item);
            player.AddEffect<FlightMasteryGravity>(item);

            //hover
            if (player.wingTime > 0 && player.controlDown && player.controlJump && !player.mount.Active)
            {
                player.position.Y -= player.velocity.Y;
                if (player.velocity.Y > 0.1f)
                    player.velocity.Y = 0.1f;
                else if (player.velocity.Y < -0.1f)
                    player.velocity.Y = -0.1f;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()

            .AddIngredient(ItemID.EmpressFlightBooster) // Soaring Insignia
            .AddIngredient(ItemID.GravityGlobe)
            .AddIngredient(ItemID.CreativeWings) // Fledgling Wings
            .AddRecipeGroup("FargowiltasSouls:AnySoulWings") // Angel or Demon wings
            .AddRecipeGroup("FargowiltasSouls:AnyElementWings") // Flame or Frost Wings
            .AddRecipeGroup("FargowiltasSouls:AnyHolidayWings") // Festive, Spooky Wings or Tattered Fairy Wings
            .AddRecipeGroup("FargowiltasSouls:AnyBossWings") // Betsy, Fishron or Empress Wings
            .AddRecipeGroup("FargowiltasSouls:AnyLunarWings") // Solar, Vortex, Nebula, or Stardust Wings
            .AddIngredient(ItemID.LongRainbowTrailWings) // Celestial Starboard

            // old recipe

            //.AddIngredient(ItemID.EmpressFlightBooster) //soaring insignia
            //.AddIngredient(ItemID.BatWings) //bat wings
            //.AddIngredient(ItemID.CreativeWings) //fledgling wings
            //.AddIngredient(ItemID.FairyWings)
            //.AddIngredient(ItemID.HarpyWings)
            //.AddIngredient(ItemID.BoneWings)
            //.AddIngredient(ItemID.FrozenWings)
            //.AddIngredient(ItemID.FlameWings)
            //.AddIngredient(ItemID.TatteredFairyWings)
            //.AddIngredient(ItemID.FestiveWings)
            //.AddIngredient(ItemID.BetsyWings)
            //.AddIngredient(ItemID.FishronWings)
            //.AddIngredient(ItemID.RainbowWings) //empress wings
            //.AddIngredient(ItemID.LongRainbowTrailWings) //celestial starboard
            //.AddIngredient(ItemID.GravityGlobe)

            .AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"))
            .Register();
        }

        public override bool WingUpdate(Player player, bool inUse)
        {   
            if (inUse)
            {
                int frameCount = 8;
                player.FargoSouls().FlightSoulWingFrameX = 0;
                if (++player.wingFrameCounter >= 4)
                {
                    player.wingFrameCounter = 0;
                    if (++player.wingFrame >= frameCount)
                    {
                        player.wingFrame = 0;
                    }
                    
                }
                
            }
            else
            {
                player.FargoSouls().FlightSoulWingFrameX = 1;
                player.wingFrameCounter = 0;
                player.wingFrame = 0;
                if (player.velocity.Y != 0)
                {
                    player.wingFrame = 1;
                }
            }
            return true;
        }
    }
    public class FlightMasteryInsignia : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<FlightMasteryHeader>();
        public override int ToggleItemType => ItemID.EmpressFlightBooster;
        
        public override void PostUpdateEquips(Player player)
        {
            player.empressBrooch = true;
        }
    }
    public class FlightMasteryGravity : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<FlightMasteryHeader>();
        public override int ToggleItemType => ModContent.ItemType<FlightMasterySoul>();
        
        public override void PostUpdateEquips(Player player)
        {
            player.gravity = Player.defaultGravity * 1.5f;
        }
    }
}
