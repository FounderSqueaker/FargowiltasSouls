using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2WitherBeast : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.DD2WitherBeastT2,
            NPCID.DD2WitherBeastT3
        );

        public int Timer = 0;
        public int Shield = -1;

        public override void SetDefaults(NPC entity)
        {
            base.SetDefaults(entity);
            entity.knockBackResist = 0f;
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(Timer);
            binaryWriter.Write(Shield);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            Timer = binaryReader.ReadInt32();
            Shield = binaryReader.ReadInt32();
        }

        public override bool SafePreAI(NPC npc)
        {
            if (!DD2Event.Ongoing)
                return base.SafePreAI(npc);

            Timer++;
            if (Timer > 60 && npc.ai[0] == 0)
                npc.ai[0] = 109;

            if (npc.ai[0] == 1)
            {
                if (Shield == -1)
                {
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = -0.5f, Volume = 2f }, npc.Center);
                    for (int i = 0; i < 10; i++)
                    {
                        float rot = MathHelper.TwoPi * i / 10f;
                        new SmallSparkle(npc.Center + Vector2.UnitX, 1.5f * Vector2.UnitX.RotatedBy(rot), Color.Purple, 1f, 15).Spawn();
                    }
                    Shield = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center - 10 * Vector2.UnitY, Vector2.Zero, ModContent.ProjectileType<OOAForcefield>(), 0, 0f, ai1: npc.HorizontalDirectionTo(EModeDD2Event.GetEterniaCrystal().Center));
                }

                if (npc.Center.Distance(EModeDD2Event.GetEterniaCrystal().Center) > 900)
                {
                    int j = (int)(npc.Bottom.X / 16);
                    int y = (int)((npc.Bottom.Y + 1) / 16);
                    if (Timer % 12 == 0)
                    {
                        npc.velocity.X = 0.8f * npc.direction;
                        SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { Volume = 0.7f }, npc.Center);
                        for (int x = j - 2; x <= j + 2; x++)
                        {
                            WorldGen.KillTile(x, y, true, true);
                            WorldGen.KillTile(x, y, true, true);
                        }
                    }
                    new SparkParticle(npc.Top + 2 * -npc.direction * npc.width / 2 * Vector2.UnitX + Main.rand.NextFloat(0, npc.height) * Vector2.UnitY, -npc.direction * Vector2.UnitX, Color.Pink, 0.1f, 4).Spawn();
                }
                npc.ai[1] = 30; // kill heal
            }
            return base.SafePreAI(npc);
        }

        public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
        {
            if (projectile.Eternity().isADD2Proj)
                return false;
            return base.CanBeHitByProjectile(npc, projectile);
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            //EModeGlobalNPC.Aura(npc, 300, BuffID.WitheredArmor, false, 119);
            //EModeGlobalNPC.Aura(npc, 300, BuffID.WitheredWeapon, false, 14);
        }
    }
}
