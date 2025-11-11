using Fargowiltas;
using FargowiltasSouls.Content.Items.Weapons.Challengers;
using FargowiltasSouls.Content.Patreon.ParadoxWolf;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Weapons.BossWeapons;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace FargowiltasSouls
{
    public class FargoSoulsSets : ModSystem
    {
        public class Items
        {
            public static bool[] AllowedSoulItemExceptions;
            public static int[] MaterialOfImportantItem;
        }
        public class Projectiles
        {
            public static bool[] PierceResistImmune;
            public static bool[] DoesNotAffectHuntress;
        }
        public class NPCs
        {

        }
        public class Tiles
        {
            public static bool[] CommonTiles;
        }

        public class Buffs
        {
            public static bool[] Debuffs;
        }

        public override void PostSetupContent()
        {
            #region Items
            SetFactory itemFactory = ItemID.Sets.Factory;

            Items.AllowedSoulItemExceptions = itemFactory.CreateBoolSet(false, [
                ModContent.ItemType<ParadoxWolfSoul>(),
                ItemID.RareEnchantment,
                ItemID.SoulofLight,
                ItemID.SoulofNight,
                ItemID.SoulofFlight,
                ItemID.SoulofFright,
                ItemID.SoulofSight,
                ItemID.SoulofMight,
                ItemID.SoulBottleFlight,
                ItemID.SoulBottleFright,
                ItemID.SoulBottleLight,
                ItemID.SoulBottleMight,
                ItemID.SoulBottleNight,
                ItemID.SoulBottleSight
            ]);

            Items.MaterialOfImportantItem = itemFactory.CreateIntSet(0);

            List<int> sacrificePreHM = [
                // squrl
                ItemType<TreeSword>(), 1,
                ItemType<MountedAcornGun>(), 1,
                ItemType<SnowballStaff>(), 1,
                ItemType<KamikazeSquirrelStaff>(), 1,

                // coffin
                ItemType<SisypheanFist>(), 1,
                ItemType<SpiritLongbow>(), 1,
                ItemType<GildedSceptre>(), 1,
                ItemType<EgyptianFlail>(), 1,
                ];
            List<int> sacrificeHM = [
                // brn
                ItemType<TheBaronsTusk>(), 1,
                ItemType<NavalRustrifle>(), 1,
                ItemType<RoseTintedVisor>(), 1,
                ItemType<DecrepitAirstrikeRemote>(), 1,

                // lifelight
                ItemType<EnchantedLifeblade>(), 1,
                ItemType<Lightslinger>(), 1,
                ItemType<CrystallineCongregation>(), 1,
                ItemType<KamikazePixieStaff>(), 1,
                ];
            var sacrifice = sacrificePreHM.Concat(sacrificeHM).ToList();

            for (int i = 0; i < sacrificeHM.Count; i += 2)
                FargoSets.Items.HardmodeSacrifice[sacrificeHM[i]] = true;
            for (int i = 0; i < sacrifice.Count; i += 2)
                FargoSets.Items.SacrificeCountDefault[sacrifice[i]] = sacrifice[i + 1];

            #endregion
            #region Projectiles
            SetFactory projectileFactory = ProjectileID.Sets.Factory;

            Projectiles.PierceResistImmune = projectileFactory.CreateBoolSet(false,
                ProjectileID.FlyingKnife,
                ProjectileID.WeatherPainShot
                );

            Projectiles.DoesNotAffectHuntress = projectileFactory.CreateBoolSet(false,
                ProjectileID.NightsEdge,
                ProjectileType<Tome>()
            );

            List<int> HuntressAIStyle =
            [
                ProjAIStyleID.Vilethorn,
                ProjAIStyleID.MagicMissile,
                ProjAIStyleID.Spear,
                ProjAIStyleID.Drill,
                ProjAIStyleID.HeldProjectile,
                ProjAIStyleID.Xenopopper,
                ProjAIStyleID.ThickLaser,
                ProjAIStyleID.Yoyo,
                ProjAIStyleID.TerrarianBeam,
                ProjAIStyleID.SleepyOctopod,
                ProjAIStyleID.ForwardStab,
                ProjAIStyleID.ShortSword
            ];

            var projKeys = ContentSamples.ProjectilesByType.Where(p => HuntressAIStyle.Contains(p.Value.aiStyle)).ToDictionary();

            for (int i = 0; i < Projectiles.DoesNotAffectHuntress.Length; i++)
            {
                Projectiles.DoesNotAffectHuntress[i] |= projKeys.ContainsKey(i) || EModeGlobalProjectile.FancySwings.Contains(i);
            }


            #endregion
            #region NPCs
            SetFactory npcFactory = NPCID.Sets.Factory;
            #endregion
            #region Tiles
            SetFactory tileFactory = TileID.Sets.Factory;

            var shovel = TileID.Sets.CanBeDugByShovel;
            var stone = TileID.Sets.Conversion.Stone;
            var sand = TileID.Sets.Conversion.Sand;
            var sandstone = TileID.Sets.Conversion.HardenedSand;
            var ice = TileID.Sets.Conversion.Ice;
            var mud = TileID.Sets.Mud;

            Tiles.CommonTiles = tileFactory.CreateBoolSet(false,
                TileID.Marble,
                TileID.Granite
            );

            for (int i = 0; i < Tiles.CommonTiles.Length; i++)
            {
                Tiles.CommonTiles[i] |= shovel[i] || stone[i] || sand[i] || sandstone[i] || ice[i] || mud[i];
            }

            #endregion
            #region Buffs
            SetFactory buffFactory = BuffID.Sets.Factory;

            Buffs.Debuffs = buffFactory.CreateBoolSet(false, [.. FargowiltasSouls.DebuffIDs]);
            #endregion
        }
    }
}
