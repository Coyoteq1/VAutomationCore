#pragma once

#include <algorithm>
#include <array>
#include <cmath>
#include <cstddef>
#include <cstdint>

namespace arpg::ai {

struct Vec2 final {
    float x{0.0f};
    float y{0.0f};

    constexpr Vec2() noexcept = default;
    constexpr Vec2(float xIn, float yIn) noexcept : x(xIn), y(yIn) {}

    [[nodiscard]] constexpr Vec2 operator+(const Vec2& rhs) const noexcept { return {x + rhs.x, y + rhs.y}; }
    [[nodiscard]] constexpr Vec2 operator-(const Vec2& rhs) const noexcept { return {x - rhs.x, y - rhs.y}; }
    [[nodiscard]] constexpr Vec2 operator*(float scalar) const noexcept { return {x * scalar, y * scalar}; }

    constexpr Vec2& operator+=(const Vec2& rhs) noexcept {
        x += rhs.x;
        y += rhs.y;
        return *this;
    }
};

[[nodiscard]] constexpr float dot(const Vec2& lhs, const Vec2& rhs) noexcept { return (lhs.x * rhs.x) + (lhs.y * rhs.y); }
[[nodiscard]] inline float lengthSq(const Vec2& value) noexcept { return dot(value, value); }
[[nodiscard]] inline float length(const Vec2& value) noexcept { return std::sqrt(lengthSq(value)); }
[[nodiscard]] inline float distance(const Vec2& lhs, const Vec2& rhs) noexcept { return length(lhs - rhs); }

[[nodiscard]] inline Vec2 normalize(const Vec2& value) noexcept {
    constexpr float epsilon = 1.0e-5f;
    const float valueLength = length(value);
    if (valueLength <= epsilon) {
        return {};
    }
    return value * (1.0f / valueLength);
}

[[nodiscard]] constexpr Vec2 perpendicular(const Vec2& value) noexcept { return {-value.y, value.x}; }
[[nodiscard]] constexpr float clamp01(float value) noexcept { return std::clamp(value, 0.0f, 1.0f); }

struct Cooldown final {
    float remainingSeconds{0.0f};

    [[nodiscard]] constexpr bool ready() const noexcept { return remainingSeconds <= 0.0f; }
    constexpr void start(float durationSeconds) noexcept { remainingSeconds = std::max(0.0f, durationSeconds); }
    constexpr void tick(float dtSeconds) noexcept { remainingSeconds = std::max(0.0f, remainingSeconds - dtSeconds); }
};

enum class BossPhase : std::uint8_t { Aggressive, DefensiveHealing, Enraged };

enum class BossAction : std::uint8_t {
    None,
    Reposition,
    ProjectileSkillshot,
    CastAoe,
    Heal,
    DashEvade,
    TeleportEvade,
    EnragedBurst
};

struct AbilityThreat final {
    Vec2 origin{};
    Vec2 direction{}; // Expected normalized direction.
    float speed{0.0f}; // 0 means instant line effect.
    float width{0.75f}; // Collision radius of the incoming ability.
    float range{10.0f};
    float spawnedSeconds{0.0f}; // Time already elapsed since spawn.
    float maxLifetimeSeconds{1.5f};
    float danger{1.0f}; // 0..1 authoring hint from gameplay scripts.
};

struct PlayerState final {
    Vec2 position{};
    Vec2 velocity{};
    bool isCastingHighImpactAbility{false};
};

struct BossState final {
    Vec2 position{};
    Vec2 velocity{};
    float healthRatio{1.0f}; // 0..1
    float collisionRadius{0.9f};
};

struct BossConfig final {
    // Phase transitions with hysteresis.
    float defensiveEnterHealthRatio{0.45f};
    float defensiveExitHealthRatio{0.62f};
    float enrageEnterHealthRatio{0.20f};
    float phaseMinHoldSeconds{1.5f};

    // Movement ranges.
    float aggressiveCastRange{8.0f};
    float aggressiveChaseRange{5.0f};
    float defensiveBaseRange{10.0f};
    float defensiveLowHealthBonusRange{3.0f};
    float enragedRange{3.8f};
    float minSpacingRange{2.5f};
    float maxSpacingRange{14.0f};
    float maxRepositionStep{2.0f};

    // Cooldowns.
    float projectileCooldownSeconds{2.6f};
    float aoeCooldownSeconds{7.5f};
    float healCooldownSeconds{14.0f};
    float dashCooldownSeconds{5.0f};
    float teleportCooldownSeconds{10.0f};
    float enragedBurstCooldownSeconds{6.5f};

