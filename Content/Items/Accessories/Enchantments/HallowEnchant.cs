using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Luminance.Core.Graphics;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class HallowEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override Color nameColor => new(150, 133, 100);


        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.LightPurple;
            Item.value = 180000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<HallowEffect>(Item);
        }


        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("FargowiltasSouls:AnyHallowHead")
                .AddIngredient(ItemID.HallowedPlateMail)
                .AddIngredient(ItemID.HallowedGreaves)
                .AddIngredient(ItemID.HallowJoustingLance)
                .AddIngredient(ItemID.Gungnir)
                .AddIngredient(ItemID.HolyWater, 50)

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }

        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Magic;
            tooltipColor = null;
            scaling = null;
            return HallowEffect.BaseDamage(Main.LocalPlayer);
        }
    }
    public class HallowEffect : AccessoryEffect
    {

        public override Header ToggleHeader => Header.GetHeader<SpiritHeader>();
        public override int ToggleItemType => ModContent.ItemType<HallowEnchant>();

        public const int RepelRadius = 350;
        public static void HealRepel(Player player)
        {
            Item effectItem = player.EffectItem<HallowEffect>();
            if (!player.HasEffectEnchant<HallowEffect>())
                return;
            int duration = player.ForceEffect<HallowEffect>() ? 120 : 60;
            player.FargoSouls().HallowRepelTime = duration;

            SoundEngine.PlaySound(SoundID.Item72);
            Particle p = new HallowEnchantBarrier(player.Center, Vector2.Zero, RepelRadius / 160f, duration + 20, player: player, baseOpacity: 0.25f);
            p.Spawn();

        }
        public static int BaseDamage(Player player)
        {
            int dmg = player.ForceEffect<HallowEffect>() ? 45 : 30;
            return (int)(dmg * player.ActualClassDamage(DamageClass.Magic));
        }
        public override void PostUpdateEquips(Player player)
        {
            if (player.FargoSouls().HallowRepelTime > 0)
            {
                player.FargoSouls().HallowRepelTime--;
                foreach (Projectile projectile in Main.projectile.Where(p => p.hostile && FargoSoulsUtil.CanDeleteProjectile(p) && p.Distance(player.Center) <= RepelRadius * 0.6f))
                {
                    projectile.velocity = Vector2.Normalize(projectile.Center - player.Center) * projectile.velocity.Length();
                    projectile.hostile = false;
                }

                int buff = BuffID.OnFire3;
                float distance = RepelRadius;

                if (player.whoAmI == Main.myPlayer)
                {
                    foreach (var npc in Main.ActiveNPCs)
                    {
                        if (npc.active && !npc.friendly && !npc.dontTakeDamage && !(npc.damage == 0 && npc.lifeMax == 5)) //critters
                        {
                            if (Vector2.Distance(player.Center, FargoSoulsUtil.ClosestPointInHitbox(npc.Hitbox, player.Center)) <= distance)
                            {
                                int dmgRate = 15;

                                if (npc.FindBuffIndex(buff) == -1)
                                    npc.AddBuff(buff, 120);

                                if (player.FargoSouls().HallowRepelTime % dmgRate == 0)
                                    player.ApplyDamageToNPC(npc, BaseDamage(player), 0f, 0, false);


                                int moltenDebuff = ModContent.BuffType<SmiteBuff>();
                                if (npc.FindBuffIndex(moltenDebuff) == -1)
                                    npc.AddBuff(moltenDebuff, 60 * 6);


                            }
                        }
                    }
                }
            }
        }
    }
}
