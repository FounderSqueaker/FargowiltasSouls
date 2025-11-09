using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Audio;

namespace FargowiltasSouls.Assets.Sounds
{
    public static class FargosSoundRegistry
    {
        public const string SoundsPath = "FargowiltasSouls/Assets/Sounds/";

        public const string WeaponsPath = SoundsPath + "Weapons/";
        public const string AccessoriesPath = SoundsPath + "Accessories/";

        public static readonly SoundStyle ThrowShort = new(SoundsPath + "ThrowShort");
        public static readonly SoundStyle ReticleBeep = new(SoundsPath + "ReticleBeep");
        public static readonly SoundStyle ReticleLockOn = new(SoundsPath + "ReticleLockOn");
        public static readonly SoundStyle Thunder = new(SoundsPath + "Thunder");
        public static readonly SoundStyle Zombie104 = new(SoundsPath + "Zombie_104");
        public static readonly SoundStyle GenericDeathray = new(SoundsPath + "GenericDeathray");
        public static readonly SoundStyle NekomiCharge = new(SoundsPath + "NekomiCharge");
        public static readonly SoundStyle StyxCharge = new(SoundsPath + "StyxCharge");

        // revives
        public static readonly SoundStyle FossilRevive = new(AccessoriesPath + "FossilRevive");
        public static readonly SoundStyle SpectreRevive = new(AccessoriesPath + "SpectreRevive");
        public static readonly SoundStyle StyxRevive = new(AccessoriesPath + "StyxRevive");

        // accessories
        public static readonly SoundStyle ZaWarudo = new(AccessoriesPath + "ZaWarudo");
        public static readonly SoundStyle ChargeSound = new SoundStyle(AccessoriesPath + "ChargeSound") with { Volume = 0.5f };

        // weapons
        public static readonly SoundStyle DiffractorStart = new(WeaponsPath + "DiffractorStart");
        public static readonly SoundStyle DiffractorLoop = new(WeaponsPath + "DiffractorLoop");
        public static readonly SoundStyle DiffractorEnd = new(WeaponsPath + "DiffractorEnd");
        public static readonly SoundStyle NukeFishronFire = new(WeaponsPath + "NukeFishronFire");
        public static readonly SoundStyle NukeFishronExplosion = new(WeaponsPath + "NukeFishronExplosion");
        public static readonly SoundStyle GeminiSpaz = new(WeaponsPath + "GeminiSpaz");
        public static readonly SoundStyle GeminiReti = new(WeaponsPath + "GeminiReti");
        public static readonly SoundStyle LeashSpin = new(WeaponsPath + "LeashSpin");
        public static readonly SoundStyle LeashBreak = new(WeaponsPath + "LeashBreak");

        // Trojan
        public static readonly SoundStyle TrojanHeadDeath = new (SoundsPath + "Challengers/Trojan/TrojanHeadDeath");
        public static readonly SoundStyle TrojanCannonDeath = new (SoundsPath + "Challengers/Trojan/TrojanCannonDeath");
        public static readonly SoundStyle TrojanLegsDeath = new(SoundsPath + "Challengers/Trojan/TrojanLegsDeath");
        public static readonly SoundStyle TrojanDeath = new (SoundsPath + "Challengers/Trojan/TrojanDeath");
        public static readonly SoundStyle TrojanGunStartup = new (SoundsPath + "Challengers/Trojan/TrojanGunStartup");
        public static readonly SoundStyle TrojanSnowball = new (SoundsPath + "Challengers/Trojan/TrojanSnowball");
        public static readonly SoundStyle TrojanSnowballSplat = new SoundStyle(SoundsPath + "Challengers/Trojan/SnowballSplat") with { Variants = [1, 2, 3] };
        public static readonly SoundStyle TrojanCannon = new(SoundsPath + "Challengers/Trojan/TrojanCannon");
        public static readonly SoundStyle Minigun = new(SoundsPath + "Challengers/Trojan/Minigun");
        public static readonly SoundStyle TrojanHookLoop = new SoundStyle(SoundsPath + "Challengers/Trojan/TrojanHookLoop") with { Volume = 0.8f};
        public static readonly SoundStyle TrojanHookTelegraph = new(SoundsPath + "Challengers/Trojan/TrojanHookTelegraph");
        public static readonly SoundStyle TrojanSummon = new(SoundsPath + "Challengers/Trojan/TrojanSummon");


