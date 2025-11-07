using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Cavern;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Cavern
{
    public class Tim : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Tim);

        public override void SetStaticDefaults()
        {
            NPCID.Sets.CantTakeLunchMoney[NPCID.Tim] = true;
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lavaImmune = true;
            npc.lifeMax *= 2;
            npc.damage /= 2;
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[BuffID.OnFire] = true;

            ResetToIdle(npc);
        }

        public int HeldProj = -1;
        public int Timer = -60;
        public int State = -1;
        public int PreviousState = -1;
        public int DrinkType = -1;
        public int DrinkStart = -1;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(State);
            binaryWriter.Write(PreviousState);
            binaryWriter.Write(Timer);
            binaryWriter.Write(DrinkType);
            binaryWriter.Write(DrinkStart);
            binaryWriter.Write(HeldProj);
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            State = binaryReader.ReadInt32();
            PreviousState = binaryReader.ReadInt32();
            Timer = binaryReader.ReadInt32();
            DrinkType = binaryReader.ReadInt32();
            DrinkStart = binaryReader.ReadInt32();
            HeldProj = binaryReader.ReadInt32();
        }

        public override void AI(NPC npc)
        {
            npc.ai[1] = 0; // stop attacks
            npc.ai[0] = 1;

            Timer++;

            if (Timer < 45)
            {
                if (State == -1)
                {
                    State = PreviousState;
                    while (State == PreviousState)
                    {
                        State = Main.rand.Next(0, 3);
                    }

                    //State = Main.rand.Next(0, 2);
                    //State = 2;
                    npc.netUpdate = true;
                }
                return;
            }

            if (State != -2 && !Main.player.Any(p => p.active && !p.dead && !p.ghost && p.Distance(npc.Center) < 3000))
            {
                // no nearby players, attempt to leave
                ResetToIdle(npc);
                Timer = -90;
                State = -2;
            }

            // timer (ai0) maxes at 649
            switch (State)
            {
                case -2:
                    Recall(npc);
                    break;
                case 0:
                    FireBreath(npc);
                    break;
                case 1:
                    MagicBlast(npc);
                    break;
                case 2:
                    SummonHat(npc);
                    break;
            }

            base.AI(npc);
        }

        public void ResetToIdle(NPC npc)
        {
            npc.ai[0] = 650; // force teleport
            Timer = 0;
            PreviousState = State;
            State = -1;
            if (HeldProj != -1)
            {
                Main.projectile[HeldProj].Kill();
                HeldProj = -1;
            }
            npc.netUpdate = true;
        }

        #region Animation Methods
        /// <summary>
        /// Makes Tim drink the given potion type at the given time.
        /// Setting pColor will make Tim emit buff particles of the corresponding color.
        /// </summary>
        public void DrinkAnim(NPC npc, int type, int timer, Color? pColor = null)
        {
            if (Timer == timer)
            {
                DrinkType = type;
                DrinkStart = Timer;
                SoundEngine.PlaySound(SoundID.Item3, npc.Center);
            }
            else if (pColor.HasValue && Timer > timer) {
                Vector2 pos = npc.TopLeft + Main.rand.NextFloat(0, npc.width) * Vector2.UnitX + Main.rand.NextFloat(0, npc.height) * Vector2.UnitY;
                new SparkParticle(pos, -1 * Vector2.UnitY, pColor.Value, 0.2f, 9).Spawn();
            }
        }

        public void ResetAnim(NPC npc)
        {
            DrinkType = -1;
            DrinkStart = -1;
            npc.netUpdate = true;
        }
        #endregion

        #region States
        public void Recall(NPC npc)
        {
            if (Main.player.Any(p => p.active && !p.dead && !p.ghost && p.Distance(npc.Center) < 2000))
            {
                ResetToIdle(npc);
            }

            DrinkAnim(npc, ItemID.RecallPotion, 45);
            if (Timer == 45)
                SoundEngine.PlaySound(SoundID.Item6, npc.Center);

            if (Timer >= 45 && Timer <= 55)
            {
                for (int i = 0; i < 6; i++)
                {
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.MagicMirror);
                }
            }

            if (Timer == 55)
            {
                npc.active = false;
                npc.netUpdate = true;
            }
        }

        public void FireBreath(NPC npc)
        {
            DrinkAnim(npc, ItemID.InfernoPotion, 45, Color.OrangeRed);

            if (Timer == 55 && FargoSoulsUtil.HostCheck)
            {
                HeldProj = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Main.rand.NextFloat(4, 8) * Vector2.Zero, ModContent.ProjectileType<TimInferno>(), npc.damage / 2, 1f, ai0: -1);
                npc.netUpdate = true;
            }
            else if (Timer > 55)
            {
                if (Timer % 55 == 0 && npc.HasValidTarget && Timer < 400)
                {
                    SoundEngine.PlaySound(SoundID.Item8, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        float rot = (Main.player[npc.target].Center - npc.Center).ToRotation();
                        float spread = 0.15f;
                        for (int i = 0; i < 4; i++)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Main.rand.NextFloat(6, 12) * Vector2.UnitX.RotatedBy(rot + Main.rand.NextFloat(-spread, spread)), ModContent.ProjectileType<TimFireball>(), npc.damage / 2, 1f);
                        }
                    }
                }
            }
            if (Timer > 450)
                ResetToIdle(npc);
        }

        public void MagicBlast(NPC npc)
        {
            DrinkAnim(npc, ItemID.MagicPowerPotion, 45, Color.Purple);

            if (!npc.HasValidTarget)
                return;

            Player p = Main.player[npc.target];

            if (Timer < 370)
            {
                if (Timer % 90 == 24)
                {
                    SoundEngine.PlaySound(SoundID.Item28, npc.Center);
                    for (int i = 0; i < 20; i++)
                    {
                        float rot = MathHelper.TwoPi * i / 20;
                        new SparkParticle(npc.Center, 6 * Vector2.UnitX.RotatedBy(rot), Color.Purple, 0.3f, 9).Spawn();
                    }
                }
                if (Timer % 90 == 44)
                {
                    SoundEngine.PlaySound(SoundID.Item8, npc.Center);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, 12 * Vector2.UnitX.RotatedBy((p.Center - npc.Center).ToRotation()), ModContent.ProjectileType<TimMagicBlast>(), npc.damage, 2f);
                }
            }

            if (Timer > 400)
                ResetToIdle(npc);
        }

        public void SummonHat(NPC npc)
        {
            DrinkAnim(npc, ItemID.SummoningPotion, 45, Color.Lime);
            if (Timer == 50)
            {
                SoundEngine.PlaySound(SoundID.Item44, npc.Center);
                if (FargoSoulsUtil.HostCheck)
                    HeldProj = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center - 20 * Vector2.UnitY, Vector2.Zero, ModContent.ProjectileType<TimHatSummon>(), npc.damage / 2, 2f);
                npc.netUpdate = true;
            }

            DrinkAnim(npc, ItemID.RegenerationPotion, 105, Color.Pink);
            if (Timer > 105 && Timer % 90 == 0 && FargoSoulsUtil.HostCheck)
            {
                int amount = (int)MathHelper.Min(npc.lifeMax - npc.life, 10);
                if (amount == 0)
                    return;

                npc.HealEffect(amount);
                npc.life += amount;
                npc.netUpdate = true;
            }

            if (Timer > 450)
                ResetToIdle(npc);
        }
        #endregion

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float time = Timer - DrinkStart;
            //if (DrinkStart != -1)
            //    Main.NewText(Timer + ", " + DrinkStart);

            if (State == -1 || DrinkStart == -1)
                return base.PreDraw(npc, spriteBatch, screenPos, drawColor);

            if (time > 15)
            {
                ResetAnim(npc);
                return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
            }

            Texture2D texture = TextureAssets.Item[DrinkType].Value;


            Texture2D timText = TextureAssets.Npc[NPCID.Tim].Value;
            Rectangle timFrame = npc.frame;
            SpriteEffects flip = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(timText, npc.Center - screenPos - Vector2.UnitY * 11 * npc.scale, timFrame, drawColor, npc.rotation, timFrame.Size() / 2, npc.scale, flip);

            Rectangle frame = texture.Frame();
            float rot = -1 * npc.spriteDirection * MathHelper.Lerp(-MathHelper.PiOver2, 0.75f * MathHelper.PiOver2, (time - 40) / 20f);
            Vector2 offset = npc.spriteDirection * npc.scale * 10 * Vector2.UnitX.RotatedBy(rot);

            Main.EntitySpriteDraw(texture, npc.Center - screenPos + offset, frame, new(255, 255, 255), rot, frame.Size() / 2, npc.scale * 1.2f, flip);
            Main.EntitySpriteDraw(texture, npc.Center - screenPos + offset, frame, drawColor, rot, frame.Size()/2, npc.scale, flip);
            return false;
        }
    }
}
