using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Cavern;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class SpookyEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override List<AccessoryEffect> ActiveSkillTooltips => 
            [AccessoryEffectLoader.GetEffect<SpookyEffect>()];
        public override Color nameColor => new(100, 78, 116);


        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Lime;
            Item.value = 250000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<SpookyEffect>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.SpookyHelmet)
            .AddIngredient(ItemID.SpookyBreastplate)
            .AddIngredient(ItemID.SpookyLeggings)
            .AddIngredient(ItemID.RavenStaff)
            .AddIngredient(ItemID.DeathSickle)
            .AddIngredient(ItemID.ButchersChainsaw)

            //psycho knife
            //eoc yoyo
            //dark harvest
            //.AddIngredient(ItemID.CursedSapling);
            //.AddIngredient(ItemID.EyeSpring);

                .AddTile<EnchantedTreeSheet>()
            .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.SummonMeleeSpeed; //using this for melee/summon as a temp solution
            tooltipColor = Color.Lerp(Color.Lerp(new(225, 90, 90), new(0, 80, 224), 0.5f), Color.LightGray, 0.3f);
            scaling = null;
            return SpookyEffect.BaseDamage(Main.LocalPlayer);
        }
    }
    public class SpookyEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<ShadowHeader>();
        public override int ToggleItemType => ModContent.ItemType<SpookyEnchant>();
        public override bool ActiveSkill => Main.LocalPlayer.HasEffectEnchant<SpookyEffect>();
        public static int BaseDamage(Player player) => (int)((player.ForceEffect<SpookyEffect>() ? 1250 : 750) * (((player.ActualClassDamage(DamageClass.Melee) + player.ActualClassDamage(DamageClass.Summon) - 2f) / 2f) + 1f)); // melee-summon 50-50 damage scaling
        public override void PostUpdateEquips(Player player)
        {
            if (player.IncrementCooldownTowards<SpookyEffect>(-1, 0))
            {
                if (player.GetCooldown<SpookyEffect>() == 1)
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Volume = 2f }, player.Center);
            }
        }
        public override void ActiveSkillJustPressed(Player player, bool stunned)
        {
            if (stunned)
                return;
            if (player.GetCooldown<SpookyEffect>() > 0)
                return;
            bool wiz = player.ForceEffect<SpookyEffect>();
            if (Main.myPlayer == player.whoAmI)
                Projectile.NewProjectile(GetSource_EffectItem(player), player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<SpookySpinScythe>(), BaseDamage(player), 1f, player.whoAmI);
            int cd = wiz ? 8 : 12;
            player.SetCooldown<SpookyEffect>(cd * 60);
            CooldownBarManager.Activate("SpookyEnchantCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Enchantments", "SpookyEnchant").Value, new(100, 78, 116),
                () => Main.LocalPlayer.GetCooldown<SpookyEffect>() / (cd * 60f), true, activeFunction: player.HasEffect<SpookyEffect>);
        }
    }
}
