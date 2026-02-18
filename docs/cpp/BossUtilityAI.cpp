#include "BossUtilityAI.hpp"

#include <cmath>
#include <limits>

namespace arpg::ai {
namespace {

[[nodiscard]] float distanceToSegment(const Vec2& point, const Vec2& a, const Vec2& b, float* outT) noexcept {
    constexpr float epsilon = 1.0e-5f;
    const Vec2 ab = b - a;
    const float abLengthSq = lengthSq(ab);
    if (abLengthSq <= epsilon) {
        if (outT != nullptr) {
            *outT = 0.0f;
        }
        return distance(point, a);
    }

    const float t = std::clamp(dot(point - a, ab) / abLengthSq, 0.0f, 1.0f);
    if (outT != nullptr) {
        *outT = t;
    }
    const Vec2 closest = a + (ab * t);
    return distance(point, closest);
}

[[nodiscard]] Vec2 safeDirection(const Vec2& value, const Vec2& fallback) noexcept {
    constexpr float epsilon = 1.0e-5f;
    if (lengthSq(value) <= epsilon) {
        return fallback;
    }
    return normalize(value);
}

} // namespace

BossUtilityAI::BossUtilityAI(BossConfig config) noexcept : config_(config) {}

void BossUtilityAI::tickCooldowns(float dtSeconds) noexcept {
    projectileCooldown_.tick(dtSeconds);
    aoeCooldown_.tick(dtSeconds);
    healCooldown_.tick(dtSeconds);
    dashCooldown_.tick(dtSeconds);
    teleportCooldown_.tick(dtSeconds);
    enragedBurstCooldown_.tick(dtSeconds);
}

void BossUtilityAI::updatePhase(float dtSeconds, float healthRatio) noexcept {
    phaseHoldTimer_ = std::max(0.0f, phaseHoldTimer_ - dtSeconds);

    if (!enrageTriggered_ && healthRatio <= config_.enrageEnterHealthRatio) {
        phase_ = BossPhase::Enraged;
        enrageTriggered_ = true;
        phaseHoldTimer_ = config_.phaseMinHoldSeconds;
        return;
    }

    if (enrageTriggered_) {
        return;
    }

    if (phaseHoldTimer_ > 0.0f) {
        return;
    }

    BossPhase targetPhase = phase_;
    if (phase_ == BossPhase::Aggressive && healthRatio <= config_.defensiveEnterHealthRatio) {
        targetPhase = BossPhase::DefensiveHealing;
    } else if (phase_ == BossPhase::DefensiveHealing && healthRatio >= config_.defensiveExitHealthRatio) {
        targetPhase = BossPhase::Aggressive;
    }

    if (targetPhase != phase_) {
        phase_ = targetPhase;
        phaseHoldTimer_ = config_.phaseMinHoldSeconds;
    }
}

BossUtilityAI::ThreatResult BossUtilityAI::evaluateThreats(
    const BossState& boss,
    const PlayerState& player,
    const AbilityThreat* threats,
    std::size_t threatCount) const noexcept {
    ThreatResult best{};
    if (threats == nullptr || threatCount == 0U) {
        return best;
    }

    for (std::size_t i = 0; i < threatCount; ++i) {
        const AbilityThreat& threat = threats[i];
        if (threat.range <= 0.0f) {
            continue;
        }

        const Vec2 direction = safeDirection(threat.direction, {1.0f, 0.0f});
        const float traveled = std::clamp(threat.spawnedSeconds * std::max(0.0f, threat.speed), 0.0f, threat.range);
        const Vec2 segmentStart = threat.origin + (direction * traveled);
        const Vec2 segmentEnd = threat.origin + (direction * threat.range);

        float tSegment = 0.0f;
        const float laneDistance = distanceToSegment(boss.position, segmentStart, segmentEnd, &tSegment);
        const float hitRadius = threat.width + boss.collisionRadius + config_.evadeSafetyMargin;
        if (laneDistance > hitRadius) {
            continue;
        }

        const float segmentRemaining = std::max(0.0f, threat.range - traveled);
        const float segmentTravelToClosest = tSegment * segmentRemaining;
        const float timeToImpact = threat.speed > 1.0e-5f ? (segmentTravelToClosest / threat.speed) : 0.0f;
        if (timeToImpact > config_.evadeReactionWindowSeconds) {
            continue;
        }

        const float timeWeight = 1.0f - clamp01(timeToImpact / config_.evadeReactionWindowSeconds);
        const float laneWeight = 1.0f - clamp01(laneDistance / hitRadius);
        const float dangerScore = clamp01(threat.danger) * (0.55f * timeWeight + 0.45f * laneWeight);
        if (dangerScore <= best.dangerScore) {
            continue;
        }

        const Vec2 awayFromPlayer = safeDirection(boss.position - player.position, perpendicular(direction));
        Vec2 lateral = perpendicular(direction);
        if (dot(lateral, awayFromPlayer) < 0.0f) {
            lateral = lateral * -1.0f;
        }

        const bool shouldTeleport = teleportCooldown_.ready() &&
            (dangerScore >= config_.teleportDangerThreshold || !dashCooldown_.ready());

        const float step = shouldTeleport ? config_.teleportDistance : config_.dashDistance;
        best.shouldEvade = true;
        best.preferTeleport = shouldTeleport;
        best.dangerScore = dangerScore;
        best.evadeTarget = boss.position + (awayFromPlayer * step) + (lateral * config_.evadeLateralOffset);
    }

    return best;
}

float BossUtilityAI::desiredSpacing(const BossState& boss) const noexcept {
    float desired = config_.aggressiveCastRange;

    if (phase_ == BossPhase::Aggressive) {
        desired = (projectileCooldown_.ready() || aoeCooldown_.ready()) ? config_.aggressiveCastRange : config_.aggressiveChaseRange;
    } else if (phase_ == BossPhase::DefensiveHealing) {
        const float lowHealthPressure = 1.0f - clamp01(boss.healthRatio);
        desired = config_.defensiveBaseRange + (lowHealthPressure * config_.defensiveLowHealthBonusRange);
    } else {
        desired = config_.enragedRange;
    }

    return std::clamp(desired, config_.minSpacingRange, config_.maxSpacingRange);
}

BossUtilityAI::LeadSolution BossUtilityAI::solveIntercept(
    const Vec2& shooterPos,
    float projectileSpeed,
    const Vec2& targetPos,
    const Vec2& targetVelocity,
    float maxLeadSeconds) const noexcept {
    LeadSolution solution{};
    if (projectileSpeed <= 1.0e-5f) {
        return solution;
    }

    constexpr float epsilon = 1.0e-5f;
    const Vec2 relPos = targetPos - shooterPos;
    const float a = dot(targetVelocity, targetVelocity) - (projectileSpeed * projectileSpeed);
    const float b = 2.0f * dot(relPos, targetVelocity);
    const float c = dot(relPos, relPos);

    float intercept = std::numeric_limits<float>::max();
    if (std::abs(a) <= epsilon) {
        if (std::abs(b) > epsilon) {
            const float t = -c / b;
            if (t > 0.0f) {
                intercept = t;
            }
        }
    } else {
        const float disc = (b * b) - (4.0f * a * c);
        if (disc >= 0.0f) {
            const float sqrtDisc = std::sqrt(disc);
            const float invTwoA = 0.5f / a;
            const float t0 = (-b - sqrtDisc) * invTwoA;
            const float t1 = (-b + sqrtDisc) * invTwoA;

            if (t0 > epsilon) {
                intercept = t0;
            }
            if (t1 > epsilon) {
                intercept = std::min(intercept, t1);
            }
        }
    }

    if (!std::isfinite(intercept)) {
        return solution;
    }

    if (intercept == std::numeric_limits<float>::max()) {
        intercept = maxLeadSeconds;
    }
    intercept = std::clamp(intercept, 0.0f, maxLeadSeconds);

    solution.valid = true;
    solution.interceptTime = intercept;
    solution.aimPoint = targetPos + (targetVelocity * intercept);
    return solution;
}

Vec2 BossUtilityAI::predictAoePoint(const BossState& boss, const PlayerState& player) const noexcept {
    const Vec2 smoothedVelocity = (player.velocity * 0.75f) + (lastPlayerVelocity_ * 0.25f);
    const Vec2 toPlayer = safeDirection(player.position - boss.position, {1.0f, 0.0f});
    Vec2 tangent = perpendicular(toPlayer);
    if (dot(tangent, smoothedVelocity) < 0.0f) {
        tangent = tangent * -1.0f;
    }

    Vec2 predicted = player.position + (smoothedVelocity * config_.aoeCastDelaySeconds);
    predicted += tangent * config_.aoeStrafeCompensation;
    if (player.isCastingHighImpactAbility) {
        // Casting players are movement-committed, so bias slightly toward current location.
        predicted = (predicted * 0.55f) + (player.position * 0.45f);
    }
    return predicted;
}

float BossUtilityAI::scoreReposition(float currentDistance, float desiredDistance) const noexcept {
    const float spacingError = std::abs(currentDistance - desiredDistance);
    const float normalizedError = clamp01(spacingError / std::max(1.0f, desiredDistance));
    float phaseScale = 0.72f;
    if (phase_ == BossPhase::DefensiveHealing) {
        phaseScale = 0.90f;
    } else if (phase_ == BossPhase::Enraged) {
        phaseScale = 0.58f;
    }
    return normalizedError * phaseScale;
}

float BossUtilityAI::scoreProjectile(
    const BossState& boss,
    const PlayerState& player,
    float targetDistance,
    const LeadSolution& lead) const noexcept {
    if (!projectileCooldown_.ready() || !lead.valid || targetDistance > config_.projectileMaxRange) {
        return 0.0f;
    }

    const float rangeError = std::abs(targetDistance - config_.projectileOptimalRange);
    const float rangeScore = 1.0f - clamp01(rangeError / std::max(1.0f, config_.projectileOptimalRange));
    const float movementScore = clamp01(length(player.velocity) / std::max(1.0f, config_.playerMaxSpeedForScoring));
    const float castCommitBonus = player.isCastingHighImpactAbility ? 0.14f : 0.0f;
    const float enragePenalty = phase_ == BossPhase::Enraged ? 0.08f : 0.0f;

    const float score = 0.30f + (0.43f * rangeScore) + (0.27f * movementScore) + castCommitBonus - enragePenalty;
    return clamp01(score);
}

float BossUtilityAI::scoreAoe(const BossState& boss, const PlayerState& player, float targetDistance) const noexcept {
    if (!aoeCooldown_.ready() || targetDistance > config_.aoeMaxRange) {
        return 0.0f;
    }

    const float rangeError = std::abs(targetDistance - config_.aoeOptimalRange);
    const float rangeScore = 1.0f - clamp01(rangeError / std::max(1.0f, config_.aoeOptimalRange));
    const float moveCommitScore = player.isCastingHighImpactAbility
        ? 1.0f
        : clamp01(length(player.velocity) / std::max(1.0f, config_.playerMaxSpeedForScoring));
    const float defensivePenalty = phase_ == BossPhase::DefensiveHealing ? 0.10f : 0.0f;

    const float score = 0.24f + (0.41f * rangeScore) + (0.35f * moveCommitScore) - defensivePenalty;
    return clamp01(score);
}

float BossUtilityAI::scoreHeal(const BossState& boss, float targetDistance) const noexcept {
    if (!healCooldown_.ready() || phase_ == BossPhase::Enraged || boss.healthRatio > config_.healStartHealthRatio) {
        return 0.0f;
    }

    const float denom = std::max(0.05f, config_.healStartHealthRatio - config_.healCriticalHealthRatio);
    const float healthNeed = clamp01((config_.healStartHealthRatio - boss.healthRatio) / denom);
    const float spacingSafety = clamp01(targetDistance / std::max(1.0f, config_.defensiveBaseRange));
    const float defensiveBonus = phase_ == BossPhase::DefensiveHealing ? 0.12f : 0.0f;

    const float score = 0.34f + (0.48f * healthNeed) + (0.18f * spacingSafety) + defensiveBonus;
    return clamp01(score);
}

float BossUtilityAI::scoreEnragedBurst(const BossState& boss, float targetDistance) const noexcept {
    if (phase_ != BossPhase::Enraged || !enragedBurstCooldown_.ready()) {
        return 0.0f;
    }

    const float closeScore = 1.0f - clamp01(targetDistance / std::max(1.0f, config_.enrageBurstRange));
    const float lowHealthPressure = 1.0f - clamp01(boss.healthRatio / std::max(0.01f, config_.enrageEnterHealthRatio));
    const float score = 0.40f + (0.40f * closeScore) + (0.20f * lowHealthPressure);
    return clamp01(score);
}

ActionCommand BossUtilityAI::update(
    float dtSeconds,
    const BossState& boss,
    const PlayerState& player,
    const AbilityThreat* threats,
    std::size_t threatCount) noexcept {
    tickCooldowns(dtSeconds);
    updatePhase(dtSeconds, clamp01(boss.healthRatio));

    const float targetDistance = distance(boss.position, player.position);
    const float desiredRange = desiredSpacing(boss);
    const ThreatResult threat = evaluateThreats(boss, player, threats, threatCount);
    const LeadSolution lead = solveIntercept(
        boss.position,
        config_.projectileSpeed,
        player.position,
        player.velocity,
        config_.projectileMaxLeadSeconds);
    const Vec2 aoePoint = predictAoePoint(boss, player);

    for (Candidate& candidate : candidates_) {
        candidate = {};
    }

    std::size_t index = 0U;

    if (threat.shouldEvade) {
        Candidate& evadeCandidate = candidates_[index++];
        evadeCandidate.action = threat.preferTeleport ? BossAction::TeleportEvade : BossAction::DashEvade;
        evadeCandidate.score = 0.92f + (0.08f * threat.dangerScore);
        evadeCandidate.moveTarget = threat.evadeTarget;
    }

    {
        const Vec2 toPlayer = safeDirection(player.position - boss.position, {1.0f, 0.0f});
        const bool tooClose = targetDistance < desiredRange;
        const Vec2 moveDir = tooClose ? (toPlayer * -1.0f) : toPlayer;
        const float step = std::min(config_.maxRepositionStep, std::abs(targetDistance - desiredRange));

        Candidate& reposition = candidates_[index++];
        reposition.action = BossAction::Reposition;
        reposition.score = scoreReposition(targetDistance, desiredRange);
        reposition.moveTarget = boss.position + (moveDir * step);
    }

    {
        Candidate& projectile = candidates_[index++];
        projectile.action = BossAction::ProjectileSkillshot;
        projectile.score = scoreProjectile(boss, player, targetDistance, lead);
        projectile.aimPoint = lead.aimPoint;
    }

    {
        Candidate& aoe = candidates_[index++];
        aoe.action = BossAction::CastAoe;
        aoe.score = scoreAoe(boss, player, targetDistance);
        aoe.aimPoint = aoePoint;
    }

    {
        Candidate& heal = candidates_[index++];
        heal.action = BossAction::Heal;
        heal.score = scoreHeal(boss, targetDistance);
    }

    {
        Candidate& burst = candidates_[index++];
        burst.action = BossAction::EnragedBurst;
        burst.score = scoreEnragedBurst(boss, targetDistance);
        burst.aimPoint = player.position;
    }

    Candidate best{};
    for (const Candidate& candidate : candidates_) {
        if (candidate.score > best.score) {
            best = candidate;
        }
    }

    lastPlayerVelocity_ = player.velocity;

    ActionCommand command{};
    command.phase = phase_;
    if (best.score < config_.actionScoreThreshold || best.action == BossAction::None) {
        return command;
    }

    command.action = best.action;
    command.utilityScore = best.score;
    command.moveTarget = best.moveTarget;
    command.aimPoint = best.aimPoint;
    return command;
}

void BossUtilityAI::onActionCommitted(BossAction action) noexcept {
    switch (action) {
    case BossAction::ProjectileSkillshot:
        projectileCooldown_.start(config_.projectileCooldownSeconds);
        break;
    case BossAction::CastAoe:
        aoeCooldown_.start(config_.aoeCooldownSeconds);
        break;
    case BossAction::Heal:
        healCooldown_.start(config_.healCooldownSeconds);
        break;
    case BossAction::DashEvade:
        dashCooldown_.start(config_.dashCooldownSeconds);
        break;
    case BossAction::TeleportEvade:
        teleportCooldown_.start(config_.teleportCooldownSeconds);
        break;
    case BossAction::EnragedBurst:
        enragedBurstCooldown_.start(config_.enragedBurstCooldownSeconds);
        break;
    case BossAction::None:
    case BossAction::Reposition:
        break;
    }
}

} // namespace arpg::ai
