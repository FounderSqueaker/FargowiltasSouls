using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;
using static Terraria.ID.NPCID;
using Microsoft.Xna.Framework.Graphics;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.CustomEnemies.OOA;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class EModeDD2Event : ModSystem
    {
        #region Static Methods
        public static NPC GetEterniaCrystal()
        {
            int n = NPC.FindFirstNPC(NPCID.DD2EterniaCrystal);
            if (n == -1)
                return null;

            return Main.npc[n];
        }

        public static int FindClosestDD2Sentry(Vector2 position, bool ignoreJammed = true)
        {
            int n = -1;
            float dist = -1;
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.Alive() || !ProjectileID.Sets.IsADD2Turret[p.type] || (p.Eternity().Jammed && !ignoreJammed))
                    continue;

                float projDist = (p.Center - position).Length();
                if (dist == -1 || projDist < dist)
                {
                    dist = projDist;
                    n = i;
                }
            }

            return n;
        }

        public static float GetWaveProgressPercent()
        {
            if (!DD2Event.Ongoing || Main.invasionProgressMax == 1)
                return -1;

            return NPC.waveKills / Main.invasionProgressMax;
        }

        public static bool IsFinalWave()
        {
            if (!DD2Event.Ongoing)
                return false;
            if (DD2Event.OngoingDifficulty == 1 && NPC.waveNumber == 5)
                return true;
            if (DD2Event.OngoingDifficulty == 2 && NPC.waveNumber == 7)
                return true;
            if (DD2Event.OngoingDifficulty == 3 && NPC.waveNumber == 7)
                return true;
            return false;
        }

        public static bool IsDD2Boss(int type)
        {
            return type == DD2DarkMageT1 || type == DD2DarkMageT3 || type == DD2OgreT2 || type == DD2OgreT3 || type == DD2Betsy;
        }
        #endregion

        #region Detours
        public override void Load()
        {
            On_DD2Event.SpawnMonsterFromGate += EMode_SpawnMonsterFromGate;
            On_Lang.GetInvasionWaveText += EMode_GetWaveText;
            On_DD2Event.SetEnemySpawningOnHold += EMode_SetEnemySpawningOnHold;
        }

        public override void Unload()
        {
            On_DD2Event.SpawnMonsterFromGate -= EMode_SpawnMonsterFromGate;
            On_Lang.GetInvasionWaveText -= EMode_GetWaveText;
            On_DD2Event.SetEnemySpawningOnHold -= EMode_SetEnemySpawningOnHold;
        }

        public static void EMode_SpawnMonsterFromGate(On_DD2Event.orig_SpawnMonsterFromGate orig, Vector2 gateBottom)
        {
            if (!WorldSavingSystem.EternityMode)
            {
                orig(gateBottom);
                return;
            }

            switch (DD2Event.OngoingDifficulty)
            {
                case 1:
                    Difficulty_1_EMode_SpawnMonsterFromGate(gateBottom);
                    break;
                case 2:
                    Difficulty_2_EMode_SpawnMonsterFromGate(gateBottom);
                    break;
                case 3:
                    Difficulty_3_EMode_SpawnMonsterFromGate(gateBottom);
                    break;
            }
        }

        public static NetworkText EMode_GetWaveText(On_Lang.orig_GetInvasionWaveText orig, int wave, params short[] npcIds)
        {
            if (!WorldSavingSystem.EternityMode || !DD2Event.Ongoing)
            {
                return orig(wave, npcIds);
            }

            switch (DD2Event.OngoingDifficulty)
            {
                case 1:
                    return orig(wave, Difficulty_1_EMode_GetWaveText(wave));
                case 2:
                    return orig(wave, Difficulty_2_EMode_GetWaveText(wave));
                case 3:
                    return orig(wave, Difficulty_3_EMode_GetWaveText(wave));
            }
            // TODO: Add Correct Text

            return orig(wave, npcIds);
        }

        private static void EMode_SetEnemySpawningOnHold(On_DD2Event.orig_SetEnemySpawningOnHold orig, int forhowLong)
        {
            if (!WorldSavingSystem.EternityMode)
                orig(forhowLong);
            else if (IsFinalWave())
                orig(2 * forhowLong);
            else if (DD2Event.OngoingDifficulty == 3)
                orig((int)(forhowLong * 1.5f));
            else
                orig(forhowLong);
        }
        #endregion

        #region Help Methods
        #region SpawnMonsterFromGate
        private static void Difficulty_1_EMode_SpawnMonsterFromGate(Vector2 gateBottom)
        {
            int x = (int) gateBottom.X;
            int y = (int) gateBottom.Y;
            int num1 = 50;
            int num2 = 6;
            if (NPC.waveNumber > 4)
                num2 = 12;
            else if (NPC.waveNumber > 3)
                num2 = 8;

            int num3 = 6;
            if (NPC.waveNumber > 4)
                num3 = 8;

            for (int i = 1; i < Main.CurrentFrameFlags.ActivePlayersCount; i++)
            {
                num1 = (int)((double)num1 * 1.3);
                num2 = (int)((double)num2 * 1.3);
                num3 = (int)((double)num3 * 1.3);
            }

            int npc = 200;
            switch (NPC.waveNumber)
            {
                case 1: // Goblins and Goblin Bombers
                    if (NPC.CountNPCS(DD2GoblinT1) + NPC.CountNPCS(DD2GoblinBomberT1) < num1)
                        npc = OOAChanceNPC(x, y, DD2GoblinT1, DD2GoblinBomberT1, 5);
                    break;
                case 2: // Goblins, Goblin Bombers, and Wyverns
                    if (Main.rand.NextBool(6) && NPC.CountNPCS(DD2WyvernT1) < num2)
                        npc = OOANPC(x, y, DD2WyvernT1);
                    else if (NPC.CountNPCS(DD2GoblinT1) + NPC.CountNPCS(DD2GoblinBomberT1) < num1)
                        npc = OOAChanceNPC(x, y, DD2GoblinT1, DD2GoblinBomberT1, 6);
                    break;
                case 3: // Goblins, Goblin Bombers, and Javelinsts
                    if (Main.rand.NextBool(5) && NPC.CountNPCS(DD2JavelinstT1) < num2)
                        npc = OOANPC(x, y, DD2JavelinstT1);
                    else if (NPC.CountNPCS(DD2GoblinT1) + NPC.CountNPCS(DD2GoblinBomberT1) < num1)
                        npc = OOAChanceNPC(x, y, DD2GoblinT1, DD2GoblinBomberT1, 5);
                    break;
                case 4: // Goblins, Goblin Bombers, Wyverns, and Javelinsts
                    if (Main.rand.NextBool(5) && NPC.CountNPCS(DD2JavelinstT1) < num3)
                        npc = OOANPC(x, y, DD2JavelinstT1);
                    else if (Main.rand.NextBool(7) && NPC.CountNPCS(DD2WyvernT1) < num2)
                        npc = OOANPC(x, y, DD2WyvernT1);
                    else if (NPC.CountNPCS(DD2GoblinT1) + NPC.CountNPCS(DD2GoblinBomberT1) < num1)
                        npc = OOAChanceNPC(x, y, DD2GoblinT1, DD2GoblinBomberT1, 4);
                    break;
                case 5: // Goblins, Goblin Bombers, Wyverns, Javelinsts, and Dark Mage
                    if (GetWaveProgressPercent() > 0.5f && !NPC.AnyNPCs(DD2DarkMageT1))
                        npc = OOANPC(x, y, DD2DarkMageT1);
                    else if (Main.rand.NextBool(12) && NPC.CountNPCS(DD2WyvernT1) < num3)
                        npc = OOANPC(x, y, DD2WyvernT1);
                    else if (Main.rand.NextBool(4) && NPC.CountNPCS(DD2JavelinstT1) < num2)
                        npc = OOANPC(x, y, DD2JavelinstT1);
                    else if (NPC.CountNPCS(DD2GoblinT1) + NPC.CountNPCS(DD2GoblinBomberT1) < num1)
                        npc = OOAChanceNPC(x, y, DD2GoblinT1, DD2GoblinBomberT1, 3);
                    break;
                default:
                    npc = OOANPC(x, y, DD2GoblinT1);
                    break;
            }

            if (Main.dedServ && npc < 200)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc);
        }

        private static void Difficulty_2_EMode_SpawnMonsterFromGate(Vector2 gateBottom)
        {
            int x = (int)gateBottom.X;
            int y = (int)gateBottom.Y;
            int num = 50;
            int num2 = 5;
            if (NPC.waveNumber > 1)
                num2 = 8;

            if (NPC.waveNumber > 3)
                num2 = 10;

            if (NPC.waveNumber > 5)
                num2 = 12;

            int num3 = 5;
            if (NPC.waveNumber > 4)
                num3 = 7;

            int num4 = 2;
            int num5 = 8;
            if (NPC.waveNumber > 3)
                num5 = 12;

            int num6 = 3;
            if (NPC.waveNumber > 5)
                num6 = 5;

            for (int i = 1; i < Main.CurrentFrameFlags.ActivePlayersCount; i++)
            {
                num = (int)((double)num * 1.3);
                num2 = (int)((double)num2 * 1.3);
                num5 = (int)((double)num * 1.3);
                num6 = (int)((double)num * 1.35);
            }

            int num7 = 200;
            int num8 = 200;
            switch (NPC.waveNumber)
            {
                case 1: // Goblins and Javelinsts
                    if (Main.rand.NextBool(20) && NPC.CountNPCS(DD2JavelinstT2) < num2)
                        num7 = OOANPC(x, y, DD2JavelinstT2);
                    else if (NPC.CountNPCS(DD2GoblinT2) < num)
                        num7 = OOANPC(x, y, DD2GoblinT2);
                    break;
                case 2: // Goblins, Drakins, and Kobold Walkers
                    if (Main.rand.NextBool(10) && NPC.CountNPCS(DD2DrakinT2) < num6)
                        num7 = OOANPC(x, y, DD2DrakinT2);
                    else if (Main.rand.NextBool(4) && NPC.CountNPCS(DD2KoboldWalkerT2) < num5)
                        num7 = OOANPC(x, y, DD2KoboldWalkerT2);
                    else if (NPC.CountNPCS(DD2GoblinT2) < num)
                        num7 = OOANPC(x, y, DD2GoblinT2);
                    break;
                case 3: // Goblins, Wyverns, Javelinsts, and Drakins
                    if (Main.rand.NextBool(7) && NPC.CountNPCS(DD2WyvernT2) < num3)
                        num7 = OOANPC(x, y, DD2WyvernT2);
                    else if (Main.rand.NextBool(9) && NPC.CountNPCS(DD2DrakinT2) < num5)
                        num7 = OOANPC(x, y, DD2DrakinT2);
                    else if (Main.rand.NextBool(8) && NPC.CountNPCS(DD2JavelinstT2) < num2)
                        num7 = OOANPC(x, y, DD2JavelinstT2);
                    else if (NPC.CountNPCS(DD2GoblinT2) < num)
                        num7 = OOANPC(x, y, DD2GoblinT2);
                    break;
                case 4: // Goblin Bombers, Wyverns, Drakins, Kobold Walkers, and Kobold Flyers
                    if (Main.rand.NextBool(10) && NPC.CountNPCS(DD2DrakinT2) < num6)
                        num7 = OOANPC(x, y, DD2DrakinT2);
                    else if (Main.rand.NextBool(9) && NPC.CountNPCS(DD2WyvernT2) < num3)
                        num7 = OOANPC(x, y, DD2WyvernT2);
                    else if (Main.rand.NextBool(7) && NPC.CountNPCS(DD2KoboldFlyerT2) < num2)
                        num7 = OOANPC(x, y, DD2KoboldFlyerT2);
                    else if (Main.rand.NextBool(3) && NPC.CountNPCS(DD2KoboldWalkerT2) < num5)
                        num7 = OOANPC(x, y, DD2KoboldWalkerT2);
                    else if (NPC.CountNPCS(DD2GoblinBomberT2) < num)
                        num7 = OOANPC(x, y, DD2GoblinBomberT2);
                    break;
                case 5: // Goblins, Goblin Bombers, Javelinsts, Wither Beasts, Kobold Walkers, and Kobold Flyers
                    if (Main.rand.NextBool(8) && NPC.CountNPCS(DD2WitherBeastT2) < num6)
                        num7 = OOANPC(x, y, DD2WitherBeastT2);
                    else if (Main.rand.NextBool(9) && NPC.CountNPCS(DD2JavelinstT2) < num3)
                        num7 = OOANPC(x, y, DD2JavelinstT2);
                    else if (Main.rand.NextBool(4) && NPC.CountNPCS(DD2KoboldWalkerT2) + NPC.CountNPCS(DD2KoboldFlyerT2) < num5)
                        num7 = OOAChanceNPC(x, y, DD2KoboldWalkerT2, DD2KoboldFlyerT2, 4);
                    else if (NPC.CountNPCS(DD2GoblinBomberT2) + NPC.CountNPCS(DD2GoblinT2) < num)
                    {
                        if (Main.rand.NextBool(3))
                            num7 = OOANPC(x, y, DD2GoblinBomberT2);

                        num8 = OOANPC(x, y, DD2GoblinT2);
                    }
                    break;
                case 6: // Goblins, Wyverns, Javelinsts, Wither Beasts, Drakins, and Kobold Flyers
                    if (Main.rand.NextBool(7) && NPC.CountNPCS(DD2DrakinT2) < num6)
                        num7 = OOANPC(x, y, DD2DrakinT2);
                    else if (Main.rand.NextBool(15) && NPC.CountNPCS(DD2WitherBeastT2) < num4)
                        num7 = OOANPC(x, y, DD2WitherBeastT2);
                    else if (Main.rand.NextBool(9) && NPC.CountNPCS(DD2KoboldFlyerT2) < num5)
                        num7 = OOANPC(x, y, DD2KoboldFlyerT2);
                    else if (Main.rand.NextBool(9) && NPC.CountNPCS(DD2WyvernT2) < num3)
                        num7 = OOANPC(x, y, DD2WyvernT2);
                    else if (Main.rand.NextBool(3) && NPC.CountNPCS(DD2JavelinstT2) < num2)
                        num7 = OOANPC(x, y, DD2JavelinstT2);
                    if (Main.rand.NextBool(3) && NPC.CountNPCS(DD2GoblinT2) < num)
                        num8 = OOANPC(x, y, DD2GoblinT2);
                    break;
                case 7: // Goblins, Goblins Bombers, Wyverns, Javelinsts, Wither Beasts, Drakins, Kobold Walkers, and Ogre
                    {

                        if (GetWaveProgressPercent() > 0.5f && !NPC.AnyNPCs(DD2OgreT2))
                            num7 = OOANPC(x, y, DD2OgreT2);
                        else if (Main.rand.NextBool(9) && NPC.CountNPCS(DD2DrakinT2) < num6)
                            num7 = OOANPC(x, y, DD2DrakinT2);
                        else if (Main.rand.NextBool(15) && NPC.CountNPCS(DD2WitherBeastT2) + NPC.CountNPCS(DD2JavelinstT2) < num4)
                            num7 = OOAChanceNPC(x, y, DD2JavelinstT2, DD2WitherBeastT2, 4);
                        else if (Main.rand.NextBool(11) && NPC.CountNPCS(DD2KoboldWalkerT2) < num5)
                            num7 = OOANPC(x, y, DD2KoboldWalkerT2);
                        else if (Main.rand.NextBool(15) && NPC.CountNPCS(DD2WyvernT2) < num3)
                            num7 = OOANPC(x, y, DD2WyvernT2);
                        if (Main.rand.NextBool(7) && NPC.CountNPCS(DD2GoblinBomberT2) < num)
                            num8 = OOAChanceNPC(x, y, DD2GoblinT2, DD2GoblinBomberT2, 3);

                        break;
                    }
                default:
                    num7 = OOANPC(x, y, DD2GoblinT2);
                    break;
            }

            if (Main.dedServ && num7 < 200)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num7);

            if (Main.dedServ && num8 < 200)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num8);
        }

        private static void Difficulty_3_EMode_SpawnMonsterFromGate(Vector2 gateBottom)
        {
            int x = (int)gateBottom.X;
            int y = (int)gateBottom.Y;
            int num = 60;
            int num2 = 7;
            if (NPC.waveNumber > 1)
                num2 = 9;

            if (NPC.waveNumber > 3)
                num2 = 12;

            if (NPC.waveNumber > 5)
                num2 = 15;

            int num3 = 7;
            if (NPC.waveNumber > 4)
                num3 = 10;

            int num4 = 2;
            if (NPC.waveNumber > 5)
                num4 = 3;

            int num5 = 12;
            if (NPC.waveNumber > 3)
                num5 = 18;

            int num6 = 4;
            if (NPC.waveNumber > 5)
                num6 = 6;

            int num7 = 4;
            for (int i = 1; i < Main.CurrentFrameFlags.ActivePlayersCount; i++)
            {
                num = (int)((double)num * 1.3);
                num2 = (int)((double)num2 * 1.3);
                num5 = (int)((double)num * 1.3);
                num6 = (int)((double)num * 1.35);
                num7 = (int)((double)num7 * 1.3);
            }

            int num8 = 200;
            int num9 = 200;
            switch (NPC.waveNumber)
            {
                case 1: // Goblin Bombers, Kobold Walkers, Kobold Flyers
                    if (Main.rand.NextBool(8) && NPC.CountNPCS(DD2KoboldFlyerT3) < num2)
                        num8 = OOANPC(x, y, DD2KoboldFlyerT3);
                    else if (Main.rand.NextBool(4) && NPC.CountNPCS(DD2KoboldWalkerT3) < num5)
                        num8 = OOANPC(x, y, DD2KoboldWalkerT3);
                    if (NPC.CountNPCS(DD2GoblinBomberT3) < num)
                        num9 = OOANPC(x, y, DD2GoblinBomberT3);
                    break;
                case 2: // Wyverns, Wither Beasts, Kobold Flyers, and Lightning Bugs
                    if (Main.rand.NextBool(3) && NPC.CountNPCS(DD2LightningBugT3) < num7)
                        num8 = OOANPC(x, y, DD2LightningBugT3);
                    else if (NPC.CountNPCS(DD2WyvernT3) < num2)
                        num8 = OOANPC(x, y, DD2WyvernT3);
                    if (Main.rand.NextBool(5) && NPC.CountNPCS(DD2KoboldFlyerT3) < num5)
                        num9 = OOANPC(x, y, DD2KoboldFlyerT3);
                    else if (Main.rand.NextBool(5) && NPC.CountNPCS(DD2WitherBeastT3) < num6)
                        num9 = OOANPC(x, y, DD2WitherBeastT3);
                    break;
                case 3: // Goblins, Wyverns, Javelinsts, Wither Beasts, Drakins, and Kobold Walkers
                    if (Main.rand.NextBool(16) && NPC.CountNPCS(DD2DrakinT3) < num6)
                        num8 = OOANPC(x, y, DD2DrakinT3);
                    else if (Main.rand.NextBool(6) && NPC.CountNPCS(DD2KoboldWalkerT3) < num5)
                        num8 = OOANPC(x, y, DD2KoboldWalkerT3);
                    else if (Main.rand.NextBool(15) && NPC.CountNPCS(DD2WitherBeastT3) < num3)
                        num8 = OOANPC(x, y, DD2WitherBeastT3);
                    else if (Main.rand.NextBool(7) && NPC.CountNPCS(DD2WyvernT3) < num2)
                        num8 = OOANPC(x, y, DD2WyvernT3);
                    if (NPC.CountNPCS(DD2GoblinT3) + NPC.CountNPCS(DD2JavelinstT3) < num)
                        num9 = OOAChanceNPC(x, y, DD2GoblinT3, DD2JavelinstT3, 10);
                    break;
                case 4: // Goblin Bombers, Javelinsts, Drakins, Kobold Flyers, Lightning Bugs, and Dark Mage
                    if (Main.rand.NextBool(20) && !NPC.AnyNPCs(DD2DarkMageT3))
                        num8 = OOANPC(x, y, DD2DarkMageT3);
                    else if (Main.rand.NextBool(9) && NPC.CountNPCS(DD2DrakinT3) < num6)
                        num8 = OOANPC(x, y, DD2DrakinT3);
                    else if (Main.rand.NextBool(15) && NPC.CountNPCS(DD2LightningBugT3) < num3)
                        num8 = OOANPC(x, y, DD2LightningBugT3);
                    else if (Main.rand.NextBool(7) && NPC.CountNPCS(DD2JavelinstT3) < num2)
                        num8 = OOANPC(x, y, DD2JavelinstT3);
                    if (Main.rand.NextBool(9) && NPC.CountNPCS(DD2KoboldFlyerT3) < num5)
                        num9 = OOANPC(x, y, DD2KoboldFlyerT3);
                    else if (Main.rand.NextBool(4) && NPC.CountNPCS(DD2GoblinBomberT3) < num)
                        num9 = OOANPC(x, y, DD2GoblinBomberT3);
                    break;
                case 5: // Goblins, Javelinsts, Wither Beasts, Drakins, Kobold Walkers, Kobold Flyers, Lightning Bugs, and Ogre
                    if (Main.rand.NextBool(20) && !NPC.AnyNPCs(DD2OgreT3))
                        num8 = OOANPC(x, y, DD2OgreT3);
                    else if (Main.rand.NextBool(10) && NPC.CountNPCS(DD2LightningBugT3) < num7)
                        num8 = OOANPC(x, y, DD2LightningBugT3);
                    else if (Main.rand.NextBool(17) && NPC.CountNPCS(DD2WitherBeastT3) < num4)
                        num8 = OOANPC(x, y, DD2WitherBeastT3);
                    else if (Main.rand.NextBool(12) && NPC.CountNPCS(DD2DrakinT3) < num6)
                        num8 = OOANPC(x, y, DD2DrakinT3);
                    else if (Main.rand.NextBool(11) && NPC.CountNPCS(DD2WitherBeastT3) + NPC.CountNPCS(DD2JavelinstT3) < num4)
                        num8 = OOAChanceNPC(x, y, DD2JavelinstT3, DD2WitherBeastT3, 8);
                    if (Main.rand.NextBool(10) && NPC.CountNPCS(DD2KoboldWalkerT3) + NPC.CountNPCS(DD2KoboldFlyerT3) < num5)
                        num9 = OOAChanceNPC(x, y, DD2KoboldWalkerT3, DD2KoboldFlyerT3, 4);
                    else if (Main.rand.NextBool(5) && NPC.CountNPCS(DD2GoblinT3) < num)
                        num9 = OOANPC(x, y, DD2GoblinT3);
                    break;
                case 6: // Goblin Bombers, Wyverns, Drakins, Kobold Flyers, Lightning Bugs, Dark Mage, and Ogre
                    if (Main.rand.NextBool(20) && !NPC.AnyNPCs(DD2OgreT3))
                        num8 = OOANPC(x, y, DD2OgreT3);
                    else if (Main.rand.NextBool(20) && !NPC.AnyNPCs(DD2DarkMageT3))
                        num8 = OOANPC(x, y, DD2DarkMageT3);
                    else if (Main.rand.NextBool(14) && NPC.CountNPCS(DD2DrakinT3) < num6)
                        num8 = OOANPC(x, y, DD2DrakinT3);
                    else if (Main.rand.NextBool(14) && NPC.CountNPCS(DD2LightningBugT3) < num7)
                        num8 = OOANPC(x, y, DD2LightningBugT3);
                    else if (Main.rand.NextBool(7) && NPC.CountNPCS(DD2KoboldFlyerT3) < num5)
                        num8 = OOANPC(x, y, DD2KoboldFlyerT3);
                    if (Main.rand.NextBool(11) && NPC.CountNPCS(DD2WyvernT3) < num2)
                        num9 = OOANPC(x, y, DD2WyvernT3);
                    else if (Main.rand.NextBool(3) && NPC.CountNPCS(DD2GoblinBomberT3) < num)
                        num9 = OOANPC(x, y, DD2GoblinBomberT3);
                    break;
                case 7: // Betsy Wave: Goblins, Goblin Bombers, Wyverns, Drakins, Wither Beasts, Dark Mage, and Ogre
                    if (BetsyBlockSpawn)
                        break;

                    float percent = (float)BetsyOOAKills / (float)BetsyOOAKillsNeeded;
                    if (percent >= 1)
                        break;

                    if (percent >= 0.3f && percent <= 0.4f && !NPC.AnyNPCs(DD2DarkMageT3)) // spawn dark mage ~30-40%
                    {
                        num8 = OOANPC(x, y, DD2DarkMageT3);
                        break;
                    }
                    if (percent >= 0.65f && percent <= 0.75f && !NPC.AnyNPCs(DD2OgreT3)) // spawn ogre ~65-75%
                    {
                        num8 = OOANPC(x, y, DD2OgreT3);
                        break;
                    }

                    if (Main.rand.NextBool(20) && NPC.CountNPCS(DD2DrakinT3) < num6)
                    {
                        num8 = OOANPC(x, y, DD2DrakinT3);
                    }
                    else if (Main.rand.NextBool(17) && NPC.CountNPCS(DD2WitherBeastT3) < num4)
                    {
                        num8 =  OOANPC(x, y, DD2WitherBeastT3);
                    }
                    else if (Main.rand.NextBool(20) && NPC.CountNPCS(DD2WyvernT3) < num2)
                    {
                        num8 = OOANPC(x, y, DD2WyvernT3);
                    }
                    else if (NPC.CountNPCS(DD2GoblinT3) + NPC.CountNPCS(DD2GoblinBomberT3) < num)
                    {
                        if (Main.rand.Next(5) == 0)
                            num8 = OOANPC(x, y, DD2GoblinBomberT3);

                        num9 = OOANPC(x, y, DD2GoblinT3);
                    }
                    break;
                default:
                    num8 = OOANPC(x, y, DD2GoblinT3);
                    break;
            }

            if (Main.dedServ && num8 < 200)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num8);

            if (Main.dedServ && num9 < 200)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num9);
        }

        private static int OOANPC(int x, int y, int type)
        {
            return NPC.NewNPC(GetSpawnSource_OldOnesArmy(), x, y, type);
        }
        private static int OOAChanceNPC(int x, int y, int commonType, int rareType, int chanceDenom)
        {
            if (Main.rand.NextBool(chanceDenom))
                return OOANPC(x, y, rareType);
            else
                return OOANPC(x, y, commonType);
        }
        #endregion

        #region GetWaveText
        private static short[] Difficulty_1_EMode_GetWaveText(int wave)
        {
            switch(wave)
            {
                case 1:
                    return [
                        DD2GoblinT1,
                        DD2GoblinBomberT1
                    ];
                case 2:
                    return [
                        DD2GoblinT1,
                        DD2GoblinBomberT1,
                        DD2WyvernT1
                    ];
                case 3:
                    return [
                        DD2GoblinT1,
                        DD2GoblinBomberT1,
                        DD2JavelinstT1
                    ];
                case 4:
                    return [
                        DD2GoblinT1,
                        DD2GoblinBomberT1,
                        DD2WyvernT1,
                        DD2JavelinstT1
                    ];
                case 5:
                    return [
                        DD2GoblinT1,
                        DD2GoblinBomberT1,
                        DD2WyvernT1,
                        DD2JavelinstT1,
                        DD2DarkMageT1
                    ];
            }
            return [];
        }

        private static short[] Difficulty_2_EMode_GetWaveText(int wave)
        {
            switch (wave)
            {
                case 1:
                    return [
                        DD2GoblinT2,
                        DD2JavelinstT2
                    ];
                case 2:
                    return [
                        DD2GoblinT2,
                        DD2DrakinT2,
                        DD2KoboldWalkerT2
                    ];
                case 3:
                    return [
                        DD2GoblinT2,
                        DD2WyvernT2,
                        DD2JavelinstT2,
                        DD2DrakinT2
                        
                    ];
                case 4:
                    return [
                        DD2GoblinBomberT2,
                        DD2WyvernT2,
                        DD2DrakinT2,
                        DD2KoboldWalkerT2,
                        DD2KoboldFlyerT2
                        
                    ];
                case 5:
                    return [
                        DD2GoblinT2,
                        DD2GoblinBomberT2,
                        DD2JavelinstT2,
                        DD2WitherBeastT2,
                        DD2KoboldWalkerT2,
                        DD2KoboldFlyerT2
                        
                    ];
                case 6:
                    return [
                        DD2GoblinT2,
                        DD2WyvernT2,
                        DD2JavelinstT2,
                        DD2WitherBeastT2,
                        DD2DrakinT2,
                        DD2KoboldFlyerT2
                    ];
                case 7:
                    return [ 
                        DD2GoblinT2,
                        DD2GoblinBomberT2,
                        DD2WyvernT2,
                        DD2JavelinstT2,
                        DD2DrakinT2,
                        DD2KoboldWalkerT2,
                        DD2OgreT2    
                    ];
            }
            return [];
        }

        private static short[] Difficulty_3_EMode_GetWaveText(int wave)
        {
            switch (wave)
            {
                case 1:
                    return [
                        DD2GoblinBomberT3,
                        DD2KoboldWalkerT3,
                        DD2KoboldFlyerT3
                    ];
                case 2:
                    return [
                        DD2WyvernT3,
                        DD2WitherBeastT3,
                        DD2KoboldFlyerT3,
                        DD2LightningBugT3
                    ];
                case 3:
                    return [
                        DD2GoblinT3,
                        DD2WyvernT3,
                        DD2JavelinstT3,
                        DD2WitherBeastT3,
                        DD2DrakinT3,
                        DD2KoboldWalkerT3
                    ];
                case 4:
                    return [
                        DD2GoblinBomberT3,
                        DD2JavelinstT3,
                        DD2DrakinT3,
                        DD2KoboldFlyerT3,
                        DD2LightningBugT3,
                        DD2DarkMageT3

                    ];
                case 5:
                    return [
                        DD2GoblinT3,
                        DD2JavelinstT3,
                        DD2WitherBeastT3,
                        DD2DrakinT3,
                        DD2KoboldWalkerT3,
                        DD2KoboldFlyerT3,
                        DD2LightningBugT3,
                        DD2OgreT3

                    ];
                case 6:
                    return [
                        DD2GoblinBomberT3,
                        DD2WyvernT3,
                        DD2DrakinT3,
                        DD2KoboldFlyerT3,
                        DD2LightningBugT3,
                        DD2DarkMageT3,
                        DD2OgreT3
                    ];
                case 7:
                    return [
                        DD2Betsy
                    ];
            }
            return [];
        }
        #endregion
        #endregion

        private static IEntitySource GetSpawnSource_OldOnesArmy() => new EntitySource_OldOnesArmy();

        public static int BetsyOOAKillsNeeded = -1;
        public static int BetsyOOAKills = -1;
        public static bool BetsyBlockSpawn = false;

        public override void PreUpdateNPCs()
        {
            if (!NPC.AnyNPCs(DD2Betsy))
            {
                BetsyBlockSpawn = false;
                BetsyOOAKills = -1;
                BetsyOOAKillsNeeded = -1;
            }
            base.PreUpdateNPCs();
        }
    }

    public class EModeDD2GlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public static bool[] IsExcludedOOANPC = Sets.Factory.CreateBoolSet(false, [NPCID.DD2LanePortal, NPCID.DD2EterniaCrystal, NPCID.DD2Betsy, ModContent.NPCType<BowlingSpit>(), ModContent.NPCType<TavernkeepPortal>()]);

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
            => Sets.BelongsToInvasionOldOnesArmy[entity.type] && !IsExcludedOOANPC[entity.type];

        public static bool IsInstance(NPC npc) => Sets.BelongsToInvasionOldOnesArmy[npc.type] && !IsExcludedOOANPC[npc.type];

        public bool DrakinBuff;
        public int SpitBall = -1;
        public int InvulTimer = 0;

        public override bool PreAI(NPC npc)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.PreAI(npc);

            if (SpitBall != -1)
            {
                NPC spit = Main.npc[SpitBall];
                npc.Center = spit.Center;
                npc.dontTakeDamage = true;
                npc.ShowNameOnHover = false;
                return false;
            }

            int invulTime = 90;
            InvulTimer++;
            if (InvulTimer < invulTime)
                npc.dontTakeDamage = true;
            else if (InvulTimer == invulTime)
                npc.dontTakeDamage = false;

            return base.PreAI(npc);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (SpitBall != -1)
                return false;
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override void ResetEffects(NPC npc)
        {
            DrakinBuff = false;
            if (SpitBall != -1 && !Main.npc[SpitBall].active)
            {
                SpitBall = -1;
                npc.dontTakeDamage = false;
                npc.ShowNameOnHover = true;
            }
        }

        public override void OnKill(NPC npc)
        {
            if (EModeDD2Event.BetsyOOAKills >= 0)
                EModeDD2Event.BetsyOOAKills += EModeDD2Event.IsDD2Boss(npc.type) ? 5 : 1;

            base.OnKill(npc);
        }
    }
}
