using System;
using System.Collections.Generic;
using System.Linq;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Achievements;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Placables;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.CustomEnemies.Desert;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Cavern;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Jungle;
using FargowiltasSouls.Content.Projectiles.Eternity.Environment;
using FargowiltasSouls.Core.ItemDropRules.Conditions;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Core.Globals
{
    public partial class EModeGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        //masochist doom
        //public bool[] masoBool = new bool[4];
        //private int Stop = 0;

        public bool BeetleOffenseAura;
        public bool BeetleDefenseAura;
        public bool BeetleUtilAura;
        public int BeetleTimer;
        public int LacCloudTimer;

        public bool PaladinsShield;
        public bool isWaterEnemy;
        public bool BeingSuckedByDread;

        //public List<int> auraDebuffs = new List<int>();
#pragma warning disable CA2211
        public static int slimeBoss = -1;
        public static int eyeBoss = -1;
        public static int eaterBoss = -1;
        public static int brainBoss = -1;
        public static int mageBoss = -1;
        public static int beeBoss = -1;
        public static int skeleBoss = -1;
        public static int deerBoss = -1;
        public static int wallBoss = -1;
        public static int retiBoss = -1;
        public static int spazBoss = -1;
        public static int destroyBoss = -1;
        public static int primeBoss = -1;
        public static int queenSlimeBoss = -1;
        public static int empressBoss = -1;
        public static int betsyBoss = -1;
        public static int fishBoss = -1;
        public static int cultBoss = -1;
        public static int moonBoss = -1;
        public static int guardBoss = -1;
        public static int fishBossEX = -1;
        public static bool spawnFishronEX;
        public static int deviBoss = -1;
        public static int abomBoss = -1;
        public static int mutantBoss = -1;
        public static int championBoss = -1;

        public static int eaterTimer;
        //public static int eaterResist;
#pragma warning restore CA2211

        public override void ResetEffects(NPC npc)
        {
            PaladinsShield = false;
            BeingSuckedByDread = false;

            if (BeetleTimer > 0 && --BeetleTimer <= 0)
            {
                BeetleDefenseAura = false;
                BeetleOffenseAura = false;
                BeetleUtilAura = false;
                LacCloudTimer = 0;
            }
        }

        public override void SetDefaults(NPC npc)
        {
            if (!WorldSavingSystem.EternityMode) return;

            npc.value = (int)(npc.value * 1.3);
            if (!npc.boss && !npc.townNPC && !npc.CountsAsACritter && npc.life > 10 && !Main.masterMode && !LumUtils.AnyBosses())
            {
                npc.lifeMax = (int)Math.Round(npc.lifeMax * 1.1f);
            }
        }

        public override bool PreAI(NPC npc)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.PreAI(npc);

            //squirrel 
            if (!Main.dayTime && Main.player.Any(p => p.Alive() && p.FargoSouls().SquirrelCharm != null))
            {
                int x = (int)npc.Center.X / 16;
                int y = (int)npc.Center.Y / 16;
                if (y < Main.worldSurface && y > 0 && x > 0 && x < Main.maxTilesX)
                {
                    Tile tile = Framing.GetTileSafely(x, y);
                    if (tile != null && tile.WallType == 0)
                    {
                        Lighting.AddLight(npc.Center, 0.5f, 0.5f, 0.5f);
                    }
                }
            }

            if (!npc.dontTakeDamage)
            {
                Vector2 tileCenter = npc.Center;
                tileCenter.X /= 16;
                tileCenter.Y /= 16;
                Tile currentTile = Framing.GetTileSafely((int)tileCenter.X, (int)tileCenter.Y);

                if (Main.raining && (npc.position.Y / 16 < Main.worldSurface))
                {
                    if (currentTile.WallType == WallID.None)
                    {
                        npc.AddBuff(BuffID.Wet, 2);
                    }
                }
            }

            if (BeetleTimer > 0 && npc.Hostile())
            {
                Vector2 pos = npc.position + new Vector2(Main.rand.Next(0, npc.width), Main.rand.Next(0, npc.height));
                float scale = Main.rand.NextFloat(0.4f, 0.8f);
                if (BeetleOffenseAura)
                {
                    Vector2 vel = new(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(1.6f, 2.4f));
                    Particle p = new SparkParticle(pos + npc.velocity, vel, Color.DarkRed, scale, 30);
                    if (Main.rand.NextBool(5)) p.Spawn();

                    //ai speed increase to slime/fighter ai
                    if (npc.aiStyle == NPCAIStyleID.Slime && !npc.noGravity)
                    {
                        if (npc.velocity.Y == 0) npc.ai[0] += 2; //rests on ground for 1/3rd of normal time
                        else if (npc.velocity.Y > 0) npc.GravityMultiplier *= 2f; //drop down faster after jumps
                    }
                    if (npc.aiStyle == NPCAIStyleID.Fighter)
                    {
                        if (Math.Abs(npc.velocity.X) < 2 && Math.Abs(npc.velocity.X) > 0.1f && npc.ai[1] == 0) 
                            npc.velocity.X += 0.5f * npc.direction; //flat speed increase
                    }
                }
                if (BeetleDefenseAura)
                {
                    Vector2 vel = new(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(1.6f, 2.4f));
                    Particle p = new SmallSparkle(pos + npc.velocity, vel + npc.velocity, Color.DarkCyan, scale / 2, Main.rand.Next(20, 30));
                    if (Main.rand.NextBool(5)) p.Spawn();

                    //knockback res up
                    if (BeetleTimer != 1)
                    {
                        if (npc.knockBackResist > 0.1f)
                            npc.knockBackResist = 0.1f;
                    }
                    else npc.knockBackResist = npc.FargoSouls().defKnockBackResist;
                }
                if (BeetleUtilAura)
                {
                    if (LacCloudTimer++ >= 180 && FargoSoulsUtil.HostCheck)
                    {
                        LacCloudTimer = Main.rand.Next(-30, 31);
                        int dmg = FargoSoulsUtil.ScaledProjectileDamage(40);
                        Projectile.NewProjectile(npc.GetSource_Misc("Lac"), npc.Center, Main.rand.NextVector2CircularEdge(1, 1) + npc.velocity, ModContent.ProjectileType<LacBeetleCloud>(), dmg, 0);
                    }
                }
            }
            return true;
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            if (BeetleTimer > 0)
            {
                if (BeetleOffenseAura)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 pos = target.position + new Vector2(Main.rand.Next(0, target.width), Main.rand.Next(0, target.height));
                        Vector2 vel = Main.rand.NextVector2CircularEdge(5, 5) * Main.rand.NextFloat(0.8f, 1.2f);
                        float scale = Main.rand.NextFloat(0.4f, 0.8f);
                        Particle p = new SparkParticle(pos, vel, Color.DarkRed, scale, Main.rand.Next(60, 90));
                        p.Spawn();
                    }

                    if (!Main.dedServ)
                        SoundEngine.PlaySound(SoundID.Shatter with { Pitch = 0.7f }, target.Center);
                }
                if (BeetleUtilAura)
                {

                }
            }
        }
        public void OnHitEither(NPC npc, Player player, int damageDone)
        {
            if (BeetleTimer > 0 && BeetleDefenseAura)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 pos = npc.position + new Vector2(Main.rand.Next(0, npc.width), Main.rand.Next(0, npc.height));
                    Vector2 vel = Main.rand.NextVector2CircularEdge(5, 5) * Main.rand.NextFloat(0.8f, 1.2f);
                    float scale = Main.rand.NextFloat(0.4f, 0.8f);
                    Particle p = new RectangleParticle(pos, vel, Color.DarkCyan, scale / 4, Main.rand.Next(20, 30), Main.rand.NextBool());
                    p.Spawn();
                }
                if (!Main.dedServ) SoundEngine.PlaySound(SoundID.Item27 with {Pitch = 0.8f, MaxInstances = 1}, npc.Center);
            }
        }
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (!WorldSavingSystem.EternityMode)
                return;
            OnHitEither(npc, Main.player[projectile.owner], damageDone);
        }
        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (!WorldSavingSystem.EternityMode)
                return;
            OnHitEither(npc, player, damageDone);
        }

        public static bool DemonCondition(Player player) =>  !Main.remixWorld || MathF.Abs(player.Center.X / 16f - Main.spawnTileX) > Main.maxTilesX / 3;
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (WorldSavingSystem.EternityMode)
            {
                spawnRate = (int)(spawnRate * 0.9);
                maxSpawns = (int)(maxSpawns * 1.2f);

                if (player.ZoneUnderworldHeight && !NPC.downedBoss2)
                {
                    if (DemonCondition(player))
                    {
                        spawnRate /= 2;
                        maxSpawns *= 2;
                    }
                }

                if ((player.ZoneTowerSolar && NPC.ShieldStrengthTowerSolar == 0)
                    || (player.ZoneTowerVortex && NPC.ShieldStrengthTowerVortex == 0)
                    || (player.ZoneTowerNebula && NPC.ShieldStrengthTowerNebula == 0)
                    || (player.ZoneTowerStardust && NPC.ShieldStrengthTowerStardust == 0))
                {
                    maxSpawns = 0;
                }
            }
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            //layers
            int x = spawnInfo.SpawnTileX;
            int y = spawnInfo.SpawnTileY;
            bool cavern = y >= Main.maxTilesY * 0.4f && y <= Main.maxTilesY * 0.8f;
            bool underground = y > Main.worldSurface && y <= Main.maxTilesY * 0.4f;
            bool surface = y < Main.worldSurface && !spawnInfo.Sky;
            bool wideUnderground = cavern || underground;
            bool underworld = spawnInfo.Player.ZoneUnderworldHeight;
            bool sky = spawnInfo.Sky;

            //times
            bool night = !Main.dayTime;
            bool day = Main.dayTime;

            //biomes
            bool noBiome = FargowiltasSouls.NoBiomeNormalSpawn(spawnInfo);
            bool ocean = spawnInfo.Player.ZoneBeach;
            bool dungeon = spawnInfo.Player.ZoneDungeon;
            bool spiderCave = spawnInfo.SpiderCave;
            bool mushroom = spawnInfo.Player.ZoneGlowshroom;
            bool jungle = spawnInfo.Player.ZoneJungle;
            bool granite = spawnInfo.Granite;
            bool marble = spawnInfo.Marble;
            bool corruption = spawnInfo.Player.ZoneCorrupt;
            bool crimson = spawnInfo.Player.ZoneCrimson;
            bool snow = spawnInfo.Player.ZoneSnow;
            bool hallow = spawnInfo.Player.ZoneHallow;
            bool desert = spawnInfo.Player.ZoneDesert;

            bool nebulaTower = spawnInfo.Player.ZoneTowerNebula;
            bool vortexTower = spawnInfo.Player.ZoneTowerVortex;
            bool stardustTower = spawnInfo.Player.ZoneTowerStardust;
            bool solarTower = spawnInfo.Player.ZoneTowerSolar;

            //events
            bool oldOnesArmy = DD2Event.Ongoing && spawnInfo.Player.ZoneOldOneArmy;
            bool frostMoon = surface && night && Main.snowMoon;
            bool pumpkinMoon = surface && night && Main.pumpkinMoon;
            bool solarEclipse = surface && day && Main.eclipse;
            bool lunarEvents = NPC.LunarApocalypseIsUp && (nebulaTower || vortexTower || stardustTower || solarTower);
            bool noEvent = Main.invasionType == 0 && !oldOnesArmy && !frostMoon && !pumpkinMoon && !solarEclipse && !lunarEvents;
            bool thunderstorm = Main.IsItStorming && surface && !snow && !spawnInfo.Player.ZoneSandstorm && noEvent;

            bool noInvasion = !spawnInfo.Invasion;//FargowiltasSouls.NoInvasion(spawnInfo);
            bool worldEvil = WorldGen.crimson;
            bool normalSpawn = !spawnInfo.PlayerInTown && noInvasion && !oldOnesArmy;
            bool wallHackerSpawn = !spawnInfo.PlayerSafe;


            //MASOCHIST MODE
            if (WorldSavingSystem.EternityMode)
            {
                //all the pre hardmode
                if (!Main.hardMode)
                {
                    //mutually exclusive world layers
                    if (surface)
                    {
                        if (night && normalSpawn)
                        {
                            if (noBiome)
                            {
                                pool[worldEvil ? NPCID.CrimsonBunny : NPCID.CorruptBunny] = NPC.downedBoss1 ? .04f : .02f;
                            }

                            if (snow)
                            {
                                pool[worldEvil ? NPCID.CrimsonPenguin : NPCID.CorruptPenguin] = NPC.downedBoss1 ? .08f : .04f;
                            }

                            if (ocean || Main.raining)
                            {
                                pool[worldEvil ? NPCID.CrimsonGoldfish : NPCID.CorruptGoldfish] = NPC.downedBoss1 ? .08f : .04f;
                            }

                            if (Main.moonPhase == 0) //full moon
                            {
                                pool[NPCID.Raven] = .3f;
                            }

                            if (jungle)
                                pool[NPCID.DoctorBones] = .05f;
                        }

                        if (thunderstorm && normalSpawn)
                        {
                            if (NPC.CountNPCS(NPCID.AngryNimbus) < 2) //abide by vanilla limit
                                pool[NPCID.AngryNimbus] = .1f;
                        }

                        if (day && NPC.downedGolemBoss && (noBiome || dungeon) && normalSpawn)
                            pool[NPCID.CultistArcherWhite] = .01f;

                        float scoutRate = 0.07f;
                        int xFromSpawn = Math.Abs(x - Main.spawnTileX);
                        bool goblinCondition = (xFromSpawn > Main.maxTilesX / 3 || Main.remixWorld) && noBiome && normalSpawn;
                        if ((!NPC.savedGoblin && goblinCondition) || (pool.TryGetValue(NPCID.GoblinScout, out float value) && value < scoutRate))
                        {
                            pool[NPCID.GoblinScout] = scoutRate;
                        }
                            

                    }
                    else if (wideUnderground)
                    {
                        if (marble && NPC.downedBoss2 && normalSpawn)
                        {
                            pool[NPCID.Medusa] = .04f;
                        }

                        if (granite && normalSpawn)
                        {
                            pool[NPCID.GraniteFlyer] = .1f;
                            pool[NPCID.GraniteGolem] = .1f;
                        }

                        if (cavern && normalSpawn)
                        {
                            if (noBiome && NPC.downedBoss3)
                                pool[NPCID.DarkCaster] = .02f;
                            if (noBiome && (!pool.ContainsKey(NPCID.RockGolem) || pool[NPCID.RockGolem] < 0.01f))
                                pool[NPCID.RockGolem] = 0.01f;
                        }

                        if (NPC.downedGoblins && !NPC.savedGoblin && !NPC.AnyNPCs(NPCID.BoundGoblin))
                            pool[NPCID.BoundGoblin] = .5f;

                        if (spiderCave && !NPC.savedStylist && !NPC.AnyNPCs(NPCID.WebbedStylist))
                            pool[NPCID.WebbedStylist] = .5f;
                    }
                    else if (underworld && normalSpawn)
                    {
                        if (wallHackerSpawn)
                            pool[NPCID.LeechHead] = .02f;
                        pool[NPCID.BlazingWheel] = .05f;
                    }
                    else if (sky && normalSpawn)
                    {
                        if (NPC.downedBoss2 && wallHackerSpawn && (x / 16f < Main.maxTilesX * 0.45 || x / 16f > Main.maxTilesX * 0.55)) //harpy condition
                            pool[NPCID.MeteorHead] = .1f;
                    }

                    //height-independent biomes
                    if (jungle)
                    {
                        if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld && normalSpawn)
                            pool[NPCID.Parrot] = .01f;
                    }

                    if (mushroom && normalSpawn)
                    {
                        pool[NPCID.MushiLadybug] = .02f;
                        pool[NPCID.ZombieMushroom] = .02f;
                        pool[NPCID.ZombieMushroomHat] = .02f;
                        pool[NPCID.AnomuraFungus] = .02f;
                        pool[NPCID.TruffleWorm] = .005f;
                        if (wallHackerSpawn)
                            pool[NPCID.FungiBulb] = .02f;
                    }

                    if (ocean && normalSpawn)
                    {
                        if (spawnInfo.Water)
                        {
                            pool[NPCID.AnglerFish] = .1f;
                        }

                        if (wallHackerSpawn)
                        {
                            pool[NPCID.PigronHallow] = .006f;
                        }

                        if (Main.bloodMoon && spawnInfo.Water)
                        {
                            pool[NPCID.EyeballFlyingFish] = .02f;
                            if (wallHackerSpawn) //technically yeah he does
                                pool[NPCID.ZombieMerman] = .02f;
                        }
                    }

                    if (normalSpawn && (!surface || Main.tile[x, y].WallType == WallID.DirtUnsafe))
                    {
                        pool[NPCID.Mimic] = .002f;
                    }
                }
                else //all the hardmode
                {
                    //mutually exclusive world layers
                    if (surface && !lunarEvents)
                    {
                        if (day)
                        {
                            if (normalSpawn)
                            {
                                if (NPC.downedGolemBoss && (noBiome || dungeon))
                                    pool[NPCID.CultistArcherWhite] = .01f;
                            }
                        }
                        else //night
                        {
                            if (Main.bloodMoon && normalSpawn)
                            {
                                pool[NPCID.ChatteringTeethBomb] = .1f;
                            }
                                
                            if (normalSpawn)
                            {
                                if (noBiome)
                                {
                                    pool[worldEvil ? NPCID.CrimsonBunny : NPCID.CorruptBunny] = .1f;
                                }

                                if (snow)
                                {
                                    pool[worldEvil ? NPCID.CrimsonPenguin : NPCID.CorruptPenguin] = .1f;
                                }

                                if (ocean || Main.raining)
                                {
                                    pool[worldEvil ? NPCID.CrimsonGoldfish : NPCID.CorruptGoldfish] = .1f;
                                }

                                if (Main.moonPhase == 0) //full moon
                                {
                                    pool[NPCID.Raven] = .1f;
                                }

                                if (NPC.downedMechBossAny && wallHackerSpawn)
                                    pool[NPCID.Probe] = 0.01f;

                                if (NPC.downedPlantBoss)
                                {
                                    pool[NPCID.SkeletonSniper] = .005f;
                                    pool[NPCID.SkeletonCommando] = .005f;
                                    pool[NPCID.TacticalSkeleton] = .005f;
                                }
                            }

                            if (noInvasion && normalSpawn)
                            {
                                #region night pumpkin moon, frost moon
                                if (noBiome)
                                {
                                    pool[NPCID.Scarecrow1] = .01f;
                                    pool[NPCID.Scarecrow2] = .01f;
                                    pool[NPCID.Scarecrow3] = .01f;
                                    pool[NPCID.Scarecrow4] = .01f;
                                    pool[NPCID.Scarecrow5] = .01f;
                                    pool[NPCID.Scarecrow6] = .01f;
                                    pool[NPCID.Scarecrow7] = .01f;
                                    pool[NPCID.Scarecrow8] = .01f;
                                    pool[NPCID.Scarecrow9] = .01f;
                                    pool[NPCID.Scarecrow10] = .01f;
                                }
                                else //in some biome
                                {
                                    if (hallow)
                                    {
                                        pool[NPCID.PresentMimic] = .01f;
                                    }
                                    else if (crimson || corruption)
                                    {
                                        pool[NPCID.Splinterling] = .05f;
                                    }

                                    if (snow)
                                    {
                                        pool[NPCID.ZombieElf] = .02f;
                                        pool[NPCID.ZombieElfBeard] = .02f;
                                        pool[NPCID.ZombieElfGirl] = .02f;

                                        pool[NPCID.ElfArcher] = .05f;
                                        pool[NPCID.ElfCopter] = .01f;
                                    }
                                }
                                #endregion
                            }
                        }

                        if (hallow && normalSpawn)
                        {
                            pool[NPCID.GingerbreadMan] = .05f;
                        }

                        if (ocean && normalSpawn)
                        {
                            if (night)
                            {
                                pool[NPCID.CreatureFromTheDeep] = .02f;
                            }
                            if (Main.bloodMoon && spawnInfo.Water && wallHackerSpawn)
                            {
                                pool[NPCID.BloodEelHead] = .02f;
                                pool[NPCID.GoblinShark] = .02f;
                            }
                            pool[NPCID.PigronHallow] = .06f;
                        }
                        else if (desert && normalSpawn)
                        {
                            pool[NPCID.DesertBeast] = .1f;
                        }
                    }
                    else if (wideUnderground)
                    {
                        if (cavern)
                        {
                            if (noBiome && NPC.downedBoss3)
                                pool[NPCID.DarkCaster] = .05f;
                        }

                        if (!NPC.savedWizard && !NPC.AnyNPCs(NPCID.BoundWizard))
                            pool[NPCID.BoundWizard] = .5f;
                    }
                    else if (underworld)
                    {
                        pool[NPCID.LeechHead] = .025f;
                        pool[NPCID.BlazingWheel] = .05f;

                        if (NPC.downedPlantBoss)
                        {
                            pool[NPCID.DiabolistRed] = .01f;
                            pool[NPCID.DiabolistWhite] = .01f;
                            pool[NPCID.Necromancer] = .01f;
                            pool[NPCID.NecromancerArmored] = .01f;
                            pool[NPCID.RaggedCaster] = .01f;
                            pool[NPCID.RaggedCasterOpenCoat] = .01f;
                        }
                    }
                    else if (sky && normalSpawn)
                    {
                        if (NPC.CountNPCS(NPCID.AngryNimbus) < 2) //abide by vanilla limit
                            pool[NPCID.AngryNimbus] = .1f;
                        pool[NPCID.MeteorHead] = .05f;
                        pool[NPCID.MartianProbe] = .01f;

                        if (NPC.downedGolemBoss)
                        {
                            pool[NPCID.SolarCrawltipedeHead] = .03f;
                            pool[NPCID.VortexHornetQueen] = .03f;
                            pool[NPCID.NebulaBrain] = .03f;
                            pool[NPCID.StardustJellyfishBig] = .03f;
                            pool[NPCID.AncientCultistSquidhead] = .03f;
                            pool[NPCID.CultistDragonHead] = .03f;
                        }
                    }

                    //height-independent biomes
                    if (jungle && normalSpawn)
                    {
                        pool[NPCID.Parrot] = .01f;

                        if (!surface)
                        {
                            pool[NPCID.BigMimicJungle] = .0015f;
                        }
                    }

                    if (spawnInfo.Lihzahrd && spawnInfo.SpawnTileType == TileID.LihzahrdBrick && normalSpawn)
                    {
                        //pool[NPCID.BlazingWheel] = .1f;
                        //pool[NPCID.SpikeBall] = .1f;
                        //pool[NPCID.BigMimicJungle] = .1f;
                    }

                    if (ocean && spawnInfo.Water && normalSpawn)
                    {
                        pool[NPCID.AnglerFish] = .1f;
                    }
                }
                // irrespective of hardmode
                if (snow && normalSpawn && (underground || day))
                {
                    pool[NPCID.SnowFlinx] = .05f;
                }

                pool[NPCID.BartenderUnconscious] = 0f;
            }
        }

        public static List<int> CrimsonEnemies =
        [
            NPCID.FaceMonster, NPCID.Crimera, NPCID.BigCrimera, NPCID.LittleCrimera, NPCID.BloodCrawler, NPCID.BloodCrawlerWall, NPCID.CrimsonGoldfish, NPCID.CrimsonBunny, NPCID.CrimsonPenguin,
            NPCID.Herpling, NPCID.Crimslime, NPCID.BigCrimslime, NPCID.LittleCrimslime, NPCID.BloodJelly, NPCID.BloodFeeder, NPCID.BloodMummy, NPCID.DesertLamiaDark, NPCID.DesertGhoulCrimson, 
            NPCID.DesertDjinn, NPCID.SandsharkCrimson, NPCID.IchorSticker, NPCID.FloatyGross, NPCID.CrimsonAxe, NPCID.PigronCrimson, NPCID.BigMimicCrimson
        ];

        public override void OnKill(NPC npc)
        {
            base.OnKill(npc);
            if (!WorldSavingSystem.EternityMode)
                return;
            if (npc.type == NPCID.Painter && WorldSavingSystem.DownedMutant && NPC.AnyNPCs(ModContent.NPCType<MutantBoss>()))
                Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ModContent.ItemType<ScremPainting>());

            if (BeetleTimer > 0 && BeetleUtilAura)
            {
                int dmg = FargoSoulsUtil.ScaledProjectileDamage(40);
                Projectile.NewProjectile(npc.GetSource_Death(), npc.Center, Vector2.Zero, ModContent.ProjectileType<LacBeetleCloud>(), dmg, 0);
            }

            int closestP = Player.FindClosest(npc.Center, 1, 1);
            if (CrimsonEnemies.Contains(npc.type) && !FargoSoulsUtil.AnyBossAlive() && closestP >= 0 && Main.player[closestP].ZoneCrimson && (Main.player[closestP].ZoneOverworldHeight || Main.player[closestP].ZoneDirtLayerHeight))
            {
                for (int i = 0; i < Main.rand.Next(1, 4); i++)
                {
                    Projectile.NewProjectileDirect(npc.GetSource_Death(), npc.Center, new Vector2(0, Main.rand.NextFloat(-14, -4)).RotatedByRandom(MathHelper.ToRadians(35)), ModContent.ProjectileType<BloodDroplet>(), 0, 0);
                }
            }         
        }
        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {          
            if (npc.boss && npc.life <= 0 && WorldSavingSystem.EternityMode)
            {
                if (npc.ModNPC == null || npc.ModNPC.Mod == Mod)
                {
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        if (Main.myPlayer == Main.player[i].whoAmI)
                            ModContent.GetInstance<FirstEternityBossAchievement>().Condition.Complete();                        
                    }
                }
            }
            base.HitEffect(npc, hit);
        }
        
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            #region tim's concoction drops

            void TimsConcoctionDrop(IItemDropRule rule)
            {
                TimsConcoctionDropCondition dropCondition = new();
                IItemDropRule conditionalRule = new LeadingConditionRule(dropCondition);
                conditionalRule.OnSuccess(rule);
                npcLoot.Add(conditionalRule);
            }
            
            switch (npc.type)
            {
                case NPCID.BlueSlime:
                    TimsConcoctionDrop(ItemDropRule.Common(npc.netID == NPCID.Pinky ? ItemID.TeleportationPotion : ItemID.RecallPotion, 3));
                    break;

                case NPCID.DemonEye:
                case NPCID.DemonEye2:
                case NPCID.DemonEyeOwl:
                case NPCID.DemonEyeSpaceship:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.NightOwlPotion, 2));
                    break;

                case NPCID.MossHornet:
                case NPCID.Hornet:
                case NPCID.HornetFatty:
                case NPCID.HornetHoney:
                case NPCID.HornetLeafy:
                case NPCID.HornetSpikey:
                case NPCID.HornetStingy:
                    TimsConcoctionDrop(new CommonDrop(ItemID.BattlePotion, 3, 1, 2, 2));
                    break;

                case NPCID.GoblinPeon:
                case NPCID.GoblinThief:
                case NPCID.GoblinWarrior:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.BuilderPotion, 4));
                    break;

                case NPCID.JungleBat:
                case NPCID.IceBat:
                case NPCID.Vampire:
                case NPCID.VampireBat:
                case NPCID.GiantFlyingFox:
                case NPCID.IlluminantBat:
                case NPCID.CaveBat:
                case NPCID.GiantBat:
                case NPCID.SporeBat:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.SonarPotion, 2));
                    break;

                case NPCID.Demon:
                case NPCID.VoodooDemon:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.WrathPotion, 1, 1, 2));
                    break;

                case NPCID.Hellbat:
                case NPCID.Lavabat:
                case NPCID.LavaSlime:
                    TimsConcoctionDrop(new CommonDrop(ItemID.RagePotion, 3, chanceNumerator: 2));
                    break;

                case NPCID.SnowFlinx:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.WarmthPotion, 1, 2, 3));
                    break;
                case NPCID.UndeadViking:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.WarmthPotion));
                    break;

                case NPCID.AngryBones:
                case NPCID.AngryBonesBig:
                case NPCID.AngryBonesBigHelmet:
                case NPCID.AngryBonesBigMuscle:
                case NPCID.HellArmoredBones:
                case NPCID.HellArmoredBonesMace:
                case NPCID.HellArmoredBonesSpikeShield:
                case NPCID.HellArmoredBonesSword:
                case NPCID.RustyArmoredBonesAxe:
                case NPCID.RustyArmoredBonesFlail:
                case NPCID.RustyArmoredBonesSword:
                case NPCID.RustyArmoredBonesSwordNoArmor:
                case NPCID.BlueArmoredBones:
                case NPCID.BlueArmoredBonesMace:
                case NPCID.BlueArmoredBonesNoPants:
                case NPCID.BlueArmoredBonesSword:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.TitanPotion, 2));
                    break;

                case NPCID.DarkCaster:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.PotionOfReturn, 1, 2, 3));
                    break;

                case NPCID.GiantShelly:
                case NPCID.GiantShelly2:
                case NPCID.GiantTortoise:
                case NPCID.IceTortoise:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.ThornsPotion, 1, 1, 2));
                    break;

                case NPCID.Zombie:
                case NPCID.BaldZombie:
                case NPCID.FemaleZombie:
                case NPCID.PincushionZombie:
                case NPCID.SlimedZombie:
                case NPCID.TwiggyZombie:
                case NPCID.ZombiePixie:
                case NPCID.ZombieSuperman:
                case NPCID.ZombieSweater:
                case NPCID.ZombieXmas:
                case NPCID.SwampZombie:
                case NPCID.SmallSwampZombie:
                case NPCID.BigSwampZombie:
                case NPCID.TorchZombie:
                case NPCID.ArmedZombie:
                case NPCID.ArmedZombieCenx:
                case NPCID.ArmedTorchZombie:
                case NPCID.ArmedZombiePincussion:
                case NPCID.ArmedZombieSlimed:
                case NPCID.ArmedZombieSwamp:
                case NPCID.ArmedZombieTwiggy:
                case NPCID.ZombieDoctor:
                    //TimsConcoctionDrop(ItemDropRule.Common(ItemID.StinkPotion, 5));
                    break;

                case NPCID.WalkingAntlion:
                case NPCID.GiantWalkingAntlion:
                case NPCID.FlyingAntlion:
                case NPCID.GiantFlyingAntlion:
                case NPCID.Tumbleweed:
                case NPCID.DesertBeast:
                    TimsConcoctionDrop(new CommonDrop(ItemID.SwiftnessPotion, 3, chanceNumerator: 2));
                    break;

                case NPCID.Antlion:
                case NPCID.TombCrawlerHead:
                case NPCID.DuneSplicerHead:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.HunterPotion, 1, 1, 2));
                    break;

                case NPCID.WallCreeper:
                case NPCID.WallCreeperWall:
                case NPCID.BlackRecluse:
                case NPCID.BlackRecluseWall:
                case NPCID.JungleCreeper:
                case NPCID.JungleCreeperWall:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.TrapsightPotion));
                    break;

                case NPCID.FireImp:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.InfernoPotion));
                    break;

                case NPCID.Harpy:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.CalmingPotion));
                    break;

                case NPCID.Crab:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.CratePotion));
                    break;

                case NPCID.FlyingFish:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.FishingPotion));
                    break;

                case NPCID.IceSlime:
                case NPCID.SpikedIceSlime:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.WaterWalkingPotion));
                    break;

                case NPCID.Piranha:
                case NPCID.Arapaima:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.GillsPotion));
                    break;

                case NPCID.BloodZombie:
                case NPCID.ZombieMerman:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.RegenerationPotion, 1));
                    break;

                case NPCID.GoblinShark:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.RegenerationPotion, 1, 3, 5));
                    break;

                case NPCID.UmbrellaSlime:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.FeatherfallPotion, 1, 1, 3));
                    break;

                case NPCID.Drippler:
                case NPCID.EyeballFlyingFish:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.HeartreachPotion, 1));
                    break;

                case NPCID.BloodEelHead:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.HeartreachPotion, 1, 3, 5));
                    break;

                case NPCID.GiantWormHead:
                case NPCID.DiggerHead:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.WormholePotion, 1, 1, 3));
                    break;

                case NPCID.GreekSkeleton:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.AmmoReservationPotion, 1, 1, 3));
                    break;

                case NPCID.GraniteFlyer:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.IronskinPotion, 1, 1, 2));
                    break;
                case NPCID.GraniteGolem:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.EndurancePotion, 1, 1, 2));
                    break;

                case NPCID.RockGolem:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.IronskinPotion, 1, 3, 8));
                    break;

                case NPCID.Shark:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.FlipperPotion, 1, 1, 3));
                    break;

                case NPCID.GoblinArcher:
                    TimsConcoctionDrop(new CommonDrop(ItemID.ArcheryPotion, 3, 1, 2, 2));
                    break;

                case NPCID.GoblinSorcerer:
                    TimsConcoctionDrop(new CommonDrop(ItemID.ManaRegenerationPotion, 1, 2, 2));
                    break;

                case NPCID.PinkJellyfish:
                case NPCID.BlueJellyfish:
                case NPCID.GreenJellyfish:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.ShinePotion));
                    break;

                case NPCID.Salamander:
                case NPCID.Salamander2:
                case NPCID.Salamander3:
                case NPCID.Salamander4:
                case NPCID.Salamander5:
                case NPCID.Salamander6:
                case NPCID.Salamander7:
                case NPCID.Salamander8:
                case NPCID.Salamander9:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.InvisibilityPotion, 1, 1, 2));
                    break;

                case NPCID.MotherSlime:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.SummoningPotion, 1, 3, 8));
                    break;

                case NPCID.Nymph:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.LovePotion, 1));
                    break;

                case NPCID.DoctorBones:
                case NPCID.Mimic:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.SpelunkerPotion, 1, 2, 3));
                    break;

                case NPCID.UndeadMiner:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.MiningPotion, 1, 2, 3));
                    break;

                case NPCID.Tim:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.MagicPowerPotion, 1, 2, 3));
                    break;

                case NPCID.BoneSerpentHead:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.ObsidianSkinPotion, 1, 3, 8));
                    break;


                case NPCID.Gnome:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.LuckPotionLesser, 1, 2, 3));
                    break;
                case NPCID.DungeonSlime:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.LuckPotion, 1, 2, 3));
                    break;
                case NPCID.Clown:
                case NPCID.EnchantedSword:
                case NPCID.CrimsonAxe:
                case NPCID.CursedHammer:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.LuckPotionGreater, 1, 2, 3));
                    break;

                case NPCID.ChaosElemental:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.TeleportationPotion, 1, 2, 3));
                    break;

                case NPCID.RainbowSlime:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.RegenerationPotion, 1, 1, 3));
                    break;

                case NPCID.RuneWizard:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.MagicPowerPotion, 1, 2, 3));
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.ManaRegenerationPotion, 1, 2, 3));
                    break;

                case NPCID.GoblinSummoner:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.SummoningPotion, 1, 3, 8));
                    break;

                case NPCID.PirateCaptain:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.AmmoReservationPotion, 1, 3, 8));
                    break;

                case NPCID.WyvernHead:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.GravitationPotion, 1, 3, 8));
                    break;

                case NPCID.BigMimicJungle:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.RedPotion));
                    goto case NPCID.BigMimicCorruption;
                case NPCID.BigMimicCorruption:
                case NPCID.BigMimicCrimson:
                case NPCID.BigMimicHallow:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.LifeforcePotion, 1, 3, 8));
                    break;

                case NPCID.ManEater:
                case NPCID.Nutcracker:
                case NPCID.Parrot:
                    //TimsConcoctionDrop(ItemDropRule.Common(ItemID.GenderChangePotion));
                    break;

                case NPCID.CorruptBunny:
                case NPCID.CrimsonBunny:
                case NPCID.CorruptGoldfish:
                case NPCID.CrimsonGoldfish:
                case NPCID.CorruptPenguin:
                case NPCID.CrimsonPenguin:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.BiomeSightPotion, 2));
                    break;

                case NPCID.Maggot:
                case NPCID.MaggotZombie:
                    TimsConcoctionDrop(ItemDropRule.Common(ItemID.StinkPotion, 3));
                    break;

                default:
                    if (npc.ModNPC != null)
                    {
                        if (npc.type == ModContent.NPCType<CactusMimic>())
                            TimsConcoctionDrop(ItemDropRule.Common(ItemID.ThornsPotion, 1, 1, 2));
                    }
                    break;
            }
            #endregion
            //if (npc.ModNPC == null || npc.ModNPC.Mod is FargowiltasSouls) //not for other mods
            //{
            int allowedRecursionDepth = 10;
            void AddDrop(IItemDropRule dropRule)
            {
                if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)
                {
                    LeadingConditionRule noTwin = new(new Conditions.MissingTwin());
                    noTwin.OnSuccess(dropRule);
                    npcLoot.Add(noTwin);
                }
                else if (npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsTail)
                {
                    LeadingConditionRule lastEater = new(new Conditions.LegacyHack_IsABoss());
                    lastEater.OnSuccess(dropRule);
                    npcLoot.Add(lastEater);
                }
                else
                {
                    npcLoot.Add(dropRule);
                }
            }

            void CheckMasterDropRule(IItemDropRule dropRule)
            {
                if (--allowedRecursionDepth > 0)
                {
                    if (dropRule != null && dropRule.ChainedRules != null)
                    {
                        foreach (IItemDropRuleChainAttempt chain in dropRule.ChainedRules)
                        {
                            if (chain != null && chain.RuleToChain != null)
                                CheckMasterDropRule(chain.RuleToChain);
                        }
                    }


                    if (dropRule is DropBasedOnMasterMode dropBasedOnMasterMode)
                    {
                        if (dropBasedOnMasterMode != null && dropBasedOnMasterMode.ruleForMasterMode != null)
                            CheckMasterDropRule(dropBasedOnMasterMode.ruleForMasterMode);
                        //if (dropBasedOnMasterMode.ruleForMasterMode is CommonDrop masterDrop)
                        //{
                        //    IItemDropRule emodeDropRule = ItemDropRule.ByCondition(
                        //        new EModeNotMasterDropCondition(),
                        //        masterDrop.itemId,
                        //        masterDrop.chanceDenominator,
                        //        masterDrop.amountDroppedMinimum,
                        //        masterDrop.amountDroppedMaximum,
                        //        masterDrop.chanceNumerator
                        //    );
                        //    npcLoot.Add(emodeDropRule);
                        //}
                    }
                }
                allowedRecursionDepth++;

                //if (dropRule is CommonDrop drop)
                //{
                if (dropRule is ItemDropWithConditionRule itemDropWithCondition && itemDropWithCondition.condition is Conditions.IsMasterMode)
                {
                    IItemDropRule emodeDropRule = ItemDropRule.ByCondition(
                        new EModeNotMasterDropCondition(),
                        itemDropWithCondition.itemId,
                        itemDropWithCondition.chanceDenominator,
                        itemDropWithCondition.amountDroppedMinimum,
                        itemDropWithCondition.amountDroppedMaximum,
                        itemDropWithCondition.chanceNumerator
                    );
                    //itemDropWithCondition.OnFailedConditions(emodeDropRule, true);
                    AddDrop(emodeDropRule);
                }
                else if (dropRule is DropPerPlayerOnThePlayer dropPerPlayer && dropPerPlayer.condition is Conditions.IsMasterMode)
                {
                    IItemDropRule emodeDropRule = ItemDropRule.ByCondition(
                        new EModeNotMasterDropCondition(),
                        dropPerPlayer.itemId,
                        dropPerPlayer.chanceDenominator,
                        dropPerPlayer.amountDroppedMinimum,
                        dropPerPlayer.amountDroppedMaximum,
                        dropPerPlayer.chanceNumerator
                    );
                    //dropPerPlayer.OnFailedConditions(emodeDropRule, true);
                    AddDrop(emodeDropRule);
                }
                //}
            }

            foreach (IItemDropRule rule in npcLoot.Get())
            {
                CheckMasterDropRule(rule);
            }
            //}
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (WorldSavingSystem.EternityMode && BeetleOffenseAura)
            {
                modifiers.FinalDamage *= 1.4f;
            }
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (WorldSavingSystem.EternityMode)
            {
                if (BeetleDefenseAura)
                    modifiers.FinalDamage *= 0.75f;

                if (PaladinsShield)
                    modifiers.FinalDamage *= 0.5f;

                if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld && ((npc.boss || LumUtils.AnyBosses()) && FargoSoulsGlobalNPC.boss.IsWithinBounds(Main.maxNPCs) && npc.Distance(Main.npc[FargoSoulsGlobalNPC.boss].Center) < 3000))
                    modifiers.FinalDamage *= 0.9f;
            }

            //normal damage calc
            base.ModifyIncomingHit(npc, ref modifiers);
        }

        //make aura enemies display them one day(tm)
        /*public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {
            for (int i = 0; i < auraDebuffs.Count; i++)
            {
                Texture2D buffIcon = Main.buffTexture[auraDebuffs[i]];
                Color buffColor = drawColor * 0.5f;
                Vector2 drawPos = npc.Top;
                drawPos.Y -= 32f;
                float mid = auraDebuffs.Count / 2f - 0.5f;
                drawPos.X -= 32f * (i - mid);
                Main.EntitySpriteDraw(buffIcon, drawPos - Main.screenPosition + new Vector2(0f, npc.gfxOffY), buffIcon.Bounds, buffColor, 0, buffIcon.Bounds.Size() / 2, 1f, SpriteEffects.None, 0);
            }
        }*/

        public static void Horde(NPC npc, int size)
        {

            if (npc == null || !npc.active)
            {
                return;
            }
            int repeatTries = 50;

            for (int i = 0; i < size; i++)
            {
                Vector2 pos = new(npc.Center.X + Main.rand.NextFloat(-2f, 2f) * npc.width, npc.Center.Y);

                if (Collision.SolidCollision(pos, npc.width, npc.height))
                {
                    if (repeatTries > 0) //retry up to the max attempts
                    {
                        repeatTries -= 1;
                        i -= 1;
                    }
                    continue;
                }

                if (FargoSoulsUtil.HostCheck)
                {
                    int j = NPC.NewNPC(npc.GetSource_FromAI(), (int)pos.X + npc.width / 2, (int)pos.Y + npc.height / 2, npc.type);
                    if (j != Main.maxNPCs)
                    {
                        NPC newNPC = Main.npc[j];
                        if (newNPC != null && newNPC.active && newNPC.type == npc.type) //super mega safeguard check
                        {
                            newNPC.velocity = Vector2.UnitX.RotatedByRandom(2 * Math.PI) * 5f;
                            newNPC.FargoSouls().CanHordeSplit = false;
                            /*
                            if (newNPC.TryGetGlobalNPC(out EModeNPCBehaviour globalNPC))
                            {
                                globalNPC.FirstTick = false;
                            }
                            */
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, j);
                        }
                    }
                }
            }
        }

        //for backwards compat
        public static void Aura(NPC npc, float distance, int buff, bool reverse = false, int dustid = DustID.GoldFlame, Color color = default)
        {
            Aura(npc, distance, reverse, dustid, color, buff);
        }

        public static void Aura(NPC npc, float distance, bool reverse = false, int dustid = -1, Color color = default, params int[] buffs)
        {
            Player p = Main.LocalPlayer;

            //if (FargowiltasSouls.Instance.MasomodeEXLoaded) distance *= reverse ? 0.5f : 2f;
            if (dustid != -1)
                FargoSoulsUtil.AuraDust(npc, distance, dustid, color, reverse);

            if (buffs.Length == 0 || buffs[0] < 0)
                return;

            //works because buffs are client side anyway :ech:
            float range = npc.Distance(p.Center);
            if (p.Alive() && (reverse ? range > distance && range < Math.Max(3000f, distance * 2) : range < distance))
            {
                foreach (int buff in buffs)
                {
                    FargoSoulsUtil.AddDebuffFixedDuration(p, buff, 2);
                }

                if (buffs.Contains(ModContent.BuffType<HexedBuff>()))
                {
                    p.FargoSouls().HexedInflictor = npc.whoAmI;
                }
            }
        }

        /*private void Shoot(NPC npc, int delay, float distance, int speed, int proj, int dmg, float kb, bool hostile = false, int dustID = -1)
        {
            int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
            if (t == -1)
                return;

            Player player = Main.player[t];
            //npc facing player target or if already started attack
            if (player.active && !player.dead && npc.direction == (Math.Sign(player.position.X - npc.position.X)) || Stop > 0)
            {
                //start the pause
                if (delay != 0 && Stop == 0 && npc.Distance(player.Center) < distance)
                {
                    Stop = delay;

                    //dust ring
                    if (dustID != -1)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            Vector2 vector6 = Vector2.UnitY * 5f;
                            vector6 = vector6.RotatedBy((i - (20 / 2 - 1)) * 6.28318548f / 20) + npc.Center;
                            Vector2 vector7 = vector6 - npc.Center;
                            int d = Dust.NewDust(vector6 + vector7, 0, 0, dustID);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].velocity = vector7;
                            Main.dust[d].scale = 1.5f;
                        }
                    }

                }
                //half way through start attack
                else if (delay == 0 || Stop == delay / 2)
                {
                    Vector2 velocity = Vector2.Zero;

                    if (npc.Distance(player.Center) < distance || delay != 0)
                    {
                        velocity = Vector2.Normalize(player.Center - npc.Center) * speed;
                    }

                    if (velocity != Vector2.Zero)
                    {
                        int p = Projectile.NewProjectile(npc.Center, velocity, proj, dmg, kb, Main.myPlayer);
                        if (p < 1000)
                        {
                            if (hostile)
                            {
                                Main.projectile[p].friendly = false;
                                Main.projectile[p].hostile = true;
                            }
                        }

                        Counter[0] = 0;
                    } 
                }
            }
        }*/
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.PreDraw(npc, spriteBatch, screenPos, drawColor);

            if (BeetleTimer > 0)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

                int type = BeetleUtilAura ? ItemID.VioletDye : BeetleOffenseAura ? ItemID.RedDye : ItemID.CyanDye;
                ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(type);
                shader.Apply(npc, new Terraria.DataStructures.DrawData?());
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            bool ret = base.CanHitPlayer(npc, target, ref cooldownSlot);
            if (!WorldSavingSystem.EternityMode)
                return ret;
            if (npc.type is NPCID.Sharkron or NPCID.Sharkron2)
            {
                int halfwidth = npc.width / 2;
                Vector2 dir = npc.velocity.SafeNormalize(Vector2.Zero);
                if (!Collision.CheckAABBvLineCollision(target.position, target.Size, npc.Center - dir * halfwidth, npc.Center + dir * halfwidth))
                    return false;
            }
            return ret;
        }
        public override bool? CanFallThroughPlatforms(NPC npc)
        {
            if (BeingSuckedByDread) return true;
            return base.CanFallThroughPlatforms(npc);
        }
        public static void CustomReflect(NPC npc, int dustID, int ratio = 1)
        {
            float distance = 2f * 16;

            Main.projectile.Where(x => x.active && x.friendly && !FargoSoulsUtil.IsSummonDamage(x, false)).ToList().ForEach(x =>
            {
                if (Vector2.Distance(x.Center, npc.Center) <= distance)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int dustId = Dust.NewDust(new Vector2(x.position.X, x.position.Y + 2f), x.width, x.height + 5, dustID, x.velocity.X * 0.2f, x.velocity.Y * 0.2f, 100, default, 1.5f);
                        Main.dust[dustId].noGravity = true;
                    }

                    // Set ownership
                    x.hostile = true;
                    x.friendly = false;
                    x.owner = Main.myPlayer;
                    x.damage /= ratio;

                    // Turn around
                    x.velocity *= -1f;

                    // Flip sprite
                    if (x.Center.X > npc.Center.X * 0.5f)
                    {
                        x.direction = 1;
                        x.spriteDirection = 1;
                    }
                    else
                    {
                        x.direction = -1;
                        x.spriteDirection = -1;
                    }

                    //x.netUpdate = true;
                }
            });
        }
    }
}
