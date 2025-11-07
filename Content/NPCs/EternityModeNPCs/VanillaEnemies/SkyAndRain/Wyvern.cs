using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.SkyAndRain;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.SkyAndRain
{
    public class Wyvern : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.WyvernHead);

        public int AttackTimer;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write(AttackTimer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            AttackTimer = binaryReader.ReadInt32();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            AttackTimer = Main.rand.Next(180);
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            //if (Main.hardMode && Main.rand.NextBool(10) && npc.FargoSouls().CanHordeSplit)
                //EModeGlobalNPC.Horde(npc, 2);
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (++AttackTimer > 120)
            {
                if (npc.Distance(Main.player[npc.target].Center) < 160 && FargoSoulsUtil.HostCheck && npc.velocity != Vector2.Zero)
                {
                    AttackTimer = 0;
                    int max = Main.hardMode ? 12 : 8;
                    Vector2 vel = Vector2.Normalize(npc.velocity) * 1.5f;
                    for (int i = 0; i < max; i++)
                    {
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, vel.RotatedBy(2f * MathHelper.Pi / max * i),
                            ModContent.ProjectileType<LightBall>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 0f, Main.myPlayer, 0f, .01f * npc.direction);
                    }
                }
            }
        }
    }

    public class WyvernSegment : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.WyvernBody,
            NPCID.WyvernBody2,
            NPCID.WyvernBody3,
            NPCID.WyvernHead,
            NPCID.WyvernLegs,
            NPCID.WyvernTail
        );

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);
            target.AddBuff(ModContent.BuffType<ClippedWingsBuff>(), 240);
        }
    }
}
