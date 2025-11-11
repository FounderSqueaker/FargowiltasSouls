using FargowiltasSouls.Content.Bosses.AbomBoss;
using FargowiltasSouls.Content.Bosses.BanishedBaron;
using FargowiltasSouls.Content.Bosses.Champions.Cosmos;
using FargowiltasSouls.Content.Bosses.Champions.Earth;
using FargowiltasSouls.Content.Bosses.Champions.Life;
using FargowiltasSouls.Content.Bosses.Champions.Nature;
using FargowiltasSouls.Content.Bosses.Champions.Shadow;
using FargowiltasSouls.Content.Bosses.Champions.Spirit;
using FargowiltasSouls.Content.Bosses.Champions.Terra;
using FargowiltasSouls.Content.Bosses.Champions.Timber;
using FargowiltasSouls.Content.Bosses.Champions.Will;
using FargowiltasSouls.Content.Bosses.CursedCoffin;
using FargowiltasSouls.Content.Bosses.DeviBoss;
using FargowiltasSouls.Content.Bosses.Lifelight;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Content.Bosses.TrojanSquirrel;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Items.Materials;
using FargowiltasSouls.Content.Items.Pets;
using FargowiltasSouls.Content.Items.Placables.MusicBoxes;
using FargowiltasSouls.Content.Items.Placables.Trophies;
using FargowiltasSouls.Content.Items.Summons;
using FargowiltasSouls.Content.Items.Weapons.Challengers;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls
{
    public partial class FargowiltasSouls
    {
        /// <summary>
        /// <b>Vanilla main bosses:</b><br />
        ///  1.0 = King Slime<br />
        ///  2.0 = Eye of Cthulhu<br />
        ///  3.0 = Eater of Worlds / Brain of Cthulhu<br />
        ///  4.0 = Queen Bee<br />
        ///  5.0 = Skeletron<br />
        ///  6.0 = Deerclops<br />
        ///  7.0 = Wall of Flesh<br />
        ///  8.0 = Queen Slime<br />
        ///  9.0 = The Twins<br />
        /// 10.0 = The Destroyer<br />
        /// 11.0 = Skeletron Prime<br />
        /// 12.0 = Plantera<br />
        /// 13.0 = Golem<br />
        /// 14.0 = Duke Fishron<br />
        /// 15.0 = Empress of Light<br />
        /// 16.0 = Betsy<br />
        /// 17.0 = Lunatic Cultist<br />
        /// 18.0 = Moon Lord
        /// </summary>
        public Dictionary<string, float> BossChecklistValues = new()
        {
            // pre hm
            {"TrojanSquirrel", 0.5f},
            {"CursedCoffin", 2.1f},
            {"DeviBoss", 7f - 1e-4f},
            // hm
            {"BanishedBaron", 8.7f},
            {"Lifelight", 11.49f},
            // post ml
            {"TimberChampion", 19.1f},
            {"TerraChampion", 19.15f},
            {"EarthChampion", 19.2f},
            {"NatureChampion", 19.25f},
            {"LifeChampion", 19.3f},
            {"ShadowChampion", 19.35f},
            {"SpiritChampion", 19.4f},
            {"WillChampion", 19.45f},
            // endgame
            {"CosmosChampion", 21.2f},
            {"AbomBoss", 22.6f},
            {"MutantBoss", 25.8f}
        };
        private void BossChecklistCompatibility()
        {
            if (ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
            {
                if (MoveDeerclopsChecklistEntry)
                {
                    #region Get Types
                    Type? BossChecklist = bossChecklist.GetType(); // BossChecklist Type can be obtained via simply Mod.GetType()
                    // As Mod.Code.GetType(string name) is not implemented however, we use Mod.Code.GetTypes() and find the other ones we need
                    Type[]? TypeList = bossChecklist.Code.GetTypes();
                    Type? BossTracker = TypeList.Where<Type?>(type => type?.Name == "BossTracker")?.First();
                    Type? EntryInfo = TypeList.Where<Type?>(type => type?.Name == "EntryInfo")?.First();
                    #endregion

                    #region Get Fields
                    // Get static instance field objects to utilize as initial object references
                    var BCInstance = BossChecklist?.GetField("instance", LumUtils.UniversalBindingFlags)?.GetValue(null);
                    var trackerInstance = BossChecklist?.GetField("bossTracker", LumUtils.UniversalBindingFlags)?.GetValue(null);
                    // Get the EntryInfo List<> field and object by using the Boss Tracker instance
                    FieldInfo? SortedEntries_Field = BossTracker?.GetField("SortedEntries", LumUtils.UniversalBindingFlags);
                    var SortedEntries = SortedEntries_Field?.GetValue(trackerInstance);
                    // Get the field needed to readd the portrait texture after we replace the EntryInfo that contained it
                    FieldInfo? PortraitTexture_Field = EntryInfo?.GetField("portraitTexture", LumUtils.UniversalBindingFlags);
                    #endregion

                    #region Get Methods
                    // As there's no way to normally use a List<> of a non-public type, hack into its List<T> and just get the methods that handle indexing
                    PropertyInfo? List_EntryInfo_Property = SortedEntries?.GetType().GetProperty("Item", LumUtils.UniversalBindingFlags);
                    MethodInfo? List_EntryInfo_GetMethod = List_EntryInfo_Property?.GetGetMethod();
                    MethodInfo? List_EntryInfo_SetMethod = List_EntryInfo_Property?.GetSetMethod();

                    // This internal BossChecklist method returns the EntryInfo we need
                    MethodInfo? FindEntryFromKey_Method = BossTracker?.GetMethod("FindEntryFromKey", LumUtils.UniversalBindingFlags);

                    // Very hackily resolve GetMethod ambiguity and obtain the method we require to make a replacement for Deerclops' EntryInfo
                    MethodInfo[]? MakeVanillaBoss_MethodList = EntryInfo?.GetMethods(LumUtils.UniversalBindingFlags);
                    MethodInfo? MakeVanillaBoss_Method = MakeVanillaBoss_MethodList?.Where(m => m.Name == "MakeVanillaBoss" && m.GetParameters().Any(p => p.Name == "npcID"))?.First();
                    void MakeVanillaBoss(ref object? info, string texturePath)
                    {
                        var obj = MakeVanillaBoss_Method?.Invoke(null, [0, 4.5f, "NPCName.Deerclops", Terraria.ID.NPCID.Deerclops, () => NPC.downedDeerclops]); // Make a replacement EntryInfo
                        if (ModContent.HasAsset(texturePath))
                        {
                            PortraitTexture_Field?.SetValue(obj, ModContent.Request<Texture2D>(texturePath)); // Readd the entry's portrait texture
                        }
                        info = obj;
                    }
                    #endregion
                    // Finalize after getting everything necessary to replace Deerclops' entry
                    var DeerclopsEntry = FindEntryFromKey_Method?.Invoke(trackerInstance, ["Terraria Deerclops"]); // Get EntryInfo via FindEntryFromKey, where the key is "<ModSource> <NPCName>"
                    if (DeerclopsEntry == List_EntryInfo_GetMethod?.Invoke(SortedEntries, [6])) // Check whether the FindEntryFromKey retval matches List[] getval for the 7th entry (array 6) which contains the original Deerclops entry
                    {
                        MakeVanillaBoss(ref DeerclopsEntry, $"{bossChecklist.Name}/Resources/BossTextures/Boss{Terraria.ID.NPCID.Deerclops}"); // Tweak the matching entry's progression value
                        List_EntryInfo_SetMethod?.Invoke(SortedEntries, [6, DeerclopsEntry]); // Set the matching entry to the original List<>
                    }
                }

                static bool AllPlayersAreDead() => Main.player.All(plr => !plr.active || plr.dead);

                void Add(string type, string bossName, List<int> npcIDs, Func<bool> downed, Func<bool> available, List<int> collectibles, List<int> spawnItems, bool hasKilledAllMessage, string portrait = null)
                {
                    bossChecklist.Call(
                        $"Log{type}",
                        this,
                        bossName,
                        BossChecklistValues[bossName],
                        downed,
                        npcIDs,
                        new Dictionary<string, object>()
                        {
                            { "spawnItems", spawnItems },
                            // { "collectibles", collectibles }, // it's fetched from npc loot? TODO: refactor method calls below
                            { "availability", available },
                            { "despawnMessage", hasKilledAllMessage ? new Func<NPC, LocalizedText>(npc =>
                                        AllPlayersAreDead() ? Language.GetText($"Mods.{Name}.NPCs.{bossName}.BossChecklistIntegration.KilledAllMessage") : Language.GetText($"Mods.{Name}.NPCs.{bossName}.BossChecklistIntegration.DespawnMessage")) :
                                    Language.GetText($"Mods.{Name}.NPCs.{bossName}.BossChecklistIntegration.DespawnMessage") },
                            {
                                "customPortrait",
                                portrait == null ? null : new Action<SpriteBatch, Rectangle, Color>((spriteBatch, rect, color) =>
                                {
                                    Texture2D tex = Assets.Request<Texture2D>(portrait, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                                    Rectangle sourceRect = tex.Bounds;
                                    float scale = Math.Min(1f, (float)rect.Width / sourceRect.Width);
                                    spriteBatch.Draw(tex, rect.Center.ToVector2(), sourceRect, color, 0f, sourceRect.Size() / 2, scale, SpriteEffects.None, 0);
                                })
                            }
                        }
                    );
                }
                bool calamity = FargowiltasSouls.CalamityMod != null;
                Add("Boss",
                    "DeviBoss",
                    [ModContent.NPCType<DeviBoss>()],
                    () => WorldSavingSystem.DownedDevi,
                    () => true,
                    [
                        ModContent.ItemType<DeviMusicBox>(),
                        ModContent.ItemType<DeviatingEnergy>(),
                        ModContent.ItemType<DeviTrophy>(),
                        ModContent.ItemType<ChibiHat>(),
                        ModContent.ItemType<BrokenBlade>()
                    ],
                    [ModContent.ItemType<DevisCurse>()],
                    true
                );
                float abomValue = calamity ? 22.6f : 20f;
                Add("Boss",
                    "AbomBoss",
                    [ModContent.NPCType<AbomBoss>()],
                    () => WorldSavingSystem.DownedAbom,
                    () => true,
                    [
                        ModContent.ItemType<AbominationnP1MusicBox>(),
                        ModContent.ItemType<AbomEnergy>(),
                        ModContent.ItemType<AbomTrophy>(),
                        ModContent.ItemType<BabyScythe>(),
                        ModContent.ItemType<BrokenHilt>()
                    ],
                    [ModContent.ItemType<AbomsCurse>()],
                    true
                );
                float mutantValue = calamity ? 30 : 23;
                Add("Boss",
                    "MutantBoss",
                    [ModContent.NPCType<MutantBoss>()],
                    () => WorldSavingSystem.DownedMutant,
                    () => true,
                    [
                        ModContent.ItemType<MutantMusicBox>(),
                        ModContent.ItemType<EternalEnergy>(),
                        ModContent.ItemType<MutantTrophy>(),
                        ModContent.ItemType<SpawnSack>(),
                        ModContent.ItemType<BrokenSpearhead>()
                    ],
                    [ModContent.ItemType<AbominationnVoodooDoll>()],
                    true
                );


                #region champions

                Add("MiniBoss",
                    "TimberChampion",
                    [ModContent.NPCType<TimberChampion>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TimberChampion],
                    () => true,
                    new List<int>(BaseForce.EnchantsIn<TimberForce>()).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    [ModContent.ItemType<SigilOfChampions>()],
                    false
                );
                Add("MiniBoss",
                    "TerraChampion",
                    [ModContent.NPCType<TerraChampion>(), ModContent.NPCType<TerraChampionBody>(), ModContent.NPCType<TerraChampionTail>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TerraChampion],
                    () => true,
                    new List<int>(BaseForce.EnchantsIn<TerraForce>()).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    [ModContent.ItemType<SigilOfChampions>()],
                    false,
                    "Content/Bosses/Champions/Terra/TerraChampion_Still"
                );
                Add("MiniBoss",
                    "EarthChampion",
                    [ModContent.NPCType<EarthChampion>(), ModContent.NPCType<EarthChampionHand>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.EarthChampion],
                    () => true,
                    new List<int>(BaseForce.EnchantsIn<EarthForce>()).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    [ModContent.ItemType<SigilOfChampions>()],
                    false,
                    "Content/Bosses/Champions/Earth/EarthChampion_Still"
                );
                Add("MiniBoss",
                    "NatureChampion",
                    [ModContent.NPCType<NatureChampion>(), ModContent.NPCType<NatureChampionHead>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.NatureChampion],
                    () => true,
                    new List<int>(BaseForce.EnchantsIn<NatureForce>()).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    [ModContent.ItemType<SigilOfChampions>()],
                    false,
                    "Content/Bosses/Champions/Nature/NatureChampion_Still"
                );
                Add("MiniBoss",
                    "LifeChampion",
                    [ModContent.NPCType<LifeChampion>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.LifeChampion],
                    () => true,
                    new List<int>(BaseForce.EnchantsIn<LifeForce>()).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    [ModContent.ItemType<SigilOfChampions>()],
                    false,
                    "Content/Bosses/Champions/Life/LifeChampion_Still"
                );
                Add("MiniBoss",
                    "ShadowChampion",
                    [ModContent.NPCType<ShadowChampion>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.ShadowChampion],
                    () => true,
                    new List<int>(BaseForce.EnchantsIn<DeathForce>()).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    [ModContent.ItemType<SigilOfChampions>()],
                    false
                );
                Add("MiniBoss",
                    "SpiritChampion",
                    [ModContent.NPCType<SpiritChampion>(), ModContent.NPCType<SpiritChampionHand>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.SpiritChampion],
                    () => true,
                    new List<int>(BaseForce.EnchantsIn<SpiritForce>()).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    [ModContent.ItemType<SigilOfChampions>()],
                    false,
                    "Content/Bosses/Champions/Spirit/SpiritChampion_Still"
                );
                Add("MiniBoss",
                    "WillChampion",
                    [ModContent.NPCType<WillChampion>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.WillChampion],
                    () => true,
                    new List<int>(BaseForce.EnchantsIn<WillForce>()).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    [ModContent.ItemType<SigilOfChampions>()],
                    false
                );

                Add("Boss",
                    "CosmosChampion",
                    [ModContent.NPCType<CosmosChampion>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.CosmosChampion],
                    () => true,
                    new List<int>(BaseForce.EnchantsIn<CosmoForce>()).Append(ModContent.ItemType<ChampionMusicBox>()).ToList(),
                    [ModContent.ItemType<SigilOfChampions>()],
                    true
                );

                #endregion champions


                #region challengers

                Add("Boss",
                    "TrojanSquirrel",
                    [ModContent.NPCType<TrojanSquirrel>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TrojanSquirrel],
                    () => true,
                    [
                        ModContent.ItemType<TrojanSquirrelTrophy>(),
                        ModContent.ItemType<TreeSword>(),
                        ModContent.ItemType<MountedAcornGun>(),
                        ModContent.ItemType<SnowballStaff>(),
                        ModContent.ItemType<KamikazeSquirrelStaff>()
                    ],
                    [ModContent.ItemType<SquirrelCoatofArms>()],
                    false,
                    "Content/Bosses/TrojanSquirrel/TrojanSquirrel_Still"
                );
                Add("Boss",
                    "Lifelight",
                    [ModContent.NPCType<Lifelight>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.Lifelight],
                    () => true,
                    [
                        ModContent.ItemType<LifelightTrophy>(),
                        ModContent.ItemType<EnchantedLifeblade>(),
                        ModContent.ItemType<Lightslinger>(),
                        ModContent.ItemType<CrystallineCongregation>(),
                        ModContent.ItemType<KamikazePixieStaff>(),
                        ModContent.ItemType<LifelightMasterPet>()
                    ],
                    [ModContent.ItemType<CrystallineEffigy>()],
                    false,
                    "Assets/Textures/Content/Bosses/Lifelight/Lifelight"
                );

                Add("Boss",
                    "BanishedBaron",
                    [ModContent.NPCType<BanishedBaron>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.BanishedBaron],
                    () => true,
                    [
                        ModContent.ItemType<BaronTrophy>(),
                        ModContent.ItemType<TheBaronsTusk>(),
                        ModContent.ItemType<RoseTintedVisor>(),
                        ModContent.ItemType<NavalRustrifle>(),
                        ModContent.ItemType<DecrepitAirstrikeRemote>(),
                    ],
                    [ModContent.ItemType<MechLure>()],
                    false
                );
                if (CursedCoffin.Enabled)
                {
                    Add("Boss",
                    "CursedCoffin",
                    //TODO: ADD LOOT
                    [ModContent.NPCType<CursedCoffin>()],
                    () => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.CursedCoffin],
                    () => true,
                    [],
                    [ModContent.ItemType<CoffinSummon>()],
                    false
                //"Content/NPCs/Challengers/CursedCoffin"
                );
                }

                #endregion challengers
            }
        }
    }
}
