using Fargowiltas.Content.UI;
using FargowiltasSouls.Assets.Particles;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Achievements;
using FargowiltasSouls.Content.Bosses.CursedCoffin;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Content.Buffs;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Expert;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Items.Accessories.Souls;
using FargowiltasSouls.Content.Items.Armor.Eridanus;
using FargowiltasSouls.Content.Items.Armor.Styx;
using FargowiltasSouls.Content.Items.Consumables;
using FargowiltasSouls.Content.Items.Dyes;
using FargowiltasSouls.Content.Items.Weapons.SwarmDrops;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Accessories.PureHeart;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.Projectiles.Armor;
using FargowiltasSouls.Content.Projectiles.Weapons.SwarmDrops;
using FargowiltasSouls.Content.UI;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core.Toggler;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;
using static FargowiltasSouls.Core.Systems.DashManager;

namespace FargowiltasSouls.Core.ModPlayers
{
    public partial class FargoSoulsPlayer : ModPlayer
    {
        public SoulToggleBackend Toggler = new();

        public Dictionary<AccessoryEffect, bool> TogglesToSync = [];

        public Dictionary<int, AccessoryEffect> SkillsToSync = [];

        public List<AccessoryEffect> disabledToggles = [];

        public List<BaseEnchant> EquippedEnchants = [];

        public bool HasEquippedSkill;

        /// <summary>
        /// Old player positions up to 1 minute ago. Updated once every 2.5 seconds.
        /// </summary>
        public Vector2[] OldPositionBig = new Vector2[24];
        public Vector2 SandsOfTimePosition;

        public bool IsStandingStill;
        public float AttackSpeed;
        /// <summary>
        /// Caches the attack speed every frame. Ensures that attack speed is properly calculated for any given place during the update loop, by retrieving the last frame's speed otherwise.
        /// </summary>
        public float CachedAttackSpeed;
        public float UseTimeDebt;
        public float WingTimeModifier = 1f;

        public bool FreeEaterSummon = true;

        public bool RustRifleReloading = false;
        public float RustRifleReloadZonePos = 0;
        public float RustRifleTimer = 0;

        public int NekomiFrameCounter;
        public int NekomiFrame;

        public int EternalFrameCounter;
        public int EternalFrame;

        public int LeashHit;

        public int EgyptianFlailCD = 0;
        public int SKSCancelTimer;

        public int RockeaterDistance = EaterLauncher.BaseDistance;

        public int The22Incident;
        public bool DevianttIntroduction = false;

        public bool SpawnedCoffinGhost = false;
        public bool Grappled = false;

        public float LockedMana = 0;

        public bool shouldShoot;
        public int useDirection = -1;
        public float useRotation = 0;
        public int swingDirection = -1;

        public static Item? ApprenticeSupportItem;
        
        public Dictionary<int, bool> KnownBuffsToPurify = [];

        public bool Toggler_ExtraAttacksDisabled = false;
        public bool Toggler_MinionsDisabled = false;
        public bool Toggler_ExtraJumpsDisabled = false;
        public int ToggleRebuildCooldown = 0;
        public int EmodeToggleCooldown = 0;
        public bool UsingAnkh => Player.HeldItem.type == ModContent.ItemType<AccursedAnkh>() && Player.ItemAnimationActive;
        public bool ImmuneToDamage => 
            BetsyDashing ||
            GoldShell || 
            SpectreGhostTime > 0;

        public bool IsStillHoldingInSameDirectionAsMovement
            => (Player.velocity.X > 0 && Player.controlRight)
            || (Player.velocity.X < 0 && Player.controlLeft)
            || Player.dashDelay < 0
            || IsDashingTimer > 0;

        //grapple check needed because grapple state extends dash state forever
        public bool IsInADashState
            => (Player.dashDelay == -1 || IsDashingTimer > 0 || ValhallaVerticalDashing > 0) && Player.grapCount <= 0;

        public bool BossAliveLastFrame = false;

        public AccessoryEffect[] ActiveSkills = new AccessoryEffect[4];
        public override void SaveData(TagCompound tag)
        {
            var playerData = new List<string>();
            if (MutantsPactSlot) playerData.Add("MutantsPactSlot");
            if (MutantsDiscountCard) playerData.Add("MutantsDiscountCard");
            if (MutantsCreditCard) playerData.Add("MutantsCreditCard");
            if (ReceivedMasoGift) playerData.Add("ReceivedMasoGift");
            if (RabiesVaccine) playerData.Add("RabiesVaccine");
            if (DeerSinew) playerData.Add("DeerSinew");
            if (OrdinaryCarrot) playerData.Add("OrdinaryCarrot");
            if (ConcentratedRainbowMatter) playerData.Add("ConcentratedRainbowMatter");
            if (Toggler_ExtraAttacksDisabled) playerData.Add("Toggler_ExtraAttacksDisabled");
            if (Toggler_MinionsDisabled) playerData.Add("Toggler_MinionsDisabled");
            if (Toggler_ExtraJumpsDisabled) playerData.Add("Toggler_ExtraJumpsDisabled");
            if (HasEquippedSkill) playerData.Add("HasEquippedSkill");

            tag.Add($"{Mod.Name}.{Player.name}.Data", playerData);

            var togglesOff = new List<string>();
            if (Toggler != null && Toggler.Toggles != null)
            {
                foreach (KeyValuePair<AccessoryEffect, Toggle> entry in Toggler.Toggles)
                {
                    if (!Toggler.Toggles[entry.Key].ToggleBool)
                        togglesOff.Add(entry.Key.Name);
                }
            }
            tag.Add($"{Mod.Name}.{Player.name}.TogglesOff", togglesOff);

            var activeSkills = new List<string>();
            foreach (var slot in ActiveSkills)
            {
                if (slot == null)
                    activeSkills.Add("Empty");
                else
                    activeSkills.Add(slot.Name);
            }
            tag.Add($"{Mod.Name}.{Player.name}.ActiveSkills", activeSkills);
            Toggler.Save();
        }

