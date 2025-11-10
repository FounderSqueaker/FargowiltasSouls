using Fargowiltas.Common.Systems;
using FargowiltasSouls.Content.Items.Accessories;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Souls;
using FargowiltasSouls.Content.Items.Armor.Eternal;
using FargowiltasSouls.Content.Items.Consumables;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Core.Toggler
{
    public class SoulToggleBackend : ToggleBackend
    {
        public readonly static string ConfigPath = Path.Combine(Main.SavePath, "ModConfigs", "FargowiltasSouls_Toggles.json");
        public Preferences Config;

        public Dictionary<AccessoryEffect, Toggle> Toggles = [];
        public bool CanPlayMaso = true;

        public const int CustomPresetCount = 3;
        public List<AccessoryEffect>[] CustomPresets = new List<AccessoryEffect>[CustomPresetCount];

        public bool Initialized;

        //not doing it in player.initialize because multiplayer clones players which makes new togglers which tries config load which has high overhead (lag)
        public override void TryLoad()
        {
            if (Initialized)
                return;

            Initialized = true;

            //Main.NewText("OOBA");
            Config = new Preferences(ConfigPath);

            Toggles = ToggleLoader.LoadedToggles;
            //TogglerPosition = new Point(0, 0);

            if (!Main.dedServ)
            {
                if (!Config.Load())
                    Save();
            }

            //Dictionary<string, int> togglerPositionUnpack = Config.Get("TogglerPosition", new Dictionary<string, int>() { { "X", Main.screenWidth / 2 - 300 }, { "Y", Main.screenHeight / 2 - 200 } });
            //TogglerPosition = new Point(togglerPositionUnpack["X"], togglerPositionUnpack["Y"]);

            //if (!Main.dedServ)
            //    FargoUIManager.SoulToggler.SetPositionToPoint(TogglerPosition);

            CanPlayMaso = true; // Config.Get("CanPlayMaso", true);

            //TODO: figure out how to extract a plain list from json, only using Dict rn because i know it can be loaded from json
            for (int i = 0; i < CustomPresets.Length; i++)
            {
                var toggleUnpack = Config.Get<Dictionary<string, bool>>($"CustomPresetsOff{i + 1}", null);
                if (toggleUnpack != null)
                {
                    List<AccessoryEffect> disabledEffects = [];
                    foreach (AccessoryEffect effect in ToggleLoader.LoadedToggles.Keys.ToList())
                    {
                        if (toggleUnpack.ContainsKey(effect.Name))
                            disabledEffects.Add(effect);
                    }
                    CustomPresets[i] = disabledEffects;
                }
            }
        }

        public override void Save()
        {
            if (!Initialized)
                return;

            if (!Main.dedServ)
            {
                //Config.Put("CanPlayMaso", CanPlayMaso);

                //Config.Put(TogglesByPlayer, ParsePackedToggles());

                //TogglerPosition = FargoUIManager.SoulToggler.GetPositionAsPoint();
                //Config.Put("TogglerPosition", UnpackPosition());

                for (int i = 0; i < CustomPresets.Length; i++)
                {
                    if (CustomPresets[i] == null)
                        continue;

                    Dictionary<string, bool> togglesOff = new(CustomPresets.Length);
                    foreach (AccessoryEffect toggle in CustomPresets[i])
                        togglesOff[toggle.Name] = false;
                    Config.Put($"CustomPresetsOff{i + 1}", togglesOff);
                }

                Config.Save();
            }
        }

        public override void LoadPlayerToggles(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (!Initialized)
                return;

            Toggles = ToggleLoader.LoadedToggles;
            SetAll(true);

            foreach (AccessoryEffect entry in modPlayer.disabledToggles)
                Main.LocalPlayer.SetToggleValue(entry, false);

            foreach (KeyValuePair<AccessoryEffect, Toggle> entry in Toggles)
                modPlayer.TogglesToSync[entry.Key] = entry.Value.ToggleBool;
        }

        /*public Dictionary<string, int> UnpackPosition() => new Dictionary<string, int>() {
            { "X", TogglerPosition.X },
            { "Y", TogglerPosition.Y }
        };*/

        public override void SetAll(bool value)
        {
            foreach (Toggle toggle in Toggles.Values)
            {
                Main.LocalPlayer.SetToggleValue(toggle.Effect, value);
            }
        }

        public override void SomeEffects()
        {
            Player player = Main.LocalPlayer;

            SetAll(true);

            player.SetToggleValue<BorealEffect>(false);
            player.SetToggleValue<ShadewoodEffect>(false);

            player.SetToggleValue<CobaltEffect>(false);
            player.SetToggleValue<AncientCobaltEffect>(false);
            player.SetToggleValue<ObsidianProcEffect>(false);
            player.SetToggleValue<CopperEffect>(false);
            player.SetToggleValue<AshWoodFireballs>(false);

            player.SetToggleValue<GladiatorSpears>(false);
            player.SetToggleValue<RedRidingEffect>(false);

            player.SetToggleValue<BeeEffect>(false);
            player.SetToggleValue<CactusEffect>(false);
            player.SetToggleValue<PumpkinEffect>(false);
            //player.SetToggleValue<ChloroMinion>(false);
            player.SetToggleValue<RainUmbrellaEffect>(false);
            player.SetToggleValue<RainInnerTubeEffect>(false);
            //player.SetToggleValue<JungleJumpEffect>(false);
            player.SetToggleValue<MoltenEffect>(false);
            player.SetToggleValue<ShroomiteShroomEffect>(false);
            player.SetToggleValue<DarkArtistMinion>(false);
            player.SetToggleValue<NecroEffect>(false);
            player.SetToggleValue<ShadowBalls>(false);
            player.SetToggleValue<SpookyEffect>(false);
            player.SetToggleValue<FossilBones>(false);
            player.SetToggleValue<FossilBones>(false);
            player.SetToggleValue<AncientHallowMinion>(false);
            player.SetToggleValue<SpectreEffect>(false);
            player.SetToggleValue<MeteorEffect>(false);
            player.SetToggleValue<ZephyrJump>(false);
            player.SetToggleValue<DevianttHearts>(false);
            player.SetToggleValue<MasoAeolusFlower>(false);
            player.SetToggleValue<SlimyShieldEffect>(false);
            player.SetToggleValue<AgitatingLensEffect>(false);
            player.SetToggleValue<SkeleMinionEffect>(false);
            player.SetToggleValue<MasoCarrotEffect>(false);
            player.SetToggleValue<NymphPerfumeEffect>(false);
            player.SetToggleValue<WretchedPouchEffect>(false);
            player.SetToggleValue<ProbeMinionEffect>(false);
            player.SetToggleValue<GelicWingSpikes>(false);
            //player.SetToggleValue<PungentEyeballCursor>(false);
            player.SetToggleValue<PungentMinion>(false);
            player.SetToggleValue<DeerclawpsEffect>(false);
            //player.SetToggleValue<CultistMinionEffect>(false);
            player.SetToggleValue<LihzahrdBoulders>(false);
            player.SetToggleValue<CelestialRuneAttacks>(false);

            player.SetToggleValue<UfoMinionEffect>(false);

            player.SetToggleValue<MasoAbom>(false);

            player.SetToggleValue<MasoRing>(false);

            player.SetToggleValue<BuilderEffect>(false);

            player.SetToggleValue<SupersonicClimbing>(false);

            player.SetToggleValue<TrawlerFogEffect>(false);


            foreach (Toggle toggle in Toggles.Values.Where(toggle => toggle.Effect.Name.Contains("Pet")))
            {
                player.SetToggleValue(toggle.Effect, false);
            }
        }

        public override void MinimalEffects()
        {
            Player player = Main.LocalPlayer;

            SetAll(false);

            player.SetToggleValue<MythrilEffect>(true);
            player.SetToggleValue<PalladiumEffect>(true);
            player.SetToggleValue<IronEffect>(true);
            player.SetToggleValue<CthulhuShield>(true);
            //player.SetToggleValue<Tin>(true);
            player.SetToggleValue<BeetleEffect>(true);
            player.SetToggleValue<SpiderEffect>(true);
            player.SetToggleValue<GoldToPiggy>(true);
            player.SetToggleValue<JungleDashEffect>(true);
            player.SetToggleValue<SupersonicTabi>(true);
            player.SetToggleValue<ValhallaDashEffect>(true);
            player.SetToggleValue<SquireMountJump>(true);
            player.SetToggleValue<SquireMountSpeed>(true);
            player.SetToggleValue<NebulaEffect>(true);
            //player.SetToggleValue<SolarEffect>(true);
            player.SetToggleValue<HuntressEffect>(true);
            player.SetToggleValue<CrystalAssassinDash>(true);
            player.SetToggleValue<GladiatorBanner>(true);
            player.SetToggleValue<ChalicePotionEffect>(true);

            player.SetToggleValue<EternityTin>(true);

            player.SetToggleValue<DeerSinewEffect>(true);
            player.SetToggleValue<MasoGraze>(true);
            player.SetToggleValue<SinisterIconDropsEffect>(true);
            player.SetToggleValue<RainbowHealEffect>(true);
            player.SetToggleValue<StabilizedGravity>(true);
            player.SetToggleValue<PrecisionSealHurtbox>(true);

            player.SetToggleValue<AgitatingLensInstall>(true);
            player.SetToggleValue<FusedLensInstall>(true);

            player.SetToggleValue<MiningHunt>(true);
            player.SetToggleValue<MiningDanger>(true);
            player.SetToggleValue<MiningSpelunk>(true);
            player.SetToggleValue<MiningShine>(true);
            player.SetToggleValue<RunSpeed>(true);
            player.SetToggleValue<SupersonicRocketBoots>(true);
            player.SetToggleValue<NoMomentum>(true);
            player.SetToggleValue<MeteorMomentumEffect>(true);
            player.SetToggleValue<FlightMasteryInsignia>(true);
            player.SetToggleValue<FlightMasteryGravity>(true);
            player.SetToggleValue<PaladinShieldEffect>(true);
            player.SetToggleValue<ShimmerImmunityEffect>(true);
            player.SetToggleValue<MasoAeolusFrog>(true);
            player.SetToggleValue<TimsConcoctionEffect>(true);

        }

        public override void SaveCustomPreset(int slot)
        {
            var togglesOff = new List<AccessoryEffect>();
            foreach (KeyValuePair<AccessoryEffect, Toggle> entry in Toggles)
            {
                if (!Toggles[entry.Key].ToggleBool)
                    togglesOff.Add(entry.Key);
            }

            if (!Main.dedServ)
            {
                CustomPresets[slot - 1] = togglesOff;
                //Save(); 
                Main.NewText(Language.GetTextValue("Mods.Fargowiltas.UI.SavedToSlot", slot), Color.Yellow);
            }
        }

        public override void LoadCustomPreset(int slot)
        {
            List<AccessoryEffect> togglesOff = CustomPresets[slot - 1];
            if (togglesOff == null)
            {
                Main.NewText(Language.GetTextValue("Mods.Fargowiltas.UI.NoTogglesFound", slot), Color.Yellow);
                return;
            }

            FargoSoulsPlayer modPlayer = Main.LocalPlayer.FargoSouls();
            modPlayer.disabledToggles = new List<AccessoryEffect>(togglesOff);

            LoadPlayerToggles(Main.LocalPlayer);
            modPlayer.disabledToggles.Clear();
        }
    }
}
