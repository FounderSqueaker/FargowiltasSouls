using FargowiltasSouls.Content.Bosses.BanishedBaron;
using FargowiltasSouls.Content.Bosses.CursedCoffin;
using FargowiltasSouls.Content.Bosses.Magmaw;
using FargowiltasSouls.Core.Systems;
using System;
using System.Drawing;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Core
{
#pragma warning disable IDE0057
    internal static class ModSceneUtils
    {
        /// <summary>
        /// Checks whether this boss exists in the near vicinity by using the SceneEffect class name to search its ModNPC type and iterate over the NPC array to find a match.
        /// SceneEffect class names are important and match the original boss file. Do not change them without modifying this to work alongside.
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        internal static bool TryFindBoss(object Type)
        {
            int? type = GetModNPC(Type)?.Type;
            NPC? nPC = SearchForNPC(type);
            if (nPC != null)
                return true;

            return false;
        }
        /// <summary>
        /// Returns the ModNPC of this SceneEffect class by intricately cutting out the boss name to use for searching.
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        internal static ModNPC? GetModNPC(object Type)
        {
            string? name = Type.ToString();
            name = name?.Remove(name.Length - 11).Remove(0, 22); // Grab the boss name intricately by cutting parts behind and at front
            ModNPC? modNPC = ModLoader.TryGetMod("FargowiltasSouls", out Mod mod) ? mod.Find<ModNPC>(name) : null; // juuuuuuuuuuuuuust in case
            return modNPC;
        }
        /// <summary>
        /// Searches for the given NPC type in the near vicinity and returns the NPC instance.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static NPC? SearchForNPC(int? type = -1)
        {
            float maxDistance = 99999f;
            NPC? closestNPC = null;
            foreach (NPC npc in Main.npc.Where(n => n != null && n.active && n.type == type))
            {
                if (Nearby(npc) && npc.Distance(Main.LocalPlayer.Center) < maxDistance)
                {
                    maxDistance = npc.Distance(Main.LocalPlayer.Center);
                    closestNPC = npc;
                }
            }
            return closestNPC;
        }
        private static bool Nearby(NPC npc)
        {
            int range = 5500;
            Rectangle value = new((int)(npc.position.X + (float)(npc.width / 2)) - range, (int)(npc.position.Y + (float)(npc.height / 2)) - range, range * 2, range * 2);
            Rectangle rectangle = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            return rectangle.IntersectsWith(value);
        }
    }
#pragma warning restore IDE0057

    internal class TrojanSquirrelSceneEffect : ModSceneEffect
    {
        public override int Music => ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
                ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/TrojanSquirrel") : MusicID.OtherworldlyBoss1;

        public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.490f;
    }

    internal class CursedCoffinSceneEffect : ModSceneEffect
    {
        public override int Music
        {
            get
            {
                int music = MusicID.OtherworldlyBoss1;
                if (ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod) && musicMod.Version >= Version.Parse("0.1.6"))
                {
                    int? type = ModSceneUtils.GetModNPC(this)?.Type;
                    NPC? nPC = ModSceneUtils.SearchForNPC(type);
                    var Phase = typeof(CursedCoffin).GetField("Phase", LumUtils.UniversalBindingFlags)?.GetValue(nPC?.ModNPC) as int?;
                    if (Phase == 0)
                    {
                        music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Silent");
                    }
                    else if (Phase >= 1)
                    {
                        music = MusicLoader.GetMusicSlot(musicMod, "Assets/Music/ShiftingSands");
                        if (Main.musicFade[music] < 0.5f)
                            Main.musicFade[music] = 0.5f;
                    }
                }
                return music;
            }
        }

        public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.510f;
    }
    internal class DeviBossSceneEffect : ModSceneEffect
    {
        public override int Music => ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
                ? MusicLoader.GetMusicSlot(musicMod, (musicMod.Version >= Version.Parse("0.1.4")) ? "Assets/Music/Strawberry_Sparkly_Sunrise" : "Assets/Music/LexusCyanixs") : MusicID.OtherworldlyHallow;
        public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.530f;
    }
    internal class BanishedBaronSceneEffect : ModSceneEffect
    {
        public override int Music
        {
            get
            {
                int? type = ModSceneUtils.GetModNPC(this)?.Type;
                NPC? nPC = ModSceneUtils.SearchForNPC(type);

                bool Phase2 = typeof(BanishedBaron).GetField("Phase", LumUtils.UniversalBindingFlags)?.GetValue(nPC?.ModNPC) as int? > 1;
                int music = Phase2 ? MusicID.Boss2 : MusicID.DukeFishron;

                if (ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod))
                {
                    music = Phase2 ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Baron2") : MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Baron");
                }
                return music;
            }
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.540f;
    }
    internal class LifelightSceneEffect : ModSceneEffect
    {
        public override int Music => ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
                ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/LieflightNoCum") : MusicID.OtherworldlyBoss1;
        public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.560f;
    }
    internal class MagmawSceneEffect : ModSceneEffect
    {
        public override bool IsLoadingEnabled(Mod mod) => Magmaw.LoadThis;
        public override int Music => MusicID.Boss2; //ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod) ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Baron") : MusicID.Boss2;
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.520f;
    }
    abstract class ChampionSceneEffect : ModSceneEffect
    {
        public override int Music => ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
                ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Champions") : MusicID.OtherworldlyBoss1;
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
    }
    internal class EarthChampionSceneEffect : ChampionSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.510f;
    }
    internal class LifeChampionSceneEffect : ChampionSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.511f;
    }
    internal class NatureChampionSceneEffect : ChampionSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.512f;
    }
    internal class ShadowChampionSceneEffect : ChampionSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.513f;
    }
    internal class SpiritChampionSceneEffect : ChampionSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.514f;
    }
    internal class TerraChampionSceneEffect : ChampionSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.515f;
    }
    internal class TimberChampionSceneEffect : ChampionSceneEffect
    {        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.516f;
    }
    internal class TimberChampionHeadSceneEffect : ChampionSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.516f;
    }
    internal class WillChampionSceneEffect : ChampionSceneEffect
    {
        //public override int Music => ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
        //        ? MusicLoader.GetMusicSlot(musicMod, (musicMod.Version >= Version.Parse("0.1.7.4")) ? "Assets/Music/WillChampion" : "Assets/Music/Champions") : MusicID.OtherworldlyBoss1;
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.517f;
    }
    internal class CosmosChampionSceneEffect : ModSceneEffect
    {
        public override int Music
        {
            get
            {
                int music = MusicID.OtherworldlyLunarBoss;
                if (ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod))
                {
                    int? type = ModSceneUtils.GetModNPC(this)?.Type;
                    NPC? nPC = ModSceneUtils.SearchForNPC(type);
                    bool Started = !(nPC?.localAI[3] == 0 && nPC?.ai[1] < 117); // Not in spawn animation
                    music = Started ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/PlatinumStar") : MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Silent");

                    if (nPC?.localAI[3] == 0 && nPC?.ai[1] > 117) // Only the small bit after spawn animation
                    {
                        Main.musicFade[Main.curMusic] += 0.2f;
                    }
                }
                return music;
            }
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.528f;
    }
    internal class AbomBossSceneEffect : ModSceneEffect
    {
        public override int Music
        {
            get
            {
                int music = MusicID.OtherworldlyPlantera;
                if (ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod))
                {
                    int? type = ModSceneUtils.GetModNPC(this)?.Type;
                    NPC? nPC = ModSceneUtils.SearchForNPC(type);
                    bool Phase2 = nPC?.localAI[3] == 2;

                    if (FargoSoulsUtil.AprilFools && musicMod.Version >= Version.Parse("0.1.5.1"))
                        music = Phase2 ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Gigachad") : MusicLoader.GetMusicSlot(musicMod, "Assets/Music/TomMorello");
                    else if (musicMod.Version >= Version.Parse("0.1.5"))
                        music = Phase2 ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Laevateinn_P2") : MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Laevateinn_P1");
                    else
                        music = MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Stigma");
                }
                return music;
            }
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.540f;
    }
    internal class MutantBossSceneEffect : ModSceneEffect
    {
        public override int Music
        {
            get
            {
                int music = MusicID.OtherworldlyTowers;
                if (ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod))
                {
                    int? type = ModSceneUtils.GetModNPC(this)?.Type;
                    NPC? nPC = ModSceneUtils.SearchForNPC(type);
                    var AttackChoice = nPC?.ai[0];

                    if (WorldSavingSystem.MasochistModeReal && musicMod.Version >= Version.Parse("0.1.1.3") && AttackChoice < -1) // Masochist+ Desparation
                    {
                        music = MusicLoader.GetMusicSlot(musicMod, "Assets/Music/StoriaShort");
                    }
                    else if (WorldSavingSystem.EternityMode && (AttackChoice >= 10 || AttackChoice < 0)) // EMode+ Phase 2/Masochist Phase 3 Transition
                    {
                        bool storia = WorldSavingSystem.MasochistModeReal && musicMod.Version >= Version.Parse("0.1.1");
                        music = storia ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/Storia") : MusicLoader.GetMusicSlot(musicMod, "Assets/Music/rePrologue");
                    }
                    else // Phase 1 and non-EMode Phase 2
                        music = MusicLoader.GetMusicSlot(musicMod, WorldSavingSystem.MasochistModeReal ? "Assets/Music/rePrologue" : "Assets/Music/SteelRed");
                }
                return music;
            }
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
        public override bool IsSceneEffectActive(Player player) => ModSceneUtils.TryFindBoss(this);
        public override float GetWeight(Player player) => 0.550f;
    }
}
