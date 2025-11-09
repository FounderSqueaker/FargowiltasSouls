global using FargowiltasSouls.Core.ModPlayers;
global using FargowiltasSouls.Core.Toggler;
global using Luminance.Common.Utilities;
global using LumUtils = Luminance.Common.Utilities.Utilities;
using Fargowiltas;
using Fargowiltas.Content.Items.Tiles;
using Fargowiltas.Content.NPCs;
using Fargowiltas.Content.Projectiles;
using Fargowiltas.Content.UI;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Bosses.CursedCoffin;
using FargowiltasSouls.Content.Bosses.VanillaEternity;
using FargowiltasSouls.Content.Buffs;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Consumables;
using FargowiltasSouls.Content.Items.Dyes;
using FargowiltasSouls.Content.Items.Misc;
using FargowiltasSouls.Content.NPCs;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.Accessories;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.BossMinions;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.CustomEnemies.Desert;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Jungle;
using FargowiltasSouls.Content.Patreon.Volknet;
using FargowiltasSouls.Content.Projectiles.Eternity.Environment;
using FargowiltasSouls.Content.Projectiles.Weapons.ChallengerItems;
using FargowiltasSouls.Content.Sky;
using FargowiltasSouls.Content.Tiles;
using FargowiltasSouls.Content.UI;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.ModCalls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.GameContent.Creative.CreativePowers;


namespace FargowiltasSouls
{
    public partial class FargowiltasSouls : Mod
    {
        public static Mod MutantMod;
        public static Mod BOBW;
        public static Mod CalamityMod;
        public static Mod MusicDisplay;


        //internal static ModKeybind FreezeKey;
        //internal static ModKeybind GoldKey;
        //internal static ModKeybind SmokeBombKey;
        //internal static ModKeybind SpecialDashKey;
        //internal static ModKeybind BombKey;
        internal static ModKeybind PrecisionSealKey;
        //internal static ModKeybind MagicalBulbKey;
        //internal static ModKeybind FrigidSpellKey;
        //internal static ModKeybind DebuffInstallKey;
        //internal static ModKeybind AmmoCycleKey;

        internal static ModKeybind ActiveSkill1Key;
        internal static ModKeybind ActiveSkill2Key;
        internal static ModKeybind ActiveSkill3Key;
        internal static ModKeybind ActiveSkill4Key; // Unused
        internal static ModKeybind ActiveSkillMenuKey;
        internal static ModKeybind SoulToggleKey;

        public static List<TitleLinkButton> fargoTitleLinks = new List<TitleLinkButton>();
        internal static ModKeybind[] ActiveSkillKeys { get => [ActiveSkill1Key, ActiveSkill2Key, ActiveSkill3Key, ActiveSkill4Key]; }


        public static List<int> DebuffIDs;
        public static List<int> DefenseReducingDebuffs;

        internal static FargowiltasSouls Instance;

        internal bool LoadedNewSprites;

        internal static float OldMusicFade;

        public UserInterface CustomResources;

        private NPC DeerclopsAICurrentlyRunning;

        internal static Dictionary<int, int> ModProjDict = [];

        public static bool DrawingTooltips = false;

        private static bool MoveDeerclopsChecklistEntry = true;

        internal struct TextureBuffer
        {
            public static readonly Dictionary<int, Asset<Texture2D>> NPC = [];
            public static readonly Dictionary<int, Asset<Texture2D>> NPCHeadBoss = [];
            public static readonly Dictionary<int, Asset<Texture2D>> Gore = [];
            public static readonly Dictionary<int, Asset<Texture2D>> Golem = [];
            public static readonly Dictionary<int, Asset<Texture2D>> Dest = [];
            public static readonly Dictionary<int, Asset<Texture2D>> GlowMask = [];
            public static readonly Dictionary<int, Asset<Texture2D>> Extra = [];
            public static readonly Dictionary<int, Asset<Texture2D>> Projectile = [];
            public static Asset<Texture2D> Ninja = null;
            public static Asset<Texture2D> Probe = null;
            public static Asset<Texture2D> BoneArm = null;
            public static Asset<Texture2D> BoneArm2 = null;
            public static Asset<Texture2D> BoneLaser = null;
            public static Asset<Texture2D> BoneEyes = null;
            public static Asset<Texture2D> Chain12 = null;
            public static Asset<Texture2D> Chain26 = null;
            public static Asset<Texture2D> Chain27 = null;
            public static Asset<Texture2D> Wof = null;
            public static Asset<Texture2D> EyeLaser = null;
        }

