using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using System.Collections.Generic;
using System.Security.Cryptography.Pkcs;
using Terraria;
using static FargowiltasSouls.Core.Systems.DashManager;

namespace FargowiltasSouls.Core.ModPlayers
{
    public partial class FargoSoulsPlayer
    {
        public Item QueenStingerItem;
        public bool EridanusSet;
        public bool EridanusEmpower;
        public int EridanusTimer;
        public bool GaiaSet;
        public bool GaiaOffense;
        public bool StyxSet;
        public int StyxMeter;
        public int StyxTimer;
        public int StyxAttackReadyTimer;
        public bool NekomiSet;
        public int NekomiMeter;
        public int NekomiTimer;
        public int NekomiHitCD;
        public int NekomiAttackReadyTimer;
        public const int SuperAttackMaxWindow = 30;

        public bool SupersonicDodge;

        //        //minions
        public bool BrainMinion;
        public bool EaterMinion;
        public bool PixieMinion;
        public bool BigBrainMinion;
        public bool DukeFishron;

        //mount
        public bool SquirrelMount;

        //pet
        public bool SeekerOfAncientTreasures;
        public bool AccursedSarcophagus;
        public bool BabySilhouette;
        public bool BabyLifelight;
        public bool BiteSizeBaron;
        public bool Nibble;
        public bool ChibiDevi;
        public bool MutantSpawn;
        public bool BabyAbom;
        public bool PetsActive;

        #region enchantments

        // new forces
        public int TerraProcCD;
        public float CosmosMoonTimer;
        public int CosmosMoonCycle;
        public bool CosmosForce;
        public bool LifeForceActive;
        public float AuraSizeBonus;
        public int TerrariaSoulProcCD;
        public int AncientShadowCD;
        public int AncientShadowCounter;

        public int TimeSinceHurt;

        //force of timber
        public bool MahoganyCanUseDR;
        public int MahoganyTimer;
        public float EbonwoodCharge;
        public float ShadewoodCharge;
        public bool WoodEnchantDiscount;
        public int PearlwoodCritDuration;
        public int PearlwoodManaCD;
        //force of terra
        public int CopperProcCD;
        public bool GuardRaised;
        public int ParryDebuffImmuneTime;
        public int ObsidianCD;
        public bool LavaWet;
        public float TinCritMax = 25;
        public float TinCrit = 0;
        public int TinProcCD;
        public bool TinCritBuffered;
        public int TungstenCD;
        public int AshwoodCD;
        public int IronReductionDuration;

        //force of cosmos
        public float NebulaEnchCD = 3 * 60;
        public float SolarEnchCharge = 0;
        public float MeteorCD = 60;

        public int ApprenticeItemCD;
        public int CactusProcCD;
        public bool ChlorophyteEnchantActive = false;
        public bool MonkEnchantActive = false;
        public bool ShinobiEnchantActive = false;
        public int monkTimer = 0;

        public int PumpkinSpawnCD;
        public int ShroomiteCD;

        public bool TitaniumDRBuff;
        public bool TitaniumCD;
        public Item SquireEnchantItem;
        public bool ValhallaEnchantActive = false;

        public int ShadowOrbRespawnTimer;
        public float NinjaCounter;
        public int NinjaDecrementCD;
        public static int NinjaDecrementMaxCD = 20;

        public Item PlatinumEffect;
        public int PalladCounter;
        public float MythrilTimer;
        public int MythrilDelay;
        public int MythrilSoundCooldown;
        public int MythrilMaxTime => Player.HasEffect<MythrilEffect>() ? Player.ForceEffect<MythrilEffect>() ? 300 : 180 : 180;
        public float MythrilMaxSpeedBonus => Player.HasEffect<MythrilEffect>() ? Player.ForceEffect<MythrilEffect>() ? 1.75f : 1.5f : 1.5f;

        public bool GalacticMinionsDeactivated = false;
        public bool GalacticMinionsDeactivatedBuffer = false; // Needed to make sure the item effect is applied during the entirety of the update cycle, so it doesn't miss anything

        public int DeactivatedMinionEffectCount = 0;

        public int CrimsonRegenAmount;
        public int CrimsonRegenTime;

        public List<int> ForbiddenTornados = [];
        public List<int> ShadowOrbs = [];
        public int IcicleCount;
        public int icicleCD;
        public int GladiatorCD;
        public int GladiatorStandardCD;
        public int IceQueenCrownCD;
        public bool GoldEnchMoveCoins;
        public bool GoldShell;
        private int goldHP;
        public int HallowHealTime;
        public float HallowHealTotal;
        public int HuntressStage;
        public int HuntressCD;
        public int HuntressMissCD;
        public double AdamantiteSpread;
        public bool HeldItemAdamantiteValid;
        public Item AdamantiteItem;

