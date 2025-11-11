using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Core.ModSupport
{
    public class MusicDisplayCompatability : ModSystem
    {
        public override void PostSetupContent()
        {
            MusicDisplay();
        }

        public void MusicDisplay()
        {
            if (FargowiltasSouls.MusicDisplay == null)
                return;

            bool foundMod = ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod);

            if (musicMod == null)
                return;

            void AddMusic(string path, string name)
            {
                LocalizedText author = Language.GetText("Mods.FargowiltasSouls.MusicDisplay." + name + ".Author");
                LocalizedText displayName = Language.GetText("Mods.FargowiltasSouls.MusicDisplay." + name + ".DisplayName");
                FargowiltasSouls.MusicDisplay.Call("AddMusic", (short)MusicLoader.GetMusicSlot(musicMod, path), displayName, author, Mod.DisplayNameClean);
            }

            AddMusic("Assets/Music/TrojanSquirrel", "NutsAndBolts");
            AddMusic("Assets/Music/ShiftingSands", "ShiftingSands");
            AddMusic("Assets/Music/Strawberry_Sparkly_Sunrise", "SparklyStrawberrySunrise");
            AddMusic("Assets/Music/Baron", "WilloftheWaves");            
            AddMusic("Assets/Music/LieflightNoCum", "IllustriousIlluminescence");
            AddMusic("Assets/Music/PlatinumStar", "PlatinumStar");
            AddMusic("Assets/Music/Laevateinn_P1", "Laevateinn");
            //if (musicMod.Version >= Version.Parse("0.1.7.4"))
            //    AddMusic("Assets/Music/WillChampion", "FinalWave");
        }
    }
}
