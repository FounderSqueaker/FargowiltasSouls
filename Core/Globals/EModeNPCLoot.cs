using Fargowiltas.Content.Items.Explosives;
using FargowiltasSouls.Content.Bosses.BanishedBaron;
using FargowiltasSouls.Content.Bosses.CursedCoffin;
using FargowiltasSouls.Content.Bosses.Lifelight;
using FargowiltasSouls.Content.Bosses.TrojanSquirrel;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Consumables;
using FargowiltasSouls.Content.Items.Pets;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.PirateInvasion;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.StupidSnowmanEvent;
using FargowiltasSouls.Core.ItemDropRules;
using FargowiltasSouls.Core.ItemDropRules.Conditions;
using FargowiltasSouls.Core.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace FargowiltasSouls.Core.Globals
{
    public partial class EModeNPCLoot : GlobalNPC
    {
        #region NPC Lists
        static List<int> EvilCritters =
        [
                NPCID.CorruptBunny,
                NPCID.CrimsonBunny,
                NPCID.CorruptGoldfish,
                NPCID.CrimsonGoldfish,
                NPCID.CorruptPenguin,
                NPCID.CrimsonPenguin
            ];
        static List<int> Mimics =
        [
                NPCID.Mimic,
                NPCID.PresentMimic,
                NPCID.IceMimic,
                NPCID.BigMimicCorruption,
                NPCID.BigMimicCrimson,
                NPCID.BigMimicHallow,
                NPCID.BigMimicJungle
            ];
        static List<int> HardmodeDesertEnemies =
        [
                NPCID.DesertBeast,
                NPCID.DesertScorpionWalk,
                NPCID.DesertScorpionWall,
                NPCID.DesertLamiaDark,
                NPCID.DesertLamiaLight,
                NPCID.DesertGhoul,
                NPCID.DesertGhoulCorruption,
                NPCID.DesertGhoulCrimson,
                NPCID.DesertGhoulHallow,
                NPCID.DesertDjinn
            ];
        static List<int> EarlyBirdEnemies =
        [
            NPCID.WyvernHead,
            NPCID.WyvernBody,
            NPCID.WyvernBody2,
            NPCID.WyvernBody3,
            NPCID.WyvernLegs,
            NPCID.WyvernTail,
            NPCID.Mimic,
            NPCID.IceMimic,
            NPCID.Medusa,
            NPCID.PigronCorruption,
            NPCID.PigronCrimson,
            NPCID.PigronHallow,
            NPCID.AngryNimbus,
            NPCID.MushiLadybug,
            NPCID.AnomuraFungus,
            NPCID.ZombieMushroom,
            NPCID.ZombieMushroomHat,
            NPCID.IceGolem,
            NPCID.SandElemental,
            NPCID.RedDevil
        ];
        static List<int> Hornets =
        [
            NPCID.Hornet,
            NPCID.HornetFatty,
            NPCID.HornetHoney,
            NPCID.HornetLeafy,
            NPCID.HornetSpikey,
            NPCID.HornetStingy,
            NPCID.BigHornetFatty,
            NPCID.BigHornetHoney,
            NPCID.BigHornetLeafy,
            NPCID.BigHornetSpikey,
            NPCID.BigHornetStingy,
            NPCID.BigMossHornet,
            NPCID.GiantMossHornet,
            NPCID.LittleHornetFatty,
            NPCID.LittleHornetHoney,
            NPCID.LittleHornetLeafy,
            NPCID.LittleHornetSpikey,
            NPCID.LittleHornetStingy,
            NPCID.LittleMossHornet,
            NPCID.MossHornet,
            NPCID.TinyMossHornet
        ];
        static List<int> MushroomEnemies =
        [
            NPCID.FungiBulb,
            NPCID.GiantFungiBulb,
            NPCID.AnomuraFungus,
            NPCID.MushiLadybug,
            NPCID.SporeBat,
            NPCID.ZombieMushroom,
            NPCID.ZombieMushroomHat,
            NPCID.SporeSkeleton,
            NPCID.FungoFish
        ];
        #endregion
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            LeadingConditionRule emodeRule = new(new EModeDropCondition());
            switch (npc.type)
            {
                #region Bosses
                case NPCID.EyeofCthulhu:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<AgitatingLens>()));
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ItemID.FallenStar, 5));
                    }
                    break;
                case NPCID.DD2Betsy:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<BetsysHeart>()));
                    }
                    break;
                case NPCID.BrainofCthulhu:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<GuttedHeart>()));

                        //to make up for no loot until dead
                        emodeRule.OnSuccess(ItemDropRule.Common(ItemID.TissueSample, 1, 60, 60));
                        emodeRule.OnSuccess(ItemDropRule.Common(ItemID.CrimtaneOre, 1, 200, 200));
                    }
                    break;
                case NPCID.Deerclops:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<Deerclawps>()));
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<DeerSinew>()));
                    }
                    break;
                case NPCID.TheDestroyer:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<RemoteControl>()));
                    }
                    break;
                case NPCID.DukeFishron:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<MutantAntibodies>()));
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<MutantsCreditCard>()));
                        emodeRule.OnSuccess(ItemDropRule.OneFromOptions(1,
                            ItemID.FuzzyCarrot,
                            ItemID.AnglerHat,
                            ItemID.AnglerVest,
                            ItemID.AnglerPants,
                            ItemID.GoldenFishingRod,
                            ItemID.GoldenBugNet,
                            ItemID.FishHook,
                            ItemID.HighTestFishingLine,
                            ItemID.AnglerEarring,
                            ItemID.TackleBox,
                            ItemID.FishermansGuide,
                            ItemID.WeatherRadio,
                            ItemID.Sextant,
                            ItemID.FinWings,
                            ItemID.BottomlessBucket,
                            ItemID.SuperAbsorbantSponge,
                            ItemID.HotlineFishingHook,
                            ItemID.PirateMap
                        ));
                    }
                    break;
                case NPCID.DungeonGuardian:
                    {
                        emodeRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<SinisterIcon>()));
                    }
                    break;
                case NPCID.EaterofWorldsBody or NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail: //just to be sure
                    {
                        LeadingConditionRule lastEater = new(new Conditions.LegacyHack_IsABoss());
                        emodeRule.OnSuccess(lastEater);
                        lastEater.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<RottingHeart>()));

                        //to make up for no loot until dead
                        lastEater.OnSuccess(ItemDropRule.Common(ItemID.ShadowScale, 1, 60, 60));
                        lastEater.OnSuccess(ItemDropRule.Common(ItemID.DemoniteOre, 1, 200, 200));
                    }
                    break;
                case NPCID.HallowBoss:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<PrecisionSeal>()));
                    }
                    break;
                case NPCID.Golem:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<LihzahrdTreasureBox>()));
                    }
                    break;
                case NPCID.IceQueen:
                    {
                        emodeRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<IceQueensShield>(), 5));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.Present, 1, 1, 5));
                    }
                    break;
                case NPCID.KingSlime:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<SlimyShield>()));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.Gel, 1, 50, 100));
                    }
                    break;
                case NPCID.CultistBoss:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<CelestialRune>()));
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<MutantsPact>()));
                    }
                    break;
                case NPCID.MartianSaucer:
                case NPCID.MartianSaucerCore:
                    {
                        emodeRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<SaucerControlConsole>(), 5));
                    }
                    break;
                case NPCID.MoonLordCore:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<ChaliceofTheMoon>()));
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ItemID.LunarOre, 150));
                    }
                    break;
                case NPCID.Plantera:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<MagicalBulb>()));
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ItemID.LifeFruit, 3));
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ItemID.ChlorophyteOre, 200));
                    }
                    break;
                case NPCID.Pumpking:
                    {
                        emodeRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<PumpkingsCape>(), 5));
                        emodeRule.OnSuccess(ItemDropRule.Common(ItemID.GoodieBag, 1, 1, 5));
                        emodeRule.OnSuccess(ItemDropRule.Common(ItemID.BloodyMachete, 10));
                    }
                    break;
                case NPCID.QueenBee:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<QueenStinger>()));
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ItemID.HerbBag, 5));
                    }
                    break;
                case NPCID.QueenSlimeBoss:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<GelicWings>()));
                    }
                    break;
                case NPCID.SkeletronHead:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<NecromanticBrew>()));
                    }
                    break;
                case NPCID.SkeletronPrime:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<ReinforcedPlating>()));
                    }
                    break;
                case NPCID.Retinazer or NPCID.Spazmatism:
                    {
                        LeadingConditionRule noTwin = new(new Conditions.MissingTwin());
                        emodeRule.OnSuccess(noTwin);
                        noTwin.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<FusedLens>()));
                    }
                    break;
                case NPCID.WallofFlesh:
                    {
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<PungentEyeball>()));
                        emodeRule.OnSuccess(FargoSoulsUtil.BossBagDropCustom(ModContent.ItemType<MutantsDiscountCard>()));
                    }
                    break;
                #endregion
                #region Normal Enemies
                case NPCID.CaveBat:
                case NPCID.GiantBat:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new RabiesVaccineDropCondition(), ModContent.ItemType<RabiesVaccine>(), 20));
                    break;
                case NPCID.BloodNautilus:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<DreadShell>()));
                    break;
                case var _ when EvilCritters.Contains(npc.type):
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<SqueakyToy>(), 10));
                    break;
                case NPCID.EyeballFlyingFish or NPCID.ZombieMerman:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.FrogLeg, 10));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.BalloonPufferfish, 10));
                    break;
                case NPCID.GiantWormHead or NPCID.DiggerHead:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.WormTooth, 1, 3, 9));
                    break;
                case var _ when Mimics.Contains(npc.type):
                    switch (npc.type)
                    {
                        case NPCID.BigMimicCorruption:
                            FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.CorruptFishingCrateHard));
                            break;
                        case NPCID.BigMimicCrimson:
                            FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.CrimsonFishingCrateHard));
                            break;
                        case NPCID.BigMimicHallow:
                            FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.HallowedFishingCrateHard));
                            break;
                        case NPCID.BigMimicJungle:
                            FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.JungleFishingCrateHard));
                            int[] items = [ItemID.AngelStatue, ItemID.FartinaJar, ItemID.StinkPotion, ItemID.Coal, ItemID.RedPotion, ItemID.GoldenShower, ItemID.MasterBait, ItemID.WaterGun, ItemID.PoopBlock, ItemID.GelBalloon];
                            npcLoot.RemoveWhere(rule => rule is CommonDrop drop && items.Contains(drop.itemId) && FargoSoulsUtil.LockJungleMimicDrops(npcLoot, rule));
                            break;
                    }
                    break;
                case NPCID.LostGirl or NPCID.Nymph:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<NymphsPerfume>(), 3));
                    break;
                case NPCID.RuneWizard:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<MysticSkull>(), 3));
                    break;
                case NPCID.Tim:
                    npcLoot.Add(ItemDropRule.ByCondition(new EModeDropCondition(), ModContent.ItemType<TimsConcoction>(), 3));
                    break;
                case NPCID.WalkingAntlion:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new DownedEvilBossDropCondition(), ItemID.FastClock, 50));
                    break;
                #endregion
                case NPCID.Everscream:
                case NPCID.SantaNK1:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.Present, 1, 1, 5));
                    break;
                case NPCID.GoblinSummoner:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<WretchedPouch>(), 4));
                    break;
                case NPCID.RainbowSlime:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<ConcentratedRainbowMatter>(), 5));
                    break;
                case NPCID.LavaSlime:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.LavaCharm, 100));
                    break;
                case NPCID.Derpling:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.TrifoldMap, 50));
                    break;
                case NPCID.DoctorBones:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<CrystalSkull>(), 2));
                    break;
                case var _ when Hornets.Contains(npc.type):
                    if (npc.type == NPCID.MossHornet)
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.Stinger, 2));
                    break;
                case NPCID.Piranha:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.AdhesiveBandage, 50));
                    break;
                case NPCID.AngryTrapper:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.Vine, 2));
                    break;
                case NPCID.BrainScrambler:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.BrainScrambler, 100));
                    break;
                case NPCID.CultistArcherWhite:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.Find<ModItem>("Fargowiltas", "CultistSummon").Type, 100));
                    break;
                case var _ when MushroomEnemies.Contains(npc.type):
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.GlowingMushroom, 1, 1, 5));
                    break;
                case NPCID.PirateShipCannon:
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<SecurityWallet>(), 4));
                    break;
                case NPCID.PirateCaptain:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.Find<ModItem>("Fargowiltas", "GoldenDippingVat").Type, 15));
                    break;
                case NPCID.MourningWood:
                    {
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.GoodieBag, 1, 1, 5));
                    }
                    break;
                case NPCID.GraniteGolem:
                case NPCID.GraniteFlyer:
                    {
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.Geode, 10, 1, 3));
                    }
                    break;
                case NPCID.Raven:
                    {
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.GoodieBag, 10));
                        break;
                    }
                case NPCID.RockGolem:
                    {
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.CopperOre, 3, 10, 30));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.TinOre, 3, 10, 30));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.IronOre, 4, 8, 25));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.LeadOre, 4, 8, 25));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.SilverOre, 5, 6, 20));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.TungstenOre, 5, 6, 20));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.GoldOre, 6, 5, 15));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.PlatinumOre, 6, 5, 15));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.CobaltOre, 5, 8, 16));
                        FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.PalladiumOre, 5, 8, 16));
                    }
                    break;
                case NPCID.MisterStabby:
                case NPCID.SnowBalla:
                case NPCID.SnowmanGangsta:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<OrdinaryCarrot>(), 25));
                    break;
                case NPCID.Shark:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<HokeyBall>(), 100));
                    break;
                case NPCID.SporeBat:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.Shroomerang, 10));
                    break;

                case NPCID.RedDevil:
                    // evil evil evil
                    foreach (var rule in npcLoot.Get(includeGlobalDrops: false))
                    {
                        if (rule is CommonDrop commonDrop)
                        {
                            if (commonDrop.itemId == ItemID.FireFeather)
                            {
                                npcLoot.Remove(rule);
                                FargoSoulsUtil.DropBasedOnEmode(npcLoot, ItemDropRule.Common(ItemID.FireFeather, 5), rule);
                            }
                        }
                        if (rule is LeadingConditionRule leadingRule)
                        {
                            var enumerator = new List<IItemDropRuleChainAttempt>(leadingRule.ChainedRules);
                            foreach (var chainedRule in enumerator)
                            {
                                if (chainedRule.RuleToChain is CommonDrop chainedCommonDrop && (chainedCommonDrop.itemId == ItemID.UnholyTrident || chainedCommonDrop.itemId == ItemID.FlowerofFire))
                                {
                                    leadingRule.ChainedRules.Remove(chainedRule);
                                    leadingRule.OnSuccess(new DropBasedOnEMode(ItemDropRule.Common(chainedCommonDrop.itemId, 3), chainedCommonDrop));
                                }
                            }
                        }
                    }
                    break;
            }
            /*
            #region early bird
            if (EarlyBirdEnemies.Contains(npc.type))
            {
                switch (npc.type)
                {
                    case NPCID.Medusa:
                        npcLoot.RemoveWhere(rule => rule is CommonDrop drop && drop.itemId == ItemID.MedusaHead && FargoSoulsUtil.LockEarlyBirdDrop(npcLoot, rule));
                        break;

                    case NPCID.WyvernHead:
                        npcLoot.RemoveWhere(rule => rule is DropBasedOnExpertMode drop && drop.ruleForNormalMode is CommonDrop drop2 && drop2.itemId == ItemID.SoulofFlight && FargoSoulsUtil.LockEarlyBirdDrop(npcLoot, rule));
                        break;

                    case NPCID.PigronHallow:
                    case NPCID.PigronCorruption:
                    case NPCID.PigronCrimson:
                        npcLoot.RemoveWhere(rule => rule is ItemDropWithConditionRule drop && drop.condition is Conditions.DontStarveIsUp && drop.itemId == ItemID.HamBat && FargoSoulsUtil.LockEarlyBirdDrop(npcLoot, rule));
                        npcLoot.RemoveWhere(rule => rule is ItemDropWithConditionRule drop && drop.condition is Conditions.DontStarveIsNotUp && drop.itemId == ItemID.HamBat && FargoSoulsUtil.LockEarlyBirdDrop(npcLoot, rule));
                        break;

                    case NPCID.Mimic:
                        npcLoot.RemoveWhere(rule => rule is OneFromOptionsDropRule drop && drop.dropIds.Contains(ItemID.DualHook) && FargoSoulsUtil.LockEarlyBirdDrop(npcLoot, rule));
                        //FargoSoulsUtil.AddEarlyBirdDrop(npcLoot, ItemDropRule.OneFromOptions(1, ItemID.TitanGlove, ItemID.PhilosophersStone, ItemID.CrossNecklace, ItemID.DualHook));
                        break;

                    case NPCID.IceMimic:
                        npcLoot.RemoveWhere(rule => rule is CommonDrop drop && drop.itemId == ItemID.ToySled && FargoSoulsUtil.LockEarlyBirdDrop(npcLoot, rule));
                        //FargoSoulsUtil.AddEarlyBirdDrop(npcLoot, ItemDropRule.OneFromOptions(1, ItemID.TitanGlove, ItemID.PhilosophersStone, ItemID.CrossNecklace, ItemID.DualHook));
                        break;

                    case NPCID.AngryNimbus:
                        npcLoot.RemoveWhere(rule => rule is CommonDrop drop && drop.itemId == ItemID.NimbusRod && FargoSoulsUtil.LockEarlyBirdDrop(npcLoot, rule));
                        break;

                    default: break;
                }

            }
            #endregion
            */
            npcLoot.Add(emodeRule);
        }

        #region Early Bird Loot
        public override void Load()
        {
            MonoModHooks.Add(ModifyNPCLoot_Method, ModifyNPCLoot_Detour);
        }
        private static readonly MethodInfo ModifyNPCLoot_Method = typeof(NPCLoader).GetMethod("ModifyNPCLoot", LumUtils.UniversalBindingFlags);
        public delegate void Orig_ModifyNPCLoot(NPC npc, NPCLoot npcLoot);
        internal static void ModifyNPCLoot_Detour(Orig_ModifyNPCLoot orig, NPC npc, NPCLoot npcLoot)
        {
            orig(npc, npcLoot);
            if (EarlyBirdEnemies.Contains(npc.type))
            {
                foreach (var rule in npcLoot.Get(includeGlobalDrops: false))
                {
                    if (AllowEarlyBirdDrop(npc, npcLoot, rule))
                    {
                        continue;
                    }
                    npcLoot.Remove(rule);
                    FargoSoulsUtil.LockEarlyBirdDrop(npcLoot, rule);
                }
            }

            // add loot exempt from hardmode lock
            switch (npc.type)
            {
                case NPCID.WyvernHead:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsPreHardmode(), ItemID.FloatingIslandFishingCrate));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.FloatingIslandFishingCrateHard));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<WyvernFeather>(), 3));
                    FargoSoulsUtil.AddEarlyBirdDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<WyvernFeather>()));
                    break;

                case NPCID.Mimic:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsPreHardmode(), ItemID.GoldenCrate));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.GoldenCrateHard));

                    FargoSoulsUtil.EModeDrop(npcLoot, new LeadingConditionRule(new Conditions.IsPreHardmode()).OnSuccess(ItemDropRule.OneFromOptions(1,
                        ItemID.BandofRegeneration,
                        ItemID.MagicMirror,
                        ItemID.CloudinaBottle,
                        ItemID.HermesBoots,
                        ItemID.Mace,
                        ItemID.ShoeSpikes
                        )));
                    break;

                case NPCID.IceMimic:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsPreHardmode(), ItemID.GoldenCrate));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.GoldenCrateHard));

                    FargoSoulsUtil.EModeDrop(npcLoot, new LeadingConditionRule(new Conditions.IsPreHardmode()).OnSuccess(ItemDropRule.OneFromOptions(1,
                        ItemID.IceBoomerang,
                        ItemID.IceBlade,
                        ItemID.IceSkates,
                        ItemID.SnowballCannon,
                        ItemID.BlizzardinaBottle,
                        ItemID.FlurryBoots
                        )));
                    break;

                case NPCID.IceGolem:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsPreHardmode(), ItemID.FrozenCrate));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.FrozenCrateHard));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<FrigidGrasp>(), 3));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.SnowGlobe));
                    FargoSoulsUtil.AddEarlyBirdDrop(npcLoot, ItemDropRule.Common(ModContent.ItemType<FrigidGrasp>()));
                    break;

                case NPCID.SandElemental:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsPreHardmode(), ItemID.OasisCrate));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.OasisCrateHard));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<SandsofTime>(), 3));

                    FargoSoulsUtil.AddEarlyBirdDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsPreHardmode(), ModContent.ItemType<SandsofTime>()));
                    break;

                case NPCID.RedDevil:
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsPreHardmode(), ItemID.LavaCrate));
                    FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.LavaCrateHard));
                    break;
            }

        }
        public static bool AllowEarlyBirdDrop(NPC npc, NPCLoot npcLoot, IItemDropRule? rule) // for mod support
        {
            return false;
        }
        #endregion
    }
    public class EModeFirstKillDrop : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            List<IItemDropRule> rules = [];

            if (npc.type == ModContent.NPCType<TrojanSquirrel>())
            {
                rules.Add(FirstKillDrop(2, ItemID.LifeCrystal));
                rules.Add(FirstKillDrop(5, ItemID.WoodenCrate));
                rules.Add(FirstKillDrop(5, ItemID.HerbBag));
            }
            else if (npc.type == ModContent.NPCType<CursedCoffin>())
            {
                rules.Add(FirstKillDrop(5, ItemID.OasisCrate)); 
            }
            else if (npc.type == ModContent.NPCType<BanishedBaron>())
            {
                rules.Add(FirstKillDrop(5, ItemID.OceanCrateHard));
            }
            else if (npc.type == ModContent.NPCType<Lifelight>())
            {
                rules.Add(FirstKillDrop(5, ItemID.HallowedFishingCrateHard));
            }

            switch (npc.type)
            {
                case NPCID.KingSlime:
                    {
                        rules.Add(FirstKillDrop(2, ItemID.LifeCrystal));
                        rules.Add(FirstKillDrop(5, ItemID.WoodenCrate));
                    }
                    break;

                case NPCID.EyeofCthulhu:
                    {
                        rules.Add(FirstKillDrop(3, ItemID.LifeCrystal));
                        rules.Add(FirstKillDrop(5, ItemID.IronCrate));
                    }
                    break;

                case NPCID.BrainofCthulhu:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.CrimsonFishingCrate));
                    }
                    break;

                case NPCID.EaterofWorldsBody or NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail: //just to be sure
                    {
                        LeadingConditionRule lastEater = new(new Conditions.LegacyHack_IsABoss());
                        lastEater.OnSuccess(FirstKillDrop(5, ItemID.CorruptFishingCrate));
                        rules.Add(lastEater);
                    }
                    break;

                case NPCID.QueenBee:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.JungleFishingCrate));
                    }
                    break;

                case NPCID.Deerclops:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.FrozenCrate));
                    }
                    break;

                case NPCID.SkeletronHead:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.DungeonFishingCrate));
                    }
                    break;

                case NPCID.WallofFlesh:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.HallowedFishingCrateHard));
                        rules.Add(FirstKillDrop(5, ItemID.LavaCrateHard));
                    }
                    break;

                case NPCID.QueenSlimeBoss:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.HallowedFishingCrateHard));
                    }
                    break;


                case NPCID.TheDestroyer:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.IronCrateHard));
                    }
                    break;

                case NPCID.SkeletronPrime:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.IronCrateHard));
                    }
                    break;

                case NPCID.Retinazer or NPCID.Spazmatism:
                    {
                        LeadingConditionRule noTwin = new(new Conditions.MissingTwin());
                        noTwin.OnSuccess(FirstKillDrop(5, ItemID.IronCrateHard));
                        rules.Add(noTwin);
                    }
                    break;

                case NPCID.Plantera:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.JungleFishingCrateHard));
                        rules.Add(FirstKillDrop(1, ModContent.ItemType<LihzahrdInstactuationBomb>()));
                    }
                    break;

                case NPCID.DD2Betsy:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.GoldenCrateHard));
                    }
                    break;

                case NPCID.Golem:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.GoldenCrateHard));
                    }
                    break;

                case NPCID.DukeFishron:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.OceanCrateHard));
                    }
                    break;

                case NPCID.HallowBoss:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.HallowedFishingCrateHard));
                    }
                    break;

                case NPCID.CultistBoss:
                    {
                        rules.Add(FirstKillDrop(5, ItemID.DungeonFishingCrateHard));
                    }
                    break;


            }

            foreach (var rule in rules)
                npcLoot.Add(rule);
        }

        private static IItemDropRule Drop(int count, int itemID) => ItemDropRule.Common(itemID, minimumDropped: count, maximumDropped: count);

        public static IItemDropRule FirstKillDrop(int amount, int itemID)
        {
            IItemDropRule rule = new LeadingConditionRule(new FirstKillCondition());
            rule.OnSuccess(Drop(amount, itemID));
            return rule;
        }
    }

    public class FirstKillCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) =>
            !info.IsInSimulation &&
            WorldSavingSystem.EternityMode &&
            info.npc.type switch
            {
                NPCID.KingSlime => !NPC.downedSlimeKing,
                NPCID.EyeofCthulhu => !NPC.downedBoss1,
                NPCID.BrainofCthulhu => !NPC.downedBoss2,
                NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail => !NPC.downedBoss2,
                NPCID.QueenBee => !NPC.downedQueenBee,
                NPCID.Deerclops => !NPC.downedDeerclops,
                NPCID.SkeletronHead => !NPC.downedBoss3,
                NPCID.WallofFlesh => !Main.hardMode,
                NPCID.QueenSlimeBoss => !NPC.downedQueenSlime,
                NPCID.TheDestroyer => !NPC.downedMechBoss1,
                NPCID.Retinazer or NPCID.Spazmatism => !NPC.downedMechBoss2,
                NPCID.SkeletronPrime => !NPC.downedMechBoss3,
                NPCID.Plantera => !NPC.downedPlantBoss,
                NPCID.DD2Betsy => !DD2Event.DownedInvasionT3,
                NPCID.Golem => !NPC.downedGolemBoss,
                NPCID.DukeFishron => !NPC.downedFishron,
                NPCID.HallowBoss => !NPC.downedEmpressOfLight,
                NPCID.CultistBoss => !NPC.downedAncientCultist,
                _ => ModdedCanDrop(info.npc.type)
                    
            };

        public bool ModdedCanDrop(int type)
        {
            if (type == ModContent.NPCType<TrojanSquirrel>())
                return !WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TrojanSquirrel];
            if (type == ModContent.NPCType<CursedCoffin>())
                return !WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.CursedCoffin];
            if (type == ModContent.NPCType<BanishedBaron>())
                return !WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.BanishedBaron];
            if (type == ModContent.NPCType<Lifelight>())
                return !WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.Lifelight];
            return false;
        }

        public bool CanShowItemDropInUI() => true;

        public string GetConditionDescription() => Language.GetTextValue("Mods.FargowiltasSouls.Conditions.FirstKill");
    }
}
