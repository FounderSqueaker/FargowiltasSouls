using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Weapons.SwarmDrops;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.Minions
{
    public class DestroyerHeadWhip : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_134";

        public float modifier;
        private int syncTimer;
        private Vector2 mousePos;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Destroyer Head");
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60 * 5;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;
            Projectile.netImportant = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.FargoSouls().CanSplit = false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
            writer.Write(modifier);

            writer.Write(mousePos.X);
            writer.Write(mousePos.Y);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
            modifier = reader.ReadSingle();

            Vector2 buffer;
            buffer.X = reader.ReadSingle();
            buffer.Y = reader.ReadSingle();
            if (Projectile.owner != Main.myPlayer)
            {
                mousePos = buffer;
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return lightColor;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glow = FargoAssets.GetTexture2D("Content/Projectiles/Weapons/Minions", "DestroyerHead2_Glow").Value;
            int num214 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y6 = num214 * Projectile.frame;
            Color color25 = Projectile.GetAlpha(Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)));
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle(0, y6, texture2D13.Width, num214),
                color25, Projectile.rotation, new Vector2(texture2D13.Width / 2f, num214 / 2f), Projectile.scale,
                Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle(0, y6, texture2D13.Width, num214),
                Projectile.GetAlpha(Color.White), Projectile.rotation, new Vector2(texture2D13.Width / 2f, num214 / 2f), Projectile.scale,
                Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.HeldItem == null || player.HeldItem.type != ModContent.ItemType<TheDestroyer>())
            {
                Projectile.Kill();
                return;
            }

            if (player.whoAmI == Main.myPlayer)
            {
                mousePos = Main.MouseWorld;

                if (++syncTimer > 20)
                {
                    syncTimer = 0;
                    Projectile.netUpdate = true;
                }
            }

            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                float modifier = 15;

                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.netUpdate = true;

                    int current = Projectile.whoAmI;
                    for (int i = 0; i <= modifier * 3; i++)
                        current = FargoSoulsUtil.NewSummonProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<DestroyerBodyWhip>(), Projectile.originalDamage, Projectile.knockBack, Projectile.owner, Main.projectile[current].identity);
                    int previous = current;
                    current = FargoSoulsUtil.NewSummonProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<DestroyerTailWhip>(), Projectile.originalDamage, Projectile.knockBack, Projectile.owner, Main.projectile[current].identity);
                    Main.projectile[previous].localAI[1] = Main.projectile[current].identity;
                    Main.projectile[previous].netUpdate = true;
                }
            }

            //keep the head looking right
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57079637f;
            Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;

            const int aislotHomingCooldown = 0;
            const int homingDelay = 10;
            float desiredFlySpeedInPixelsPerFrame = 40 + modifier * 4;

            Projectile.ai[aislotHomingCooldown]++;
            if (Projectile.ai[aislotHomingCooldown] > homingDelay)
            {
                Projectile.ai[aislotHomingCooldown] = homingDelay; //cap this value 

                if (Projectile.Distance(mousePos) > 16 * 5)
                {
                    Vector2 desiredVelocity = Projectile.SafeDirectionTo(mousePos) * desiredFlySpeedInPixelsPerFrame;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.04f);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<LightningRodBuff>(), Main.rand.Next(300, 1200));
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, -Projectile.velocity.X * 0.2f,
                    -Projectile.velocity.Y * 0.2f, 100, default, 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 2f;
                dust = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.RedTorch, -Projectile.velocity.X * 0.2f,
                    -Projectile.velocity.Y * 0.2f, 100);
                Main.dust[dust].velocity *= 2f;
            }
            SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);
        }
    }
}