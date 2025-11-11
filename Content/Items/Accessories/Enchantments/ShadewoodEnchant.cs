using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static FargowiltasSouls.Content.Items.Accessories.Forces.TimberForce;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class ShadewoodEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(88, 104, 118);


        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Blue;
            Item.value = 10000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<ShadewoodEffect>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ShadewoodHelmet)
                .AddIngredient(ItemID.ShadewoodBreastplate)
                .AddIngredient(ItemID.ShadewoodGreaves)
                .AddIngredient(ItemID.ViciousMushroom)
                .AddRecipeGroup("FargowiltasSouls:RambutanOrBloodOrange")
                .AddIngredient(ItemID.Deathweed)

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            Player player = Main.LocalPlayer;
            scaling = (int)((player.HeldItem.damage + player.FindAmmo([player.HeldItem.useAmmo]).damage) * player.ActualClassDamage(player.HeldItem.DamageType));
            if (scaling < 0)
                scaling = 0;

            scaling = (int)ShadewoodEffect.GetScaledDamage(player, (float)scaling);

            damageClass = DamageClass.Melee;
            tooltipColor = null;
            return 100;
        }
    }
    public class ShadewoodEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<TimberHeader>();
        public override int ToggleItemType => ModContent.ItemType<ShadewoodEnchant>();
        public static int Range(Player player, bool forceEffect) => (int)((forceEffect ? 400f : 200f) * (1f + player.FargoSouls().AuraSizeBonus));
        public static int MaxCharge = 180;
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();

            if (player.whoAmI != Main.myPlayer)
                return;
            bool forceEffect = modPlayer.ForceEffect<ShadewoodEnchant>();
            int dist = Range(player, forceEffect);
            bool found = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    Vector2 npcComparePoint = FargoSoulsUtil.ClosestPointInHitbox(npc, player.Center);
                    if (player.Distance(npcComparePoint) < dist && (forceEffect || Collision.CanHitLine(player.Center, 0, 0, npcComparePoint, 0, 0)))
                    {
                        if (!found)
                        {
                            found = true;
                            modPlayer.ShadewoodCharge += 1;
                        }
                        else
                            modPlayer.ShadewoodCharge += 0.1f;
                        if (Main.rand.NextBool(3))
                        {
                            int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood);
                            Main.dust[d].velocity = Main.dust[d].position.DirectionTo(player.Center) * 4f;
                        }
                    }
                        
                }
            }
            if (!found)
                modPlayer.ShadewoodCharge -= 0.1f;
            if (modPlayer.ShadewoodCharge >= MaxCharge)
                modPlayer.ShadewoodCharge = MaxCharge;
            if (player.whoAmI == Main.myPlayer)
                CooldownBarManager.Activate("ShadewoodEnchantBuildup", FargoAssets.GetTexture2D("Content/Items/Accessories/Enchantments", "ShadewoodEnchant").Value, Color.DarkRed,
                    () => (int)(Main.LocalPlayer.FargoSouls().ShadewoodCharge) / (float)MaxCharge, true, activeFunction: player.HasEffect<ShadewoodEffect>);
            if (!MoltenAuraProj.CombinedAura(player))
            {
                int visualProj = ModContent.ProjectileType<ShadewoodAuraProj>();
                if (player.ownedProjectileCounts[visualProj] <= 0)
                {
                    Projectile.NewProjectile(GetSource_EffectItem(player), player.Center, Vector2.Zero, visualProj, 0, 0, Main.myPlayer);
                }
            }

        }
        public static float GetScaledDamage(Player player, float sourceDamage)
        {
            bool force = player.ForceEffect<ShadewoodEffect>();
            float softcapMult = force ? 2.5f : 1f;

            if (sourceDamage > 50f * softcapMult)
                sourceDamage = ((100f * softcapMult) + sourceDamage) / 3f;

            sourceDamage += player.ForceEffect<ShadewoodEffect>() ? 45 : 15;
            return sourceDamage;
        }
        public override void OnHitNPCEither(Player player, NPC target, NPC.HitInfo hitInfo, DamageClass damageClass, int baseDamage, Projectile projectile, Item item)
        {
            ShadewoodProc(player, target, projectile, baseDamage);
        }
        public static void ShadewoodProc(Player player, NPC target, Projectile projectile, int baseDamage)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            bool forceEffect = modPlayer.ForceEffect<ShadewoodEnchant>();
            if (modPlayer.ShadewoodCharge >= MaxCharge && (projectile == null || projectile.type != ModContent.ProjectileType<SuperBlood>()) && player.whoAmI == Main.myPlayer)
            {
                modPlayer.ShadewoodCharge = 0;

                float explosionDamage = GetScaledDamage(player, baseDamage);
                float dirX = player.HorizontalDirectionTo(target.Center);
                float max = 9;
                for (int i = 0; i < max; i++)
                {
                    Vector2 vel = player.DirectionTo(target.Center).RotatedBy(MathHelper.PiOver2 * -0.15f * dirX + MathHelper.PiOver2 * 0.6f * (i - max / 2) / max).RotatedByRandom(MathHelper.PiOver2 * 0.05f);
                    vel *= Main.rand.NextFloat(4, 6);
                    vel *= 2;
                    Vector2 center = Main.rand.NextVector2FromRectangle(target.Hitbox);
                    Projectile.NewProjectile(player.GetSource_Misc(""), center, vel, ModContent.ProjectileType<SuperBlood>(), (int)explosionDamage, 0f, Main.myPlayer);
                }

                if (forceEffect)
                {
                    target.AddBuff(BuffID.Ichor, 120);
                }
            }
        }
    }
}
