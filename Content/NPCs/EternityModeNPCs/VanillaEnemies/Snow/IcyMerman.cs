using Fargowiltas.Content.Items.Tiles;
using Fargowiltas.Content.Projectiles;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Snow
{
    public class IcyMerman : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.IcyMerman);

        public int State = 0;
        public Vector2 targetPos;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(State);
            binaryWriter.WriteVector2(targetPos);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            State = binaryReader.ReadInt32();
            targetPos = binaryReader.ReadVector2();
        }

        public override void SetDefaults(NPC entity)
        {
            base.SetDefaults(entity);
        }

        public override void OnFirstTick(NPC npc)
        {
            // Immune to water physics
            npc.waterMovementSpeed = 1f;
 
            if (!npc.wet)
            {
                State = 3;
                return;
            }
        }

        public override bool SafePreAI(NPC npc)
        {
            npc.TargetClosest();
            switch (State)
            {
                // Hiding
                case 0:
                    if (!npc.wet)
                    {
                        State = 2;
                        break;
                    }
                    if (!npc.HasValidTarget)
                        break;

                    Player target = Main.player[npc.target];
                    if (npc.life < npc.lifeMax)
                    {
                        targetPos = target.Center;
                        State = 1;
                        break;
                    }

                    float attackDist = 110;
                    bool inRange = Math.Abs(target.Center.X - npc.Center.X) < attackDist && target.Center.Y < npc.Center.Y;
                    if (inRange)
                    {
                        targetPos = target.Center;
                        npc.noTileCollide = true;
                        State = 1;
                    }
                    break;
                // Lunging
                case 1:
                    FargoSoulsUtil.DustRing(npc.Center, 20, DustID.Frost, 5f);
                    Vector2 targetPoint = targetPos - Vector2.UnitY * 200;
                    float distanceScale = MathHelper.Clamp(npc.Distance(targetPoint) / 1000f, 0f, 1f);
                    float vel = 7f + 10f * distanceScale;
                    npc.velocity = npc.DirectionTo(targetPoint) * vel;
                    SoundStyle sound = Main.rand.NextFromList(SoundID.Zombie21, SoundID.Zombie22, SoundID.Zombie23);
                    SoundEngine.PlaySound(sound, npc.Center);
                    State = 2;
                    break;
                // Completing Lunge
                case 2:
                    destroyIce(npc);
                    Lighting.AddLight(npc.Center, new Vector3(1, 1, 1));
                    if (npc.velocity.Y > 0)
                    {
                        npc.noTileCollide = false;
                        State = 3;
                    }
                    break;
                // Normal AI
                case 3:
                    return base.SafePreAI(npc);
            }
            return false;
        }

        public void destroyIce(NPC npc)
        {
            if (!FargoSoulsUtil.HostCheck)
                return;

            // Iterate to the right
            float xPos = npc.Top.X - (npc.Top.X % 16) - 16;
            float y = npc.Top.Y - (npc.Top.Y % 16) - 16;
            int yPosition = (int) (y / 16f);
            int range = 2;
            for (int x = -range; x <= range; x++)
            {
                int xPosition = (int)(x + (xPos / 16f));

                if (xPosition < 0 || xPosition >= Main.maxTilesX || yPosition < 0 || yPosition >= Main.maxTilesY)
                    break;

                Tile t = Main.tile[xPosition, yPosition];
                if (t == null)
                    break;

                if (!FargoGlobalProjectile.OkayToDestroyTileAt(xPosition, yPosition) || FargoGlobalProjectile.TileIsLiterallyAir(t))
                    break;

                if (t.TileType != TileID.BreakableIce)
                    break;
                else
                {
                    WorldGen.KillTile(xPosition, yPosition, noItem: true);
                    Vector2 blockPos = new(16 * xPosition, 16 * yPosition);
                    SoundEngine.PlaySound(SoundID.Item27, blockPos);
                    for (int i = 0; i < 5; i++)
                        Dust.NewDust(blockPos, 16, 16, DustID.Ice);
                }

            }
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // reduced opacity when hiding
            if (State == 0)
            {
                Texture2D texture = TextureAssets.Npc[npc.type].Value;
                Rectangle frame = npc.frame;
                SpriteEffects flip = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                Vector2 origin = frame.Size() / 2 + (5 * Vector2.UnitY);
                Main.EntitySpriteDraw(texture, npc.Center - screenPos, frame, drawColor * 0.25f, npc.rotation, origin, npc.scale, flip);
                return false;
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            // merman should only spawn in water
            base.EditSpawnPool(pool, spawnInfo);
            if (!spawnInfo.Water)
                pool[NPCID.IcyMerman] = 0f;
        }
    }
}
