using FargowiltasSouls.Assets.Textures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Tiles
{
    public class BouncyMushroomTile : ModTile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Tiles", Name);

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBouncy[Type] = true;

            VanillaFallbackOnModDeletion = TileID.MushroomBlock;

            AddMapEntry(Color.LightBlue);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        

        
    }
}
