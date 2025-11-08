using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Projectiles.Accessories.BionomicCluster;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Eternity
{
    public class TimsConcoction : SoulsItem
    {
        public override List<AccessoryEffect> ActiveSkillTooltips =>
            [AccessoryEffectLoader.GetEffect<TimsInspectEffect>()];

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

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ActiveEffects(player, Item);
        }
        public static void ActiveEffects(Player player, Item item)
        {
            player.AddEffect<TimsConcoctionEffect>(item);
            player.AddEffect<TimsInspectEffect>(item);
        }
    }
    public class TimsConcoctionEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<BionomicHeader>();
        public override int ToggleItemType => ModContent.ItemType<TimsConcoction>();
        public override void PostUpdateEquips(Player player)
        {
            player.FargoSouls().TimsConcoction = true;
        }
    }

    public class TimsInspectEffect : AccessoryEffect
    {
        public override Header ToggleHeader => null;
        public override bool ActiveSkill =>  true;
        public override int ToggleItemType => ModContent.ItemType<TimsConcoction>();

        public override void ActiveSkillJustPressed(Player player, bool stunned)
        {
            if (player.GetCooldown<TimsInspectEffect>() > 0 || stunned)
                return;

            player.SetCooldown<TimsInspectEffect>(90);
            player.FargoSouls().TimsInspect = !player.FargoSouls().TimsInspect;
            SoundEngine.PlaySound(SoundID.Item130, player.Center);
            if (player.ownedProjectileCounts[ModContent.ProjectileType<TimsInspectProjectile>()] == 0)
                Projectile.NewProjectile(player.GetSource_Accessory(EffectItem(player)), player.Center, Vector2.Zero, ModContent.ProjectileType<TimsInspectProjectile>(), 0, 0f, player.whoAmI);
        }
    }
}