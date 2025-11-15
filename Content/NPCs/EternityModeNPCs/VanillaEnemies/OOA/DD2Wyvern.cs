using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities.Terraria.Utilities;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2Wyvern : EModeNPCBehaviour
    {

        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.DD2WyvernT1,
            NPCID.DD2WyvernT2,
            NPCID.DD2WyvernT3
        );

        public int State = 0;
        public int Timer = 0;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[NPCID.DD2WyvernT1] = 6;
            NPCID.Sets.TrailingMode[NPCID.DD2WyvernT1] = 1;

            NPCID.Sets.TrailCacheLength[NPCID.DD2WyvernT2] = 6;
            NPCID.Sets.TrailingMode[NPCID.DD2WyvernT2] = 1;

            NPCID.Sets.TrailCacheLength[NPCID.DD2WyvernT3] = 6;
            NPCID.Sets.TrailingMode[NPCID.DD2WyvernT3] = 1;
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);
            npc.noTileCollide = false;
            npc.lifeMax = (int)(npc.lifeMax * 0.66667f);
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(Timer);
            binaryWriter.Write(State);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            Timer = binaryReader.ReadInt32();
            State = binaryReader.ReadInt32();
        }

        public override bool? CanFallThroughPlatforms(NPC npc) => true;

        public override bool SafePreAI(NPC npc)
        {
            Timer++;
            if (Timer < 60)
                return base.SafePreAI(npc);

            //FargoSoulsUtil.PrintAI(npc);

            if (DD2Event.Ongoing && npc.HasNPCTarget)
            {
                Vector2 crystalCenter = EModeDD2Event.GetEterniaCrystal().Center;
                float crystalDist = npc.Distance(crystalCenter);
                if (crystalDist <= 250)
                {
                    if (State == 0)
                        State = 1;
                    npc.ai[0] = 0;
                    npc.direction = (int)npc.HorizontalDirectionTo(crystalCenter);
                    npc.spriteDirection = npc.direction;
                    npc.velocity *= 0.8f;
                    if (npc.Center.Y - crystalCenter.Y < 50 && crystalDist < 100)
                        npc.velocity.Y = -1;

                    if (Timer >= 240 - (15 * 3) && Timer % 15 == 0)
                    {
                        float spread = 0.15f;
                        SoundEngine.PlaySound(SoundID.DD2_WyvernHurt, npc.Center);
                        SoundEngine.PlaySound(SoundID.Item20, npc.Center);
                        if (FargoSoulsUtil.HostCheck)
                        {
                            Projectile p = Projectile.NewProjectileDirect(npc.GetSource_FromThis(), npc.Center + npc.direction * 10 * Vector2.UnitX, (crystalCenter.X - 10 - npc.Center.X) / 50 * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(-spread, spread)), ProjectileID.BallofFire, FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.9f), 1f);
                            p.timeLeft = 80;
                            p.hostile = true;
                            p.friendly = false;
                            p.tileCollide = false;
                        }
                    }
                    if (Timer > 240)
                    {
                        Timer = 60;
                    }
                    return false;
                }
            }


            if (Timer >= 240 && npc.HasPlayerTarget && npc.HasValidTarget)
            {
                Player p = Main.player[npc.target];
                if (npc.Center.Distance(p.Center) < 100)
                    return base.SafePreAI(npc);
                Vector2 pos = npc.Center + 115 * Vector2.UnitX.RotatedBy(Main.rand.NextFloatDirection());
                for (int i = 0; i < 2; i++)
                {
                    Vector2 pos2 = pos.RotatedBy(MathHelper.Pi * i, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromThis(), npc.Center, 8 * Vector2.UnitY + Vector2.UnitX * npc.direction, ProjectileID.BallofFire, npc.damage / 6, 2);
                        proj.friendly = false;
                        proj.hostile = true;
                        proj.timeLeft = 30;
                    }
                    SoundEngine.PlaySound(SoundID.Item43, pos2);
                }
                Timer = 60;
            }

            return base.SafePreAI(npc);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);
            //target.AddBuff(BuffID.Rabies, 3600);
        }
    }
}