        public override void Load()
        {
            Instance = this;
            ModLoader.TryGetMod("Fargowiltas", out MutantMod);
            ModLoader.TryGetMod("FargoSeeds", out BOBW);
            ModLoader.TryGetMod("CalamityMod", out CalamityMod);
            ModLoader.TryGetMod("MusicDisplay", out MusicDisplay);

            List<TitleLinkButton> titleLinks = fargoTitleLinks;
            titleLinks.Add(MakeSimpleButton("TitleLinks.Discord", "https://discord.gg/fargo", 0));
            titleLinks.Add(MakeSimpleButton("TitleLinks.Wiki", "https://fargosmods.wiki.gg", 1));
            titleLinks.Add(MakeSimpleButton("Mods.FargowiltasSouls.UI.TitleLinks.Patreon", "https://www.patreon.com/c/fargoteam", 2));
            titleLinks.Add(MakeSimpleButton("Mods.FargowiltasSouls.UI.TitleLinks.Github", "https://github.com/Fargowilta", 3));

            SkyManager.Instance["FargowiltasSouls:EridanusSky"] = new EridanusSky();

            SkyManager.Instance["FargowiltasSouls:AbomBoss1"] = new AbomSkyP1();
            SkyManager.Instance["FargowiltasSouls:AbomBoss"] = new AbomSky();

            SkyManager.Instance["FargowiltasSouls:MutantBoss1"] = new MutantSkyP1();
            SkyManager.Instance["FargowiltasSouls:MutantBoss"] = new MutantSky();
            SkyManager.Instance["FargowiltasSouls:MutantBoss2"] = new MutantSky2();

            SkyManager.Instance["FargowiltasSouls:MoonLordSky"] = new MoonLordSky();

            SkyManager.Instance["FargowiltasSouls:CultistSky"] = new CultistSky();

            //FreezeKey = KeybindLoader.RegisterKeybind(this, "Freeze", "P");
            //GoldKey = KeybindLoader.RegisterKeybind(this, "Gold", "O");
            //SmokeBombKey = KeybindLoader.RegisterKeybind(this, "SmokeBomb", "I");
            //SpecialDashKey = KeybindLoader.RegisterKeybind(this, "SpecialDash", "C");
            //BombKey = KeybindLoader.RegisterKeybind(this, "Bomb", "Z");
            PrecisionSealKey = KeybindLoader.RegisterKeybind(this, "PrecisionSeal", "LeftShift");
            //MagicalBulbKey = KeybindLoader.RegisterKeybind(this, "MagicalBulb", "N");
            //FrigidSpellKey = KeybindLoader.RegisterKeybind(this, "FrigidSpell", "U");
            //DebuffInstallKey = KeybindLoader.RegisterKeybind(this, "DebuffInstall", "Y");
            //AmmoCycleKey = KeybindLoader.RegisterKeybind(this, "AmmoCycle", "L");

            ActiveSkill1Key = KeybindLoader.RegisterKeybind(this, "ActiveSkill1", "Z");
            ActiveSkill2Key = KeybindLoader.RegisterKeybind(this, "ActiveSkill2", "X");
            ActiveSkill3Key = KeybindLoader.RegisterKeybind(this, "ActiveSkill3", "C");
            //ActiveSkill4Key = KeybindLoader.RegisterKeybind(this, "ActiveSkill4", "V");
            ActiveSkillMenuKey = KeybindLoader.RegisterKeybind(this, "ActiveSkillMenu", "N");
            SoulToggleKey = KeybindLoader.RegisterKeybind(this, "EffectToggle", ".");

            ToggleLoader.Load();
            FargoUIManager.LoadUI();

            if (Main.netMode != NetmodeID.Server)
            {
                #region shaders

                //loading refs for shaders
                Ref<Effect> lcRef = new(Assets.Request<Effect>("Assets/Effects/Armor/LifeChampionShader", AssetRequestMode.ImmediateLoad).Value);
                Ref<Effect> wcRef = new(Assets.Request<Effect>("Assets/Effects/Armor/WillChampionShader", AssetRequestMode.ImmediateLoad).Value);
                Ref<Effect> gaiaRef = new(Assets.Request<Effect>("Assets/Effects/Armor/GaiaShader", AssetRequestMode.ImmediateLoad).Value);
                //Ref<Effect> textRef = new(Assets.Request<Effect>("Assets/Effects/TextShader", AssetRequestMode.ImmediateLoad).Value);
                //Ref<Effect> invertRef = new(Assets.Request<Effect>("Assets/Effects/Invert", AssetRequestMode.ImmediateLoad).Value);
                //Ref<Effect> finalSparkRef = new(Assets.Request<Effect>("Assets/Effects/FinalSpark", AssetRequestMode.ImmediateLoad).Value);
                //Ref<Effect> mutantDeathrayRef = new(Assets.Request<Effect>("Assets/Effects/PrimitiveShaders/MutantFinalDeathrayShader", AssetRequestMode.ImmediateLoad).Value);
                //Ref<Effect> willDeathrayRef = new(Assets.Request<Effect>("Assets/Effects/PrimitiveShaders/WillDeathrayShader", AssetRequestMode.ImmediateLoad).Value);
                //Ref<Effect> willBigDeathrayRef = new(Assets.Request<Effect>("Assets/Effects/PrimitiveShaders/WillBigDeathrayShader", AssetRequestMode.ImmediateLoad).Value);
                //Ref<Effect> deviBigDeathrayRef = new(Assets.Request<Effect>("Assets/Effects/PrimitiveShaders/DeviTouhouDeathrayShader", AssetRequestMode.ImmediateLoad).Value);
                //Ref<Effect> deviRingRef = new(Assets.Request<Effect>("Assets/Effects/PrimitiveShaders/DeviRingShader", AssetRequestMode.ImmediateLoad).Value);
                //Ref<Effect> genericDeathrayRef = new(Assets.Request<Effect>("Assets/Effects/PrimitiveShaders/GenericDeathrayShader", AssetRequestMode.ImmediateLoad).Value);
                //Ref<Effect> blobTrailRef = new(Assets.Request<Effect>("Assets/Effects/PrimitiveShaders/BlobTrailShader", AssetRequestMode.ImmediateLoad).Value);

                //loading shaders from refs
                GameShaders.Misc["LCWingShader"] = new MiscShaderData(lcRef, "LCWings");
                GameShaders.Armor.BindShader(ModContent.ItemType<LifeDye>(), new ArmorShaderData(lcRef, "LCArmor").UseColor(new Color(1f, 0.647f, 0.839f)).UseSecondaryColor(Color.Goldenrod));

                GameShaders.Misc["WCWingShader"] = new MiscShaderData(wcRef, "WCWings");
                GameShaders.Armor.BindShader(ModContent.ItemType<WillDye>(), new ArmorShaderData(wcRef, "WCArmor").UseColor(Color.DarkOrchid).UseSecondaryColor(Color.LightPink).UseImage("Images/Misc/noise"));

                GameShaders.Misc["GaiaShader"] = new MiscShaderData(gaiaRef, "GaiaGlow");
                GameShaders.Armor.BindShader(ModContent.ItemType<GaiaDye>(), new ArmorShaderData(gaiaRef, "GaiaArmor").UseColor(new Color(0.44f, 1, 0.09f)).UseSecondaryColor(new Color(0.5f, 1f, 0.9f)));

                GameShaders.Misc["FargowiltasSouls:QueenSlime"] = new MiscShaderData(Main.PixelShaderRef, "QueenSlime");
                GameShaders.Misc["FargowiltasSouls:QueenSlime"].UseImage1(Assets.Request<Texture2D>("Assets/Textures/EModeResprites/QueenSlimePalettes/EternityQueenSlime"));
                GameShaders.Misc["FargowiltasSouls:QueenSlime"].UseImage2("Images/Extra_" + 179);
                
                GameShaders.Misc["FargowiltasSouls:KingSlime"] = new MiscShaderData(Main.PixelShaderRef, "QueenSlime");
                GameShaders.Misc["FargowiltasSouls:KingSlime"].UseImage1(Assets.Request<Texture2D>("Assets/Textures/EModeResprites/QueenSlimePalettes/EternityKingSlime"));
                GameShaders.Misc["FargowiltasSouls:KingSlime"].UseImage2("Images/Extra_" + 179);

                //GameShaders.Misc["PulseUpwards"] = new MiscShaderData(textRef, "PulseUpwards");
                //GameShaders.Misc["PulseDiagonal"] = new MiscShaderData(textRef, "PulseDiagonal");
                //GameShaders.Misc["PulseCircle"] = new MiscShaderData(textRef, "PulseCircle");
                //GameShaders.Misc["FargowiltasSouls:MutantDeathray"] = new MiscShaderData(mutantDeathrayRef, "TrailPass");
                //GameShaders.Misc["FargowiltasSouls:WillDeathray"] = new MiscShaderData(willDeathrayRef, "TrailPass");
                //GameShaders.Misc["FargowiltasSouls:WillBigDeathray"] = new MiscShaderData(willBigDeathrayRef, "TrailPass");
                //GameShaders.Misc["FargowiltasSouls:DeviBigDeathray"] = new MiscShaderData(deviBigDeathrayRef, "TrailPass");
                //GameShaders.Misc["FargowiltasSouls:DeviRing"] = new MiscShaderData(deviRingRef, "TrailPass");
                //GameShaders.Misc["FargowiltasSouls:GenericDeathray"] = new MiscShaderData(genericDeathrayRef, "TrailPass");
                //GameShaders.Misc["FargowiltasSouls:BlobTrail"] = new MiscShaderData(blobTrailRef, "TrailPass");

                //Filters.Scene["FargowiltasSouls:FinalSpark"] = new Filter(new FinalSparkShader(finalSparkRef, "FinalSpark"), EffectPriority.High);
                //Filters.Scene["FargowiltasSouls:Invert"] = new Filter(new TimeStopShader(invertRef, "Main"), EffectPriority.VeryHigh);

                Filters.Scene["FargowiltasSouls:Solar"] = new Filter(Filters.Scene["MonolithSolar"].GetShader(), EffectPriority.Medium);
                Filters.Scene["FargowiltasSouls:Vortex"] = new Filter(Filters.Scene["MonolithVortex"].GetShader(), EffectPriority.Medium);
                Filters.Scene["FargowiltasSouls:Nebula"] = new Filter(Filters.Scene["MonolithNebula"].GetShader(), EffectPriority.Medium);
                Filters.Scene["FargowiltasSouls:Stardust"] = new Filter(Filters.Scene["MonolithStardust"].GetShader(), EffectPriority.Medium);

                //Filters.Scene["Shockwave"] = new Filter(new ScreenShaderData(shockwaveRef, "Shockwave"), EffectPriority.VeryHigh);
                //Filters.Scene["Shockwave"].Load();

                #endregion shaders
            }

            //            PatreonMiscMethods.Load(this);

            LoadDetours();
        }

