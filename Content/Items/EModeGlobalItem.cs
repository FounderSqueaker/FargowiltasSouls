using Fargowiltas.Content.Items.Summons.Abom;
using Fargowiltas.Content.Items.Summons.Mutant;
using Fargowiltas.Content.Items.Summons.VanillaCopy;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Summons;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.Projectiles.Weapons;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace FargowiltasSouls.Content.Items
{
    public class EModeGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override void Load()
        {
            On_Player.GrantPrefixBenefits += EModePrefixChanges;
        }

        public override void Unload()
        {
            On_Player.GrantPrefixBenefits -= EModePrefixChanges;
        }
        const float newViolentBaseAttackSpeed = 0.005f;
        private static void EModePrefixChanges(On_Player.orig_GrantPrefixBenefits orig, Player self, Item item)
        {
            orig(self, item);
            if (!WorldSavingSystem.EternityMode)
                return;
            if (item.prefix >= PrefixID.Hard && item.prefix <= PrefixID.Warding)
            {
                if (!Main.hardMode)
                {
                    self.statDefense -= 1;
                }
                self.statLifeMax2 += 5;
            }
            if (item.prefix >= PrefixID.Wild && item.prefix <= PrefixID.Violent)
            {
                int prefixMultiplier = item.prefix - PrefixID.Wild + 1;
                self.GetAttackSpeed(DamageClass.Melee) -= 0.01f * prefixMultiplier;
                self.FargoSouls().AttackSpeed += newViolentBaseAttackSpeed * prefixMultiplier;
            }
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(item, tooltips);
            if (!WorldSavingSystem.EternityMode)
            {
                return;
            }
            if (item.type is ItemID.DivingHelmet or ItemID.DivingGear or ItemID.JellyfishDivingGear or ItemID.ArcticDivingGear)
            {
                string text = "\n" + Language.GetTextValue("Mods.FargowiltasSouls.Items.Extra.SpaceBreathImmunity");
                foreach (var tooltip in tooltips)
                {
                    if (item.type == ItemID.DivingHelmet && tooltip.Name == $"Tooltip0")
                    {
                        tooltip.Text += text;
                    }
                    if (item.type is ItemID.DivingGear or ItemID.JellyfishDivingGear && tooltip.Name == $"Tooltip1")
                    {
                        tooltip.Text += text;
                    }
                    if (item.type == ItemID.ArcticDivingGear && tooltip.Name == $"Tooltip2")
                    {
                        tooltip.Text += text;
                    }
                }
            }
            if (item.type is ItemID.MeteorHelmet or ItemID.MeteorSuit or ItemID.MeteorLeggings)
            {
                foreach (var tooltip in tooltips)
                {
                    if (tooltip.Name == "SetBonus")
                    {
                        tooltip.Text += "\n" + Language.GetTextValue("Mods.FargowiltasSouls.Items.Extra.SpaceBreathImmunity");
                    }
                }
            }
            EmodeItemBalance.BalanceTooltips(item, ref tooltips);
            if (item.prefix >= PrefixID.Hard && item.prefix <= PrefixID.Warding)
            {
                const int life = 5;
                foreach (TooltipLine tooltip in tooltips)
                {
                    if (tooltip.Name == "PrefixAccDefense")
                    {
                        if (!Main.hardMode)
                        {
                            List<char> text = tooltip.Text.ToList();
                            text[1] = (char)((int)text[1] - 1);
                            tooltip.Text = new string(text.ToArray());
                        }

                        tooltip.Text += "\n" + Language.GetTextValue("Mods.FargowiltasSouls.Items.Extra.DefensePrefixMaxLife", life);
                    }
                }
            }
            if (item.prefix >= PrefixID.Wild && item.prefix <= PrefixID.Violent)
            {
                foreach (TooltipLine tooltip in tooltips)
                {
                    if (tooltip.Name == "PrefixAccMeleeSpeed")
                    {
                        int prefixMultiplier = item.prefix - PrefixID.Wild + 1;
                        float attackSpeed = (float)System.Math.Round(newViolentBaseAttackSpeed * prefixMultiplier * 100, 1);
                        tooltip.Text = Language.GetTextValue("Mods.FargowiltasSouls.Items.Extra.ViolentPrefix", attackSpeed);
                    }
                }
            }
            if (item.type == ItemID.AvengerEmblem)
            {
                foreach (TooltipLine tooltip in tooltips)
                {
                    if (tooltip.Name == "Tooltip0" && tooltip.Text.Contains("12%"))
                        tooltip.Text = tooltip.Text.Replace("12%", "15%");
                }
            }
        }
        public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
        {
            if (!WorldSavingSystem.EternityMode)
                return;
            //ammo nerf
            //if (ammo.ammo == AmmoID.Arrow || ammo.ammo == AmmoID.Bullet || ammo.ammo == AmmoID.Dart)
            //{
            //    damage -= (int)Math.Round(ammo.damage * player.GetDamage(DamageClass.Ranged).Additive * 0.5, MidpointRounding.AwayFromZero); //always round up
            //}
        }
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            base.UpdateAccessory(item, player, hideVisual);
            if (!WorldSavingSystem.EternityMode)
            {
                return;
            }
            if (item.type == ItemID.JungleRose)
            {
                player.FargoSouls().HasJungleRose = true;
            }
            if (item.type == ItemID.AvengerEmblem)
            {
                player.GetDamage(DamageClass.Generic) += 0.03f;
            }
        }
        public override void HoldItem(Item item, Player player)
        {
            if (!WorldSavingSystem.EternityMode)
            {
                base.HoldItem(item, player);
                return;
            }
            EModePlayer ePlayer = player.Eternity();
            if (item.type == ItemID.MythrilHalberd || item.type == ItemID.MythrilSword)
            {
                if (!player.ItemAnimationActive)
                {
                    if (ePlayer.MythrilHalberdTimer < 121)
                        ePlayer.MythrilHalberdTimer++;
                }
                if (player.itemAnimation == 1) //reset on last frame
                {
                    ePlayer.MythrilHalberdTimer = 0;
                }

                if (ePlayer.MythrilHalberdTimer == 120 && player.whoAmI == Main.myPlayer)
                {
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Accessories/MythrilCharged"), player.Center);
                }
            }
            else
            {
                ePlayer.MythrilHalberdTimer = 0;
            }
            base.HoldItem(item, player);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (!WorldSavingSystem.EternityMode)
            {
                return base.CanUseItem(item, player);
            }

            EModePlayer ePlayer = player.Eternity();

            if (item.type == ItemID.CobaltSword)
            {
                ePlayer.CobaltHitCounter = 0;
            }

            /*
            if (item.type == ItemID.RodOfHarmony && LumUtils.AnyBosses())
            {
                player.hurtCooldowns[0] = 0;
                var defense = player.statDefense;
                float endurance = player.endurance;
                player.statDefense.FinalMultiplier *= 0;
                player.endurance = 0;
                player.Hurt(PlayerDeathReason.ByCustomReason(Language.GetTextValue("Mods.FargowiltasSouls.DeathMessage.RodOfHarmony", player.name)), player.statLifeMax2 / 7, 0, false, false, 0, false);
                player.statDefense = defense;
                player.endurance = endurance;
            }
            */
            //TODO: mana pot rework
            /*
            if (item.healMana > 0)
            {
                return !player.HasBuff(BuffID.ManaSickness);
            }
            */
            return base.CanUseItem(item, player);
        }
        //TODO: mana pot rework
        /*
        public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
        {
            if (WorldSavingSystem.EternityMode)
            {
                healValue = (int)(healValue * 1.5f);
            }
            base.GetHealMana(item, player, quickHeal, ref healValue);
        }
        */
        public override bool ConsumeItem(Item item, Player player)
        {
            if ((item.type == ModContent.ItemType<SquirrelCoatofArms>() && !WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TrojanSquirrel]) ||
                (item.type == ItemID.SuspiciousLookingEye && !NPC.downedBoss1) ||
                (item.type == ItemID.SlimeCrown && !NPC.downedSlimeKing) ||
                (item.type == ItemID.WormFood && !NPC.downedBoss2)||
                (item.type == ItemID.BloodySpine && !NPC.downedBoss2)||
                (item.type == ItemID.Abeemination && !NPC.downedQueenBee)||
                (item.type == ModContent.ItemType<CoffinSummon>() && !WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.CursedCoffin])||
                (item.type == ItemID.DeerThing && !NPC.downedDeerclops)||
                (item.type == ModContent.ItemType<SuspiciousSkull>() && !NPC.downedBoss3)||
                (item.type == ModContent.ItemType<DevisCurse>() && !WorldSavingSystem.DownedDevi)||
                (item.type == ModContent.ItemType<FleshyDoll>() && !Main.hardMode)||
                (item.type == ItemID.QueenSlimeCrystal && !NPC.downedQueenSlime)||
                (item.type == ModContent.ItemType<MechLure>() && !WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.BanishedBaron])||
                (item.type == ItemID.MechanicalWorm && !NPC.downedMechBoss1)||
                (item.type == ItemID.MechanicalEye && !NPC.downedMechBoss2)||
                (item.type == ItemID.MechanicalSkull && !NPC.downedMechBoss3)||
                (item.type == ModContent.ItemType<CrystallineEffigy>() && !WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.Lifelight])||
                (item.type == ModContent.ItemType<PlanterasFruit>() && !NPC.downedPlantBoss)||
                (item.type == ModContent.ItemType<LihzahrdPowerCell2>() && !NPC.downedGolemBoss)||
                (item.type == ModContent.ItemType<TruffleWorm2>() && !NPC.downedFishron)||
                (item.type == ModContent.ItemType<PrismaticPrimrose>() && !NPC.downedEmpressOfLight)||
                (item.type == ModContent.ItemType<CultistSummon>() && !NPC.downedAncientCultist)||
                (item.type == ItemID.CelestialSigil && !NPC.downedMoonlord)||
                (item.type == ModContent.ItemType<BetsyEgg>() && !WorldSavingSystem.DownedBetsy)||
                (item.type == ModContent.ItemType<AbomsCurse>() && !WorldSavingSystem.DownedAbom)||
                (item.type == ModContent.ItemType<MutantsCurse>() && !WorldSavingSystem.DownedMutant)||
                (item.type == ItemID.MechdusaSummon && !NPC.downedMechBoss1 && !NPC.downedMechBoss2 && !NPC.downedMechBoss3))
            {
                return false;
            }
            
            return base.ConsumeItem(item, player);
        }
        public override void ModifyItemScale(Item item, Player player, ref float scale)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            string extra = string.Empty;
            float balanceNumber = -1;
            string[] balanceTextKeys = null;
            EmodeItemBalance.EmodeBalance(ref item, ref balanceNumber, ref balanceTextKeys, ref extra);
            if (balanceTextKeys != null)
            {
                for (int i = 0; i < balanceTextKeys.Length; i++)
                {
                    if (balanceTextKeys[i] == "Scale" || balanceTextKeys[i] == "ScaleNoTooltip")
                        scale += float.Parse(extra, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture);
                }
            }
        }
        public override bool? UseItem(Item item, Player player)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.UseItem(item, player);
            EModePlayer ePlayer = player.Eternity();

            if (item.type == ItemID.MechdusaSummon && Main.zenithWorld)
            {
                Main.time = 18000;
            }
            return base.UseItem(item, player);
        }
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (player.Eternity().MythrilHalberdTimer >= 120 && (item.type == ItemID.MythrilSword))
            {
                float attackSpeedContrib = player.FargoSouls().AttackSpeed - 1;
                attackSpeedContrib /= 3;
                attackSpeedContrib += 1;
                damage *= 8 * attackSpeedContrib;
            }
        }
        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            switch (item.type)
            {
                case ItemID.TopazStaff:
                    if (EmodeItemBalance.HasEmodeChange(player, ItemID.TopazStaff).Contains("TopazStaff"))
                        crit += 6;
                    break;
                default:
                    break;
            }
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
            switch (item.type)
            {
                default:
                    break;
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
        public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!WorldSavingSystem.EternityMode)
                return;
            EModePlayer ePlayer = player.Eternity();
            switch (item.type)
            {
                case ItemID.OrichalcumSword:
                    modifiers.SourceDamage *= SpearRework.OrichalcumDoTDamageModifier(target.lifeRegen);
                    break;
                default:
                    break;
            }
        }
        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!WorldSavingSystem.EternityMode)
                return;
            EModePlayer ePlayer = player.Eternity();
            switch (item.type)
            {
                case ItemID.CobaltSword:
                    if (ePlayer.CobaltHitCounter < 2) //only twice per swing
                    {
                        Projectile p = FargoSoulsUtil.NewProjectileDirectSafe(player.GetSource_OnHit(target), target.position + Vector2.UnitX * Main.rand.Next(target.width) + Vector2.UnitY * Main.rand.Next(target.height), Vector2.Zero, ModContent.ProjectileType<CobaltExplosion>(), (int)(hit.SourceDamage * 0.4f), 0f, Main.myPlayer);
                        if (p != null)
                            p.FargoSouls().CanSplit = false;
                        ePlayer.CobaltHitCounter++;
                    }
                    break;
                case ItemID.PalladiumSword:
                    {
                        if (target.Hostile()) //may add more checks here idk
                        {
                            player.AddBuff(BuffID.RapidHealing, 60 * 5);
                            if (player.Eternity().PalladiumHealTimer <= 0)
                            {
                                player.FargoSouls().HealPlayer(1);
                                player.Eternity().PalladiumHealTimer = 30;
                            }
                        }
                        break;
                    }
            }
        }
        public override float UseSpeedMultiplier(Item item, Player player)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.UseSpeedMultiplier(item, player);
            if (item.type == ItemID.SapphireStaff && EmodeItemBalance.HasEmodeChange(player, item.type).Contains("SapphireStaff"))
            {
                return 1.5f;
            }
            if (item.type == ItemID.EmeraldStaff && EmodeItemBalance.HasEmodeChange(player, item.type).Contains("EmeraldStaff"))
            {
                return 0.75f;
            }
            return base.UseSpeedMultiplier(item, player);
        }
        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (!WorldSavingSystem.EternityMode)
                return;
            switch (item.type)
            {                    
                case ItemID.ChlorophyteSaber:
                    if (EmodeItemBalance.HasEmodeChange(player, item.type).Contains("ChlorophyteSaber"))
                    {
                        velocity *= 2f;
                    }
                    break;

                case ItemID.JackOLanternLauncher:
                    if (EmodeItemBalance.HasEmodeChange(player, item.type).Contains("JackOLanternLauncher"))
                    {
                        velocity *= 1.5f;
                    }
                    break;
                case ItemID.SapphireStaff:
                    if (EmodeItemBalance.HasEmodeChange(player, item.type).Contains("SapphireStaff"))
                    {
                        velocity = velocity.RotatedByRandom(MathHelper.PiOver2 * 0.2f);
                    }
                    break;
            }
        }
    }
}
