using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Buffs
{
    internal class FargoGlobalBuff : GlobalBuff
    {
        public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare)
        {
            if (WorldSavingSystem.EternityMode)
            {
                switch (type)
                {
                    case BuffID.ShadowDodge:
                        if (EmodeItemBalance.HasEmodeChange(Main.LocalPlayer, ItemID.HallowedPlateMail).Contains("HolyDodge"))
                        {
                            tip += "\n" + Language.GetTextValue("Mods.FargowiltasSouls.EModeBalance.ShadowDodge");
                        }
                        break;
                    case BuffID.ChaosState:
                        if (EmodeItemBalance.HasEmodeChange(Main.LocalPlayer, ItemID.RodofDiscord).Contains("RodofDiscord"))
                        {
                            tip += "\n" + Language.GetTextValue("Mods.FargowiltasSouls.EModeBalance.RodofDiscord");
                        }
                        break;
                }
            }
        }

        public static int[] DebuffsToLetDecreaseNormally => [
            BuffID.Frozen,
            BuffID.Stoned,
            BuffID.Cursed,
            BuffID.ChaosState,
            ModContent.BuffType<FusedBuff>(),
            ModContent.BuffType<TimeFrozenBuff>(),
            ModContent.BuffType<StunnedBuff>()
        ];

        public override void Update(int type, Player player, ref int buffIndex)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            EModePlayer emodePlayer = player.Eternity();

            switch (type)
            {
                case BuffID.Slimed:
                    Main.buffNoTimeDisplay[type] = false;
                    if (WorldSavingSystem.EternityMode)
                        player.FargoSouls().Slimed = true;
                    break;

                case BuffID.OnFire:
                    if (WorldSavingSystem.EternityMode && Main.raining && player.position.Y < Main.worldSurface * 16
                        && Framing.GetTileSafely(player.Center).WallType == WallID.None && player.buffTime[buffIndex] > 2)
                        player.buffTime[buffIndex] -= 1;
                    break;

                case BuffID.Chilled:
                    if (WorldSavingSystem.EternityMode && player.buffTime[buffIndex] > 60 * 15)
                        player.buffTime[buffIndex] = 60 * 15;
                    break;

                case BuffID.Dazed:
                    if (player.whoAmI == Main.myPlayer && player.buffTime[buffIndex] % 60 == 55)
                        SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Debuffs/DizzyBird"));
                    break;

                case BuffID.Tipsy:
                    if (player.whoAmI == Main.myPlayer && EmodeItemBalance.HasEmodeChange(player, ItemID.Ale).Contains("Tipsy"))
                        player.statDefense -= 2;
                    break;

                default:
                    break;
            }
            if ((WorldSavingSystem.EternityMode || modPlayer.UsingAnkh) && player.buffTime[buffIndex] > 5 && Main.debuff[type] && emodePlayer.ShorterDebuffsTimer <= 0
                && !Main.buffNoTimeDisplay[type]
                && type != BuffID.Tipsy && (!BuffID.Sets.NurseCannotRemoveDebuff[type] || type == BuffID.ManaSickness || type == BuffID.PotionSickness)
                && !DebuffsToLetDecreaseNormally.Contains(type)
                && !(type == BuffID.Confused && FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.brainBoss, NPCID.BrainofCthulhu)))
            {
                player.buffTime[buffIndex] -= 1;
                if (modPlayer.UsingAnkh && player.itemTime % 2 == 1)
                {
                    player.buffTime[buffIndex] -= 1;
                }
            }

            base.Update(type, player, ref buffIndex);
        }

        public override void Update(int type, NPC npc, ref int buffIndex)
        {
            switch (type)
            {
                case BuffID.BrokenArmor:
                    npc.FargoSouls().BrokenArmor = true;
                    break;

                //                //case BuffID.Chilled: npc.GetGlobalNPC<NPCs.FargoSoulsGlobalNPC>().Chilled = true; break;

                case BuffID.Cursed:
                    npc.FargoSouls().Cursed = true;
                    break;

                case BuffID.Electrified:
                    npc.FargoSouls().Electrified = true;
                    break;

                case BuffID.Slimed:
                    npc.FargoSouls().Slimed = true;
                    break;

                case BuffID.Suffocation:
                    npc.FargoSouls().Suffocation = true;
                    break;

                case BuffID.OnFire:
                    if (WorldSavingSystem.EternityMode && Main.raining && npc.position.Y < Main.worldSurface * 16
                        && Framing.GetTileSafely(npc.Center).WallType == WallID.None && npc.buffTime[buffIndex] > 2)
                        npc.buffTime[buffIndex] -= 1;
                    break;

                default:
                    break;
            }
        }
    }
}