        public override void Unload()
        {
            //NPC.LunarShieldPowerMax = NPC.downedMoonlord ? 50 : 100;

            static void RestoreSprites(Dictionary<int, Asset<Texture2D>> buffer, Asset<Texture2D>[] original)
            {
                foreach (KeyValuePair<int, Asset<Texture2D>> pair in buffer)
                    original[pair.Key] = pair.Value;

                buffer.Clear();
            }

            RestoreSprites(TextureBuffer.NPC, TextureAssets.Npc);
            RestoreSprites(TextureBuffer.NPCHeadBoss, TextureAssets.NpcHeadBoss);
            RestoreSprites(TextureBuffer.Gore, TextureAssets.Gore);
            RestoreSprites(TextureBuffer.Golem, TextureAssets.Golem);
            RestoreSprites(TextureBuffer.Dest, TextureAssets.Dest);
            RestoreSprites(TextureBuffer.GlowMask, TextureAssets.GlowMask);
            RestoreSprites(TextureBuffer.Extra, TextureAssets.Extra);
            RestoreSprites(TextureBuffer.Projectile, TextureAssets.Projectile);

            if (TextureBuffer.Ninja != null)
                TextureAssets.Ninja = TextureBuffer.Ninja;
            if (TextureBuffer.Probe != null)
                TextureAssets.Probe = TextureBuffer.Probe;
            if (TextureBuffer.BoneArm != null)
                TextureAssets.BoneArm = TextureBuffer.BoneArm;
            if (TextureBuffer.BoneArm2 != null)
                TextureAssets.BoneArm2 = TextureBuffer.BoneArm2;
            if (TextureBuffer.BoneLaser != null)
                TextureAssets.BoneLaser = TextureBuffer.BoneLaser;
            if (TextureBuffer.BoneEyes != null)
                TextureAssets.BoneEyes = TextureBuffer.BoneEyes;
            if (TextureBuffer.Chain12 != null)
                TextureAssets.Chain12 = TextureBuffer.Chain12;
            if (TextureBuffer.Chain26 != null)
                TextureAssets.Chain26 = TextureBuffer.Chain26;
            if (TextureBuffer.Chain27 != null)
                TextureAssets.Chain27 = TextureBuffer.Chain27;
            if (TextureBuffer.Wof != null)
                TextureAssets.Wof = TextureBuffer.Wof;

            ToggleLoader.Unload();

            //FreezeKey = null;
            //GoldKey = null;
            //SmokeBombKey = null;
            //SpecialDashKey = null;
            //BombKey = null;
            SoulToggleKey = null;
            PrecisionSealKey = null;
            //MagicalBulbKey = null;
            //FrigidSpellKey = null;
            //DebuffInstallKey = null;
            //AmmoCycleKey = null;

            ActiveSkill1Key = null;
            ActiveSkill2Key = null;
            ActiveSkill3Key = null;
            //ActiveSkill4Key = null;
            ActiveSkillMenuKey = null;

            DebuffIDs?.Clear();
            DefenseReducingDebuffs?.Clear();

            ModProjDict.Clear();

            Instance = null;

            UnloadDetours();
        }

        public override object Call(params object[] args) => ModCallManager.ProcessAllModCalls(this, args); // Our mod calls can be found in ModCalls.cs.

