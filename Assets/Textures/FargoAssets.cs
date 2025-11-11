using Luminance.Assets;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

// Base namespace for convinience
namespace FargowiltasSouls.Assets.Textures
{
    public class FargoAssets
    {
        public static string Filepath => "FargowiltasSouls/Assets/Textures/";
        public static string TilePath => Filepath + "Tiles/";

        /// <summary>
        /// Retrieves the asset string associated with a texture
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public static string GetAssetString(string path, string name) => Filepath + path + "/" + name;

        /// <summary>
        /// Shorthand for for grabbing a <see cref="Texture2D"></see>, utilizing Luminance's <see cref="LazyAsset{AssetType}"></see> loading system.
        /// When attempting to grab an <see cref="Asset{T}"></see>, use .Asset instead of .Value
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static LazyAsset<Texture2D> GetTexture2D(string path, string name, AssetRequestMode mode = AssetRequestMode.AsyncLoad) => LazyAsset<Texture2D>.Request(GetAssetString(path, name), mode);

        //TODO: Change these to use Luminance's LazyAsset loading system.

        #region Additive Textures
        public static Asset<Texture2D> BlobBloomTexture => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/BlobGlow");
        public static Asset<Texture2D> BloomTexture => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/Bloom");
        public static Asset<Texture2D> BloomParticleTexture => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/BloomParticle");
        public static Asset<Texture2D> BloomFlareTexture => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/BloomFlare");
		public static Asset<Texture2D> DeviBorderTexture => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/DeviBorder");
        public static Asset<Texture2D> HardEdgeRing => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/HardEdgeRing");
        public static Asset<Texture2D> SoftEdgeRing => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/SoftEdgeRing");
        public static Asset<Texture2D> Scorch => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/Scorch");
        public static Asset<Texture2D> Smoke => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/Smoke");

        public static Asset<Texture2D> BlurredLightTexture => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/BlurredLight");

        public static Asset<Texture2D> LightRayTexture => ModContent.Request<Texture2D>(Filepath + "AdditiveTextures/LightRay");
        #endregion

        #region Misc Shader Textures
        public static Asset<Texture2D> DeviRingTexture => ModContent.Request<Texture2D>(Filepath + "MiscShaderTextures/Ring1");
        public static Asset<Texture2D> DeviRing2Texture => ModContent.Request<Texture2D>(Filepath + "MiscShaderTextures/Ring2");
        public static Asset<Texture2D> DeviRing3Texture => ModContent.Request<Texture2D>(Filepath + "MiscShaderTextures/Ring3");
        public static Asset<Texture2D> DeviRing4Texture => ModContent.Request<Texture2D>(Filepath + "MiscShaderTextures/Ring4");
        #endregion

        #region Noise
        public static Asset<Texture2D> PerlinNoise => ModContent.Request<Texture2D>(Filepath + "Noise/BlurryPerlinNoise");
        public static Asset<Texture2D> ColorNoiseMap => ModContent.Request<Texture2D>(Filepath + "Noise/ColorNoiseMap");

        public static Asset<Texture2D> CracksNoise => ModContent.Request<Texture2D>(Filepath + "Noise/CracksNoise");
        public static Asset<Texture2D> CrustyNoise => ModContent.Request<Texture2D>(Filepath + "Noise/CrustyNoise");

        public static Asset<Texture2D> DottedNoise => ModContent.Request<Texture2D>(Filepath + "Noise/DottedNoise");
        public static Asset<Texture2D> HarshNoise => ModContent.Request<Texture2D>(Filepath + "Noise/HarshNoise");
        public static Asset<Texture2D> HoneycombNoise => ModContent.Request<Texture2D>(Filepath + "Noise/HoneycombNoise");
        public static Asset<Texture2D> LessCrustyNoise => ModContent.Request<Texture2D>(Filepath + "Noise/LessCrustyNoise");

        public static Asset<Texture2D> SandyNoise => ModContent.Request<Texture2D>(Filepath + "Noise/SandyNoise");
        public static Asset<Texture2D> SmokyNoise => ModContent.Request<Texture2D>(Filepath + "Noise/SmokyNoise");
        public static Asset<Texture2D> TurbulentNoise => ModContent.Request<Texture2D>(Filepath + "Noise/TurbulentNoise");
        public static Asset<Texture2D> WavyNoise => ModContent.Request<Texture2D>(Filepath + "Noise/WavyNoise");
        public static Asset<Texture2D> Techno1Noise => ModContent.Request<Texture2D>(Filepath + "Noise/Techno1Noise");

        #endregion

