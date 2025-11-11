using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class AncientShadowEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(94, 85, 220);

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Pink;
            Item.value = 100000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<AncientShadowBolt>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()

                .AddIngredient(ItemID.AncientShadowHelmet)
                .AddIngredient(ItemID.AncientShadowScalemail)
                .AddIngredient(ItemID.AncientShadowGreaves)
                .AddIngredient(ItemID.WarAxeoftheNight)
                .AddIngredient(ItemID.TentacleSpike)
                .AddIngredient(ItemID.PurpleClubberfish)

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Magic;
            tooltipColor = null;
            scaling = null;
            return (int)(AncientShadowBolt.BaseDamage(Main.LocalPlayer));
        }
    }
    public class AncientShadowBolt : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<ShadowHeader>();
        public override int ToggleItemType => ModContent.ItemType<AncientShadowEnchant>();
        public override void PostUpdateMiscEffects(Player player)
        {
            if (player.FargoSouls().AncientShadowCD > 0)
                player.FargoSouls().AncientShadowCD--;
        }
        public override void OnHitNPCEither(Player player, NPC target, NPC.HitInfo hitInfo, DamageClass damageClass, int baseDamage, Projectile projectile, Item item)
        {
            if (hitInfo.Crit)
            {
                AshadowProc(player, target);
                FargoSoulsPlayer modPlayer = player.FargoSouls();
                // stack effect
                modPlayer.AncientShadowCounter++;
            }
        }
        public static int BaseDamage(Player player) => (int)((player.ForceEffect<AncientShadowBolt>() ? 105 : 20) * player.ActualClassDamage(DamageClass.Magic));
        public static void AshadowProc(Player player, NPC target)
        {
            if (!player.HasEffectEnchant<AncientShadowBolt>())
                return;
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (modPlayer.AncientShadowCD <= 0)
            {
                int cdLength = 60 * 4;

                int damage = BaseDamage(player);

                static float DamageFormula(float x) => x / MathF.Sqrt(x * x + 1); // decaying linear scaling
                float mult = 1f + 6f * DamageFormula(modPlayer.AncientShadowCounter / 80f);
                damage = (int)(damage * mult);

                for (int i = 0; i < 2; i++)
                {
                    int side = i == 0 ? 1 : -1;
                    Projectile.NewProjectile(player.GetSource_EffectItem<AncientShadowBolt>(), player.Center, player.DirectionTo(target.Center) * 20, ModContent.ProjectileType<ShadowBolt>(),
                        damage, 0, player.whoAmI, ai1: side);
                }

                SoundEngine.PlaySound(SoundID.Item103, player.Center);


                modPlayer.AncientShadowCD = cdLength;
                modPlayer.AncientShadowCounter = 0;
            }
        }
    }
}
