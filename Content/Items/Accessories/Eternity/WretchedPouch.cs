using System;
using System.Runtime.InteropServices;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Projectiles.Accessories.BionomicCluster;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Eternity
{
    [AutoloadEquip(EquipType.Waist)]
    public class WretchedPouch : SoulsItem
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
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(0, 4);
        }
        public static void ActiveEffects(Player player, Item item)
        {
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.buffImmune[ModContent.BuffType<ShadowflameBuff>()] = true;

            player.AddEffect<WretchedPouchEffect>(item);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ActiveEffects(player, Item);
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Magic;
            tooltipColor = null;
            scaling = null;
            return (int)(280 * Main.LocalPlayer.ActualClassDamage(DamageClass.Magic)); //display max charge damage
        }
    }
    public class WretchedPouchEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<LithosphericHeader>();
        public override int ToggleItemType => ModContent.ItemType<WretchedPouch>();
        public override bool ExtraAttackEffect => true;

        public override void PostUpdate(Player player) // PostUpdate necessary to be after attack speed
        {
            if (player.HasEffect<LithosphericEffect>()) // combined
                return;
            const int MaxChargeTime = 60 * 8;

            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (player.HeldItem != null && player.HeldItem.damage > 0 && player.controlUseItem) // build up charges
            {
                modPlayer.WretchedPouchCD += 1 * player.FargoSouls().CachedAttackSpeed;

                float charge = modPlayer.WretchedPouchCD / (float)MaxChargeTime;
                charge = MathHelper.Clamp(charge, 0, 1);
                int freq = 30 - (int)MathF.Round(29 * charge);

                if (!modPlayer.MasochistSoul)
                    player.endurance -= 0.1f * charge;

                if (Main.GameUpdateCount % freq == 0)
                {
                    int d = Dust.NewDust(player.position, player.width, player.height, DustID.Shadowflame, player.velocity.X * 0.4f, player.velocity.Y * 0.4f, 0, new Color(), 3f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 5f;
                }
                if (player.whoAmI == Main.myPlayer)
                    CooldownBarManager.Activate("WretchedPouchCharge", FargoAssets.GetTexture2D("Content/Items/Accessories/Eternity", "WretchedPouch").Value, Color.DarkMagenta, 
                        () => Main.LocalPlayer.FargoSouls().WretchedPouchCD / (float)MaxChargeTime, true, activeFunction: () => player.HasEffect<WretchedPouchEffect>());
            }
            else
            {
                // maximum charge: 8 seconds
                float charge = modPlayer.WretchedPouchCD / (float)MaxChargeTime;
                if (modPlayer.WretchedPouchCD > 0)
                    modPlayer.WretchedPouchCD--;
                if (charge < 0.2f)
                    return;
                charge = MathHelper.Clamp(charge, 0, 1);
                modPlayer.WretchedPouchCD = 0;

                if (player.whoAmI == Main.myPlayer)
                {
                    Vector2 vel = player.Center.DirectionTo(Main.MouseWorld);

                    vel *= 11f;

                    SoundEngine.PlaySound(SoundID.Item103, player.Center);

                    int dam = 40 + (int)(40 * charge * 6);
                    dam = (int)(dam * player.ActualClassDamage(DamageClass.Magic));

                    void ShootTentacle(Vector2 baseVel, float variance, int aiMin, int aiMax)
                    {
                        Vector2 speed = baseVel.RotatedBy(variance * (Main.rand.NextDouble() - 0.5));
                        float ai0 = Main.rand.Next(aiMin, aiMax) * (1f / 1000f);
                        if (Main.rand.NextBool())
                            ai0 *= -1f;
                        float ai1 = Main.rand.Next(aiMin, aiMax) * (1f / 1000f);
                        if (Main.rand.NextBool())
                            ai1 *= -1f;
                        int p = Projectile.NewProjectile(GetSource_EffectItem(player), player.Center, speed, ModContent.ProjectileType<ShadowflameTentacle>(), dam, 4f, player.whoAmI, ai0, ai1);
                    };

                    int max = (int)(charge * 16);
                    for (int i = 0; i < max; i++) //shoot everywhere
                        ShootTentacle(vel, 1f, 30, 50);
                }

            }
        }
    }
}