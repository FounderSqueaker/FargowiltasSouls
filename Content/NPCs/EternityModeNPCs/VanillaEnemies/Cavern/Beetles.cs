using System;
using System.Linq;
using Fargowiltas.Common.Configs;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Jungle;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Humanizer;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Cavern
{
    public abstract class Beetles : EModeNPCBehaviour
    {
        protected virtual Color AuraColor { get; }

        protected virtual int AuraSize => 300;
        protected virtual int DustType { get; }

        protected virtual void BeetleEffect(NPC affectedNPC) { }

        public int Frame;
        public int FrameCounter;
        public int TimeWithoutFriend;
        public int LacCloudTimer;
        public bool MovingToTarget;

        public override void OnFirstTick(NPC npc)
        {
            npc.defense = npc.defDefense = 5; // originally 10
            npc.height = 32;
            npc.noGravity = true;
        }
        public override bool SafePreAI(NPC npc)
        {
            if (FrameCounter++ % 3 == 0)
            {
                if (Frame < 2) Frame++;
                else Frame = 0;
            }
            npc.direction = npc.spriteDirection = Math.Sign(npc.velocity.X);

            foreach (NPC n in Main.npc.Where(n => n.active && !n.friendly && n.type != npc.type))
            {
                if (n.Distance(npc.Center) < AuraSize)
                {
                    //actual buff
                    BeetleEffect(n);
                    n.Eternity().BeetleTimer = 60;

                    //line of dust
                    Vector2 headpos = npc.Center + new Vector2(8 * npc.direction, -8);
                    int length = (int)(headpos - n.Top).Length() / 10;
                    Vector2 offset = Vector2.Normalize(headpos - n.Top) * 10f;
                    for (int i = 0; i <= length; i++)
                    {
                        int w = Dust.NewDust(n.Top + offset * i, 0, 0, DustType, Alpha: 150, Scale: 0.5f);
                        Main.dust[w].noGravity = true;
                        Main.dust[w].velocity = Vector2.Zero;
                    }
                }
            }

            MovingToTarget = false;
            foreach (NPC n in Main.npc.Where(n => n.active && !n.friendly && n.type != npc.type))
            {
                //choose an enemy to follow, separate search so break works
                if (Collision.CanHitLine(npc.position, npc.width, npc.height, n.position, n.width, n.height))
                {
                    Vector2 pos = n.Top + new Vector2(-32 * n.direction, -24);
                    if (npc.Center.Distance(pos) > 2 && npc.Center.Distance(pos) < 16*100)
                    {
                        npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, pos, npc.velocity, 0.1f, 0.5f);
                        if (npc.Center.Distance(pos) < 16*2) npc.direction = npc.spriteDirection = n.direction;
                        MovingToTarget = true;
                        TimeWithoutFriend = 0;
                        break;
                    }
                }
            }

            if (!MovingToTarget)
            {
                TimeWithoutFriend++;
                if (TimeWithoutFriend >= 30) //going rogue
                {
                    npc.TargetClosest();
                    if (npc.HasValidTarget)
                    {
                        npc.SimpleFlyMovement(Main.player[npc.target].Center - npc.Center, 0.05f);
                        //npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, Main.player[npc.target].Center, npc.velocity, 0.03f, 0.2f);
                        npc.direction = npc.spriteDirection = Math.Sign(npc.HorizontalDirectionTo(Main.player[npc.target].Center));
                    }
                    if (npc.type == NPCID.LacBeetle) LacCloudTimer++;
                }
                else npc.velocity *= 0.98f;
            }

            if (npc.type == NPCID.LacBeetle && npc.HasValidTarget && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0)) //dye clouds
            {
                if (LacCloudTimer++ >= 180)
                {
                    LacCloudTimer = Main.rand.Next(-30, 31);
                    int dmg = FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage);
                    Projectile.NewProjectile(npc.GetSource_Death(), npc.Center, Vector2.Zero, ModContent.ProjectileType<LacBeetleCloud>(), dmg, 0);
                }
            }
            return false;
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);
        }

        public override bool? CanFallThroughPlatforms(NPC npc) => true;
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => TimeWithoutFriend >= 30 && !MovingToTarget;

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //flying anim
            Vector2 pos = npc.Center - Main.screenPosition;
            string type = npc.type == NPCID.LacBeetle ? "lac" : npc.type == NPCID.CyanBeetle ? "cyan" : "cochineal";
            Texture2D t = FargoAssets.GetTexture2D("FargowiltasSouls/Content/NPCs/EternityModeNPCs/VanillaEnemies/Cavern/" + type + "_beetle", Name, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            int frameHeight = t.Height / 3;
            int frame = frameHeight * Frame;
            Rectangle rectangle = new(0, frame, t.Width, frameHeight);
            SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(t, pos, rectangle, drawColor, npc.rotation, rectangle.Size() / 2f, npc.scale, effects, 0f);

            //aura. javier HATES auras.
            /*Color darkColor = Color.Lerp(AuraColor, Color.Black, 0.5f);
            Color mediumColor = AuraColor;
            Color lightColor2 = Color.Lerp(AuraColor, Color.White, 0.5f);

            Vector2 auraPos = npc.Center;
            var target = Main.LocalPlayer;
            var blackTile = TextureAssets.MagicPixel;
            var diagonalNoise = FargoAssets.WavyNoise;
            if (!blackTile.IsLoaded || !diagonalNoise.IsLoaded)
                return false;
            var maxOpacity = ModContent.GetInstance<FargoClientConfig>().TransparentFriendlyProjectiles;

            ManagedShader borderShader = ShaderManager.GetShader("FargowiltasSouls.GenericInnerAura");
            borderShader.TrySetParameter("colorMult", 7.35f);
            borderShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            borderShader.TrySetParameter("radius", AuraSize);
            borderShader.TrySetParameter("anchorPoint", auraPos);
            borderShader.TrySetParameter("screenPosition", Main.screenPosition);
            borderShader.TrySetParameter("screenSize", Main.ScreenSize.ToVector2());
            borderShader.TrySetParameter("playerPosition", target.Center);
            borderShader.TrySetParameter("maxOpacity", maxOpacity);
            borderShader.TrySetParameter("darkColor", darkColor.ToVector4());
            borderShader.TrySetParameter("midColor", mediumColor.ToVector4());
            borderShader.TrySetParameter("lightColor", lightColor2.ToVector4());
            borderShader.TrySetParameter("opacityAmp", 1f);

            Main.spriteBatch.GraphicsDevice.Textures[1] = diagonalNoise.Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, borderShader.WrappedEffect, Main.GameViewMatrix.TransformationMatrix);
            Rectangle rekt = new(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth, Main.screenHeight);
            Main.spriteBatch.Draw(blackTile.Value, rekt, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            */
            return false;
        }
    }

    public class CochinealBeetle : Beetles
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.CochinealBeetle);
        protected override Color AuraColor => Color.Red;
        protected override int DustType => DustID.LavaMoss;
        protected override void BeetleEffect(NPC affectedNPC) => affectedNPC.Eternity().BeetleOffenseAura = true;
    }

    public class CyanBeetle : Beetles
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.CyanBeetle);
        protected override Color AuraColor => Color.Cyan;
        protected override int DustType => DustID.XenonMoss;
        protected override void BeetleEffect(NPC affectedNPC) => affectedNPC.Eternity().BeetleDefenseAura = true;
    }

    public class LacBeetle : Beetles
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.LacBeetle);
        protected override Color AuraColor => Color.Magenta;
        protected override int DustType => DustID.VioletMoss;
        protected override void BeetleEffect(NPC affectedNPC) => affectedNPC.Eternity().BeetleUtilAura = true;
    }
}
