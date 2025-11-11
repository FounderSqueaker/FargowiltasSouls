using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Night
{
    public class PossessedArmor : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.PossessedArmor);
        public float Timer = 0f;
        public const int DashPositionCount = 20;
        public Vector2[] DashPositions = new Vector2[DashPositionCount];
        public int UsedDashPositions = 0;
        public int ConsumedDashPositions = 0;
        public override void SetDefaults(NPC npc)
        {
            npc.knockBackResist = 0f;
        }
        public override void AI(NPC npc)
        {
            base.AI(npc);

            //EModeGlobalNPC.Aura(npc, 400, BuffID.BrokenArmor, false, 37);

            if (npc.GetLifePercent() < 0.5f && Timer == 0f)
            {
                Timer = 1; // activate shade dash
                //npc.dontTakeDamage = true; // immaterial until shade dash ends
            }
            if (Timer > 0 && npc.HasPlayerTarget)
            {
                Timer++;
                if (Timer % 3 == 0 && UsedDashPositions < DashPositionCount)
                {
                    if (UsedDashPositions == 1)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost with { Volume = 2f }, npc.Center);
                    }
                    UsedDashPositions++;
                    Vector2 previousPos = UsedDashPositions <= 1 ? npc.Center : DashPositions[UsedDashPositions - 2];
                    //Vector2 dir = Vector2.Lerp(npc.DirectionTo(Main.player[npc.target].Center), previousPos.DirectionTo(Main.player[npc.target].Center), 0.5f);
                    Vector2 dir = npc.DirectionTo(Main.player[npc.target].Center);
                    if (Collision.CanHitLine(previousPos, 1, 1, previousPos + dir * 45, 1, 1))
                        previousPos += dir * 45;
                    DashPositions[UsedDashPositions - 1] = previousPos;
                }
                if (UsedDashPositions >= DashPositionCount && Timer % 1 == 0) // dash
                {
                    if (ConsumedDashPositions == 1)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaivePierce with { Volume = 2f }, npc.Center);
                    }
                    ConsumedDashPositions++;
                    npc.Center = DashPositions[ConsumedDashPositions - 1];
                    for (int i = 0; i <= ConsumedDashPositions - 1; i++)
                    {
                        DashPositions[i] = npc.Center;
                    }
                    if (ConsumedDashPositions >= DashPositionCount)
                    {
                        UsedDashPositions = 0;
                        ConsumedDashPositions = 0;
                        Timer = -1;
                        //npc.dontTakeDamage = false;
                    }
                }
            }
        }

        public override void OnKill(NPC npc)
        {
            base.OnKill(npc);

            if (FargoSoulsUtil.HostCheck && Main.rand.NextBool())
                FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), npc.Center, NPCID.Ghost);
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPCID.PossessedArmor].Value;
            SpriteEffects spriteEffects = npc.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            for (int i = 0; i < UsedDashPositions; i++)
            {
                Color color = Color.Purple with {A = 0};
                Vector2 value4 = DashPositions[i];
                DrawData oldGlow = new(texture, value4 - screenPos + new Vector2(0, npc.gfxOffY), npc.frame, color * (npc.Opacity / 1.5f), npc.rotation, npc.frame.Size() / 2, npc.scale, spriteEffects, 0);
                GameShaders.Misc["LCWingShader"].UseColor(Color.Purple).UseSecondaryColor(Color.White);
                GameShaders.Misc["LCWingShader"].Apply(oldGlow);
                oldGlow.Draw(spriteBatch);
            }
            if (Timer > 0)
            {
                //Main.spriteBatch.UseBlendState(BlendState.Additive);
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (Timer > 0)
            {
                Main.spriteBatch.ResetToDefault();
            }
        }
    }
}