        #region Trails
        public static Asset<Texture2D> DeviBackStreak => ModContent.Request<Texture2D>(Filepath + "Trails/DevBackStreak");
        public static Asset<Texture2D> DeviInnerStreak => ModContent.Request<Texture2D>(Filepath + "Trails/DevInnerStreak");
        public static Asset<Texture2D> FadedGlowStreak => ModContent.Request<Texture2D>(Filepath + "Trails/FadedGlowStreak");
        public static Asset<Texture2D> FadedStreak => ModContent.Request<Texture2D>(Filepath + "Trails/FadedStreak");
        public static Asset<Texture2D> FadedThinGlowStreak => ModContent.Request<Texture2D>(Filepath + "Trails/FadedThinGlowStreak");
        public static Asset<Texture2D> GenericStreak => ModContent.Request<Texture2D>(Filepath + "Trails/GenericStreak");
        public static Asset<Texture2D> MagmaStreak => ModContent.Request<Texture2D>(Filepath + "Trails/MagmaStreak");
        public static Asset<Texture2D> MutantStreak => ModContent.Request<Texture2D>(Filepath + "Trails/MutantStreak");
        public static Asset<Texture2D> WillStreak => ModContent.Request<Texture2D>(Filepath + "Trails/WillStreak");
        #endregion

        /*public class Tiles
        {
            public static Asset<Texture2D> CrackedSandstoneBricks => ModContent.Request<Texture2D>(TilePath + "CrackedSandstoneBricks");
            public static Asset<Texture2D> FMMBanner => ModContent.Request<Texture2D>(TilePath + "FMMBanner");
            public static Asset<Texture2D> LifeRevitalizerPlaced => ModContent.Request<Texture2D>(TilePath + "LifeRevitalizerPlaced");
            public static Asset<Texture2D> LifeRevitalizerHighlight => ModContent.Request<Texture2D>(TilePath + "LifeRevitalizerPlaced_Highlight");
            public static Asset<Texture2D> MutantStatue => ModContent.Request<Texture2D>(TilePath + "MutantStatue");
            public static Asset<Texture2D> MutantStatueGift => ModContent.Request<Texture2D>(TilePath + "MutantStatueGift");
            public static Asset<Texture2D> MutantStatueGiftHighlight => ModContent.Request<Texture2D>(TilePath + "MutantStatueGift_Highlight");

            public class Trophies
            {
                public static Asset<Texture2D> TrojanSquirrelTrophy => ModContent.Request<Texture2D>(TilePath + "Trophies/TrojanSquirrelTrophy");
                public static Asset<Texture2D> CursedCoffinTrophy => ModContent.Request<Texture2D>(TilePath + "Trophies/CursedCoffinTrophy");
                public static Asset<Texture2D> DevianttTrophy => ModContent.Request<Texture2D>(TilePath + "Trophies/DeviTrophy");
                public static Asset<Texture2D> BaronTrophy => ModContent.Request<Texture2D>(TilePath + "Trophies/BaronTrophy");
                public static Asset<Texture2D> LifelightTrophy => ModContent.Request<Texture2D>(TilePath + "Trophies/LifelightTrophy");
                public static Asset<Texture2D> EridanusTrophy => ModContent.Request<Texture2D>(TilePath + "Trophies/EridanusTrophy");
                public static Asset<Texture2D> AbominationnTrophy => ModContent.Request<Texture2D>(TilePath + "Trophies/AbominationnTrophy");
                public static Asset<Texture2D> MutantTrophy => ModContent.Request<Texture2D>(TilePath + "Trophies/MutantTrophy");
            }

            public class Relics
            {
                public static Asset<Texture2D> TrojanSquirrelRelic => ModContent.Request<Texture2D>(TilePath + "Relics/TrojanSquirrelRelic");
                public static Asset<Texture2D> CursedCoffinRelic => ModContent.Request<Texture2D>(TilePath + "Relics/CursedCoffinRelic");
                public static Asset<Texture2D> DevianttRelic => ModContent.Request<Texture2D>(TilePath + "Relics/DeviRelic");
                public static Asset<Texture2D> BaronRelic => ModContent.Request<Texture2D>(TilePath + "Relics/BaronRelic");
                public static Asset<Texture2D> LifelightRelic => ModContent.Request<Texture2D>(TilePath + "Relics/LifelightRelic");
                public static Asset<Texture2D> TimberChampionRelic => ModContent.Request<Texture2D>(TilePath + "Relics/TimberChampionRelic");
                public static Asset<Texture2D> TerraChampionRelic => ModContent.Request<Texture2D>(TilePath + "Relics/TerraChampionRelic");
                public static Asset<Texture2D> EarthChampionRelic => ModContent.Request<Texture2D>(TilePath + "Relics/EarthChampionRelic");
                public static Asset<Texture2D> LifeChampionRelic => ModContent.Request<Texture2D>(TilePath + "Relics/LifeChampionRelic");
                public static Asset<Texture2D> DeathChampionRelic => ModContent.Request<Texture2D>(TilePath + "Relics/DeathChampionRelic");
                public static Asset<Texture2D> SpiritChampionRelic => ModContent.Request<Texture2D>(TilePath + "Relics/SpiritChampionRelic");
                public static Asset<Texture2D> WillChampionRelic => ModContent.Request<Texture2D>(TilePath + "Relics/WillChampionRelic");
                public static Asset<Texture2D> NatureChampionRelic => ModContent.Request<Texture2D>(TilePath + "Relics/NatureChampionRelic");
                public static Asset<Texture2D> EridanusRelic => ModContent.Request<Texture2D>(TilePath + "Relics/ErdanusRelic");
                public static Asset<Texture2D> AbominationnRelic => ModContent.Request<Texture2D>(TilePath + "Relics/AbomRelic");
                public static Asset<Texture2D> MutantRelic => ModContent.Request<Texture2D>(TilePath + "Relics/MutantRelic");
            }

            public class MusicBoxes
            {
                public static Asset<Texture2D> MainMenuMusicBox => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/MainMenuMusicBoxSheet");
                public static Asset<Texture2D> TrojanMusicBox => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/TrojanMusicBoxSheet");
                public static Asset<Texture2D> CoffinMusicBox => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/CoffinMusicBoxSheet");
                public static Asset<Texture2D> DeviMusicBox => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/DeviMusicBoxSheet");
                public static Asset<Texture2D> BaronMusicBoxP1 => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/BaronP1MusicBoxSheet");
                public static Asset<Texture2D> BaronMusicBoxP2 => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/BaronP2MusicBoxSheet");
                public static Asset<Texture2D> LifelightMusicBox => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/LifelightMusicBoxSheet");
                public static Asset<Texture2D> ChampionMusicBox => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/ChampionMusicBoxSheet");
                public static Asset<Texture2D> EridanusMusicBox => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/EridanusMusicBoxSheet");
                public static Asset<Texture2D> AbominationnMusicBoxP1 => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/AbominationnP1MusicBoxMusicBoxSheet");
                public static Asset<Texture2D> AbominationnMusicBoxP2 => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/AbominationnP2MusicBoxMusicBoxSheet");
                public static Asset<Texture2D> MutantMusicBox => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/MutantMusicBoxSheet");
                public static Asset<Texture2D> RePrologueMusicBox => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/RePrologueMusicBoxSheet");
                public static Asset<Texture2D> StoriaMusicBox => ModContent.Request<Texture2D>(TilePath + "MusicBoxes/StoriaMusicBoxSheet");

            }

        }*/
        
