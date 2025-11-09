using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.ModPlayers;
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
    public class SpectreEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

        }

        public override Color nameColor => new(172, 205, 252);


        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Lime;
            Item.value = 250000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<SpectreEffect>(Item);
            player.AddEffect<SpectreOnHitEffect>(Item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("FargowiltasSouls:AnySpectreHead")
            .AddIngredient(ItemID.SpectreRobe)
            .AddIngredient(ItemID.SpectrePants)
            .AddIngredient<FossilEnchant>()
            .AddIngredient(ItemID.SpectreStaff)
            .AddIngredient(ItemID.AbigailsFlower)

                .AddTile<EnchantedTreeSheet>()
            .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Magic;
            tooltipColor = null;
            scaling = null;
            return SpectreOnHitEffect.BaseDamage(Main.LocalPlayer);
        }
    }
    public class SpectreEffect : AccessoryEffect
    {

        public override Header ToggleHeader => null;
        public static void SpectreRevive(Player player)
        {   
            if (!Main.dedServ)
                SoundEngine.PlaySound(FargosSoundRegistry.SpectreRevive, player.Center);
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            static Projectile[] XWay(int num, IEntitySource spawnSource, Vector2 pos, int type, float speed, int damage, float knockback, int player)
            {
                Projectile[] projs = new Projectile[num];
                double spread = 2 * Math.PI / num;
                for (int i = 0; i < num; i++)
                    projs[i] = FargoSoulsUtil.NewProjectileDirectSafe(spawnSource, pos, new Vector2(speed, speed).RotatedBy(spread * i), type, damage, knockback, player);
                return projs;
            }

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

                if (player.HasEffectEnchant<SpectreEffect>() && !modPlayer.TerrariaSoul && !modPlayer.Eternity)
                    modPlayer.SpectreGhostTime = LumUtils.SecondsToFrames(5f);

                string text = Language.GetTextValue($"Mods.{FargowiltasSouls.Instance.Name}.Message.Revived");
                CombatText.NewText(player.Hitbox, Color.LightCyan, text, true);
                Main.NewText(text, Color.LightCyan);

                player.AddBuff(ModContent.BuffType<FossilReviveCDBuff>(), reviveCooldown);
            };

            if (modPlayer.Eternity)
            {
                Revive(player.statLifeMax2 / 2 > 300 ? player.statLifeMax2 / 2 : 300, 10800);
                //if (player.HasEffect<SpectreOnHitEffect>())
                 //   XWay(30, player.GetSource_Misc("FossilEnchant"), player.Center, ModContent.ProjectileType<FossilBone>(), 15, spiritDamage, 0, player.whoAmI);
            }
            else if (modPlayer.TerrariaSoul)
            {
                Revive(300, 14400);
                //if (player.HasEffect<SpectreOnHitEffect>())
                //    XWay(25, player.GetSource_Misc("FossilEnchant"), player.Center, ModContent.ProjectileType<FossilBone>(), 15, spiritDamage, 0, player.whoAmI);
            }
            else
            {
                bool forceEffect = modPlayer.ForceEffect<SpectreEnchant>();
                Revive(forceEffect ? 200 : 100, 18000);
                if (player.HasEffect<SpectreOnHitEffect>())
                    XWay(forceEffect ? 20 : 10, player.GetSource_EffectItem<SpectreEffect>(), player.Center, ModContent.ProjectileType<SpectreSpirit>(), 15, SpectreOnHitEffect.BaseDamage(player), 0, player.whoAmI);
            }
        }
        public static void GhostUpdate(Player player)
        {
            player.immune = true;
            player.immuneTime = 90;
            player.hurtCooldowns[0] = 90;
            player.hurtCooldowns[1] = 90;
            player.stealth = 1;
            player.controlUseItem = false;
            player.controlUseTile = false;
            player.cursed = true;
            player.controlThrow = false;
            player.controlMount = false;
            player.GrantInfiniteFlight();

            player.controlHook = false;
            player.RemoveAllGrapplingHooks();
            player.releaseHook = true;
            if (player.mount.Active)
                player.mount.Dismount(player);
            //fargoPlayer.Stunned = true;
            player.FargoSouls().NoUsingItems = 2;

            if (player.ownedProjectileCounts[ModContent.ProjectileType<SpectreGhostProj>()] <= 0)
                Projectile.NewProjectile(player.GetSource_EffectItem<SpectreEffect>(), player.Center.X, player.Center.Y, 0f, 0f, ModContent.ProjectileType<SpectreGhostProj>(), 0, 0, Main.myPlayer);
            player.moveSpeed *= 1.3f;
        }

        /*
        public override void OnHitNPCEither(Player player, NPC target, NPC.HitInfo hitInfo, DamageClass damageClass, int baseDamage, Projectile projectile, Item item)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (!target.immortal && modPlayer.SpectreCD <= 0 && Main.rand.NextBool())
            {
                bool spectreForceEffect = modPlayer.ForceEffect<SpectreEnchant>();
                if (projectile == null)
                {
                    //forced orb spawn reeeee
                    float num = 4f;
                    float speedX = Main.rand.Next(-100, 101);
                    float speedY = Main.rand.Next(-100, 101);
                    float num2 = (float)Math.Sqrt((double)(speedX * speedX + speedY * speedY));
                    num2 = num / num2;
                    speedX *= num2;
                    speedY *= num2;
                    Projectile p = FargoSoulsUtil.NewProjectileDirectSafe(GetSource_EffectItem(player), target.position, new Vector2(speedX, speedY), ProjectileID.SpectreWrath, hitInfo.Damage / 2, 0, player.whoAmI, target.whoAmI);

                    if ((spectreForceEffect || (hitInfo.Crit && Main.rand.NextBool(5))) && p != null)
                    {
                        SpectreHeal(player, target, p);
                        modPlayer.SpectreCD = spectreForceEffect ? 5 : 20;
                    }
                }
                else if (projectile.type != ProjectileID.SpectreWrath)
                {
                    SpectreHurt(projectile);

                    if (spectreForceEffect || (hitInfo.Crit && Main.rand.NextBool(5)))
                        SpectreHeal(player, target, projectile);

                    modPlayer.SpectreCD = spectreForceEffect ? 5 : 20;
                }
            }
        }
        public void SpectreHeal(Player player, NPC npc, Projectile proj)
        {
            if (npc.canGhostHeal && !player.moonLeech)
            {
                float num = 0.2f;
                num -= proj.numHits * 0.05f;
                if (num <= 0f)
                {
                    return;
                }
                float num2 = proj.damage * num;
                if ((int)num2 <= 0)
                {
                    return;
                }
                if (Main.player[Main.myPlayer].lifeSteal <= 0f)
                {
                    return;
                }
                Main.player[Main.myPlayer].lifeSteal -= num2 * 5; //original damage

                float num3 = 0f;
                int num4 = proj.owner;
                for (int i = 0; i < 255; i++)
                {
                    if (Main.player[i].active && !Main.player[i].dead && ((!Main.player[proj.owner].hostile && !Main.player[i].hostile) || Main.player[proj.owner].team == Main.player[i].team))
                    {
                        float num5 = Math.Abs(Main.player[i].position.X + (Main.player[i].width / 2) - proj.position.X + (proj.width / 2)) + Math.Abs(Main.player[i].position.Y + (Main.player[i].height / 2) - proj.position.Y + (proj.height / 2));
                        if (num5 < 1200f && (Main.player[i].statLifeMax2 - Main.player[i].statLife) > num3)
                        {
                            num3 = Main.player[i].statLifeMax2 - Main.player[i].statLife;
                            num4 = i;
                        }
                    }
                }
                Projectile.NewProjectile(proj.GetSource_FromThis(), proj.position.X, proj.position.Y, 0f, 0f, ProjectileID.SpiritHeal, 0, 0f, proj.owner, num4, num2);
            }
        }

        public void SpectreHurt(Projectile proj)
        {
            int num = proj.damage / 2;
            if (proj.damage / 2 <= 1)
            {
                return;
            }
            int num2 = 1000;
            if (Main.player[Main.myPlayer].ghostDmg > (float)num2)
            {
                return;
            }
            Main.player[Main.myPlayer].ghostDmg += (float)num;
            int[] array = new int[200];
            int num3 = 0;
            int num4 = 0;
            for (int i = 0; i < 200; i++)
            {
                if (Main.npc[i].CanBeChasedBy(this, false))
                {
                    float num5 = Math.Abs(Main.npc[i].position.X + (Main.npc[i].width / 2) - proj.position.X + (proj.width / 2)) + Math.Abs(Main.npc[i].position.Y + (Main.npc[i].height / 2) - proj.position.Y + (proj.height / 2));
                    if (num5 < 800f)
                    {
                        if (Collision.CanHit(proj.position, 1, 1, Main.npc[i].position, Main.npc[i].width, Main.npc[i].height) && num5 > 50f)
                        {
                            array[num4] = i;
                            num4++;
                        }
                        else if (num4 == 0)
                        {
                            array[num3] = i;
                            num3++;
                        }
                    }
                }
            }
            if (num3 == 0 && num4 == 0)
            {
                return;
            }
            int num6;
            if (num4 > 0)
            {
                num6 = array[Main.rand.Next(num4)];
            }
            else
            {
                num6 = array[Main.rand.Next(num3)];
            }
            float num7 = 4f;
            float num8 = Main.rand.Next(-100, 101);
            float num9 = Main.rand.Next(-100, 101);
            float num10 = (float)Math.Sqrt((double)(num8 * num8 + num9 * num9));
            num10 = num7 / num10;
            num8 *= num10;
            num9 *= num10;
            Projectile.NewProjectile(proj.GetSource_FromThis(), proj.position.X, proj.position.Y, num8, num9, ProjectileID.SpectreWrath, num, 0f, proj.owner, (float)num6, 0);
        }
        */
    }
    public class SpectreOnHitEffect : AccessoryEffect
    {

        public override Header ToggleHeader => Header.GetHeader<SpiritHeader>();
        public override int ToggleItemType => ModContent.ItemType<SpectreEnchant>();
        public static int BaseDamage(Player player) => (int)((player.ForceEffect<SpectreEffect>() ? 320 : 160) * player.ActualClassDamage(DamageClass.Magic));
        public int damageCopy;
        public override void OnHurt(Player player, Player.HurtInfo info)
        {
            if (player.FargoSouls().TerrariaSoul)
                return;
            damageCopy += info.Damage;
            for (int i = 0; i < 5; i++)
            {
                if (damageCopy < 40)
                    break;
                damageCopy -= 40 + (10 * i);

                float velX = Main.rand.Next(-5, 6) * 3f;
                float velY = Main.rand.Next(-5, 6) * 3f;
                Projectile.NewProjectile(GetSource_EffectItem(player), player.position.X + velX, player.position.Y + velY, velX, velY, ModContent.ProjectileType<SpectreSpirit>(), BaseDamage(player), 0f, player.whoAmI);
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
