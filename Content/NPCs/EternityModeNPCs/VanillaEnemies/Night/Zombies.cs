using System.Collections.Generic;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Night
{
    public class Zombies : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.Zombie,
            NPCID.ArmedZombie,
            NPCID.ArmedZombieCenx,
            NPCID.ArmedZombiePincussion,
            NPCID.ArmedZombieSlimed,
            NPCID.ArmedZombieSwamp,
            NPCID.ArmedZombieTwiggy,
            NPCID.BaldZombie,
            NPCID.FemaleZombie,
            NPCID.PincushionZombie,
            NPCID.SlimedZombie,
            NPCID.TwiggyZombie,
            NPCID.ZombiePixie,
            NPCID.ZombieRaincoat,
            NPCID.ZombieSuperman,
            NPCID.ZombieSweater,
            NPCID.ZombieXmas,
            NPCID.SwampZombie,
            NPCID.SmallSwampZombie,
            NPCID.BigSwampZombie,
            NPCID.TorchZombie,
            NPCID.ArmedTorchZombie,
            NPCID.ZombieDoctor,
            NPCID.ZombieEskimo,
            NPCID.ArmedZombieEskimo,
            NPCID.ZombieMushroom,
            NPCID.ZombieMushroomHat,
            NPCID.ZombieElf,
            NPCID.ZombieElfBeard,
            NPCID.ZombieElfGirl,
            NPCID.SmallSlimedZombie,
            NPCID.BigSlimedZombie,
            NPCID.ZombieMerman,
            NPCID.TheGroom,
            NPCID.TheBride,
            NPCID.MaggotZombie
        );
        public static List<int> RegularZombies = //no halloween/christmas or biome variants 
        [
            NPCID.Zombie,
            NPCID.ArmedZombie,
            NPCID.ArmedZombieCenx,
            NPCID.ArmedZombiePincussion,
            NPCID.ArmedZombieSlimed,
            NPCID.ArmedZombieSwamp,
            NPCID.ArmedZombieTwiggy,
            NPCID.BaldZombie,
            NPCID.FemaleZombie,
            NPCID.PincushionZombie,
            NPCID.SlimedZombie,
            NPCID.TwiggyZombie,
            NPCID.SwampZombie,
            NPCID.SmallSwampZombie,
            NPCID.BigSwampZombie,
            NPCID.TorchZombie,
            NPCID.ArmedTorchZombie,
            NPCID.SmallSlimedZombie,
            NPCID.BigSlimedZombie
        ];

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);
            if (Main.rand.NextBool(8) && npc.FargoSouls().CanHordeSplit && npc.type != NPCID.ZombieMerman && npc.type != NPCID.TheGroom && npc.type != NPCID.TheBride)
                EModeGlobalNPC.Horde(npc, 4);

            if (RegularZombies.Contains(npc.type) && Main.raining && Main.LocalPlayer.ZoneOverworldHeight)
                npc.Transform(NPCID.ZombieRaincoat);
            //if (npc.type != NPCID.ZombieEskimo && Main.LocalPlayer.ZoneSnow && Main.rand.NextBool())
                //npc.Transform(NPCID.ZombieEskimo);
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (npc.type == NPCID.ZombieRaincoat)
            {
                if (npc.wet)
                {
                    //slime ai
                    npc.aiStyle = 1;
                }
                else
                {
                    //zombie ai
                    npc.aiStyle = 3;
                }
            }

            if (npc.ai[2] >= 45f && npc.ai[3] == 0f && FargoSoulsUtil.HostCheck)
            {
                int tileX = (int)(npc.position.X + npc.width / 2 + 15 * npc.direction) / 16;
                int tileY = (int)(npc.position.Y + npc.height - 15) / 16 - 1;
                Tile tile = Framing.GetTileSafely(tileX, tileY);
                if (tile.TileType == TileID.ClosedDoor || tile.TileType == TileID.TallGateClosed)
                {
                    //WorldGen.KillTile(tileX, tileY);
                    WorldGen.OpenDoor(tileX, tileY, npc.direction);
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, tileX, tileY, npc.direction);
                }
            }
        }



        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<RottingBuff>(), 300);
            if (npc.type == NPCID.TheBride || npc.type == NPCID.TheGroom)
                target.AddBuff(ModContent.BuffType<UnluckyBuff>(), 60 * 30);
        }

        public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
        {
            base.OnHitNPC(npc, target, hit);
        }

        public override void OnKill(NPC npc)
        {
            base.OnKill(npc);

            switch (npc.type)
            {
                case NPCID.SmallSlimedZombie:
                case NPCID.SlimedZombie:
                case NPCID.BigSlimedZombie:
                case NPCID.ArmedZombieSlimed:
                    if (Main.rand.NextBool() && FargoSoulsUtil.HostCheck)
                        FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, NPCID.BlueSlime);
                    break;

                default:
                    break;
            }
        }
    }
}
