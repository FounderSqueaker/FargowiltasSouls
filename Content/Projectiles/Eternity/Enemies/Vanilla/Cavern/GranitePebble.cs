using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Cavern
{
    public class GranitePebble : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Eternity/Enemies/Vanilla/Cavern", Name);
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
        }

        public ref float Owner => ref Projectile.ai[0];
        public ref float Rotation => ref Projectile.ai[1];
        public ref float Index => ref Projectile.ai[2];

        public int State = 0;
        public int Timer = 0;

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            writer.Write(State);
            writer.Write(Timer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            State = reader.ReadInt32();
            Timer = reader.ReadInt32();
        }

        public override void AI()
        {
            NPC npc = Main.npc[(int)Owner];
            if (!npc.active)
            {
                Projectile.Kill();
                return;
            }

            // Shielding
            if (State == 0)
            {
                Rotation += 0.1f;
                Projectile.Center = npc.Center -  45 * Vector2.UnitY.RotatedBy(Rotation);
                Projectile.rotation += 0.1f;

                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, Projectile.velocity * 0.3f, Scale: 0.75f * Projectile.scale);
                d.noGravity = true;

                if (npc.ai[2] >= 0)
                {

                    if (!npc.HasValidTarget)
                    {
                        Projectile.Kill();
                        return;
                    }

                    State = 1;
                }
            }
            // Firing
            else if (State == 1)
            {
                if (npc.ai[2] / 8 >= Index)
                {
                    Player target = Main.player[npc.target];
                    float rot = (target.Center - Projectile.Center).ToRotation();
                    rot += Main.rand.NextFloat(-0.15f, 0.15f);
                    Projectile.velocity = 1f * Vector2.UnitX.RotatedBy(rot);
                    SoundEngine.PlaySound(SoundID.Item91, npc.Center);

                    for (int i = 0; i < 5; i++)
                    {
                        float sparkRot = Main.rand.NextFloat(0, MathHelper.TwoPi);


                        new SparkParticle(Projectile.Center + 5 * Vector2.UnitX.RotatedBy(sparkRot), 3 * Vector2.UnitX.RotatedBy(sparkRot), Color.Lerp(Color.SkyBlue, Color.Blue, 0.8f), 0.3f, 15).Spawn();
                    }

                    State = 2;
                    Projectile.netUpdate = true;
                }
                Projectile.rotation += 0.15f;
            }
            // Traveling
            else
            {
                if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
                    Projectile.tileCollide = true;
                Projectile.rotation += 0.15f;
                Projectile.velocity *= 1.044f;

                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, Projectile.velocity * 0.5f, Scale: 2f * Projectile.scale);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, TorchID.Blue);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            for (int i = 0; i < 4; i++)
            {
                Dust.NewDust(Projectile.Center, 2, 2, DustID.Granite);
            }
            base.OnKill(timeLeft);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Rectangle frame = new(0, 0, texture.Width, texture.Height);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Rotation + Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
