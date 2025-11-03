using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Materials;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Eternity
{
    [AutoloadEquip(EquipType.Front, EquipType.Back)]
    public class HeartoftheMasochist : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Accessories/Eternity", Name);
        public override bool Eternity => true;
        public override int NumFrames => 7;

        public override List<AccessoryEffect> ActiveSkillTooltips =>
            [AccessoryEffectLoader.GetEffect<BetsyDashEffect>(),
             AccessoryEffectLoader.GetEffect<ParryEffect>(),
             AccessoryEffectLoader.GetEffect<IceShieldEffect>(),
             AccessoryEffectLoader.GetEffect<AmmoCycleEffect>()];

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 7));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
            ItemID.Sets.ItemNoGravity[Item.type] = true;
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(0, 9);
            //Item.defense = 10;
        }

        public override void UpdateInventory(Player player)
        {
            player.AddEffect<AmmoCycleEffect>(Item);
            player.AddEffect<ChalicePotionEffect>(Item);
        }

        public override void UpdateVanity(Player player)
        {
            player.AddEffect<AmmoCycleEffect>(Item);
            player.AddEffect<ChalicePotionEffect>(Item);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<AmmoCycleEffect>(Item);

            FargoSoulsPlayer fargoPlayer = player.FargoSouls();
            player.GetDamage(DamageClass.Generic) += 0.08f;
            player.GetCritChance(DamageClass.Generic) += 8;
            fargoPlayer.MasochistHeart = true;
            player.endurance += 0.04f;

            //pumpking's cape
            player.AddEffect<PumpkingsCapeEffect>(Item);
            player.AddEffect<ParryEffect>(Item);

            //ice queen's crown
            player.buffImmune[BuffID.Frozen] = true;
            IceQueensShield.AddEffects(player, Item);

            //saucer control console
            player.buffImmune[BuffID.VortexDebuff] = true;
            player.buffImmune[ModContent.BuffType<UnstableBuff>()] = true;
            player.AddEffect<UfoMinionEffect>(Item);
            player.AddEffect<AmmoCycleEffect>(Item);

            //betsy's heart
            player.buffImmune[BuffID.OgreSpit] = true;
            player.buffImmune[BuffID.WitheredWeapon] = true;
            player.buffImmune[BuffID.WitheredArmor] = true;
            fargoPlayer.BetsysHeartItem = Item;
            player.AddEffect<BetsyDashEffect>(Item);

            //mutant antibodies
            player.buffImmune[ModContent.BuffType<OceanicMaulBuff>()] = true;
            fargoPlayer.MutantAntibodies = true;
            if (player.mount.Active && player.mount.Type == MountID.CuteFishron)
                player.dripping = true;

            //chalice of the moon
            player.buffImmune[ModContent.BuffType<CurseoftheMoonBuff>()] = true;
            //player.buffImmune[BuffID.ChaosState] = true;
            player.AddEffect<ChalicePotionEffect>(Item);
            player.AddEffect<MasoTrueEyeMinion>(Item);
            fargoPlayer.GravityGlobeEXItem = Item;
            MinionsDeactivatedEffect.DeactivateMinions(fargoPlayer, Item);

            //precision seal
            player.buffImmune[ModContent.BuffType<SmiteBuff>()] = true;
            fargoPlayer.PrecisionSeal = true;
            player.AddEffect<PrecisionSealHurtbox>(Item);

            //heart of maso
            player.buffImmune[BuffID.MoonLeech] = true;
            //player.buffImmune[ModContent.BuffType<PoweroftheCosmosBuff>()] = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()

            .AddIngredient(ModContent.ItemType<PumpkingsCape>())
            .AddIngredient(ModContent.ItemType<IceQueensShield>())
            .AddIngredient(ModContent.ItemType<SaucerControlConsole>())
            .AddIngredient(ModContent.ItemType<BetsysHeart>())
            .AddIngredient(ModContent.ItemType<MutantAntibodies>())
            .AddIngredient(ModContent.ItemType<PrecisionSeal>())
            .AddIngredient(ModContent.ItemType<ChaliceofTheMoon>())
            .AddIngredient(ItemID.LunarBar, 15)
            .AddIngredient(ModContent.ItemType<DeviatingEnergy>(), 10)

            .AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"))
            .Register();
        }
    }
}