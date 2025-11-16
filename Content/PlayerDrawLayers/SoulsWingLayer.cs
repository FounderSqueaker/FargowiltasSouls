using FargowiltasSouls.Assets.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.PlayerDrawLayers
{
    public class FlightMasterySoulWingLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(Terraria.DataStructures.PlayerDrawLayers.Wings);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            if (drawInfo.drawPlayer.dead || (drawInfo.drawPlayer.wings != EquipLoader.GetEquipSlot(Mod, "FlightMasterySoul", EquipType.Wings)))
                return false;

            if (drawInfo.shadow != 0)
                return false;

            return true;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Vector2 drawPosition = drawInfo.Position - Main.screenPosition + new Vector2(drawInfo.drawPlayer.width / 2,
                drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2) + Vector2.UnitY * 7f;

            drawPosition += new Vector2(-9, -7) * drawInfo.drawPlayer.Directions;

            Texture2D wingTexture = FargoAssets.GetTexture2D("Content/Items/Accessories/Souls", "FlightMasterySoulWing").Value;

            int frameCount = 8;

            Player player = drawInfo.drawPlayer;

            if (Main.gameMenu)
                player.FargoSouls().FlightSoulWingFrameX = 1;

            Rectangle frame = new((wingTexture.Width / 2) * player.FargoSouls().FlightSoulWingFrameX, wingTexture.Height / frameCount * drawInfo.drawPlayer.wingFrame, wingTexture.Width / 2, wingTexture.Height / frameCount);
            DrawData data = new(wingTexture, drawPosition.Floor(), frame, drawInfo.colorArmorBody, drawInfo.drawPlayer.bodyRotation, frame.Size() * 0.5f, 1f, drawInfo.playerEffect)
            {
                shader = drawInfo.cWings
            };
            drawInfo.DrawDataCache.Add(data);

        }
    }
    public class EternitySoulWingLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(Terraria.DataStructures.PlayerDrawLayers.Wings);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            if (drawInfo.drawPlayer.dead || drawInfo.drawPlayer.wings != EquipLoader.GetEquipSlot(Mod, "DimensionSoul", EquipType.Wings) && drawInfo.drawPlayer.wings != EquipLoader.GetEquipSlot(Mod, "EternitySoul", EquipType.Wings))
                return false;

            if (drawInfo.shadow != 0)
                return false;

            return true;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Vector2 drawPosition = drawInfo.Position - Main.screenPosition + new Vector2(drawInfo.drawPlayer.width / 2,
                drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2) + Vector2.UnitY * 7f;

            drawPosition += new Vector2(-9, -7) * drawInfo.drawPlayer.Directions;

            Texture2D wingTexture = FargoAssets.GetTexture2D("Content/Items/Accessories/Souls", "EternitySoulWing").Value;

            int frameCount = 6;
            Rectangle frame = new(0, wingTexture.Height / frameCount * drawInfo.drawPlayer.wingFrame, wingTexture.Width, wingTexture.Height / frameCount);
            DrawData data = new(wingTexture, drawPosition.Floor(), frame, drawInfo.colorArmorBody, drawInfo.drawPlayer.bodyRotation, frame.Size() * 0.5f, 1f, drawInfo.playerEffect)
            {
                shader = drawInfo.cWings
            };
            drawInfo.DrawDataCache.Add(data);
        }
    }
}
