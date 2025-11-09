using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Cavern
{
    public class UndeadMiner : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.UndeadMiner);

        public int Counter;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(Counter);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            Counter = binaryReader.ReadInt32();
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (Counter == 180)
            {
                if (npc.DeathSound != null)
                    SoundEngine.PlaySound(npc.DeathSound.Value, npc.Center);
                FargoSoulsUtil.DustRing(npc.Center, 32, DustID.Teleporter, 5f, default, 2f);
            }

            if (++Counter > 240)
            {
                Counter = 0;
                NetSync(npc);

                if (FargoSoulsUtil.HostCheck && npc.HasValidTarget && npc.Distance(Main.player[npc.target].Center) < 800)
                {
                    Vector2 speed = Main.player[npc.target].Center - npc.Center;
                    speed.Y -= Math.Abs(speed.X) * 0.25f; //account for gravity
                    speed.X += Main.rand.Next(-20, 21);
                    speed.Y += Main.rand.Next(-20, 21);
                    speed.Normalize();
                    speed *= 12f;
                    int p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, speed, ProjectileID.BombSkeletronPrime, FargoSoulsUtil.ScaledProjectileDamage(npc.damage, 1.25f), 0f, Main.myPlayer);
                    if (p.IsWithinBounds(Main.maxProjectiles))
                    {
                        //Main.projectile[p].damage = 1;
                    }
                        
                }
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<LethargicBuff>(), 600);
            target.AddBuff(BuffID.Blackout, 300);
            target.AddBuff(BuffID.NoBuilding, 300);
        }
    }
}
