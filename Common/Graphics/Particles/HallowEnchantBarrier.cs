using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace FargowiltasSouls.Common.Graphics.Particles
{
    public class HallowEnchantBarrier : Particle
    {
        public override string AtlasTextureName => "FargowiltasSouls.HallowEnchantBarrier";

        public readonly bool UseBloom;

        public readonly float BaseOpacity = 1;
        public override int FrameCount => 4;
        public int CurrentFrame = 0;
        public Player Player;
        public override BlendState BlendState => BlendState.AlphaBlend;
        public HallowEnchantBarrier(Vector2 worldPosition, Vector2 velocity, float scale, int lifetime, float baseOpacity = 1, float rotation = 0f, float rotationSpeed = 0f, Player player = null)
        {
            Position = worldPosition;
            Velocity = velocity;
            DrawColor = Color.White;
            Scale = new(scale);
            Lifetime = lifetime;
            Rotation = rotation;
            RotationSpeed = rotationSpeed;
            UseBloom = false;
            Player = player;

            BaseOpacity = baseOpacity;
        }
        public override void Update()
        {
            if (Player != null && Player.Alive())
                Position = Player.Center;
            if ((int)(LifetimeRatio * Lifetime) % 8 == 0)
            {
                CurrentFrame++;
                CurrentFrame %= FrameCount;
            }
            /*
            if (LifetimeRatio > 0.6f)
            {
                float decay = (LifetimeRatio - 0.6f) / 0.4f;
                Opacity = BaseOpacity * (1 - decay);
                CurrentFrame = (int)(decay * FrameCount);
            }
            else
                CurrentFrame = 0;
            */
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            int height = Texture.Frame.Height / FrameCount;
            Frame = new(0, CurrentFrame * height, Texture.Frame.Width, height - 8);

            Vector2 screenPos = Position - Main.screenPosition;

            spriteBatch.Draw(Texture, screenPos, Frame, DrawColor * Opacity * BaseOpacity, Rotation, null, Scale, SpriteEffects.None);
        }
    }
}
