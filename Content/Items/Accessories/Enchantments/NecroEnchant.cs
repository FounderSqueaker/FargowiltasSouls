using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class NecroEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(86, 86, 67);


        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Orange;
            Item.value = 50000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<NecroEffect>(Item);
        }


        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.NecroHelmet)
                .AddIngredient(ItemID.NecroBreastplate)
                .AddIngredient(ItemID.NecroGreaves)
                .AddIngredient(ItemID.BookofSkulls)
                .AddIngredient(ItemID.BoneWhip)
                .AddIngredient(ItemID.GravediggerShovel)

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
    }
    public class NecroEffect : AccessoryEffect
    {

        public override Header ToggleHeader => Header.GetHeader<ShadowHeader>();
        public override int ToggleItemType => ModContent.ItemType<NecroEnchant>();
        public override bool ExtraAttackEffect => true;
        public override bool MutantsPresenceAffects => true;
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (modPlayer.NecroCD != 0)
                modPlayer.NecroCD--;
        }
        public static void NecroSpawnGraveEnemy(NPC npc, Player player, FargoSoulsPlayer modPlayer)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<NecroGrave>()] < 15)
            {
                int damage = npc.lifeMax / 4;
                if (damage > 5)
                    Projectile.NewProjectile(player.GetSource_Accessory(player.EffectItem<NecroEffect>()), npc.Bottom, new Vector2(0, -4), ModContent.ProjectileType<NecroGrave>(), 0, 0, player.whoAmI, damage);

                //if (modPlayer.ShadowForce || modPlayer.WizardEnchantActive)
                //{
                //    modPlayer.NecroCD = 15;
                //}
                //else
                //{
                //    modPlayer.NecroCD = 30;
                //}
            }
        }
        public static void NecroSpawnGraveBoss(FargoSoulsGlobalNPC globalNPC, NPC npc, Player player, int damage)
        {
            globalNPC.NecroDamage += damage;

            if (globalNPC.NecroDamage > npc.lifeMax / 10 && player.ownedProjectileCounts[ModContent.ProjectileType<NecroGrave>()] < 45)
            {
                globalNPC.NecroDamage = 0;

                int dam = npc.lifeMax / 40;
                if (dam > 0)
                    Projectile.NewProjectile(player.GetSource_Accessory(player.EffectItem<NecroEffect>()), npc.Bottom, new Vector2(0, -4), ModContent.ProjectileType<NecroGrave>(), 0, 0, player.whoAmI, dam);
            }
        }
    }
}
