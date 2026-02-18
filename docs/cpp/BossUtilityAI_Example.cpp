#include "BossUtilityAI.hpp"

#include <array>
#include <iostream>

namespace {

const char* toString(arpg::ai::BossAction action) {
    using arpg::ai::BossAction;
    switch (action) {
    case BossAction::None:
        return "None";
    case BossAction::Reposition:
        return "Reposition";
    case BossAction::ProjectileSkillshot:
        return "ProjectileSkillshot";
    case BossAction::CastAoe:
        return "CastAoe";
    case BossAction::Heal:
        return "Heal";
    case BossAction::DashEvade:
        return "DashEvade";
    case BossAction::TeleportEvade:
        return "TeleportEvade";
    case BossAction::EnragedBurst:
        return "EnragedBurst";
    default:
        return "Unknown";
    }
}

const char* toString(arpg::ai::BossPhase phase) {
    using arpg::ai::BossPhase;
    switch (phase) {
    case BossPhase::Aggressive:
        return "Aggressive";
    case BossPhase::DefensiveHealing:
        return "DefensiveHealing";
    case BossPhase::Enraged:
        return "Enraged";
    default:
        return "Unknown";
    }
}

} // namespace

int main() {
    using namespace arpg::ai;

    BossConfig config{};
    BossUtilityAI ai(config);

    BossState boss{};
    boss.position = {0.0f, 0.0f};
    boss.healthRatio = 0.52f;

    PlayerState player{};
    player.position = {7.0f, 2.5f};
    player.velocity = {2.2f, 0.5f};

    std::array<AbilityThreat, 2> threats{};
    threats[0].origin = {10.0f, 2.0f};
    threats[0].direction = {-1.0f, 0.0f};
    threats[0].speed = 15.0f;
    threats[0].width = 0.7f;
    threats[0].range = 14.0f;
    threats[0].spawnedSeconds = 0.10f;
    threats[0].maxLifetimeSeconds = 1.0f;
    threats[0].danger = 0.9f;

    threats[1].origin = {2.0f, -8.0f};
    threats[1].direction = {0.0f, 1.0f};
    threats[1].speed = 8.0f;
    threats[1].width = 1.0f;
    threats[1].range = 10.0f;
    threats[1].spawnedSeconds = 0.0f;
    threats[1].maxLifetimeSeconds = 1.3f;
    threats[1].danger = 0.4f;

    constexpr float dt = 0.1f;
    for (int frame = 0; frame < 20; ++frame) {
        if (frame == 8) {
            boss.healthRatio = 0.39f; // Defensive phase transition.
        }
        if (frame == 14) {
            boss.healthRatio = 0.17f; // Enrage transition.
            player.isCastingHighImpactAbility = true;
        }

        const ActionCommand command = ai.update(dt, boss, player, threats.data(), threats.size());
        std::cout << "Frame " << frame
                  << " | phase=" << toString(command.phase)
                  << " | action=" << toString(command.action)
                  << " | score=" << command.utilityScore
                  << " | moveTarget=(" << command.moveTarget.x << ", " << command.moveTarget.y << ")"
                  << " | aimPoint=(" << command.aimPoint.x << ", " << command.aimPoint.y << ")\n";

        if (command.action != BossAction::None) {
            ai.onActionCommitted(command.action);
        }

        player.position += player.velocity * dt;
        threats[0].spawnedSeconds += dt;
        threats[1].spawnedSeconds += dt;
    }

    return 0;
}
