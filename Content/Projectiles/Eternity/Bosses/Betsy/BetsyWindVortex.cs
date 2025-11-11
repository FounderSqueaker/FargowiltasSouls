using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Bosses;
using FargowiltasSouls.Content.Buffs.Eternity;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy
{
    public class BetsyWindVortex : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles", "Empty");


        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.tileCollide = false;
            Projectile.FargoSouls().DeletionImmuneRank = 1;
        }

        LoopedSoundInstance windNoise;

        public override void AI()
        {
            Projectile.timeLeft++;

            float progress = 0;

            if (Projectile.ai[1] <= 90) // fade in
            {
                progress = -3 + (Projectile.ai[1] / 30);
            }


            if (Projectile.ai[2] > 0) // fade out
            {
                Projectile.ai[2]--;
                progress = -3 + (Projectile.ai[2] / 30);
                if (Projectile.ai[2] == 1)
                {
                    Projectile.Kill();
                    return;
                }
            }

            if (progress > 0)
                progress = 0;

            windNoise ??= LoopedSoundManager.CreateNew(FargosSoundRegistry.DeviFallLoop, () =>
            {
                return !Projectile.active;
            });

            windNoise?.Update(Projectile.Center, sound =>
            {
                sound.Volume = MathHelper.Lerp(0, 2, 1 + (progress / 3));
            });

            NPC owner = Main.npc[(int)Projectile.ai[0]];
            if (!owner.Alive() || owner.type != NPCID.DD2Betsy || owner.GetGlobalNPC<Content.Bosses.VanillaEternity.Betsy>().State != 7)
            {
                Projectile.Kill();
                return;
            }

            if (!Main.dedServ)
            {
                if (!Filters.Scene["CrystalDestructionVortex"].IsActive())
                {
                    Filters.Scene.Activate("CrystalDestructionVortex", default(Vector2));
                }
                Filters.Scene["CrystalDestructionVortex"].GetShader().UseIntensity(5f).UseProgress(progress)
                    .UseTargetPosition(Projectile.Center);
            }

            if (Projectile.ai[1]++ % 14 == 0)
            {
                //SoundEngine.PlaySound(FargosSoundRegistry.DeviFallLoop, Projectile.Center);
                //SoundEngine.PlaySound(SoundID.Item24, Projectile.Center);
                //SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -1f, Volume = 2f }, Projectile.Center);
            }
            void suck()
            {
                Player p = Main.LocalPlayer;
                if (p.Alive())
                {
                    float dist = p.Distance(Projectile.Center);
                    if (dist > 1000)
                        return;

                    float dragSpeed = dist / 80;
                    if (p.velocity.Y != 0 && !Collision.SolidCollision(p.position + Projectile.DirectionFrom(p.Center).RotatedBy(MathHelper.Pi / 2) * dragSpeed, p.width, p.height))
                        p.position += Projectile.DirectionFrom(p.Center).RotatedBy(MathHelper.Pi / 2) * dragSpeed * 1.5f;
                    p.wingTime++; // inf flight
                    p.position += Projectile.DirectionFrom(p.Center) * dragSpeed / 2f;
                    p.AddBuff(ModContent.BuffType<LowGroundEridanusBuff>(), 2);
                    if (dist > 350)
                        p.position += Projectile.DirectionFrom(p.Center) * dragSpeed / 2f;

                    if (p.velocity.Length() > 15)
                    {
                        p.velocity.Normalize();
                        p.velocity *= 14;
                    }

                    //if (p.velocity.Y <= 0 && dist < 350)
                    //    p.position += Projectile.DirectionFrom(p.Center).RotatedBy(MathHelper.PiOver2) * 3 * dragSpeed;
                }
            }
            suck();

        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
                Filters.Scene.Deactivate("CrystalDestructionVortex");

            base.OnKill(timeLeft);
        }
    }
}
