using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies
{
    public class EarlyBirdEnemies : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.WyvernHead,
            NPCID.WyvernBody,
            NPCID.WyvernBody2,
            NPCID.WyvernBody3,
            NPCID.WyvernLegs,
            NPCID.WyvernTail,
            NPCID.Mimic,
            NPCID.IceMimic,
            NPCID.Medusa,
            NPCID.PigronCorruption,
            NPCID.PigronCrimson,
            NPCID.PigronHallow,
            NPCID.AnglerFish,
            NPCID.AngryNimbus,
            NPCID.MushiLadybug,
            NPCID.AnomuraFungus,
            NPCID.ZombieMushroom,
            NPCID.ZombieMushroomHat,
            NPCID.IceGolem,
            NPCID.SandElemental,
            NPCID.RockGolem
        );

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);
            if (!Main.hardMode)
            {
                switch (npc.type)
                {
                    //always round if dividing/multiplying an int by a non-whole number
                    case NPCID.IceGolem:
                    case NPCID.SandElemental:
                        npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.3);
                        npc.defense /= 4;
                        npc.damage /= 2;
                        break;
                    case NPCID.WyvernHead:
                    case NPCID.WyvernBody:
                    case NPCID.WyvernBody2:
                    case NPCID.WyvernBody3:
                    case NPCID.WyvernLegs:
                    case NPCID.WyvernTail:
                        npc.lifeMax /= 2;
                        npc.defense /= 2;
                        npc.damage /= 2;
                        break;
                    case NPCID.Mimic:
                    case NPCID.IceMimic:
                        if (!Main.remixWorld)
                        {
                            npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.6);
                            npc.damage /= 2;
                            npc.defense /= 2;
                            npc.knockBackResist *= 1.25f;
                            npc.value /= 2;
                        }
                        break;
                    case NPCID.AngryNimbus:
                        npc.lifeMax /= 3;
                        npc.damage /= 2;
                        npc.defense /= 4;
                        npc.knockBackResist *= 2;
                        break;
                    case NPCID.Medusa:
                        npc.lifeMax /= 2;
                        npc.damage = (int)Math.Round(npc.damage / 1.5);
                        npc.defense = (int)Math.Round(npc.defense / 1.5);
                        break;
                    case NPCID.ZombieMushroom:
                    case NPCID.ZombieMushroomHat:
                        //also applies to weird prehm surface variants
                        npc.lifeMax /= 2;
                        npc.damage /= 2;
                        npc.knockBackResist *= 1.15f;
                        break;
                    case NPCID.AnomuraFungus:
                    case NPCID.MushiLadybug:
                        npc.lifeMax /= 2;
                        npc.damage /= 2;
                        npc.defense /= 2;
                        npc.knockBackResist *= 1.5f;
                        break;
                    case NPCID.PigronCorruption:
                    case NPCID.PigronCrimson:
                    case NPCID.PigronHallow:
                        npc.lifeMax = (int)Math.Round(npc.lifeMax / 1.5);
                        npc.damage /= 2;
                        npc.defense = (int)Math.Round(npc.defense / 1.5);
                        break;
                    case NPCID.RockGolem:
                        npc.lifeMax /= 4;
                        npc.defense = (int)Math.Round(npc.defense / 3.5);
                        npc.damage /= 3;
                        npc.value = (int)Math.Round(npc.value / 1.5);
                        npc.knockBackResist *= 2f;
                        break;
                    case NPCID.AnglerFish:
                        npc.lifeMax = (int)Math.Round(npc.lifeMax / 1.5);
                        npc.damage /= 3;
                        npc.defense /= 4;
                        npc.value /= 2;
                        npc.knockBackResist *= 2f;
                        break;
                    default: break;
                }
            }
        }
    }
}
