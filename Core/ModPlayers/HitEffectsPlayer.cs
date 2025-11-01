using System;
using FargowiltasSouls.Content.Bosses.AbomBoss;
using FargowiltasSouls.Content.Bosses.DeviBoss;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Content.Items.Accessories;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Items.Armor.Nekomi;
using FargowiltasSouls.Content.Items.Armor.Styx;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.Projectiles.Armor;
using FargowiltasSouls.Content.Projectiles.Eternity.Buffs;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Cavern;
using FargowiltasSouls.Content.Projectiles.Weapons.Minions;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;
using static FargowiltasSouls.FargowiltasSouls;

namespace FargowiltasSouls.Core.ModPlayers
{
    public partial class FargoSoulsPlayer
    {
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.hostile)
                return;

            if (MinionCrits && proj.DamageType.CountsAsClass(DamageClass.Summon))
            {
                if (Main.rand.Next(100) < Player.ActualClassCrit(DamageClass.Summon))
                    modifiers.SetCrit();
            }

            if (SqueakyToy)
            {
                modifiers.SetMaxDamage(1);
                Squeak(target.Center, 0.4f);
                return;
            }

            ModifyHitNPCBoth(target, ref modifiers, proj.DamageType);
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {


            if (SqueakyToy)
            {
                modifiers.SetMaxDamage(1);
                Squeak(target.Center, 0.4f);
                return;
            }

            ModifyHitNPCBoth(target, ref modifiers, item.DamageType);
        }

