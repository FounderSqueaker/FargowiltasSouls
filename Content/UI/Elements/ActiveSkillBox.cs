using FargowiltasSouls.Core.AccessoryEffectSystem;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Graphics;
using Terraria.ID;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Audio;
using Terraria.Localization;
using FargowiltasSouls.Assets.Textures;
using Fargowiltas.Assets.Textures;
using Terraria.ModLoader.UI;

namespace FargowiltasSouls.Content.UI.Elements
{
    public abstract class ActiveSkillBox : UIPanel
    {
        public static DynamicSpriteFont Font => Terraria.GameContent.FontAssets.ItemStack.Value;
        public string Mod;
        public float Opacity = 1f;
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            CheckSlotElegible(Main.LocalPlayer);
            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }
        public abstract void CheckSlotElegible(Player player);
        public abstract AccessoryEffect GetEffect();
        public abstract void OnClicked();
        public virtual void PreDraw(SpriteBatch spriteBatch, Vector2 position) { }
        public virtual void ModifyTooltip(ref string tooltip) { }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            AccessoryEffect effect = GetEffect();
            base.DrawSelf(spriteBatch);
            CheckSlotElegible(Main.LocalPlayer);
            Vector2 position = GetDimensions().Position();
            PreDraw(spriteBatch, position);

            bool hovering = ContainsPoint(Main.MouseScreen);
            ActiveSkillBox held = ActiveSkillMenu.MouseHeldElement;
            bool notHoldingOther = held == null || held == this;
            if ((hovering || held == this) && (ActiveSkillMenu.MouseHoveredElement != this))
            {
                if (notHoldingOther)
                {
                    ActiveSkillMenu.MouseHoveredElement = this;
                    return;
                }
            }
            int item = -1;

