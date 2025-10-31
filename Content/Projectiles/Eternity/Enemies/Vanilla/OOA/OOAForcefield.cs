using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA;
using FargowiltasSouls.Core;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
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
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA
{
    public class OOAForcefield : ModProjectile
    {
        public ref float Timer => ref Projectile.ai[0];

        public ref float ArcAngle => ref Projectile.ai[1];

        public ref float Width => ref Projectile.ai[2];

        public ref float maxTime => ref Projectile.localAI[2];

        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles", "Empty");

        public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;

        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 5;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(npc);
            writer.Write(Projectile.localAI[1]);
            writer.Write(Projectile.localAI[2]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            npc = reader.Read7BitEncodedInt();
            Projectile.localAI[1] = reader.ReadSingle();
            Projectile.localAI[2] = reader.ReadSingle();
        }
        int npc;
        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Parent parent && parent.Entity is NPC parentNpc && (parentNpc.type == NPCID.DD2WitherBeastT2 || parentNpc.type == NPCID.DD2WitherBeastT3))
            {
                npc = parentNpc.whoAmI;
            }
        }

        public override void AI()
        {
            NPC parent = Main.npc[npc];
            if (parent.active && (parent.type == NPCID.DD2WitherBeastT2 || parent.type == NPCID.DD2WitherBeastT3))
            {
                Projectile.timeLeft++;
                Projectile.Center = parent.Center - 10 * Vector2.UnitY;
                Projectile.velocity *= 0;
                Projectile.direction = (int)Projectile.HorizontalDirectionTo(EModeDD2Event.GetEterniaCrystal().Center);

                float n = Timer > 60 ? 60 : Timer;
                float radius = 20 * MathF.Pow(n, 0.5f);

                if (Timer >= 10 && FargoSoulsUtil.HostCheck)
                {
                    Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center + 0.7f * radius * Projectile.direction * Vector2.UnitX, Vector2.Zero, ModContent.ProjectileType<OOAForceFieldProj>(), 0, 0f, ai0: radius);
                }
            }
            else
            {
                Projectile.Kill();
            }
            Timer++;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Color color = Color.Purple;
            Vector2 pos = Projectile.Center;
            float n = Timer > 60 ? 60 : Timer;
            float timeLerp = MathF.Pow(n, 0.5f);
            float radius = 0 + 20 * timeLerp;
            float arcAngle = MathHelper.PiOver2 - ArcAngle * MathHelper.PiOver2;

            float arcWidth = Timer > 60 ? 0.3f : Timer / 200f;

            var blackTile = TextureAssets.MagicPixel;
            var noise = FargoAssets.SmokyNoise;
            if (!blackTile.IsLoaded || !noise.IsLoaded)
            {
                return false;
            }

            var maxOpacity = 0.25f;
            float fade = 0.5f;
            if (timeLerp > 1 - fade)
            {
                float fadeinLerp = (timeLerp - (1 - fade)) / fade;
                maxOpacity *= 1 - fadeinLerp;
            }

            ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.DestroyerScanTelegraph");
            shader.TrySetParameter("colorMult", 7.35f);
            shader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            shader.TrySetParameter("radius", radius);
            shader.TrySetParameter("arcAngle", arcAngle.ToRotationVector2());
            shader.TrySetParameter("arcWidth", arcWidth);
            shader.TrySetParameter("anchorPoint", pos);
            shader.TrySetParameter("screenPosition", Main.screenPosition);
            shader.TrySetParameter("screenSize", Main.ScreenSize.ToVector2());
            shader.TrySetParameter("maxOpacity", 0.5f);
            shader.TrySetParameter("color", color.ToVector4());


            Main.spriteBatch.GraphicsDevice.Textures[1] = noise.Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, shader.WrappedEffect, Main.GameViewMatrix.TransformationMatrix);
            Rectangle rekt = new(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth, Main.screenHeight);
            Main.spriteBatch.Draw(blackTile.Value, rekt, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    public class OOAForceFieldProj : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles", "Empty");

        public override void SetDefaults()
        {
            Projectile.timeLeft = 2;
            Projectile.width = 25;
            Projectile.height = 50;
            Projectile.hostile = true;
        }

        public override void AI()
        {
            Projectile.height = 50;

            float distance = 3f * 24;

            Main.projectile.Where(x => EModeGlobalProjectile.CanBeAbsorbed(x) && !x.Eternity().isADD2Proj).ToList().ForEach(x =>
            {
                if (Vector2.Distance(x.Center, Projectile.Center) <= distance)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        int dustId = Dust.NewDust(new Vector2(x.position.X, x.position.Y + 2f), x.width, x.height + 5, DustID.PinkTorch, x.velocity.X * 0.2f, x.velocity.Y * 0.2f, 100, default, 1.5f);
                        Main.dust[dustId].noGravity = true;
                    }

                    SoundEngine.PlaySound(SoundID.MaxMana with { MaxInstances = 2 }, Projectile.Center);
                    x.Kill();
                }
            });
        }
    }
}
