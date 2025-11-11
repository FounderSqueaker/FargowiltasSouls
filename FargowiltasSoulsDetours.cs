using Fargowiltas;
using FargowiltasSouls.Common;
using FargowiltasSouls.Content.Bosses.VanillaEternity;
using FargowiltasSouls.Content.Buffs;
using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Patreon.ParadoxWolf;
using FargowiltasSouls.Content.PlayerDrawLayers;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Accessories.DubiousCircuitry;
using FargowiltasSouls.Content.Sky;
using FargowiltasSouls.Content.Tiles;
using FargowiltasSouls.Content.UI;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace FargowiltasSouls
{
    public partial class FargowiltasSouls : ICustomDetourProvider
    {
        private static readonly MethodInfo? CombinedHooks_ModifyHitNPCWithProj_Method = typeof(CombinedHooks).GetMethod("ModifyHitNPCWithProj", LumUtils.UniversalBindingFlags);

        private static readonly MethodInfo? On_Player_PickAmmo_Method = typeof(Player).GetMethod("PickAmmo", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo? On_TileLoader_PickPowerCheck_Method = typeof(TileLoader).GetMethod("PickPowerCheck", LumUtils.UniversalBindingFlags);

        public delegate void Orig_CombinedHooks_ModifyHitNPCWithProj(Projectile projectile, NPC nPC, ref NPC.HitModifiers modifiers);

        public delegate int Orig_StrikeNPC_HitInfo_bool_bool(NPC nPC, NPC.HitInfo hit, bool fromNet, bool noPlayerInteraction);

        public delegate void Orig_PickAmmo(Player self, Item sItem, ref int projToShoot, ref float speed, ref bool canShoot, ref int totalDamage, ref float KnockBack, out int usedAmmoItemId, bool dontConsume);

        public delegate void Orig_PickPowerCheck(Tile target, int pickPower, ref int damage);

        public void LoadDetours()
        {
            On_Main.MouseText_DrawItemTooltip_GetLinesInfo += MouseText_DrawItemTooltip_GetLinesInfo;
            On_Main.DrawInterface_35_YouDied += DrawInterface_35_YouDied;
            On_Main.DrawMenu += DrawMenu;

            On_WorldGen.MakeDungeon += CheckBricks;
            On_WorldGen.KillTile_GetItemDrops += JungleHerbDrop;

            On_Player.CheckSpawn_Internal += LifeRevitalizer_CheckSpawn_Internal;
            On_Player.AddBuff += AddBuff;
            On_Player.QuickHeal_GetItemToUse += QuickHeal_GetItemToUse;
            On_Player.CheckDrowning += PreventGillsDrowning;
            //On_Player.HorsemansBlade_SpawnPumpkin += HorsemansBlade_SpawnPumpkin;
            On_Player.ItemCheck_Shoot += InterruptShoot;

            On_Projectile.AI_019_Spears_GetExtensionHitbox += AI_019_Spears_GetExtensionHitbox;
            //On_Projectile.IsDamageDodgable += IsDamageDodgable;

            On_Item.AffixName += AffixName;

            On_NPC.AI_123_Deerclops_TryMakingSpike_FindBestY += AI_123_Deerclops_TryMakingSpike_FindBestY;
            On_NPCUtils.TargetClosestBetsy += TargetClosestBetsy;

            On_ShimmerTransforms.IsItemTransformLocked += IsItemTransformLocked;

            //On_Projectile.Damage += PhantasmArrowRainFix;

            On_Player.PutHallowedArmorSetBonusOnCooldown += ShadowDodgeNerf;

            On_NPC.SpawnOnPlayer += SetSpawnPlayer;
            On_Player.ApplyTouchDamage += ApplyTouchDamage;
            On_Player.StatusFromNPC += RemoveAnnoyingNPCDebuffs;
            On_Player.Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float += IgnorePlayerImmunityCooldowns;
            On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool += GetTileType;
            On_Player.ApplyItemTime += MinerEnchBuffMoreToolSpeed;
            On_Player.ItemCheck_UseMiningTools_TryHittingWall += MinerEnchWallHammerSpeed;
            On_NPC.AI_123_Deerclops += AI_123_Deerclops;
            On_Projectile.StatusPlayer += StatusPlayer;

            On_ItemSlot.PickItemMovementAction += AllowSouls;
            On_Main.HoverOverNPCs += HoverOverNPCs;
            On_Player.QuickGrapple += DontDismountOnGrappleShoot;
            On_Player.GrappleMovement += DontDismountOnGrappleLand;
        }

        private void SetSpawnPlayer(On_NPC.orig_SpawnOnPlayer orig, int plr, int Type)
        {
            orig(plr, Type);

        }

        public void UnloadDetours()
        {
            On_Main.MouseText_DrawItemTooltip_GetLinesInfo -= MouseText_DrawItemTooltip_GetLinesInfo;
            On_Main.DrawInterface_35_YouDied -= DrawInterface_35_YouDied;
            On_Main.DrawMenu -= DrawMenu;

            On_WorldGen.MakeDungeon -= CheckBricks;
            On_WorldGen.KillTile_GetItemDrops -= JungleHerbDrop;

            On_Player.CheckSpawn_Internal -= LifeRevitalizer_CheckSpawn_Internal;
            On_Player.AddBuff -= AddBuff;
            On_Player.QuickHeal_GetItemToUse -= QuickHeal_GetItemToUse;
            On_Player.CheckDrowning -= PreventGillsDrowning;
            //On_Player.HorsemansBlade_SpawnPumpkin -= HorsemansBlade_SpawnPumpkin;
            On_Player.ItemCheck_Shoot -= InterruptShoot;

            On_Projectile.AI_019_Spears_GetExtensionHitbox -= AI_019_Spears_GetExtensionHitbox;
            //On_Projectile.IsDamageDodgable -= IsDamageDodgable;

            On_Item.AffixName -= AffixName;

            On_NPC.AI_123_Deerclops_TryMakingSpike_FindBestY -= AI_123_Deerclops_TryMakingSpike_FindBestY;
            On_NPCUtils.TargetClosestBetsy -= TargetClosestBetsy;

            On_ShimmerTransforms.IsItemTransformLocked -= IsItemTransformLocked;

            //On_Projectile.Damage -= PhantasmArrowRainFix;

            On_Player.PutHallowedArmorSetBonusOnCooldown -= ShadowDodgeNerf;

            On_NPC.SpawnOnPlayer -= SetSpawnPlayer;
            On_Player.ApplyTouchDamage -= ApplyTouchDamage;
            On_Player.StatusFromNPC -= RemoveAnnoyingNPCDebuffs;
            On_Player.Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float -= IgnorePlayerImmunityCooldowns;
            On_Player.ItemCheck_UseMiningTools_ActuallyUseMiningTool -= GetTileType;
            On_Player.ApplyItemTime -= MinerEnchBuffMoreToolSpeed;
            On_Player.ItemCheck_UseMiningTools_TryHittingWall -= MinerEnchWallHammerSpeed;
            On_NPC.AI_123_Deerclops -= AI_123_Deerclops;
            On_Projectile.StatusPlayer -= StatusPlayer;

            On_ItemSlot.PickItemMovementAction -= AllowSouls;
            On_Main.HoverOverNPCs -= HoverOverNPCs;
            On_Player.QuickGrapple -= DontDismountOnGrappleShoot;
            On_Player.GrappleMovement -= DontDismountOnGrappleLand;
        }

        private int AllowSouls(On_ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item checkItem)
        {
            bool shouldAllow = (context == -10 || context == -11) && LoaderManager.Get<AccessorySlotLoader>().ModSlotCheck(checkItem, slot, context) && FargoSoulsSets.Items.AllowedSoulItemExceptions[checkItem.type];

            if (!WorldSavingSystem.EternityMode)
                return orig(inv, context, slot, checkItem);

            if (shouldAllow)
                return 1;

            int result = orig(inv, context, slot, checkItem);

            //if (shouldAllow)
            //    checkItem.accessory = false;

            return result;
        }

        private static void CheckBricks(On_WorldGen.orig_MakeDungeon orig, int x, int y)
        {
            orig(x, y);
            if (GenVars.crackedType == 482)
                WorldSavingSystem.DungeonBrickType = "G";
            if (GenVars.crackedType == 483)
                WorldSavingSystem.DungeonBrickType = "P";
        }

        internal static void JungleHerbDrop(On_WorldGen.orig_KillTile_GetItemDrops orig, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops)
        {
            Player tileplayer = Main.player[Player.FindClosest(new Vector2(x, y) * 16f, 16, 16)];
            if (tileplayer.HasEffect<JungleHerbEffect>() && (tileCache.TileType == TileID.MatureHerbs || tileCache.TileType == TileID.BloomingHerbs))
            {
                int num = tileCache.TileFrameX / 18;
                dropItem = 313 + num;
                int tiledrop = 307 + num;
                if (num == 6)
                {
                    dropItem = 2358;
                    tiledrop = 2357;
                }

                bool flag = WorldGen.IsHarvestableHerbWithSeed(tileCache.TileType, num);
                dropItemStack = Main.rand.Next(1, 3);
                secondaryItem = tiledrop;
                secondaryItemStack = Main.rand.Next(1, 6);
                if (flag)
                {
                    secondaryItem = tiledrop;
                    secondaryItemStack = Main.rand.Next(1, 4);
                }
                return;
            }
            orig(x, y, tileCache, out dropItem, out dropItemStack, out secondaryItem, out secondaryItemStack, includeLargeObjectDrops);
        }

        void ICustomDetourProvider.ModifyMethods()
        {
            HookHelper.ModifyMethodWithDetour(CombinedHooks_ModifyHitNPCWithProj_Method, CombinedHooks_ModifyHitNPCWithProj);
            HookHelper.ModifyMethodWithDetour(On_Player_PickAmmo_Method, NerfCoinGun);
            HookHelper.ModifyMethodWithDetour(On_TileLoader_PickPowerCheck_Method, MakeCommonTilesEasierToBreak);
        }

        private static bool LifeRevitalizer_CheckSpawn_Internal(
    On_Player.orig_CheckSpawn_Internal orig,
    int x, int y)
        {
            if (orig(x, y))
                return true;

            //Main.NewText($"{x} {y}");

            int revitalizerType = ModContent.TileType<LifeRevitalizerPlaced>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -3; j <= -1; j++)
                {
                    int newX = x + i;
                    int newY = y + j;

                    if (!WorldGen.InWorld(newX, newY))
                        return false;

                    Tile tile = Framing.GetTileSafely(newX, newY);
                    if (tile.TileType != revitalizerType)
                        return false;
                }
            }

            return true;
        }

        private void AddBuff(
            Terraria.On_Player.orig_AddBuff orig,
            Player self, int type, int timeToAdd, bool quiet, bool foodHack)
        {
            if (WorldSavingSystem.EternityMode && DeerclopsAICurrentlyRunning is NPC deer && deer.ai[0] == 3 && type == BuffID.Slow && timeToAdd == 720)
                return; // Remove stupid Slow debuff from deer
            FargoSoulsPlayer modPlayer = self.FargoSouls();
            if (Main.debuff[type]
                && timeToAdd > 3 //dont affect auras
                && !Main.buffNoTimeDisplay[type] //dont affect hidden time debuffs
                && !BuffID.Sets.NurseCannotRemoveDebuff[type] //only affect debuffs that nurse can cleanse
                && (modPlayer.ParryDebuffImmuneTime > 0
                    || modPlayer.ImmuneToDamage
                    || modPlayer.MonkDashing > 0
                    || modPlayer.TitaniumDRBuff)
                && modPlayer.TurtleShellHP > 0
                && FargoSoulsSets.Buffs.Debuffs[type])
            {
                return; //doing it this way so that debuffs previously had are retained, but existing debuffs also cannot be extended by reapplying
            }

            orig(self, type, timeToAdd, quiet, foodHack);
        }

        private Item QuickHeal_GetItemToUse(On_Player.orig_QuickHeal_GetItemToUse orig, Player self)
        {
            Item value = orig(self);

            int num3 = 58;
            if (self.useVoidBag())
            {
                num3 = 98;
            }
            for (int i = 0; i < num3; i++)
            {
                Item item = ((i >= 58) ? self.bank4.item[i - 58] : self.inventory[i]);
                if (item.stack <= 0 || item.type <= ItemID.None || !item.potion || item.healLife <= 0)
                {
                    continue;
                }
                if (self.HasEffect<ShroomiteMushroomPriority>() && item.type == ItemID.Mushroom)
                    return item;
            }

            return value;
        }

        // Tungsten Enchant extended spear hitbox fix
        public static bool AI_019_Spears_GetExtensionHitbox(On_Projectile.orig_AI_019_Spears_GetExtensionHitbox orig, Projectile self, out Rectangle extensionBox)
        {
            bool ret = orig(self, out extensionBox);
            if (ret)
            {
                Vector2 dif = extensionBox.Center.ToVector2() - self.Center;
                Vector2 extra = dif * (self.scale - 1);
                extensionBox.Location += extra.ToPoint();
                extensionBox.Inflate(0, (int)(extensionBox.Height * 0.5f * (self.scale - 1)));
            }
            return ret;
        }

        /*public static bool IsDamageDodgable(On_Projectile.orig_IsDamageDodgable orig, Projectile self)
        {
            if (self.type == ModContent.ProjectileType<RemoteLightning>() || self.type == ModContent.ProjectileType<RemoteLightningExplosion>())
                return false;
            else return orig(self);
        }*/

        public static string AffixName(On_Item.orig_AffixName orig, Item self)
        {
            string text = orig(self);
            if (self.ModItem != null && self.ModItem is SoulsItem soulsItem)
            {
                text = text.ArticlePrefixAdjustmentString(soulsItem.Articles.ToArray());
            }
            return text;
        }
        public static void TargetClosestBetsy(On_NPCUtils.orig_TargetClosestBetsy orig, NPC searcher, bool faceTarget = true, Vector2? checkPosition = null)
        {
            if (searcher.TypeAlive(NPCID.DD2Betsy) && searcher.TryGetGlobalNPC(out Betsy betsy) && betsy.TargetPlayer)
            {
                NPCUtils.TargetClosestCommon(searcher, faceTarget, checkPosition);
                return;
            }
            orig(searcher, faceTarget, checkPosition);
        }
        public static void MouseText_DrawItemTooltip_GetLinesInfo(On_Main.orig_MouseText_DrawItemTooltip_GetLinesInfo orig, Item item, ref int yoyoLogo, ref int researchLine, float oldKB, ref int numLines, string[] toolTipLine, bool[] preFixLine, bool[] badPreFixLine, string[] toolTipNames, out int prefixlineIndex)
        {
            DrawingTooltips = true;
            orig(item, ref yoyoLogo, ref researchLine, oldKB, ref numLines, toolTipLine, preFixLine, badPreFixLine, toolTipNames, out prefixlineIndex);
            DrawingTooltips = false;
        }
        /*public static void HorsemansBlade_SpawnPumpkin(On_Player.orig_HorsemansBlade_SpawnPumpkin orig, Player self, int npcIndex, int dmg, float kb)
        {
            NPC npc = Main.npc[npcIndex];
            if (npc.type is NPCID.GolemFistLeft or NPCID.GolemFistRight && WorldSavingSystem.EternityMode && npc.TryGetGlobalNPC(out GolemFist golemFist) && golemFist.RunEmodeAI)
                return;
            orig(self, npcIndex, dmg, kb);
        }*/

        private void InterruptShoot(On_Player.orig_ItemCheck_Shoot orig, Player self, int i, Item sItem, int weaponDamage)
        {

            if (SwordGlobalItem.BroadswordRework(sItem) && sItem.TryGetGlobalItem(out SwordGlobalItem sword) && !sword.VanillaShoot)
            {
                FargoSoulsPlayer mplayer = self.FargoSouls();

                mplayer.shouldShoot = true;
                return;
            }
            orig(self, i, sItem, weaponDamage);
        }
        public static void DrawInterface_35_YouDied(On_Main.orig_DrawInterface_35_YouDied orig)
        {
            orig();
            if (Main.LocalPlayer.dead && Main.LocalPlayer.Eternity().PreventRespawn() && Main.netMode != NetmodeID.SinglePlayer)
            {
                float num = -60f;
                string value = Lang.inter[38].Value;
                //DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, FontAssets.DeathText.Value, value, new Vector2((float)(screenWidth / 2) - FontAssets.DeathText.Value.MeasureString(value).X / 2f, (float)(screenHeight / 2) + num), player[myPlayer].GetDeathAlpha(Microsoft.Xna.Framework.Color.Transparent), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                if (Main.LocalPlayer.lostCoins > 0)
                {
                    num += 50f;
                    //string textValue = Language.GetTextValue("Game.DroppedCoins", player[myPlayer].lostCoinString);
                    //DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, FontAssets.MouseText.Value, textValue, new Vector2((float)(screenWidth / 2) - FontAssets.MouseText.Value.MeasureString(textValue).X / 2f, (float)(screenHeight / 2) + num), player[myPlayer].GetDeathAlpha(Microsoft.Xna.Framework.Color.Transparent), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                }
                num += (float)((Main.LocalPlayer.lostCoins > 0) ? 24 : 50);
                num += 20f;
                float num2 = 0.7f;
                //string textValue2 = Language.GetTextValue("Game.RespawnInSuffix", ((float)(int)(1f + (float)player[myPlayer].respawnTimer / 60f)).ToString());
                //DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, FontAssets.DeathText.Value, textValue2, new Vector2((float)(screenWidth / 2) - FontAssets.MouseText.Value.MeasureString(textValue2).X * num2 / 2f, (float)(screenHeight / 2) + num), player[myPlayer].GetDeathAlpha(Microsoft.Xna.Framework.Color.Transparent), 0f, default(Vector2), num2, SpriteEffects.None, 0f);

                // draw our maso you can't respawn text
                num += 60;
                num2 = 0.5f;
                string mode = WorldSavingSystem.MasochistModeReal ? Language.GetTextValue("Mods.FargowiltasSouls.UI.Masochist") : Language.GetTextValue("Mods.FargowiltasSouls.UI.Eternity");
                string text = Language.GetTextValue("Mods.FargowiltasSouls.UI.NoRespawn", mode);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.DeathText.Value, text,
                    new Vector2((float)(Main.screenWidth / 2) - FontAssets.DeathText.Value.MeasureString(text).X * num2 / 2, (float)(Main.screenHeight / 2) + num),
                    Main.LocalPlayer.GetDeathAlpha(Microsoft.Xna.Framework.Color.Transparent), 0f, default, num2, SpriteEffects.None, 0f);
            }
        }

        private static void DrawMenu(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            float upBump = 0;
            byte b = (byte)((255 + Main.tileColor.R * 2) / 3);
            Mod mod = FargowiltasSouls.Instance;
            Vector2 anchorPosition = new Vector2(18f, (float)(Main.screenHeight - 116 - 22) - upBump);
            Color color = new Microsoft.Xna.Framework.Color(b, b, b, 255);
            upBump += 32f;
            if (MenuLoader.CurrentMenu is FargoMenuScreen)
            {
                if (!WorldGen.drunkWorldGen && Main.menuMode == 0)
                {
                    FargowiltasSouls.DrawTitleLinks(color, upBump);
                    upBump += 32f;
                }
                if (!WorldGen.drunkWorldGen)
                {
                    string text = mod.DisplayName + " " + mod.Version;
                    Vector2 origin = FontAssets.MouseText.Value.MeasureString(text);
                    origin.X *= 0.5f;
                    origin.Y *= 0.5f;
                    for (int i = 0; i < 5; i++)
                    {
                        Microsoft.Xna.Framework.Color color2 = Microsoft.Xna.Framework.Color.Black;
                        if (i == 4)
                        {
                            color2 = color;
                            color2.R = (byte)((255 + color2.R) / 2);
                            color2.G = (byte)((255 + color2.R) / 2);
                            color2.B = (byte)((255 + color2.R) / 2);
                        }
                        color2.A = (byte)((float)(int)color2.A * 0.3f);
                        int num = 0;
                        int num2 = 0;
                        if (i == 0)
                        {
                            num = -2;
                        }
                        if (i == 1)
                        {
                            num = 2;
                        }
                        if (i == 2)
                        {
                            num2 = -2;
                        }
                        if (i == 3)
                        {
                            num2 = 2;
                        }
                        DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, text, new Vector2(origin.X + (float)num + 10f, (float)Main.screenHeight - origin.Y + (float)num2 - (Main.menuMode == 0 ? 85f : 25f) - upBump), color2, 0f, origin, 1f, SpriteEffects.None, 0f);
                    }

                }
            }
            orig(self, gameTime);
        }
        public static void CombinedHooks_ModifyHitNPCWithProj(Orig_CombinedHooks_ModifyHitNPCWithProj orig, Projectile projectile, NPC nPC, ref NPC.HitModifiers modifiers)
        {
            // Whip tag damage nerf
            if (WorldSavingSystem.EternityMode)
            {
                int tags = 0;
                for (int i = 0; i < nPC.buffType.Length; i++)
                {
                    int type = nPC.buffType[i];
                    if (BuffID.Sets.IsATagBuff[type])
                        tags++;
                }
                if (tags > 1)
                {
                    float perStack = 0.5f;
                    float mult = (1 - perStack + perStack * tags) / tags;
                    ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type] *= mult;
                    // IMPORTANT that this is reset after the hit!
                    // This is done in FargoSoulsPlayer.OnHitNPCWithProj using the following variable
                    projectile.FargoSouls().TagStackMultiplier = mult;
                }
            }
            orig(projectile, nPC, ref modifiers);
        }

        private static bool IsItemTransformLocked(On_ShimmerTransforms.orig_IsItemTransformLocked orig, int type)
        {
            bool ret = orig(type);
            //Rod of Harmony post Mutant
            if (type == ItemID.RodofDiscord)
            {
                return !WorldSavingSystem.DownedMutant;
            }

            return ret;
        }

        private static int AI_123_Deerclops_TryMakingSpike_FindBestY(On_NPC.orig_AI_123_Deerclops_TryMakingSpike_FindBestY orig, NPC self, ref Point sourceTileCoords, int x)
        {
            if (!WorldSavingSystem.EternityMode)
                return orig(self, ref sourceTileCoords, x);

            int num = sourceTileCoords.Y;
            NPCAimedTarget targetData = self.GetTargetData();
            if (!targetData.Invalid)
            {
                Rectangle hitbox = targetData.Hitbox;
                Vector2 vector = new Vector2(hitbox.Center.X, hitbox.Bottom);
                int num2 = (int)(vector.Y / 16f);
                int num3 = Math.Sign(num2 - num);
                int num4 = num2 + num3 * 15;
                int? num5 = null;
                float num6 = float.PositiveInfinity;
                for (int i = num; i != num4; i += num3)
                {
                    if (WorldGen.SolidTile(x, i))
                    {
                        float num7 = new Point(x, i).ToWorldCoordinates().Distance(vector);
                        if (!num5.HasValue || !(num7 >= num6))
                        {
                            num5 = i;
                            num6 = num7;
                        }
                    }
                }
                if (num5.HasValue)
                {
                    num = num5.Value;
                }
            }
            for (int j = 0; j < 20; j++)
            {
                if (num < 10)
                {
                    break;
                }
                if (!WorldGen.SolidTile(x, num))
                {
                    break;
                }
                num--;
            }
            for (int k = 0; k < 20; k++)
            {
                if (num > Main.maxTilesY - 10)
                {
                    break;
                }
                if (WorldGen.SolidTile(x, num))
                {
                    break;
                }
                num++;
            }
            return num;
        }

        private static void PreventGillsDrowning(On_Player.orig_CheckDrowning orig, Player player)
        {
            bool hadGills = false;
            if (WorldSavingSystem.EternityMode && Main.getGoodWorld && player.HasEffect<ChalicePotionEffect>())
            {
                hadGills = player.gills;
                player.gills = false;
            }
            orig(player);
            if (WorldSavingSystem.EternityMode && Main.getGoodWorld && player.HasEffect<ChalicePotionEffect>())
            {
                player.gills = hadGills;
            }
        }

        /*public static void PhantasmArrowRainFix(On_Projectile.orig_Damage orig, Projectile self)
        { // this detour makes it so Arrow Rain projectiles spawned from max stack Red Riding Enchantment do not proc Phantasm's phantom arrows
            if (self is not null && self.friendly && self.owner.IsWithinBounds(Main.maxPlayers) && self.owner == Main.myPlayer)
            {
                int phantasmTime = -1;
                var player = Main.player[self.owner];
                bool phantasmAverted = false;
                phantasmTime = player.phantasmTime;
                var globalProj = self.FargoSouls();
                if (phantasmTime > 0 && globalProj.ArrowRain)
                {
                    phantasmAverted = true;
                    player.phantasmTime = 0;
                }
                orig(self);
                if (phantasmAverted)
                    player.phantasmTime = phantasmTime;
            }
            else orig(self);
        }*/

        public static void ShadowDodgeNerf(On_Player.orig_PutHallowedArmorSetBonusOnCooldown orig, Player self)
        { //Hallowed dodge nerf
            orig(self);
            if (EmodeItemBalance.HasEmodeChange(self, ItemID.HallowedPlateMail).Contains("HolyDodge"))
                self.shadowDodgeTimer = 60 * 45;
        }

        internal void NerfCoinGun(Orig_PickAmmo orig, Player self, Item sItem, ref int projToShoot, ref float speed, ref bool canShoot, ref int totalDamage, ref float KnockBack, out int usedAmmoItemId, bool dontConsume)
        {
            orig(self, sItem, ref projToShoot, ref speed, ref canShoot, ref totalDamage, ref KnockBack, out usedAmmoItemId, dontConsume);
            if (self is not null)
            {
                if (canShoot && sItem.type == ItemID.CoinGun && projToShoot >= ProjectileID.CopperCoin && projToShoot <= ProjectileID.PlatinumCoin && EmodeItemBalance.HasEmodeChange(self, sItem.type).Contains("CoinGun"))
                {
                    if (projToShoot == ProjectileID.CopperCoin)
                        totalDamage = (int)Math.Ceiling(totalDamage * 1.6f);
                    else if (projToShoot == ProjectileID.SilverCoin)
                        totalDamage = (int)Math.Ceiling(totalDamage * 0.9f);
                    else if (projToShoot == ProjectileID.GoldCoin)
                        totalDamage = (int)Math.Ceiling(totalDamage * 0.47f);
                    else if (projToShoot == ProjectileID.PlatinumCoin)
                        totalDamage = (int)Math.Ceiling(totalDamage * 0.275f);
                }
            }
        }
        private void ApplyTouchDamage(On_Player.orig_ApplyTouchDamage orig, Player self, int tileId, int x, int y)
        {
            if (((tileId == TileID.Cactus && Main.dontStarveWorld) || tileId == TileID.RollingCactus) && self.HasEffect<CactusPassiveEffect>())
                return;
            if (self.FargoSouls().PureHeart && (tileId == TileID.CorruptThorns || tileId == TileID.CrimsonThorns || tileId == TileID.JungleThorns)) //leave out plantera thorns
                return;
            orig(self, tileId, x, y);
        }
        public void RemoveAnnoyingNPCDebuffs(On_Player.orig_StatusFromNPC orig, Player self, NPC nPC)
        {
            if (WorldSavingSystem.EternityMode && nPC.type is NPCID.SkeletronHead or NPCID.SkeletronHand or NPCID.Creeper)
                return;
            orig(self, nPC);
        }
        public double IgnorePlayerImmunityCooldowns(On_Player.orig_Hurt_PlayerDeathReason_int_int_refHurtInfo_bool_bool_int_bool_float_float_float orig, Player self, PlayerDeathReason damageSource, int Damage, int hitDirection, out Player.HurtInfo info, bool pvp, bool quiet, int cooldownCounter, bool dodgeable, float armorPenetration, float scalingArmorPenetration, float knockback)
        {
            var value = orig(self, damageSource, Damage, hitDirection, out info, pvp, quiet, cooldownCounter, dodgeable, armorPenetration, scalingArmorPenetration, knockback);
            int proj = damageSource.SourceProjectileType;
            if ((proj == ModContent.ProjectileType<RemoteLightning>() || proj == ModContent.ProjectileType<RemoteLightningExplosion>()) && cooldownCounter == ImmunityCooldownID.WrongBugNet && self.HasEffect<RemoteControlDR>() && !self.HasBuff<SuperchargedBuff>())
            {
                int general = 0; //ImmunityCooldownID.General
                int boss = ImmunityCooldownID.Bosses;
                int ogre = ImmunityCooldownID.DD2OgreKnockback;
                int bugNet = ImmunityCooldownID.WrongBugNet;
                int lava = ImmunityCooldownID.Lava;
                int[] cooldown = self.hurtCooldowns; // Below is checking for when every immunity cooldown time is equal and above 0 meaning the hit is directly AFTER a dodge
                bool dodging = cooldown[bugNet] > 0 && cooldown[general] == cooldown[boss] && cooldown[boss] == cooldown[ogre] && cooldown[ogre] == cooldown[bugNet] && cooldown[bugNet] == cooldown[lava];
                if (dodging)
                {
                    self.hurtCooldowns[bugNet] = 0;
                    bool immuneCheck = self.immune;
                    self.immune = false;
                    value = orig(self, damageSource, Damage, hitDirection, out info, pvp, true, cooldownCounter, false, armorPenetration, scalingArmorPenetration, 0f);
                    if (!self.immune)
                        self.immune = immuneCheck;
                }
                else
                    value = orig(self, damageSource, Damage, hitDirection, out info, pvp, true, cooldownCounter, false, armorPenetration, scalingArmorPenetration, 0f);
            }
            return value;
        }
        private void GetTileType(On_Player.orig_ItemCheck_UseMiningTools_ActuallyUseMiningTool orig, Player self, Item sItem, out bool canHitWalls, int x, int y)
        {
            EModeGlobalTile.CaptureTileTypeBeingMined = Main.tile[x, y].TileType;
            orig(self, sItem, out canHitWalls, x, y);
            EModeGlobalTile.CaptureTileTypeBeingMined = null;
        }
        public void MinerEnchBuffMoreToolSpeed(On_Player.orig_ApplyItemTime orig, Player self, Item sItem, float multiplier, bool? callUseItem)
        {
            if (AchievementsHelper.CurrentlyMining && self.FargoSouls().MiningImmunity && EModeGlobalTile.CaptureTileTypeBeingMined != null)
            {
                int tile = (int)EModeGlobalTile.CaptureTileTypeBeingMined;
                if (Main.tileAxe[tile] || (sItem.pick == 0 && Main.tileHammer[tile]))
                { //Miner Enchantment
                    float speedMult = self.FargoSouls().ForceEffect<MinerEnchant>() ? 0.25f : 0.5f; // This affects useTime
                    orig(self, sItem, multiplier * speedMult, callUseItem);
                    return;
                }
            }
            orig(self, sItem, multiplier, callUseItem);
        }
        private void MinerEnchWallHammerSpeed(On_Player.orig_ItemCheck_UseMiningTools_TryHittingWall orig, Player self, Item sItem, int wX, int wY)
        {
            if (self.FargoSouls().MiningImmunity)
            { //Miner Enchantment
                float speedMult = self.FargoSouls().ForceEffect<MinerEnchant>() ? 0.25f : 0.5f;
                int time = sItem.useTime;
                sItem.useTime = (int)Math.Max(1, sItem.useTime * speedMult);
                orig(self, sItem, wX, wY);
                sItem.useTime = time;
            }
            else orig(self, sItem, wX, wY);
        }
        public static void MakeCommonTilesEasierToBreak(Orig_PickPowerCheck orig, Tile target, int pickPower, ref int damage)
        {
            if (!Main.getGoodWorld && WorldSavingSystem.EternityMode && FargoSoulsSets.Tiles.CommonTiles[target.TileType])
            {
                damage *= 2;
            }
            orig(target, pickPower, ref damage);
        }
        private void AI_123_Deerclops(On_NPC.orig_AI_123_Deerclops orig, NPC self)
        {
            DeerclopsAICurrentlyRunning = self;
            orig(self); // Capture whether Deerclops is the one currently inflicting a debuff or the sorts.
            DeerclopsAICurrentlyRunning = null;
        }
        private void StatusPlayer(On_Projectile.orig_StatusPlayer orig, Projectile self, int playerIndex)
        {
            if (self.type == ProjectileID.DeerclopsIceSpike && WorldSavingSystem.EternityMode && self.GetSourceNPC()?.type == NPCID.Deerclops)
                return; // Remove annoying Frozen debuff from EMode+ Deer spikes
            orig(self, playerIndex);
        }
        private void HoverOverNPCs(On_Main.orig_HoverOverNPCs orig, Main self, Rectangle mouseRectangle)
        {
            orig(self, mouseRectangle);
            if (WorldSavingSystem.EternityMode && WallofFleshEye.realLife > -1 && WallofFleshEye.realImmune != null)
            {
                Main.npc[WallofFleshEye.realLife].dontTakeDamage = (bool)WallofFleshEye.realImmune;
                WallofFleshEye.realLife = -1;
                WallofFleshEye.realImmune = null;
            }
        }
        public void DontDismountOnGrappleShoot(On_Player.orig_QuickGrapple orig, Player self)
        {
            bool check = false;
            if (self.FargoSouls().SquireEnchantItem != null && self.mount.Active)
            {
                check = true;
                typeof(Mount).GetField("_active", LumUtils.UniversalBindingFlags)?.SetValue(self.mount, false);
            }
            orig(self);
            if (check)
            {
                typeof(Mount).GetField("_active", LumUtils.UniversalBindingFlags)?.SetValue(self.mount, true);
            }
        }
        public void DontDismountOnGrappleLand(On_Player.orig_GrappleMovement orig, Player self)
        {
            bool check = false;
            if (self.FargoSouls().SquireEnchantItem != null && self.mount.Active)
            {
                check = true;
                typeof(Mount).GetField("_active", LumUtils.UniversalBindingFlags)?.SetValue(self.mount, false);
            }
            orig(self);
            if (check)
            {
                typeof(Mount).GetField("_active", LumUtils.UniversalBindingFlags)?.SetValue(self.mount, true);
            }
        }
    }
}
