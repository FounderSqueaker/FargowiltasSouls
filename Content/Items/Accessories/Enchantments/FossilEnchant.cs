using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class FossilEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(140, 92, 59);


        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Blue;
            Item.value = 40000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<FossilEffect>(Item);
            player.AddEffect<FossilBones>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FossilHelm)
                .AddIngredient(ItemID.FossilShirt)
                .AddIngredient(ItemID.FossilPants)
                .AddIngredient(ItemID.AmberStaff)
                .AddIngredient(ItemID.BoneDagger, 90)
                .AddIngredient(ItemID.AntlionClaw)

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
    }
    public class FossilEffect : AccessoryEffect
    {

        public override Header ToggleHeader => null;
        public static void FossilRevive(Player player)
        {
            if (!Main.dedServ)
                SoundEngine.PlaySound(FargosSoundRegistry.FossilRevive, player.Center);
            if (player.HasEffect<SpectreEffect>())
                return;

            static Projectile[] XWay(int num, IEntitySource spawnSource, Vector2 pos, int type, float speed, int damage, float knockback, int player)
            {
                Projectile[] projs = new Projectile[num];
                double spread = 2 * Math.PI / num;
                for (int i = 0; i < num; i++)
                    projs[i] = FargoSoulsUtil.NewProjectileDirectSafe(spawnSource, pos, new Vector2(speed, speed).RotatedBy(spread * i), type, damage, knockback, player);
                return projs;
            }

            FargoSoulsPlayer modPlayer = player.FargoSouls();

            void Revive(int healAmount, int reviveCooldown)
            {
                player.statLife = healAmount;
                player.HealEffect(healAmount);

                player.immune = true;
                player.immuneTime = 120;
                player.hurtCooldowns[0] = 120;
                player.hurtCooldowns[1] = 120;

                int max = player.buffType.Length;
                for (int i = 0; i < max; i++)
                {
                    int timeLeft = player.buffTime[i];
                    if (timeLeft <= 0)
                        continue;

                    int buffType = player.buffType[i];
                    if (buffType <= 0)
                        continue;

                    if (timeLeft > 5
                        && Main.debuff[buffType]
                        && !Main.buffNoTimeDisplay[buffType]
                        && !BuffID.Sets.NurseCannotRemoveDebuff[buffType])
                    {
                        player.DelBuff(i);

                        i--;
                        max--; //just in case, to prevent being stuck here forever
                    }
                }

                string text = Language.GetTextValue($"Mods.{FargowiltasSouls.Instance.Name}.Message.Revived");
                CombatText.NewText(player.Hitbox, Color.SandyBrown, text, true);
                Main.NewText(text, Color.SandyBrown);

                player.AddBuff(ModContent.BuffType<FossilReviveCDBuff>(), reviveCooldown);
            };

            bool forceEffect = modPlayer.ForceEffect<FossilEnchant>();
            Revive(forceEffect ? 200 : 50, 18000);
            if (player.HasEffect<FossilBones>())
                XWay(forceEffect ? 20 : 10, player.GetSource_EffectItem<FossilEffect>(), player.Center, ModContent.ProjectileType<FossilBone>(), 15, 0, 0, player.whoAmI);
        }
    }
    public class FossilBones : AccessoryEffect
    {

        public override Header ToggleHeader => Header.GetHeader<SpiritHeader>();
        public override int ToggleItemType => ModContent.ItemType<FossilEnchant>();
        public int damageCopy;
        public override void OnHurt(Player player, Player.HurtInfo info)
        {
            if (player.HasEffect<SpectreEffect>())
                return;
            //spawn bones
            damageCopy += info.Damage;
            for (int i = 0; i < 2; i++)
            {
                if (damageCopy < 40)
                    break;
                damageCopy -= 40 + (10*i);
                float velX = Main.rand.Next(-5, 6) * 3f;
                float velY = Main.rand.Next(-5, 6) * 3f;
                Projectile.NewProjectile(GetSource_EffectItem(player), player.position.X + velX, player.position.Y + velY, velX, velY, ModContent.ProjectileType<FossilBone>(), 0, 0f, player.whoAmI);
            }
        }
        public override void PostUpdateEquips(Player player)
        {
            if (damageCopy > 40 && !player.immune)
            {
                damageCopy -= damageCopy;
            }
        }
    }
}