        public bool CanCobaltJump;
        public bool JustCobaltJumped;
        public int CobaltCooldownTimer;
        public bool CobaltEnchantActive = false;
        public bool ApprenticeEnchantActive;
        public bool DarkArtistEnchantActive;
        public int BeeCD;
        public int ChloroTimer;
        public int BeetleAttackCD;
        public int Beetles;
        public float BeetleCharge;
        public int BeetleHitCD;
        public int SpookyCD;
        public int BorealCD;
        public int PalmWoodForceCD;
        public bool CrystalEnchantActive = false;
        public int CrystalDashFirstStrikeCD;
        public int CoyoteTime;

        public int MonkDashing;

        public int NecroCD;
        public Projectile CrystalSmokeBombProj = null;
        public bool FirstStrike;
        public int SmokeBombCD;
        public bool CrystalAssassinDiagonal;

        public int ShadowDashTimer;
        public int IFrameDashTimer;
        public bool IFrameDash;
        public int EarthTimer;
        public int EarthAdamantiteCharge;

        //public int RainCD;

        public int RedRidingArrowCD;

        public int DashCD;

        public bool SnowVisual;
        public int SpectreCD;
        public int SpectreGhostTime;
        public int ForbiddenCD;
        public bool MinionCrits;
        public int SpiderCD;
        //public bool squireReduceIframes;
        public bool FreezeTime;
        public int freezeLength;
        public bool ChillSnowstorm;
        public int chillLength;
        public int CHILL_DURATION => Player.HasEffect<FrostEffect>() ? 60 * 20 : 60 * 15;

        public int HallowRepelTime;
        public int TurtleCounter;
        public float TurtleShellHP = TurtleEffect.TurtleShellMaxHP;
        public bool TurtleShellBroken;
        public bool ShellHide;
        public int ValhallaVerticalDashing;
        public int VortexCD;
        public bool WizardEnchantActive;
        public bool WizardTooltips;
        public Item WizardedItem;

        public int CritterAttackTimer;

        public bool MiningImmunity;

        public HashSet<int> ForceEffects = [];

        #endregion

        //        //soul effects
        public bool MeleeSoul;
        public bool MagicSoul;
        public bool RangedSoul;
        public bool SummonSoul;
        public bool ColossusSoul;
        public bool SupersonicSoul;
        public bool WorldShaperSoul;
        public bool FlightMasterySoul;
        public int FlightSoulWingFrameX;
        public bool BuilderMode;
        public bool DimensionSoul;
        public bool UniverseSoul;
        public bool UniverseSoulBuffer;  // Needed to make sure the item effect is applied during the entirety of the update cycle, so it doesn't miss anything
        public bool UniverseCore;
        public bool FishSoul1;
        public bool FishSoul2;
        public bool TerrariaSoul;
        public bool VoidSoul;
        public int HealTimer;
        public int HurtTimer;
        public bool Eternity;
        public float TinEternityDamage;

        public int MinionSlotsNonstack;
        public int SentrySlotsNonstack;

        //maso items
        public Item SlimyShieldItem;
        public bool SlimyShieldFalling;
        public int FallthroughCD;
        public int AgitatingLensCD;
        public bool BerserkedFromAgitation = false;
        public Item RottingHeartItem;
        public int RottingHeartCD;
        public int GuttedHeartCD = 60; //should prevent spawning despite disabled toggle when loading into world
        public Item NecromanticBrewItem;
        public float NecromanticBrewRotation;
        public int IsDashingTimer;
        public bool DeerSinewNerf;
        public int DeerSinewFreezeCD;
        public bool PureHeart;
        public bool PungentEyeballMinion;
        public bool CrystalSkullMinion;
        public int WyvernBallsCD;
        public bool FusedLens;
        public bool FusedLensCursed;
        public bool FusedLensIchor;
        public bool DubiousCircuitry;
        public bool Supercharged;
        public bool TwinsInstall;
        public int RemoteCD;
        public bool SuperInstall;
        public bool Probes;
        public bool MagicalBulb;
        public bool PlanterasChild;
        public bool CrystalSkull;
        public bool PungentEyeball;
        public Item LihzahrdTreasureBoxItem;
        public int GroundPound;
        public Item BetsysHeartItem;
        public bool BetsyDashing;
        public int SpecialDashCD;
        public bool MutantAntibodies;
        public Item GravityGlobeEXItem;
        public int AdditionalAttacksTimer;
        public bool MoonChalice;
        public bool LunarCultist;
        public bool TrueEyes;
        public Item AbomWandItem;
        public int AbomWandCD;
        public bool MasochistSoul;
        public Item MasochistSoulItem;
        public bool MasochistHeart;
        public bool SandsofTime;
        public bool SecurityWallet;
        public int FrigidGemstoneCD;
        public float WretchedPouchCD;
        public bool NymphsPerfume;
        public bool NymphsPerfumeRespawn;
        public int NymphsPerfumeRestoreLife;
        public int NymphsPerfumeCD = 30;
        public bool RainbowSlime;
        public bool SkeletronArms;
        public bool CirnoGraze;
        public bool MiniSaucer;
        public bool SupremeDeathbringerFairy;
        public bool GodEaterImbue;
        public Item MutantSetBonusItem;
        public bool AbomMinion;
        public bool PhantasmalRing;
        public bool RabiesVaccine;
        public bool TwinsEX;
        public bool TimsConcoction;
        public bool TimsInspect;
        public int TimsInspectCD;
        public bool ReceivedMasoGift;
        public bool DeviGraze;
        public bool Graze;
        public float GrazeRadius;
        public int DeviGrazeCounter;
        public int CirnoGrazeCounter;
        public double DeviGrazeBonus;
        public Item DevianttHeartItem;
        public int DevianttHeartsCD;
        public Item MutantEyeItem;
        public bool MutantEyeVisual;
        public int MutantEyeCD;
        public bool AbominableWandRevived;
        public bool AbomRebirth;
        public bool WasHurtBySomething;
        public bool PrecisionSeal;
        public bool PrecisionSealNoDashNoJump;
        public Item GelicWingsItem;
        public int GelicCD;

