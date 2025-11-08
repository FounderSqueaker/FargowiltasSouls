using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Projectiles.Accessories;
using FargowiltasSouls.Content.Projectiles.Accessories.HeartOfTheMaster;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
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
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Eternity
{
    //[AutoloadEquip(EquipType.Face)]
    public class IceQueensShield : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Accessories/Eternity", Name);
        public override bool Eternity => true;
        public override List<AccessoryEffect> ActiveSkillTooltips =>
            [AccessoryEffectLoader.GetEffect<IceShieldEffect>()];

        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(0, 6);
            Item.defense = 5;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.endurance += 0.05f;
            AddEffects(player, Item);
        }

        public static void AddEffects(Player player, Item item)
        {
            FargoSoulsPlayer fargoPlayer = player.FargoSouls();
            player.buffImmune[BuffID.Frozen] = true;
            player.AddEffect<IceShieldEffect>(item);
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Magic;
            tooltipColor = null;
            scaling = null;
            return IceShieldEffect.BaseDamage(Main.LocalPlayer);
        }
    }
    public class IceShieldEffect : AccessoryEffect
    {
        public override Header ToggleHeader => null;
        public override bool ActiveSkill => true;
        public override int ToggleItemType => ModContent.ItemType<IceQueensShield>();
        public static int CD => 60 * 10;
        public static int BaseDamage(Player player) => (int)(330 * player.ActualClassDamage(DamageClass.Magic));
        public override void PostUpdateEquips(Player player)
        {
            player.IncrementCooldownTowards<IceShieldEffect>(-1, 0);
        }
        public override void ActiveSkillJustPressed(Player player, bool stunned)
        {
            if (stunned)
                return;
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            // summon ice shield
            if (player.whoAmI == Main.myPlayer && player.HasEffect<IceShieldEffect>())
            {
                int shield = ModContent.ProjectileType<IceShield>();
                if (player.ownedProjectileCounts[shield] > 0)
                    return;

                if (player.GetCooldown<IceShieldEffect>() <= 0)
                {
                    SoundEngine.PlaySound(SoundID.Item30, player.Center);
                    Projectile.NewProjectile(player.GetSource_EffectItem<IceShieldEffect>(), player.Center, Vector2.Zero, shield, BaseDamage(player), 1f, player.whoAmI);
                }
            }
        }
        public override void ActiveSkillHeld(Player player, bool stunned)
        {
           
        }
        public override void ActiveSkillJustReleased(Player player, bool stunned)
        {
            // shatter shield
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.TypeAlive<IceShield>() && proj.owner == player.whoAmI)
                {
                    proj.damage *= 3;
                    proj.ai[0] = IceShield.ShatterTime - 1;
                }
            }
        }
    }
}