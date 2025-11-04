using System;
using System.IO;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.BloodMoon
{
    public class BloodEel : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(NPCID.BloodEelHead);

        public int AttackTimer;
        public int State;
        public bool Cycle;
        public float Dist;
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write7BitEncodedInt(AttackTimer);
            binaryWriter.Write7BitEncodedInt(State);
            binaryWriter.Write(Cycle);
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            AttackTimer = binaryReader.Read7BitEncodedInt();
            State = binaryReader.Read7BitEncodedInt();
            Cycle = binaryReader.ReadBoolean();
        }
        public override void SetDefaults(NPC npc)
        {
            npc.lifeMax *= 2;
        }
        public override bool SafePreAI(NPC npc)
        {
            Player target = Main.player[npc.target];
            if (State != 0)
            { //vanilla shit
                if (npc.velocity.X < 0f) npc.spriteDirection = 1;
                else if (npc.velocity.X > 0f) npc.spriteDirection = -1;
                if (npc.ai[0] != 0)
                {
                    npc.ai[3] = npc.whoAmI;
                    npc.realLife = npc.whoAmI;
                    npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
                }

                if (State == 1)
                {
                    Dist = Math.Sign(npc.Center.X - target.Center.X);
                    Vector2 pos = target.Center + new Vector2(800 * Dist, -300);
                    if (AttackTimer < 120) AttackTimer++;
                    Movement(npc, pos);
                    if (npc.Distance(pos) < 25)
                    {
                        State = 2;
                        AttackTimer = 0;
                        npc.netUpdate = true;
                    }
                }
                if (State == 2)
                {
                    Vector2 pos2 = target.Center + new Vector2(-800 * Dist, -300);
                    Movement(npc, pos2);
                    if (AttackTimer++ % 13 == 0 && FargoSoulsUtil.HostCheck)
                    {
                        SoundEngine.PlaySound(SoundID.Item171, npc.Center);
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, new Vector2(0, -10), ModContent.ProjectileType<EelChumBucket>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.5f), 0, Main.myPlayer);
                    }
                    if (npc.Distance(pos2) < 25)
                    {
                        State = 0;
                        AttackTimer = 0;
                        Dist = 0;
                        npc.netUpdate = true;
                    }
                }
                if (State == 3)
                {
                    Dist = Math.Sign(npc.Center.X - target.Center.X);
                    Vector2 pos = target.Center + new Vector2(300 * Dist, -200);
                    if (AttackTimer < 120) AttackTimer++;
                    Movement(npc, pos);
                    if (npc.Distance(pos) < 25)
                    {
                        State = 4;
                        AttackTimer = 0;
                        npc.netUpdate = true;
                    }
                }
                if (State == 4)
                {
                    Vector2 acceleration = Vector2.Normalize(npc.velocity).RotatedBy(-Math.PI / 2) * 16f * 16f / 150;
                    npc.velocity = Vector2.Normalize(npc.velocity) * 16 + acceleration;
                    AttackTimer++;

                    float randRot = Main.rand.NextFloat(MathHelper.Pi, MathHelper.TwoPi);
                    new SparkParticle(npc.Center + npc.velocity + 50 * Vector2.UnitX.RotatedBy(npc.rotation + randRot + MathHelper.TwoPi), -3 * Vector2.UnitX.RotatedBy(npc.rotation + randRot + MathHelper.TwoPi), Color.DarkRed, 0.4f, 20).Spawn();
                    Lighting.AddLight(npc.Center, TorchID.Red);

                    float rotgoal = npc.SafeDirectionTo(target.Center).ToRotation() + MathHelper.PiOver2;
                    if ((AttackTimer >= 90 && npc.rotation <= rotgoal + 0.2 && npc.rotation >= rotgoal - 0.2) || AttackTimer >= 300)
                    {
                        State = 0;
                        AttackTimer = 0;
                        if (FargoSoulsUtil.HostCheck)
                        {
                            SoundEngine.PlaySound(SoundID.NPCDeath13 with { Pitch = 0.8f }, npc.Center);
                            for (int i = 0; i < 6; i++)
                            {
                                Vector2 vel = (30 - (Math.Abs(i-3)*3)) * Vector2.UnitX.RotatedBy(npc.rotation - MathHelper.PiOver2).RotatedBy(Math.PI / 10 * i - MathHelper.PiOver4);
                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, vel, ModContent.ProjectileType<EelDrippler>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.5f), 0, Main.myPlayer);
                            }
                        }
                    }
                }
                return false;
            }
            return base.SafePreAI(npc);
        }

        public override void AI(NPC npc)
        {
            Player target = Main.player[npc.target];
            AttackTimer++;
            npc.ai[1] = -1;
            if (target.Alive() && npc.Distance(target.Center) >= 16*20 && AttackTimer >= 180)
            {
                if (Cycle == true)
                {
                    State = 1; //chum bombs
                    AttackTimer = 0;
                    Cycle = false;
                }
                else
                {
                    State = 3; //destroyer circle ripoff
                    AttackTimer = 0;
                    Cycle = true;
                }
            }
        }
        private void Movement(NPC npc, Vector2 target)
        {
            float speedmult = MathHelper.Lerp(1, 3, (float)AttackTimer / 120f); //speeds up if travelling to destination for too long bc rungod :(
            float accel = 0.8f * speedmult;
            float decel = 1.3f * speedmult;
            float resistance = npc.velocity.Length() * accel / (40 * speedmult);
            npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, target, npc.velocity, accel - resistance, decel + resistance);
        }
    }
    public class BloodEelSegment : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.BloodEelHead,
            NPCID.BloodEelBody,
            NPCID.BloodEelTail
        );

        public override bool SafePreAI(NPC npc)
        {
            DelegateMethods.v3_1 = new Vector3(0.8f, 0f, 0);
            Utils.PlotTileLine(npc.Center, npc.Center + npc.velocity, 10, DelegateMethods.CastLight);
            return base.SafePreAI(npc);
        }

        //pierce resist
        public override void SafeModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.numHits > 0 && !FargoSoulsUtil.IsSummonDamage(projectile) && !FargoSoulsSets.Projectiles.PierceResistImmune[projectile.type] && !EModeGlobalProjectile.PierceResistImmuneAiStyles.Contains(projectile.aiStyle))
                modifiers.FinalDamage *= 1f / MathF.Pow(1.75f, projectile.numHits);

            if ((projectile.maxPenetrate >= 20 || projectile.maxPenetrate <= -1) && EModeGlobalProjectile.PierceResistImmuneAiStyles.Contains(projectile.aiStyle))
            { //only affects projs of the type that are effectively infinite pierce
                modifiers.FinalDamage *= 0.7f;
            }
        }
        public override void SafeModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0.7f;
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.lifeRegen >= 0) return;
            npc.lifeRegen /= 2;
            damage /= 2;
        } 

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 600);
        }
    }
}
