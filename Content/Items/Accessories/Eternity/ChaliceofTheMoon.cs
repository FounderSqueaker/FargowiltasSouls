using System.Collections.Generic;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Buffs.Minions;
using FargowiltasSouls.Content.Patreon.DanielTheRobot;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using FargowiltasSouls.Content.Projectiles.Accessories.ChaliceOfTheMoon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Eternity
{
    [LegacyName("GalacticGlobe")]
    public class ChaliceofTheMoon : SoulsItem
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
            //Item.defense = 10;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(0, 8);
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<MoonChaliceCup>();
            Item.UseSound = null;
            Item.useTime = 180;
            Item.useAnimation = 180;
        }
        /* public override bool? UseItem(Player player)
        {
            if (player.itemAnimation > 0 && player.itemTime == 0)
            {
                if (!Main.dedServ)
                {
                    int i;
                    if (player.itemAnimation >= 61)
                    {
                        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, MathHelper.Pi);
                        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, MathHelper.Pi);
                    }
                    else if (player.itemAnimation <= 30)
                    {
                        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, MathHelper.Pi);
                        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, MathHelper.Pi);
                    }
                    else
                    {
                        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, MathHelper.Pi);
                        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, MathHelper.Pi);
                    }

                    if (player.itemAnimation == 90)
                    {
                        for (i = 0; i < ChalicePotionEffect.ChaliceBuffsUse.Count; i += 3)
                        {
                            int buff = ChalicePotionEffect.ChaliceBuffsUse[i];
                            int duration = 108000; //buff == BuffID.Lucky ? 60 * 60 * 15 : 60 * 60 * 8;
                            player.AddBuff(buff, duration);
                        }
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_3") { Pitch = -0.6f });
                    }
                    else if (player.itemAnimation == 60)
                    {
                        for (i = 1; i < ChalicePotionEffect.ChaliceBuffsUse.Count; i += 3)
                        {
                            int buff = ChalicePotionEffect.ChaliceBuffsUse[i];
                            int duration = 108000; //buff == BuffID.Lucky ? 60 * 60 * 15 : 60 * 60 * 8;
                            player.AddBuff(buff, duration);
                        }
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_3") { Pitch = -0.4f });
                    }
                    else if (player.itemAnimation == 30)
                    {
                        for (i = 2; i < ChalicePotionEffect.ChaliceBuffsUse.Count; i += 3)
                        {
                            int buff = ChalicePotionEffect.ChaliceBuffsUse[i];
                            int duration = 108000; //buff == BuffID.Lucky ? 60 * 60 * 15 : 60 * 60 * 8;
                            player.AddBuff(buff, duration);
                        }
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_3") { Pitch = -0.2f });
                    }
                }
            }
            return base.UseItem(player);
        }
        */
        public override void UpdateInventory(Player player)
        {
            player.AddEffect<ChalicePotionEffect>(Item);
        }
        public override void UpdateVanity(Player player)
        {
            player.AddEffect<ChalicePotionEffect>(Item);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[ModContent.BuffType<CurseoftheMoonBuff>()] = true;
            //player.buffImmune[ModContent.BuffType<PoweroftheCosmosBuff>()] = true;

            player.AddEffect<ChalicePotionEffect>(Item);
            player.AddEffect<MasoTrueEyeMinion>(Item);

            player.FargoSouls().GravityGlobeEXItem = Item;
            player.FargoSouls().WingTimeModifier += 1f;
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Summon;
            tooltipColor = null;
            scaling = null;
            return (int)(MasoTrueEyeMinion.BaseDamage(Main.LocalPlayer) * Main.LocalPlayer.ActualClassDamage(DamageClass.Summon));
        }
        public override void SafeModifyTooltips(List<TooltipLine> tooltips)
        {
            
        }
    }
    public class ChalicePotionEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<HeartHeader>();
        public override int ToggleItemType => ModContent.ItemType<ChaliceofTheMoon>();
        public static List<int> ChaliceBuffs =
        [
            // potions
            BuffID.Ironskin,
            BuffID.Regeneration,
            BuffID.Swiftness,
            BuffID.ManaRegeneration,
            BuffID.MagicPower,
            BuffID.AmmoReservation,
            BuffID.Archery,
            BuffID.Builder,
            BuffID.Crate,
            BuffID.Endurance,
            BuffID.Fishing,
            BuffID.Gills,
            BuffID.Lucky,
            BuffID.Heartreach,
            BuffID.Lifeforce,
            BuffID.Mining,
            BuffID.ObsidianSkin,
            BuffID.Rage,
            BuffID.Wrath,
            BuffID.Sonar,
            BuffID.Summoning,
            BuffID.Thorns,
            BuffID.Titan,
            BuffID.Warmth,
            BuffID.WaterWalking,
            BuffID.WellFed3,
            // buff stations
            BuffID.Sharpened,
            BuffID.AmmoBox,
            BuffID.Clairvoyance,
            BuffID.Bewitched,
            BuffID.WarTable
           // BuffID.Honey
        ];
        public static List<int> ChaliceBuffsUse =
        [
            BuffID.AmmoReservation,
            BuffID.Archery,
            BuffID.Battle,
            BuffID.Builder,
            BuffID.BiomeSight,
            BuffID.Calm,
            BuffID.Crate,
            BuffID.Dangersense,
            BuffID.Endurance,
            BuffID.WellFed3,
            BuffID.Featherfall,
            BuffID.Fishing,
            BuffID.Flipper,
            BuffID.Gills,
            BuffID.Gravitation,
            BuffID.Heartreach,
            BuffID.Hunter,
            BuffID.Inferno,
            BuffID.Invisibility,
            BuffID.Ironskin,
            BuffID.Lifeforce,
            BuffID.Lucky,
            BuffID.MagicPower,
            BuffID.ManaRegeneration,
            BuffID.Mining,
            BuffID.NightOwl,
            BuffID.ObsidianSkin,
            BuffID.Rage,
            BuffID.Regeneration,
            BuffID.Shine,
            BuffID.Sonar,
            BuffID.Spelunker,
            BuffID.Summoning,
            BuffID.Swiftness,
            BuffID.Thorns,
            BuffID.Titan,
            BuffID.Warmth,
            BuffID.WaterWalking,
            BuffID.Wrath,
            BuffID.AmmoBox,
            BuffID.Bewitched,
            BuffID.Clairvoyance,
            BuffID.Sharpened,
            BuffID.WarTable,
        ];
        public override void PostUpdateEquips(Player player)
        {
            if (ModContent.TryFind("Fargowiltas", "Omnistation", out ModBuff omnibuff))
                ChaliceBuffs.Add(omnibuff.Type);
            foreach (int buff in ChaliceBuffs)
            {
                int duration = buff == BuffID.Lucky ? 60 * 60 * 15 : 2;
                player.AddBuff(buff, duration);
            }
        }
    }
    public class MasoTrueEyeMinion : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<HeartHeader>();
        public override int ToggleItemType => ModContent.ItemType<ChaliceofTheMoon>();
        public override bool MinionEffect => true;
        public static int BaseDamage(Player player) => 60;
        public override void PostUpdateEquips(Player player)
        {
            if (!player.HasBuff<SouloftheMasochistBuff>())
                player.AddBuff(ModContent.BuffType<TrueEyesBuff>(), 2);
        }
    }
    public class MinionsDeactivatedEffect : AccessoryEffect
    {
        public static void DeactivateMinions(FargoSoulsPlayer modPlayer, Item item)
        {
            if (modPlayer.Player.AddEffect<MinionsDeactivatedEffect>(item))
                modPlayer.GalacticMinionsDeactivated = modPlayer.GalacticMinionsDeactivatedBuffer = true;
        }
        public override Header ToggleHeader => Header.GetHeader<HeartHeader>();
        public override int ToggleItemType => EffectItem(Main.LocalPlayer) != null ? EffectItem(Main.LocalPlayer).type : -1;
    }
}
