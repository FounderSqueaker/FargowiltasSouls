using FargowiltasSouls.Content.Projectiles.Deathrays;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy
{
    internal class DD2CrystalDeathray : MutantSpecialDeathray
    {
        public DD2CrystalDeathray() : base(20, 1.25f) { }
        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.hide = true;
            Projectile.penetrate = -1;

            Projectile.FargoSouls().TimeFreezeImmune = true;
        }

        public override bool? CanDamage() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.Distance(FargoSoulsUtil.ClosestPointInHitbox(targetHitbox, Projectile.Center)) < TipOffset.Length())
                return true;

            return base.Colliding(projHitbox, targetHitbox);
        }

        Vector2 TipOffset => 18f * Projectile.scale * Projectile.velocity;

        public override void AI()
        {
            base.AI();
            Projectile.frameCounter += 60;

            NPC npc = Main.npc[(int)Projectile.ai[0]];

            if (!Main.dedServ && Main.LocalPlayer.active)
                FargoSoulsUtil.ScreenshakeRumble(6);

            Projectile.velocity = Projectile.velocity.SafeNormalize(-Vector2.UnitY);

            float num801 = 10f;
            Projectile.scale = (float)Math.Cos(Projectile.localAI[0] * MathHelper.PiOver2 / maxTime) * num801;
            if (Projectile.scale > num801)
                Projectile.scale = num801;

            if (npc.active && npc.type == NPCID.DD2EterniaCrystal)
            {
                Projectile.Center = npc.Center + 50f * Projectile.velocity + TipOffset + Main.rand.NextVector2Circular(5, 5);
            }
            else
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.localAI[0] == 0f)
            {
                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Weapons/Railgun") with { Volume = 0.5f }, Projectile.Center);
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Thunder") with { Volume = 0.5f }, npc.Center + Projectile.velocity * Math.Min(Main.screenWidth / 2, 900f));
                }

                Vector2 dustPos = npc.Center + Projectile.velocity * 50f;

                for (int i = 0; i < 40; i++)
                {
                    int dust = Dust.NewDust(dustPos - new Vector2(16, 16), 32, 32, DustID.Smoke, 0f, 0f, 100, default, 2f);
                    Main.dust[dust].velocity -= Projectile.velocity * 2;
                    Main.dust[dust].velocity *= 3f;
                    Main.dust[dust].velocity += npc.velocity / 2;
                }

                for (int i = 0; i < 30; i++)
                {
                    int dust = Dust.NewDust(dustPos - new Vector2(16, 16), 32, 32, DustID.FireworkFountain_Pink, 0f, 0f, 100, default, 2f);
                    Main.dust[dust].velocity -= Projectile.velocity * 2;
                    Main.dust[dust].velocity *= 5f;
                    Main.dust[dust].velocity *= Main.rand.NextFloat(1f, 2f);
                    Main.dust[dust].velocity += npc.velocity / 2;
                }

                for (int j = 0; j < 10; j++)
                {
                    int gore = Gore.NewGore(Projectile.GetSource_FromThis(), dustPos, -Projectile.velocity, Main.rand.Next(61, 64), 1f);
                    Main.gore[gore].velocity -= Projectile.velocity;
                    Main.gore[gore].velocity.Y += 2f;
                    Main.gore[gore].velocity *= 4f;
                    Main.gore[gore].velocity += npc.velocity / 2;
                }
            }

            if (++Projectile.localAI[0] >= maxTime)
            {
                Projectile.Kill();
                return;
            }

            float amount = 0.5f;
            Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], 3000f, amount);

            Projectile.position -= Projectile.velocity;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            const int increment = 200;
            for (int i = 0; i < 3000; i += increment)
            {
                float offset = i + Main.rand.NextFloat(-increment, increment);
                if (offset < 0)
                    offset = 0;
                if (offset > 3000)
                    offset = 3000;
                float spawnRange = Projectile.scale * 16f;
                int d = Dust.NewDust(Projectile.position + Projectile.velocity * offset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-spawnRange, spawnRange),
                    Projectile.width, Projectile.height, DustID.PinkTorch, Scale: Main.rand.NextFloat(2f, 3f));
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity += Projectile.velocity * 20f;
                //Main.dust[d].velocity *= Main.rand.NextFloat(12f, 24f) / 10f * Projectile.scale;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(ItemID.BrightPurpleDye);
            shader.Apply(Projectile, new Terraria.DataStructures.DrawData?());

            bool retval = base.PreDraw(ref lightColor);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return retval;
        }
    }
}
