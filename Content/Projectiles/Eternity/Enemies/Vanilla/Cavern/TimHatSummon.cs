using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Cavern
{
    public class TimHatSummon : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_238";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.FargoSouls().DeletionImmuneRank = 1;
        }
        public ref float owner => ref Projectile.ai[0];
        public ref float target => ref Projectile.ai[1];

        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Parent parent && parent.Entity is NPC parentNPC && parentNPC.type == NPCID.Tim)
                owner = parentNPC.whoAmI;
            else
                owner = -1;
            base.OnSpawn(source);
        }

        public override void AI()
        {
            if (owner < 0 || owner > Main.maxNPCs)
                Projectile.Kill();
            NPC tim = Main.npc[(int)owner];
            if (!tim.active)
                Projectile.Kill();

            if (Projectile.ai[2]++ % 20 == 0)
                SoundEngine.PlaySound(SoundID.Item24 with { Volume = 1.5f, PitchVariance = 0.6f }, Projectile.Center);

            if (target == -1)
            {
                float dist = -1;
                foreach (Player p in Main.ActivePlayers)
                {
                    if (p.dead || p.ghost)
                        continue;

                    float pDist = p.Distance(Projectile.Center);
                    if ((dist < 0 || pDist < dist) && pDist < 3000)
                    {
                        target = p.whoAmI;
                        dist = pDist;
                    }
                }
                Projectile.velocity *= 0.9f;
                return;
            }

            Player player = Main.player[(int)target];
            if (!player.active || player.dead || player.ghost)
            {
                target = -1;
                return;
            }

            Projectile.direction = (int)Projectile.HorizontalDirectionTo(player.Center);
            Projectile.spriteDirection = Projectile.direction;
            float rot = (player.Center - Projectile.Center).ToRotation();

            if (Math.Abs(Projectile.velocity.ToRotation() - rot) > 1.2f)
                Projectile.velocity *= 0.93f;

            Projectile.velocity += 0.3f * Vector2.UnitX.RotatedBy(rot);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.White;
            Texture2D text = TextureAssets.Item[ItemID.WizardHat].Value;
            Rectangle frame = text.Frame();
            Vector2 origin2 = frame.Size() / 2;
            SpriteEffects flip = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            int length = Projectile.oldPos.Length;
            for (int i = 0; i < length; i++)
            {
                float opac = Projectile.Opacity * (length-i) / length;
                Main.EntitySpriteDraw(text, Projectile.oldPos[i] - Main.screenPosition + origin2, frame, lightColor * opac * 0.7f, 0, origin2, Projectile.scale, flip);
            }


            Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition, frame, Color.LightGreen, 0, origin2, Projectile.scale * 1.3f, flip);
            Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition, frame, lightColor, 0, origin2, Projectile.scale, flip);
            return false;
        }
    }
}
