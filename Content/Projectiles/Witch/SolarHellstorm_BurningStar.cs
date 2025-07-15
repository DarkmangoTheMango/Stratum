using ReLogic.Utilities;
using Stratum.Content.Particles;
using Terraria.Audio;

namespace Stratum.Content.Projectiles.Witch;

public class BurningStar : ModProjectile
{
    enum BehaviorMode
    {
        Spawn,
        Attack,
        Death
    }

    BehaviorMode currentMode = BehaviorMode.Spawn;

    int deathAnimationTimer = 0;

    int DeathAnimDuration = 100;
    int DeathHoldDuration = 30;
    int DeathCalmBeforeExplosion = 10;

    float MaxShake = 10f;

    SlotId soundSlot;

    // New fields for projectile spawning
    int attackProjectileSpawnTimer = 0;
    int attackProjectileSpawnInterval = 40; // ticks between spawns

    public override string Texture => Stratum.AssetPath + "/Textures/Noise/TurbulentNoise";

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(480f);
        Projectile.scale = 1f;
        Projectile.timeLeft = 300;

        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        switch (currentMode)
        {
            case BehaviorMode.Spawn:
                EnterAttackPhase();
                break;

            case BehaviorMode.Attack:
                UpdateAttackPhase();
                break;

            case BehaviorMode.Death:
                UpdateDeathPhase();
                break;
        }
    }

    void EnterAttackPhase()
    {
        currentMode = BehaviorMode.Attack;
    }

    void UpdateAttackPhase()
    {
        const int fadeInDuration = 10;
        int elapsed = 300 - Projectile.timeLeft;
        float progress = MathHelper.Clamp(elapsed / (float)fadeInDuration, 0f, 1f);

        Lighting.AddLight(Projectile.Center, new Color(255, 128, 0).ToVector3() * 5f * Projectile.scale);

        SpawnEmberParticles();
        MaintainLoopingSound();

        // Increment the spawn timer
        attackProjectileSpawnTimer++;

        if (attackProjectileSpawnTimer >= (attackProjectileSpawnInterval / 2))
        {
            attackProjectileSpawnTimer = 0;

            Player target2 = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            float projectileSpeed = 30f;

            // Estimate travel time to the player
            float distance2 = Vector2.Distance(Projectile.Center, target2.Center);
            float estimatedTime = distance2 / projectileSpeed * 2;

            // Predict future position
            Vector2 predictedPosition = target2.Center + target2.velocity * estimatedTime;

            // Direction toward predicted position
            Vector2 toFuturePos = predictedPosition - Projectile.Center;
            Vector2 baseDirection = toFuturePos.SafeNormalize(Vector2.UnitY);

            // Add spread by rotating the base direction randomly within ±60 degrees
            float spread = MathHelper.ToRadians(60f);
            float angleOffset = Main.rand.NextFloat(-spread, spread);
            Vector2 finalDirection = baseDirection.RotatedBy(angleOffset);

            Vector2 spawnVelocity = finalDirection * projectileSpeed;

            // Spawn position at edge of sun
            Vector2 spawnOffset = finalDirection * (Projectile.width / 3f);
            Vector2 spawnPosition = Projectile.Center + spawnOffset;

            // Spawn the SolarFlare
            Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                spawnPosition,
                spawnVelocity,
                ModContent.ProjectileType<SolarFlare>(),
                200,
                0f,
                Projectile.owner
            );

            // Burst of ember particles in that direction
            int emberCount = Main.rand.Next(10, 15);
            for (int i = 0; i < emberCount; i++)
            {
                float emberAngle = Main.rand.NextFloat(-0.25f, 0.25f);
                Vector2 emberVelocity = finalDirection.RotatedBy(emberAngle) * Main.rand.NextFloat(6f, 14f);
                float emberScale = Main.rand.NextFloat(2f, 3f);

                ParticleManager.NewParticle<SolarEmber>(spawnPosition, emberVelocity, default, emberScale);
            }
        }

        if (Projectile.timeLeft <= 1)
            TriggerDeath();
    }

    void UpdateDeathPhase()
    {

        deathAnimationTimer++;

        float progress = MathHelper.Clamp(deathAnimationTimer / (float)DeathAnimDuration, 0f, 1f);
        float eased = 0.5f - 0.5f * MathF.Cos(MathHelper.Pi * progress);

        if (deathAnimationTimer <= DeathAnimDuration)
            Projectile.scale = MathHelper.Lerp(1f, 0.2f, eased);
        else
            Projectile.scale = 0.2f;

        Lighting.AddLight(Projectile.Center, new Color(255, 50, 0).ToVector3() * 6f * Projectile.scale);

        MaintainLoopingSound();
        SpawnDeathParticles(progress);

        int totalDeathDuration = DeathAnimDuration + DeathHoldDuration;

        if (deathAnimationTimer >= totalDeathDuration)
            Explode();
    }

    void TriggerDeath()
    {
        if (currentMode != BehaviorMode.Death)
        {
            currentMode = BehaviorMode.Death;
            Projectile.timeLeft = int.MaxValue;
            SoundEngine.PlaySound(new SoundStyle(Stratum.SoundPath + "/Custom/Witch/SolarHellstorm_Collapse"), Projectile.Center);
        }
    }

    void SpawnEmberParticles()
    {
        for (int i = 0; i < 2; i++)
        {
            Vector2 offset = Main.rand.NextVector2Circular(1f, 1f) * 200f * Projectile.scale;
            Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(10f, 20f) * Projectile.scale;
            float scale = Main.rand.NextFloat(2f, 3f) * Projectile.scale;

            ParticleManager.NewParticle<SolarEmber>(Projectile.Center + offset, velocity, default, scale);
        }
    }

    void SpawnDeathParticles(float progress)
    {
        if (progress >= 0.7f && deathAnimationTimer % 10 == 0)
        {
            for (int i = 0; i < 5; i++)
                ParticleManager.NewParticle<StarCollapseFlash>(Projectile.Center, Vector2.Zero, default, 0, 0.15f, Projectile.whoAmI);
        }

        bool shouldSpawn = deathAnimationTimer < DeathAnimDuration + DeathHoldDuration - DeathCalmBeforeExplosion;

        if (!shouldSpawn)
            return;

        int maxParticles = 4;
        int minParticles = 1;
        float spawnProgress = MathF.Pow(progress, 2.5f);
        int count = (int)MathHelper.Lerp(minParticles, maxParticles, spawnProgress);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(160f, 1000f);
            Vector2 position = Projectile.Center + offset;
            Vector2 toCenter = (Projectile.Center - position).SafeNormalize(Vector2.UnitY);
            float pull = MathHelper.Lerp(5f, 20f, spawnProgress);
            Vector2 velocity = toCenter * Main.rand.NextFloat(1f, 2f) * pull;

            ParticleManager.NewParticle<SolarEmber>(position, velocity, default, 1f);
        }
    }

    void Explode()
    {
        ParticleManager.NewParticle<SupernovaFlash>(Projectile.Center, Vector2.Zero, default, 2f).rotation = Main.rand.NextFloat(MathHelper.Pi);
        Projectile.NewProjectileDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Supernova>(), 150, 20, Projectile.owner);

        int flareCount = 30;
        float flareSpeed = 4f;
        float flareRadius = Projectile.width / 2f;
        float firstRingOffset = 100f; // push it forward
        float halfStep = MathHelper.TwoPi / (flareCount * 2f);

        // FIRST RING — slightly ahead
        for (int i = 0; i < flareCount; i++)
        {
            float angle = MathHelper.TwoPi * i / flareCount;
            Vector2 direction = angle.ToRotationVector2();

            Vector2 spawnPosition = Projectile.Center + direction * (flareRadius + firstRingOffset);
            Vector2 velocity = direction * flareSpeed;

            int projIndex = Projectile.NewProjectile(
                Projectile.GetSource_Death(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<SolarFlare>(),
                200,
                0f,
                Projectile.owner
            );

            if (Main.projectile[projIndex].ModProjectile is SolarFlare solarFlare)
                solarFlare.NoHoming = true;
        }

        // SECOND RING — base radius, rotated halfway between
        for (int i = 0; i < flareCount; i++)
        {
            float angle = MathHelper.TwoPi * i / flareCount + halfStep;
            Vector2 direction = angle.ToRotationVector2();

            Vector2 spawnPosition = Projectile.Center + direction * (flareRadius - firstRingOffset);
            Vector2 velocity = direction * flareSpeed;

            int projIndex = Projectile.NewProjectile(
                Projectile.GetSource_Death(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<SolarFlare>(),
                200,
                0f,
                Projectile.owner
            );

            if (Main.projectile[projIndex].ModProjectile is SolarFlare solarFlare)
                solarFlare.NoHoming = true;
        }




        Projectile.Kill();
    }

    void MaintainLoopingSound()
    {
        if (!SoundEngine.TryGetActiveSound(soundSlot, out _))
        {
            var tracker = new ProjectileAudioTracker(Projectile);

            soundSlot = SoundEngine.PlaySound(new SoundStyle(Stratum.SoundPath + "/Custom/Witch/SolarHellstorm_SolarWindsLoop")
            {
                IsLooped = true,
                SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
            }, Projectile.Center, soundInstance =>
            {
                soundInstance.Position = Projectile.Center;
                soundInstance.Pitch = MathHelper.Lerp(0f, 0.5f, 1f - Projectile.scale);
                soundInstance.Volume = MathHelper.Lerp(1f, 0.5f, 1f - Projectile.scale);
                return tracker.IsActiveAndInGame();
            });
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        float shakeStrength = MathHelper.Lerp(0f, MaxShake, 1f - Projectile.scale);
        Vector2 shakeOffset = Main.rand.NextVector2CircularEdge(1f, 1f) * shakeStrength;

        DrawBloom(shakeOffset);

        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, Stratum.BurningStarShader.Value, Main.GameViewMatrix.TransformationMatrix);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + shakeOffset, texture.Bounds, Projectile.GetAlpha(lightColor), Projectile.rotation, texture.Size() * 0.5f, Projectile.scale * 0.94f, SpriteEffects.None, 0);

        Stratum.BurningStarShader.Value.Parameters["uTime"].SetValue(Main.GameUpdateCount / 40f);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        return false;
    }

    void DrawBloom(Vector2 offset)
    {
        Texture2D bloom = ModContent.Request<Texture2D>(Stratum.AssetPath + "/Textures/Bloom").Value;
        Texture2D flare = ModContent.Request<Texture2D>(Stratum.AssetPath + "/Textures/RadialFlare").Value;

        Color tint = new Color(255, 128, 0, 0);

        for (int i = 0; i < 5; i++)
            Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition + offset, bloom.Bounds, Projectile.GetAlpha(tint), Projectile.rotation, bloom.Size() * 0.5f, 0.9f * Projectile.scale, SpriteEffects.None, 0);

        float flareScale = 1.8f * Projectile.scale;
        float rotationSpeed = Main.GameUpdateCount * 0.01f;

        Main.EntitySpriteDraw(flare, Projectile.Center - Main.screenPosition + offset, flare.Bounds, Projectile.GetAlpha(tint), Projectile.rotation + rotationSpeed, flare.Size() * 0.5f, flareScale, SpriteEffects.None, 0);
        Main.EntitySpriteDraw(flare, Projectile.Center - Main.screenPosition + offset, flare.Bounds, Projectile.GetAlpha(tint), Projectile.rotation - rotationSpeed, flare.Size() * 0.5f, flareScale * 0.9f, SpriteEffects.None, 0);
    }
}
