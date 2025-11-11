using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Desert
{
    public class Antlion : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Antlion);

        public int AttackTimer;
        public int VacuumTimer;
        public int BiteTimer;
        public int BittenPlayer = -1;
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(BiteTimer);
            binaryWriter.Write(BittenPlayer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            BiteTimer = binaryReader.ReadInt32();
            BittenPlayer = binaryReader.ReadInt32();
        }
        public override void AI(NPC npc)
        {
            base.AI(npc);

            //suck in nearby players
            foreach (Player p in Main.player.Where(x => x.active && !x.dead))
            {
                if (BittenPlayer == -1 && BiteTimer >= 0 && p.velocity.Y == 0 && npc.Distance(p.Center) < 120 && npc.Center.Y >= p.Center.Y-32 && !p.immune)
                {
                    VacuumTimer++;
                    if (VacuumTimer > 120)
                        VacuumTimer = 120;
                    float dragSpeed = MathHelper.Lerp(0.35f, 0.75f, VacuumTimer/120);
                    p.velocity.X += dragSpeed * -npc.direction;

                    int dust = Dust.NewDust(p.position, p.width, p.height, DustID.Sand, 15f*-npc.direction, 1, 0, default, 1);
                    Main.dust[dust].noGravity = true;
                }
                else VacuumTimer = 0;
            }

            //bite
            if (BittenPlayer != -1)
            {
                Player victim = Main.player[BittenPlayer];
                if (BiteTimer > 0 && victim.active && !victim.ghost && !victim.dead
                    && (npc.Distance(victim.Center) < 160 || victim.whoAmI != Main.myPlayer)
                    && victim.FargoSouls().MashCounter < 20)
                {
                    victim.AddBuff(ModContent.BuffType<GrabbedBuff>(), 2);
                    victim.velocity = Vector2.Zero;
                    victim.Center = npc.Top;
                }
                else
                {
                    BittenPlayer = -1;
                    BiteTimer = -90; //cooldown

                    //spits you upwards
                    victim.velocity.Y -= 15;
                    victim.velocity.X += Main.rand.NextFloat(-5f, 5f);
                    SoundEngine.PlaySound(SoundID.NPCDeath13, npc.Center);
                    victim.immune = true;
                    victim.immuneTime = Math.Max(victim.immuneTime, 60);
                    npc.netUpdate = true;
                    NetSync(npc);
                }
            }
            if (BiteTimer < 0)
                BiteTimer++;
            if (BiteTimer > 0)
                BiteTimer--;

            //sand balls
            /*if (AttackTimer > 0)
            {
                if (AttackTimer == 75)
                {
                    SoundEngine.PlaySound(SoundID.Item5, npc.position);
                }

                AttackTimer--;
            }

            if (AttackTimer <= 0)
            {
                float num265 = 12f;
                Vector2 pos = new(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
                float velocityX = Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2) - pos.X;
                float velocityY = Main.player[npc.target].position.Y - pos.Y;
                float num268 = (float)Math.Sqrt((double)(velocityX * velocityX + velocityY * velocityY));
                num268 = num265 / num268;
                velocityX *= num268 * 1.5f;
                velocityY *= num268 * 1.5f;

                if (FargoSoulsUtil.HostCheck && Main.player[npc.target].Center.Y <= npc.Center.Y && Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
                {
                    int num269 = 10;
                    int num270 = 31;
                    int proj = Projectile.NewProjectile(npc.GetSource_FromThis(), pos.X, pos.Y, velocityX, velocityY, num270, num269, 0f, Main.myPlayer, 0f, 0);
                    if (proj != Main.maxProjectiles)
                    {
                        Main.projectile[proj].ai[0] = 2f;
                        Main.projectile[proj].timeLeft = 300;
                        Main.projectile[proj].friendly = false;
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
                    }
                    npc.netUpdate = true;

                    AttackTimer = 75;
                }
            }*/

            //never fire sand balls from vanilla
            npc.ai[0] = 10;
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (BittenPlayer != -1)
                return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            base.ModifyHitPlayer(npc, target, ref modifiers);
            if (BittenPlayer == -1 && BiteTimer == 0)
            {
                target.longInvince = true;
                modifiers.FinalDamage *= 0.25f;
            }
        }
        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);
            if (BittenPlayer == -1 && BiteTimer == 0)
            {
                target.AddBuff(BuffID.Bleeding, 300); //im keeping the bleeding here it fits
                BittenPlayer = target.whoAmI;
                BiteTimer = 360;
                //NetSync(npc, false);

                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    // remember that this is target client side; we sync to server
                    var netMessage = Mod.GetPacket();
                    netMessage.Write((byte)FargowiltasSouls.PacketID.SyncAntlionGrab);
                    netMessage.Write((byte)npc.whoAmI);
                    netMessage.Write((byte)BittenPlayer);
                    netMessage.Write(BiteTimer);
                    netMessage.Send();
                }
            }
        }
    }
}
