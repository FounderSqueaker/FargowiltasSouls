using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Corruption;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Corruption
{
    public class EaterofSouls : EModeNPCBehaviour
    {
        //public EatersAndCrimeras() : base(420, ModContent.ProjectileType<CursedFlameHostile2>(), 8f, 0.8f, 75, 600, 45) { }

        public int AttackTimer = 0;

        public int JawLerp;

        public int JawClose;

        //Jaw Rotation breaks entirely when above 0.4f.
        public float JawRot;

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

        public override NPCMatcher CreateMatcher() =>
            new NPCMatcher().MatchTypeRange(
                NPCID.EaterofSouls,
                NPCID.BigEater,
                NPCID.LittleEater
            );

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.CursedInferno] = true;

            AttackTimer = Main.rand.Next(-80, 80);
        }

        public override void AI(NPC npc)
        {
            //npc.aiStyle = -1;

            JawRot = MathHelper.Lerp(JawRot, 0, ++JawClose * 0.08f);
            if (JawRot <= 0)
                JawRot = 0;


            ++AttackTimer;

            if (AttackTimer >= 360)
            {
                npc.velocity *= 0.8f;
                JawRot = MathHelper.Lerp(0, 0.4f, ++JawLerp * 0.07f);
                if (JawRot >= 0.4f)
                    JawRot = 0.4f;

                Vector2 dir = (npc.rotation + MathHelper.PiOver2).ToRotationVector2();
                float buildup = (AttackTimer - 360f) / (480f - 360f);
                int size = (int)(buildup * 16);
                if (Main.rand.NextBool(0.25f + 0.75f * buildup))
                    Dust.NewDust(npc.Center + dir * 30 * npc.scale - Vector2.One * size / 2, size, size, DustID.CursedTorch, dir.X * 1, dir.Y * 1, Scale: 1f + 1 * buildup);
            }

            if (AttackTimer >= 480)
            {
                Vector2 dir = (npc.rotation + MathHelper.PiOver2).ToRotationVector2();

                if (FargoSoulsUtil.HostCheck && npc.HasPlayerTarget)
                    Projectile.NewProjectile(Entity.GetSource_None(), npc.Center, dir * 8, ModContent.ProjectileType<CursedFlameHostile2>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.8f), 0, Main.myPlayer);
                AttackTimer = 0;
                JawLerp = 0;
                JawClose = 0;

                npc.velocity -= dir * 3f;
            }
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 position = npc.Center - Main.screenPosition;
            Texture2D Eater = ModContent.Request<Texture2D>("FargowiltasSouls/Content/NPCs/EternityModeNPCs/VanillaEnemies/Corruption/EaterofSouls", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D Jaw = ModContent.Request<Texture2D>("FargowiltasSouls/Content/NPCs/EternityModeNPCs/VanillaEnemies/Corruption/EaterJaw", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            Vector2 Origin = Jaw.Size() / 2f;
            Origin += Vector2.UnitX.RotatedBy((npc.rotation + JawRot));
            Origin -= Vector2.UnitY.RotatedBy((npc.rotation + JawRot) - 2);

            spriteBatch.Draw(Eater, npc.Center - Main.screenPosition, npc.frame, drawColor, npc.rotation, npc.frame.Size() / 2, npc.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(Jaw, position, null, drawColor, JawRot + npc.rotation, Origin , npc.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(Jaw, position, null, drawColor, -JawRot + npc.rotation, Origin, npc.scale, SpriteEffects.FlipHorizontally, 0f);
            return npc.IsABestiaryIconDummy && base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.Weak, 300);
        }
    }
}
