using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Hell;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using log4net.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Hell
{
    public class Demons : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.Demon,
            NPCID.VoodooDemon,
            NPCID.RedDevil
        );

        public int Counter;
        public float Angle;
        public List<int> DevilDemons = [];
        public override void SetDefaults(NPC npc)
        {
            if (npc.type == NPCID.RedDevil)
            {
                npc.npcSlots = 3;
                if (npc.knockBackResist > 0.2f)
                    npc.knockBackResist = 0.2f;
                if (Main.hardMode)
                {
                    if (npc.lifeMax < 2200)
                        npc.lifeMax = 2200;
                }
                else
                {
                    if (npc.lifeMax > 600)
                        npc.lifeMax = 600;
                    npc.defense = 10;
                    npc.damage = 40;
                }
                npc.value = 100 * 100 * 5; // 5 gold
            }
            else
            {
                if (NPC.downedMechBossAny)
                {
                    if (npc.lifeMax < 550)
                        npc.lifeMax = 550;
                }
                else
                {
                    if (npc.lifeMax < 200)
                        npc.lifeMax = 200;
                }
            }
        }
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Angle);
            binaryWriter.Write(Counter);
            binaryWriter.Write(DevilDemons.Count);
            for (int i = 0; i < DevilDemons.Count; i++)
                binaryWriter.Write(DevilDemons[i]);
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            Angle = binaryReader.ReadSingle();
            Counter = binaryReader.ReadInt32();
            int demonCount = binaryReader.ReadInt32();
            for (int i = 0; i < demonCount; i++)
                DevilDemons[i] = binaryReader.ReadInt32();
        }
        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            if (npc.type == NPCID.RedDevil)
            {
                if (FargoSoulsUtil.HostCheck)
                {
                    int type = NPCID.Demon;
                    int count = Main.hardMode ? 3 : 2;
                    for (int i = 0; i < count; i++)
                    {
                        int j = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X + npc.width / 2, (int)npc.Center.Y + npc.height / 2, type);
                        if (j != Main.maxNPCs)
                        {
                            NPC newNPC = Main.npc[j];
                            if (newNPC != null && newNPC.active)
                            {
                                newNPC.velocity = Vector2.UnitX.RotatedByRandom(2 * Math.PI) * 5f;
                                newNPC.FargoSouls().CanHordeSplit = false;
                                DevilDemons.Add(j);
                                /*
                                if (newNPC.TryGetGlobalNPC(out EModeNPCBehaviour globalNPC))
                                {
                                    globalNPC.FirstTick = false;
                                }
                                */
                                if (Main.netMode == NetmodeID.Server)
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, j);
                            }
                        }
                    }
                    npc.netUpdate = true;
                }
            }

            if (npc.type == NPCID.Demon)
            {
                int hordeAmt = Main.rand.Next(5) + 1;
                if (!Main.hardMode)
                    hordeAmt = 2;

                if (Main.rand.NextBool(4) && npc.FargoSouls().CanHordeSplit)
                    EModeGlobalNPC.Horde(npc, hordeAmt);
            }
        }
        public override bool SafePreAI(NPC npc)
        {
            bool lineOfSight = npc.HasPlayerTarget && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0);

            if (npc.type == NPCID.Demon || npc.type == NPCID.VoodooDemon)
            {
                NPC devil = Main.npc.FirstOrDefault(n => n.TypeAlive(NPCID.RedDevil) && n.GetGlobalNPC<Demons>().DevilDemons.Contains(npc.whoAmI));

                if (npc.HasPlayerTarget && (Main.hardMode || devil != null))
                {
                    npc.noTileCollide = !lineOfSight;
                }
                if (devil != null)
                {
                    npc.target = devil.target;


                    if (Counter < 0) // dash attack
                    {
                        if (!npc.HasPlayerTarget)
                        {
                            Counter = 0;
                            npc.netUpdate = true;
                        }
                        Counter--;
                        npc.knockBackResist = 0f;

                        int timer = -Counter;

                        int windupTime = 120;
                        int chargeTime = 35 + 35;
                        int endTime = 150;
                        if (timer < windupTime) // windup
                        {
                            Vector2 angle = Main.player[npc.target].HorizontalDirectionTo(npc.Center) * Vector2.UnitX.RotatedBy(Angle);
                            npc.spriteDirection = npc.direction = -angle.X.NonZeroSign();
                            if (timer < windupTime - chargeTime)
                            {
                                Vector2 windupPos = Main.player[npc.target].Center + angle * 250;
                                float maxSpeed = 18f;
                                float accel = 0.3f;
                                float decel = 0.5f;
                                float resistance = npc.velocity.Length() * accel / maxSpeed;
                                npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, windupPos, npc.velocity, accel - resistance, decel + resistance);

                                if (timer == windupTime - chargeTime - 15)
                                {
                                    FargoSoulsUtil.DustRing(npc.Center, 32, DustID.Shadowflame, 10f);
                                }
                            }
                            else if (timer == windupTime - chargeTime)
                            {
                                npc.velocity *= 0;
                            }
                            else
                            {
                                npc.velocity += npc.DirectionTo(Main.player[npc.target].Center) * 0.4f;
                                npc.noGravity = true;
                            }
                        }
                        else if (timer < windupTime + endTime) // dash end
                        {
                            MoveToDevil();
                        }
                        else
                        {
                            Counter = 0;
                            npc.netUpdate = true;
                        }
                    }
                    else
                    {
                        npc.knockBackResist = npc.FargoSouls().defKnockBackResist;
                        MoveToDevil();

                    }
                    return false;
                    void MoveToDevil()
                    {
                        npc.noGravity = false;
                        Vector2 idlePosition = devil.Center;
                        if (devil.HasPlayerTarget)
                            idlePosition += devil.DirectionTo(Main.player[devil.target].Center) * 80;
                        Vector2 toIdlePosition = idlePosition - npc.Center;
                        float distance = toIdlePosition.Length();
                        float speed = 16f;
                        float inertia = 40f;
                        toIdlePosition.Normalize();
                        toIdlePosition *= speed;
                        npc.velocity = (npc.velocity * (inertia - 1f) + toIdlePosition) / inertia;
                        if (npc.velocity == Vector2.Zero)
                        {

                            npc.velocity.X = -0.15f;
                            npc.velocity.Y = -0.05f;
                        }
                        npc.spriteDirection = npc.direction = npc.velocity.X.NonZeroSign();
                    }
                }
                else
                {
                    if (npc.ai[0] == 295f && Counter == 0 && npc.HasPlayerTarget && Main.player[npc.target].Distance(npc.Center) < 800 && lineOfSight)
                    {
                        Counter = -1;
                        npc.netUpdate = true;
                    }
                    if (Counter < 0) // dash attack
                    {
                        if (!npc.HasPlayerTarget)
                        {
                            Counter = 0;
                            npc.netUpdate = true;
                        }
                        Counter--;
                        npc.knockBackResist = 0f;

                        int timer = -Counter;

                        int windupTime = 120;
                        int chargeTime = 35 + 35;
                        int endTime = 10;
                        if (timer < windupTime) // windup
                        {
                            Vector2 angle = Main.player[npc.target].HorizontalDirectionTo(npc.Center) * Vector2.UnitX.RotatedBy(Angle);
                            npc.spriteDirection = npc.direction = -angle.X.NonZeroSign();
                            if (timer < windupTime - chargeTime)
                            {
                                Vector2 windupPos = Main.player[npc.target].Center + angle * 250;
                                float maxSpeed = 18f;
                                float accel = 0.3f;
                                float decel = 0.5f;
                                float resistance = npc.velocity.Length() * accel / maxSpeed;
                                npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, windupPos, npc.velocity, accel - resistance, decel + resistance);

                                if (timer == windupTime - chargeTime - 15)
                                {
                                    FargoSoulsUtil.DustRing(npc.Center, 32, DustID.Shadowflame, 10f);
                                }
                            }
                            else if (timer == windupTime - chargeTime)
                            {
                                npc.velocity *= 0;
                            }
                            else
                            {
                                npc.velocity += npc.DirectionTo(Main.player[npc.target].Center) * 0.4f;
                                npc.noGravity = true;
                            }
                        }
                        else if (timer < windupTime + endTime) // dash end
                        {
                            npc.velocity *= 0.85f;
                        }
                        else
                        {
                            Counter = 0;
                            npc.ai[0] = 296f;
                            npc.netUpdate = true;
                        }
                        return false;
                    }
                    else
                    {
                        npc.knockBackResist = npc.FargoSouls().defKnockBackResist;
                    }
                }
            }
            else if (npc.type == NPCID.RedDevil)
            {
                if (npc.HasPlayerTarget)
                {
                    npc.noTileCollide = !lineOfSight;
                }
                bool anyDemons = DevilDemons.Any(i => Main.npc[i].TypeAlive(NPCID.Demon));
                if (anyDemons) // phase 1: demons
                {
                    npc.defense = npc.defDefense + 50;

                    Counter++;
                    int attackDelay = 60 * 3;
                    if (Counter >= attackDelay && lineOfSight && npc.Distance(Main.player[npc.target].Center) < 800) // force a demon to do a dash attack
                    {
                        Counter = 0;
                        var readyDemons = DevilDemons.Where(n => Main.npc[n].TypeAlive(NPCID.Demon) && Main.npc[n].GetGlobalNPC<Demons>().Counter == 0);
                        if (readyDemons.Any() && FargoSoulsUtil.HostCheck)
                        {
                            int demonID = Main.rand.NextFromCollection(readyDemons.ToList());
                            Demons demon = Main.npc[demonID].GetGlobalNPC<Demons>();
                            demon.Counter = -1;
                            demon.Angle = Main.rand.Next(-1, 2) * MathHelper.PiOver2 * 0.6f;
                            Main.npc[demonID].netUpdate = true;
                            npc.netUpdate = true;
                        }
                    }
                }
                else // phase 2: no demons
                {
                    npc.defense = npc.defDefense;

                    int attackDelay = 60 * 4;
                    int artilleryWindup = 70;
                    int artilleryTelegraph = 40;
                    npc.knockBackResist = npc.FargoSouls().defKnockBackResist;
                    Counter++;
                    if (Counter > attackDelay && Counter < attackDelay + artilleryWindup)
                    {
                        npc.knockBackResist = 0f;
                        Vector2 windupPos = Main.player[npc.target].Center + Vector2.UnitX * Main.player[npc.target].HorizontalDirectionTo(npc.Center) * 450;
                        npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, windupPos, npc.velocity, 0.8f, 0.8f);
                        npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(Main.player[npc.target].Center).NonZeroSign();
                        if (Counter > attackDelay + artilleryWindup - artilleryTelegraph)
                        {
                            // dust
                            for (int i = 0; i < 3; i++)
                            {
                                Vector2 vel = new Vector2(npc.direction, -1.7f) * 9;
                                vel.X += Main.rand.NextFloat(-1, 1) * 4f;
                                vel.Y -= Main.rand.NextFloat(0, 1) * 6f;
                                vel *= 2;
                                int d = Dust.NewDust(npc.Center + vel, 10, 10, DustID.Torch, vel.X, vel.Y, Scale: 2f);
                                Main.dust[d].noGravity = true;
                            }
                        }
                        return false;
                    }
                    else if (Counter >= attackDelay + artilleryWindup)
                    {
                        Counter = 0;
                        // fireballs
                        SoundEngine.PlaySound(SoundID.Item20 with { Pitch = -0.25f }, npc.Center);
                        npc.velocity.X = npc.direction * 3;
                        npc.velocity.Y = 1.5f;
                        if (FargoSoulsUtil.HostCheck)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                Vector2 vel = new Vector2(npc.direction, -1.7f) * 9;
                                vel.X += Main.rand.NextFloat(-1, 1) * 1.4f;
                                vel.Y -= Main.rand.NextFloat(0, 1) * 3.7f;
                                vel.X += (i - 1) * 5;
                                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + vel * 2, vel, ModContent.ProjectileType<DemonFireball>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 1f);
                            }
                        }
                    }
                }
            }
            /*
            if ((npc.type == NPCID.Demon && npc.ai[0] == 100f)
            || (npc.type == NPCID.RedDevil && ++Counter > 300))
            {
                Counter = 0;

                int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                if (t != -1 && npc.Distance(Main.player[t].Center) < 800 && FargoSoulsUtil.HostCheck)
                {
                    int amount = npc.type == NPCID.RedDevil ? 9 : 6;
                    int damage = FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, npc.type == NPCID.RedDevil ? 4f / 3 : 1);
                    FargoSoulsUtil.XWay(amount, npc.GetSource_FromThis(), npc.Center, ProjectileID.DemonSickle, 1, damage, .5f);
                }
            }
            */

            if (npc.type == NPCID.VoodooDemon) //can ignite itself to burn up its doll
            {
                const int dollBurningTime = 600;

                if (npc.lavaWet && npc.HasValidTarget
                    && (npc.Distance(Main.player[npc.target].Center) < 450 || Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0)))
                {
                    npc.buffImmune[BuffID.OnFire] = false;
                    npc.buffImmune[BuffID.OnFire3] = false;
                    npc.AddBuff(BuffID.OnFire, dollBurningTime + 60);
                }

                if (npc.onFire || npc.onFire3)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath10 with { Pitch = 0.5f }, npc.Center);

                    for (int i = 0; i < 3; i++) //NOTICE ME
                    {
                        int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, 0f, 0f, 0, default, (float)Counter / dollBurningTime * 3f);
                        Main.dust[d].noGravity = !Main.rand.NextBool(5);
                        Main.dust[d].noLight = true;
                        Main.dust[d].velocity *= Main.rand.NextFloat(12f);
                    }

                    if (++Counter > dollBurningTime) //doll fully burned up
                    {
                        npc.Transform(NPCID.Demon);

                        int guide = NPC.FindFirstNPC(NPCID.Guide);
                        if (guide != -1 && Main.npc[guide].active && FargoSoulsUtil.HostCheck)
                        {
                            Main.npc[guide].SimpleStrikeNPC(int.MaxValue, 0, false, 0, null, false, 0, true);

                            int p = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                            if (p != -1)
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    int npcType = Main.rand.NextBool() ? NPCID.LeechHead : NPCID.TheHungryII;

                                    FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, npcType,
                                        velocity: new Vector2(Main.rand.NextFloat(-5, 5) * 2, Main.rand.NextFloat(-5, 5) * 2));
                                }

                                if (WorldSavingSystem.MasochistModeReal && !FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.wallBoss, NPCID.WallofFlesh))
                                {
                                    NPC.SpawnWOF(Main.player[npc.target].Center);
                                }
                            }
                        }
                    }
                }
            }
            return base.SafePreAI(npc);
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.lifeRegen >= 0)
                return;
            base.UpdateLifeRegen(npc, ref damage);

            if (npc.type == NPCID.VoodooDemon && npc.onFire)
            {
                damage /= 2;
                npc.lifeRegen /= 2;
            }
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            base.ModifyNPCLoot(npc, npcLoot);
            FargoSoulsUtil.EModeDrop(npcLoot, ItemDropRule.Common(ItemID.Blindfold, 50));
        }
    }
}
