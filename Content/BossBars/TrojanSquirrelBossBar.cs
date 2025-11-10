using FargowiltasSouls.Content.Bosses.Champions.Shadow;
using FargowiltasSouls.Content.Bosses.TrojanSquirrel;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace FargowiltasSouls.Content.BossBars
{
    public class TrojanSquirrelBossBar : ModBossBar
    {
        private int bossHeadIndex = -1;

        public Asset<Texture2D> Frame;

        public Asset<Texture2D> Bars;

        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            (Texture2D barTexture, Vector2 barCenter, _, _, Color iconColor, float life, float lifeMax, float shield, float shieldMax, float iconScale, bool showText, Vector2 textOffset) = drawParams;
          
            Frame = ModContent.Request<Texture2D>("FargowiltasSouls/Content/BossBars/TrojanSquirrelFrame", AssetRequestMode.ImmediateLoad);
            Bars = ModContent.Request<Texture2D>("FargowiltasSouls/Content/BossBars/TrojanSquirrelBars", AssetRequestMode.ImmediateLoad);

            int headTextureIndex = npc.GetBossHeadTextureIndex();
            if (headTextureIndex == -1)
            {
                NPCLoader.BossHeadSlot(npc, ref headTextureIndex);
                if (headTextureIndex == -1)
                    return false;
            }
            
            float lifeRatio = LumUtils.Saturate(life / lifeMax);

            Texture2D iconTexture = TextureAssets.NpcHeadBoss[headTextureIndex].Value;
            Rectangle iconFrame = iconTexture.Frame();

            Point barSize = new(456, 22);
            Point topLeftOffset = new(32, 24);          

            Rectangle bgFrame = Bars.Frame(verticalFrames: 3, frameY: 0);
            bgFrame.Width = Bars.Width();
            
            Rectangle barFrame = Bars.Frame(verticalFrames: 3, frameY: 0);
            barFrame.Y += topLeftOffset.Y;
            barFrame.Width = (int)(Bars.Width() * lifeRatio);
            barFrame.Height = barSize.Y;

            Rectangle barShieldFrame = Bars.Frame(verticalFrames: 3, frameY: 1);
            barShieldFrame.Y += topLeftOffset.Y;
            barShieldFrame.Width = 2;
            barShieldFrame.Height = barSize.Y;

            Rectangle tipShieldFrame = Bars.Frame(verticalFrames: 3, frameY: 1);
            tipShieldFrame.X += topLeftOffset.X;
            tipShieldFrame.Y += topLeftOffset.Y;
            tipShieldFrame.Width = (int)(Bars.Width() * lifeRatio);
            tipShieldFrame.Height = barSize.Y;

            Rectangle barPosition = Utils.CenteredRectangle(barCenter, barSize.ToVector2());
            Vector2 barTopLeft = barPosition.TopLeft();
            Vector2 topLeft = barTopLeft - topLeftOffset.ToVector2();

            int shieldScale = (int)(barSize.X * shield / shieldMax);
            shieldScale -= shieldScale % 2;

            int lifeScale = (int)(barSize.X * life / lifeMax);
            lifeScale -= lifeScale % 2;

            // Background.
            spriteBatch.Draw(Bars.Value, barTopLeft + new Vector2(6, 0), bgFrame, Color.White * 0.6f, 0f, Vector2.Zero, 1f, 0, 0f);

            //Vector2 stretchScale = new(scale / barFrame.Width, 1f);          
            Main.spriteBatch.Draw(Bars.Value, barTopLeft + new Vector2(6, 0), tipShieldFrame, Color.White, 0f, Vector2.Zero, 1, 0, 0f);
            Main.spriteBatch.Draw(Bars.Value, barTopLeft + new Vector2(4, 0), barFrame, Color.White, 0f, Vector2.Zero, 1, 0, 0f);

            // Bar itself (shield).
            if (shield > 0f)
            {
                spriteBatch.Draw(Bars.Value, barTopLeft, barShieldFrame, Color.White, 0f, Vector2.Zero, 1, 0, 0f);
                spriteBatch.Draw(Bars.Value, barTopLeft + new Vector2(shieldScale - 2, 0f), tipShieldFrame, Color.White, 0f, Vector2.Zero, 1f, 0, 0f);
            }

            // Frame.
            Rectangle frameFrame = Frame.Frame(verticalFrames: 1, frameY: 0);
            spriteBatch.Draw(Frame.Value, topLeft + new Vector2(16, 10), frameFrame, Color.White, 0f, Vector2.Zero, 1f, 0, 0f);

            // Icon.
            Vector2 iconOffset = new(22f, 20f);
            Vector2 iconSize = new(26f, 28f);
            Vector2 iconPosition = iconOffset + iconSize * 0.5f;
            spriteBatch.Draw(iconTexture, topLeft + iconPosition, iconFrame, iconColor, 0f, iconFrame.Size() / 2f, iconScale, 0, 0f);

            // Health text.
            if (BigProgressBarSystem.ShowText && showText)
            {
                if (shield > 0f)
                    BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, shield, shieldMax);
                else
                    BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, life, lifeMax);
            }
            return false;
        }
        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)/* tModPorter Note: life and shield current and max values are now separate to allow for hp/shield number text draw */
        {
            NPC npc = FargoSoulsUtil.NPCExists(info.npcIndexToAimAt);

            if (npc == null || !npc.active)
                return false;

            bossHeadIndex = npc.GetBossHeadTextureIndex();

            if (npc.ModNPC is TrojanSquirrel trojanSquirrel)
            {
                float lifeSegments = 0;
                if (trojanSquirrel.head != null)
                    lifeSegments += trojanSquirrel.head.life;
                if (trojanSquirrel.arms != null)
                    lifeSegments += trojanSquirrel.arms.life;
                life = npc.life + lifeSegments;
                lifeMax = npc.lifeMax + trojanSquirrel.lifeMaxHead + trojanSquirrel.lifeMaxArms;
            }

            return true;
        }
    }
}
