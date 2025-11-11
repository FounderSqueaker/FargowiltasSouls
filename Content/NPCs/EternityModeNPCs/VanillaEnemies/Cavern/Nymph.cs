using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Cavern;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Cavern
{
    public class Nymph : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(NPCID.LostGirl, NPCID.Nymph);

        public int Counter;

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lavaImmune = true;

            if (Main.hardMode)
            {
                npc.lifeMax *= 4;
                npc.damage *= 2;
                npc.defense *= 2;
            }
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            if (npc.type == NPCID.Nymph)
            {
                EModeGlobalNPC.Aura(npc, 250, ModContent.BuffType<HexedBuff>(), true, DustID.PinkTorch);

                if (--Counter < 0)
                {
                    Counter = 300;

                    if (FargoSoulsUtil.HostCheck && npc.HasPlayerTarget && npc.Distance(Main.player[npc.target].Center) < 1000)
                    {
                        Vector2 spawnVel = npc.DirectionFrom(Main.player[npc.target].Center) * 10f;
                        for (int i = -3; i < 3; i++)
                        {
                            Projectile.NewProjectile(npc.GetSource_FromThis(),
                                  npc.Center, spawnVel.RotatedBy(Math.PI / 7 * i),
                                  ModContent.ProjectileType<FakeHeart2>(),
                                  20, 0f, Main.myPlayer, 30, 90 + 10 * i, ai2: npc.target);
                        }
                    }
                }
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);
            npc.life += hurtInfo.Damage * 2;
            if (npc.life > npc.lifeMax)
                npc.life = npc.lifeMax;
            CombatText.NewText(npc.Hitbox, CombatText.HealLife, hurtInfo.Damage * 2);
            npc.netUpdate = true;
        }
    }
}
