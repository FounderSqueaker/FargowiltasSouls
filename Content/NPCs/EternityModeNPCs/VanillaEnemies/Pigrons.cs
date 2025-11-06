using System;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Ocean;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies
{
    public class Pigrons : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.PigronCorruption,
            NPCID.PigronCrimson,
            NPCID.PigronHallow
        );
        private int SpinTimer;
        private float realrotation;
        public override bool SafePreAI(NPC npc)
        {
            if (npc.Distance(Main.player[npc.target].Center) < 60 * 16)
                SpinTimer++;
            if (SpinTimer >= 420)
                SpinTimer = 0;
            if (SpinTimer > 300)
            {
                if (SpinTimer < 302)
                {
                    SoundEngine.PlaySound(SoundID.Zombie20, npc.Center);
                    npc.velocity = new Vector2(10 * npc.direction, 0);
                }
                float dukerotation = (float)(Math.PI * 2f / (120 / 2));
                npc.velocity = npc.velocity.RotatedBy((0 - dukerotation) * npc.direction);
                npc.knockBackResist = 0.1f;
                npc.noTileCollide = true;
                realrotation -= dukerotation * npc.direction;
                npc.rotation = realrotation;
                if (FargoSoulsUtil.HostCheck && SpinTimer % 10 == 0)
                {
                    Vector2 spawn = Vector2.Normalize(npc.velocity) * (npc.width + 20) / 2f + npc.Center;
                    Vector2 vel = Vector2.Zero + Main.rand.NextVector2Circular(5, 5);
                    Projectile.NewProjectile(npc.GetSource_FromThis(), spawn, vel, ModContent.ProjectileType<PigronBubble>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.6f), 0f, Main.myPlayer, -10f, npc.target);
                }
                return false;
            }
            else
            {
                npc.knockBackResist = npc.FargoSouls().defKnockBackResist;
                realrotation = npc.rotation;
                return base.SafePreAI(npc);
            }
        }
        public override bool? CanFallThroughPlatforms(NPC npc) => true;
        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            //target.AddBuff(ModContent.BuffType<SqueakyToyBuff>(), 120);
            //target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 600);
            target.FargoSouls().MaxLifeReduction += 30;
            target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 15 * 60);
        }
    }
}