        // consumable perma
        public bool MutantsDiscountCard;
        public bool MutantsCreditCard;
        public bool MutantsPactSlot;
        public bool DeerSinew;
        public bool OrdinaryCarrot;
        public bool ConcentratedRainbowMatter;

        // buffs
        public bool Ambrosia;
        public bool SpecialDash;

        //debuffs
        public bool Hexed;
        public int HexedInflictor = -1;
        public bool Unstable;
        private int unstableCD;
        public bool Fused;
        public int FusedStandStillTime;
        public bool Shadowflame;
        public bool Daybroken;
        public bool Oiled;
        public bool DeathMarked;
        public bool noDodge;
        public bool NoMomentum;
        public bool Bloodthirsty;
        public bool Unlucky;
        public bool DisruptedFocus;
        public bool BaronsBurden;
        public bool BleedingOut;

        public bool Smite;
        public bool Anticoagulation;
        public bool IvyVenom;
        public bool GodEater;               //defense removed, endurance removed, colossal DOT
        public bool FlamesoftheUniverse;    //activates various vanilla debuffs
        public int StatLifePrevious = -1;   //used for mutantNibble
        public bool Asocial;                //disables minions, disables pets
        public bool WasAsocial;
        public bool HidePetToggle0 = true;
        public bool HidePetToggle1 = true;
        public bool Defenseless;            //-30 defense, no damage reduction, cross necklace and knockback prevention effects disabled
        public bool Infested;               //weak DOT that grows exponentially stronger
        public int MaxInfestTime;
        public bool FirstInfection = true;
        public float InfestedDust;
        public bool Rotting;                //inflicts DOT and almost every stat reduced
        public bool SqueakyToy;             //all attacks do one damage and make squeaky noises
        public bool Atrophied;              //melee speed and damage reduced. maybe Player cannot fire melee projectiles?
        public bool Jammed;                 //ranged damage and speed reduced, all non-custom ammo set to baseline ammos
        public bool Slimed;
        public byte lightningRodTimer;
        public bool ReverseManaFlow;
        public bool CurseoftheMoon;
        public bool OceanicMaul;
        public int MaxLifeReduction;
        public int CurrentLifeReduction;
        public int LifeReductionUpdateTimer;
        public bool Midas;
        public bool HadMutantPresence;
        public bool MutantPresence;
        public bool MutantPresenceBuffer;
        public bool MutantDesperation;
        public int PresenceTogglerTimer;
        public bool MutantFang;
        public bool Swarming;
        public bool LowGround;
        public int FallthroughTimer;
        public bool Illuminated;
        public bool Mash;
        public bool GrabDamage;
        public bool[] MashPressed = new bool[4];
        public float MashCounter;
        public float FramesSinceLastMash;
        public int StealingCooldown;
        //public bool LihzahrdBlessing;
        public bool Berserked;
        public bool Stunned;
        public bool HaveCheckedAttackSpeed;
        public bool HasJungleRose;
        public bool Quicksanding;


        public int ReallyAwfulDebuffCooldown;

        public Item SquirrelCharm;
        public bool OxygenTank;

        public int DreadShellVulnerabilityTimer;
        public bool LanternGuarding;
        public int shieldTimer;
        public int shieldCD;
        public int shieldHeldTime;
        public bool wasHoldingShield;
        public int LightslingerHitShots;
        public int ChargeSoundDelay = 0;

        public int NoUsingItems;

        public bool HasDash;
        private DashType fargoDash;
        public DashType FargoDash
        {
            get => fargoDash;
            set
            {
                fargoDash = value;
                if (value != DashType.None)
                    HasDash = true;
            }
        }
        public bool CanShinobiTeleport;

        public int WeaponUseTimer;

    }
}