        public static void DropDevianttsGift(Player player)
        {
            Item.NewItem(null, player.Center, ItemID.SilverPickaxe);
            Item.NewItem(null, player.Center, ItemID.SilverAxe);
            Item.NewItem(null, player.Center, ItemID.SilverHammer);

            Item.NewItem(null, player.Center, ItemID.WaterCandle);

            Item.NewItem(null, player.Center, ItemID.Torch, 200);
            Item.NewItem(null, player.Center, ItemID.LesserHealingPotion, 15);
            Item.NewItem(null, player.Center, ItemID.RecallPotion, 15);
            if (Main.netMode != NetmodeID.SinglePlayer)
                Item.NewItem(null, player.Center, ItemID.WormholePotion, 15);

            //Item.NewItem(null, player.Center, ModContent.ItemType<DevianttsSundial>());
            Item.NewItem(null, player.Center, ModContent.ItemType<EternityAdvisor>());
            Item.NewItem(null, player.Center, ModContent.ItemType<EnchantedTree>());

            void GiveItem(string modName, string itemName, int amount = 1)
            {
                if (ModContent.TryFind(modName, itemName, out ModItem modItem))
                    Item.NewItem(null, player.Center, modItem.Type, amount);
            }

            GiveItem("Fargowiltas", "AutoHouse", 2);
            GiveItem("Fargowiltas", "MiniInstaBridge", 2);

            Item.NewItem(null, player.Center, ModContent.ItemType<EurusSock>());
            Item.NewItem(null, player.Center, ModContent.ItemType<PuffInABottle>());
            Item.NewItem(null, player.Center, ItemID.BugNet);
            Item.NewItem(null, player.Center, ItemID.Squirrel);

            if (Main.zenithWorld || Main.remixWorld)
            {
                Item.NewItem(null, player.Center, ItemID.ObsidianSkinPotion, 5);
            }

            bool isTerry = player.name.ToLower().Contains("terry");

            if (isTerry)
            {
                GiveItem("Fargowiltas", "HalfInstavator");
                GiveItem("Fargowiltas", "RegalStatue");
                Item.NewItem(null, player.Center, ItemID.PlatinumCoin);
                Item.NewItem(null, player.Center, ItemID.GrapplingHook);
                Item.NewItem(null, player.Center, ItemID.LifeCrystal, 4);
                Item.NewItem(null, player.Center, ItemID.ManaCrystal, 2);
                Item.NewItem(null, player.Center, ModContent.ItemType<SandsofTime>());
            }

            //only give once per world
            if (!WorldSavingSystem.ReceivedTerraStorage)
            {
                int units = isTerry ? 16 : 4;
                if (ModLoader.TryGetMod("MagicStorage", out Mod _))
                {
                    GiveItem("MagicStorage", "StorageHeart");
                    GiveItem("MagicStorage", "CraftingAccess");
                    GiveItem("MagicStorage", "StorageUnit", units);

                    WorldSavingSystem.ReceivedTerraStorage = true;
                    if (Main.netMode != NetmodeID.SinglePlayer)
                        NetMessage.SendData(MessageID.WorldData); //sync world in mp
                }
                else if (ModLoader.TryGetMod("MagicStorageExtra", out Mod _))
                {
                    GiveItem("MagicStorageExtra", "StorageHeart");
                    GiveItem("MagicStorageExtra", "CraftingAccess");
                    GiveItem("MagicStorageExtra", "StorageUnit", units);

                    WorldSavingSystem.ReceivedTerraStorage = true;
                    if (Main.netMode != NetmodeID.SinglePlayer)
                        NetMessage.SendData(MessageID.WorldData); //sync world in mp
                }
            }
        }

