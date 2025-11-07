using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.CustomEnemies.OOA;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    internal class BartenderUnconscious : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.BartenderUnconscious);

        public int State;
        public int Timer;

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            base.OnSpawn(npc, source);

            Timer = 300;
            if (source is EntitySource_Parent parent && parent.Entity is NPC parentNPC && parentNPC.type == ModContent.NPCType<TavernkeepPortal>())
                State = 1;
            else
                State = -1;

            if (Main.dedServ)
                NetSync(npc);
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(State);
            binaryWriter.Write(Timer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            State = binaryReader.ReadInt32();
            Timer = binaryReader.ReadInt32();
        }

        public override bool SafePreAI(NPC npc)
        {
            if (State < 0)
                return base.SafePreAI(npc);

            Timer--;
            int rate = (int)Math.Floor(300f - Timer);

            if (Timer % rate == 0)
                Gore.NewGore(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, GoreID.OgreSpit1);

            if (State == 1 && npc.velocity.Y == 0)
            {
                SoundEngine.PlaySound(FargosSoundRegistry.DeviFloorImpact, npc.Center);
                Gore.NewGoreDirect(npc.GetSource_FromThis(), npc.Bottom, new Vector2(5, 0), Main.rand.Next(11, 14), Scale: 0.8f);
                Gore.NewGoreDirect(npc.GetSource_FromThis(), npc.Bottom, new Vector2(-5, 0), Main.rand.Next(11, 14), Scale: 0.8f);

                int[] gores = [GoreID.OgreSpit1, GoreID.OgreSpit2, GoreID.OgreSpit3];
                for (int i = 0; i < 20; i++)
                {
                    Gore.NewGore(npc.GetSource_FromThis(), npc.Bottom, new Vector2(0, -1).RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)), Main.rand.NextFromList(gores));
                }

                State = 0;
            }

            return base.SafePreAI(npc);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D text = TextureAssets.Npc[npc.type].Value;
            Rectangle frame = text.Frame();
            Vector2 origin2 = frame.Size() / 2;
            SpriteEffects flip = npc.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Color color = Color.Lerp(Color.Green, drawColor, (400 - Timer) / 300f);
            if (Timer < 100)
                color = drawColor;

            Main.EntitySpriteDraw(text, npc.Center - Main.screenPosition, frame, color, 0, origin2, npc.scale, flip);
            return false;
        }
    }
}
