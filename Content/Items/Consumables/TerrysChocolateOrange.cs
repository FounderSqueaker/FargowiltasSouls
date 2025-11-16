using FargowiltasSouls.Assets.Textures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace FargowiltasSouls.Content.Items.Consumables
{
    public class TerrysChocolateOrange : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Consumables", Name);

        public override void SetStaticDefaults()
        {
            // Done this way to prevent the FrameCounter from ever incrementing.
            // Ticks per second is set to 1 to prevent weird divide by zero error. ¯\_(ツ)_/¯
            DrawAnimationVertical drawAnim = new(1, 3);
            drawAnim.NotActuallyAnimating = true;

            Main.RegisterItemAnimation(Type, drawAnim);

            ItemID.Sets.FoodParticleColors[Type] = [
                Color.Brown,
                Color.DarkOrange,
                Color.RosyBrown
                ];

            Item.ResearchUnlockCount = 20;
            ItemID.Sets.IsFood[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(40, 36, BuffID.WellFed3, 36000);
            Item.UseSound = new SoundStyle("FargowiltasSouls/Assets/Sounds/Chippy/chippy") with { Variants = [1, 2, 3, 4, 5], PitchVariance = 1f};
            Item.value = Item.sellPrice(0, 0, 10, 0);
            Item.rare = ItemRarityID.Orange;
        }
    }
}
