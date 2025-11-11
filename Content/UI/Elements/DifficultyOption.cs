using Fargowiltas.Content.Projectiles;
using Fargowiltas.Content.UI;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Content.NPCs;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using static Terraria.GameContent.Creative.CreativePowers;

namespace FargowiltasSouls.Content.UI.Elements
{
    public abstract class DifficultyOption : UIPanel
    {
        public static DynamicSpriteFont Font => Terraria.GameContent.FontAssets.ItemStack.Value;
        public float Opacity = 1f;
        public abstract string NameKey { get; }
        public abstract string TooltipText();

        public DifficultyOption()
        {
            // Better sub-pixel scaling for sprites; makes them not-blurry.
            OverrideSamplerState = SamplerState.PointClamp;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        public abstract void OnClicked();
        public virtual void PreDraw(SpriteBatch spriteBatch, Vector2 position, float scale) { }
        public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 position, float scale) { }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Vector2 position = GetDimensions().Position();
            bool hovering = ContainsPoint(Main.MouseScreen);

            float scale = 1f;
            if (hovering)
                scale = 1.2f;

            PreDraw(spriteBatch, position, scale);
            //base.DrawSelf(spriteBatch);
            PostDraw(spriteBatch, position, scale);


            if (hovering)
            {
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    OnClicked();
                    FargoUIManager.Close<DifficultySelectionMenu>();
                }
                else
                {
                    string name = Language.GetTextValue(NameKey);
                    string leftClick = Language.GetTextValue("Mods.FargowiltasSouls.UI.LeftClickEnable");

                    string text = name;
                    text += $"\n[c/787878:{leftClick}]";
                    text += "\n" + TooltipText();

                    UICommon.TooltipMouseText(text);
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

    public class VanillaDifficultyOption : DifficultyOption
    {
        public static void DisableEternity()
        {
            if (!Masochist.CanToggleEternity())
                return;
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                WorldSavingSystem.ShouldBeEternityMode = false;
            }
            else
            {
                if (WorldSavingSystem.ShouldBeEternityMode)
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Difficulty" + "Deactivate") with { Volume = 0.5f });

                var netMessage = FargowiltasSouls.Instance.GetPacket();
                netMessage.Write((byte)FargowiltasSouls.PacketID.ToggleEternityMode);
                netMessage.Write((byte)Main.LocalPlayer.whoAmI);
                netMessage.Write((byte)0); // 0 = disable emode
                netMessage.Send();
            }
        }
        public override void OnClicked()
        {
            DisableEternity();
        }
        public override string NameKey => "Mods.FargowiltasSouls.UI.Standard";
        public override string TooltipText()
        {
            return Language.GetTextValue("Mods.FargowiltasSouls.UI.ExpandedStandard");
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 position, float scale)
        {
            var texture = FargoAssets.UI.OncomingMutantnt.Value;
            Vector2 center = position + new Vector2(Width.Pixels / 2, Height.Pixels / 2) - scale * texture.Size() / 2;
            spriteBatch.Draw(texture, center, texture.Bounds, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }
    public class EternityDifficultyOption : DifficultyOption
    {
        public static void EnableEternity()
        {
            if (!Masochist.CanToggleEternity())
                return;
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                bool changed = false;
                if (Main.GameModeInfo.IsJourneyMode)
                {
                    var slider = CreativePowerManager.Instance.GetPower<DifficultySliderPower>();
                    DifficultySelectionMenu.JourneyMode_SetValue.Invoke(slider, [0.66f]);
                }
                else
                {
                    if (Main.GameMode != GameModeID.Expert)
                        changed = true;
                    Main.GameMode = GameModeID.Expert;
                }
                if (changed)
                    FargoSoulsUtil.PrintLocalization("Mods.Fargowiltas.Items.ModeToggle.Expert", new Color(175, 75, 255));

                WorldSavingSystem.ShouldBeEternityMode = true;
            }
            else
            {
                if (Main.GameMode != GameModeID.Expert || !WorldSavingSystem.ShouldBeEternityMode)
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Difficulty" + "Emode") with { Volume = 1f });
                var netMessage = FargowiltasSouls.Instance.GetPacket();
                netMessage.Write((byte)FargowiltasSouls.PacketID.ToggleEternityMode);
                netMessage.Write((byte)Main.LocalPlayer.whoAmI);
                netMessage.Write((byte)1); // 1 = set to emode
                netMessage.Send();
            }
            int deviType = ModContent.NPCType<UnconsciousDeviantt>();
            if (!WorldSavingSystem.SpawnedDevi && !NPC.AnyNPCs(deviType))
            {
                WorldSavingSystem.SpawnedDevi = true;

                Vector2 spawnPos = (Main.zenithWorld || Main.remixWorld) ? Main.LocalPlayer.Center : Main.LocalPlayer.Center - 1000 * Vector2.UnitY;
                Projectile.NewProjectile(Main.LocalPlayer.GetSource_Misc(""), spawnPos, Vector2.Zero, ModContent.ProjectileType<SpawnProj>(), 0, 0, Main.myPlayer, deviType);

                FargoSoulsUtil.PrintLocalization("Announcement.HasAwoken", new Color(175, 75, 255), Language.GetTextValue("Mods.Fargowiltas.NPCs.Deviantt.DisplayName"));
            }
        }
        public override void OnClicked()
        {
            EnableEternity();
        }
        public const string LocPath = "Mods.FargowiltasSouls.UI.";
        public override string NameKey => LocPath + "Eternity";
        public override string TooltipText()
        {
            string text = Language.GetTextValue($"{LocPath}EternityOption");
            text += $"\n{Language.GetTextValue($"{LocPath}ExpandedFeatures")}";
            return text;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 position, float scale)
        {
            var texture = FargoAssets.UI.OncomingMutantTexture.Value;
            Vector2 center = position + new Vector2(Width.Pixels / 2, Height.Pixels / 2) - scale * texture.Size() / 2;
            spriteBatch.Draw(texture, center, texture.Bounds, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }
    public class MasoDifficultyOption : DifficultyOption
    {
        public static void EnableMasochist()
        {
            if (!Masochist.CanToggleEternity())
                return;
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                bool changed = false;
                if (Main.GameModeInfo.IsJourneyMode)
                {
                    var slider = CreativePowerManager.Instance.GetPower<DifficultySliderPower>();
                    DifficultySelectionMenu.JourneyMode_SetValue.Invoke(slider, [1f]);
                }
                else
                {
                    if (Main.GameMode != GameModeID.Master)
                        changed = true;
                    Main.GameMode = GameModeID.Master;
                }
                if (changed)
                    FargoSoulsUtil.PrintLocalization("Mods.Fargowiltas.Items.ModeToggle.Master", new Color(175, 75, 255));

                WorldSavingSystem.ShouldBeEternityMode = true;
            }
            else
            {
                if (Main.GameMode != GameModeID.Master || !WorldSavingSystem.ShouldBeEternityMode)
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Difficulty" + "Maso") with { Volume = 1f });

                var netMessage = FargowiltasSouls.Instance.GetPacket();
                netMessage.Write((byte)FargowiltasSouls.PacketID.ToggleEternityMode);
                netMessage.Write((byte)Main.LocalPlayer.whoAmI);
                netMessage.Write((byte)2); // 2 = set to emode
                netMessage.Send();
            }
            int deviType = ModContent.NPCType<UnconsciousDeviantt>();
            if (!WorldSavingSystem.SpawnedDevi && !NPC.AnyNPCs(deviType))
            {
                WorldSavingSystem.SpawnedDevi = true;

                Vector2 spawnPos = (Main.zenithWorld || Main.remixWorld) ? Main.LocalPlayer.Center : Main.LocalPlayer.Center - 1000 * Vector2.UnitY;
                Projectile.NewProjectile(Main.LocalPlayer.GetSource_Misc(""), spawnPos, Vector2.Zero, ModContent.ProjectileType<SpawnProj>(), 0, 0, Main.myPlayer, deviType);

                FargoSoulsUtil.PrintLocalization("Announcement.HasAwoken", new Color(175, 75, 255), Language.GetTextValue("Mods.Fargowiltas.NPCs.Deviantt.DisplayName"));
            }
        }
        public override void OnClicked()
        {
            EnableMasochist();
        }
        public const string LocPath = "Mods.FargowiltasSouls.UI.";
        public override string NameKey => LocPath + "Masochist";
        public override string TooltipText()
        {
            string text = Language.GetTextValue($"{LocPath}MasochistOption");
            text += $"\n{Language.GetTextValue($"{LocPath}ExpandedFeatures")}";
            if (Main.netMode != NetmodeID.SinglePlayer)
                text += $"\n{Language.GetTextValue($"{LocPath}MasochistMultiplayer")}";
            return text;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 position, float scale)
        {
            var texture = FargoAssets.UI.OncomingMutantTexture.Value;
            Vector2 center = position + new Vector2(Width.Pixels / 2, Height.Pixels / 2) - scale * texture.Size() / 2;
            spriteBatch.Draw(texture, center, texture.Bounds, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            texture = FargoAssets.UI.OncomingMutantAura.Value;
            center = position + new Vector2(Width.Pixels / 2, Height.Pixels / 2) - scale * texture.Size() / 2;
            spriteBatch.Draw(texture, center, texture.Bounds, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
    }
}
