using FargowiltasSouls.Content.Bosses.TrojanSquirrel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.Champions.Timber
{
    public class TimberHook : TrojanHook
    {
        public override string Texture => "FargowiltasSouls/Content/Bosses/TrojanSquirrel/TrojanHook";

        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.extraUpdates = 2;

            Projectile.FargoSouls().DeletionImmuneRank = 2;
        }

        public override bool? CanDamage() => false;
        protected override bool flashingZapEffect => false;

        //Vector2 oldPos;

        public override void AI()
        {
            NPC npc = FargoSoulsUtil.NPCExists(Projectile.ai[0], ModContent.NPCType<TimberChampion>());
            if (npc == null)
            {
                Projectile.Kill();
                return;
            }

            Projectile.rotation = npc.SafeDirectionTo(Projectile.Center).ToRotation() + MathHelper.PiOver2;

            if (--Projectile.ai[1] > 0)
            {
                if (!Projectile.tileCollide && !Collision.SolidCollision(Projectile.Center, 0, 0))
                    Projectile.tileCollide = true;

                Projectile.velocity = npc.SafeDirectionTo(Main.player[npc.target].Center) * Projectile.velocity.Length();
            }
            else
            {
                Projectile.extraUpdates = 0;
                Projectile.tileCollide = false;
                Projectile.velocity = Vector2.Zero;

                if (Projectile.localAI[0] == 0)
                {
                    //flag to turn off y collision
                    npc.localAI[0] = Math.Sign(Projectile.Center.X - npc.Center.X);

                    Projectile.localAI[0] = 1;

                    Projectile.localAI[1] = npc.SafeDirectionTo(Projectile.Center).ToRotation();
                }

                if (Projectile.Distance(npc.Center) > 600)
                    npc.localAI[0] = Math.Sign(Projectile.Center.X - npc.Center.X);

                if (Math.Abs(MathHelper.WrapAngle(npc.SafeDirectionTo(Main.player[npc.target].Center).ToRotation() - npc.SafeDirectionTo(Projectile.Center).ToRotation())) > MathHelper.PiOver2)
                {
                    Projectile.Kill();
                    return;
                }

                Vector2 tug = 42f * npc.SafeDirectionTo(Projectile.Center);
                float lerp = Math.Min(npc.Distance(Projectile.Center) / 2400, 1f);
                lerp = lerp * 0.8f + 0.2f;
                lerp *= 0.06f;
                npc.velocity = Vector2.Lerp(npc.velocity, tug, lerp);

                if (Projectile.timeLeft > 180)
                    Projectile.timeLeft = 180;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            NPC npc = FargoSoulsUtil.NPCExists(Projectile.ai[0], ModContent.NPCType<TimberChampion>());
            if (npc != null)
            {
                Texture2D texture = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/TrojanSquirrel/TrojanChain", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;             
                Vector2 position = Projectile.Center;
                Vector2 mountedCenter = npc.Center;
                Rectangle? sourceRectangle = new Rectangle?();
                Vector2 origin = new(texture.Width * 0.5f, texture.Height * 0.5f);
                float num1 = texture.Height;
                Vector2 vector24 = mountedCenter - position;
                float rotation = (float)Math.Atan2(vector24.Y, vector24.X) - 1.57f;
                bool flag = true;
                if (float.IsNaN(position.X) && float.IsNaN(position.Y))
                    flag = false;
                if (float.IsNaN(vector24.X) && float.IsNaN(vector24.Y))
                    flag = false;
                while (flag)
                    if (vector24.Length() < num1 + 1.0)
                    {
                        flag = false;
                    }
                    else
                    {
                        Vector2 vector21 = vector24;
                        vector21.Normalize();
                        position += vector21 * num1;
                        vector24 = mountedCenter - position;
                        Color color2 = Projectile.GetAlpha(lightColor);
                        Main.EntitySpriteDraw(texture, position - Main.screenPosition, sourceRectangle, color2, rotation, origin, 1f, SpriteEffects.None, 0);
                    }
            }

            Texture2D texture2D13 = TextureAssets.Projectile[Projectile.type].Value;         
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            SpriteEffects effects = SpriteEffects.None;
            Color color = Projectile.GetAlpha(lightColor);

            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[1] = 0;
            return false;
        }
    }
}