using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Railgun.Projectiles {

    static internal class Utils {
        static public void KillDust(Vector2 position, Vector2 velocity, int dustId) {
            velocity.Normalize();
            SoundEngine.PlaySound(SoundID.Dig, position);
            for (int i = 0; i < 10; i++) {
                var _dustId = Dust.NewDust(position, 4, 4, dustId);
                Main.dust[_dustId].noGravity = true;
                Dust dust = Main.dust[_dustId];
                dust.velocity -= velocity * 5f;
            }
        }

        static public void ProjectileTrail(Vector2 position, Vector2 velocity) {
            Dust.NewDustPerfect(position, 113, Vector2.Zero);
            Dust dust = Dust.NewDustPerfect(position + velocity, DustID.Electric,
                -velocity.RotatedByRandom(MathHelper.Pi / 32) * Main.rand.NextFloat(2f),
                Scale: .75f
            );
            dust.noGravity = true;
        }

    }

    public class RailgunProjectilePlatinum : ModProjectile {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PlatinumCoin}";

        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.PlatinumCoin);
            Projectile.extraUpdates = 10;
            Projectile.penetrate = 20;

            AIType = ProjectileID.PlatinumCoin;
        }

        public override void AI() {
            Utils.ProjectileTrail(Projectile.oldPosition, Projectile.velocity);
            base.AI();
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Utils.KillDust(Projectile.position, Projectile.velocity, DustID.Silver);
            return base.OnTileCollide(oldVelocity);
        }
    }
    public class RailgunProjectileGold : ModProjectile {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.GoldCoin}";

        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.GoldCoin);
            Projectile.extraUpdates = 10;
            Projectile.penetrate = 20;

            AIType = ProjectileID.GoldCoin;
        }

        public override void AI() {
            Utils.ProjectileTrail(Projectile.oldPosition, Projectile.velocity);
            base.AI();
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Utils.KillDust(Projectile.position, Projectile.velocity, DustID.Sunflower);
            return base.OnTileCollide(oldVelocity);
        }
    }
    public class RailgunProjectileSilver : ModProjectile {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.SilverCoin}";

        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.SilverCoin);
            Projectile.extraUpdates = 10;
            Projectile.penetrate = 20;

            AIType = ProjectileID.SilverCoin;
        }

        public override void AI() {
            Utils.ProjectileTrail(Projectile.oldPosition, Projectile.velocity);
            base.AI();
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Utils.KillDust(Projectile.position, Projectile.velocity, DustID.Silver);
            return base.OnTileCollide(oldVelocity);
        }
    }
    public class RailgunProjectileCopper : ModProjectile {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CopperCoin}";

        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.CopperCoin);
            Projectile.extraUpdates = 10;
            Projectile.penetrate = 20;

            AIType = ProjectileID.CopperCoin;
        }

        public override void AI() {
            Utils.ProjectileTrail(Projectile.oldPosition, Projectile.velocity);
            base.AI();
        }

        public override bool OnTileCollide(Vector2 oldVelocity) {
            Utils.KillDust(Projectile.position, Projectile.velocity, DustID.Copper);
            return base.OnTileCollide(oldVelocity);
        }
    }
}