        //bool sheet
        public override void PostSetupContent()
        {
            try
            {
                //CalamityCompatibility = new CalamityCompatibility(this).TryLoad() as CalamityCompatibility;
                //ThoriumCompatibility = new ThoriumCompatibility(this).TryLoad() as ThoriumCompatibility;
                //SoACompatibility = new SoACompatibility(this).TryLoad() as SoACompatibility;
                //MasomodeEXCompatibility = new MasomodeEXCompatibility(this).TryLoad() as MasomodeEXCompatibility;
                //BossChecklistCompatibility = (BossChecklistCompatibility)new BossChecklistCompatibility(this).TryLoad();

                //if (BossChecklistCompatibility != null)
                //    BossChecklistCompatibility.Initialize();

                if (ModLoader.TryGetMod("Wikithis", out Mod wikithis) && !Main.dedServ)
                {
                    wikithis.Call("AddModURL", this, "https://fargosmods.wiki.gg/wiki/{}");

                    // You can also use call ID for some calls!
                    //wikithis.Call(0, this, "https://examplemod.wiki.gg/wiki/{}");

                    // Alternatively, you can use this instead, if your wiki is on terrariamods.fandom.com
                    //wikithis.Call(0, this, "https://terrariamods.fandom.com/wiki/Example_Mod/{}");
                    //wikithis.Call("AddModURL", this, "https://terrariamods.fandom.com/wiki/Example_Mod/{}");

                    // If there wiki on other languages (such as russian, spanish, chinese, etch), then you can also call that:
                    //wikithis.Call(0, this, "https://examplemod.wiki.gg/zh/wiki/{}", GameCulture.CultureName.Chinese)

                    // If you want to replace default icon for your mod, then call this. Icon should be 30x30, either way it will be cut.
                    //wikithis.Call("AddWikiTexture", this, ModContent.Request<Texture2D>(pathToIcon));
                    //wikithis.Call(3, this, ModContent.Request<Texture2D>(pathToIcon));
                }

                DebuffIDs =
                [
                    BuffID.Bleeding,
                    BuffID.OnFire,
                    BuffID.Rabies,
                    BuffID.Confused,
                    BuffID.Weak,
                    BuffID.BrokenArmor,
                    BuffID.Darkness,
                    BuffID.Slow,
                    BuffID.Cursed,
                    BuffID.Poisoned,
                    BuffID.Silenced,
                    39,
                    44,
                    46,
                    47,
                    67,
                    68,
                    69,
                    70,
                    80,
                    //88, this is chaos state
                    //BuffID.ManaSickness, this is mana sickness, why was it here?
                    103,
                    137,
                    144,
                    145,
                    149,
                    156,
                    160,
                    163,
                    164,
                    195,
                    196,
                    197,
                    199,
                    ModContent.BuffType<AnticoagulationBuff>(),
                    ModContent.BuffType<AntisocialBuff>(),
                    ModContent.BuffType<AtrophiedBuff>(),
                    ModContent.BuffType<BerserkedBuff>(),
                    ModContent.BuffType<BloodthirstyBuff>(),
                    ModContent.BuffType<ClippedWingsBuff>(),
                    ModContent.BuffType<CurseoftheMoonBuff>(),
                    ModContent.BuffType<DefenselessBuff>(),
                    ModContent.BuffType<FlamesoftheUniverseBuff>(),
                    ModContent.BuffType<HallowIlluminatedBuff>(),
                    ModContent.BuffType<FusedBuff>(),
                    ModContent.BuffType<GodEaterBuff>(),
                    ModContent.BuffType<HexedBuff>(),
                    ModContent.BuffType<InfestedBuff>(),
                    ModContent.BuffType<NeurotoxinBuff>(),
                    ModContent.BuffType<IvyVenomBuff>(),
                    ModContent.BuffType<JammedBuff>(),
                    ModContent.BuffType<LethargicBuff>(),
                    ModContent.BuffType<LightningRodBuff>(),
                    ModContent.BuffType<LowGroundBuff>(),
                    ModContent.BuffType<MarkedforDeathBuff>(),
                    ModContent.BuffType<MidasBuff>(),
                    //ModContent.BuffType<PoweroftheCosmosBuff>(),
                    ModContent.BuffType<OceanicMaulBuff>(),
                    ModContent.BuffType<OceanicSealBuff>(),
                    ModContent.BuffType<OiledBuff>(),
                    ModContent.BuffType<RushJobBuff>(),
                    ModContent.BuffType<ReverseManaFlowBuff>(),
                    ModContent.BuffType<RottingBuff>(),
                    ModContent.BuffType<ShadowflameBuff>(),
                    ModContent.BuffType<SmiteBuff>(),
                    ModContent.BuffType<SqueakyToyBuff>(),
                    ModContent.BuffType<StunnedBuff>(),
                    ModContent.BuffType<SwarmingBuff>(),
                    ModContent.BuffType<UnstableBuff>(),
                    ModContent.BuffType<AbomFangBuff>(),
                    ModContent.BuffType<AbomPresenceBuff>(),
                    ModContent.BuffType<MutantFangBuff>(),
                    ModContent.BuffType<MutantPresenceBuff>(),
                    ModContent.BuffType<AbomRebirthBuff>(),
                    ModContent.BuffType<TimeFrozenBuff>(),
                    ModContent.BuffType<TwinsInstallBuff>()
                ];
                DefenseReducingDebuffs =
                [
                    33,
                    36,
                    69,
                    195,
                    203,
                    ModContent.BuffType<CurseoftheMoonBuff>(),
                    ModContent.BuffType<BerserkedBuff>(), //technically only reduces dr but eh
                    ModContent.BuffType<DefenselessBuff>(),
                    ModContent.BuffType<FlamesoftheUniverseBuff>(),
                    ModContent.BuffType<OceanicMaulBuff>(),
                    ModContent.BuffType<TwinsInstallBuff>()
                ];

                const int k = 1000;
                FargoSets.NPCs.SwarmHealth[ModContent.NPCType<RoyalSubject>()] = 5100;
                FargoSets.NPCs.SwarmHealth[ModContent.NPCType<GelatinSubject>()] = 10 * k;
                FargoSets.NPCs.SwarmHealth[ModContent.NPCType<CrystalLeaf>()] = 80 * k;

                BossChecklistCompatibility();

                //Mod bossHealthBar = ModLoader.GetMod("FKBossHealthBar");
                //if (bossHealthBar != null)
                //{
                //    //bossHealthBar.Call("RegisterHealthBarMini", ModContent.NPCType<BabyGuardian>());
                //    bossHealthBar.Call("RegisterHealthBarMini", ModContent.NPCType<TimberChampion>());
                //    bossHealthBar.Call("RegisterHealthBarMini", ModContent.NPCType<TimberChampionHead>());
                //    bossHealthBar.Call("RegisterHealthBarMini", ModContent.NPCType<EarthChampion>());
                //    bossHealthBar.Call("RegisterHealthBarMini", ModContent.NPCType<LifeChampion>());
                //    bossHealthBar.Call("RegisterHealthBarMini", ModContent.NPCType<WillChampion>());
                //    bossHealthBar.Call("RegisterHealthBarMini", ModContent.NPCType<ShadowChampion>());
                //    bossHealthBar.Call("RegisterHealthBarMini", ModContent.NPCType<SpiritChampion>());
                //    bossHealthBar.Call("RegisterHealthBarMini", ModContent.NPCType<TerraChampion>());
                //    bossHealthBar.Call("RegisterHealthBarMini", ModContent.NPCType<NatureChampion>());

                //    bossHealthBar.Call("hbStart");
                //    bossHealthBar.Call("hbSetColours", new Color(1f, 1f, 1f), new Color(1f, 1f, 0.5f), new Color(1f, 0f, 0f));
                //    bossHealthBar.Call("hbFinishSingle", ModContent.NPCType<CosmosChampion>());

                //    bossHealthBar.Call("hbStart");
                //    bossHealthBar.Call("hbSetColours", new Color(1f, 0f, 1f), new Color(1f, 0.2f, 0.6f), new Color(1f, 0f, 0f));
                //    bossHealthBar.Call("hbFinishSingle", ModContent.NPCType<DeviBoss>());

                //    bossHealthBar.Call("RegisterDD2HealthBar", ModContent.NPCType<AbomBoss>());

                //    bossHealthBar.Call("hbStart");
                //    bossHealthBar.Call("hbSetColours", new Color(55, 255, 191), new Color(0f, 1f, 0f), new Color(0f, 0.5f, 1f));
                //    //bossHealthBar.Call("hbSetBossHeadTexture", GetTexture("Content/NPCs/MutantBoss/MutantBoss_Head_Boss"));
                //    bossHealthBar.Call("hbSetTexture",
                //        bossHealthBar.GetTexture("UI/MoonLordBarStart"), null,
                //        bossHealthBar.GetTexture("UI/MoonLordBarEnd"), null);
                //    bossHealthBar.Call("hbSetTextureExpert",
                //        bossHealthBar.GetTexture("UI/MoonLordBarStart_Exp"), null,
                //        bossHealthBar.GetTexture("UI/MoonLordBarEnd_Exp"), null);
                //    bossHealthBar.Call("hbFinishSingle", ModContent.NPCType<MutantBoss>());
                //}

                //mutant shop
                Mod fargos = MutantMod;
                fargos.Call("AddSummon", 0.5f, "FargowiltasSouls", "SquirrelCoatofArms", new Func<bool>(() => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TrojanSquirrel]), Item.buyPrice(0, 4));
                fargos.Call("AddSummon", 2.79f, "FargowiltasSouls", "CoffinSummon", new Func<bool>(() => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.CursedCoffin]), Item.buyPrice(0, 9));
                fargos.Call("AddSummon", 6.9f, "FargowiltasSouls", "DevisCurse", new Func<bool>(() => WorldSavingSystem.DownedDevi), Item.buyPrice(0, 17, 50));
                fargos.Call("AddSummon", 8.7f, "FargowiltasSouls", "MechLure", new Func<bool>(() => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.BanishedBaron]), Item.buyPrice(0, 22));
                fargos.Call("AddSummon", 11.49f, "FargowiltasSouls", "CrystallineEffigy", new Func<bool>(() => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.Lifelight]), Item.buyPrice(0, 45));
                fargos.Call("AddSummon", 18.009f, "FargowiltasSouls", "ChampionySigil", new Func<bool>(() => WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.CosmosChampion]), Item.buyPrice(5));
                fargos.Call("AddSummon", 18.01f, "FargowiltasSouls", "AbomsCurse", new Func<bool>(() => WorldSavingSystem.DownedAbom), Item.buyPrice(10));
                //fargos.Call("AddSummon", 18.01f, "FargowiltasSouls", "TruffleWormEX", () => WorldSavingSystem.downedFishronEX, Item.buyPrice(10));
                fargos.Call("AddSummon", 18.02f, "FargowiltasSouls", "MutantsCurse", new Func<bool>(() => WorldSavingSystem.DownedMutant), Item.buyPrice(20));

                //stat sheet
                fargos.Call("AddPermaUpgrade", new Item(ModContent.ItemType<RabiesVaccine>()), () => Main.LocalPlayer.FargoSouls().RabiesVaccine);
                fargos.Call("AddPermaUpgrade", new Item(ModContent.ItemType<MutantsDiscountCard>()), () => Main.LocalPlayer.FargoSouls().MutantsDiscountCard);
                fargos.Call("AddPermaUpgrade", new Item(ModContent.ItemType<MutantsCreditCard>()), () => Main.LocalPlayer.FargoSouls().MutantsCreditCard);

                // emode world gen screen toggle
                Action<bool> setEmode = (value) => WorldSavingSystem.QueueEnableEternityMode = value;
                BOBW.Call("AddWorldGenToggle", Name, "Mods.FargoSeeds.WorldGenMenu.HeaderGeneral", "Mods.FargowiltasSouls.UI.Eternity", "Mods.FargowiltasSouls.UI.TogglesEternity", new Color(28, 222, 152), FargoAssets.Filepath + "UI/OncomingMutant", false, setEmode);
            }
            catch (Exception e)
            {
                Logger.Warn("FargowiltasSouls PostSetupContent Error: " + e.StackTrace + e.Message);
            }
        }

        public static void ManageMusicTimestop(bool playMusicAgain)
        {
            if (Main.dedServ)
                return;

            if (playMusicAgain)
            {
                if (OldMusicFade > 0)
                {
                    Main.musicFade[Main.curMusic] = OldMusicFade;
                    OldMusicFade = 0;
                }
            }
            else
            {
                if (OldMusicFade == 0)
                {
                    OldMusicFade = Main.musicFade[Main.curMusic];
                }
                else
                {
                    for (int i = 0; i < Main.musicFade.Length; i++)
                        Main.musicFade[i] = 0f;
                }
            }
        }

        static float ColorTimer;

        public static Color EModeColor()
        {
            Color mutantColor = new(28, 222, 152);
            Color abomColor = new(255, 224, 53);
            Color deviColor = new(255, 51, 153);

            ColorTimer += 0.5f;

            if (ColorTimer >= 300)
                ColorTimer = 0;

            if (ColorTimer < 100)
                return Color.Lerp(mutantColor, abomColor, ColorTimer / 100);
            else if (ColorTimer < 200)
                return Color.Lerp(abomColor, deviColor, (ColorTimer - 100) / 100);
            else
                return Color.Lerp(deviColor, mutantColor, (ColorTimer - 200) / 100);
        }

        internal enum PacketID : byte
        {
            RequestGuttedCreeper,
            RequestPearlwoodStar,
            SyncCultistDamageCounterToServer,
            RequestCreeperHeal,
            RequestDeviGift,
            //SyncEModeNPC,
            SpawnFishronEX,
            SyncFishronEXLife,
            SyncTogglesOnJoin,
            SyncOneToggle,
            SyncDefaultToggles,
            SyncActiveSkill,
            SyncCanPlayMaso,
            SyncNanoCoreMode,
            //SpawnBossTryFromNPC,
            HealNPC,
            SyncSnatcherGrab,
            SyncCursedSpiritGrab,
            SyncCursedSpiritRelease,
            SyncAntlionGrab,
            SyncTuskRip,
            DropMutantGift,
            ToggleEternityMode,
            WakeUpDeviantt,
            WakeUpMutant,
            SyncSoulVortexHit,
            ClearNPCBuffFromClient
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte data = reader.ReadByte();
            if (Enum.IsDefined(typeof(PacketID), data))
            {
                switch ((PacketID)data)
                {
                    case PacketID.RequestGuttedCreeper: //server side spawning creepers
                        if (Main.netMode == NetmodeID.Server)
                        {
                            byte p = reader.ReadByte();
                            int multiplier = reader.ReadByte();
                            int n = NPC.NewNPC(NPC.GetBossSpawnSource(p), (int)Main.player[p].Center.X, (int)Main.player[p].Center.Y, ModContent.NPCType<CreeperGutted>(), 0,
                                p, 0f, multiplier, 0);
                            if (n != Main.maxNPCs)
                            {
                                Main.npc[n].velocity = Vector2.UnitX.RotatedByRandom(2 * Math.PI) * 8;
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                            }
                        }
                        break;

                    case PacketID.RequestPearlwoodStar: //client to server
                        if (Main.netMode == NetmodeID.Server)
                        {
                            int p = reader.ReadByte();
                            int n = reader.ReadByte();
                            Item.NewItem(Main.player[p].GetSource_OnHit(Main.npc[n]), Main.npc[n].Hitbox, ItemID.ManaCloakStar);
                        }
                        break;

                    case PacketID.SyncCultistDamageCounterToServer: //client to server
                        if (Main.netMode == NetmodeID.Server)
                        {
                            int cult = reader.ReadByte();

                            LunaticCultist cultist = Main.npc[cult].GetGlobalNPC<LunaticCultist>();
                            cultist.MeleeDamageCounter += reader.ReadInt32();
                            cultist.RangedDamageCounter += reader.ReadInt32();
                            cultist.MagicDamageCounter += reader.ReadInt32();
                            cultist.MinionDamageCounter += reader.ReadInt32();
                        }
                        break;

                    case PacketID.RequestCreeperHeal: //refresh creeper
                        if (Main.netMode != NetmodeID.SinglePlayer)
                        {
                            byte player = reader.ReadByte();
                            NPC creeper = Main.npc[reader.ReadByte()];
                            if (creeper.active && creeper.type == ModContent.NPCType<CreeperGutted>() && creeper.ai[0] == player)
                            {
                                int damage = creeper.lifeMax - creeper.life;
                                creeper.life = creeper.lifeMax;
                                if (damage > 0)
                                    CombatText.NewText(creeper.Hitbox, CombatText.HealLife, damage);
                                if (Main.netMode == NetmodeID.Server)
                                    creeper.netUpdate = true;
                            }
                        }
                        break;



                    case PacketID.RequestDeviGift: //devi gifts
                        if (Main.netMode == NetmodeID.Server)
                        {
                            Player player = Main.player[reader.ReadByte()];
                            DropDevianttsGift(player);
                        }
                        break;


                    //case PacketID.SyncEModeNPC: // New maso sync
                    //    {
                    //        int npcToSync = reader.ReadInt32();
                    //        int npcType = reader.ReadInt32();
                    //        int bytesLength = reader.ReadInt32();
                    //        //Logger.Debug($"got {npcToSync} {npcType}, real is {Main.npc[npcToSync].active} {Main.npc[npcToSync].type}");
                    //        if (Main.npc[npcToSync].active && Main.npc[npcToSync].type == npcType)
                    //        {
                    //            Main.npc[npcToSync].GetGlobalNPC<NewEModeGlobalNPC>().NetRecieve(reader);
                    //        }
                    //        else if (bytesLength > 0) //in case of desync between client/server, just clear the rest of the message from the buffer
                    //        {
                    //            reader.ReadBytes(bytesLength);
                    //        }
                    //    }
                    //    break;

                    case PacketID.SpawnFishronEX: //server side spawning fishron EX
                        if (Main.netMode == NetmodeID.Server)
                        {
                            byte target = reader.ReadByte();
                            int x = reader.ReadInt32();
                            int y = reader.ReadInt32();
                            EModeGlobalNPC.spawnFishronEX = true;
                            NPC.NewNPC(NPC.GetBossSpawnSource(target), x, y, NPCID.DukeFishron, 0, 0f, 0f, 0f, 0f, target);
                            EModeGlobalNPC.spawnFishronEX = false;
                            ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", Language.GetTextValue("Mods.FargowiltasSouls.NPCs.DukeFishronEX.DisplayName")), new Color(50, 100, 255));
                        }
                        break;

                    case PacketID.SyncFishronEXLife: //confirming fish EX max life
                        {
                            int f = reader.ReadInt32();
                            Main.npc[f].lifeMax = reader.ReadInt32();
                        }
                        break;

                    case PacketID.SyncTogglesOnJoin: //sync toggles on join
                        {
                            Player player = Main.player[reader.ReadByte()];
                            FargoSoulsPlayer modPlayer = player.FargoSouls();
                            byte count = reader.ReadByte();
                            List<AccessoryEffect> keys = ToggleLoader.LoadedToggles.Keys.ToList();

                            for (int i = 0; i < count; i++)
                            {
                                modPlayer.Toggler.Toggles[keys[i]].ToggleBool = reader.ReadBoolean();
                            }
                        }
                        break;

                    case PacketID.SyncOneToggle: //sync single toggle
                        {
                            Player player = Main.player[reader.ReadByte()];
                            player.SetToggleValue(AccessoryEffectLoader.GetEffect(reader.ReadString()), reader.ReadBoolean());
                        }
                        break;
                    case PacketID.SyncActiveSkill:
                        {
                            Player player = Main.player[reader.ReadByte()];
                            int slotIndex = reader.ReadInt32();
                            int skillIndex = reader.ReadInt32();
                            AccessoryEffect skill = skillIndex < 0 ? null : AccessoryEffectLoader.AccessoryEffects[skillIndex];

                            player.FargoSouls().ActiveSkills[slotIndex] = skill;

                        }
                        break;
                    case PacketID.SyncDefaultToggles:
                        {
                            Player player = Main.player[reader.ReadByte()];
                            FargoSoulsPlayer modPlayer = player.FargoSouls();
                            modPlayer.Toggler_ExtraAttacksDisabled = reader.ReadBoolean();
                            modPlayer.Toggler_MinionsDisabled = reader.ReadBoolean();
                            modPlayer.Toggler_ExtraJumpsDisabled = reader.ReadBoolean();
                        }
                        break;

                    case PacketID.SyncCanPlayMaso: //server acknowledges a CanPlayMaso player
                        if (Main.netMode == NetmodeID.Server)
                        {
                            WorldSavingSystem.CanPlayMaso = reader.ReadBoolean();
                        }
                        break;

                    case PacketID.SyncNanoCoreMode:
                        {
                            Player player = Main.player[reader.ReadByte()];
                            player.GetModPlayer<NanoPlayer>().NanoCoreMode = reader.ReadInt32();
                        }
                        break;

                    //case PacketID.SpawnBossTryFromNPC:
                    //    if (Main.netMode == NetmodeID.Server)
                    //    {
                    //        int p = reader.ReadInt32();
                    //        int originalType = reader.ReadInt32();
                    //        int bossType = reader.ReadInt32();
                    //        FargoSoulsUtil.SpawnBossTryFromNPC(p, originalType, bossType);
                    //    }
                    //    break;

                    case PacketID.HealNPC:
                        {
                            NPC npc = FargoSoulsUtil.NPCExists(reader.ReadByte());
                            int heal = reader.ReadInt32();
                            if (npc != null)
                            {
                                npc.life += heal;
                                if (npc.life > npc.lifeMax)
                                    npc.life = npc.lifeMax;
                                npc.HealEffect(heal);
                                npc.netUpdate = true;
                            }
                        }
                        break;

                    case PacketID.SyncSnatcherGrab: // client to server
                        {
                            NPC npc = FargoSoulsUtil.NPCExists(reader.ReadByte());
                            if (npc.TryGetGlobalNPC(out Snatchers snatcher))
                            {
                                snatcher.BittenPlayer = reader.ReadByte();
                                snatcher.BiteTimer = reader.ReadInt32();
                                npc.netUpdate = true;
                            }
                        }
                        break;
                    case PacketID.SyncCursedSpiritGrab: // client to server
                        {
                            NPC npc = FargoSoulsUtil.NPCExists(reader.ReadByte());
                            if (npc.ModNPC is CursedSpirit spirit)
                            {
                                spirit.BittenPlayer = reader.ReadByte();
                                spirit.BiteTimer = reader.ReadInt32();
                                npc.netUpdate = true;

                                NPC owner = FargoSoulsUtil.NPCExists(spirit.Owner, ModContent.NPCType<CursedCoffin>());
                                if (owner.TypeAlive<CursedCoffin>())
                                {
                                    // Forces Coffin to enter grab punish state
                                    owner.As<CursedCoffin>().ForceGrabPunish = 1;
                                    owner.netUpdate = true;
                                }
                            }
                        }
                        break;

                    case PacketID.SyncCursedSpiritRelease: // client to server
                        {
                            NPC npc = FargoSoulsUtil.NPCExists(reader.ReadByte());
                            if (npc.ModNPC is CursedSpirit spirit)
                            {
                                Player victim = Main.player[reader.ReadByte()];
                                spirit.BittenPlayer = -1;
                                spirit.BiteTimer = -90; //cooldown

                                // dash away otherwise it's bullshit
                                npc.velocity = -npc.SafeDirectionTo(victim.Center) * 12;
                                victim.immune = true;
                                victim.immuneTime = Math.Max(victim.immuneTime, 30);
                                victim.hurtCooldowns[0] = Math.Max(victim.hurtCooldowns[0], 30);
                                victim.hurtCooldowns[1] = Math.Max(victim.hurtCooldowns[1], 30);

                                npc.netUpdate = true;
                                spirit.Timer = 0;
                                spirit.AI3 = 0;

                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                            }
                        }
                        break;


                    case PacketID.SyncTuskRip: // client to server
                        {
                            NPC target = FargoSoulsUtil.NPCExists(reader.ReadByte());
                            Player player = FargoSoulsUtil.PlayerExists(reader.ReadByte());
                            IEnumerable<Projectile> embeddedShrapnel = Main.projectile.Where(p => p.TypeAlive<BaronTuskShrapnel>() && p.owner == player.whoAmI && p.As<BaronTuskShrapnel>().EmbeddedNPC == target);
                            foreach (Projectile proj in embeddedShrapnel)
                            {
                                proj.ai[1] = 2;
                                proj.netUpdate = true;
                            }
                        }
                        break;
                    case PacketID.DropMutantGift:
                        {
                            int i = reader.ReadInt32();
                            int j = reader.ReadInt32();
                            WorldGen.KillTile(i, j);
                        }
                        break;

                    case PacketID.ToggleEternityMode:
                        {
                            Player player = FargoSoulsUtil.PlayerExists(reader.ReadByte());
                            int diff = reader.ReadByte();
                            if (Main.netMode == NetmodeID.Server)
                            {
                                string toggle = diff == 2 ? "Master" : diff == 1 ? "Expert" : "None";
                                if (diff != 0)
                                {
                                    bool changed = false;
                                    if (Main.GameModeInfo.IsJourneyMode)
                                    {
                                        float value = diff == 2 ? 1f : 0.66f;
                                        var slider = CreativePowerManager.Instance.GetPower<DifficultySliderPower>();
                                        typeof(CreativePowers.DifficultySliderPower).GetMethod("SetValueKeyboardForced", LumUtils.UniversalBindingFlags).Invoke(slider, [value]);
                                    }
                                    else
                                    {
                                        switch (diff)
                                        {
                                            case 1:
                                                if (Main.GameMode != GameModeID.Expert)
                                                    changed = true;
                                                Main.GameMode = GameModeID.Expert;
                                                break;
                                            case 2:
                                                if (Main.GameMode != GameModeID.Master)
                                                    changed = true;
                                                Main.GameMode = GameModeID.Master;
                                                break;
                                        }
                                    }
                                    if (changed)
                                        FargoSoulsUtil.PrintLocalization($"Mods.Fargowiltas.Items.ModeToggle.{toggle}", new Color(175, 75, 255));
                                }

                                WorldSavingSystem.ShouldBeEternityMode = diff != 0;
                                if (diff != 0)
                                {
                                    WorldSavingSystem.SpawnedDevi = true;
                                }

                                NetMessage.SendData(MessageID.WorldData); //sync world
                            }
                            else
                            {
                                string mode;
                                float volume = 0.5f;
                                switch (diff)
                                {
                                    case 1:
                                        mode = "Emode";
                                        break;
                                    case 2:
                                        mode = "Maso";
                                        break;
                                    default:
                                        mode = "Deactivate";
                                        volume = 1;
                                        break;
                                }
                                SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Difficulty" + mode) with { Volume = volume });
                            }
                        }
                        break;
                    case PacketID.WakeUpDeviantt:
                        {
                            NPC npc = FargoSoulsUtil.NPCExists(reader.ReadByte());
                            if (npc.ModNPC is UnconsciousDeviantt)
                            {
                                if (Main.netMode == NetmodeID.Server)
                                    UnconsciousDeviantt.WakeUp(npc);
                            }
                        }
                        break;
                    case PacketID.WakeUpMutant:
                        {
                            NPC npc = FargoSoulsUtil.NPCExists(reader.ReadByte());
                            if (npc.ModNPC is ReleasedMutant && Main.netMode == NetmodeID.Server)
                            {
                                npc.Transform(ModContent.NPCType<Mutant>());
                                WorldSavingSystem.HaveForcedMutantFromKS = true;
                                NetMessage.SendData(MessageID.WorldData);
                            }
                        }
                        break;
                    case PacketID.SyncSoulVortexHit:
                        {
                            NPC npc = FargoSoulsUtil.NPCExists(reader.ReadByte());
                            int ai2 = reader.ReadInt32();
                            float ai3 = reader.ReadSingle();
                            Vector2 velocity = reader.ReadVector2();
                            if (npc != null && npc.active && npc.ModNPC is SoulVortex && Main.netMode == NetmodeID.Server)
                            {
                                npc.ai[2] = ai2;
                                npc.ai[3] = ai3;
                                npc.velocity = velocity;
                                npc.netUpdate = true;
                            }
                        }
                        break;
                    case PacketID.ClearNPCBuffFromClient:
                        {
                            NPC npc = FargoSoulsUtil.NPCExists(reader.ReadByte());
                            int buffType = reader.ReadByte();
                            if (npc != null)
                            {
                                int index = npc.FindBuffIndex(buffType);
                                if (index > -1)
                                    npc.DelBuff(index);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public static bool NoInvasion(NPCSpawnInfo spawnInfo)
        {
            return !spawnInfo.Invasion && (!Main.pumpkinMoon && !Main.snowMoon || spawnInfo.SpawnTileY > Main.worldSurface || Main.dayTime) &&
                   (!Main.eclipse || spawnInfo.SpawnTileY > Main.worldSurface || !Main.dayTime);
        }

        public static bool NoBiome(NPCSpawnInfo spawnInfo)
        {
            Player player = spawnInfo.Player;
            return player.ZonePurity && !player.ZoneJungle && !player.ZoneDungeon && !player.ZoneCorrupt && !player.ZoneCrimson && !player.ZoneHallow && !player.ZoneSnow && !player.ZoneUndergroundDesert;
        }

        public static bool NoZoneAllowWater(NPCSpawnInfo spawnInfo) => !spawnInfo.Sky && !spawnInfo.Player.ZoneMeteor && !spawnInfo.SpiderCave;

        public static bool NoZone(NPCSpawnInfo spawnInfo) => NoZoneAllowWater(spawnInfo) && !spawnInfo.Water;

        public static bool NormalSpawn(NPCSpawnInfo spawnInfo)
        {
            return !spawnInfo.PlayerInTown && NoInvasion(spawnInfo);
        }

        public static bool NoZoneNormalSpawn(NPCSpawnInfo spawnInfo) => NormalSpawn(spawnInfo) && NoZone(spawnInfo);

        public static bool NoZoneNormalSpawnAllowWater(NPCSpawnInfo spawnInfo) => NormalSpawn(spawnInfo) && NoZoneAllowWater(spawnInfo);

        public static bool NoBiomeNormalSpawn(NPCSpawnInfo spawnInfo) => NormalSpawn(spawnInfo) && NoBiome(spawnInfo) && NoZone(spawnInfo);

        public static TitleLinkButton MakeSimpleButton(string textKey, string linkUrl, int horizontalFrameIndex)
        {   
            //yummy vanilla code 
            Asset<Texture2D> val = FargoAssets.UI.MainMenu.TitleLinkButtons;
            Rectangle value = val.Frame(4, 2, horizontalFrameIndex);
            Rectangle value2 = val.Frame(4, 2, horizontalFrameIndex, 1);
            value.Width--;
            value.Height--;
            value2.Width--;
            value2.Height--;
            return new TitleLinkButton
            {
                TooltipTextKey = textKey,
                LinkUrl = linkUrl,
                FrameWehnSelected = value2,
                FrameWhenNotSelected = value,
                Image = val
            };
        }
        public static void DrawTitleLinks(Color menuColor, float upBump)
        {
            List<TitleLinkButton> titleLinks = fargoTitleLinks;
            Vector2 anchorPosition = new Vector2(18f, (float)(Main.screenHeight - 85 - 22) - upBump);
            for (int i = 0; i < titleLinks.Count; i++)
            {          
                titleLinks[i].Draw(Main.spriteBatch, anchorPosition);
                anchorPosition.X += 30f;
            }
        }

    }
}