        // Baron
        public static readonly SoundStyle BaronLaserTelegraph = new(SoundsPath + "Challengers/Baron/BaronLaserTelegraph");
        public static readonly SoundStyle BaronLaserSoundSlow = new(SoundsPath + "Challengers/Baron/BaronLaserSound_Slow");
        public static readonly SoundStyle BaronHit = new(SoundsPath + "Challengers/Baron/BaronHit");
        public static readonly SoundStyle BaronRoar = new(SoundsPath + "Challengers/Baron/BaronRoar");
        public static readonly SoundStyle BaronYell = new(SoundsPath + "Challengers/Baron/BaronYell");
        public static readonly SoundStyle BaronAmbience = new(SoundsPath + "Challengers/Baron/BaronAmbience");
        public static readonly SoundStyle BaronThrusterLoop = new(SoundsPath + "Challengers/Baron/BaronThrusterLoop");
        public static readonly SoundStyle BaronPropellerEject = new(SoundsPath + "Challengers/Baron/BaronPropellerEject");
        public static readonly SoundStyle NukeBeep = new(SoundsPath + "Challengers/Baron/NukeBeep");
        public static readonly SoundStyle BaronNukeShoot = new(SoundsPath + "Challengers/Baron/NukeShoot");
        public static readonly SoundStyle BaronNukeExplosion = new(SoundsPath + "Challengers/Baron/NukeExplosion");
        public static readonly SoundStyle BaronDashP1 = new(SoundsPath + "Challengers/Baron/BaronP1Dash");
        public static readonly SoundStyle BaronDashP2 = new(SoundsPath + "Challengers/Baron/BaronP2Dash");

        // Lifelight
        public static readonly SoundStyle LifelightDash = new(SoundsPath + "Challengers/Lifelight/LifelightDash");
        public static readonly SoundStyle LifelightDeathray = new(SoundsPath + "Challengers/Lifelight/LifelightDeathray") ;
        public static readonly SoundStyle LifelightDeathrayShort = new(SoundsPath + "Challengers/Lifelight/LifelightDeathrayShort");
        public static readonly SoundStyle LifelightPixieDash = new(SoundsPath + "Challengers/Lifelight/LifelightPixieDash");
        public static readonly SoundStyle LifelightRuneSound = new(SoundsPath + "Challengers/Lifelight/LifelightRuneSound");
        public static readonly SoundStyle LifelightScreech1 = new(SoundsPath + "Challengers/Lifelight/LifelightScreech1");
        public static readonly SoundStyle LifelightShotPrep = new(SoundsPath + "Challengers/Lifelight/LifelightShotPrep");

        // Coffin
        public static readonly SoundStyle CoffinBigShot = new(SoundsPath + "Challengers/Coffin/CoffinBigShot");
        public static readonly SoundStyle CoffinHandCharge = new(SoundsPath + "Challengers/Coffin/CoffinHandCharge");
        public static readonly SoundStyle CoffinPhaseTransition = new (SoundsPath + "Challengers/Coffin/CoffinPhaseTransition");
        public static readonly SoundStyle CoffinShot = new(SoundsPath + "Challengers/Coffin/CoffinShot");
        public static readonly SoundStyle CoffinSlam = new(SoundsPath + "Challengers/Coffin/CoffinSlam");
        public static readonly SoundStyle CoffinSoulShot = new(SoundsPath + "Challengers/Coffin/CoffinSoulShot");
        public static readonly SoundStyle CoffinSpiritDrone = new(SoundsPath + "Challengers/Coffin/CoffinSpiritDrone");

