using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.NetModules;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.CustomEnemies.OOA
{
    public class BowlingSpit : ModNPC
    {
        public override string Texture => "Terraria/Images/Projectile_676";

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            NPCID.Sets.ImmuneToAllBuffs[Type] = true;
            NPCID.Sets.BelongsToInvasionOldOnesArmy[Type] = true;
            this.ExcludeFromBestiary();
        }
        public override void SetDefaults()
        {
            NPC.width = 20;
            NPC.height = 20;
            NPC.scale *= 2;
            NPC.lifeMax = 750;
            NPC.defense = 10;
            NPC.damage = 100;
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit9;
            NPC.DeathSound = SoundID.NPCDeath12;
        }

        public ref float timer => ref NPC.ai[0];
        public ref float state => ref NPC.ai[1];

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }

        private int GetGoreType()
        {
            return Main.rand.NextFromList(GoreID.OgreSpit1, GoreID.OgreSpit2, GoreID.OgreSpit3);
        }

        public override void AI()
        {

            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.JungleGrass, Scale: 1);
                d.noGravity = true;
            }


            NPC.direction = (int)(NPC.velocity.X / Math.Abs(NPC.velocity.X));
            if (NPC.velocity.Y != 0)
            {
                NPC.velocity.Y += 0.4f;
            }
            else if (state == 0)
            {
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                FargoSoulsUtil.ScreenshakeRumble(2);
                for (int i = 0; i < 10; i++)
                {
                    int type = Main.rand.NextFromList(GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3);
                    Gore.NewGore(NPC.GetSource_FromThis(), NPC.BottomLeft + Main.rand.NextFloat(-75, 75) * Vector2.UnitX, -1 * Vector2.UnitY, type);
                }
                for (int i = 0; i < 20; i++)
                {
                    Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), GetGoreType());
                }
                state = 1;
            }
            else
            {
                NPC.velocity.X += NPC.direction * 0.002f;

                // eat people!
                foreach (NPC n in Main.npc.Where(x => x.active && EModeDD2GlobalNPC.IsInstance(x)))
                {
                    if (n.EModeDD2().SpitBall == -1 && !n.noGravity && !EModeDD2Event.IsDD2Boss(n.type) && Collision.CheckAABBvAABBCollision(n.position, new(n.width, n.height), NPC.position, new(NPC.width, NPC.height)))
                    {
                        SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);
                        NPC.scale += 0.2f;
                        NPC.width = (int)(20 * NPC.scale);
                        NPC.height = (int)(20 * NPC.scale);
                        NPC.Center = NPC.Center - 2 * NPC.scale * Vector2.UnitY;
                        NPC.velocity.X *= 0.85f;
                        int healVal = (int)(NPC.lifeMax * 0.1f);
                        NPC.lifeMax += healVal;

                        if (FargoSoulsUtil.HostCheck)
                        {
                            NPC.HealEffect(healVal);
                            NPC.life += healVal;
                        }
                        n.EModeDD2().SpitBall = NPC.whoAmI;

                        FargoSoulsUtil.DustRing(NPC.Center, 20, DustID.JungleTorch, NPC.scale);
                        NPC.netUpdate = true;
                    }
                }
            }

            NPC.rotation += NPC.velocity.X / 10f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit)
        {
            if (target.type == NPCID.DD2EterniaCrystal)
            {
                NPC.life = 0;
                OnKill();
            }
            base.OnHitNPC(target, hit);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), GetGoreType());
            Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), GetGoreType());
            base.HitEffect(hit);
        }

        public override void OnKill()
        {
            for (int i = 0; i < 40; i++)
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.JungleGrass, Scale: 3);
                d.noGravity = true;
                d.velocity *= 3;
            }
            for (int i = 0; i < 20; i++)
            {
                int type = Main.rand.NextFromList(GoreID.OgreSpit1, GoreID.OgreSpit2, GoreID.OgreSpit3);
                Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, 3 * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), type);
            }

            foreach (NPC n in Main.npc.Where(x => x.active && EModeDD2GlobalNPC.IsInstance(x) && x.EModeDD2().SpitBall == NPC.whoAmI))
            {
                n.velocity.Y = -3f;
                n.velocity.X = Main.rand.NextFloat(-3, 3);
                n.netUpdate = true;
            }
            base.OnKill();
        }
    }
}
