using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Projectiles.Weapons.Minions;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Eternity
{
    public class MutantEye : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Accessories/Eternity", Name);
        public override bool Eternity => true;
        public override List<AccessoryEffect> ActiveSkillTooltips =>
            [AccessoryEffectLoader.GetEffect<BombKeyEffect>()];
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 18));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;

            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(1);
        }


        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            FargoSoulsPlayer fargoPlayer = player.FargoSouls();

            player.buffImmune[ModContent.BuffType<Buffs.Boss.MutantFangBuff>()] = true;
            //player.buffImmune[ModContent.BuffType<Buffs.Boss.MutantPresence>()] = true;

            fargoPlayer.MutantEyeItem = Item;
            player.AddEffect<BombKeyEffect>(Item);
            if (!hideVisual)
                fargoPlayer.MutantEyeVisual = true;

            if (player.IncrementCooldownTowards<BombKeyEffect>(-1, 0))
            {
                if (player.GetCooldown<BombKeyEffect>() == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item4, player.Center);

                    const int max = 50; //make some indicator dusts
                    for (int i = 0; i < max; i++)
                    {
                        Vector2 vector6 = Vector2.UnitY * 8f;
                        vector6 = vector6.RotatedBy((i - (max / 2 - 1)) * 6.28318548f / max) + Main.LocalPlayer.Center;
                        Vector2 vector7 = vector6 - Main.LocalPlayer.Center;
                        int d = Dust.NewDust(vector6 + vector7, 0, 0, DustID.Vortex, 0f, 0f, 0, default, 2f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity = vector7;
                    }

                    for (int i = 0; i < 30; i++)
                    {
                        int d = Dust.NewDust(player.position, player.width, player.height, DustID.Vortex, 0f, 0f, 0, default, 2.5f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].noLight = true;
                        Main.dust[d].velocity *= 8f;
                    }
                }
            }

            if (player.whoAmI == Main.myPlayer && fargoPlayer.MutantEyeVisual && player.GetCooldown<BombKeyEffect>() <= 0
                && player.ownedProjectileCounts[ModContent.ProjectileType<PhantasmalRing2>()] <= 0)
            {
                Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<PhantasmalRing2>(), 0, 0f, Main.myPlayer);
            }

            if (fargoPlayer.AbomWandCD > 0)
                fargoPlayer.AbomWandCD--;
        }
    }
    public class BombKeyEffect : AccessoryEffect
    {
        public override Header ToggleHeader => null;
        public override bool ActiveSkill => true;
        public override int ToggleItemType => ModContent.ItemType<MutantEye>();
        public override void ActiveSkillJustPressed(Player player, bool stunned)
        {
            if (stunned)
                return;
            player.FargoSouls().BombKey();
        }
    }
}