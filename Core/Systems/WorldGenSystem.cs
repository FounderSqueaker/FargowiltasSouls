using FargowiltasSouls.Content.Tiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace FargowiltasSouls.Core.Systems
{
    public class WorldGenSystem : ModSystem
    {
        public static bool TryPlacingStatue(int baseCheckX, int baseCheckY)
        {
            List<int> legalBlocks =
                [
                    TileID.Stone,
                    TileID.Grass,
                    TileID.Dirt,
                    TileID.SnowBlock,
                    TileID.IceBlock,
                    TileID.ClayBlock,
                    TileID.Mud,
                    TileID.JungleGrass,
                    TileID.Sand,
                    TileID.Ash,
                    TileID.AshGrass
                ];

            bool canPlaceStatueHere = true;
            for (int i = 0; i < 3; i++) //check no obstructing blocks
                for (int j = 0; j < 4; j++)
                {
                    Tile tile = Framing.GetTileSafely(baseCheckX + i, baseCheckY + j);
                    if (WorldGen.SolidOrSlopedTile(tile))
                    {
                        canPlaceStatueHere = false;
                        break;
                    }
                }
            for (int i = 0; i < 3; i++) //check for solid foundation
            {
                Tile tile = Framing.GetTileSafely(baseCheckX + i, baseCheckY + 4);
                if (!WorldGen.SolidTile(tile) || !legalBlocks.Contains(tile.TileType))
                {
                    canPlaceStatueHere = false;
                    break;
                }
            }

            if (canPlaceStatueHere)
            {
                for (int i = 0; i < 3; i++) //MAKE SURE nothing in the way
                    for (int j = 0; j < 4; j++)
                        WorldGen.KillTile(baseCheckX + i, baseCheckY + j);

                WorldGen.PlaceTile(baseCheckX, baseCheckY + 4, TileID.GrayBrick, false, true);
                WorldGen.PlaceTile(baseCheckX + 1, baseCheckY + 4, TileID.GrayBrick, false, true);
                WorldGen.PlaceTile(baseCheckX + 2, baseCheckY + 4, TileID.GrayBrick, false, true);
                Tile tile = Main.tile[baseCheckX, baseCheckY + 4]; tile.Slope = 0;
                tile = Main.tile[baseCheckX + 1, baseCheckY + 4]; tile.Slope = 0;
                tile = Main.tile[baseCheckX + 2, baseCheckY + 4]; tile.Slope = 0;
                WorldGen.PlaceTile(baseCheckX + 1, baseCheckY + 3, ModContent.TileType<MutantStatueGift>(), false, true);

                return true;
            }

            return false;
        }
        public override void PostWorldGen()
        {
            /*WorldGen.PlaceTile(Main.spawnTileX - 1, Main.spawnTileY, TileID.GrayBrick, false, true);
            WorldGen.PlaceTile(Main.spawnTileX, Main.spawnTileY, TileID.GrayBrick, false, true);
            WorldGen.PlaceTile(Main.spawnTileX + 1, Main.spawnTileY, TileID.GrayBrick, false, true);
            Main.tile[Main.spawnTileX - 1, Main.spawnTileY].slope(0);
            Main.tile[Main.spawnTileX, Main.spawnTileY].slope(0);
            Main.tile[Main.spawnTileX + 1, Main.spawnTileY].slope(0);
            WorldGen.PlaceTile(Main.spawnTileX, Main.spawnTileY - 1, ModContent.Find<ModTile>("Fargowiltas", "RegalStatueSheet"), false, true);*/


            int positionX = Main.spawnTileX - 1; //offset by dimensions of statue
            int positionY = Main.spawnTileY - 4;
            int checkUp = -30;
            int checkDown = 10;
            bool placed = false;
            for (int offsetX = -50; offsetX <= 50; offsetX++)
            {
                for (int offsetY = checkUp; offsetY <= checkDown; offsetY++)
                {
                    if (TryPlacingStatue(positionX + offsetX, positionY + offsetY))
                    {
                        placed = true;
                        WorldSavingSystem.PlacedMutantStatue = true;
                        break;
                    }
                }

                if (placed)
                    break;
            }

        }


        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int mushroomIndex = getStepIndex(tasks, "Mushroom Patches");
            tasks.Insert(mushroomIndex + 1, new PassLegacy("Bouncy Mushroom", addBouncyMushrooms));


            base.ModifyWorldGenTasks(tasks, ref totalWeight);
        }

        private int getStepIndex(List<GenPass> tasks, string name)
        {
            return tasks.FindIndex(genPass => genPass.Name.Equals(name));
        }

        public void addWaterToUGDesert()
        {
            Rectangle undergroundDesertLocation = GenVars.UndergroundDesertLocation;
            int x15 = undergroundDesertLocation.Center.X;
            int j4 = undergroundDesertLocation.Top - 10;


            //if (GenVars.UndergroundDesertLocation.Contains(new Point(num936, num937)))


            //    WorldGen.Pyramid(x15, j4);
        }

        private void addBouncyMushrooms(GenerationProgress progress, GameConfiguration configuration)
        {
            for (int i = 0; i < GenVars.numMushroomBiomes; i++)
            {
                Point mushroomPoint = GenVars.mushroomBiomesPosition[i];

                //pick some random points close by the center point
                for (int j = 0; j < 10; j++)
                {
                    int x = WorldGen.genRand.Next(mushroomPoint.X - 50, mushroomPoint.X + 50);
                    int y = WorldGen.genRand.Next(mushroomPoint.Y - 50, mushroomPoint.Y + 50);

                    //try to move to an open surface
                    Tile tile;

                    do
                    {
                        tile = Main.tile[x, y--];

                    } while (tile.HasTile);

                    y += 2;

                    if (!Main.tile[x, y].HasTile)
                    {
                        continue;
                    }

                    //ore spot
                    if (WorldGen.genRand.NextBool())
                    {
                        int strength = WorldGen.genRand.Next(5, 15);
                        int steps = WorldGen.genRand.Next(5, 15);

                        WorldGen.OreRunner(x, y, strength, steps, (ushort)ModContent.TileType<BouncyMushroomTile>());
                    }
                    else
                    {
                        int height = WorldGen.genRand.Next(2, 6);
                        int width = WorldGen.genRand.Next(3, 8);

                        createMarioMushroom(x, y, height, width);
                    }
                        
                }

                

                
            }

            
        }

        private void createMarioMushroom(int x, int y, int height, int width)
        {
            if (width % 2 == 0)
            {
                width++;
            }

            y = y - height;

            //surface
            for (int i = x - width / 2; i <= x + width / 2; i++)
            {
                addBlock(i, y, ModContent.TileType<BouncyMushroomTile>());
            }

            //stalk
            for (int i = y; i < y + height; i++)
            {
                addBlock(x, i, ModContent.TileType<BouncyMushroomTile>());
            }
        }

        private void addBlock(int x, int y, int type)
        {
            WorldGen.PlaceTile(x, y, type, mute: true, forced: true);

            //Main.tile[x, y].TileType = (ushort)type;
            //Main.tile[x, y].ClearBlockPaintAndCoating();
            //WorldGen.SquareTileFrame(x, y);

            //if (Main.netMode == 2)
            //{
            //    NetMessage.SendTileSquare(-1, x, y);
            //}
        }
    }
}
