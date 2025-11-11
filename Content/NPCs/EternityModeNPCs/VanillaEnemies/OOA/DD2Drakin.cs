using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2Drakin : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.DD2DrakinT2,
            NPCID.DD2DrakinT3
        );

        public int Timer = -60;
        public int TimeStart = -1;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(Timer);
            binaryWriter.Write(TimeStart);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            Timer = binaryReader.ReadInt32();
            TimeStart = binaryReader.ReadInt32();
        }

        const int range = 300; 

        public override bool SafePreAI(NPC npc)
        {
            Timer++;
            if (Timer < 0)
                return base.SafePreAI(npc);

            if (Timer > 300)
            {
                if (TimeStart == -1 && FindAlliesInRange(npc))
                {
                    SoundEngine.PlaySound(SoundID.DD2_WyvernScream with { Pitch = -0.7f, Variants = [0] }, npc.Center);
                    TimeStart = Timer;
                    return false;
                }
            }
            if (TimeStart >= 0)
            {
                npc.ai[0] = 0;
                int timeElapsed = Timer - TimeStart;
                if (timeElapsed < 10)
                    npc.velocity.X *= 0.9f;
                else
                    npc.velocity.X *= 0f;
                
                if (timeElapsed > 120)
                {
                    TimeStart = -1;
                    Timer = 0;
                    return base.SafePreAI(npc);
                }

                // Shout visual
                Vector2 mouthPos = npc.Center + new Vector2(npc.spriteDirection * 30, 5);
                float rot = MathHelper.PiOver2 - npc.spriteDirection * MathHelper.PiOver2 + Main.rand.NextFloat(-MathHelper.Pi / 3, MathHelper.Pi / 3);
                new SparkParticle(mouthPos + 10 * Vector2.UnitX.RotatedBy(rot), 3 * Vector2.UnitX.RotatedBy(rot), Color.Purple, 0.3f, 8).Spawn();

                const float speedMult = 2f;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n.Distance(npc.Center) < range + 100 && EModeDD2GlobalNPC.IsInstance(n) && !EModeDD2Event.IsDD2Boss(n.type) && !n.EModeDD2().DrakinBuff && n.whoAmI != npc.whoAmI)
                    {
                        n.EModeDD2().DrakinBuff = true;
                        if (n.velocity.Length() == 0)
                            continue;
                        if (timeElapsed % 2 == 0)
                           new SparkParticle(n.Top - (n.width * n.spriteDirection) * Vector2.UnitX + Main.rand.Next(0, n.height) * Vector2.UnitY, Main.rand.NextFloat(1, 3) * Vector2.UnitX.RotatedBy(n.rotation), Color.Purple, 0.5f, 14).Spawn();
                        n.position.X += (speedMult - 1) * n.velocity.X;
                        if (n.noGravity)
                            n.position.Y += (speedMult - 1) * n.velocity.Y;
                    }
                }
                return false;
            }

            return base.SafePreAI(npc);
        }

        private bool FindAlliesInRange(NPC npc)
        {
            foreach (var n in Main.ActiveNPCs)
            {
                if (n.Distance(npc.Center) < range && EModeDD2GlobalNPC.IsInstance(n) && !EModeDD2Event.IsDD2Boss(n.type) && n.whoAmI != npc.whoAmI)
                    return true;
            }
            return false;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (TimeStart > -1)
                npc.frame.Y = npc.frame.Height * 14;

            Texture2D texture = TextureAssets.Npc[npc.type].Value;
            Rectangle frame = npc.frame;
            SpriteEffects flip = npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float t = Timer - TimeStart;
            if (TimeStart > -1 && t < 60)
            {
                float scale = (120 * t - (t * t)) / (2 * 720);
                float opacity = 1 - (scale / 2.5f);
                Main.EntitySpriteDraw(texture, npc.position - screenPos + npc.Size / 2, frame, drawColor * opacity, npc.rotation, frame.Size() / 2, npc.scale * scale, flip);
            }
            Main.EntitySpriteDraw(texture, npc.position - screenPos + npc.Size/2, frame, drawColor, npc.rotation, frame.Size() / 2, npc.scale, flip);
            return false;
        }
    }
}
