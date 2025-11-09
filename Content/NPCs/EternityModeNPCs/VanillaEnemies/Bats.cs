using System.IO;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies
{
    public class Bats : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.JungleBat,
            NPCID.IceBat,
            NPCID.SporeBat
        );
        public int HangState;
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(HangState);
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            HangState = binaryReader.ReadInt32();
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            //if (Main.rand.NextBool(4)) Horde(npc, Main.rand.Next(5) + 1);
        }
        /*public override bool SafePreAI(NPC npc)
        {
            Point point = FindCeiling(npc.Center.ToTileCoordinates());
            Vector2 ceil = point.ToWorldCoordinates();
            Dust d = Dust.NewDustPerfect(ceil, DustID.BlueTorch); d.velocity = Vector2.Zero; d.noGravity = true;

            if (npc.life == npc.lifeMax && npc.Center.Distance(ceil) <= 160 && WorldGen.TileIsExposedToAir(point.X, point.Y) && Collision.CanHitLine(npc.Center, npc.width, npc.height, ceil, 0, 0))
            {
                if (npc.Center.Distance(ceil) > 16)
                {
                    HangState = 0;
                    npc.noTileCollide = true;
                    npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, ceil, npc.velocity, 0.5f, 0.5f);
                }
                else
                {
                    HangState = 1;
                    npc.noTileCollide = false;
                    npc.velocity = Vector2.Zero;
                    npc.Center = ceil;
                }
                return false;
            }
            else return base.SafePreAI(npc);
        }*/
        public override void AI(NPC npc)
        {
            base.AI(npc);
        }

        public static Point FindCeiling(Point p)
        {
            if (WorldGen.SolidTile(p))
            {
                while (WorldGen.SolidTile(p.X, p.Y + 1) && p.Y >= 1)
                    p.Y++;
            }
            else
            {
                while (!WorldGen.SolidTile(p.X, p.Y - 1) && p.Y < Main.maxTilesY)
                    p.Y--;
            }
            return p;
        }

        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (HangState == 1)
                npc.frame.Y = 4 * frameHeight;
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            //target.AddBuff(BuffID.Bleeding, 300);
        }
    }
}
