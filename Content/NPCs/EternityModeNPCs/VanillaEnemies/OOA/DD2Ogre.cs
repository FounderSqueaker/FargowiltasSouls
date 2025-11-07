using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.CustomEnemies.OOA;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2Ogre : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.DD2OgreT2,
            NPCID.DD2OgreT3
        );

        public override void SetStaticDefaults()
        {
            
            base.SetStaticDefaults();
        }

        public override void SetDefaults(NPC entity)
        {
            base.SetDefaults(entity);
            entity.damage /= 2;
            float mult = entity.type == NPCID.DD2OgreT2 ? 2 : 1.5f;
            entity.lifeMax = (int)(entity.lifeMax * mult);
            //entity.scale *= 2;
        }

        public int AnimState = -1;
        public int AnimStartTime = -1;
        public int LastFrame = -1;
        public bool NormalAI = false;

        public int Timer = 0;
        public int State = -1;
        public int PreviousState = -1;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(AnimState);
            binaryWriter.Write(AnimStartTime);
            binaryWriter.Write(LastFrame);
            binaryWriter.Write(Timer);
            binaryWriter.Write(State);
            binaryWriter.Write(PreviousState);
            binaryWriter.Write(NormalAI);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            AnimState = binaryReader.ReadInt32();
            AnimStartTime = binaryReader.ReadInt32();
            LastFrame = binaryReader.ReadInt32();
            Timer = binaryReader.ReadInt32();
            State = binaryReader.ReadInt32();
            PreviousState = binaryReader.ReadInt32();
            NormalAI = binaryReader.ReadBoolean();
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            base.OnSpawn(npc, source);
            //FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, NPCID.DD2DarkMageT1, target: npc.target);
        }

        public override bool? CanFallThroughPlatforms(NPC npc)
        {
            return true;
        }

        public enum AttackStates
        {
            Spawning = -1,
            Idle,
            SnotBaseball,
            Bowling,
            ChargeSlam,
            JumpSlash,
            JumpSlash2
        }

        public override bool SafePreAI(NPC npc)
        {
            NormalAI = false;
            if (npc.Distance(Main.LocalPlayer.Center) > 3000 && !DD2Event.Ongoing)
            {
                npc.active = false;
                return false;
            }

            if (!Main.player.Any(p => p.active && !p.dead && !p.ghost))
                return base.SafePreAI(npc);

            if (!HasAttackTarget(npc))
                npc.TargetClosest();

            Timer++;
            switch ((AttackStates)State)
            {
                case AttackStates.Spawning:
                {
                    AnimState = (int)AnimationStates.Idle;
                    if (Timer < 60 || npc.velocity.Y != 0)
                        return base.SafePreAI(npc);
                    State = (int)AttackStates.Bowling;
                    Timer = -1;
                    break;
                }
                case AttackStates.Idle:
                    AnimState = (int)AnimationStates.None;
                    if (HasAttackTarget(npc))
                    {
                        ChooseAttack(npc);
                        return false;
                    }
                    return base.SafePreAI(npc);
                case AttackStates.SnotBaseball:
                    SnotBaseball(npc);
                    break;
                case AttackStates.Bowling:
                    Bowling(npc);
                    break;
                case AttackStates.ChargeSlam:
                    ChargeSlam(npc);
                    break;
                case AttackStates.JumpSlash:
                case AttackStates.JumpSlash2:
                    JumpSlash(npc);
                    break;

            }

            return false;
        }

        public override void AI(NPC npc)
        {
            NormalAI = true;
            base.AI(npc);
        }

        #region AI States
        private void SnotBaseball(NPC npc)
        {
            npc.velocity *= 0;
            int delay = 120;
            int shotCount = DD2Event.Ongoing ? 1 : 3;
            if (Timer < delay - 70)
                Timer = delay - 70;

            if (Timer % delay == delay - 32)
            {
                BeginAnimation(npc, (int)AnimationStates.Spit);
                SoundEngine.PlaySound(SoundID.DD2_OgreSpit, npc.Center);
            }
            else if (Timer % delay == delay - 1)
            {
                SoundEngine.PlaySound(FargosSoundRegistry.ThrowShort with { Volume = 0.5f }, npc.Center);
                Vector2 pos = npc.Center + ((npc.direction * npc.width / 2) - 15) * Vector2.UnitX - 4 * Vector2.UnitY;
                if (FargoSoulsUtil.HostCheck)
                {
                    FargoSoulsUtil.DustRing(pos, 20, DustID.GemEmerald, 2f, scale: 2);
                    Projectile.NewProjectile(npc.GetSource_FromThis(), pos, new Vector2(npc.direction * 0.3f, -8), ModContent.ProjectileType<SnotBaseball>(), (int)(npc.damage / 3), 2f, ai2: npc.target);
                }
            }
            else if (Timer % delay == 42 && Timer > delay)
            {
                SoundEngine.PlaySound(SoundID.DD2_OgreAttack, npc.Center);
                BeginAnimation(npc, (int)AnimationStates.Swing);
            }
            else if (Timer % delay == 56)
                npc.direction = (int)npc.HorizontalDirectionTo(Main.player[npc.target].Center);

            if (Timer >= ((shotCount * delay) + 78))
                ResetToIdle(npc);
        }

        private void Bowling(NPC npc)
        {
            NPC crystal = EModeDD2Event.GetEterniaCrystal();
            if (crystal == null || npc.Distance(crystal.Center) < 500)
            {
                ChooseAttack(npc);
                return;
            }

            if (Timer == 1)
            {
                npc.velocity *= 0;
                npc.direction = (int)npc.HorizontalDirectionTo(crystal.Center);
                BeginAnimation(npc, (int)AnimationStates.Spit);
                SoundEngine.PlaySound(SoundID.DD2_OgreSpit, npc.Center);
            }
            if (Timer == 49)
            {
                BeginAnimation(npc, (int)AnimationStates.Throw);
                SoundEngine.PlaySound(SoundID.DD2_OgreAttack, npc.Center);
            }
            if (Timer == 56)
            {
                SoundEngine.PlaySound(FargosSoundRegistry.ThrowShort, npc.Center);
                Vector2 pos = npc.Center + ((npc.direction * npc.width / 2) - 15) * Vector2.UnitX - 4 * Vector2.UnitY;
                FargoSoulsUtil.DustRing(pos, 30, DustID.JungleTorch, 4);
                if (FargoSoulsUtil.HostCheck)
                    FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromThis(), pos, ModContent.NPCType<BowlingSpit>(), velocity: new (npc.direction * 3.5f, -8));
            }

            if (Timer > 75)
            {
                ResetAnimation(npc);
                ResetToIdle(npc);
            }
        }

        private void ChargeSlam(NPC npc)
        {
            int timeToCharge = 90;
            if (AnimState != (int)AnimationStates.Rest)
            {
                if (Timer == 0)
                    BeginAnimation(npc, (int)AnimationStates.Charge);
                if (Timer < timeToCharge) // charging
                {
                    npc.direction = (int)npc.HorizontalDirectionTo(Main.player[npc.target].Center);
                    npc.velocity *= 0;
                    if (Timer % (timeToCharge / 3) == 0)
                    {
                        float pitch = 0.1f * (int)(Timer / 30);
                        SoundEngine.PlaySound(SoundID.Item9 with { Pitch = pitch }, npc.Center);
                        for (int i = 0; i < 20; i++)
                        {
                            float rot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                            new SparkParticle((npc.Center + 10 * Vector2.UnitY) + 50 * Vector2.UnitX.RotatedBy(rot), -2 * Vector2.UnitX.RotatedBy(rot), Color.White, 0.6f, 25).Spawn();
                        }
                    }
                }
                else if (Timer == timeToCharge) // jump
                {
                    npc.velocity = new Vector2(npc.direction * 4, -20);
                    SoundEngine.PlaySound(SoundID.DD2_OgreRoar, npc.Center);
                    SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Bottom, Vector2.Zero, ProjectileID.DD2OgreSmash, FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f);
                    BeginAnimation(npc, (int)AnimationStates.Jump);
                }
                else if (npc.velocity.Y != 0)
                {
                    npc.velocity.Y -= -0.1f;
                    if (Timer % 10 == 0 && !Main.dedServ)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int snotType = Main.rand.NextFromList(GoreID.OgreSpit1, GoreID.OgreSpit2, GoreID.OgreSpit3);
                            Gore.NewGore(npc.GetSource_FromThis(), npc.Bottom, 1 * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), snotType);
                        }
                    }
                }

                if (npc.velocity.Y >= 10)
                {
                    npc.velocity.X *= 0.9f;
                    for (int i = 0; i < 3; i++)
                    {
                        new SparkParticle(npc.TopLeft + Main.rand.NextFloat(0, npc.width) * Vector2.UnitX + npc.velocity, -1 * Main.rand.NextFloat(1, 3) * Vector2.UnitY, Color.White, 0.3f, 9).Spawn();
                    }
                }

                if (Timer > 120 && Collision.SolidCollision(npc.position + npc.velocity, npc.width, npc.height))
                {
                    Timer = 0;
                    SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Bottom, Vector2.Zero, ProjectileID.DD2OgreSmash, FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f);
                    BeginAnimation(npc, (int)AnimationStates.Rest);

                    SoundEngine.PlaySound(SoundID.NPCDeath19, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        float speed = 9f;
                        for (int i = 0; i < 12; i++)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Bottom, speed * Vector2.UnitX.RotatedBy(3 * MathHelper.PiOver2 + Main.rand.NextFloat(MathHelper.PiOver4 / 2, 1.5f * MathHelper.PiOver4)), ModContent.ProjectileType<SnotBaseballSplit>(), (int)(npc.damage / 4f), 2f);
                        for (int i = 0; i < 12; i++)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Bottom, speed * Vector2.UnitX.RotatedBy(3 * MathHelper.PiOver2 - Main.rand.NextFloat(MathHelper.PiOver4 / 2, 1.5f * MathHelper.PiOver4)), ModContent.ProjectileType<SnotBaseballSplit>(), (int)(npc.damage / 4f), 2f);
                    }
                    if (!Main.dedServ)
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            int snotType = Main.rand.NextFromList(GoreID.OgreSpit1, GoreID.OgreSpit2, GoreID.OgreSpit3);
                            Gore.NewGore(npc.GetSource_FromThis(), npc.Bottom, 5 * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), snotType);
                        }
                    }
                }
            }
            else
            {
               if (Timer >= 15)
               {
                    ResetAnimation(npc);
                    ResetToIdle(npc);
               }
            }
        }

        private void JumpSlash(NPC npc)
        {
            Player p = Main.player[npc.target];
            if (State == (int)AttackStates.JumpSlash)
            {
                npc.direction = (int)npc.HorizontalDirectionTo(p.Center);
                if (Timer < 10)
                    npc.velocity *= 0.8f;
                if (Timer == 10)
                {
                    SoundEngine.PlaySound(SoundID.DD2_OgreRoar, npc.Center);
                    BeginAnimation(npc, (int)AnimationStates.FastCharge);
                }
                if (Timer == 28)
                {
                    BeginAnimation(npc, (int)AnimationStates.Jump);
                    npc.velocity.Y = -18;
                    npc.velocity.X = 2f * npc.direction;
                }
                if (Timer > 28)
                {
                    npc.velocity.X += 0.02f * npc.direction;
                }
                if (Timer > 50)
                {
                    if (Math.Abs(p.Center.Y - npc.Center.Y) < npc.height / 4 && Math.Abs(p.Center.X - npc.Center.X) < 350)
                    {
                        AnimState = (int)AnimationStates.Hold;
                        AnimStartTime = 13;
                        State = (int)AttackStates.JumpSlash2;
                        Timer = 0;
                        npc.velocity.Y = 0.001f;
                        npc.noGravity = true;
                        BeginAnimation(npc, (int)AnimationStates.Swing);
                        NetSync(npc);
                        npc.netUpdate = true;

                        FargoSoulsUtil.DustRing(npc.Center, 30, DustID.Smoke, 5f, scale: 3f);
                        SoundEngine.PlaySound(SoundID.DD2_OgreAttack, npc.Center);
                    }
                    if (npc.velocity.Y >= 10)
                    {
                        npc.velocity.X *= 0.9f;
                        for (int i = 0; i < 3; i++)
                        {
                            new SparkParticle(npc.TopLeft + Main.rand.NextFloat(0, npc.width) * Vector2.UnitX + npc.velocity, -1 * Main.rand.NextFloat(1, 3) * Vector2.UnitY, Color.White, 0.3f, 9).Spawn();
                        }
                    }
                    if (Collision.SolidCollision(npc.position + npc.velocity, npc.width, npc.height))
                    {
                        SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound, npc.Center);
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Bottom, Vector2.Zero, ProjectileID.DD2OgreSmash, FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f);
                        BeginAnimation(npc, (int)AnimationStates.Rest);
                        ResetToIdle(npc);
                    }
                }
            }
            else
            {
                if (Timer == 1)
                {
                    npc.direction = (int)npc.HorizontalDirectionTo(p.Center);
                    SoundEngine.PlaySound(SoundID.Item69, npc.Center);
                }
                if (Timer < 15)
                {
                    npc.velocity.X = 20 * npc.direction;
                    for (int i = 0; i < 2; i++)
                        new SparkParticle(npc.Top - npc.direction * npc.width / 2 * Vector2.UnitX + Main.rand.NextFloat(0, npc.height) * Vector2.UnitY, -npc.direction * Main.rand.NextFloat(1,3) * Vector2.UnitX, Color.White, 0.7f, 13).Spawn();
                }
                if (Timer > 15)
                {
                    BeginAnimation(npc, (int)AnimationStates.Jump);
                    npc.noGravity = false;
                    npc.velocity.X *= 0.8f;
                }
                if (npc.velocity.Y >= 10)
                {
                    npc.velocity.X *= 0.9f;
                    for (int i = 0; i < 3; i++)
                    {
                        new SparkParticle(npc.TopLeft + Main.rand.NextFloat(0, npc.width) * Vector2.UnitX + npc.velocity, -1 * Main.rand.NextFloat(1, 3) * Vector2.UnitY, Color.White, 0.3f, 9).Spawn();
                    }
                }
                if (Collision.SolidCollision(npc.position + npc.velocity, npc.width, npc.height))
                {
                    SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Bottom, Vector2.Zero, ProjectileID.DD2OgreSmash, FargoSoulsUtil.ScaledProjectileDamage(npc.damage), 0f);
                    BeginAnimation(npc, (int)AnimationStates.Rest);
                    ResetToIdle(npc);
                }
            }
            npc.spriteDirection = npc.direction;
        }
        #endregion

        #region Help Methods
        public void ResetState(NPC npc)
        {
            Timer = -1;
            NetSync(npc);
            npc.netUpdate = true;
        }

        public void ResetToIdle(NPC npc)
        {
            if (State != (int)AttackStates.Idle)
                PreviousState = State;
            if (PreviousState == (int)AttackStates.JumpSlash2)
                PreviousState--;
            State = (int)AttackStates.Idle;
            ResetState(npc);
            //ResetAnimation(npc);
        }

        public void ChooseAttack(NPC npc)
        {
            ResetState(npc);
            ResetAnimation(npc);
            if (FargoSoulsUtil.HostCheck)
            {
                List<AttackStates> states = [];
                for (int i = 1; i <= 4; i++)
                {
                    if (PreviousState != i)
                    {
                        states.Add((AttackStates)i);
                    }
                }
                State = (int)Main.rand.NextFromCollection(states);
            }
            NetSync(npc);
        }

        // Can't use HasValidTarget since crystal is targeted
        private bool HasAttackTarget(NPC npc)
        {
            if (!npc.HasPlayerTarget)
                return false;
            Player p = Main.player[npc.target];
            return (p.active && !p.dead && !p.ghost);
        }
        #endregion
        #region Animation
        private enum AnimationStates
        {
            None = -1,
            Idle,
            Spit,
            Swing,
            FastCharge,
            Charge,
            Jump,
            Rest,
            Throw,
            Hold
        }

        private void Animate(NPC npc)
        {
            if (AnimState == (int)AnimationStates.None)
            {
                npc.frame.Y = LastFrame;
                npc.frame.X = 2;
            }

            switch ((AnimationStates)AnimState)
            {
                case AnimationStates.Spit:
                    Animate_Spit(npc);
                    break;
                case AnimationStates.Swing:
                    Animate_Swing(npc);
                    break;
                case AnimationStates.FastCharge:
                    Animate_FastCharge(npc);
                    break;
                case AnimationStates.Charge:
                    Animate_Charge(npc);
                    break;
                case AnimationStates.Jump:
                    Animate_Jump(npc);
                    break;
                case AnimationStates.Rest:
                    npc.frame.X = 2;
                    npc.frame.Y = 47;
                    break;
                case AnimationStates.Throw:
                    Animate_Throw(npc);
                    break;
                case AnimationStates.Hold:
                    npc.frame.X = 2;
                    npc.frame.Y = AnimStartTime;
                    break;
                default:
                    break;
            }
            LastFrame = npc.frame.Y;
        }

        public void ResetAnimation(NPC npc)
        {
            AnimState = (int)AnimationStates.None;
            AnimStartTime = -1;
            NetSync(npc);
            npc.netUpdate = true;
        }

        public void BeginAnimation(NPC npc, int state)
        {
            ResetAnimation(npc);
            AnimStartTime = Timer;
            AnimState = state;
            NetSync(npc);
            npc.netUpdate = true;
        }

        public void Animate_Spit(NPC npc)
        {
            int numFrames = 8;
            int timePerFrame = 6;
            npc.frame.X = 2;
            npc.frame.Y = Find_Frame(22, timePerFrame);
            if (Timer - AnimStartTime > numFrames * timePerFrame)
                ResetAnimation(npc);
        }

        public void Animate_Swing(NPC npc)
        {
            int numFrames = 8;
            int timePerFrame = 4;
            npc.frame.X = 2;
            npc.frame.Y = Find_Frame(12, timePerFrame);
            if (Timer - AnimStartTime > numFrames * timePerFrame)
                ResetAnimation(npc);
        }

        public void Animate_FastCharge(NPC npc)
        {
            int numFrames = 3;
            int timePerFrame = 6;
            npc.frame.X = 2;
            npc.frame.Y = Find_Frame(39, timePerFrame);
            if (Timer - AnimStartTime > numFrames * timePerFrame)
                ResetAnimation(npc);
        }

        public void Animate_Charge(NPC npc)
        {
            int numFrames = 3;
            int timePerFrame = 30;
            npc.frame.X = 2;
            npc.frame.Y = Find_Frame(39, timePerFrame);
            if (Timer - AnimStartTime > numFrames * timePerFrame)
                ResetAnimation(npc);
        }

        public void Animate_Jump(NPC npc)
        {
            npc.frame.X = 2;
            float yVel = - npc.velocity.Y;
            
            if (yVel > 7)
                npc.frame.Y = 44;
            else if (yVel > -2)
                npc.frame.Y = 45;
            else if (yVel > -10)
                npc.frame.Y = 46;
            else
                npc.frame.Y = 47;

            if (yVel == 0)
                ResetAnimation(npc);
        }

        public void Animate_Throw(NPC npc)
        {
            int numFrames = 8;
            int timePerFrame = 4;
            npc.frame.Y = Find_Frame(31, timePerFrame);
            if (Timer - AnimStartTime > numFrames * timePerFrame)
                ResetAnimation(npc);
        }

        private int Find_Frame(int startFrame, int timePerFrame) 
            => startFrame + (int)Math.Floor((decimal)(Timer - AnimStartTime) / timePerFrame);
        #endregion

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!NormalAI && !npc.IsABestiaryIconDummy)
                Animate(npc);
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.FargoSouls().AddBuffNoStack(ModContent.BuffType<StunnedBuff>(), 60);
            //target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 300);
            //target.AddBuff(BuffID.BrokenArmor, 300);
        }

        public override void OnKill(NPC npc)
        {
            // instantly end T2 OOA
            if (DD2Event.Ongoing && DD2Event.OngoingDifficulty == 2 && NPC.waveNumber == 7)
            {
                NPC.waveKills = 220;
                DD2Event.CheckProgress(npc.type);
            }

            base.OnKill(npc);
        }
    }
}
