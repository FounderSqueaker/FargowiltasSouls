using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Bosses.CursedCoffin;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Souls;
using FargowiltasSouls.Content.Items.Materials;
using FargowiltasSouls.Content.Projectiles.Accessories.BionomicCluster;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.GameContent.Animations.Actions.NPCs;

namespace FargowiltasSouls.Content.Items.Accessories
{
    public class SoulLantern : SoulsItem
    {
        public override bool IsLoadingEnabled(Mod mod) => false;
        public override List<AccessoryEffect> ActiveSkillTooltips => 
            [AccessoryEffectLoader.GetEffect<SoulLanternEffect>()];
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Accessories", Name);
        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public int SoulLanternID;

        public override void SaveData(TagCompound tag)
        {
            string soul = SoulLanternID.ToString();
            if (SoulLanternID >= NPCID.Count)
            {
                soul = ContentSamples.NpcsByNetId[SoulLanternID].ModNPC.Name;
            }
            tag.Add("Soul", soul);
        }

        public override void LoadData(TagCompound tag)
        {
            string? soul = tag["Soul"] is string str ? str : null;
            if (soul != null)
            {
                if (int.TryParse(soul, out int soulID) && soulID < NPCID.Count)
                {
                    SoulLanternID = soulID;
                }
                else if (ModContent.TryFind(soul, out ModNPC modNPC))
                {
                    SoulLanternID = modNPC.Type;
                }
            }
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 1);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ActiveEffects(player, Item);
        }
        public static void ActiveEffects(Player player, Item item)
        {
            player.AddEffect<SoulLanternEffect>(item);
        }
        int animationTimer = 0;
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (SoulLanternID > 0)
            {
                int animationSpeed = 5;

                animationTimer++;
                animationTimer %= animationSpeed * 6;
                Texture2D texture = ModContent.Request<Texture2D>(Texture + "_Active").Value;
                int sizeY = texture.Height / 6;
                int frameY = (animationTimer / animationSpeed) * sizeY;
                Rectangle frame2 = new(0, frameY, texture.Width, sizeY);
                spriteBatch.Draw(texture, position, frame2, drawColor, 0, frame2.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                return false;
            }
            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }
        public override void SafeModifyTooltips(List<TooltipLine> tooltips)
        {
            if (SoulLanternID > 0)
            {
                int i = tooltips.FindIndex(line => line.Name == "Tooltip1");
                if (i != -1)
                    tooltips[i].Text = string.Format(tooltips[i].Text, ContentSamples.NpcsByNetId[SoulLanternID].TypeName);
            }
        }
    }

    public class SoulLanternEffect : AccessoryEffect
    {
        public override bool IsLoadingEnabled(Mod mod) => false;
        public override Header ToggleHeader => null;
        public override int ToggleItemType => ModContent.ItemType<SoulLantern>();
        public override bool ActiveSkill => true;
        public static bool InSpawnNPC;
        public static int CurrentPlayerLanternID;
        public override void Load()
        {
            //On_NPC.SpawnNPC += SpawnNPCDetour;
            //On_NPC.NewNPC += NewNPCDetour;
        }
        public override void Unload()
        {
            //On_NPC.SpawnNPC -= SpawnNPCDetour;
            //On_NPC.NewNPC -= NewNPCDetour;
        }
        private static void SpawnNPCDetour(On_NPC.orig_SpawnNPC orig)
        {
            CurrentPlayerLanternID = 0;
            InSpawnNPC = true;
            orig();
            CurrentPlayerLanternID = 0;
            InSpawnNPC = false;
        }
        private static int NewNPCDetour(On_NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start = 0, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f, float ai3 = 0f, int Target = 255)
        {

            if (InSpawnNPC && CurrentPlayerLanternID > 0 && CurrentPlayerLanternID != Type && !Main.rand.NextBool(4))
            {
                return Main.maxNPCs;
            }
            return orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);
        }
        public override void PostUpdateEquips(Player player)
        {
            /*
            if (EffectItem(player).ModItem is SoulLantern soulLantern && soulLantern.SoulLanternID is int id)
            {
                if (id > 0)
                {
                    NPCTypeToSpawn = id;
                    for (int i = 0; i < 3; i++)
                        NPC.SpawnNPC();
                    NPCTypeToSpawn = -1;
                }
            }
            */
        }
        public override void ActiveSkillJustPressed(Player player, bool stunned)
        {
            if (player.whoAmI == Main.myPlayer && !stunned && EffectItem(player).ModItem is SoulLantern soulLantern)
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.Hitbox.Contains((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y))
                    {
                        soulLantern.SoulLanternID = npc.type;
                        SoundEngine.PlaySound(CursedCoffin.SoulShotSFX, npc.Center);
                        for (int i = 0; i < 30; i++)
                        {
                            Vector2 vel = npc.DirectionTo(player.Center) * Main.rand.NextFloat(6, 9);
                            Dust.NewDust(Main.rand.NextVector2FromRectangle(npc.Hitbox), 0, 0, DustID.Shadowflame, vel.X, vel.Y);
                        }
                        break;
                    }
                }
            }
        }
    }
}