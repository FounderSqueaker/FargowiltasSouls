using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Patreon.DanielTheRobot;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Cavern;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Cavern
{
    public class GraniteElemental : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.GraniteFlyer);

        public int AttackTimer = 0;
        public int DamageStored = 0;
        public bool wasInvul = false;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(AttackTimer);
            binaryWriter.Write(DamageStored);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            AttackTimer = binaryReader.ReadInt32();
            DamageStored = binaryReader.ReadInt32();
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            // Absorb Projectiles
            if (npc.dontTakeDamage)
            {
                float distance = 2f * 16;

                Main.projectile.Where(x => EModeGlobalProjectile.CanBeAbsorbed(x)).ToList().ForEach(x =>
                {
                    if (Vector2.Distance(x.Center, npc.Center) <= distance)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            float rot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                            int dustId = Dust.NewDust(new Vector2(x.position.X, x.position.Y + 2f), x.width, x.height + 5, DustID.BlueTorch, x.velocity.X * 0.2f, x.velocity.Y * 0.2f, 100, default, 1.5f);
                            Main.dust[dustId].noGravity = true;
                            float scale = Main.rand.NextFloat(2, 4);

                            new SparkParticle(npc.Center + 5 * Vector2.UnitX.RotatedBy(rot), scale * Vector2.UnitX.RotatedBy(rot), Color.Lerp(Color.SkyBlue, Color.Blue, 0.8f), 0.1f * scale, 15).Spawn();
                        }

                        SoundEngine.PlaySound(SoundID.MaxMana with {MaxInstances = 2}, npc.Center);
                        DamageStored += x.damage;
                        x.active = false;
                    }
                });
            }
            // EModeGlobalNPC.CustomReflect(npc, DustID.Granite, 2);
        }

        public override void SafePostAI(NPC npc)
        {
            base.SafePostAI(npc);
            if (npc.dontTakeDamage)
            {
                npc.velocity *= 0.99f;
                npc.noGravity = true;
                wasInvul = true;

                float randRot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                new SparkParticle(npc.Center + 50 * Vector2.UnitX.RotatedBy(randRot), -8 * Vector2.UnitX.RotatedBy(randRot), Color.Lerp(Color.SkyBlue, Color.Blue, 0.6f), 0.2f, 5).Spawn();
                Lighting.AddLight(npc.Center, TorchID.Blue);
            }
            else
            {
                if (wasInvul)
                {
                    if (npc.HasValidTarget)
                    {
                        SoundEngine.PlaySound(SoundID.Item72, npc.Center);
                        float rot = (Main.player[npc.target].Center - npc.Center).ToRotation();
                        if (FargoSoulsUtil.HostCheck)
                        {
                            float scale = 1 + (DamageStored / 80f);
                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, 0.5f * Vector2.UnitX.RotatedBy(rot), ModContent.ProjectileType<GraniteBolt>(), (int)(scale * npc.damage / 6), 0f);
                        }
                        npc.velocity = -Vector2.UnitX.RotatedBy(rot);
                    }

                    wasInvul = false;
                }
                DamageStored = 0;
                AttackTimer = 0;
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            //target.FargoSouls().AddBuffNoStack(BuffID.Stoned, 60);
        }
    }
}