        public void ModifyHitNPCBoth(NPC target, ref NPC.HitModifiers modifiers, DamageClass damageClass)
        {
            var critDisabled = typeof(NPC.HitModifiers).GetField("_critOverride", LumUtils.UniversalBindingFlags)?.GetValue(modifiers) as bool?;
            if (MinionCrits && damageClass.CountsAsClass(DamageClass.Summon) && critDisabled != false)
            {
                float crit = 0f; // spider enchant crits don't deal extra damage, but summon spiderlings
                /*if (EridanusSet || Player.HasEffect<LifeForceEffect>())
                {
                    crit = 0.25f; // crits deal 1.25x damage
                    if (!Player.ProcessDamageTypeFromHeldItem().CountsAsClass(DamageClass.Summon))
                        crit = 0.15f; // crits reduced to 1.15x
                }*/

                modifiers.CritDamage -= (modifiers.CritDamage.Additive - 1) * (1 - crit);
            }

            modifiers.ModifyHitInfo += (ref NPC.HitInfo hitInfo) =>
            {

                if (hitInfo.Crit)
                {
                    if (MinionCrits && Player.HasEffectEnchant<SpiderEffect>() && damageClass.CountsAsClass(DamageClass.Summon))
                    {
                        // summon spiderlings
                        if (Player.whoAmI == Main.myPlayer && SpiderCD <= 0)
                        {
                            SpiderCD = 30;
                            Vector2 spawnPos = Main.rand.NextVector2FromRectangle(Player.Hitbox);
                            Vector2 vel = spawnPos.DirectionTo(target.Center) * 5f;
                            Projectile.NewProjectile(Player.GetSource_EffectItem<SpiderEffect>(), spawnPos, vel, ModContent.ProjectileType<SpiderEnchantSpiderling>(), SpiderEnchantSpiderling.SpiderDamage(Player), 0.5f, Main.myPlayer, ai2: target.whoAmI);
                        }
                    }
                    if (UniverseCore && !damageClass.CountsAsClass(DamageClass.Summon)) // cosmic core
                    {
                        float crit = Player.ActualClassCrit(damageClass) / 2;
                        if (Main.rand.NextFloat(100) < crit) //supercrit
                        {
                            hitInfo.Damage *= 2;
                            target.AddBuff(ModContent.BuffType<FlamesoftheUniverseBuff>(), 240);
                            SoundEngine.PlaySound(SoundID.Item147 with { Pitch = 1, Volume = 0.7f }, target.Center);
                        }
                    }


                }

                if (UniverseCore && damageClass.CountsAsClass(DamageClass.Summon) && critDisabled != false)
                {
                    float crit = Player.GetTotalCritChance(damageClass) / 2;
                    if (Main.rand.NextFloat(100) < crit)
                    {
                        hitInfo.Damage *= 2;
                        target.AddBuff(ModContent.BuffType<FlamesoftheUniverseBuff>(), 240);
                        SoundEngine.PlaySound(SoundID.Item147 with { Pitch = 1, Volume = 0.7f }, target.Center);
                    }
                }

                if (Hexed && HexedInflictor == target.whoAmI)
                {
                    Vector2 speed = Main.rand.NextFloat(1, 2) * Vector2.UnitX.RotatedByRandom(Math.PI * 2);
                    float ai1 = 30 + Main.rand.Next(30);
                    Projectile.NewProjectile(target.GetSource_FromThis(), Player.Center, speed, ModContent.ProjectileType<HostileHealingHeart>(), hitInfo.Damage / 5, 0f, Main.myPlayer, target.whoAmI, ai1);
                    hitInfo.Null();
                    return;
                }
            };

            if (DeerSinewNerf)
            {
                float ratio = Math.Min(Player.velocity.Length() / 20f, 1f);
                modifiers.FinalDamage *= MathHelper.Lerp(1f, 0.85f, ratio);
            }

            if (FirstStrike)
            {
                modifiers.SetCrit();
                //modifiers.FinalDamage *= 1.25f;
                Player.ClearBuff(ModContent.BuffType<FirstStrikeBuff>());
                //target.defense -= 5;
                //target.AddBuff(BuffID.BrokenArmor, 600);
            }

            if (Illuminated)
            {
                float maxBonus = 0.15f;

                Color light = Lighting.GetColor(Player.Center.ToTileCoordinates());
                float modifier = (light.R + light.G + light.B) / 700f;
                modifier = MathHelper.Clamp(modifier, 0, 1);

                modifier = 1 + maxBonus * modifier;
                modifiers.FinalDamage *= modifier;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)/* tModPorter If you don't need the Projectile, consider using OnHitNPC instead */
        {
            if (target.type == NPCID.TargetDummy || target.friendly)
                return;

            //if (proj.minion)// && proj.type != ModContent.ProjectileType<CelestialRuneAncientVision>() && proj.type != ModContent.ProjectileType<SpookyScythe>())
            //    TryAdditionalAttacks(proj.damage, proj.DamageType);

            if (proj.FargoSouls().TagStackMultiplier != 1)
            {
                // Reset the whip tag damage nerf
                // Nerf itself is done in FargowiltasSoulsDetours detour for CombinedHooks.ModifyHitNPCWithProj
                ProjectileID.Sets.SummonTagDamageMultiplier[proj.type] /= proj.FargoSouls().TagStackMultiplier;
                proj.FargoSouls().TagStackMultiplier = 1;
            }

            OnHitNPCEither(target, hit, proj.DamageType, projectile: proj);
        }

        private void OnHitNPCEither(NPC target, NPC.HitInfo hitInfo, DamageClass damageClass, Projectile projectile = null, Item item = null)
        {

            //doing this so that damage-inheriting effects dont double dip or explode due to taking on crit boost
            int GetBaseDamage()
            {
                // TODO: I guess? test this
                int baseDamage = hitInfo.SourceDamage;
                if (projectile != null)
                    baseDamage = projectile.damage;
                else if (item != null)
                    baseDamage = Player.GetWeaponDamage(item);
                return baseDamage;
            }

            if (StyxSet)
            {
                StyxMeter += (int)(hitInfo.Damage * StyxCrown.StyxChargeMultiplier(Player, StyxCrown.ChargeContext.DealDamage));
                if (StyxTimer <= 0 && !target.friendly && target.lifeMax > 5 && target.type != NPCID.TargetDummy)
                    StyxTimer = 60;
            }


            if (Player.HasEffect<TitaniumEffect>() && (projectile == null || projectile.type != ProjectileID.TitaniumStormShard))
            {
                TitaniumEffect.TitaniumShards(this, Player);
            }

            if (DevianttHeartItem != null && DevianttHeartsCD <= 0 && Player.HasEffect<DevianttHearts>()
                && (projectile == null || (projectile.type != ModContent.ProjectileType<FriendRay>() && projectile.type != ModContent.ProjectileType<FriendHeart>())))
            {
                DevianttHeartsCD = 600;

                if (Main.myPlayer == Player.whoAmI)
                {
                    Vector2 offset = 300 * Player.DirectionFrom(Main.MouseWorld);
                    for (int i = -3; i <= 3; i++)
                    {
                        Vector2 spawnPos = Player.Center + offset.RotatedBy(Math.PI / 7 * i);
                        Vector2 speed = Vector2.Normalize(Main.MouseWorld - spawnPos);

                        int baseHeartDamage = AbomWandItem == null ? 15 : 170;
                        //heartDamage = (int)(heartDamage * Player.ActualClassDamage(DamageClass.Summon));

                        float ai1 = (Main.MouseWorld - spawnPos).Length() / 17;

                        if (MutantEyeItem == null)
                            FargoSoulsUtil.NewSummonProjectile(Player.GetSource_Accessory(DevianttHeartItem), spawnPos, 17f * speed, ModContent.ProjectileType<FriendHeart>(), baseHeartDamage, 3f, Player.whoAmI, -1, ai1);
                        else
                            FargoSoulsUtil.NewSummonProjectile(Player.GetSource_Accessory(DevianttHeartItem), spawnPos, speed, ModContent.ProjectileType<FriendRay>(), baseHeartDamage, 3f, Player.whoAmI, (float)Math.PI / 7 * i);

                        FargoSoulsUtil.HeartDust(spawnPos, speed.ToRotation());
                    }
                }
            }

            if (GodEaterImbue)
            {
                /*if (target.FindBuffIndex(ModContent.BuffType<GodEater>()) < 0 && target.aiStyle != 37)
                {
                    if (target.type != ModContent.NPCType<MutantBoss>())
                    {
                        target.DelBuff(4);
                        target.buffImmune[ModContent.BuffType<GodEater>()] = false;
                    }
                }*/
                target.AddBuff(ModContent.BuffType<GodEaterBuff>(), 420);
            }


            //            /*if (PalladEnchant && !TerrariaSoul && palladiumCD == 0 && !target.immortal && !Player.moonLeech)
            //            {
            //                int heal = damage / 10;

            //                if ((EarthForce) && heal > 16)
            //                    heal = 16;
            //                else if (!EarthForce && !WizardEnchant && heal > 8)
            //                    heal = 8;
            //                else if (heal < 1)
            //                    heal = 1;
            //                Player.statLife += heal;
            //                Player.HealEffect(heal);
            //                palladiumCD = 240;
            //            }*/

            if (FusedLens)
            {
                if (FusedLensCursed)
                    target.AddBuff(BuffID.CursedInferno, 120);
                if (FusedLensIchor)
                    target.AddBuff(BuffID.Ichor, 120);
            }

            if (Supercharged)
            {
                target.AddBuff(BuffID.Electrified, 240);
                //target.AddBuff(ModContent.BuffType<LightningRodBuff>(), 60);
            }

            if (Player.HasEffect<IvyVenomEffect>())
            {
                target.AddBuff(ModContent.BuffType<IvyVenomBuff>(), 60);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.type == NPCID.TargetDummy || target.friendly)
                return;

            OnHitNPCEither(target, hit, item.DamageType, item: item);
        }
        private void ApplyDR(Player player, float dr, ref Player.HurtModifiers modifiers)
        {
            player.endurance += dr;
            if (WorldSavingSystem.EternityMode && FargowiltasSouls.CalamityMod == null)
            {
                //Formula that emulates multiplicative DR scaling
                //This formula essentially assumes each DR source is 15 %, and scales your DR so each additional 15 % reduces your damage taken by 15 % compared to the previous value
                //The value of 15 % was chosen to make the scaling more lenient than a lower value would
                // Only takes effect if your dr is larger than 0.15% (below this value the formula would scale UP your DR, actually)
                float r = 0.15f;
                if (player.endurance >= r)
                    player.endurance = 1 - MathF.Pow(1 - r, player.endurance / r);
            }
        }
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            float dr = 0;

            //if (npc.FargoSouls().Corrupted || npc.FargoSouls().CorruptedForce)
            //    dr += 0.2f;

            if (npc.FargoSouls().BloodDrinker)
                dr -= 0.3f;

            if (Smite)
                dr -= 0.2f;

            if (CurseoftheMoon)
                dr -= 0.2f;

            if (Fused)
                dr += 0.5f;

            if (Illuminated && GelicWingsItem == null)
            {
                float maxDRReduction = 0.25f;

                Color light = Lighting.GetColor(Player.Center.ToTileCoordinates());
                float modifier = (light.R + light.G + light.B) / 700f;
                modifier = MathHelper.Clamp(modifier, 0, 1);

                modifier = maxDRReduction * modifier;
                dr -= modifier;
            }

            dr += Player.AccessoryEffects().ContactDamageDR(npc, ref modifiers);

            ApplyDR(Player, dr, ref modifiers);
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            float dr = 0;

            if (Smite)
                dr -= 0.2f;

            if (CurseoftheMoon)
                dr -= 0.2f;

            if (Illuminated)
            {
                float maxDRReduction = 0.25f;

                Color light = Lighting.GetColor(Player.Center.ToTileCoordinates());
                float modifier = (light.R + light.G + light.B) / 700f;
                modifier = MathHelper.Clamp(modifier, 0, 1);

                modifier = maxDRReduction * modifier;
                dr -= modifier;
            }

            dr += Player.AccessoryEffects().ProjectileDamageDR(proj, ref modifiers);

            ApplyDR(Player, dr, ref modifiers);
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            OnHitByEither(npc, null);

            if (Fused)
            {
                int fusedType = ModContent.BuffType<FusedBuff>();
                npc.AddBuff(fusedType, Player.buffTime[Player.FindBuffIndex(fusedType)]);
                Player.ClearBuff(fusedType);
                Player.buffImmune[fusedType] = true; //avoid being debuffed by it again if you run into a fused inflicting enemy
            }
            else if (npc.FargoSouls().Fused)
            {
                int fusedType = ModContent.BuffType<FusedBuff>();
                int fusedIndex = npc.FindBuffIndex(fusedType);
                Player.AddBuff(fusedType, npc.buffTime[fusedIndex]);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    var netMessage = Instance.GetPacket(); // Broadcast item request to server
                    netMessage.Write((byte)PacketID.ClearNPCBuffFromClient);
                    netMessage.Write((byte)npc.whoAmI);
                    netMessage.Write((byte)fusedType);
                    netMessage.Send();
                }
                else
                {
                    npc.DelBuff(fusedIndex);
                }
            }
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            OnHitByEither(null, proj);
        }

        public void OnHitByEither(NPC npc, Projectile proj)
        {
            if (Anticoagulation && Main.myPlayer == Player.whoAmI)
            {
                Entity source = null;
                if (npc != null)
                    source = npc;
                else if (proj != null)
                    source = proj;

                int type = ModContent.ProjectileType<Bloodshed>();
                for (int i = 0; i < 6; i++)
                {
                    if (Main.rand.NextBool(Player.ownedProjectileCounts[type] + 2))
                    {
                        const float speed = 12f;
                        Projectile.NewProjectile(Player.GetSource_OnHurt(source), Player.Center, Main.rand.NextVector2Circular(speed, speed), type, 0, 0f, Main.myPlayer, 0f);
                    }
                }
            }
            /*
            if (ModContent.GetInstance<SoulConfig>().BigTossMode)
            {
                AddBuffNoStack(ModContent.BuffType<StunnedBuff>(), 60);

                Vector2 attacker = default;
                if (npc != null)
                    attacker = npc.Center;
                else if (proj != null)
                    attacker = proj.Center;
                if (attacker != default)
                    Player.velocity = Vector2.Normalize(Player.Center - attacker) * 30 * 2;
            }
            */
        }

        public override bool CanBeHitByNPC(NPC npc, ref int CooldownSlot)
        {
            if (ImmuneToDamage)
                return false;
            return true;
        }

        public override bool CanBeHitByProjectile(Projectile proj)
        {
            FargoSoulsPlayer modPlayer = Player.FargoSouls();
            if (ImmuneToDamage)
                return false;
            if (!modPlayer.ShellHide && Player.HasEffect<PrecisionSealHurtbox>() && !proj.Colliding(proj.Hitbox, GetPrecisionHurtbox()))
                return false;
            return true;
        }
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)/* tModPorter Override ImmuneTo, FreeDodge or ConsumableDodge instead to prevent taking damage */
        {
            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.deviBoss, ModContent.NPCType<DeviBoss>()) && EModeGlobalNPC.deviBoss.IsWithinBounds(Main.maxNPCs))
                ((DeviBoss)Main.npc[EModeGlobalNPC.deviBoss].ModNPC).playerInvulTriggered = true;

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.abomBoss, ModContent.NPCType<AbomBoss>()) && EModeGlobalNPC.abomBoss.IsWithinBounds(Main.maxNPCs))
                ((AbomBoss)Main.npc[EModeGlobalNPC.abomBoss].ModNPC).playerInvulTriggered = true;

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>()) && EModeGlobalNPC.mutantBoss.IsWithinBounds(Main.maxNPCs))
                ((MutantBoss)Main.npc[EModeGlobalNPC.mutantBoss].ModNPC).playerInvulTriggered = true;

            if (DeathMarked)
                modifiers.SourceDamage *= 1.5f;

            if (MutantDesperation)
                modifiers.SourceDamage *= 2f;

            if (Player.whoAmI == Main.myPlayer && !noDodge && Player.HasEffect<SqueakEffect>())
            {
                int chanceDenominator = 10;

                if (Main.rand.NextBool(chanceDenominator))
                {
                    Squeak(Player.Center, 0.4f);
                    modifiers.SetMaxDamage(1);
                }
            }

            modifiers.ModifyHurtInfo += TryParryAttack;

            if (Player.FargoSouls().ShellHide && !Player.HasBuff(ModContent.BuffType<ShellSmashBuff>()))
            {
                modifiers.DisableSound();
                SoundEngine.PlaySound(SoundID.Item148 with { Volume = 2f }, Player.Center);
            }

            if (StyxSet && !ImmuneToDamage && Player.ownedProjectileCounts[ModContent.ProjectileType<StyxArmorScythe>()] > 0)
            {
                modifiers.ModifyHurtInfo += (ref Player.HurtInfo hurtInfo) =>
                {
                    if (hurtInfo.Damage <= 1) return;

                    int scythesSacrificed = 0;
                    const int maxSacrifice = 4;
                    const double maxDR = 0.20;
                    int scytheType = ModContent.ProjectileType<StyxArmorScythe>();
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].type == scytheType && Main.projectile[i].owner == Player.whoAmI)
                        {
                            if (Player.whoAmI == Main.myPlayer)
                                Main.projectile[i].Kill();
                            if (++scythesSacrificed >= maxSacrifice)
                                break;
                        }
                    }

                    // should not go below 1 due to math so no hacking here
                    hurtInfo.Damage = (int)(hurtInfo.Damage * (1.0f - (float)maxDR / maxSacrifice * scythesSacrificed));
                };
            }
        }
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (SupersonicDodge && !noDodge)
            {
                if (Player.brainOfConfusionItem != null)
                {
                    Player.brainOfConfusionItem = null;
                }
                if (Player.blackBelt) // no stack, instead increase chance
                {
                    Player.blackBelt = false;
                }
                int denom = 6;
                if (Main.rand.NextBool(denom))
                {
                    Player.SetImmuneTimeForAllTypes(Player.longInvince ? 120 : 80);
                    if (Player.whoAmI == Main.myPlayer)
                    {
                        NetMessage.SendData(MessageID.Dodge, -1, -1, null, Player.whoAmI, 1f);
                    }
                    if (Player.HasEffect<HallowedPendantEffect>())
                        HallowedPendantEffect.PendantRays(Player, FargoSoulsUtil.HighestDamageTypeScaling(Player, 800), 1200);
                    return true;
                }
            }
            return base.FreeDodge(info);
        }
        public override void OnHurt(Player.HurtInfo info)
        {
            Player player = Main.LocalPlayer;
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            WasHurtBySomething = true;

            MahoganyCanUseDR = false;

            TimeSinceHurt = 0;

            if (Player.HasBuff(ModContent.BuffType<TitaniumDRBuff>())
                && !Player.HasBuff(ModContent.BuffType<TitaniumCDBuff>()))
            {
                Player.AddBuff(ModContent.BuffType<TitaniumCDBuff>(), LumUtils.SecondsToFrames(15));
            }

            if (DeerSinewNerf && DeerSinewFreezeCD <= 0 && info.Damage > 10 && (info.DamageSource.SourceNPCIndex.IsWithinBounds(Main.maxNPCs) || (info.DamageSource.SourceProjectileType.IsWithinBounds(Main.maxProjectiles) && Main.projectile[info.DamageSource.SourceProjectileType].aiStyle != ProjAIStyleID.FallingTile)))
            {
                DeerSinewFreezeCD = 240;
                FargoSoulsUtil.AddDebuffFixedDuration(Player, BuffID.Frozen, 10);
            }

            if (NekomiSet && NekomiHitCD <= 0 && info.Damage > 10)
            {
                NekomiHitCD = 60;

                const int heartsLost = 1;
                int meterPerHeart = NekomiHood.MAX_METER / NekomiHood.MAX_HEARTS;
                int meterLost = meterPerHeart * heartsLost;

                int heartsToConsume = NekomiMeter / meterPerHeart;
                if (heartsToConsume > heartsLost)
                    heartsToConsume = heartsLost;
                Player.AddBuff(BuffID.RapidHealing, LumUtils.SecondsToFrames(heartsToConsume) * 4 / heartsLost);

                NekomiMeter -= meterLost;
                if (NekomiMeter < 0)
                    NekomiMeter = 0;
            }

            /*if ((player.HasEffectEnchant<TurtleSmashEffect>() || player.FargoSouls().ShellHide) && !player.HasBuff(ModContent.BuffType<ShellSmashBuff>()))
            {
                int shelldmg = info.SourceDamage;
                if (player.HasEffectEnchant<TurtleEffect>())
                    TurtleShellHP -= shelldmg;
                if (!Main.dedServ && player.FargoSouls().ShellHide)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (shelldmg < 60)
                            break;
                        shelldmg -= 60;
                        int i = j % 9;
                        Vector2 pos = Main.rand.NextVector2FromRectangle(Player.Hitbox);
                        Vector2 vel = Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(4, 8);
                        int type = i + 1;
                        Gore.NewGore(player.GetSource_Accessory(player.EffectItem<TurtleEffect>()), pos, vel, ModContent.Find<ModGore>(Mod.Name, $"TurtleFragment{type}").Type, Main.rand.NextFloat(0.7f, 1.3f));
                    }
                }
            }*/

            if (Defenseless)
            {
                SoundEngine.PlaySound(SoundID.Item27, Player.Center);
                for (int i = 0; i < 30; i++)
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.t_SteampunkMetal, 0, 0, 0, default, 2f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 5f;
                }
            }

            if (Midas && Main.myPlayer == Player.whoAmI)
                Player.DropCoins();

            if (info.Damage > 1)
            {
                DeviGrazeBonus = 0;
                DeviGrazeCounter = 0;
            }

            if (Main.myPlayer == Player.whoAmI)
            {
                if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld && FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>()) && EModeGlobalNPC.mutantBoss.IsWithinBounds(Main.maxNPCs))
                {
                    if (!Player.HasBuff(ModContent.BuffType<TimeFrozenBuff>()))
                    {
                        The22Incident += 1;
                        Rectangle rect = new Rectangle((int)Player.Center.X - 111, (int)Player.Center.Y, 222, 222);
                        for (int i = 0; i < The22Incident; i++)
                            CombatText.NewText(rect, Color.DarkOrange, The22Incident, true);
                        //doing it this way to ensure we dont accidentally skip over the checks
                        //but also so that it doesnt harass godmode testing forever
                        LocalizedText DeathText = Language.GetText("Mods.FargowiltasSouls.DeathMessage.TwentyTwo");
                        if (The22Incident == 22 || The22Incident == 23 || The22Incident == 24)
                        {
                            Player.KillMe(PlayerDeathReason.ByCustomReason(DeathText.ToNetworkText(Player.name)), 22222222, 0);
                            Projectile.NewProjectile(Player.GetSource_Death(), Player.Center, Vector2.Zero, ModContent.ProjectileType<TwentyTwo>(), 0, 0f, Main.myPlayer);
                            ScreenShakeSystem.StartShake(10, shakeStrengthDissipationIncrement: 10f / 30);
                        }
                    }
                }
                else
                {
                    The22Incident = 0;
                }
            }
        }
    }
}