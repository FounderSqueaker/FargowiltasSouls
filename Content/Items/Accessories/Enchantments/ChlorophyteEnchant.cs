using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class ChlorophyteEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(36, 137, 0);

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.LightPurple;
            Item.value = 150000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            //player.AddEffect<ChloroMinion>(item);
            player.FargoSouls().ChlorophyteEnchantActive = true;
            //player.AddEffect<JungleJumpEffect>(Item);
            //player.jumpBoost = true;
            //player.noFallDmg = true;
            player.AddEffect<JungleHerbEffect>(Item);
            player.AddEffect<JungleDashEffect>(Item);
            player.AddEffect<JungleSporesEffect>(Item);
            player.AddEffect<JungleDashCooldownEffect>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("FargowiltasSouls:AnyChloroHead")
                .AddIngredient(ItemID.ChlorophytePlateMail)
                .AddIngredient(ItemID.ChlorophyteGreaves)
                .AddIngredient(null, "JungleEnchant")
                .AddIngredient(ItemID.ChlorophyteClaymore)
                .AddIngredient(ItemID.JungleRose) // replace later

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Generic;
            tooltipColor = null;
            scaling = null;
            return JungleSporesEffect.BaseDamage(Main.LocalPlayer, true);
        }
    }
    public class JungleDashCooldownEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<NatureHeader>();
        public override int ToggleItemType => ModContent.ItemType<JungleEnchant>();

        public override void PostUpdateMiscEffects(Player player)
        {
            if (player.miscCounter % 4 == 3 && player.dashDelay > 0) // Reduced dash cooldown by 25%
                player.dashDelay--;
        }
    }
    /*public class ChloroMinion : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<NatureHeader>();
        public override int ToggleItemType => ModContent.ItemType<ChlorophyteEnchant>();
        public override bool MinionEffect => true;
        public static int BaseDamage(Player player)
        {
            int dmg = player.ForceEffect<ChloroMinion>() ? 50 : 30;
            if (player.HasEffect<NatureEffect>())
                dmg = 150;
            return dmg;
        }
        public override void PostUpdateEquips(Player player)
        {
            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[ModContent.ProjectileType<Chlorofuck>()] == 0)
            {
                const int max = 5;
                float rotation = 2f * (float)Math.PI / max;
                for (int i = 0; i < max; i++)
                {
                    Vector2 spawnPos = player.Center + new Vector2(60, 0f).RotatedBy(rotation * i);
                    FargoSoulsUtil.NewSummonProjectile(GetSource_EffectItem(player), spawnPos, Vector2.Zero, ModContent.ProjectileType<Chlorofuck>(), BaseDamage(player), 10f, player.whoAmI, Chlorofuck.Cooldown, rotation * i);
                }
            }
        }
    }*/
}