        public override void LoadData(TagCompound tag)
        {
            FargoUIManager.Close<ActiveSkillMenu>();

            var playerData = tag.GetList<string>($"{Mod.Name}.{Player.name}.Data");
            MutantsPactSlot = playerData.Contains("MutantsPactSlot");
            MutantsDiscountCard = playerData.Contains("MutantsDiscountCard");
            MutantsCreditCard = playerData.Contains("MutantsCreditCard");
            ReceivedMasoGift = playerData.Contains("ReceivedMasoGift");
            RabiesVaccine = playerData.Contains("RabiesVaccine");
            DeerSinew = playerData.Contains("DeerSinew");
            OrdinaryCarrot = playerData.Contains("OrdinaryCarrot");
            ConcentratedRainbowMatter = playerData.Contains("ConcentratedRainbowMatter");
            Toggler_ExtraAttacksDisabled = playerData.Contains("Toggler_ExtraAttacksDisabled");
            Toggler_MinionsDisabled = playerData.Contains("Toggler_MinionsDisabled");
            Toggler_ExtraJumpsDisabled = playerData.Contains("Toggler_ExtraJumpsDisabled");
            HasEquippedSkill = playerData.Contains("HasEquippedSkill");

            List<string> disabledToggleNames = tag.GetList<string>($"{Mod.Name}.{Player.name}.TogglesOff").ToList();
            disabledToggles = ToggleLoader.LoadedToggles.Keys.Where(x => disabledToggleNames.Contains(x.Name)).ToList();

            List<string> savedSkills = tag.GetList<string>($"{Mod.Name}.{Player.name}.ActiveSkills").ToList();
            for (int i = 0; i < ActiveSkills.Length; i++)
            {
                if (savedSkills.Count <= i)
                    break;
                ActiveSkills[i] = savedSkills[i] == "Empty" ? null : AccessoryEffectLoader.AccessoryEffects.Find(x => x.Name == savedSkills[i]);
            }
        }
        public override void OnEnterWorld()
        {
            Toggler.TryLoad();
            Toggler.LoadPlayerToggles(Player);
            disabledToggles.Clear();
            CooldownBarManager.Instance.RemoveAllChildren();
            ResetOldPosition();

            if (WorldSavingSystem.QueueEnableEternityMode)
            {
                WorldSavingSystem.QueueEnableEternityMode = false;
                EternityDifficultyOption.EnableEternity();
            }

            if (ClientConfig.Instance.MusicModNotification && !ModLoader.TryGetMod("FargowiltasMusic", out Mod _))
            {
                Main.NewText(Language.GetTextValue($"Mods.{Mod.Name}.Message.NoMusic1"), Color.LimeGreen);
                Main.NewText(Language.GetTextValue($"Mods.{Mod.Name}.Message.NoMusic2"), Color.LimeGreen);
            }
            if (!ModLoader.TryGetMod("FargowiltasCrossmod", out Mod soulsDLC))
            {
                List<string> supportedMods = [];
                if (FargowiltasSouls.CalamityMod != null)
                {
                    supportedMods.Add(FargowiltasSouls.CalamityMod.DisplayName);
                }
                if (ModLoader.TryGetMod("NoxusBoss", out Mod WotG))
                {
                    supportedMods.Add(WotG.DisplayName);
                }
                if (supportedMods.Count > 0)
                {
                    string modsString = "";
                    for (int i = 0; i < supportedMods.Count; i++)
                    {
                        modsString += supportedMods[i];
                        if (i + 2 < supportedMods.Count)
                        {
                            modsString += ", ";
                        }
                        else if (i + 1 < supportedMods.Count)
                        {
                            modsString += " and ";
                        }
                    }
                    Main.NewText(Language.GetTextValue($"Mods.{Mod.Name}.Message.NoDLC1", modsString), Color.Green);
                    Main.NewText(Language.GetTextValue($"Mods.{Mod.Name}.Message.NoDLC2"), Color.Green);
                }
            }

            if (ClientConfig.Instance.WikiNotification)
                Main.NewText(Language.GetTextValue($"Mods.{Mod.Name}.Message.Wiki"), Color.Lime);

            if (Toggler.CanPlayMaso)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    WorldSavingSystem.CanPlayMaso = true;
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    //Main.NewText("send it");
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)FargowiltasSouls.PacketID.SyncCanPlayMaso);
                    packet.Write(Toggler.CanPlayMaso);
                    packet.Send();
                }
            }

        }
        public override void ResetEffects()
        {
            HasDash = false;
            FargoDash = DashType.None;

            AttackSpeed = 1f;

            if (NoUsingItems > 0)
                NoUsingItems--;

            SupersonicDodge = false;

            //            Wood = false;

            WingTimeModifier = 1f;

            if (Player.Alive())
                SpawnedCoffinGhost = false;

            QueenStingerItem = null;
            EridanusSet = false;
            GaiaSet = false;
            StyxSet = false;
            if (StyxAttackReadyTimer > 0)
                StyxAttackReadyTimer--;
            NekomiSet = false;
            if (NekomiHitCD > 0)
                NekomiHitCD--;
            if (NekomiAttackReadyTimer > 0)
                NekomiAttackReadyTimer--;

            BrainMinion = false;
            EaterMinion = false;
            PixieMinion = false;
            BigBrainMinion = false;
            DukeFishron = false;

            SquirrelMount = false;

            SeekerOfAncientTreasures = false;
            AccursedSarcophagus = false;
            BabyLifelight = false;
            BabySilhouette = false;
            BiteSizeBaron = false;
            Nibble = false;
            ChibiDevi = false;
            MutantSpawn = false;
            BabyAbom = false;

            //            #region enchantments 
            PetsActive = true;
            //CrimsonRegen = false;
            LifeForceActive = false;
            CosmosForce = false;
            MinionCrits = false;
            FirstStrike = false;
            ShellHide = false;
            GoldShell = false;
            LavaWet = false;

            WoodEnchantDiscount = false;
            SnowVisual = false;
            ApprenticeEnchantActive = false;
            DarkArtistEnchantActive = false;
            CrystalEnchantActive = false;
            ChlorophyteEnchantActive = false;

            MonkEnchantActive = false;
            ShinobiEnchantActive = false;
            PlatinumEffect = null;
            CobaltEnchantActive = false;
            SquireEnchantItem = null;
            ValhallaEnchantActive = false;
            TitaniumDRBuff = false;
            TitaniumCD = false;
            CrystalAssassinDiagonal = false;



            //            #endregion

            MiningImmunity = false;

            //souls
            MeleeSoul = false;
            MagicSoul = false;
            RangedSoul = false;
            SummonSoul = false;
            ColossusSoul = false;
            SupersonicSoul = false;
            WorldShaperSoul = false;
            FlightMasterySoul = false;
            BuilderMode = false;
            DimensionSoul = false;
            if (!UniverseSoulBuffer)
            {
                UniverseSoul = false;
            }
            UniverseSoulBuffer = false;
            UniverseCore = false;
            FishSoul1 = false;
            FishSoul2 = false;
            TerrariaSoul = false;
            VoidSoul = false;
            Eternity = false;

            MinionSlotsNonstack = 0;
            SentrySlotsNonstack = 0;

            DeactivatedMinionEffectCount = 0;
            if (!GalacticMinionsDeactivatedBuffer)
            {
                GalacticMinionsDeactivated = false;
            }
            GalacticMinionsDeactivatedBuffer = false;

            ForceEffects?.Clear();

            //maso
            SlimyShieldItem = null;
            RottingHeartItem = null;
            NecromanticBrewItem = null;
            DeerSinewNerf = false;
            PureHeart = false;
            PungentEyeballMinion = false;
            CrystalSkullMinion = false;
            FusedLens = false;
            FusedLensCursed = false;
            FusedLensIchor = false;
            TwinsInstall = false;
            DubiousCircuitry = false;
            Supercharged = false;
            Probes = false;
            MagicalBulb = false;
            PlanterasChild = false;
            CrystalSkull = false;
            PungentEyeball = false;
            LanternGuarding = false;
            LihzahrdTreasureBoxItem = null;
            BetsysHeartItem = null;
            BetsyDashing = false;
            MutantAntibodies = false;
            GravityGlobeEXItem = null;
            MoonChalice = false;
            LunarCultist = false;
            TrueEyes = false;
            AbomWandItem = null;
            MasochistSoul = false;
            MasochistSoulItem = null;
            MasochistHeart = false;
            SandsofTime = false;
            SecurityWallet = false;
            NymphsPerfume = false;
            NymphsPerfumeRespawn = false;
            RainbowSlime = false;
            SkeletronArms = false;
            CirnoGraze = false;
            MiniSaucer = false;
            SupremeDeathbringerFairy = false;
            GodEaterImbue = false;
            MutantSetBonusItem = null;
            AbomMinion = false;
            PhantasmalRing = false;
            TwinsEX = false;
            TimsConcoction = false;
            DeviGraze = false;
            Graze = false;
            GrazeRadius = 100f;
            DevianttHeartItem = null;
            MutantEyeItem = null;
            MutantEyeVisual = false;
            AbomRebirth = false;
            WasHurtBySomething = false;
            PrecisionSeal = false;
            GelicWingsItem = null;

            Ambrosia = false;
            SpecialDash = false;

            //debuffs
            Hexed = false;
            Unstable = false;
            Fused = false;
            Shadowflame = false;
            Daybroken = false;
            Oiled = false;
            Slimed = false;
            noDodge = false;
            NoMomentum = false;
            Bloodthirsty = false;
            DisruptedFocus = false;
            BaronsBurden = false;
            BleedingOut = false;

            Smite = false;
            Anticoagulation = false;
            GodEater = false;
            FlamesoftheUniverse = false;
            IvyVenom = false;
            Asocial = false;
            Defenseless = false;
            Infested = false;
            Rotting = false;
            SqueakyToy = false;
            Atrophied = false;
            Jammed = false;
            ReverseManaFlow = false;
            CurseoftheMoon = false;
            OceanicMaul = false;
            DeathMarked = false;
            Midas = false;
            if (!MutantPresenceBuffer)
            {
                if (MutantPresence == false)
                    PresenceTogglerTimer = 0;
                MutantPresence = MutantPresence && Player.HasBuff(ModContent.BuffType<MutantPresenceBuff>());
            }
            MutantPresenceBuffer = false;
            HadMutantPresence = MutantPresence;
            MutantDesperation = false;
            MutantFang = false;
            Swarming = false;
            LowGround = false;
            Illuminated = false;
            //LihzahrdBlessing = false;
            Berserked = false;
            Stunned = false;
            HasJungleRose = false;
            Quicksanding = false;
            HaveCheckedAttackSpeed = false;
            SquirrelCharm = null;
            OxygenTank = false;
            //IronEnchantShield = false;
            WizardedItem = null;

            EquippedEnchants.Clear();

            WizardTooltips = false;

            if (WizardEnchantActive)
            {
                WizardEnchantActive = false;
                List<Item> accessories = [];
                for (int i = 3; i < 10; i++)
                    if (Player.IsItemSlotUnlockedAndUsable(i))
                        accessories.Add(Player.armor[i]);
                AccessorySlotLoader loader = LoaderManager.Get<AccessorySlotLoader>();
                ModAccessorySlotPlayer modSlotPlayer = Player.GetModPlayer<ModAccessorySlotPlayer>();
                for (int i = 0; i < modSlotPlayer.SlotCount; i++)
                    if (loader.ModdedIsItemSlotUnlockedAndUsable(i, Player))
                        accessories.Add(loader.Get(i, Player).FunctionalItem);
                for (int i = 0; i < accessories.Count - 1; i++)
                {
                    if (!accessories[i].IsAir && (accessories[i].type == ModContent.ItemType<WizardEnchant>() || accessories[i].type == ModContent.ItemType<CosmoForce>()))
                    {
                        WizardEnchantActive = true;
                        Item ench = accessories[i + 1];
                        if (ench != null && !ench.IsAir && ench.ModItem != null && ench.ModItem is BaseEnchant)
                        {
                            WizardedItem = ench;
                        }
                        break;
                    }
                }
            }

            if (!Mash && MashCounter > 0)
                MashCounter--;
            Mash = false;
            GrabDamage = false;

            if (Player.grapCount <= 0)
                Grappled = false;

            if (!FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>()) && !EModeGlobalNPC.mutantBoss.IsWithinBounds(Main.maxNPCs))
                The22Incident = 0;

        }
        public override void OnRespawn()
        {
            if (NymphsPerfumeRespawn)
                NymphsPerfumeRestoreLife = 6;
            ResetOldPosition();
        }
        public override void ModifyScreenPosition()
        {
            Projectile ghost = Main.projectile.FirstOrDefault(p => p.TypeAlive<CoffinPlayerSoul>() && p.owner == Player.whoAmI);
            if (ghost != null && ghost.Alive())
            {
                Main.screenPosition = ghost.Center - (new Vector2(Main.screenWidth, Main.screenHeight) / 2);
            }
        }
        public override void UpdateDead()
        {
            bool wasSandsOfTime = SandsofTime;
            bool wasNymphsPerfumeRespawn = NymphsPerfumeRespawn;

            ResetEffects();

            SandsofTime = wasSandsOfTime;
            NymphsPerfumeRespawn = wasNymphsPerfumeRespawn;

            DevianttIntroduction = false;

            if (!SpawnedCoffinGhost && NPC.AnyNPCs(ModContent.NPCType<CursedCoffin>()))
            {
                SpawnedCoffinGhost = true;
                Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<CoffinPlayerSoul>(), 0, 0, Player.whoAmI);
            }

            if (SandsofTime && !LumUtils.AnyBosses() && Player.respawnTimer > 10)
                Player.respawnTimer -= Eternity ? 6 : 1;

            //maso disables respawning during mp boss
            /*if (WorldSavingSystem.MasochistModeReal && LumUtils.AnyBosses())
            {
                if (Player.respawnTimer < 10)
                    Player.respawnTimer = 10;

                if (Main.netMode == NetmodeID.MultiplayerClient && Main.npc[FargoSoulsGlobalNPC.boss].HasValidTarget && Main.npc[FargoSoulsGlobalNPC.boss].HasPlayerTarget)
                    Player.Center = Main.player[Main.npc[FargoSoulsGlobalNPC.boss].target].Center;
            }*/

            ReallyAwfulDebuffCooldown = 0;
            ParryDebuffImmuneTime = 0;
            WingTimeModifier = 1f;
            FreeEaterSummon = true;

            AbominableWandRevived = false;

            EridanusTimer = 0;
            StyxMeter = 0;
            StyxTimer = 0;
            StyxAttackReadyTimer = 0;
            NekomiMeter = 0;
            NekomiTimer = 0;
            NekomiAttackReadyTimer = 0;

            CirnoGrazeCounter = 0;

            //debuffs
            unstableCD = 0;
            lightningRodTimer = 0;

            BuilderMode = false;
            NoUsingItems = 0;

            FreezeTime = false;
            freezeLength = 0;

            ChillSnowstorm = false;
            chillLength = 0;

            SlimyShieldFalling = false;
            RottingHeartCD = 60;
            GuttedHeartCD = 60;
            IsDashingTimer = 0;
            GroundPound = 0;
            NymphsPerfumeCD = 30;
            WretchedPouchCD = 0;


            DeviGrazeBonus = 0;
            MutantEyeCD = 60;

            MythrilTimer = 0;
            MythrilDelay = 20;

            Mash = false;
            GrabDamage = false;
            WizardEnchantActive = false;
            MashCounter = 0;

            MaxLifeReduction = 0;
            CurrentLifeReduction = 0;

            The22Incident = 0;
        }



        public override void ModifyLuck(ref float luck)
        {
            if (Unlucky)
                luck -= 1.0f;

            Unlucky = false;
        }

        internal List<int> prevDyes = null;

        public void ManageLifeReduction()
        {
            if (MaxLifeReduction > 100)
                MaxLifeReduction = 100;

            if (OceanicMaul && LifeReductionUpdateTimer <= 0)
                LifeReductionUpdateTimer = 1; //trigger life reduction behaviour

            if (LifeReductionUpdateTimer > 0)
            {
                const int threshold = 30;
                if (LifeReductionUpdateTimer++ > threshold)
                {
                    LifeReductionUpdateTimer = 1;

                    if (OceanicMaul) //with maul, real max life gradually decreases to the desired point
                    {
                        if (MutantFang) //update faster
                            LifeReductionUpdateTimer = threshold - 10;

                        int newLifeReduction = CurrentLifeReduction + 5;
                        if (newLifeReduction > MaxLifeReduction)
                            newLifeReduction = MaxLifeReduction;
                        if (newLifeReduction > Player.statLifeMax2 - 100) //i.e. max life wont go below 100
                            newLifeReduction = Player.statLifeMax2 - 100;

                        if (CurrentLifeReduction < newLifeReduction)
                        {
                            CurrentLifeReduction = newLifeReduction;
                            CombatText.NewText(Player.Hitbox, Color.DarkRed, Language.GetTextValue($"Mods.{Mod.Name}.Buffs.OceanicMaulBuff.LifeDown"));
                        }
                    }
                    else //after maul wears off, real max life gradually recovers to normal value
                    {
                        CurrentLifeReduction -= 5;
                        if (MaxLifeReduction > CurrentLifeReduction)
                            MaxLifeReduction = CurrentLifeReduction;
                        CombatText.NewText(Player.Hitbox, Color.DarkGreen, Language.GetTextValue($"Mods.{Mod.Name}.Buffs.OceanicMaulBuff.LifeUp"));
                    }
                }
            }

            if (CurrentLifeReduction > 0)
            {
                if (CurrentLifeReduction > Player.statLifeMax2 - 100) //i.e. max life wont go below 100
                    CurrentLifeReduction = Player.statLifeMax2 - 100;
                Player.statLifeMax2 -= CurrentLifeReduction;
                //if (Player.statLife > Player.statLifeMax2) Player.statLife = Player.statLifeMax2;
            }
            else if (!OceanicMaul) //deactivate behaviour
            {
                CurrentLifeReduction = 0;
                MaxLifeReduction = 0;
                LifeReductionUpdateTimer = 0;
            }
        }
        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (Player.HasEffect<NinjaEffect>() && item.IsWeapon())
            {
                if (NinjaCounter >= 1)
                {
                    //velocity *= 2f;
                    //knockback *= 2f;
                    NinjaCounter--;
                    NinjaDecrementCD = NinjaDecrementMaxCD;
                }
            }
        }
        public override void ModifyWeaponCrit(Item item, ref float crit)
        {
            if (Player.HasEffect<NinjaEffect>() && item.IsWeapon())
            {
                if (NinjaCounter >= 1)
                {
                    crit += Player.ForceEffect<NinjaEffect>() ? 30 : 16;
                }
            }
        }
        public override float UseSpeedMultiplier(Item item)
        {
            int useTime = item.useTime;
            int useAnimate = item.useAnimation;

            if (useTime <= 0 || useAnimate <= 0 || item.damage <= 0)
                return base.UseSpeedMultiplier(item);

            if (!HaveCheckedAttackSpeed)
            {
                HaveCheckedAttackSpeed = true;

                AttackSpeed += Player.AccessoryEffects().ModifyUseSpeed(item);

                if (Berserked)
                {
                    AttackSpeed += .1f;
                }

                //if (MagicSoul && item.CountsAsClass(DamageClass.Magic))
                //{
                //    AttackSpeed += .2f;
                //}

                if (Player.HasEffect<MythrilEffect>())
                {
                    MythrilEffect.CalcMythrilAttackSpeed(this, item);
                }

                if (Player.HasEffect<AdamantiteEffect>())
                {
                    AdamantiteEffect.CalcAdamantiteAttackSpeed(Player, item);
                }
                
                float originalAttackSpeed = AttackSpeed;
                float originalUseTime = useTime / AttackSpeed;
                if (UseTimeDebt > 1f)
                {
                    //when accummulated enough debt, pay it off. use time will round down this tick.
                    UseTimeDebt -= 1f;
                }
                else //normally, force use time to round up
                {
                    //modify attack speed so it rounds up
                    int useTimeRoundUp = (int)Math.Round(useTime / AttackSpeed, MidpointRounding.ToPositiveInfinity);
                    //Main.NewText($"pre {useTime / AttackSpeed}, target {useTimeRoundUp}");
                    while (useTime / AttackSpeed < useTimeRoundUp)
                        AttackSpeed -= .01f; //small increments to avoid skipping past any integers
                    //Main.NewText($"result {useTime / AttackSpeed}");

                    float newUseTime = useTime / AttackSpeed;
                    UseTimeDebt += newUseTime - originalUseTime; //track the sub-1 unit "debt" of shorter useTime
                }
                //Main.NewText($"oldASpd: {originalAttackSpeed}, newASpd: {AttackSpeed}, oldUT: {originalUseTime}, newUT: {(useTime / AttackSpeed)}, debt: {UseTimeDebt}");

                //checks so weapons dont break
                while (useTime / AttackSpeed < 1)
                {
                    AttackSpeed -= .01f;
                }

                while (useAnimate / AttackSpeed < 3)
                {
                    AttackSpeed -= .01f;
                }

                if (AttackSpeed < .1f)
                    AttackSpeed = .1f;
            }

            if (item.shoot >= ProjectileID.None && ProjectileID.Sets.IsAWhip[item.shoot] && AttackSpeed > 1)
            {
                Player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += AttackSpeed - 1f;
                return 1f;
            }
            CachedAttackSpeed = AttackSpeed;
            return AttackSpeed;
        }
        public override void OnConsumeMana(Item item, int manaConsumed)
        {
            if (ReverseManaFlow)
            {
                LockedMana += manaConsumed;
            }
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            //            if (squireReduceIframes && (SquireEnchant || ValhallaEnchant))
            //            {
            //                if (Main.rand.NextBool(3))
            //                {
            //                    float scale = ValhallaEnchant ? 2f : 1.5f;
            //                    int type = ValhallaEnchant ? 87 : 91;
            //                    int dust = Dust.NewDust(Player.position, Player.width, Player.height, type, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 87, default(Color), scale);
            //                    Main.dust[dust].noGravity = true;
            //                    Main.dust[dust].velocity *= 1.8f;
            //                    Main.dust[dust].velocity.Y -= 0.5f;
            //                    if (Main.rand.NextBool(4))
            //                    {
            //                        Main.dust[dust].noGravity = false;
            //                        Main.dust[dust].scale *= 0.5f;
            //                    }
            //                    Main.PlayerDrawDust.Add(dust);
            //                }
            //                fullBright = true;
            //            }

            if (Shadowflame)
            {
                if (Main.rand.NextBool(4) && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width, Player.height, DustID.Shadowflame, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    drawInfo.DustCache.Add(dust);
                }
                fullBright = true;
            }

            if (Rotting)
            {
                if (drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width, Player.height, DustID.Blood, Player.velocity.X * 0.1f, Player.velocity.Y * 0.1f, 0, default, 2f);
                    Main.dust[dust].noGravity = Main.rand.NextBool();
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    drawInfo.DustCache.Add(dust);
                }
            }

            if (Smite)
            {
                if (drawInfo.shadow == 0f)
                {
                    Color color = Main.DiscoColor;
                    int index2 = Dust.NewDust(Player.position, Player.width, Player.height, DustID.GemDiamond, 0.0f, 0.0f, 100, color, 1f);
                    Main.dust[index2].velocity *= 2f;
                    Main.dust[index2].noGravity = true;
                    drawInfo.DustCache.Add(index2);
                }
            }

            if (Anticoagulation)
            {
                if (drawInfo.shadow == 0f)
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Blood);
                    Main.dust[d].velocity *= 2f;
                    Main.dust[d].scale += 1f;
                }
            }

            if (Hexed)
            {
                if (Main.rand.NextBool(3) && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width, Player.height, DustID.BubbleBlock, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 2.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 2f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    Main.dust[dust].color = Color.GreenYellow;
                    drawInfo.DustCache.Add(dust);
                }
                if (Main.rand.NextBool() && drawInfo.shadow == 0f)
                {
                    int index2 = Dust.NewDust(Player.position, Player.width, Player.height, DustID.RuneWizard, 0.0f, 0.0f, 100, default, 2.5f);
                    Main.dust[index2].noGravity = true;
                    drawInfo.DustCache.Add(index2);
                }
                fullBright = true;
            }

            if (Infested)
            {
                if (Main.rand.NextBool(4) && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width, Player.height, DustID.JungleSpore, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, InfestedDust);
                    Main.dust[dust].noGravity = true;
                    //Main.dust[dust].velocity *= 1.8f;
                    // Main.dust[dust].velocity.Y -= 0.5f;
                    drawInfo.DustCache.Add(dust);
                }
                fullBright = true;
            }

            if (CurrentLifeReduction > 0)
            {
                if (Main.rand.NextBool() && drawInfo.shadow == 0f)
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Blood);
                    Main.dust[d].velocity *= 2f;
                    Main.dust[d].scale += 1f;
                    drawInfo.DustCache.Add(d);
                }
            }

            if (GodEater)
            {
                if (Main.rand.NextBool(3) && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width + 4, Player.height + 4, DustID.GemAmethyst, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.2f;
                    Main.dust[dust].velocity.Y -= 0.15f;
                    drawInfo.DustCache.Add(dust);
                }
                r *= 0.15f;
                g *= 0.03f;
                b *= 0.09f;
                fullBright = true;
            }

            if (FlamesoftheUniverse)
            {
                /*drawInfo.drawPlayer.onFire = true;
                drawInfo.drawPlayer.onFire2 = true;
                drawInfo.drawPlayer.onFrostBurn = true;
                drawInfo.drawPlayer.ichor = true;
                drawInfo.drawPlayer.burned = true;*/
                if (Main.rand.NextBool(4) && drawInfo.shadow == 0f)
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.VilePowder, Player.velocity.X * 0.2f, Player.velocity.Y * 0.2f, 100, new Color(50 * Main.rand.Next(6) + 5, 50 * Main.rand.Next(6) + 5, 50 * Main.rand.Next(6) + 5), 2.5f);
                    Main.dust[d].velocity.Y -= 1;
                    Main.dust[d].velocity *= 2f;
                    Main.dust[d].noGravity = true;
                    drawInfo.DustCache.Add(d);
                }
                fullBright = true;
            }

            if (CurseoftheMoon)
            {
                if (Main.rand.NextBool(5))
                {
                    int d = Dust.NewDust(Player.Center, 0, 0, DustID.Vortex, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 3f;
                    drawInfo.DustCache.Add(d);
                }
                if (Main.rand.NextBool(5))
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Vortex, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity.Y -= 1f;
                    Main.dust[d].velocity *= 2f;
                    drawInfo.DustCache.Add(d);
                }
            }

            if (DeathMarked)
            {
                if (Main.rand.NextBool() && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position - new Vector2(2f, 2f), Player.width, Player.height, DustID.Asphalt, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 0, default, 1.5f);
                    Main.dust[dust].velocity.Y--;
                    if (!Main.rand.NextBool(3))
                    {
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].scale += 0.5f;
                        Main.dust[dust].velocity *= 3f;
                        Main.dust[dust].velocity.Y -= 0.5f;
                    }
                    drawInfo.DustCache.Add(dust);
                }
                r *= 0.2f;
                g *= 0.2f;
                b *= 0.2f;
                fullBright = true;
            }

            if (Fused)
            {
                if (Main.rand.NextBool() && drawInfo.shadow == 0f)
                {
                    int dust = Dust.NewDust(Player.position + new Vector2(Player.width / 2, Player.height / 5), 0, 0, DustID.Torch, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 0, default, 2f);
                    Main.dust[dust].velocity.Y -= 2f;
                    Main.dust[dust].velocity *= 2f;
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].scale += 0.5f;
                        Main.dust[dust].noGravity = true;
                    }
                    drawInfo.DustCache.Add(dust);
                }
            }

            if (Supercharged)
            {
                if (Main.rand.NextBool(6) && drawInfo.shadow == 0f && !Main.gamePaused)
                {
                    Vector2 dir = Main.rand.NextVector2Circular(20, 15);
                    Particle p = new RectangleParticle(Player.Center + dir, Player.velocity * 0.7f + dir, Color.Teal, Main.rand.NextFloat(0.1f, 0.25f), 50, true);
                    p.Spawn();
                }
            }

            if (TwinsInstall)
            {
                if (Main.rand.NextBool(3) && drawInfo.shadow == 0f && !Main.gamePaused)
                {
                    bool windleft = false;
                    if (Main.windSpeedCurrent < 0)
                        windleft = true;
                    Vector2 spawnpos = windleft ? Player.Right + new Vector2(5, 0) : Player.Left + new Vector2(-5, 0);
                    Vector2 dir = new(Main.rand.NextFloat(0, 2), Main.rand.NextFloat(-18, -22));
                    Color color = Color.Lerp(Color.Green, Color.Yellow, Main.rand.NextFloat(0, 1));
                    float offset = windleft ? (-MathHelper.PiOver2 + Main.rand.NextFloatDirection() * 0.51f) : MathHelper.Pi*2 + Main.rand.NextFloatDirection() * 0.51f;
                    Particle p = new SmokeParticle(spawnpos, Player.velocity * 0.7f + dir, color, 50, Main.rand.NextFloat(0.4f, 0.8f), 0.05f, offset, false);
                    p.Velocity.X += Main.windSpeedCurrent * 10;
                    p.Spawn();
                }
            }
        }
        public void ConcentratedRainbowMatterTryAutoHeal()
        {
            if (ConcentratedRainbowMatter
                && Player.statLife < Player.statLifeMax2
                && Player.potionDelay <= 0
                && Player.HasEffect<RainbowHealEffect>())
            {
                Item potion = Player.QuickHeal_GetItemToUse();
                if (potion != null)
                {
                    int heal = Player.GetHealLife(potion); //GetHealMultiplier(potion.healLife);
                    float threshold = ClientConfig.Instance.RainbowHealThreshold / 100f;
                    if (Player.statLife < Player.statLifeMax2 * threshold && //heal when low
                        //Player.statLife < Player.statLifeMax2 - heal && //only heal when full benefit (no wasted overheal)
                        Main.npc.Any(n => n.active && n.damage > 0 && !n.friendly //only heal when danger nearby (not after respawn in safety)
                            && Player.Distance(n.Center) < 1200 && (n.noTileCollide || Collision.CanHitLine(Player.Center, 0, 0, n.Center, 0, 0))))
                    {
                        Player.QuickHeal();
                    }
                }
            }
        }


        private PlayerDeathReason DeathByLocalization(string key)
        {
            LocalizedText DeathText = Language.GetText($"Mods.FargowiltasSouls.DeathMessage.{key}");
            return PlayerDeathReason.ByCustomReason(DeathText.ToNetworkText(Player.name));
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            bool retVal = true;

            bool dodgable = true;
            if ((damageSource.SourceProjectileLocalIndex != -1 && !Main.projectile[damageSource.SourceProjectileLocalIndex].IsDamageDodgable()) || (damageSource.SourceNPCIndex != -1 && !Main.npc[damageSource.SourceNPCIndex].IsDamageDodgeable()))
                dodgable = false;

            if (Player.statLife <= 0 && dodgable) //revives
            {
                if (Player.whoAmI == Main.myPlayer && retVal && AbomRebirth)
                {
                    if (!WasHurtBySomething)
                    {
                        Player.statLife = 1;
                        return false; //short circuits the rest, this is deliberate
                    }
                }
                if (Player.whoAmI == Main.myPlayer && retVal && Player.HasEffect<SpectreEffect>() && !Player.HasBuff<FossilReviveCDBuff>())
                {
                    SpectreEffect.SpectreRevive(Player);
                    retVal = false;
                }
                if (Player.whoAmI == Main.myPlayer && retVal && Player.HasEffect<FossilEffect>() && !Player.HasBuff<FossilReviveCDBuff>())
                {
                    FossilEffect.FossilRevive(Player);
                    retVal = false;
                }

                if (Player.whoAmI == Main.myPlayer && retVal && MutantSetBonusItem != null && Player.FindBuffIndex(ModContent.BuffType<MutantRebirthBuff>()) == -1)
                {
                    TryCleanseDebuffs();
                    Player.statLife = Player.statLifeMax2;
                    Player.HealEffect(Player.statLifeMax2);
                    Player.immune = true;
                    Player.immuneTime = 180;
                    Player.hurtCooldowns[0] = 180;
                    Player.hurtCooldowns[1] = 180;
                    string text = Language.GetTextValue($"Mods.{Mod.Name}.Message.Revived");
                    Main.NewText(text, Color.LimeGreen);
                    Player.AddBuff(ModContent.BuffType<MutantRebirthBuff>(), LumUtils.SecondsToFrames(120f));
                    retVal = false;
                }

                if (Player.whoAmI == Main.myPlayer && retVal && AbomWandItem != null && !AbominableWandRevived)
                {
                    SoundEngine.PlaySound(FargosSoundRegistry.StyxRevive, Player.Center);
                    AbominableWandRevived = true;
                    int heal = 1;
                    Player.statLife = heal;
                    Player.HealEffect(heal);
                    Player.immune = true;
                    Player.immuneTime = 120;
                    Player.hurtCooldowns[0] = 120;
                    Player.hurtCooldowns[1] = 120;
                    string text = Language.GetTextValue($"Mods.{Mod.Name}.Message.Revived");
                    CombatText.NewText(Player.Hitbox, Color.Yellow, text, true);
                    Main.NewText(text, Color.Yellow);
                    Player.AddBuff(ModContent.BuffType<AbomRebirthBuff>(), 900);
                    retVal = false;
                    for (int i = 0; i < 24; i++)
                    {
                        Projectile.NewProjectile(Player.GetSource_Accessory(AbomWandItem), Player.Center, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(4f, 16f),
                            ModContent.ProjectileType<StyxArmorScythe2>(), 0, 10f, Main.myPlayer, -60 - Main.rand.Next(60), -1);
                    }
                }
            }

            //killed by damage over time
            if (damage == 10.0 && hitDirection == 0 && damageSource.SourceOtherIndex == 8)
            {
                if (Infested)
                    damageSource = DeathByLocalization("Infested");

                if (Anticoagulation)
                    damageSource = DeathByLocalization("Anticoagulation");

                if (Rotting)
                    damageSource = DeathByLocalization("Rotting");

                if (IvyVenom)
                    damageSource = PlayerDeathReason.ByOther(9);

                if (Shadowflame)
                    damageSource = DeathByLocalization("Shadowflame");

                if (Daybroken)
                    damageSource = PlayerDeathReason.ByOther(8);

                if (GodEater || FlamesoftheUniverse || CurseoftheMoon || MutantFang)
                    damageSource = DeathByLocalization("DivineWrath");
            }

            /*if (MutantPresence)
            {
                damageSource = PlayerDeathReason.ByCustomReason(Player.name + " was penetrated.");
            }*/

            if (StatLifePrevious > 0 && Player.statLife > StatLifePrevious)
                StatLifePrevious = Player.statLife;

            if (!retVal)
            {
                //if (!Main.dedServ)
                    //SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Accessories/Revive"), Player.Center);

                if (Player.whoAmI == Main.myPlayer && MutantSetBonusItem != null)
                    Projectile.NewProjectile(Player.GetSource_Accessory(MutantSetBonusItem), Player.Center, -Vector2.UnitY, ModContent.ProjectileType<GiantDeathray>(), (int)(7000 * Player.ActualClassDamage(DamageClass.Magic)), 10f, Player.whoAmI);
            }

            
            if (Main.myPlayer == Player.whoAmI && WorldSavingSystem.EternityMode && retVal)
            {
                if (Player.statLife <= 0)
                {      
                    if (LumUtils.AnyBosses())
                    {
                        ModContent.GetInstance<FiftyDeathToEternityBossAchievement>().IntCondition.Value++;
                    }
                }
            }

            return retVal;
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            base.Kill(damage, hitDirection, pvp, damageSource);
            SandsOfTimePosition = OldPositionBig[^1];

        }
        public void ResetOldPosition()
        {
            for (int i = 0; i < OldPositionBig.Length; i++)
                OldPositionBig[i] = Player.Center;
        }
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (GaiaOffense) //set armor and accessory shaders to gaia shader if set bonus is triggered
            {
                int gaiaShader = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<GaiaDye>());
                drawInfo.cBody = gaiaShader;
                drawInfo.cHead = gaiaShader;
                drawInfo.cLegs = gaiaShader;
                drawInfo.cWings = gaiaShader;
                drawInfo.cHandOn = gaiaShader;
                drawInfo.cHandOff = gaiaShader;
                drawInfo.cShoe = gaiaShader;
            }

            if (GuardRaised)
            {
                drawInfo.drawPlayer.shieldRaised = true;
                if (shieldTimer > 0)
                {
                    int shader = 0;
                    if (Player.HasEffect<PumpkingsCapeEffect>())
                        shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.PixieDye);
                    else if (Player.HasEffect<LithosphericEffect>())
                        shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.PurpleOozeDye);
                    else if (Player.HasEffect<DreadShellEffect>())
                        shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.BloodbathDye);
                    else if (Player.HasEffect<SilverEffect>())
                        shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.ReflectiveSilverDye);

                    if (shader != 0)
                    {
                        drawInfo.cBody = shader;
                        drawInfo.cHead = shader;
                        drawInfo.cLegs = shader;
                        drawInfo.cWings = shader;
                        drawInfo.cHandOn = shader;
                        drawInfo.cHandOff = shader;
                        drawInfo.cShoe = shader;
                        drawInfo.cBack = shader;
                        drawInfo.cBackpack = shader;
                        drawInfo.cShield = shader;
                        drawInfo.cNeck = shader;
                        drawInfo.cHandOn = shader;
                        drawInfo.cHandOff = shader;
                        drawInfo.cBalloon = shader;
                        drawInfo.cBalloonFront = shader;
                        drawInfo.cFace = shader;
                        drawInfo.cFaceHead = shader;
                        drawInfo.cFront = shader;
                    }
                }
            }

            if (!(Player.armor[1].type != ModContent.ItemType<StyxChestplate>() && Player.armor[11].type != ModContent.ItemType<StyxChestplate>()))
            {
                drawInfo.bodyGlowMask = ModContent.ItemType<StyxChestplate>();
                drawInfo.bodyGlowColor = Color.White;
                drawInfo.armGlowColor = Color.White;
            }
            if (!(Player.armor[1].type != ModContent.ItemType<EridanusBattleplate>() && Player.armor[11].type != ModContent.ItemType<EridanusBattleplate>()))
            {
                drawInfo.bodyGlowMask = ModContent.ItemType<EridanusBattleplate>();
                drawInfo.bodyGlowColor = Color.White;
                drawInfo.armGlowColor = Color.White;
            }
        }

        
        

        public override void OnExtraJumpStarted(ExtraJump jump, ref bool playSound)
        {
            if (Player.HasEffect<CobaltEffect>() && Player.whoAmI == Main.myPlayer)
            {
                Projectile p = FargoSoulsUtil.NewProjectileDirectSafe(Player.GetSource_EffectItem<CobaltEffect>(), Player.Center, Vector2.Zero, ModContent.ProjectileType<CobaltExplosion>(), AncientCobaltEffect.BaseDamage(Player) / 2, 0f, Main.myPlayer);
                if (p != null)
                    p.FargoSouls().CanSplit = false;
            }
            if (Player.HasEffect<GelicWingSpikes>())
            {
                if (Player.whoAmI == Main.myPlayer && jump is UnicornMountJump)
                {
                    int dam = 25;
                    for (int j = -1; j <= 1; j += 2)
                    {
                        Vector2 baseVel = Vector2.UnitX.RotatedBy(MathHelper.ToRadians(-10 * j));
                        int max = 4;
                        int ai2 = Player.FargoSouls().PureHeart ? 1 : 0;
                        for (int i = 0; i < max; i++)
                        {
                            Vector2 vel = Main.rand.NextFloat(5f, 10f) * j * baseVel.RotatedBy(-MathHelper.PiOver4 * 0.8f / max * i * j);
                            Projectile.NewProjectile(Player.GetSource_Accessory(Player.EffectItem<GelicWingSpikes>()), Player.Bottom - Vector2.UnitY * 8, vel, ModContent.ProjectileType<GelicWingSpike>(), FargoSoulsUtil.HighestDamageTypeScaling(Player, dam), 5f, Main.myPlayer, 0, 0, ai2);
                        }
                    }
                }
            }
        }
        public override void ModifyExtraJumpDurationMultiplier(ExtraJump jump, ref float duration)
        {
            if (Player.HasEffect<CobaltEffect>())
                duration *= 1.2f;
        }
        public void AddPet(bool toggle, bool vanityToggle, int buff, int proj)
        {
            if (vanityToggle)
            {
                PetsActive = false;
                return;
            }

            if (Player.whoAmI == Main.myPlayer && toggle && Player.FindBuffIndex(buff) == -1 && Player.ownedProjectileCounts[proj] < 1)
            {
                Projectile p = Main.projectile[Projectile.NewProjectile(Player.GetSource_Misc("Pet"), Player.Center.X, Player.Center.Y, 0f, -1f, proj, 0, 0f, Player.whoAmI)];
                p.netUpdate = true;
            }
        }
        public void AddMinion(Item item, bool toggle, int proj, int damage, float knockback)
        {
            if (Player.whoAmI != Main.myPlayer) return;
            if (Player.ownedProjectileCounts[proj] < 1 && Player.whoAmI == Main.myPlayer && toggle)
            {
                FargoSoulsUtil.NewSummonProjectile(Player.GetSource_Accessory(item), Player.Center, -Vector2.UnitY, proj, damage, knockback, Main.myPlayer);
            }
        }

        private void KillPets()
        {
            int petId = Player.miscEquips[0].buffType;
            int lightPetId = Player.miscEquips[1].buffType;

            Player.buffImmune[petId] = true;
            Player.buffImmune[lightPetId] = true;

            Player.ClearBuff(petId);
            Player.ClearBuff(lightPetId);

            //memorizes Player selections
            if (!WasAsocial)
            {
                HidePetToggle0 = Player.hideMisc[0];
                HidePetToggle1 = Player.hideMisc[1];

                WasAsocial = true;
            }

            //disables pet and light pet too!
            if (!Player.hideMisc[0])
            {
                Player.TogglePet();
            }

            if (!Player.hideMisc[1])
            {
                Player.ToggleLight();
            }

            Player.hideMisc[0] = true;
            Player.hideMisc[1] = true;
        }

        public static void Squeak(Vector2 center, float volume = 1f)
        {
            if (!Main.dedServ)
                SoundEngine.PlaySound(new SoundStyle($"FargowiltasSouls/Assets/Sounds/SqueakyToy/squeak{Main.rand.Next(1, 7)}") with { Volume = volume }, center);
        }

        private int InfestedExtraDot()
        {
            int buffIndex = Player.FindBuffIndex(ModContent.BuffType<InfestedBuff>());
            if (buffIndex == -1)
            {
                buffIndex = Player.FindBuffIndex(ModContent.BuffType<NeurotoxinBuff>());
                if (buffIndex == -1)
                    return 0;
            }

            int timeLeft = Player.buffTime[buffIndex];
            float baseVal = (float)(MaxInfestTime - timeLeft) / 90; //change the denominator to adjust max power of DOT
            int modifier = (int)(baseVal * baseVal + 4);

            InfestedDust = baseVal / 10 + 1f;
            if (InfestedDust > 5f)
                InfestedDust = 5f;

            return modifier * 2;
        }

        //        public override void CatchFish(Item fishingRod, Item bait, int power, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType, ref bool junk)
        //        {
        //            if (bait.type == ModContent.ItemType<TruffleWormEX>())
        //            {
        //                caughtType = 0;
        //                bool spawned = false;
        //                for (int i = 0; i < 1000; i++)
        //                {
        //                    if (Main.projectile[i].active && Main.projectile[i].bobber
        //                        && Main.projectile[i].owner == Player.whoAmI && Player.whoAmI == Main.myPlayer)
        //                    {
        //                        Main.projectile[i].ai[0] = 2f; //cut fishing lines
        //                        Main.projectile[i].netUpdate = true;

        //                        if (!spawned && Main.projectile[i].wet && WorldSavingSystem.EternityMode && !NPC.AnyNPCs(NPCID.DukeFishron)) //should spawn boss
        //                        {
        //                            spawned = true;
        //                            if (Main.netMode == NetmodeID.SinglePlayer) //singlePlayer
        //                            {
        //                                EModeGlobalNPC.spawnFishronEX = true;
        //                                NPC.NewNPC((int)Main.projectile[i].Center.X, (int)Main.projectile[i].Center.Y + 100,
        //                                    NPCID.DukeFishron, 0, 0f, 0f, 0f, 0f, Player.whoAmI);
        //                                EModeGlobalNPC.spawnFishronEX = false;
        //                                Main.NewText("Duke Fishron EX has awoken!", 50, 100, 255);
        //                            }
        //                            else if (Main.netMode == NetmodeID.MultiPlayerClient) //MP, broadcast(?) packet from spawning Player's client
        //                            {
        //                                var netMessage = mod.GetPacket();
        //                                netMessage.Write((byte)FargowiltasSouls.PacketID.SpawnFishronEX);
        //                                netMessage.Write((byte)Player.whoAmI);
        //                                netMessage.Write((int)Main.projectile[i].Center.X);
        //                                netMessage.Write((int)Main.projectile[i].Center.Y + 100);
        //                                netMessage.Send();
        //                            }
        //                            else if (Main.netMode == NetmodeID.Server)
        //                            {
        //                                ChatHelper.BroadcastChatMessage(Terraria.Localization.NetworkText.FromLiteral("???????"), Color.White);
        //                            }
        //                        }
        //                    }
        //                }
        //                if (spawned)
        //                {
        //                    bait.stack--;
        //                    if (bait.stack <= 0)
        //                        bait.SetDefaults(0);
        //                }
        //            }
        //        }

        public override void PostNurseHeal(NPC nurse, int health, bool removeDebuffs, int price)
        {
            if (Player.HasEffect<GuttedHeartMinions>())
                GuttedHeartMinions.NurseHeal(Player);
        }

        public int frameCounter = 0;
        public int frameSnow = 1;
        public int frameMutantAura = 0;
        //public int frameMutantLightning = 0;

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
        {
            if (BetsyDashing || ShellHide && TurtleCounter >= 70 || GoldShell || SpectreGhostTime > 0)
            {
                foreach (var layer in PlayerDrawLayerLoader.Layers)
                {
                    //if (layer.Mod == null) //Only hide vanilla layers, optional. Does not hide modded layers added as Between or Multiple, but will hide layers marked as Before/After regardless
                    //{
                    layer.Hide();
                    //}
                }
            }
            else if (SpecialDash)
            {
                foreach (var layer in PlayerDrawLayerLoader.Layers)
                {
                    if (layer == PlayerDrawLayers.MountBack || layer == PlayerDrawLayers.MountFront)
                    {
                        layer.Hide();
                    }
                }
            }

            if (drawInfo.drawPlayer.wings == EquipLoader.GetEquipSlot(Mod, "FlightMasterySoul", EquipType.Wings))
            {
                foreach (var layer in PlayerDrawLayerLoader.Layers)
                {
                    if (layer == Terraria.DataStructures.PlayerDrawLayers.Wings)
                    {
                        //layer.Hide();
                    }
                }
            }

            //if (SquirrelMount)
            //{
            //    foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers)
            //    {
            //        layer.


            //        if (layer != PlayerLayer.MountBack && PlayerLayer != PlayerLayer.MountFront && PlayerLayer != PlayerLayer.MiscEffectsFront && PlayerLayer != PlayerLayer.MiscEffectsBack)
            //        {
            //            PlayerLayer.visible = false;
            //        }
            //    }
            //}
            if (SwordGlobalItem.BroadswordRework(drawInfo.heldItem) && drawInfo.drawPlayer.ItemAnimationActive)
            {

                Terraria.DataStructures.PlayerDrawLayers.HeldItem.Hide();
            }
        }

        public int GetHealMultiplier(int heal)
        {
            float multiplier = 1f;
            bool squire = SquireEnchantItem != null;
            bool valhalla = ValhallaEnchantActive;
            if ((squire || valhalla))
            {
                bool forceEffect = SquireEnchantItem != null && SquireEnchantItem.ModItem is BaseEnchant && (ForceEffect<SquireEnchant>() || ForceEffect<ValhallaKnightEnchant>());
                if (Eternity)
                    multiplier = 5f;
                else if (forceEffect && valhalla)
                    multiplier = 1.25f;
                else if (valhalla || (forceEffect && squire))
                    multiplier = 1.15f;
                else if (squire)
                    multiplier = 1.10f;
            }

            if (MutantPresence)
                multiplier *= 0.8f;

            heal = (int)(heal * multiplier);

            return heal;
        }
        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            healValue = GetHealMultiplier(healValue);
            if (Player.HasEffect<ShroomiteHealEffect>())
                if (item.type == ItemID.Mushroom && healValue < 75)
                    healValue = 50;

            if (Player.HasEffect<HallowEffect>())
            {
                
                if (FargowiltasSouls.DrawingTooltips)
                {
                    float mult = Player.ForceEffect<HallowEffect>() ? 1.7f : 1.4f;
                    healValue = (int)(healValue * mult);
                }
                    
                else
                    healValue = 0;
            }
        }

        public void HealPlayer(int amount)
        {
            amount = GetHealMultiplier(amount);
            Player.statLife += amount;
            if (Player.statLife > Player.statLifeMax2)
                Player.statLife = Player.statLifeMax2;
            Player.HealEffect(amount);
        }

        public override void CopyClientState(ModPlayer clientClone)
        {
            FargoSoulsPlayer modPlayer = clientClone as FargoSoulsPlayer;
            modPlayer.Toggler = Toggler;
        }

        public void SyncToggle(AccessoryEffect effect)
        {
            if (!TogglesToSync.ContainsKey(effect))
                TogglesToSync.Add(effect, Player.GetToggle(effect).ToggleBool);
        }

        public void SyncActiveSkill(int index)
        {
            if (!SkillsToSync.ContainsKey(index))
                SkillsToSync.Add(index, ActiveSkills[index]);
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket defaultPacket = Mod.GetPacket();
            defaultPacket.Write((byte)FargowiltasSouls.PacketID.SyncDefaultToggles);
            defaultPacket.Write((byte)Player.whoAmI);
            defaultPacket.Write(Toggler_ExtraAttacksDisabled);
            defaultPacket.Write(Toggler_MinionsDisabled);
            defaultPacket.Write(Toggler_ExtraJumpsDisabled);
            defaultPacket.Send(toWho, fromWho);

            foreach (KeyValuePair<AccessoryEffect, bool> toggle in TogglesToSync)
            {
                ModPacket packet = Mod.GetPacket();

                packet.Write((byte)FargowiltasSouls.PacketID.SyncOneToggle);
                packet.Write((byte)Player.whoAmI);
                packet.Write(toggle.Key.FullName);
                packet.Write(toggle.Value);

                packet.Send(toWho, fromWho);
            }

            TogglesToSync.Clear();

            foreach (KeyValuePair<int, AccessoryEffect> skill in SkillsToSync)
            {
                ModPacket packet = Mod.GetPacket();

                packet.Write((byte)FargowiltasSouls.PacketID.SyncActiveSkill);
                packet.Write((byte)Player.whoAmI);
                packet.Write(skill.Key);
                int skillIndex = skill.Value == null ? -1 : skill.Value.Index;
                packet.Write(skillIndex);

                packet.Send(toWho, fromWho);
            }

            SkillsToSync.Clear();
        }
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            FargoSoulsPlayer modPlayer = clientPlayer as FargoSoulsPlayer;
            if (modPlayer.Toggler.Toggles != Toggler.Toggles)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)FargowiltasSouls.PacketID.SyncTogglesOnJoin);
                packet.Write((byte)Player.whoAmI);
                packet.Write((byte)Toggler.Toggles.Count);

                for (int i = 0; i < Toggler.Toggles.Count; i++)
                {
                    packet.Write(Toggler.Toggles.Values.ElementAt(i).ToggleBool);
                }

                packet.Send();
            }
        }

        public void AddBuffNoStack(int buff, int duration)
        {
            if (!Player.HasBuff(buff) && ReallyAwfulDebuffCooldown <= 0)
            {
                Player.AddBuff(buff, duration);
                int d = Player.FindBuffIndex(buff);
                if (d != -1) //if debuff successfully applied
                    ReallyAwfulDebuffCooldown = Player.buffTime[d] + 240;
            }
        }

        public void TryAdditionalAttacks(int damage, DamageClass damageType)
        {
            // :)
        }

        public Rectangle GetPrecisionHurtbox()
        {
            Rectangle hurtbox = Player.Hitbox;
            hurtbox.X += hurtbox.Width / 2;
            hurtbox.Y += hurtbox.Height / 2;
            hurtbox.Width = Math.Min(hurtbox.Width, hurtbox.Height);
            hurtbox.Height = Math.Min(hurtbox.Width, hurtbox.Height);
            hurtbox.X -= hurtbox.Width / 2;
            hurtbox.Y -= hurtbox.Height / 2;
            return hurtbox;
        }
        public bool ForceEffect(ModItem modItem, bool allowPaused = false)
        {
            bool CheckForces(int type)
            {
                int force = BaseEnchant.Force[type];
                if (force <= 0)
                    return BaseEnchant.CraftsInto[type] > 0 && CheckForces(BaseEnchant.CraftsInto[type]); //check force of enchant it crafts into, recursively
                return ForceEffects.Contains(force);
            }
            bool CheckWizard(int type)
            {
                if (WizardedItem != null && !WizardedItem.IsAir && WizardedItem.type == type)
                    return true;
                return BaseEnchant.CraftsInto[type] > 0 && CheckWizard(BaseEnchant.CraftsInto[type]);
            }
            if (TerrariaSoul)
                return true;

            if (CosmosForce)
                return true;

            if ((Main.gamePaused && !allowPaused) || modItem == null || modItem.Item == null || modItem.Item.IsAir)
                return false;

            if (modItem is BaseSoul || modItem is BaseForce)
                return true;

            if (modItem is BaseEnchant)
            {
                if (CheckWizard(modItem.Item.type) || CheckForces(modItem.Item.type))
                    return true;
            }


            return false;
        }
        public bool ForceEffect<T>(bool allowPaused = false) where T : BaseEnchant => ForceEffect(ModContent.GetInstance<T>(), allowPaused);
        public bool ForceEffect(int? enchType, bool allowPaused = false)
        {
            if (enchType == null || enchType <= 0)
                return false;

            ModItem item = ModContent.GetModItem((int)enchType);
            return item != null && ForceEffect(item, allowPaused);
        }
        public override void PreSavePlayer()
        {
            SquireEnchant.ResetMountStats(this);
        }

    }
}
