using System;
using System.Collections.Generic;
using System.Linq;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Content.Buffs;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Accessories.DubiousCircuitry;
using FargowiltasSouls.Content.Projectiles.Deathrays;
using FargowiltasSouls.Content.Projectiles.Eternity.Environment;
using FargowiltasSouls.Content.Projectiles.Weapons.Minions;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Eternity
{
    [LegacyName("GroundStick")]
    public class RemoteControl : SoulsItem
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Items/Accessories/Eternity", Name);
        public override bool Eternity => true;

        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override List<AccessoryEffect> ActiveSkillTooltips =>
            [AccessoryEffectLoader.GetEffect<RemoteLightningEffect>()];
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(0, 4);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[BuffID.Electrified] = true;
            player.buffImmune[ModContent.BuffType<LightningRodBuff>()] = true;
            player.AddEffect<RemoteControlDR>(Item);
            player.AddEffect<ProbeMinionEffect>(Item);
            player.AddEffect<RemoteLightningEffect>(Item);
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Ranged;
            tooltipColor = null;
            scaling = null;
            return RemoteLightningEffect.BaseDamage(Main.LocalPlayer);
        }
    }
    public class RemoteControlDR : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<DubiousHeader>();
        public override int ToggleItemType => ModContent.ItemType<RemoteControl>();
        private static readonly int[] ElectricAttacks =
        [
            ProjectileID.DeathLaser,
            ProjectileID.EyeLaser,
            ProjectileID.PinkLaser,
            ProjectileID.EyeBeam,
            ProjectileID.MartianTurretBolt,
            ProjectileID.BrainScramblerBolt,
            ProjectileID.GigaZapperSpear,
            ProjectileID.RayGunnerLaser,
            ProjectileID.SaucerLaser,
            ProjectileID.NebulaLaser,
            ProjectileID.VortexVortexLightning,
            ProjectileID.DD2LightningBugZap,
            ProjectileID.EyeBeam,
            ModContent.ProjectileType<RainExplosion>()
        ];
        public static bool ElectricAttack(Projectile projectile, Player player)
        {
            if (player.HasBuff<SuperchargedBuff>() || NPC.AnyNPCs(ModContent.NPCType<MutantBoss>()))
                return false;
            if (projectile.ModProjectile == null)
            {
                if (projectile.aiStyle == ProjAIStyleID.MartianDeathRay || projectile.aiStyle == ProjAIStyleID.ThickLaser || projectile.aiStyle == ProjAIStyleID.LightningOrb || ElectricAttacks.Contains(projectile.type))
                    return true;
            }
            else if (projectile.ModProjectile is BaseDeathray || ElectricAttacks.Contains(projectile.type))
            {
                return true;
            }
            else
            {
                string name = projectile.ModProjectile.Name.ToLower();
                if (name.Contains("lightning") || name.Contains("electr") || name.Contains("thunder") || name.Contains("laser") || name.Contains("zap") || name.Contains("beam"))
                    return true;
            }
            return false;
        }
        public override void ModifyHitByProjectile(Player player, Projectile projectile, ref Player.HurtModifiers modifiers)
        {
            float dr = 0;
            if (EffectItem(player).type == ModContent.ItemType<RemoteControl>() && ElectricAttack(projectile, player) && player.whoAmI == Main.myPlayer)
                dr = 0.5f;
            modifiers.FinalDamage *= 1 - dr;
        }
        public override void OnHitByProjectile(Player player, Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (ElectricAttack(proj, player) && player.whoAmI == Main.myPlayer)
            {
                int duration = hurtInfo.Damage * 30;
                if (duration > 3600)
                    duration = 3600;
                if (duration < 600)
                    duration = 600;
                if (EffectItem(player).type != ModContent.ItemType<RemoteControl>())
                    duration /= 2;
                player.AddBuff(ModContent.BuffType<SuperchargedBuff>(), duration);
                SoundEngine.PlaySound(SoundID.Thunder, player.Center);
                for (int i = 0; i < duration / 80; i++)
                {
                    Vector2 dir = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) - player.velocity / 10;
                    Vector2 spd = Main.rand.NextVector2Circular(15, 15);
                    Particle p = new ElectricSpark(player.Center + dir * 10, spd, Color.Teal, Main.rand.NextFloat(2f, 4f), 120);
                    p.Spawn();
                }
            }
        }
    }
    public class ProbeMinionEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<DubiousHeader>();
        public override int ToggleItemType => ModContent.ItemType<RemoteControl>();
        public override bool MinionEffect => true;
        public override void PostUpdateEquips(Player player)
        {
            if (player.whoAmI == Main.myPlayer && player.HasEffect<ProbeMinionEffect>() && player.FargoSouls().Supercharged)
            {
                player.FargoSouls().Probes = true;
                const int damage = 20;
                const int max = 3;
                float rotation = 2f * (float)Math.PI / max;
                if (player.ownedProjectileCounts[ModContent.ProjectileType<RemoteProbe>()] < 3)
                {
                    for (int i = 0; i < max; i++)
                    {
                        Vector2 spawnPos = player.Center + new Vector2(60, 0f).RotatedBy(rotation * i);
                        FargoSoulsUtil.NewSummonProjectile(GetSource_EffectItem(player), spawnPos, Vector2.Zero, ModContent.ProjectileType<RemoteProbe>(), damage, 10f, player.whoAmI, rotation * i);
                    }
                }
            }
            else player.FargoSouls().Probes = false;
        }
    }
    public class RemoteLightningEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<DubiousHeader>();
        public override int ToggleItemType => ModContent.ItemType<RemoteControl>();
        public override bool ActiveSkill => Main.LocalPlayer.HasEffect<RemoteLightningEffect>();
        public static int BaseDamage (Player player) => (int)(800 * player.ActualClassDamage(DamageClass.Ranged));
        public override void PostUpdateEquips(Player player)
        {
            player.IncrementCooldownTowards<RemoteLightningEffect>(-1, 0);
        }
        public override void ActiveSkillJustPressed(Player player, bool stunned)
        {
            if (player.GetCooldown<RemoteLightningEffect>() > 0 || stunned)
                return;
            if (Main.myPlayer == player.whoAmI && FargoSoulsUtil.HostCheck)
            {
                Vector2 pos = new(Main.MouseWorld.ToTileCoordinates().X * 16 + 8, player.Center.Y - 575);
                float angle = MathHelper.Pi * 0.7f;
                Projectile.NewProjectile(GetSource_EffectItem(player), pos, Vector2.Zero, ModContent.ProjectileType<RemoteScanTelegraph>(), BaseDamage(player), 0f, Main.myPlayer, 0, angle, 1000);
            }
            player.SetCooldown<RemoteLightningEffect>(720);
            CooldownBarManager.Activate("RemoteCD", FargoAssets.GetTexture2D("Content/Items/Accessories/Eternity", "RemoteControl").Value, Color.Lerp(Color.Gray, Color.DarkOliveGreen, 0.25f), () => Main.LocalPlayer.GetCooldown<RemoteLightningEffect>() / 720f, activeFunction: null);
        }
    }
}