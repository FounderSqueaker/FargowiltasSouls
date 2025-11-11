using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class CactusEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(121, 158, 29);

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Blue;
            Item.value = 20000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<CactusEffect>(Item);
            player.AddEffect<CactusPassiveEffect>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CactusHelmet)
                .AddIngredient(ItemID.CactusBreastplate)
                .AddIngredient(ItemID.CactusLeggings)
                .AddIngredient(ItemID.CactusSword)
                .AddIngredient(ItemID.PinkPricklyPear)
                .AddIngredient(ItemID.Waterleaf)
                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Generic;
            tooltipColor = null;
            scaling = null;
            return CactusEffect.BaseDamage(Main.LocalPlayer);
        }
    }

    public class CactusEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<LifeHeader>();
        public override int ToggleItemType => ModContent.ItemType<CactusEnchant>();

        public override bool ExtraAttackEffect => true;

        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();

            if (modPlayer.CactusProcCD > 0)
            {
                modPlayer.CactusProcCD--;
            }
        }

        public override void TryAdditionalAttacks(Player player, int damage, DamageClass damageType)
        {
            if (player.whoAmI != Main.myPlayer || !HasEffectEnchant(player))
                return;

            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (modPlayer.CactusProcCD == 0)
            {
                CactusSpray(player, player.Center);
                modPlayer.CactusProcCD = 20;
            }
        }
        public static void CactusProc(NPC npc, Player player)
        {
            CactusSpray(player, npc.Center);
        }
        public static int BaseDamage(Player player) => FargoSoulsUtil.HighestDamageTypeScaling(player, player.FargoSouls().ForceEffect<CactusEnchant>() ? 40 : 16);
        private static void CactusSpray(Player player, Vector2 position)
        {
            int numNeedles = player.FargoSouls().ForceEffect<CactusEnchant>() ? Main.rand.Next(10, 17) : Main.rand.Next(5, 9);
            int range = player.FargoSouls().ForceEffect<CactusEnchant>() ? 8 : 4;
            float rngrot = Main.rand.NextFloat(0, MathHelper.TwoPi);
            for (int i = 0; i < numNeedles; i++)
            {
                Vector2 vel = Vector2.UnitX.RotatedBy((MathHelper.TwoPi / numNeedles * i) + Main.rand.NextFloat(-0.1f, 0.1f)).RotatedBy(rngrot) * (range + Main.rand.NextFloat(-0.5f, 0.5f));
                Projectile.NewProjectile(player.GetSource_EffectItem<CactusEffect>(), position, vel, ModContent.ProjectileType<CactusNeedle>(), BaseDamage(player), 5f, ai0: 1);
            }
        }
    }

    public class CactusPassiveEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<LifeHeader>();
        public override int ToggleItemType => ModContent.ItemType<CactusEnchant>();
    }
}