        // Deviantt
        public static readonly SoundStyle DeviSwing = new(SoundsPath + "Siblings/Deviantt/DeviSwing");
        public static readonly SoundStyle DeviHeartExplosion = new(SoundsPath + "Siblings/Deviantt/DeviHeartExplosion");
        public static readonly SoundStyle DeviTeleport = new(SoundsPath + "Siblings/Deviantt/DeviTeleport");
        public static readonly SoundStyle DeviDeathray = new(SoundsPath + "Siblings/Deviantt/DeviDeathray");
        public static readonly SoundStyle DeviAxeImpact = new(SoundsPath + "Siblings/Deviantt/DeviAxeImpact");
        public static readonly SoundStyle DeviMimicSmall = new(SoundsPath + "Siblings/Deviantt/DeviMimicSmall");
        public static readonly SoundStyle DeviMimicBig = new(SoundsPath + "Siblings/Deviantt/DeviMimicBig");
        public static readonly SoundStyle DeviPaladinTeleport = new(SoundsPath + "Siblings/Deviantt/DeviPaladinTeleport");
        public static readonly SoundStyle DeviPaladinThrow = new(SoundsPath + "Siblings/Deviantt/DeviPaladinThrow");
        public static readonly SoundStyle DeviWyvernOrb = new(SoundsPath + "Siblings/Deviantt/DeviWyvernOrb");
        public static readonly SoundStyle DeviWyvernOrbImpact = new(SoundsPath + "Siblings/Deviantt/DeviWyvernOrbImpact");
        public static readonly SoundStyle DeviIceOrb = new(SoundsPath + "Siblings/Deviantt/DeviIceOrb");
        public static readonly SoundStyle DeviHeartCast = new(SoundsPath + "Siblings/Deviantt/DeviHeartCast");

        public static readonly SoundStyle DeviFloorImpact = new(SoundsPath + "Siblings/Deviantt/DeviFloorImpact");
        public static readonly SoundStyle DeviFallLoop = new(SoundsPath + "Siblings/Deviantt/DeviFallLoop");



        // Mutant
        public static readonly SoundStyle MutantUnpredictive = new(SoundsPath + "Siblings/Mutant/MutantUnpredictive");
        public static readonly SoundStyle MutantPredictive = new(SoundsPath + "Siblings/Mutant/MutantPredictive");
        public static readonly SoundStyle PenetratorThrow = new(SoundsPath + "Siblings/Mutant/PenetratorThrow");
        public static readonly SoundStyle PenetratorExplosion = new(SoundsPath + "Siblings/Mutant/PenetratorExplosion");
        public static readonly SoundStyle MutantSword = new(SoundsPath + "Siblings/Mutant/MutantSword");

        public static readonly SoundStyle MutantLand = new(SoundsPath + "Siblings/Mutant/MutantLand");
        public static readonly SoundStyle MutantKSKill = new(SoundsPath + "Siblings/Mutant/MutantKSKill");

        // Abominationn
        public static readonly SoundStyle StyxGazer = new(SoundsPath + "Siblings/Abominationn/StyxGazer");
        public static readonly SoundStyle AbomScytheSwing = new(SoundsPath + "Siblings/Abominationn/ScytheSwing");
        public static readonly SoundStyle AbomScytheSplit = new(SoundsPath + "Siblings/Abominationn/ScytheSplit");
        public static readonly SoundStyle AbomSpawnSound = new(SoundsPath + "Siblings/Abominationn/SpawnSound");

        // Mechs

        public static readonly SoundStyle TwinsWarning = new(SoundsPath + "VanillaEternity/Mechs/TwinsWarning");
        public static readonly SoundStyle DestroyerScan = new(SoundsPath + "VanillaEternity/Mechs/DestroyerScan");
        public static readonly SoundStyle ElectricOrbHum = new(SoundsPath + "VanillaEternity/Mechs/ElectricOrbHum");
        public static readonly SoundStyle ElectricOrbShot = new(SoundsPath + "VanillaEternity/Mechs/ElectricOrbShot");
        public static readonly SoundStyle TwinsDeathray = new(SoundsPath + "VanillaEternity/Mechs/TwinsDeathray");

        // WoF
        public static readonly SoundStyle WoFSuck = new(SoundsPath + "VanillaEternity/WallofFlesh/WoFSuck");
        public static readonly SoundStyle WoFScreech = new(SoundsPath + "VanillaEternity/WallofFlesh/WoFScreech");
        public static readonly SoundStyle WoFGrowl = new(SoundsPath + "VanillaEternity/WallofFlesh/WoFGrowl");
        public static readonly SoundStyle WoFDeathrayTelegraph = new(SoundsPath + "VanillaEternity/WallofFlesh/WoFDeathrayTelegraph");
    }
}