        //TODO: Change this to use Luminance's LazyAsset loading system.
        public class UI
        {   
            public static Asset<Texture2D> CooldownBarTexture => ModContent.Request<Texture2D>(Filepath + "UI/CooldownBar", AssetRequestMode.ImmediateLoad);
            public static Asset<Texture2D> CooldownBarFillTexture => ModContent.Request<Texture2D>(Filepath + "UI/CooldownBarFill", AssetRequestMode.ImmediateLoad);
            public static Asset<Texture2D> OncomingMutantnt => ModContent.Request<Texture2D>(Filepath + "UI/OncomingMutantnt", AssetRequestMode.ImmediateLoad);
            public static Asset<Texture2D> OncomingMutantTexture => ModContent.Request<Texture2D>(Filepath + "UI/OncomingMutant", AssetRequestMode.ImmediateLoad);
            public static Asset<Texture2D> OncomingMutantAura => ModContent.Request<Texture2D>(Filepath + "UI/OncomingMutantAura", AssetRequestMode.ImmediateLoad);

            public static Asset<Texture2D> EnchantSlotIcon => ModContent.Request<Texture2D>(Filepath + "UI/EnchantSlotIcon", AssetRequestMode.ImmediateLoad);

            public class ActiveSkillMenu
            {
                public static LazyAsset<Texture2D> ActiveSkillMenuButton => LazyAsset<Texture2D>.Request(Filepath + "UI/ActiveSkillMenuButton");
                public static LazyAsset<Texture2D> ActiveSkillMenuButtonHover => LazyAsset<Texture2D>.Request(Filepath + "UI/ActiveSkillMenuButtonHover");
            }

            public class MainMenu
            {
                public static Asset<Texture2D> MenuLogo => ModContent.Request<Texture2D>(Filepath + "UI/MenuLogo", AssetRequestMode.ImmediateLoad);
                public static Asset<Texture2D> MenuLogoGlow => ModContent.Request<Texture2D>(Filepath + "UI/MenuLogo_Glow", AssetRequestMode.ImmediateLoad);
                public static Asset<Texture2D> ForgorMenuLogo => ModContent.Request<Texture2D>(Filepath + "UI/ForgorMenuLogo", AssetRequestMode.ImmediateLoad);
                public static Asset<Texture2D> ForgorMenuLogoGlow => ModContent.Request<Texture2D>(Filepath + "UI/ForgorMenuLogo_Glow", AssetRequestMode.ImmediateLoad);
                public static Asset<Texture2D> TitleLinkButtons => ModContent.Request<Texture2D>(Filepath + "UI/TitleLinkButtons", AssetRequestMode.ImmediateLoad);
                public static Asset<Texture2D> MutantWorldBorder => ModContent.Request<Texture2D>(Filepath + "UI/MutantWorldBorder", AssetRequestMode.ImmediateLoad);
            }
        }
    }
}
