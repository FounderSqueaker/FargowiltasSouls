using Fargowiltas.Content.UI;
using FargowiltasSouls.Content.UI;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items
{
    public class Masochist : SoulsItem
    {
        public override string Texture => "FargowiltasSouls/Content/Items/Placeholder";

        public string mode;
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public static bool CanPlayMaso => WorldSavingSystem.CanPlayMaso || Main.LocalPlayer.active && Main.LocalPlayer.FargoSouls().Toggler.CanPlayMaso;

        public override void SafeModifyTooltips(List<TooltipLine> tooltips)
        {
            base.SafeModifyTooltips(tooltips);

            if (CanPlayMaso)
            {
                TooltipLine line = new(Mod, "tooltip", Language.GetTextValue($"Mods.{Mod.Name}.Items.{Name}.ExtraTooltip"));
                tooltips.Add(line);
            }
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (CanPlayMaso)
            {
                if (line.Mod == "Terraria" && line.Name == "ItemName" || line.Mod == Mod.Name && line.Name == "tooltip")
                {
                    Main.spriteBatch.End(); //end and begin main.spritebatch to apply a shader
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Main.UIScaleMatrix);
                    ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.Text");
                    shader.TrySetParameter("mainColor", new Color(28, 222, 152));
                    shader.TrySetParameter("secondaryColor", new Color(168, 245, 228));
                    shader.Apply("PulseUpwards");
                    Utils.DrawBorderString(Main.spriteBatch, line.Text, new Vector2(line.X, line.Y), Color.White, 1); //draw the tooltip manually
                    Main.spriteBatch.End(); //then end and begin again to make remaining tooltip lines draw in the default way
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
                    return false;
                }
            }
            return true;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
            Item.material = false;
        }

        public override bool? UseItem(Player player)
        {
            if ((Main.netMode == NetmodeID.SinglePlayer || Main.myPlayer == player.whoAmI) && CanToggleEternity())
                FargoUIManager.Toggle<DifficultySelectionMenu>();
            return true;
        }
        public static bool CanToggleEternity() // exists for DLC compat
        {
            return !LumUtils.AnyBosses();
        }
    }
}