    // Projectile / AOE params.
    float projectileSpeed{18.0f};
    float projectileMaxLeadSeconds{1.2f};
    float projectileOptimalRange{8.0f};
    float projectileMaxRange{13.5f};
    float aoeCastDelaySeconds{0.75f};
    float aoeOptimalRange{9.0f};
    float aoeMaxRange{13.0f};
    float aoeStrafeCompensation{1.2f};
    float playerMaxSpeedForScoring{8.0f};

    // Heal behaviour.
    float healStartHealthRatio{0.40f};
    float healCriticalHealthRatio{0.18f};

    // Reactive dodge.
    float evadeReactionWindowSeconds{0.45f};
    float evadeSafetyMargin{0.30f};
    float teleportDangerThreshold{0.75f};
    float dashDistance{4.5f};
    float teleportDistance{7.0f};
    float evadeLateralOffset{1.4f};

    // Misc.
    float actionScoreThreshold{0.12f};
    float enrageBurstRange{4.0f};
};

struct ActionCommand final {
    BossAction action{BossAction::None};
    BossPhase phase{BossPhase::Aggressive};
    float utilityScore{0.0f};
    Vec2 moveTarget{};
    Vec2 aimPoint{};
};

class BossUtilityAI final {
public:
    explicit BossUtilityAI(BossConfig config = {}) noexcept;

    [[nodiscard]] ActionCommand update(
        float dtSeconds,
        const BossState& boss,
        const PlayerState& player,
        const AbilityThreat* threats,
        std::size_t threatCount) noexcept;

    // Call this once your gameplay layer confirms execution of the selected action.
    void onActionCommitted(BossAction action) noexcept;

    [[nodiscard]] BossPhase phase() const noexcept { return phase_; }
    [[nodiscard]] const BossConfig& config() const noexcept { return config_; }

private:
    struct ThreatResult final {
        bool shouldEvade{false};
        bool preferTeleport{false};
        float dangerScore{0.0f};
        Vec2 evadeTarget{};
    };

    struct LeadSolution final {
        bool valid{false};
        float interceptTime{0.0f};
        Vec2 aimPoint{};
    };

    struct Candidate final {
        BossAction action{BossAction::None};
        float score{0.0f};
        Vec2 moveTarget{};
        Vec2 aimPoint{};
    };

    void tickCooldowns(float dtSeconds) noexcept;
    void updatePhase(float dtSeconds, float healthRatio) noexcept;

    [[nodiscard]] ThreatResult evaluateThreats(
        const BossState& boss,
        const PlayerState& player,
        const AbilityThreat* threats,
        std::size_t threatCount) const noexcept;

    [[nodiscard]] float desiredSpacing(const BossState& boss) const noexcept;
    [[nodiscard]] LeadSolution solveIntercept(
        const Vec2& shooterPos,
        float projectileSpeed,
        const Vec2& targetPos,
        const Vec2& targetVelocity,
        float maxLeadSeconds) const noexcept;

    [[nodiscard]] Vec2 predictAoePoint(const BossState& boss, const PlayerState& player) const noexcept;
    [[nodiscard]] float scoreReposition(float currentDistance, float desiredDistance) const noexcept;
    [[nodiscard]] float scoreProjectile(
        const BossState& boss,
        const PlayerState& player,
        float targetDistance,
        const LeadSolution& lead) const noexcept;

    [[nodiscard]] float scoreAoe(const BossState& boss, const PlayerState& player, float targetDistance) const noexcept;
    [[nodiscard]] float scoreHeal(const BossState& boss, float targetDistance) const noexcept;
    [[nodiscard]] float scoreEnragedBurst(const BossState& boss, float targetDistance) const noexcept;

    static constexpr std::size_t kCandidateCount = 6;

    BossConfig config_{};
    BossPhase phase_{BossPhase::Aggressive};
    bool enrageTriggered_{false};
    float phaseHoldTimer_{0.0f};
    Vec2 lastPlayerVelocity_{};

    Cooldown projectileCooldown_{};
    Cooldown aoeCooldown_{};
    Cooldown healCooldown_{};
    Cooldown dashCooldown_{};
    Cooldown teleportCooldown_{};
    Cooldown enragedBurstCooldown_{};

    std::array<Candidate, kCandidateCount> candidates_{};
};

} // namespace arpg::ai
