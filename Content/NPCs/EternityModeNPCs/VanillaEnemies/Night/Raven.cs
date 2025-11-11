using System;
using System.IO;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Desert;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Night
{
    public class Raven : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Raven);
        public int SwoopTimer;
        readonly int swoopStart = 60 * 4;
        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);
            //if (Main.rand.NextBool(5) && npc.FargoSouls().CanHordeSplit)
                //EModeGlobalNPC.Horde(npc, 3);
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(SwoopTimer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            SwoopTimer = binaryReader.ReadInt32();
        }
        public override bool SafePreAI(NPC npc)
        {
            if (SwoopTimer == 0)
            {
                SwoopTimer = Main.rand.Next(1, 60 * 3);
                npc.netUpdate = true;
            }
            bool lineOfSight = npc.HasPlayerTarget && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0) && npc.Distance(Main.player[npc.target].Center) < 450;
            if (!lineOfSight && SwoopTimer > swoopStart - 30 && (SwoopTimer < swoopStart || !npc.HasPlayerTarget))
            {
                SwoopTimer = swoopStart - 30 - Main.rand.Next(30, 120);
            }

            if (SwoopTimer >= swoopStart)
            {
                npc.knockBackResist = 0f;
                Player player = Main.player[npc.target];
                float startupEnd = swoopStart + 40;
                float telegraphEnd = startupEnd + 15;
                float swoopEnd = telegraphEnd + 80;
                float endlagEnd = swoopEnd + 40;

                float xOffset = 300;
                float yOffset = -200;

                if (SwoopTimer <= startupEnd) // startup movement
                {
                    Vector2 desiredPos = player.Center + player.HorizontalDirectionTo(npc.Center) * Vector2.UnitX * xOffset + Vector2.UnitY * yOffset;
                    float maxSpeed = 18f;
                    float accel = 0.6f;
                    float decel = 0.8f;
                    float resistance = npc.velocity.Length() * accel / maxSpeed;
                    npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, desiredPos, npc.velocity, accel - resistance, decel + resistance);
                    if (SwoopTimer < startupEnd)
                    {
                        SwoopTimer++;
                    }
                    else // minimum time succeeded, look for condition
                    {
                        if (MathF.Abs(npc.Center.Y - desiredPos.Y) < 50)
                        {
                            SwoopTimer = (int)startupEnd + 1;
                            npc.netUpdate = true;
                        }
                    }
                    npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(player.Center).NonZeroSign();
                }
                if (SwoopTimer > startupEnd && SwoopTimer <= telegraphEnd) // telegraph, slowdown
                {
                    if (SwoopTimer == startupEnd + 1)
                    {
                        FargoSoulsUtil.DustRing(npc.Center, 32, DustID.InfernoFork, 10f);
                        SoundEngine.PlaySound(SoundID.Macaw with {Pitch = -0.25f, Volume = 1.5f, MaxInstances = 2}, npc.Center);
                    }
                    npc.velocity *= 0.85f;
                    npc.direction = npc.spriteDirection = npc.HorizontalDirectionTo(player.Center).NonZeroSign();
                    SwoopTimer++;
                }
                if (SwoopTimer > telegraphEnd && SwoopTimer <= swoopEnd) // swoop
                {
                    if (SwoopTimer == telegraphEnd + 1)
                    {
                        SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.3f, PitchVariance = 0.2f }, npc.Center);
                    }
                    if (SwoopTimer % 20 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item32 with { Volume = 5 }, npc.Center);
                    }
                    Vector2 dir = Vector2.UnitX * npc.direction;
                    dir.Normalize();
                    float angleDown = MathHelper.PiOver4 * 1.2f;
                    float progress = LumUtils.InverseLerp(telegraphEnd, swoopEnd, SwoopTimer);
                    // if facing left, rotate clockwise. otherwise, anti-clockwise
                    angleDown *= npc.direction;
                    dir = dir.RotatedBy(angleDown - 2 * angleDown * progress);
                    dir *= 11f;
                    npc.velocity = Vector2.Lerp(npc.velocity, dir, 0.08f);

                    SwoopTimer++;
                }
                if (SwoopTimer > swoopEnd && SwoopTimer <= endlagEnd)
                {
                    npc.velocity *= 0.96f;
                    Vector2 desiredPos = player.Center + player.HorizontalDirectionTo(npc.Center) * Vector2.UnitX * xOffset + Vector2.UnitY * yOffset;
                    float maxSpeed = 8f;
                    float accel = 0.25f;
                    float decel = 0.5f;
                    float resistance = npc.velocity.Length() * accel / maxSpeed;
                    npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, desiredPos, npc.velocity, accel - resistance, decel + resistance);
                    SwoopTimer++;
                }
                if (SwoopTimer > endlagEnd)
                {
                    SwoopTimer = -60 * 4;
                    npc.netUpdate = true;
                }
                return false;
            }
            else
            {
                SwoopTimer++;
                npc.knockBackResist = npc.FargoSouls().defKnockBackResist;
            }

            return base.SafePreAI(npc);
        }
        public override bool? CanFallThroughPlatforms(NPC npc)
        {
            if (SwoopTimer >= swoopStart)
                return true;
            return base.CanFallThroughPlatforms(npc);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            FargoSoulsUtil.AddDebuffFixedDuration(target, BuffID.Darkness, 60*10);
        }
    }
}