            if (effect != null)
            {
                if (effect.ToggleItemType > 0)
                    item = effect.ToggleItemType;
                else if (effect.EffectItem(Main.LocalPlayer) != null && effect.EffectItem(Main.LocalPlayer).type > ItemID.None)
                    item = effect.EffectItem(Main.LocalPlayer).type;
            }
            Vector2 itemPos = position + new Vector2(9, 9);
            float scale = 1f;
            if (ActiveSkillMenu.MouseHeldElement == this)
            {
                scale = 1.2f * Main.cursorScale;
                itemPos = Main.MouseScreen - new Vector2(14, 14);

            }
            if (hovering && scale == 1 && notHoldingOther)
                scale = 1.2f;
            if (item > -1)
            {
                
                Vector2 scaleOffset = -Vector2.One * 10 * (scale - 1);
                Color color = Color.White;
                Utils.DrawBorderString(spriteBatch, $"[i:{item}]", itemPos + scaleOffset, color, scale);
                if (!Main.LocalPlayer.AccessoryEffects().Equipped(effect))
                {
                    Texture2D cross = FargoMutantAssets.UI.Toggler.Cross.Value;
                    Main.spriteBatch.Draw(cross, itemPos + new Vector2(-3, -12), null, Color.Red * 0.8f, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
                }
            }
            if (hovering)
            {
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    OnClicked();
                }
                else if (ActiveSkillMenu.MouseHeldElement == null && effect != null)
                {
                    string locPath = $"Mods.{effect.Mod.Name}.ActiveSkills";
                    string name = Language.GetTextValue($"{locPath}.{effect.Name}.DisplayName");
                    string tooltip = Language.GetTextValue($"{locPath}.{effect.Name}.Tooltip");
                    string unequipped = Language.GetTextValue($"{locPath}.Unequipped");
                    ModifyTooltip(ref tooltip);

                    string text = $"{name}\n{tooltip}";

                    if (!Main.LocalPlayer.AccessoryEffects().Equipped(effect))
                        text = "[c/BC5252:" + unequipped + "]" + "\n" + text;

                    
                    if (!text.Contains($"Mods.{effect.Mod.Name}"))
                    {
                        UICommon.TooltipMouseText(text);
                        /*
                        Item hoverItem = new(2);
                        hoverItem.createTile = -1;
                        hoverItem.material = false;
                        hoverItem.consumable = false;
                        hoverItem.SetNameOverride(text);
                        Main.HoverItem = hoverItem;
                        Main.instance.MouseText("", 0, 0);
                        Main.hoverItemName = text;
                        Main.mouseText = true;
                        */
                        //Main.instance.MouseText(text, 0, 0);
                        /*
                        Utils.DrawBorderString(
                            spriteBatch,
                            text,
                            textPosition,
                            Color.White);
                        */
                    }
                }
            }
            

        }
    }
    public class AvailableSkillBox : ActiveSkillBox
    {
        public AccessoryEffect Effect;

        public AvailableSkillBox(AccessoryEffect effect, string mod, float width)
        {
            Effect = effect;
            Mod = mod;

            Width.Set(width, 0);
            Height.Set(width, 0);
        }
        public override void CheckSlotElegible(Player player)
        {
            if (Effect != null && Effect.EffectItem(player) == null)
            {
                Effect = null;
                ActiveSkillMenu.ShouldRefresh = true;
            }

        }
        public override AccessoryEffect GetEffect() => Effect;
        public override void OnClicked()
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            if (ActiveSkillMenu.MouseHeldElement == null)
            {
                ActiveSkillMenu.MouseHeldElement = this;
            }
            else
            {
                ActiveSkillMenu.MouseHeldElement = null;
                ActiveSkillMenu.ShouldRefresh = true;
            }
        }
    }
    public class EquippedSkillBox : ActiveSkillBox
    {
        public int Slot;

        public EquippedSkillBox(int slot,  float width)
        {
            Slot = slot;

            Width.Set(width, 0);
            Height.Set(width, 0);
        }
        public override void CheckSlotElegible(Player player)
        {
            /*
            var skill = player.FargoSouls().ActiveSkills[Slot];
            AccessoryEffectPlayer aPlayer = player.AccessoryEffects();
            if (skill != null && !aPlayer.Active(skill))
            {
                Opacity = 0.4f;
            }
            else
                Opacity = 1f;
            */
        }
        public override AccessoryEffect GetEffect() => Main.LocalPlayer.FargoSouls().ActiveSkills[Slot];
        public override void OnClicked()
        {
            FargoSoulsPlayer modPlayer = Main.LocalPlayer.FargoSouls();
            if (ActiveSkillMenu.MouseHeldElement == null)
            {
                if (modPlayer.ActiveSkills[Slot] != null)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    modPlayer.ActiveSkills[Slot] = null;
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        modPlayer.SyncActiveSkill(Slot);
                }
                ActiveSkillMenu.ShouldRefresh = true;
            }
            else
            {
                if (ActiveSkillMenu.MouseHeldElement is AvailableSkillBox mouseSkill)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    modPlayer.ActiveSkills[Slot] = mouseSkill.Effect;
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        modPlayer.SyncActiveSkill(Slot);
                }
                ActiveSkillMenu.MouseHeldElement = null;
                ActiveSkillMenu.ShouldRefresh = true;
            }
        }
        public override void PreDraw(SpriteBatch spriteBatch, Vector2 position)
        {
            var keys = FargowiltasSouls.ActiveSkillKeys[Slot].GetAssignedKeys();
            string keyText = "";
            foreach (var key in keys)
                keyText += $"{key} ";
            keyText = keyText.Replace("Mouse", "M");
            Utils.DrawBorderString(spriteBatch, keyText, position + new Vector2(-3 /*32*/, 32), Color.White);
        }
        public override void ModifyTooltip(ref string tooltip)
        {
            if (Opacity < 1)
            {
                string prepend = Language.GetTextValue("Mods.FargowiltasSouls.UI.DisabledUnequipped");
                tooltip = prepend + "\n" + tooltip;
            }
        }
    }
}
