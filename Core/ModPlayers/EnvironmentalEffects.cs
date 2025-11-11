using Fargowiltas.Content.Buffs;
using Fargowiltas.Content.Projectiles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Projectiles.Eternity.Environment;
using FargowiltasSouls.Content.Tiles;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace FargowiltasSouls.Core.ModPlayers
{
    public partial class EModePlayer : ModPlayer
    {
        public int MasomodeFreezeTimer;
        public int MasomodeSpaceBreathTimer;
        public int LightningCounter;
        public int LightLevelCounter;
        public int HallowFlipCheckTimer;


        public override void PreUpdate()
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            if (!Player.Alive())
                return;

            if (LumUtils.AnyBosses())
                return;

            FargoSoulsPlayer fargoSoulsPlayer = Player.FargoSouls();

            Tile currentTile = getPlayerTile();

            if (!NPC.downedBoss3 && Player.ZoneDungeon && !NPC.AnyNPCs(NPCID.DungeonGuardian) && !Main.drunkWorld)
            {
                NPC.SpawnOnPlayer(Player.whoAmI, NPCID.DungeonGuardian);
            }

            if (WorldUpdatingSystem.CorruptWaterTimer > 0 && WorldUpdatingSystem.CorruptWaterTimer % 5 == 0) //dust interval
            {
                EvilWaterDust(DustID.CursedTorch);
            }

            if (WorldUpdatingSystem.CrimsonWaterTimer > 0 && WorldUpdatingSystem.CrimsonWaterTimer % 5 == 0) //dust interval
            {
                EvilWaterDust(DustID.IchorTorch);
            }

            //water biome effects
            if (WaterWet)
            {
                //quicksand alpha, theres literally no water in ug... unless?
                if (Player.ZoneUndergroundDesert)
                {
                    //    FargoSoulsUtil.AddDebuffFixedDuration(Player, BuffID.Shimmer, 2);
                }

                //make ichor proj do this?
                if (Player.ZoneCrimson && WorldUpdatingSystem.CrimsonWaterTimer > 0)
                {
                    FargoSoulsUtil.AddDebuffFixedDuration(Player, BuffID.Ichor, 300);
                }

                //make eater fireballs do this
                if (Player.ZoneCorrupt && WorldUpdatingSystem.CorruptWaterTimer > 0)
                {
                    FargoSoulsUtil.AddDebuffFixedDuration(Player, BuffID.CursedInferno, 60);
                }
            }

            // Pure Heart-affected biome debuffs
            if (Player.ZoneDesert && !fargoSoulsPlayer.PureHeart)
            {
                //DesertDebuffs(currentTile);
            }

            if (Player.ZoneSnow && (Player.ZoneDirtLayerHeight || Player.ZoneRockLayerHeight))
            {
                SpawnIcicles(); //pheart makes deal no dmg
            }

            if (Player.ZoneJungle && !fargoSoulsPlayer.PureHeart)
            {
                JungleStorming();

                //reduce max storm
                //while storming ... spawn rates up
                //certain enemies go crazy
            }

            if (Player.ZoneCorrupt && !fargoSoulsPlayer.PureHeart)
            {
                FargoSoulsUtil.AddDebuffFixedDuration(Player, BuffID.Darkness, 2);
            }

            if (Player.ZoneHallow)
            {
                HallowedIlluminated(); //pheart prevents dr reduction
            }

            /*if (Player.ZoneUnderworldHeight && !fargoSoulsPlayer.PureHeart)
            {
                UnderworldFire();
            }*/

            if (Player.Center.ToTileCoordinates().Y <= (Main.worldSurface * 0.25) && !fargoSoulsPlayer.PureHeart)
            {
                SpaceBreathMeter();
            }

            if (Main.raining)
            {
                RainLightning(currentTile); //pheart makes deal no dmg
            }

            if (Main.bloodMoon && !fargoSoulsPlayer.PureHeart)
            {
                //Player.AddBuff(BuffID.WaterCandle, 2);
            }

            //boss environs
            //deerclops
            if (!NPC.downedDeerclops && Player.ZoneRockLayerHeight && Player.ZoneSnow && !LumUtils.AnyBosses() && !fargoSoulsPlayer.PureHeart)
            {
                //DeerclopsHands();
            }

            // hallow lifelight sparks
            if (!WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.Lifelight] && Player.ZoneHallow && Player.ZoneRockLayerHeight && !LumUtils.AnyBosses() && !fargoSoulsPlayer.PureHeart)
            {
                //LifelightSparkles();
            }

            //other stuff not prevent by pure heart
            if (Player.ZoneMeteor)
            {
                MeteorFallenStars();
            }

            //TryBouncingBlocks();
        }

        

        private void EvilWaterDust(int dustId)
        {
            return;
            //search around player for water, add dust
            Vector2 playerPos = Player.Center;
            int radius = 1000;

            for (int x = -radius; x <= radius; x += 2)
            {
                for (int y = -radius; y <= radius; y += 2)
                {
                    int xPosition = (int)(x + playerPos.X / 16.0f);
                    int yPosition = (int)(y + playerPos.Y / 16.0f);

                    if (xPosition < 0 || xPosition >= Main.maxTilesX || yPosition < 0 || yPosition >= Main.maxTilesY)
                        continue;

                    Tile tile = Main.tile[xPosition, yPosition];

                    // Circle
                    if (x * x + y * y <= radius)
                    {
                        if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Water)
                        {
                            Vector2 dustPos = new Vector2(xPosition, yPosition).ToWorldCoordinates();

                            Dust.NewDust(dustPos, 22, 22, dustId, 0.0f, 0.0f, 120);
                        }
                    }
                }
            }
        }

        private void SpawnIcicles()
        {
            if (!FargoSoulsUtil.HostCheck)
                return;

            if (Player.townNPCs >= 2f)
                return;

            int maxIcicles = 25;
            int spawningRange = 60;
            int airNeeded = 5;
            int icicleDamage = 20;

            //icicle spawning 
            if (Main.rand.NextBool(30) && LumUtils.CountProjectiles([ModContent.ProjectileType<FallingIcicle>()]) < maxIcicles)
            {

                Vector2 playerPos = Player.Center;
                bool icicleSpawned = false;
                int attempts = 300;
                while (!icicleSpawned && attempts > 0)
                {
                    attempts--;
                    int x = Main.rand.Next(4, spawningRange) * (Main.rand.NextBool() ? 1 : -1);
                    int y = Main.rand.Next(4, spawningRange) * (Main.rand.NextBool() ? 1 : -1);

                    int xPosition = (int)(x + playerPos.X / 16.0f);
                    int yPosition = (int)(y + playerPos.Y / 16.0f);

                    if (xPosition < 0 || xPosition >= Main.maxTilesX || yPosition < 0 || yPosition >= Main.maxTilesY)
                        continue;

                    Tile tile = Main.tile[xPosition, yPosition];

                    if (tile == null)
                        continue;

                    if ((tile.TileType == TileID.IceBlock || tile.TileType == TileID.BreakableIce || tile.TileType == TileID.SnowBlock) && tile.BlockType == BlockType.Solid)
                    {
                        //check if tile has open air beneath it 
                        bool fail = false;

                        for (int i = 1; i <= airNeeded; i++)
                        {
                            int yPosition2 = (int)(y + i + playerPos.Y / 16.0f);
                            Tile tile2 = Main.tile[xPosition, yPosition2];

                            if (tile2.TileType != 0)
                            {
                                fail = true;
                                break;
                            }
                        }

                        if (fail)
                        {
                            continue; ;
                        }

                        //spawn the icicle
                        yPosition = (int)(y + 1 + playerPos.Y / 16.0f);
                        Vector2 spawnPos = new Vector2(xPosition, yPosition).ToWorldCoordinates();
                        //fiddle to line up to tile
                        spawnPos.X -= 0f;
                        spawnPos.Y += 5f;

                        bool icicleNearby = false;
                        foreach (Projectile p in Main.ActiveProjectiles)
                        {
                            if (p.type == ModContent.ProjectileType<FallingIcicle>() && p.position.Distance(spawnPos) < 16 * 3)
                            {
                                icicleNearby = true;
                                break;
                            }
                        }

                        if (!icicleNearby)
                        {
                            Projectile icicle = FargoSoulsUtil.NewProjectileDirectSafe(Player.GetSource_NaturalSpawn(), spawnPos, Vector2.Zero, ModContent.ProjectileType<FallingIcicle>(), icicleDamage, 1, Main.myPlayer);
                            icicleSpawned = true;
                        }
                    }
                }
            }
        }

        private void FallDamageDebuff()
        {
            //falling gives you dazed. wings save you
            /*if (Player.velocity.Y == 0f && Player.wingsLogic == 0 && !Player.noFallDmg && !Player.ghost && !Player.dead)
            {
                int num21 = 25;
                num21 += Player.extraFall;
                int num22 = (int)(Player.position.Y / 16f) - Player.fallStart;
                if (Player.mount.CanFly)
                {
                    num22 = 0;
                }
                if (Player.mount.Cart && Minecart.OnTrack(Player.position, Player.width, Player.height))
                {
                    num22 = 0;
                }
                if (Player.mount.Type == 1)
                {
                    num22 = 0;
                }
                Player.mount.FatigueRecovery();

                if (((Player.gravDir == 1f && num22 > num21) || (Player.gravDir == -1f && num22 < -num21)))
                {
                    Player.immune = false;
                    int dmg = (int)(num22 * Player.gravDir - num21) * 10;
                    if (Player.mount.Active)
                        dmg = (int)(dmg * Player.mount.FallDamage);

                    Player.Hurt(PlayerDeathReason.ByOther(0), dmg, 0);
                    Player.AddBuff(BuffID.Dazed, 120);
                }
                Player.fallStart = (int)(Player.position.Y / 16f);
            }*/
        }

        private void JungleStorming()
        {
            if (Main.netMode == NetmodeID.SinglePlayer && Player.ZoneOverworldHeight)
            {
                int day = 86400;
                int hour = day / 24;

                //rain increases if alredy raining
                if (Main.IsItRaining)
                {
                    if (Main.maxRaining < 0.9f && Main.windSpeedCurrent < 0.8f && Main.rand.NextBool(600))
                    {
                        //rain
                        Main.raining = true;
                        Main.maxRaining = Main.cloudAlpha = Math.Min(Main.maxRaining + 0.02f, 0.9f);
                        //wind
                        Main.windSpeedTarget = Main.windSpeedCurrent = Math.Min(Main.windSpeedCurrent + 0.02f, 0.8f);

                        if (Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendData(MessageID.WorldData);
                            Main.SyncRain();
                        }

                        //Main.NewText("storm increased.." + Main.maxRaining);
                    }
                }
                //rain increased chnce to start
                else if (WorldUpdatingSystem.rainCD == 0)
                {
                    if (Main.rand.NextBool(7200))
                    {
                        //rain
                        Main.rainTime = hour * 4;
                        Main.raining = true;
                        Main.maxRaining = Main.cloudAlpha = 0.02f;

                        //Main.NewText("rain started");

                        WorldUpdatingSystem.rainCD = 43200;// 1/2 day cooldown
                    }
                }

                if (Main.maxRaining == 0.9f)
                {
                    //rain full power
                }
            }
        }

        private void MeteorFallenStars()
        {
            //5x star rate
            Star.starfallBoost = 5;

            //manually spawn day stars during day
            if (Main.dayTime && FargoSoulsUtil.HostCheck)
            {
                int starProj = ModContent.ProjectileType<FallenStarDay>();

                for (int m = 0; m < Main.dayRate; m++)
                {
                    double num7 = (double)Main.maxTilesX / 4200.0;
                    num7 *= (double)Star.starfallBoost;
                    if (!((double)Main.rand.Next(8000) < 10.0 * num7))
                    {
                        continue;
                    }
                    int num8 = 12;

                    int randDist = Main.rand.Next(1, 200);
                    float posX = Player.position.X + (float)Main.rand.Next(-randDist, randDist + 1);
                    float posY = Main.rand.Next((int)((double)Main.maxTilesY * 0.05));
                    posY *= 16;
                    Vector2 position = new Vector2(posX, posY);
                    int num11 = -1;//whether or not star travels towards the player

                    if (!Collision.SolidCollision(position, 16, 16))
                    {
                        //Main.NewText("star spawn");

                        float speedX = Main.rand.Next(-100, 101);
                        float speedY = Main.rand.Next(200) + 100;
                        float num16 = (float)Math.Sqrt(speedX * speedX + speedY * speedY);
                        num16 = (float)num8 / num16;
                        speedX *= num16;
                        speedY *= num16;
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), position.X, position.Y, speedX, speedY, starProj, 100, 0f, Main.myPlayer, 0f, num11);
                    }
                }
            }
        }

        private void DesertDebuffs(Tile currentTile)
        {

            // Main.NewText("hi " + Player.ZoneDirtLayerHeight + " " + Player.ZoneRockLayerHeight);

            if ((Player.ZoneDirtLayerHeight || Player.ZoneRockLayerHeight) && Player.wet)
            {
                Player.AddBuff(ModContent.BuffType<QuicksandBuff>(), 60);
            }

            /*
            if (Player.ZoneOverworldHeight && currentTile.WallType == WallID.None)
            {
                if (Main.dayTime)
                {
                    if (!Player.wet && Main.IsItRaining && !hasUmbrella())
                    {
                        FargoSoulsUtil.AddDebuffFixedDuration(Player, BuffID.Weak, 2);
                    }
                }
                else
                {
                    if (!Player.resistCold && !Player.HasBuff(BuffID.Campfire) && !Player.HasBuff<Semistation>() && !Player.HasBuff<Omnistation>() && !Main.SceneMetrics.HasCampfire && !ItemID.Sets.Torches[Player.HeldItem.type])
                    {
                        FargoSoulsUtil.AddDebuffFixedDuration(Player, BuffID.Chilled, 2);
                    }
                }
            }
            */
        }

        private void HallowedIlluminated()
        {
            if (Player.ZoneRockLayerHeight)
            {
                if (++HallowFlipCheckTimer > 6) //reduce computation
                {
                    HallowFlipCheckTimer = 0;

                    float playerAbove = Player.Center.Y - 16 * 50;
                    float playerBelow = Player.Center.Y + 16 * 50;
                    if (playerAbove / 16 < Main.maxTilesY && playerBelow / 16 < Main.maxTilesY
                        && !Collision.CanHitLine(new Vector2(Player.Left.X, playerAbove), 0, 0, new Vector2(Player.Left.X, playerBelow), 0, 0)
                        && !Collision.CanHitLine(new Vector2(Player.Right.X, playerAbove), 0, 0, new Vector2(Player.Right.X, playerBelow), 0, 0))
                    {
                        if (!Main.wallHouse[Framing.GetTileSafely(Player.Center).WallType]
                            && !Main.wallHouse[Framing.GetTileSafely(Player.TopLeft).WallType]
                            && !Main.wallHouse[Framing.GetTileSafely(Player.TopRight).WallType]
                            && !Main.wallHouse[Framing.GetTileSafely(Player.BottomLeft).WallType]
                            && !Main.wallHouse[Framing.GetTileSafely(Player.BottomRight).WallType])
                        {
                            Player.AddBuff(ModContent.BuffType<HallowIlluminatedBuff>(), 90);
                        }
                    }
                }
            }
        }

        /*private void UnderworldFire()
        {
            bool anyAshwoodEffect = Player.HasEffect<AshWoodEffect>() || Player.HasEffect<ObsidianEffect>();

            if (anyAshwoodEffect || !(Player.fireWalk || Player.lavaMax > 0))
                FargoSoulsUtil.AddDebuffFixedDuration(Player, BuffID.OnFire, 2);
        }*/

        private void RainLightning(Tile currentTile)
        {
            if (!FargoSoulsUtil.HostCheck)
                return;

            if (Player.ZoneOverworldHeight
                && !hasUmbrella() && currentTile.WallType == WallID.None)
            {
                Player.AddBuff(BuffID.Wet, 2);

                if (Main.IsItStorming && !Player.ZoneSnow && !Player.ZoneSandstorm)
                    LightningCounter++;

                int lighntningMinSeconds = WorldSavingSystem.MasochistModeReal ? 10 : 17;
                if (LightningCounter >= LumUtils.SecondsToFrames(lighntningMinSeconds))
                {
                    Point tileCoordinates = Player.Top.ToTileCoordinates();

                    tileCoordinates.X += Main.rand.Next(-25, 25);
                    tileCoordinates.Y -= Main.rand.Next(4, 8);


                    bool foundMetal = false;
                    if (WorldSavingSystem.MasochistModeReal)
                        foundMetal = true;

                    /* TODO: make this work
                    for (int x = -5; x < 5; x++)
                    {
                        for (int y = -5; y < 5; y++)
                        {
                            int testX = tileCoordinates.X + x;
                            int testY = tileCoordinates.Y + y;
                            Tile tile = Main.tile[testX, testY];
                            if (IronTiles.Contains(tile.TileType) ||IronTiles.Contains(tile.WallType))
                            {
                                foundMetal = true;
                                tileCoordinates.X = testX;
                                tileCoordinates.Y = testY;
                                Main.NewText("found metal");
                                break;
                            }
                        }
                    }
                    */

                    if (LumUtils.AnyBosses() && !WorldSavingSystem.MasochistModeReal)
                    {
                        LightningCounter = 0;
                    }
                    else if (Main.rand.NextBool(300) || foundMetal)
                    {
                        //tends to spawn in ceilings if the Player goes indoors/underground


                        //for (int index = 0; index < 10 && !WorldGen.SolidTile(tileCoordinates.X, tileCoordinates.Y) && tileCoordinates.Y > 10; ++index) 
                        //    tileCoordinates.Y -= 1;

                        float ai1 = Player.Center.Y;
                        LightningCounter = 0;
                        int projType = ModContent.ProjectileType<RainLightning>();
                        Vector2 pos = new(tileCoordinates.X * 16 + 8, tileCoordinates.Y * 16 + 17 - 900);

                        int damage = (Main.hardMode ? 120 : 60) / 4;
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, projType, damage, 2f, Main.myPlayer, Vector2.UnitY.ToRotation(), ai1);
                    }
                }
            }
        }

        private Tile getPlayerTile()
        {
            Vector2 tileCenter = Player.Center;
            tileCenter.X /= 16;
            tileCenter.Y /= 16;
            Tile currentTile = Framing.GetTileSafely((int)tileCenter.X, (int)tileCenter.Y);

            return currentTile;
        }

        private void SpaceBreathMeter()
        {
            if (!Player.buffImmune[BuffID.Suffocation] && Player.whoAmI == Main.myPlayer)
            {
                bool immunity = !Player.armor[0].IsAir && (Player.armor[0].type == ItemID.FishBowl || Player.armor[0].type == ItemID.GoldGoldfishBowl);
                if (Player.accDivingHelm || Player.spaceGun)
                    immunity = true;

                bool inLiquid = Collision.DrownCollision(Player.position, Player.width, Player.height, Player.gravDir);
                if (!inLiquid && !immunity)
                {
                    Player.breath -= 3;
                    if (++MasomodeSpaceBreathTimer > 20)
                    {
                        MasomodeSpaceBreathTimer = 0;
                        Player.breath--;
                    }
                    if (Player.breath == 0)
                        SoundEngine.PlaySound(SoundID.Drown);
                    if (Player.breath <= 0)
                        Player.AddBuff(BuffID.Suffocation, 2);

                    if (Player.breath < -10) //don't stack far into negatives
                    {
                        Player.breath = -10;
                    }
                }
            }
        }

        private void DeerclopsHands()
        {
            if (!FargoSoulsUtil.HostCheck)
                return;

            if (Player.ZoneHallow)
                return;

            if (Player.townNPCs >= 2f)
                return;

            Color light = Lighting.GetColor(Player.Center.ToTileCoordinates());
            float lightLevel = light.R + light.G + light.B;

            if (lightLevel >= 500)
                return;

            LightLevelCounter++;
            if (LightLevelCounter > LumUtils.SecondsToFrames(20) && Main.rand.NextBool(600))
            {
                Vector2 pos = Player.Center + Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 270;
                bool failed = false;
                for (int i = 0; i < 200; i++) // try to find a dark spot
                {
                    pos = Player.Center + Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 270;
                    Color lightAtPos = Lighting.GetColor(pos.ToTileCoordinates());
                    float lightLevelAtPos = lightAtPos.R + lightAtPos.G + lightAtPos.B;
                    if (lightLevelAtPos < 500)
                        break;
                    if (i == 199) // failed
                        failed = true;
                }
                if (!failed)
                {
                    LightLevelCounter = 0;

                    int projType = ModContent.ProjectileType<DeerclopsDarknessHand>();

                    int damage = (Main.hardMode ? 120 : 60) / 4;
                    int p = Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, projType, damage, 2f, Main.myPlayer);
                    if (p.IsWithinBounds(Main.maxProjectiles))
                    {
                        Main.projectile[p].light = 1f;
                    }

                    Lighting.AddLight(pos, 1f, 1f, 1f);
                }
            }
        }

        private void LifelightSparkles()
        {
            if (!FargoSoulsUtil.HostCheck)
                return;

            if (Player.townNPCs >= 2f)
                return;

            Color light = Lighting.GetColor(Player.Center.ToTileCoordinates());
            float lightLevel = light.R + light.G + light.B;

            if (lightLevel < 500)
                return;

            LightLevelCounter++;
            if (LightLevelCounter > LumUtils.SecondsToFrames(10) && Main.rand.NextBool(300))
            {
                LightLevelCounter = 0;
                Vector2 pos = Player.Center;
                int projType = ModContent.ProjectileType<LifelightEnvironmentStar>();

                int damage = (Main.hardMode ? 120 : 60) / 4;
                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, projType, damage, 2f, Main.myPlayer, -120);
            }
        }




        private bool hasUmbrella()
        {
            return Player.HeldItem.type == ItemID.Umbrella || Player.HeldItem.type == ItemID.TragicUmbrella
                || Player.armor[0].type == ItemID.UmbrellaHat || Player.armor[0].type == ItemID.Eyebrella
                || Player.HasEffect<RainUmbrellaEffect>();
        }

        //make mushrooms bounce more t:m:
    private void TryBouncingBlocks()
        {
            Vector2 vector4 = Collision.TileCollision(Player.position, Player.velocity, Player.width, Player.height, false, false, (int)Player.gravDir);
            bool falling = false;
            if (vector4.Y > Player.gravity)
            {
                falling = true;
            }
            if (vector4.Y < 0f - Player.gravity)
            {
                falling = true;
            }



            bool num = !Player.wet && !Player.shimmering && (Player.velocity.Y >= 5f || Player.velocity.Y <= -5f);
            bool flag = false;
            bool flag2 = false;
            float num2 = 1f;
            if (!num)
            {
                return;
            }
            bool flag3 = false;
            int num3 = 0;
            foreach (Point touchedTile in Player.TouchedTiles)
            {
                Tile tile = Main.tile[touchedTile.X, touchedTile.Y];
                if (tile != null && tile.HasTile && !tile.IsActuated && (tile.TileType == ModContent.TileType<BouncyMushroomTile>()))
                {
                    flag3 = true;
                    num3 = touchedTile.Y;
                    break;
                }
            }
            if (!flag3)
            {
                return;
            }

            Main.NewText("yoooo");

            Player.velocity.Y *= -0.8f;
            if (Player.controlJump)
            {
                Player.velocity.Y = MathHelper.Clamp(Player.velocity.Y, -13f, 13f);
            }
            Player.position.Y = num3 * 16 - ((Player.velocity.Y < 0f) ? Player.height : (-16));
            Player.FloorVisuals(falling);
            if (flag2)
            {
                Vector2 vector = (Player.fullRotation - (float)Math.PI / 2f).ToRotationVector2();
                if (vector.Y > 0f)
                {
                    vector.Y *= -1f;
                }
                vector.Y = vector.Y * 0.5f + -0.5f;
                float num4 = 0f - vector.Y;
                if (num4 < 0f)
                {
                    num4 = 0f;
                }
                float num5 = num4 * 1.5f + 1f;
                float value = Math.Abs(Player.velocity.Y) * num5 * num2;
                value = MathHelper.Clamp(value, 2f, 16f);
                Player.velocity = vector * value;
                float num6 = 20f;
                Vector2 vector2 = Player.Center + (Player.fullRotation + (float)Math.PI / 2f).ToRotationVector2() * num6;
                vector2 = Player.Bottom;
                ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings
                {
                    PositionInWorld = vector2
                }, Player.whoAmI);
            }
            Player.velocity.Y = MathHelper.Clamp(Player.velocity.Y, -20f, 20f);
            if (Player.velocity.Y * Player.gravDir < 0f)
            {
                Player.fallStart = (int)Player.position.Y / 16;
            }
        }
    }
}
