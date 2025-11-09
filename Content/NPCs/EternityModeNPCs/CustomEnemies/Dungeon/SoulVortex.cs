using Fargowiltas;
using FargowiltasSouls.Assets.Textures;
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
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.CustomEnemies.Desert
{
    public class SoulVortex : ModNPC
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Eternity/Enemies/Vanilla/Dungeon", Name);
        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[Type] = 15;
            NPCID.Sets.TrailingMode[Type] = 3;
            NPCID.Sets.ImmuneToAllBuffs[Type] = true;
            this.ExcludeFromBestiary();
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            NPC.HideStrikeDamage = true;
            NPC.immortal = true;
            NPC.damage = 70;
            NPC.lifeMax = 1000;
            NPC.width = NPC.height = 40;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.knockBackResist = 0.6f;
            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.DD2_GhastlyGlaiveImpactGhost;
            NPC.scale = 0;
            NPC.friendly = false;
            
            base.SetDefaults();
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> t = TextureAssets.Npc[Type];
            Main.spriteBatch.UseBlendState(BlendState.Additive);

            for(int i = 0; i < NPC.oldPos.Length; i++)
            {
                Main.EntitySpriteDraw(t.Value, NPC.oldPos[i] - Main.screenPosition + NPC.Size/2, null, drawColor * 0.8f * (1 - (float)i / NPC.oldPos.Length), NPC.oldRot[i] * 0.8f, t.Size() / 2, NPC.scale * 1.5f * MathHelper.Lerp(1, 0.5f, (float)i / NPC.oldPos.Length), SpriteEffects.None);
            }
            
            Main.EntitySpriteDraw(t.Value, NPC.Center - Main.screenPosition, null, drawColor * 0.5f, NPC.rotation * 0.8f, t.Size() / 2, NPC.scale * 1.7f, SpriteEffects.None);
            Main.EntitySpriteDraw(t.Value, NPC.Center - Main.screenPosition, null, drawColor * 0.5f, NPC.rotation * 0.9f, t.Size() / 2, NPC.scale * 1.7f, SpriteEffects.None);
            Main.EntitySpriteDraw(t.Value, NPC.Center - Main.screenPosition, null, drawColor * 1, NPC.rotation * 1.5f, t.Size() / 2, NPC.scale * 1.5f, SpriteEffects.None);
            Main.EntitySpriteDraw(t.Value, NPC.Center - Main.screenPosition, null, drawColor * 1, -NPC.rotation * 1.25f, t.Size() / 2, NPC.scale * 1.25f, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(t.Value, NPC.Center - Main.screenPosition, null, drawColor * 1, NPC.rotation, t.Size() / 2, NPC.scale, SpriteEffects.None);
            Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);
            return false;
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (NPC.ai[2] == 0 || projectile.knockBack > NPC.ai[3])
            {
                return null;
            }
            return false;
        }
        public override bool? CanBeHitByItem(Player player, Item item)
        {
            if (NPC.ai[2] == 0 || player.GetWeaponKnockback(item) > NPC.ai[3])
            {
                return null;
            }
            return false;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return NPC.scale >= 1;
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            NPC.ai[2] = 30;
            NPC.ai[3] = projectile.knockBack;
            NPC.velocity = projectile.AngleTo(NPC.Center).ToRotationVector2() * projectile.knockBack * NPC.knockBackResist;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = FargowiltasSouls.Instance.GetPacket();
                packet.Write((byte)FargowiltasSouls.PacketID.SyncSoulVortexHit);
                packet.Write((int)NPC.ai[2]);
                packet.Write(NPC.ai[3]);
                packet.WriteVector2(NPC.velocity);
                packet.Send();
            }else if (Main.netMode == NetmodeID.Server)
            {
                NPC.netUpdate = true;
            }

                base.OnHitByProjectile(projectile, hit, damageDone);
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            NPC.ai[2] = 30;
            NPC.ai[3] = player.GetWeaponKnockback(item);
            NPC.velocity = player.AngleTo(NPC.Center).ToRotationVector2() * player.GetWeaponKnockback(item) * NPC.knockBackResist;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = FargowiltasSouls.Instance.GetPacket();
                packet.Write((byte)FargowiltasSouls.PacketID.SyncSoulVortexHit);
                packet.Write((int)NPC.ai[2]);
                packet.Write(NPC.ai[3]);
                packet.WriteVector2(NPC.velocity);
                packet.Send();
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                NPC.netUpdate = true;
            }
            base.OnHitByItem(player, item, hit, damageDone);
        }
        public override void AI()
        {
            NPC.rotation -= MathHelper.ToRadians(2);
            NPC owner = Main.npc[(int)NPC.ai[0]];
            Lighting.AddLight(NPC.Center, new Vector3(0.3f, 0.35f, 0.4f));
            if (!(owner.active && (owner.type == NPCID.RaggedCaster || owner.type == NPCID.RaggedCasterOpenCoat)))
            {
                NPC.ai[1]--;
                float x = NPC.ai[1] / 120f;
                //ease out quad but reversed because ai[1] goes down so ease in quad
                NPC.scale = MathHelper.Lerp(0, 1, 1 - (1 - x) * (1 - x));
                NPC.rotation -= MathHelper.ToRadians(MathHelper.Lerp(2, 1, NPC.scale));
                int size = (int)(40 * NPC.scale);
                int diff = Math.Abs(NPC.width - size);
                NPC.width = NPC.height = size;
                NPC.Center += new Vector2(diff, diff) / 2;
                if (NPC.scale <= 0 && Main.netMode != NetmodeID.MultiplayerClient) NPC.active = false;

                if (!Main.dedServ)
                {
                    Vector2 pos = NPC.Center + new Vector2(0, Main.rand.NextFloat(30, 60)).RotatedByRandom(MathHelper.TwoPi);
                    Vector2 vel = -pos.DirectionTo(NPC.Center) * 6 + NPC.velocity;
                    Dust.NewDustDirect(NPC.Center, 1, 1, DustID.SpectreStaff, vel.X, vel.Y, 100, Scale: 2).noGravity = true;
                }
            }
            else
            {
                if (owner.HasValidTarget && NPC.ai[1] >= 120)
                {
                    Player target = Main.player[owner.target];
                    NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.AngleTo(target.Center).ToRotationVector2() * 3, 0.04f);
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        if (Main.npc[i].active && i != NPC.whoAmI && Main.npc[i].type == Type && Main.npc[i].Hitbox.Intersects(NPC.Hitbox))
                        {
                            NPC.velocity -= NPC.DirectionTo(Main.npc[i].Center) * 0.4f;
                        }
                    }
                }

                if (NPC.scale < 1)
                {
                    NPC.ai[1]++;
                    float x = NPC.ai[1] / 120f;
                    //ease out quad
                    NPC.scale = MathHelper.Lerp(0, 1, 1 - (1 - x) * (1 - x));
                    NPC.rotation -= MathHelper.ToRadians(MathHelper.Lerp(2, 1, NPC.scale));
                    int size = (int)(40 * NPC.scale);
                    int diff = Math.Abs(NPC.width - size);
                    NPC.width = NPC.height = size;
                    NPC.Center -= new Vector2(diff, diff)/2;

                    if (!Main.dedServ)
                    {
                        Vector2 pos = NPC.Center + new Vector2(0, Main.rand.NextFloat(30, 60)).RotatedByRandom(MathHelper.TwoPi);
                        Vector2 vel = pos.DirectionTo(NPC.Center) * 2;

                        Dust.NewDustDirect(pos, 1, 1, DustID.SpectreStaff, vel.X, vel.Y, 100, Scale: 2).noGravity = true;
                    }
                    
                    if (NPC.ai[1] == 120 && owner.HasValidTarget)
                    {
                        if (!Main.dedServ)
                        SoundEngine.PlaySound(SoundID.DD2_GhastlyGlaiveImpactGhost, NPC.Center);
                        NPC.velocity = NPC.AngleTo(Main.player[owner.target].Center).ToRotationVector2() * 10;
                    }
                }
            }
            

            if (!Main.dedServ)
            {
                NPC.localAI[0] += 0.1f;
                if (NPC.localAI[2] == 0) NPC.localAI[2] = MathHelper.ToRadians(Main.rand.NextFloat(0, 360));
                if (NPC.localAI[3] == 0) NPC.localAI[3] = MathHelper.ToRadians(Main.rand.NextFloat(0, 360));

                Vector2 dustpos = NPC.Center + new Vector2(MathF.Sin(NPC.localAI[0]) * 70, MathF.Cos(NPC.localAI[0]) * 15).RotatedBy(NPC.localAI[2]);
                Dust orbit = Dust.NewDustPerfect(dustpos, DustID.SpectreStaff, Scale: 2);
                orbit.velocity = NPC.velocity;
                orbit.noGravity = true;

                dustpos = NPC.Center + new Vector2(MathF.Cos(NPC.localAI[0]) * 70, MathF.Sin(NPC.localAI[0]) * 15).RotatedBy(NPC.localAI[3]);
                Dust orbit2 = Dust.NewDustPerfect(dustpos, DustID.SpectreStaff, Scale: 2);
                orbit2.velocity = NPC.velocity;
                orbit2.noGravity = true;
            }

            if (NPC.ai[2] > 0) NPC.ai[2]--;

            base.AI();
        }
    }
}
