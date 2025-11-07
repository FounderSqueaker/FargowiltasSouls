using FargowiltasSouls.Content.Projectiles.Eternity.Environment;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Crimson
{
    public class FaceMonster : EModeNPCBehaviour
    {
        public int BloodTimer = 0;
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.FaceMonster);

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(BloodTimer);
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            BloodTimer = binaryReader.ReadInt32();
        }
        public override bool SafePreAI(NPC npc)
        {
            if (BloodTimer == 0)
            {
                BloodTimer = Main.rand.Next(180);
                npc.netUpdate = true;
            }
            bool lineOfSight = npc.HasPlayerTarget && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0);
            if (lineOfSight)
                BloodTimer++;
            if (BloodTimer > 60 * 7 && lineOfSight)
            {
                BloodTimer = 0;
                if (FargoSoulsUtil.HostCheck)
                {
                    for (int i = 0; i < Main.rand.Next(1, 4); i++)
                    {
                        Projectile.NewProjectileDirect(npc.GetSource_Death(), npc.Center, new Vector2(0, Main.rand.NextFloat(-14, -4)).RotatedByRandom(MathHelper.ToRadians(35)), ModContent.ProjectileType<BloodDroplet>(), 0, 0);
                    }
                }
                
            }
            return base.SafePreAI(npc);
        }
    }
}
