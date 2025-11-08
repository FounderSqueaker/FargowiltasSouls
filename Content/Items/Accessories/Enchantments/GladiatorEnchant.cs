using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.Projectiles.Weapons.Minions;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.ModPlayers;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class GladiatorEnchant : BaseEnchant
    {
        public override List<AccessoryEffect> ActiveSkillTooltips =>
            [AccessoryEffectLoader.GetEffect<GladiatorBanner>()];
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(156, 146, 78);


        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Blue;
            Item.value = 40000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<GladiatorBanner>(Item);
            player.AddEffect<GladiatorSpears>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GladiatorHelmet)
                .AddIngredient(ItemID.GladiatorBreastplate)
                .AddIngredient(ItemID.GladiatorLeggings)
                .AddIngredient(ItemID.Gladius)
                .AddIngredient(ItemID.Spear)
                .AddIngredient(ItemID.Javelin, 300)

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            Player player = Main.LocalPlayer;
            bool wiz = player.ForceEffect<GladiatorSpears>();
            bool buff = player.HasBuff<GladiatorBuff>();
            scaling = (int)((player.HeldItem.damage + player.FindAmmo([player.HeldItem.useAmmo]).damage) * player.ActualClassDamage(DamageClass.Ranged)) / (buff ? 4 : 6);
            if (scaling < 0)
                scaling = 0;
            if (wiz)
                scaling *= 2;

            int softcapMult = wiz ? 5 : 1;
            if (scaling > (10 * softcapMult))
                scaling = (int)((20 * softcapMult) + scaling) / 3;
            scaling = (int)Math.Round((decimal)scaling, MidpointRounding.AwayFromZero);

            int divisor = (buff ? 4 : 6) / (player.ForceEffect<GladiatorSpears>() ? 2 : 1);
            damageClass = DamageClass.Ranged;
            tooltipColor = null;
            return 100 / divisor;
        }
    }
    public class GladiatorBanner : AccessoryEffect
    {

        public override Header ToggleHeader => Main.LocalPlayer.HasEffect<WillEffect>() ? Header.GetHeader<WillHeader>() : null;
        public override int ToggleItemType => ModContent.ItemType<GladiatorEnchant>();
        public override bool ActiveSkill => Main.LocalPlayer.HasEffectEnchant<GladiatorBanner>();
        public override bool MutantsPresenceAffects => Main.LocalPlayer.HasEffect<WillEffect>();
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (player.HasEffect<WillEffect>())
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<GladiatorSpirit>()] == 0 && player.whoAmI == Main.myPlayer)
                {
                    Projectile proj = Projectile.NewProjectileDirect(GetSource_EffectItem(player), player.Center, Vector2.Zero, ModContent.ProjectileType<GladiatorSpirit>(), 0, 0f, player.whoAmI);
                    proj.netUpdate = true;
                }
            }
            player.IncrementCooldownTowards<GladiatorBanner>(-1, 0);
            if (player.HasBuff<GladiatorBuff>())
            {
                float stats = 0.08f;
                if (modPlayer.ForceEffect<GladiatorEnchant>())
                    stats = 0.16f;
                //player.GetDamage(DamageClass.Generic) += stats;
                player.endurance += stats;
                player.noKnockback = true;
            }
        }
        public override void ActiveSkillJustPressed(Player player, bool stunned)
        {
            if (!stunned)
                ActivateGladiatorBanner(player);
        }
        public static void ActivateGladiatorBanner(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (!player.HasEffectEnchant<GladiatorBanner>())
                return;
            if (player.whoAmI == Main.myPlayer && player.HasEffect<GladiatorBanner>())
            {
                int GladiatorStandard = ModContent.ProjectileType<GladiatorStandard>();

                if (player.ownedProjectileCounts[GladiatorStandard] <= 0)
                    player.SetCooldown<GladiatorBanner>(0);

                if (player.GetCooldown<GladiatorBanner>() <= 0)
                {
                    foreach (Projectile p in Main.projectile.Where(p => p.TypeAlive(GladiatorStandard) && p.owner == player.whoAmI))
                        p.Kill();
                    Projectile.NewProjectile(player.GetSource_EffectItem<GladiatorBanner>(), player.Top, Vector2.UnitY * 25, GladiatorStandard, modPlayer.ForceEffect<GladiatorEnchant>() ? 200 : 50, 3f, player.whoAmI);
                    player.SetCooldown<GladiatorBanner>(LumUtils.SecondsToFrames(15));

                    if (player.whoAmI == Main.myPlayer)
                        CooldownBarManager.Activate("GladiatorStandardCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Enchantments", "GladiatorEnchant").Value, new(156, 146, 78), 
                            () => 1f - (float)Main.LocalPlayer.GetCooldown<GladiatorBanner>() / LumUtils.SecondsToFrames(15), activeFunction: () => player.HasEffect<GladiatorBanner>());
                }
            }
        }
    }
    public class GladiatorSpears : AccessoryEffect
    {

        public override Header ToggleHeader => Header.GetHeader<WillHeader>();
        public override int ToggleItemType => ModContent.ItemType<GladiatorEnchant>();

        public override bool ExtraAttackEffect => true;
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (modPlayer.GladiatorCD > 0)
                modPlayer.GladiatorCD--;
        }
        public override void OnHitNPCEither(Player player, NPC target, NPC.HitInfo hitInfo, DamageClass damageClass, int baseDamage, Projectile projectile, Item item)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (player.whoAmI == Main.myPlayer && modPlayer.GladiatorCD <= 0 && (projectile == null || projectile.type != ModContent.ProjectileType<GladiatorJavelin>()))
            {
                bool force = modPlayer.ForceEffect<GladiatorEnchant>();

                bool buff = player.HasBuff<GladiatorBuff>();
                float spearDamage = baseDamage / (buff ? 4f : 6f);
                spearDamage *= player.ActualClassDamage(DamageClass.Ranged) / player.ActualClassDamage(hitInfo.DamageType);
                spearDamage = (float)Math.Round(spearDamage);

                if (force)
                    spearDamage *= 2;

                if ((int)spearDamage > 0)
                {
                    float softcapMult = force ? 5f : 1f;
                    if (spearDamage > (10f * softcapMult))
                        spearDamage = ((20f * softcapMult) + spearDamage) / 3f; // refer to Boreal Wood Enchantment for new softcap

                    Item effectItem = EffectItem(player);
                    for (int i = 0; i < 3; i++)
                    {
                        const int arrivalTime = 15;
                        Vector2 spawn = new(target.Center.X + Main.rand.NextFloat(-300, 300), target.Center.Y - Main.rand.Next(600, 801));
                        Vector2 aim = target.Center + (target.velocity * arrivalTime * Main.rand.NextFloat(0.7f, 1.3f));
                        float speed = (aim - spawn).Length() / arrivalTime * Main.rand.NextFloat(0.8f, 1.2f);
                        //Vector2 speed = target.Center + target.velocity * i * 5 * Main.rand.NextFloat(0.5f, 1.5f) - spawn;
                        //speed.Normalize();
                        //speed *= 15f * Main.rand.NextFloat(0.8f, 1.2f);

                        Projectile.NewProjectile(player.GetSource_Accessory(effectItem), spawn, Vector2.Normalize(aim - spawn).RotatedByRandom(MathHelper.Pi / 20) * speed, ModContent.ProjectileType<GladiatorJavelin>(), (int)spearDamage, 4f, Main.myPlayer);
                    }

                    modPlayer.GladiatorCD = 30;
                    modPlayer.GladiatorCD = buff ? modPlayer.GladiatorCD : (int)Math.Round(modPlayer.GladiatorCD * 1.5f);
                }
            }
        }
    }
}
