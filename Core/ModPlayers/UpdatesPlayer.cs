using System;
using System.Linq;
using Fargowiltas.Content.Items.Explosives;
using Fargowiltas.Content.NPCs;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Bosses.VanillaEternity;
using FargowiltasSouls.Content.Buffs;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Expert;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Items.Accessories.Souls;
using FargowiltasSouls.Content.Items.Armor.Styx;
using FargowiltasSouls.Content.Items.Consumables;
using FargowiltasSouls.Content.Items.Weapons.Challengers;
using FargowiltasSouls.Content.Items.Weapons.SwarmDrops;
using FargowiltasSouls.Content.Projectiles.Accessories.HeartOfTheMaster;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace FargowiltasSouls.Core.ModPlayers
{
    public partial class FargoSoulsPlayer
    {
        public override void PreUpdate()
        {
            Toggler.TryLoad();

            if (DevianttIntroduction)
            {
                if (Player.TalkNPC?.type == ModContent.NPCType<Deviantt>() && Player.TalkNPC.HasGivenName)
                {
                    Main.npcChatText = Language.GetTextValue("Mods.FargowiltasSouls.NPCs.UnconsciousDeviantt.Introduction", Player.TalkNPC.GivenName);
                    DevianttIntroduction = false;
                }
            }

            EaterofWorlds.CheckReset();

            if (Player.CCed)
            {
                Player.doubleTapCardinalTimer[2] = 2;
                Player.doubleTapCardinalTimer[3] = 2;
            }

            if (HurtTimer > 0)
                HurtTimer--;

            IsStandingStill = Math.Abs(Player.velocity.X) < 0.05 && Math.Abs(Player.velocity.Y) < 0.05;

            if (!Infested && !FirstInfection)
                FirstInfection = true;


            if (Unstable && Player.whoAmI == Main.myPlayer)
            {
                if (unstableCD == 0)
                {
                    Vector2 pos = Player.position;

                    int x = Main.rand.Next((int)pos.X - 500, (int)pos.X + 500);
                    int y = Main.rand.Next((int)pos.Y - 500, (int)pos.Y + 500);
                    Vector2 teleportPos = new(x, y);

                    while (Collision.SolidCollision(teleportPos, Player.width, Player.height) && teleportPos.X > 50 && teleportPos.X < (double)(Main.maxTilesX * 16 - 50) && teleportPos.Y > 50 && teleportPos.Y < (double)(Main.maxTilesY * 16 - 50))
                    {
                        x = Main.rand.Next((int)pos.X - 500, (int)pos.X + 500);
                        y = Main.rand.Next((int)pos.Y - 500, (int)pos.Y + 500);
                        teleportPos = new(x, y);
                    }

                    Player.Teleport(teleportPos, 1);
                    NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, Player.whoAmI, teleportPos.X, teleportPos.Y, 1);

                    unstableCD = 60;
                }
                unstableCD--;
            }

            if (OxygenTank)
            {
                RustedOxygenTank.PassiveEffect(Player);
            }

            if (GoldShell)
                GoldUpdate();

            //horizontal dash
            if (MonkDashing > 0)
            {
                MonkDashing--;

                //no loss of height
                //Player.maxFallSpeed = 0f;
                //Player.fallStart = (int)(Player.position.Y / 16f);
                //Player.gravity = 0f;
                //Player.position.Y = Player.oldPosition.Y;

                if (MonkDashing == 0 && Player.mount.Active)
                {
                    if (Player.velocity.Length() > Player.mount._data.dashSpeed)
                    {
                        float difference = Player.velocity.Length() / Player.mount._data.dashSpeed;

                        Player.velocity *= 1 / difference;
                    }
                }
            }
            //vertical dash
            else if (MonkDashing < 0)
            {
                MonkDashing++;

                if (Player.velocity.Y > 0)
                {
                    Player.maxFallSpeed *= 10;
                    Player.gravity = 8;
                    //deactivate hover or those mounts refuse to dash down
                    //Player.mount._data.usesHover = false;
                }

                if (MonkDashing == 0 && Player.mount.Active)
                {
                    Player.velocity *= 0.5f;

                    //add hover back
                    // Player.mount._data.usesHover = BaseSquireMountData.usesHover;
                }
            }
            ForbiddenTornados.Clear();
            ShadowOrbs.Clear();
            bool forbidden = Player.HasEffect<ForbiddenEffect>() || Player.HasEffect<SpiritTornadoEffect>();
            bool shadow = Player.HasEffect<ShadowBalls>();
            if (forbidden || shadow)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (forbidden && (p.TypeAlive<ForbiddenTornado>() || p.TypeAlive<SpiritTornado>()) && p.owner == Player.whoAmI)
                    {
                        ForbiddenTornados.Add(p.whoAmI);
                    }
                    if (shadow && p.TypeAlive<ShadowEnchantOrb>() && p.owner == Player.whoAmI)
                    {
                        ShadowOrbs.Add(p.whoAmI);
                    }
                }
            }
           
            
        }

        public override void PostUpdate()
        {
            if (!FreeEaterSummon && !Main.npc.Any(n => n.active && (n.type == NPCID.EaterofWorldsHead || n.type == NPCID.EaterofWorldsBody || n.type == NPCID.EaterofWorldsTail)))
            {
                FreeEaterSummon = true;
            }

            if (NymphsPerfumeRestoreLife > 0 && --NymphsPerfumeRestoreLife == 0)
            {
                if (Player.statLife < Player.statLifeMax2)
                    Player.statLife = Player.statLifeMax2;
                //doing it like this so it accounts for your lifeMax after respawn
                //regular OnRespawn() doesnt account for lifeforce, and is lowered by dying with oceanic maul
            }

            if (SquireEnchantItem != null && BaseMountType != -1)
            {
                SquireEnchant.ResetMountStats(this);
            }

            if (!Player.HasEffect<DCUEffect>() || !Player.mount.Active || Player.mount.Type != MountID.Drill)
            {
                Mount.amountOfBeamsAtOnce = 2;
            }

            ConcentratedRainbowMatterTryAutoHeal();

        }


        public override void PostUpdateBuffs()
        {
            if (Berserked && !Player.CCed)
            {
                if (Player.HeldItem != null && Player.HeldItem.IsWeapon() && Player.HeldItem.type != ModContent.ItemType<BoomShuriken>())
                {
                    Player.controlUseItem = true;
                    Player.releaseUseItem = true;
                }
                
            }
            if (LowGround)
            {
                Player.gravControl = false;
                Player.gravControl2 = false;
            }

            if (Quicksanding)
            {

            }
        }
        public override void PostUpdateEquips()
        {
            if (LumUtils.AnyBosses())
            {
                if (!BossAliveLastFrame)
                {
                    BossAliveLastFrame = true;
                    TinCrit = TinEffect.TinFloor(Player);
                    EbonwoodCharge = 0;
                    HuntressStage = 0;
                    NekomiMeter = 0;

                    Beetles = 0;
                    BeetleCharge = 0;
                    BeetleHitCD = 0;
                }
            }
            else
                BossAliveLastFrame = false;
            if (Graze && NekomiSet)
            {
                GrazeRadius *= DeviGraze || CirnoGraze ? 1.5f : 0.75f;
            }

            if (SpectreGhostTime > 0)
                SpectreEffect.GhostUpdate(Player);

            if (DeerSinew)
            {
                if (Player.AddEffect<DeerSinewEffect>(ModContent.GetInstance<DeerSinew>().Item))
                {
                    if (DeerSinewFreezeCD > 0)
                        DeerSinewFreezeCD--;
                }
            }

            if (OrdinaryCarrot)
            {
                if (Player.AddEffect<MasoCarrotEffect>(ModContent.GetInstance<OrdinaryCarrot>().Item))
                {
                    Player.scope = true;
                }
            }
                

            if (ConcentratedRainbowMatter)
            {
                Player.buffImmune[ModContent.BuffType<FlamesoftheUniverseBuff>()] = true;
                Player.AddEffect<RainbowHealEffect>(ModContent.GetInstance<ConcentratedRainbowMatter>().Item);
            }
                

            if (NoMomentum && !Player.mount.Active)
            {
                Player.runAcceleration *= 5f;
                Player.runSlowdown *= 5f;

                if (!IsStillHoldingInSameDirectionAsMovement)
                    Player.runSlowdown += 7f;
            }

            if (RottingHeartItem != null)
            {
                if (!IsStillHoldingInSameDirectionAsMovement)
                    Player.runSlowdown += 0.2f;
            }

            if (!Player.HasEffect<StardustEffect>())
                FreezeTime = false;

            UpdateShield();

            if (!Player.HasEffect<TimsInspectEffect>())
                Player.FargoSouls().TimsInspect = false;
            Player.IncrementCooldownTowards<TimsInspectEffect>(-1, 0);

            Player.wingTimeMax = (int)(Player.wingTimeMax * WingTimeModifier);

            if (MutantAntibodies && Player.wet)
            {
                Player.wingTime = Player.wingTimeMax;
                Player.AddBuff(ModContent.BuffType<RefreshedBuff>(), LumUtils.SecondsToFrames(30f));
            }

            if (StyxSet)
            {
                Player.accDreamCatcher = true; //dps counter is on

                //even if you attack weaker enemies or with less dps, you'll eventually get a charge
                if (StyxTimer > 0 && --StyxTimer == 1) //yes, 1, to avoid a possible edge case of frame perfect attacks blocking this
                {
                    int dps = Player.getDPS();
                    if (dps != 0)
                    {
                        int diff = StyxCrown.MINIMUM_DPS - dps;
                        if (diff > 0)
                            StyxMeter += (int)(diff * StyxCrown.StyxChargeMultiplier(Player, StyxCrown.ChargeContext.DPS) / 2f); //from testing: compared to 1.4.3, this was giving twice as much meter. thus we're halving it
                    }

                    //if (Player.getDPS() == 0) Main.NewText("bug! styx timer ran with 0 dps, show this to terry");
                }
            }
            else
            {
                StyxMeter = 0;
                StyxTimer = 0;
            }

            if (!GaiaSet)
                GaiaOffense = false;

            if (!EridanusSet)
                EridanusEmpower = false;

            if (RabiesVaccine)
                Player.buffImmune[BuffID.Rabies] = true;

            if (AbomWandItem != null)
                AbomWandUpdate();

            if (DevianttHeartItem != null)
            {
                if (DevianttHeartsCD > 0)
                    DevianttHeartsCD--;
            }

            if (Player.HasBuff<TwinsInstallBuff>() && !Player.HasEffect<FusedLensInstall>())
                Player.ClearBuff(ModContent.BuffType<TwinsInstallBuff>());

            if (Player.HasBuff<BerserkerInstallBuff>() && !Player.HasEffect<AgitatingLensInstall>() && !Player.HasEffect<SupremeDashEffect>())
            {
                Player.ClearBuff(ModContent.BuffType<BerserkerInstallBuff>());
                int stunDuration = 120; //2sec
                Player.AddBuff(ModContent.BuffType<BerserkerInstallCDBuff>(), 60 * 10);
                Player.AddBuff(ModContent.BuffType<StunnedBuff>(), stunDuration);
            }

            if (BetsysHeartItem != null || QueenStingerItem != null || Player.HasEffect<SupremeDashEffect>())
            {
                Player.IncrementCooldownTowards<SpecialDashEffect>(-1, 0);
                if (Player.GetCooldown<SpecialDashEffect>() == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item9, Player.Center);
                    for (int i = 0; i < 10; i++)
                    {
                        Color color = Color.Yellow;
                        if (Player.HasEffect<BetsyDashEffect>())
                            color = Color.OrangeRed;
                        else if (Player.HasEffect<SupremeDashEffect>())
                            color = Color.Lerp(Color.GhostWhite, Color.Transparent, 0.5f);
                        Vector2 vel = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 10).RotatedByRandom(MathHelper.Pi / 10) * Main.rand.NextFloat(3, 8);
                        Particle p = new SparkParticle(Player.Center, vel, color, 1, 30);
                        p.Spawn();
                    }
                    /*for (int i = 0; i < 30; i++)
                    {
                        int dust = DustID.GemTopaz;
                        if (Player.HasEffect<SupremeDashEffect>())
                            dust = DustID.Bone;
                        int d = Dust.NewDust(Player.position, Player.width, Player.height, dust, 0, 0, 0, default, 2.5f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity *= 4f;
                    }*/
                }
            }
            else
            {
                Player.IncrementCooldownTowards<SpecialDashEffect>(1, LumUtils.SecondsToFrames(7));
            }

            if (SlimyShieldItem != null || LihzahrdTreasureBoxItem != null)
                OnLandingEffects();

            if (noDodge)
            {
                Player.onHitDodge = false;
                Player.shadowDodge = false;
                Player.blackBelt = false;
                Player.brainOfConfusionItem = null;
            }

            #region dashes

            if (Player.dashType != 0)
                HasDash = true;

            if (PrecisionSealNoDashNoJump)
            {
                Player.dashType = 0;
                /*Player.GetJumpState(ExtraJump.CloudInABottle).Disable();
                Player.GetJumpState(ExtraJump.SandstormInABottle).Disable();
                Player.GetJumpState(ExtraJump.BlizzardInABottle).Disable();
                Player.GetJumpState(ExtraJump.FartInAJar).Disable();
                Player.GetJumpState(ExtraJump.TsunamiInABottle).Disable();
                Player.GetJumpState(ExtraJump.UnicornMount).Disable();*/
                Player.ConsumeAllExtraJumps();
                JungleJumping = false;
                DashCD = 2;
                IsDashingTimer = 0;
                HasDash = false;
                Player.dashDelay = 10;

                if (fastFallCD < 2)
                    fastFallCD = 2;
            }
            if (Player.dashDelay > 0 && DashCD > 0)
                Player.dashDelay = Math.Max(DashCD, Player.dashDelay);

            DashManager.AddDashes(Player);
            DashManager.ManageDashes(Player);

            if (LihzahrdTreasureBoxItem != null)
                TryFastfallUpdate();
            if (Player.HasEffect<DeerclawpsEffect>() && IsInADashState)
                DeerclawpsEffect.DeerclawpsAttack(Player, Player.Bottom);
            if (Player.HasEffect<JungleSporesEffect>() && IsInADashState)
                JungleSporesEffect.JungleAttack(Player, Player.Bottom);

            #endregion dashes

            if (NecromanticBrewItem != null && IsInADashState && Player.HasEffect<NecroBrewSpin>())
            {
                //if (NecromanticBrewRotation == 0)
                //{
                //    NecromanticBrewRotation = 0.001f;
                //    Player.velocity.X *= 1.1f;
                //}

                float dashSpeedBoost = 0.2f * Player.velocity.X;
                Player.position.X += dashSpeedBoost;
                if (Collision.SolidCollision(Player.position, Player.width, Player.height))
                    Player.position.X -= dashSpeedBoost;

                //Collision.StepUp(ref Player.position, ref Player.velocity, Player.width, Player.height, ref Player.stepSpeed, ref Player.gfxOffY, (int)Player.gravDir, Player.controlUp);

                Player.noKnockback = true;

                NecromanticBrewRotation += 0.6f * Math.Sign(Player.velocity.X == 0 ? Player.direction : Player.velocity.X);
                Player.fullRotation = NecromanticBrewRotation;
                Player.fullRotationOrigin = Player.Center - Player.position;
            }
            else if (NecromanticBrewRotation != 0)
            {
                Player.fullRotation = 0f;
                NecromanticBrewRotation = 0f;
            }
            if (Player.FargoSouls().Toggler_ExtraJumpsDisabled && Player.wingTimeMax > 0)
                Player.ConsumeAllExtraJumps();
        }
        public override void UpdateLifeRegen()
        {
            if (UsingAnkh)
                Player.lifeRegen += 3;
            if (Ambrosia)
                Player.lifeRegen += 4;
        }
        public override void UpdateBadLifeRegen()
        {
            if (Player.electrified && Player.wet)
                Player.lifeRegen -= 16;

            if (GrabDamage)
                Player.lifeRegen -= WorldSavingSystem.MasochistModeReal ? 30 : 20;

            void DamageOverTime(int badLifeRegen, bool affectLifeRegenCount = false)
            {
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;

                if (affectLifeRegenCount && Player.lifeRegenCount > 0)
                    Player.lifeRegen = 0;

                Player.lifeRegenTime = 0;
                Player.lifeRegen -= badLifeRegen;
            }

            if (Shadowflame)
                DamageOverTime(16);

            if (Daybroken)
                DamageOverTime(30);

            if (GodEater)
            {
                DamageOverTime(170, true);

                Player.lifeRegenCount -= 70;
            }

            if (Infested)
                DamageOverTime(InfestedExtraDot());

            if (Rotting)
                DamageOverTime(2);

            if (CurseoftheMoon)
                DamageOverTime(10);

            if (MutantPresence)
            {
                //if (Player.lifeRegen > 5)
                //    Player.lifeRegen = 5;
            }

            if (FlamesoftheUniverse)
                DamageOverTime(79, true);

            if (IvyVenom && !Player.venom)
                DamageOverTime(16, true);

            if (Anticoagulation)
                DamageOverTime(4, true);

            if (BleedingOut)
                DamageOverTime(Main.hardMode ? 80 : 20, true);

            if (Player.HasEffect<AshWoodEffect>() || Oiled) //i dont think theres a better way to do this
            {
                int regen = 0;
                if (Player.onFire) regen += 8;
                if (Player.onFire3) regen += 8;
                if (Player.onFrostBurn) regen += 16;
                if (Player.onFrostBurn2) regen += 16;
                if (Player.onFire2) regen += 24;
                if (Player.burned) regen += 60;

                if (Shadowflame) regen += 16;
                if (Daybroken) regen += 30;
                if (FlamesoftheUniverse) regen += 79;

                if (regen > 0)
                {
                    if (Player.HasEffect<AshWoodEffect>())
                    {
                        Player.lifeRegen += regen;
                        if (Player.lifeRegen > 0)
                            Player.lifeRegen = 0;
                    }
                    else if (Oiled)
                        Player.lifeRegen -= regen;
                }
            }

            if (Player.lifeRegen < 0)
            {
                LeadEffect.ProcessLeadEffectLifeRegen(Player);
            }

            //placed here so it runs after our modded dots
            if (WorldSavingSystem.EternityMode && !WorldSavingSystem.MasochistModeReal)
            {
                //silently make it much harder to die to DOT debuffs
                if (Player.lifeRegen < 0 && Player.statLife < 10)
                    Player.lifeRegen = 0;
            }
        }

        public override void PostUpdateMiscEffects()
        {
            TimeSinceHurt++;

            if (SandsOfTimeChannel > 0 && Player.ItemTimeIsZero && !Player.ItemAnimationActive)
                SandsOfTimeChannel = 0;
            else if (SandsOfTimeChannel < 0)
                SandsOfTimeChannel++;

            if (MinionSlotsNonstack > 0)
                Player.maxMinions += MinionSlotsNonstack;
            if (SentrySlotsNonstack > 0)
                Player.maxTurrets += SentrySlotsNonstack;

            if (MoonChalice || MasochistHeart) // remove regular minion toggle once galactic toggle is available
                Toggler_MinionsDisabled = false;

            if (GalacticMinionsDeactivated)
            {
                int accessoryminioncount = DeactivatedMinionEffectCount;
                int minioncount = Player.maxMinions - (int)Player.slotsMinions;
                if (DeactivatedMinionEffectCount > 0)
                    Player.GetDamage(DamageClass.Generic) += accessoryminioncount * 0.04f; // 4% each
                if (minioncount > 0)
                    Player.GetDamage(DamageClass.Generic) += minioncount * 0.02f; // 2% each
            }


            /*if (Fused && Math.Abs(Player.velocity.X) < 0.5f)
            {
                FusedStandStillTime++;
                if (FusedStandStillTime >= 60)
                {
                    Player.ClearBuff(ModContent.BuffType<FusedBuff>());
                }
            }
            else
                FusedStandStillTime = 0;*/

            if (ToggleRebuildCooldown > 0)
                ToggleRebuildCooldown--;

            if (EmodeToggleCooldown > 0)
                EmodeToggleCooldown--;

            if (!Player.ItemAnimationActive)
                swingDirection = -1;

            if (CosmosMoonTimer > 0) // naturally degrades
                CosmosMoonTimer--;
            else
                CosmosMoonTimer = 0;

            if (FallthroughCD > 0)
                FallthroughCD--;

            if (VortexCD > 0)
                VortexCD--;

            if (WretchedPouchCD > 0 && !Player.HasEffect<WretchedPouchEffect>())
                WretchedPouchCD--;

            if (LightslingerHitShots > 0 && Player.HeldItem.type != ModContent.ItemType<Lightslinger>())
                LightslingerHitShots--;

            if (MeteorCD < MeteorEffect.Cooldown && !Player.HasEffect<MeteorEffect>())
                MeteorCD++;

            if (MythrilTimer > 0 && !Player.HasEffect<MythrilEffect>())
                MythrilTimer--;
            if (MythrilSoundCooldown > 0)
                MythrilSoundCooldown--;

            if (TinCrit > 0 && !Player.HasEffect<TinEffect>())
                TinCrit = 0;

            if (NinjaCounter > 0 && !Player.HasEffect<NinjaEffect>())
                NinjaCounter = 0;

            if (NinjaDecrementCD > 0)
                NinjaDecrementCD--;

            if (ShadewoodCharge > 0 && !Player.HasEffect<ShadewoodEffect>())
                ShadewoodCharge = 0;

            if (!Player.HasEffectEnchant<BeetleEffect>())
            {
                Beetles = 0;
                BeetleCharge = 0;
                BeetleHitCD = 0;
            }
            if (!Player.ForceEffect<BeetleEffect>() && Beetles > 6)
            {
                Beetles = 6;
            }

            if (HuntressStage > 0 && !Player.HasEffect<HuntressEffect>())
                HuntressStage--;
            if (HuntressStage < 0)
                HuntressStage = 0;

            if (EbonwoodCharge > 0 && !Player.HasEffect<EbonwoodEffect>())
                EbonwoodCharge = 0;

            if (HallowRepelTime > 0 && !Player.HasEffect<HallowEffect>())
                HallowRepelTime = 0;

            //these are here so that emode minion nerf can properly detect the real set bonuses over in EModePlayer postupdateequips
            /*if (SquireEnchantActive)
                Player.setSquireT2 = true;
            if (ValhallaEnchantActive)
                Player.setSquireT3 = true;

            if (ApprenticeEnchantActive)
                Player.setApprenticeT2 = true;
            if (DarkArtistEnchantActive)
                Player.setApprenticeT3 = true;

            if (MonkEnchantActive)
                Player.setMonkT2 = true;

            if (ShinobiEnchantActive)
                Player.setMonkT3 = true;*/


            if (Player.channel && WeaponUseTimer < 2 && (Player.controlUseItem || Player.controlUseTile))
                WeaponUseTimer = 2;
            if (--WeaponUseTimer < 0)
                WeaponUseTimer = 0;

            if (IsDashingTimer > 0)
            {
                IsDashingTimer--;
                Player.dashDelay = -1;
            }

            if (CoyoteTime > 0)
                CoyoteTime--;

            if (CrystalDashFirstStrikeCD > 0)
                CrystalDashFirstStrikeCD--;

            if (GoldEnchMoveCoins)
            {
                ChestUI.MoveCoins(Player.inventory, Player.bank.item, ContainerTransferContext.FromUnknown(Player));
                GoldEnchMoveCoins = false;
            }

            if (SpectreCD > 0)
                SpectreCD--;
            if (SpectreGhostTime > 0)
                SpectreGhostTime--;

            if (LockedMana > 0)
            {
                float previousLockedMana = LockedMana;
                if (Player.manaRegenDelay <= 0)
                    LockedMana -= 1;
                Player.statManaMax2 -= (int)Player.FargoSouls().LockedMana;
                if ((int)LockedMana != (int)previousLockedMana)
                {
                    Player.statMana += 1;
                }

            }
            if (BorealCD > 0)
                BorealCD--;

            if (PalmWoodForceCD > 0)
                PalmWoodForceCD--;

            if (ChargeSoundDelay > 0)
                ChargeSoundDelay--;

            if (RustRifleReloading && Player.HeldItem.type == ModContent.ItemType<NavalRustrifle>())
            {
                RustRifleTimer++;
            }

            if (Player.HeldItem.type == ModContent.ItemType<EgyptianFlail>())
            {
                EgyptianFlailCD--;
                if (EgyptianFlailCD == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item103 with { Volume = 0.8f }, Player.Center);

                    for (int i = 0; i < 20; i++)
                    {
                        int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.GemAmethyst, 0, 0, 0, default, 2.5f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity *= 4f;
                    }
                }
            }
            if (SKSCancelTimer > 0)
                SKSCancelTimer--;

            if (ParryDebuffImmuneTime > 0)
            {
                ParryDebuffImmuneTime--;
                DreadShellVulnerabilityTimer = 0;
            }
            else if (DreadShellVulnerabilityTimer > 0)
            {
                DreadShellVulnerabilityTimer--;

                Player.statDefense -= 15;
                Player.endurance -= 0.15f;
            }

            if (HallowHealTime > 0)
            {
                const int healDelay = 60;
                if (Player.HasEffect<HallowEffect>() && HallowHealTime % healDelay == 0)
                {
                    int heal = (int)Math.Round(HallowHealTotal / 10);
                    Player.Heal(heal);
                }
                HallowHealTime--;
            }
            if (++frameCounter >= 60)
                frameCounter = 0;

            if (HealTimer > 0)
                HealTimer--;


            if (Player.grapCount > 0)
                Grappled = true;

            if (FallthroughTimer > 0)
                FallthroughTimer--;

            if (LowGround)
            {
                Player.waterWalk = false;
                Player.waterWalk2 = false;
                if (FallthroughTimer < 2)
                    FallthroughTimer = 2;
            }

            if (DashCD > 0)
                DashCD--;

            if (ReallyAwfulDebuffCooldown > 0)
                ReallyAwfulDebuffCooldown--;

            if (OceanicMaul && FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.fishBossEX, NPCID.DukeFishron))
            {
                Player.statLifeMax2 /= 5;
                if (Player.statLifeMax2 < 100)
                    Player.statLifeMax2 = 100;
            }

            if (StealingCooldown > 0 && !Player.dead)
                StealingCooldown--;

            if (Graze) //decrease graze bonus over time
            {
                if (++DeviGrazeCounter > 60)
                {
                    DeviGrazeCounter = 0;
                    if (DeviGrazeBonus > 0f)
                        DeviGrazeBonus -= 0.01;
                }

                if (CirnoGrazeCounter > 0)
                    CirnoGrazeCounter--;
            }

            if (Slimed)
            {
                //slowed effect
                Player.moveSpeed *= .75f;
                Player.jump = (int)Math.Round(Player.jump * 0.75);
            }

            if (GodEater)
            {
                Player.statDefense *= 0;
                Player.endurance = 0;
            }

            if (Defenseless)
            {
                Player.endurance /= 2;
            }

            if (Asocial)
            {
                KillPets();
                Player.maxMinions = (int)(Player.maxMinions / 1.5);
                Player.maxTurrets = (int)(Player.maxTurrets / 1.5);
            }
            else if (WasAsocial) //should only occur when above debuffs end
            {
                Player.hideMisc[0] = HidePetToggle0;
                Player.hideMisc[1] = HidePetToggle1;

                WasAsocial = false;
            }

            if (UsingAnkh)
            {
                Player.statDefense -= 10;
            }

            if (Rotting)
            {
                Player.moveSpeed *= 0.9f;
                //Player.statLifeMax2 -= Player.statLifeMax / 5;
                Player.statDefense -= 10;
                Player.endurance -= 0.1f;
                AttackSpeed -= 0.1f;
                Player.GetDamage(DamageClass.Generic) -= 0.1f;
            }

            ManageLifeReduction();

            if (Eternity)
                Player.statManaMax2 = 999;
            else if (UniverseSoul)
                Player.statManaMax2 += 200;

            if (Player.HasEffect<CelestialRuneAttacks>() && AdditionalAttacksTimer > 0)
                AdditionalAttacksTimer--;

            StatLifePrevious = Player.statLife;
        }
    }
}
