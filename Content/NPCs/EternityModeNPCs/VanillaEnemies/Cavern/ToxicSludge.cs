using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Cavern
{
    public class ToxicSludge : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.ToxicSludge);
        public bool Exploding;
        public int Timer;
        public override void AI(NPC npc)
        {
            base.AI(npc);
            //EModeGlobalNPC.Aura(npc, 200, BuffID.Poisoned, false, 188);

            if (Exploding)
            {
                npc.ai[0] = -3000; //stop jumping
                if (npc.velocity.Y == 0)
                    Timer++;

                if (Timer % 10 == 0 && Timer != 0) SoundEngine.PlaySound(SoundID.Zombie55 with {Volume = 1.5f, Pitch = MathHelper.Lerp(0, 1, (float)(Timer/30))}, npc.Center);
                if (Timer > 0)
                {
                    //rapid flashing
                    if (Timer % 10 > 5)
                        npc.Opacity = MathHelper.Lerp(npc.Opacity, 0.25f, 0.2f);
                    else npc.Opacity = MathHelper.Lerp(npc.Opacity, 0.75f, 0.2f);;

                    if (Timer == 60)
                    {
                        npc.life = 0;
                        npc.dontTakeDamage = false;
                        npc.checkDead();
                    }
                }
            }
        }
        public override bool CheckDead(NPC npc)
        {
            if (Exploding && Timer >= 60) return true;
            npc.life = 1;
            npc.active = true;
            Exploding = true;
            npc.dontTakeDamage = true;
            npc.netUpdate = true;
            if (npc.buffType[0] != 0) //cleanse all buffs
                npc.DelBuff(0);
            return false;
        }
        public override void OnKill(NPC npc)
        {
            if (FargoSoulsUtil.HostCheck)
            {
                int p = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ProjectileID.DD2OgreSpit, 0, 1f);
                if (p.IsWithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[p].timeLeft = 0; //insta ooze explosion
                }
            }
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (Exploding)
            {
                Texture2D texture = TextureAssets.Npc[NPCID.ToxicSludge].Value;
                SpriteEffects effects = npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Color color = Color.Lerp(drawColor, Color.Red, (float)Timer / 60f);
                spriteBatch.Draw(texture, npc.Center - Main.screenPosition, npc.frame, color * npc.Opacity, npc.rotation, npc.frame.Size() / 2, npc.scale, effects, 0f);
                return false;
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}
