using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Buffs.Minions;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Consumables;
using FargowiltasSouls.Content.Items.Materials;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Souls
{
    [AutoloadEquip(/*EquipType.Head, */EquipType.Front, EquipType.Back, EquipType.Shield)]
    public class MasochistSoul : BaseSoul
    {
        public override bool Eternity => true;
        public override List<AccessoryEffect> ActiveSkillTooltips =>
            [AccessoryEffectLoader.GetEffect<BetsyDashEffect>(),
             AccessoryEffectLoader.GetEffect<ParryEffect>(),
             AccessoryEffectLoader.GetEffect<DiveEffect>(),
             AccessoryEffectLoader.GetEffect<DebuffInstallKeyEffect>(),
             AccessoryEffectLoader.GetEffect<FrigidGraspKeyEffect>(),
             AccessoryEffectLoader.GetEffect<IceShieldEffect>(),
             AccessoryEffectLoader.GetEffect<TimsInspectEffect>(),
             AccessoryEffectLoader.GetEffect<RemoteLightningEffect>(),
             AccessoryEffectLoader.GetEffect<BulbKeyEffect>(),
             AccessoryEffectLoader.GetEffect<AmmoCycleEffect>()];

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 50));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.value = 5000000;
            Item.rare = ItemRarityID.Expert;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.autoReuse = true;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTurn = true;
            Item.UseSound = SoundID.DD2_BetsyFlameBreath with { MaxInstances = 1, SoundLimitBehavior = Terraria.Audio.SoundLimitBehavior.IgnoreNew, Pitch = -1f, Volume = 2f };
        }
        public static readonly Color ItemColor = new(255, 51, 153, 0);
        protected override Color? nameColor => ItemColor;
        public override bool CanUseItem(Player player) => SandsofTime.CanUse(player);
        public override void UseItemFrame(Player player) => SandsofTime.Use(player);
        public override bool? UseItem(Player player) => true;

        public static void PassiveEffect(Player player, Item item)
        {
            BionomicCluster.PassiveEffect(player, item);
            LithosphericCluster.PassiveEffect(player, item);

            player.AddEffect<AmmoCycleEffect>(item);
            player.AddEffect<ChalicePotionEffect>(item);
        }

        public override void UpdateInventory(Player player) => PassiveEffect(player, Item);
        public override void UpdateVanity(Player player) => PassiveEffect(player, Item);
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            FargoSoulsPlayer fargoPlayer = player.FargoSouls();

            BionomicCluster.PassiveEffect(player, Item);
            LithosphericCluster.PassiveEffect(player, Item);

            BionomicCluster.ActiveEffect(player, Item);
            LithosphericCluster.ActiveEffect(player, Item);

            fargoPlayer.MasochistSoul = true;
            fargoPlayer.MasochistSoulItem = Item;

            player.AddBuff(ModContent.BuffType<SouloftheMasochistBuff>(), 2);

            //stat modifiers
            DamageClass damageClass = player.ProcessDamageTypeFromHeldItem();
            player.GetDamage(damageClass) += 0.12f;
            player.GetCritChance(damageClass) += 12f;
            player.endurance += 0.05f;
            //player.GetArmorPenetration(DamageClass.Generic) += 50;
            player.statLifeMax2 += player.statLifeMax / 10;
            //player.lifeRegen += 4;
            fargoPlayer.WingTimeModifier += 0.5f;
            player.moveSpeed += 0.1f;

            //slimy shield
            player.buffImmune[BuffID.Slimed] = true;

            player.AddEffect<SlimeFallEffect>(Item);
            player.AddEffect<PlatformFallthroughEffect>(Item);

            //if (player.AddEffect<SlimyShieldEffect>(Item))
                //player.FargoSouls().SlimyShieldItem = Item;

            //agitating lens
            //player.AddEffect<AgitatingLensEffect>(Item);
            //player.AddEffect<AgitatingLensInstall>(Item);
            player.AddEffect<DebuffInstallKeyEffect>(Item);

            //queen stinger
            //player.honey = true;
            player.npcTypeNoAggro[210] = true;
            player.npcTypeNoAggro[211] = true;
            player.npcTypeNoAggro[42] = true;
            player.npcTypeNoAggro[176] = true;
            player.npcTypeNoAggro[231] = true;
            player.npcTypeNoAggro[232] = true;
            player.npcTypeNoAggro[233] = true;
            player.npcTypeNoAggro[234] = true;
            player.npcTypeNoAggro[235] = true;
            fargoPlayer.QueenStingerItem = Item;

            //necromantic brew
            fargoPlayer.NecromanticBrewItem = Item;
            player.AddEffect<NecroBrewSpin>(Item);

            //deerclawps
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.AddEffect<DeerclawpsEffect>(Item);
            player.AddEffect<DeerclawpsDashDR>(Item);

            //supreme deathbringer fairy
            fargoPlayer.SupremeDeathbringerFairy = true;
            player.AddEffect<SupremeDashEffect>(Item);

            //pure heart
            fargoPlayer.PureHeart = true;

            //rotting heart
            fargoPlayer.RottingHeartItem = Item;
            //player.AddEffect<RottingHeartEaters>(Item);
            if (fargoPlayer.RottingHeartCD > 0)
                fargoPlayer.RottingHeartCD -= 2;

            //gutted heart
            player.AddEffect<GuttedHeartEffect>(Item);
            player.AddEffect<GuttedHeartMinions>(Item);
            //fargoPlayer.GuttedHeartCD -= 2; //faster spawns

            //gelic wings
            player.FargoSouls().GelicWingsItem = Item;
            //player.AddEffect<GelicWingJump>(Item);

            //mutant antibodies
            player.buffImmune[BuffID.Wet] = true;
            player.buffImmune[BuffID.Rabies] = true;
            fargoPlayer.MutantAntibodies = true;
            if (player.mount.Active && player.mount.Type == MountID.CuteFishron)
                player.dripping = true;

            //lump of flesh
            player.buffImmune[BuffID.Blackout] = true;
            player.buffImmune[BuffID.Obstructed] = true;
            player.buffImmune[BuffID.Dazed] = true;
            fargoPlayer.CrystalSkull = true;
            fargoPlayer.PungentEyeball = true;
            player.AddEffect<PungentEyeballCursor>(Item);
            player.buffImmune[ModContent.BuffType<CrystalSkullBuff>()] = true;
            /*if (!player.ZoneDungeon)
            {
                player.npcTypeNoAggro[NPCID.SkeletonSniper] = true;
                player.npcTypeNoAggro[NPCID.SkeletonCommando] = true;
                player.npcTypeNoAggro[NPCID.TacticalSkeleton] = true;
                player.npcTypeNoAggro[NPCID.DiabolistRed] = true;
                player.npcTypeNoAggro[NPCID.DiabolistWhite] = true;
                player.npcTypeNoAggro[NPCID.Necromancer] = true;
                player.npcTypeNoAggro[NPCID.NecromancerArmored] = true;
                player.npcTypeNoAggro[NPCID.RaggedCaster] = true;
                player.npcTypeNoAggro[NPCID.RaggedCasterOpenCoat] = true;
            }*/


            //sinister icon
            player.AddEffect<SinisterIconEffect>(Item);
            player.AddEffect<SinisterIconDropsEffect>(Item);

            //dubious circuitry
            player.buffImmune[BuffID.CursedInferno] = true;
            player.buffImmune[BuffID.Ichor] = true;
            player.buffImmune[BuffID.Electrified] = true;
            fargoPlayer.FusedLens = true;
            fargoPlayer.DubiousCircuitry = true;
            player.AddEffect<FusedLensInstall>(Item);
            player.AddEffect<FusedLensStats>(Item);
            player.AddEffect<DebuffInstallKeyEffect>(Item);
            player.AddEffect<RemoteControlDR>(Item);
            //player.AddEffect<ProbeMinionEffect>(Item);
            player.AddEffect<RemoteLightningEffect>(Item);
            player.AddEffect<ReinforcedStats>(Item);
            player.noKnockback = true;

            //magical bulb
            player.buffImmune[BuffID.Venom] = true;
            fargoPlayer.MagicalBulb = true;
            player.AddEffect<BulbKeyEffect>(Item);
            player.AddEffect<BulbRegenEffect>(Item);

            //ice queen's crown
            IceQueensShield.AddEffects(player, Item);

            //lihzahrd treasure
            player.buffImmune[ModContent.BuffType<DaybrokenBuff>()] = true;
            fargoPlayer.LihzahrdTreasureBoxItem = Item;
            player.AddEffect<LihzahrdGroundPound>(Item);
            player.AddEffect<DiveEffect>(Item);
            player.AddEffect<LihzahrdBoulders>(Item);

            //saucer control console
            player.AddEffect<AmmoCycleEffect>(Item);

            //betsy's heart
            player.buffImmune[BuffID.OgreSpit] = true;
            player.buffImmune[BuffID.WitheredWeapon] = true;
            player.buffImmune[BuffID.WitheredArmor] = true;
            fargoPlayer.BetsysHeartItem = Item;
            player.AddEffect<SpecialDashEffect>(Item);
            player.AddEffect<BetsyDashEffect>(Item);

            //pumpking's cape
            player.AddEffect<PumpkingsCapeEffect>(Item);
            player.AddEffect<ParryEffect>(Item);

            //celestial rune
            //player.AddEffect<CelestialRuneAttacks>(Item);

            //verdant doomsayer
            fargoPlayer.MoonChalice = true;

            //chalice of the moon
            player.buffImmune[BuffID.VortexDebuff] = true;
            //player.buffImmune[BuffID.ChaosState] = true;
            fargoPlayer.GravityGlobeEXItem = Item;
            player.AddEffect<ChalicePotionEffect>(Item);
            MinionsDeactivatedEffect.DeactivateMinions(fargoPlayer, Item);

            //heart of master
            fargoPlayer.MasochistHeart = true;
            player.buffImmune[BuffID.MoonLeech] = true;

            //precision seal
            fargoPlayer.PrecisionSeal = true;
            player.AddEffect<PrecisionSealHurtbox>(Item);

            //dread shell
            player.AddEffect<DreadShellEffect>(Item);
            player.AddEffect<ParryEffect>(Item);

            //sadism
            player.buffImmune[ModContent.BuffType<AnticoagulationBuff>()] = true;
            player.buffImmune[ModContent.BuffType<AntisocialBuff>()] = true;
            player.buffImmune[ModContent.BuffType<AtrophiedBuff>()] = true;
            player.buffImmune[ModContent.BuffType<BerserkedBuff>()] = true;
            player.buffImmune[ModContent.BuffType<BloodthirstyBuff>()] = true;
            player.buffImmune[ModContent.BuffType<ClippedWingsBuff>()] = true;
            player.buffImmune[ModContent.BuffType<CurseoftheMoonBuff>()] = true;
            player.buffImmune[ModContent.BuffType<DefenselessBuff>()] = true;
            player.buffImmune[ModContent.BuffType<FlamesoftheUniverseBuff>()] = true;
            //player.buffImmune[ModContent.BuffType<HallowIlluminatedBuff>()] = true;
            player.buffImmune[ModContent.BuffType<FusedBuff>()] = true;
            //player.buffImmune[ModContent.BuffType<GodEater>()] = true;
            player.buffImmune[ModContent.BuffType<HexedBuff>()] = true;
            player.buffImmune[ModContent.BuffType<InfestedBuff>()] = true;
            player.buffImmune[ModContent.BuffType<IvyVenomBuff>()] = true;
            player.buffImmune[ModContent.BuffType<JammedBuff>()] = true;
            player.buffImmune[ModContent.BuffType<LethargicBuff>()] = true;
            player.buffImmune[ModContent.BuffType<LightningRodBuff>()] = true;
            //player.buffImmune[ModContent.BuffType<LowGroundBuff>()] = true;
            player.buffImmune[ModContent.BuffType<MarkedforDeathBuff>()] = true;
            player.buffImmune[ModContent.BuffType<MidasBuff>()] = true;
            //player.buffImmune[ModContent.BuffType<PoweroftheCosmosBuff>()] = true;
            player.buffImmune[ModContent.BuffType<OiledBuff>()] = true;
            player.buffImmune[ModContent.BuffType<OceanicMaulBuff>()] = true;
            player.buffImmune[ModContent.BuffType<ReverseManaFlowBuff>()] = true;
            player.buffImmune[ModContent.BuffType<RottingBuff>()] = true;
            player.buffImmune[ModContent.BuffType<ShadowflameBuff>()] = true;
            player.buffImmune[ModContent.BuffType<SmiteBuff>()] = true;
            player.buffImmune[ModContent.BuffType<SqueakyToyBuff>()] = true;
            player.buffImmune[ModContent.BuffType<SwarmingBuff>()] = true;
            player.buffImmune[ModContent.BuffType<StunnedBuff>()] = true;
            player.buffImmune[ModContent.BuffType<UnluckyBuff>()] = true;
            player.buffImmune[ModContent.BuffType<UnstableBuff>()] = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()

            .AddIngredient(ModContent.ItemType<SinisterIcon>())
            .AddIngredient(ModContent.ItemType<SupremeDeathbringerFairy>())
            .AddIngredient(ModContent.ItemType<BionomicCluster>())
            .AddIngredient(ModContent.ItemType<DubiousCircuitry>())
            .AddIngredient(ModContent.ItemType<PureHeart>())
            .AddRecipeGroup("FargowiltasSouls:AnyLithosphericLantern")
            .AddIngredient(ModContent.ItemType<VerdantDoomsayerMask>())
            .AddIngredient(ModContent.ItemType<HeartoftheMasochist>())
            .AddIngredient(ModContent.ItemType<DeviatingEnergy>(), 15)
            .AddIngredient(ModContent.ItemType<AbomEnergy>(), 15)

            .AddTile<CrucibleCosmosSheet>()


            .Register();
        }
    }
}
