using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class MeteorEnchant : BaseEnchant
    {
        public static readonly Color NameColor = new(95, 71, 82);
        public override Color nameColor => NameColor;


        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Green;
            Item.value = 100000;
        }

        public const int METEOR_ADDED_DURATION = 450;

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            AddEffects(player, Item);
        }
        public static void AddEffects(Player player, Item item)
        {
            player.AddEffect<MeteorMomentumEffect>(item);
            //player.AddEffect<MeteorTrailEffect>(item);
            player.AddEffect<MeteorEffect>(item);
        }

        public override void AddRecipes()
        {
            CreateRecipe()

                .AddIngredient(ItemID.MeteorHelmet)
                .AddIngredient(ItemID.MeteorSuit)
                .AddIngredient(ItemID.MeteorLeggings)
                .AddIngredient(ItemID.StarCannon)
                .AddIngredient(ItemID.Magiluminescence)
                .AddIngredient(ItemID.Starfury)
                .AddCondition(Condition.DownedEowOrBoc)

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Magic;
            tooltipColor = null;
            scaling = null;
            return MeteorEffect.BaseDamage(Main.LocalPlayer);
        }
    }
    public class MeteorMomentumEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<CosmoHeader>();
        public override int ToggleItemType => ModContent.ItemType<MeteorEnchant>();
        public override void PostUpdateEquips(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.hasMagiluminescence = true;
                // magilumi lighting
                DelegateMethods.v3_1 = new Vector3(1.2f, 0.6f, 0.4f);
                Utils.PlotTileLine(player.Center, player.Center + player.velocity * 6f, 20f, DelegateMethods.CastLightOpen);
                Utils.PlotTileLine(player.Left, player.Right, 20f, DelegateMethods.CastLightOpen);

                /*if (!player.FargoSouls().NoMomentum && !player.mount.Active) //overriden by nomomentum
                {
                    player.runAcceleration *= 1.3f;
                    player.runSlowdown *= 1.3f;
                }
                if (player.HasEffect<MeteorTrailEffect>())
                {
                    const int SparkDelay = 2;
                    int Timer = (int)(Main.GlobalTimeWrappedHourly * 60) % 60;
                    if (player.velocity != Vector2.Zero && Timer % SparkDelay == 0)
                    {
                        for (int i = -1; i < 2; i += 2)
                        {
                            Vector2 vel = (-player.velocity).RotatedBy(i * MathHelper.Pi / 7).RotatedByRandom(MathHelper.Pi / 12);
                            int damage = 22;
                            Vector2 pos = player.Center;
                            Vector2 offset = Vector2.Normalize(player.velocity).RotatedBy(MathHelper.PiOver2 * -i) * (player.width / 2);
                            Projectile.NewProjectile(GetSource_EffectItem(player), pos + offset, vel, ModContent.ProjectileType<MeteorFlame>(), FargoSoulsUtil.HighestDamageTypeScaling(player, damage), 0.5f, player.whoAmI);
                        }
                    }
                }
                */
            }
        }
    }
    /*
    public class MeteorTrailEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<CosmoHeader>();
        public override int ToggleItemType => ModContent.ItemType<MeteorEnchant>();
        public override bool ExtraAttackEffect => true;
    }
    */
    public class MeteorEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<CosmoHeader>();
        public override int ToggleItemType => ModContent.ItemType<MeteorEnchant>();
        public override bool ExtraAttackEffect => true;
        public const int Cooldown = 60 * 10;
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (player.whoAmI == Main.myPlayer)
            {
                float denominator = 10f;
                if (modPlayer.ForceEffect<MeteorEnchant>())
                    denominator = 6f;
                modPlayer.MeteorCD -= player.velocity.Length() / denominator;
            }
        }
        public static int BaseDamage(Player player) => (int)((player.FargoSouls().ForceEffect<MeteorEnchant>() ? 160 : 45) * player.ActualClassDamage(DamageClass.Magic));
        public override void OnHitNPCEither(Player player, NPC target, NPC.HitInfo hitInfo, DamageClass damageClass, int baseDamage, Projectile projectile, Item item)
        {
            if (player.whoAmI != Main.myPlayer)
                return;
            FargoSoulsPlayer modPlayer = player.FargoSouls();

            if (modPlayer.MeteorCD > 0)
                return;

            bool forceEffect = modPlayer.ForceEffect<MeteorEnchant>();
            modPlayer.MeteorCD = Cooldown;

            if (player.whoAmI == Main.myPlayer)
                CooldownBarManager.Activate("MeteorEnchantCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Enchantments", "MeteorEnchant").Value, Color.Lerp(MeteorEnchant.NameColor, Color.OrangeRed, 0.75f), 
                
                    () => 1f - Main.LocalPlayer.FargoSouls().MeteorCD / (float)Cooldown, activeFunction: () => player.HasEffect<MeteorEffect>()); 

            Vector2 pos = new(player.Center.X + Main.rand.NextFloat(-1000, 1000), player.Center.Y - 1000);
            Vector2 vel = new(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(8, 12));

            pos.X = target.Center.X + Main.rand.NextFloat(-320, 320);

            //can retarget better at them, but dont aim meteors upwards
            Vector2 predictive = Main.rand.NextFloat(10f, 30f) * target.velocity;
            pos.X += predictive.X;
            Vector2 targetPos = target.Center;
            if (pos.Y < targetPos.Y)
            {
                vel = FargoSoulsUtil.PredictiveAim(pos, targetPos, target.velocity / 3, 12f);
            }
            SoundEngine.PlaySound(FargosSoundRegistry.ThrowShort, pos);

            int force = forceEffect ? 1 : 0;
            int i = Projectile.NewProjectile(GetSource_EffectItem(player), pos, vel, ModContent.ProjectileType<MeteorEnchantMeatball>(), BaseDamage(player), 0.5f, player.whoAmI, force);
        }
    }
}
