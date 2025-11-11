using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Hell;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Hell
{
    public class HellBats : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange([NPCID.Hellbat, NPCID.Lavabat]);
        public bool Death;
        public int Timer = 0;
        public static int DeathChargeTime => 60;
        public override void SetDefaults(NPC npc)
        {
            if (NPC.downedMechBossAny && npc.lifeMax < 300)
            {
                npc.lifeMax = 300;
            }
        }
        public override bool SafePreAI(NPC npc)
        {
            if (Death)
            {
                Timer++;
                if (!npc.HasPlayerTarget)
                {
                    npc.TargetClosest();
                }
                if (npc.HasPlayerTarget)
                {
                    if (Timer < DeathChargeTime * 0.2f)
                    {
                        float spd = 0.2f;
                        npc.velocity += npc.DirectionTo(Main.player[npc.target].Center) * spd;
                    }
                    else
                    {
                        npc.velocity *= 0.94f;
                    }
                }
                int dAmount = (int)(6 * (float)Timer / DeathChargeTime);
                for (int i = 0; i < dAmount; i++)
                {
                    float vel = 12f * (float)Timer / DeathChargeTime;
                    int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, Scale: 4f);
                    Main.dust[d].velocity = (Main.dust[d].position - npc.Center).SafeNormalize(Vector2.UnitX) * vel;
                    Main.dust[d].noGravity = true;
                }
                if (Timer >= DeathChargeTime)
                {
                    npc.life = 0;
                    npc.dontTakeDamage = false;
                    npc.checkDead();
                }
            }
            return base.SafePreAI(npc);
        }
        public override bool CheckDead(NPC npc)
        {
            if (Death && Timer >= DeathChargeTime)
                return true;
            if (!Death)
                npc.velocity *= 0;
            npc.life = 1;
            npc.active = true;
            Death = true;
            npc.dontTakeDamage = true;
            npc.netUpdate = true;
            return false;
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (Death)
            {
                Texture2D tex = TextureAssets.Npc[npc.type].Value;
                spriteBatch.Draw(tex, npc.Center - screenPos + Vector2.UnitY * npc.gfxOffY, npc.frame, npc.GetAlpha(drawColor), npc.rotation, npc.frame.Size() / 2, npc.scale, SpriteEffects.None, 0);

                //Main.spriteBatch.UseBlendState(BlendState.Additive);
                float lerp = (float)Timer / DeathChargeTime;
                Color glowColor = Color.Red;
                float glowOpacity = lerp;
                spriteBatch.Draw(tex, npc.Center - screenPos + Vector2.UnitY * npc.gfxOffY, npc.frame, glowColor * glowOpacity, npc.rotation, npc.frame.Size() / 2, npc.scale, SpriteEffects.None, 0);
                //Main.spriteBatch.ResetToDefault();
                return false;
            }
            return true;
        }
        public override void OnKill(NPC npc)
        {
            if (FargoSoulsUtil.HostCheck)
            {
                // explod
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<BatExplosion>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 2f), 1f);
            }
        }
    }
}
