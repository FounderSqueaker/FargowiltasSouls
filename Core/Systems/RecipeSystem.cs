using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Items.Accessories.Souls;
using FargowiltasSouls.Content.Items.Misc;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Core.Systems
{
    public class RecipeSystem : ModSystem
    {
        /*internal static List<int> DivingAccessoryList =
        [
            ItemID.DivingHelmet
        ];*/
        public static string AnyItem(int id) => $"{Lang.misc[37]} {Lang.GetItemName(id)}";

        public static string AnyItem(string fargoSoulsLocalizationKey) => $"{Lang.misc[37]} {Language.GetTextValue($"Mods.FargowiltasSouls.RecipeGroups.{fargoSoulsLocalizationKey}")}";

        public static string ItemXOrY(int id1, int id2) => $"{Lang.GetItemName(id1)} {Language.GetTextValue($"Mods.FargowiltasSouls.RecipeGroups.Or")} {Lang.GetItemName(id2)}";

        public override void AddRecipeGroups()
        {
            RecipeGroup group;

            //drax
            group = new RecipeGroup(() => ItemXOrY(ItemID.Drax, ItemID.PickaxeAxe), ItemID.Drax, ItemID.PickaxeAxe);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyDrax", group);

            //dungeon enemies
            group = new RecipeGroup(() => AnyItem("BonesBanner"), ItemID.AngryBonesBanner, ItemID.BlueArmoredBonesBanner, ItemID.HellArmoredBonesBanner, ItemID.RustyArmoredBonesBanner);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyBonesBanner", group);

            //cobalt
            /*group = new RecipeGroup(() => ItemXOrY(ItemID.CobaltRepeater, ItemID.PalladiumRepeater), ItemID.CobaltRepeater, ItemID.PalladiumRepeater);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyCobaltRepeater", group);

            //mythril
            group = new RecipeGroup(() => ItemXOrY(ItemID.MythrilRepeater, ItemID.OrichalcumRepeater), ItemID.MythrilRepeater, ItemID.OrichalcumRepeater);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyMythrilRepeater", group);

            //adamantite
            group = new RecipeGroup(() => ItemXOrY(ItemID.AdamantiteRepeater, ItemID.TitaniumRepeater), ItemID.AdamantiteRepeater, ItemID.TitaniumRepeater);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyAdamantiteRepeater", group);

            //evil wood
            group = new RecipeGroup(() => ItemXOrY(ItemID.Ebonwood, ItemID.Shadewood), ItemID.Ebonwood, ItemID.Shadewood);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyEvilWood", group);

            //any adamantite
            group = new RecipeGroup(() => ItemXOrY(ItemID.AdamantiteBar, ItemID.TitaniumBar), ItemID.AdamantiteBar, ItemID.TitaniumBar);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyAdamantite", group);*/

            //shroomite head
            group = new RecipeGroup(() => AnyItem(ItemID.ShroomiteHelmet), ItemID.ShroomiteHelmet, ItemID.ShroomiteMask, ItemID.ShroomiteHeadgear);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyShroomHead", group);

            //orichalcum head
            group = new RecipeGroup(() => AnyItem(ItemID.OrichalcumHelmet), ItemID.OrichalcumHelmet, ItemID.OrichalcumMask, ItemID.OrichalcumHeadgear);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyOriHead", group);

            //palladium head
            group = new RecipeGroup(() => AnyItem(ItemID.PalladiumHelmet), ItemID.PalladiumHelmet, ItemID.PalladiumMask, ItemID.PalladiumHeadgear);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyPallaHead", group);

            //cobalt head
            group = new RecipeGroup(() => AnyItem(ItemID.CobaltHelmet), ItemID.CobaltHelmet, ItemID.CobaltHat, ItemID.CobaltMask);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyCobaltHead", group);

            //mythril head
            group = new RecipeGroup(() => AnyItem(ItemID.MythrilHelmet), ItemID.MythrilHelmet, ItemID.MythrilHat, ItemID.MythrilHood);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyMythrilHead", group);

            //titanium head
            group = new RecipeGroup(() => AnyItem(ItemID.TitaniumHelmet), ItemID.TitaniumHelmet, ItemID.TitaniumMask, ItemID.TitaniumHeadgear);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyTitaHead", group);

            //hallowed head
            group = new RecipeGroup(() => AnyItem(ItemID.HallowedHelmet), ItemID.HallowedHelmet, ItemID.HallowedMask, ItemID.HallowedHeadgear, ItemID.HallowedHood);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyHallowHead", group);

            //ancient hallow
            group = new RecipeGroup(() => AnyItem(ItemID.AncientHallowedHelmet), ItemID.AncientHallowedHelmet, ItemID.AncientHallowedHeadgear, ItemID.AncientHallowedHood, ItemID.AncientHallowedMask);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyAncientHallowHead", group);

            //adamantite head
            group = new RecipeGroup(() => AnyItem(ItemID.AdamantiteHelmet), ItemID.AdamantiteHelmet, ItemID.AdamantiteMask, ItemID.AdamantiteHeadgear);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyAdamHead", group);

            //chloro head
            group = new RecipeGroup(() => AnyItem(ItemID.ChlorophyteHelmet), ItemID.ChlorophyteHelmet, ItemID.ChlorophyteMask, ItemID.ChlorophyteHeadgear);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyChloroHead", group);

            //spectre head
            group = new RecipeGroup(() => ItemXOrY(ItemID.SpectreHood, ItemID.SpectreMask), ItemID.SpectreHood, ItemID.SpectreMask);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnySpectreHead", group);

            //beetle ench
            //beetle body
            group = new RecipeGroup(() => ItemXOrY(ItemID.BeetleShell, ItemID.BeetleScaleMail), ItemID.BeetleShell, ItemID.BeetleScaleMail);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyBeetle", group);
            //colored husk
            group = new RecipeGroup(() => AnyItem("ColorHusk"), ItemID.VioletHusk, ItemID.CyanHusk, ItemID.RedHusk);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyColoredHusk", group);

            //jungle ench
            //regrowth
            group = new RecipeGroup(() => ItemXOrY(ItemID.StaffofRegrowth, ItemID.AcornAxe), ItemID.StaffofRegrowth, ItemID.AcornAxe);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyStaffofRegrowth", group);

            //snow ench
            //hood
            group = new RecipeGroup(() => ItemXOrY(ItemID.EskimoHood, ItemID.PinkEskimoHood), ItemID.EskimoHood, ItemID.PinkEskimoHood);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnySnowHood", group);
            //coat
            group = new RecipeGroup(() => ItemXOrY(ItemID.EskimoCoat, ItemID.PinkEskimoCoat), ItemID.EskimoCoat, ItemID.PinkEskimoCoat);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnySnowCoat", group);
            //pants
            group = new RecipeGroup(() => ItemXOrY(ItemID.EskimoPants, ItemID.PinkEskimoPants), ItemID.EskimoPants, ItemID.PinkEskimoPants);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnySnowPants", group);

            //wizard ench
            //tier 1 robe
            group = new RecipeGroup(() => ItemXOrY(ItemID.AmethystRobe, ItemID.TopazRobe), ItemID.AmethystRobe, ItemID.TopazRobe);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyTier1Robe", group);
            //tier 2 robe
            group = new RecipeGroup(() => AnyItem(ItemID.SapphireRobe), ItemID.SapphireRobe, ItemID.EmeraldRobe, ItemID.RubyRobe);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyTier2Robe", group);
            //tier 3 robe
            group = new RecipeGroup(() => ItemXOrY(ItemID.AmberRobe, ItemID.DiamondRobe), ItemID.AmberRobe, ItemID.DiamondRobe);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyTier3Robe", group);

            //flight mastery soul
            //soul craft wings
            group = new RecipeGroup(() => ItemXOrY(ItemID.AngelWings, ItemID.DemonWings), ItemID.AngelWings, ItemID.DemonWings);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnySoulWings", group);
            //elemental feather wings
            group = new RecipeGroup(() => ItemXOrY(ItemID.FlameWings, ItemID.FrozenWings), ItemID.FlameWings, ItemID.FrozenWings);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyElementWings", group);
            //holiday wings
            group = new RecipeGroup(() => AnyItem("HolidayWings"), ItemID.FestiveWings, ItemID.SpookyWings, ItemID.TatteredFairyWings);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyHolidayWings", group);
            //boss wings
            group = new RecipeGroup(() => AnyItem("BossWings"), ItemID.BetsyWings, ItemID.FishronWings, ItemID.RainbowWings);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyBossWings", group);
            //lunar wings
            group = new RecipeGroup(() => AnyItem("LunarWings"), ItemID.WingsSolar, ItemID.WingsVortex, ItemID.WingsNebula, ItemID.WingsStardust);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyLunarWings", group);

            //            //phasesabers
            //            group = new RecipeGroup(() => AnyItem("Phasesaber"), ItemID.RedPhasesaber, ItemID.BluePhasesaber, ItemID.GreenPhasesaber, ItemID.PurplePhasesaber, ItemID.WhitePhasesaber,
            //                ItemID.YellowPhasesaber);
            //            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyPhasesaber", group);

            //            //vanilla butterflies
            //            group = new RecipeGroup(() => AnyItem("Butterfly"), ItemID.JuliaButterfly, ItemID.MonarchButterfly, ItemID.PurpleEmperorButterfly,
            //                ItemID.RedAdmiralButterfly, ItemID.SulphurButterfly, ItemID.TreeNymphButterfly, ItemID.UlyssesButterfly, ItemID.ZebraSwallowtailButterfly);
            //            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyButterfly", group);

            //berserker soul
            //Any evil orb melee
            group = new RecipeGroup(() => ItemXOrY(ItemID.BallOHurt, ItemID.TheRottedFork), ItemID.BallOHurt, ItemID.TheRottedFork);
            RecipeGroup.RegisterGroup("FargowiltasSouls:BallOHurtOrTheRottedFork", group);
            //phaseblades
            group = new RecipeGroup(() => AnyItem("Phaseblade"), 
                ItemID.RedPhasesaber, ItemID.BluePhaseblade, ItemID.GreenPhaseblade, ItemID.PurplePhaseblade, ItemID.WhitePhaseblade, ItemID.YellowPhaseblade);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyPhaseblade", group);
            //dungeon biomechest melee
            group = new RecipeGroup(() => ItemXOrY(ItemID.ScourgeoftheCorruptor, ItemID.VampireKnives), ItemID.ScourgeoftheCorruptor, ItemID.VampireKnives);
            RecipeGroup.RegisterGroup("FargowiltasSouls:ScourgeoftheCorruptorOrVampireKnives", group);

            //vanilla squirrels
            group = new RecipeGroup(() => AnyItem(ItemID.Squirrel),
                ItemID.Squirrel,
                ItemID.SquirrelRed,
                ItemID.SquirrelGold,
                ItemID.GemSquirrelAmber,
                ItemID.GemSquirrelAmethyst,
                ItemID.GemSquirrelDiamond,
                ItemID.GemSquirrelEmerald,
                ItemID.GemSquirrelRuby,
                ItemID.GemSquirrelSapphire,
                ItemID.GemSquirrelTopaz,
                ModContent.ItemType<TopHatSquirrelCaught>(),
                ModContent.Find<ModItem>("Fargowiltas", "Squirrel").Type
            );
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnySquirrel", group);

            //vanilla fruits
            group = new RecipeGroup(() => AnyItem("ForestFruit"), ItemID.Apple, ItemID.Grapefruit, ItemID.Lemon);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyForestFruit", group);
            group = new RecipeGroup(() => ItemXOrY(ItemID.Plum, ItemID.Cherry), ItemID.Plum, ItemID.Cherry);
            RecipeGroup.RegisterGroup("FargowiltasSouls:PlumOrCherry", group);
            group = new RecipeGroup(() => ItemXOrY(ItemID.Mango, ItemID.Pineapple), ItemID.Mango, ItemID.Pineapple);
            RecipeGroup.RegisterGroup("FargowiltasSouls:MangoOrPineapple", group);
            group = new RecipeGroup(() => ItemXOrY(ItemID.Elderberry, ItemID.BlackCurrant), ItemID.Elderberry, ItemID.BlackCurrant);
            RecipeGroup.RegisterGroup("FargowiltasSouls:ElderberryOrBlackcurrant", group);
            group = new RecipeGroup(() => ItemXOrY(ItemID.Rambutan, ItemID.BloodOrange), ItemID.Rambutan, ItemID.BloodOrange);
            RecipeGroup.RegisterGroup("FargowiltasSouls:RambutanOrBloodOrange", group);
            //group = new RecipeGroup(() => ItemXOrY(ItemID.Coconut, ItemID.Banana), ItemID.Coconut, ItemID.Banana);
            //RecipeGroup.RegisterGroup("FargowiltasSouls:CoconutOrBanana", group); //uses both rn
            group = new RecipeGroup(() => ItemXOrY(ItemID.SpicyPepper, ItemID.Pomegranate), ItemID.SpicyPepper, ItemID.Pomegranate);
            RecipeGroup.RegisterGroup("FargowiltasSouls:SpicyPepperOrPomegranate", group);
            group = new RecipeGroup(() => ItemXOrY(ItemID.Starfruit, ItemID.Dragonfruit), ItemID.Starfruit, ItemID.Dragonfruit);
            RecipeGroup.RegisterGroup("FargowiltasSouls:StarfruitOrDragonfruit", group);

            //            //vanilla fish
            //            group = new RecipeGroup(() => AnyItem("CommonFish"), ItemID.AtlanticCod, ItemID.Bass, ItemID.Trout, ItemID.RedSnapper, ItemID.Salmon, ItemID.Tuna);
            //            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyCommonFish", group);

            //vanilla birds
            group = new RecipeGroup(() => AnyItem(ItemID.Bird), ItemID.Bird, ItemID.BlueJay, ItemID.Cardinal, ItemID.GoldBird, ItemID.Duck, ItemID.MallardDuck, ItemID.Grebe, ItemID.Penguin, ItemID.Seagull);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyBird", group);

            //            //vanilla scorpions
            //            group = new RecipeGroup(() => AnyItem(ItemID.Scorpion), ItemID.Scorpion, ItemID.BlackScorpion);
            //            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyScorpion", group);

            //            //gold pick
            //            group = new RecipeGroup(() => AnyItem(ItemID.GoldPickaxe), ItemID.GoldPickaxe, ItemID.PlatinumPickaxe);
            //            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyGoldPickaxe", group);

            //            //fish trash
            //            group = new RecipeGroup(() => AnyItem("FishingTrash"), ItemID.OldShoe, ItemID.TinCan, ItemID.FishingSeaweed);
            //            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyFishingTrash", group);

            //vanilla rotten chunk/vertebrae
            group = new RecipeGroup(() => ItemXOrY(ItemID.RottenChunk, ItemID.Vertebrae), ItemID.RottenChunk, ItemID.Vertebrae);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyRottenChunk", group);

            //vanilla gold and plat ore
            group = new RecipeGroup(() => ItemXOrY(ItemID.GoldOre, ItemID.PlatinumOre), ItemID.GoldOre, ItemID.PlatinumOre);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyGoldOre", group);

            //vanilla gold and plat bar
            group = new RecipeGroup(() => ItemXOrY(ItemID.GoldBar, ItemID.PlatinumBar), ItemID.GoldBar, ItemID.PlatinumBar);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyGoldBar", group);

            //vanilla demonite bars bar
            group = new RecipeGroup(() => ItemXOrY(ItemID.DemoniteBar, ItemID.CrimtaneBar), ItemID.DemoniteBar, ItemID.CrimtaneBar);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyDemoniteBar", group);

            //mythril and ori bar
            group = new RecipeGroup(() => ItemXOrY(ItemID.MythrilBar, ItemID.OrichalcumBar), ItemID.MythrilBar, ItemID.OrichalcumBar);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyMythrilBar", group);

            //any evil orb magic
            group = new RecipeGroup(() => ItemXOrY(ItemID.Vilethorn, ItemID.CrimsonRod), ItemID.Vilethorn, ItemID.CrimsonRod);
            RecipeGroup.RegisterGroup("FargowiltasSouls:VilethornOrCrimsonRod", group);

            //any shellphone because they made it 5 fucking different items
            group = new RecipeGroup(() => AnyItem(ItemID.Shellphone), ItemID.Shellphone, ItemID.ShellphoneDummy, ItemID.ShellphoneHell, ItemID.ShellphoneOcean, ItemID.ShellphoneSpawn);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyShellphone", group);

            //any litho lantern for the same reason as above
            group = new RecipeGroup(() => AnyItem("LithosphericCluster"), ModContent.ItemType<LithosphericCluster>(), ModContent.ItemType<LithosphericClusterInactive>());
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyLithosphericLantern", group);

            // any gem
            group = new RecipeGroup(() => AnyItem("Gem"), ItemID.Diamond, ItemID.Amber, ItemID.Ruby, ItemID.Emerald, ItemID.Sapphire, ItemID.Topaz, ItemID.Amethyst);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyGem", group);

            // any panic necklace
            group = new RecipeGroup(() => AnyItem(ItemID.PanicNecklace), ItemID.PanicNecklace, ItemID.SweetheartNecklace);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyPanicNecklace", group);

            // any sharktooth necklace
            group = new RecipeGroup(() => AnyItem(ItemID.SharkToothNecklace), ItemID.SharkToothNecklace, ItemID.StingerNecklace);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnySharktoothNecklace", group);

            // any quiver
            group = new RecipeGroup(() => AnyItem(ItemID.MagicQuiver), ItemID.MagicQuiver, ItemID.MoltenQuiver, ItemID.StalkersQuiver);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyQuiver", group);

            group = new RecipeGroup(() => AnyItem(ItemID.SniperScope), ItemID.SniperScope, ItemID.ReconScope);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnySniperScope", group);

            group = new RecipeGroup(() => AnyItem(ItemID.MagicCuffs), ItemID.MagicCuffs, ItemID.CelestialCuffs);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyMagicCuffs", group);

            group = new RecipeGroup(() => AnyItem(ItemID.ManaFlower), ItemID.ManaFlower, ItemID.ArcaneFlower, ItemID.MagnetFlower, ItemID.ManaCloak);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyManaFlower", group);

            group = new RecipeGroup(() => AnyItem("SentryAccessory"), ItemID.MonkBelt, ItemID.SquireShield, ItemID.HuntressBuckler, ItemID.ApprenticeScarf);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnySentryAccessory", group);

            group = new RecipeGroup(() => AnyItem("GemStaves"), 
                ItemID.AmethystStaff, ItemID.TopazStaff, ItemID.SapphireStaff, ItemID.EmeraldStaff, 
                ItemID.RubyStaff, ItemID.DiamondStaff, ItemID.AmberStaff);
            RecipeGroup.RegisterGroup("FargowiltasSouls:AnyGemStaff", group);
        }
        public override void PostAddRecipes()
        {
            foreach (Recipe recipe in Main.recipe.Where(r => r.createItem != null))
            {
                //disable shimmer decrafts
                if (recipe.createItem.ModItem != null && (recipe.createItem.ModItem is BaseEnchant || recipe.createItem.ModItem is BaseForce || recipe.createItem.ModItem is BaseSoul))
                {
                    recipe.DisableDecraft();
                    foreach (var item in recipe.requiredItem)
                        FargoSoulsSets.Items.MaterialOfImportantItem[item.type] = recipe.createItem.type;
                }
                    


                // disable pre-evil meteorite recipes
                /*
                if (recipe.HasIngredient(ItemID.MeteoriteBar))
                {
                    LocalizedText desc = Language.GetText($"Mods.FargowiltasSouls.Conditions.PostEvilEternity");
                    Condition c = new(desc, () => WorldSafrievingSystem.EternityMode && Condition.DownedEowOrBoc.IsMet());
                    recipe.AddCondition(c);
                }
                */
                /*if (recipe.createItem.accessory && !DivingAccessoryList.Contains(recipe.createItem.type))
                {
                    foreach (Item item in recipe.requiredItem.Where(i => DivingAccessoryList.Contains(i.type)))
                    {
                        DivingAccessoryList.Add(recipe.createItem.type);
                    }
                }*/
            }
        }
    }
}
