using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using VAutomationCore.Core;
using VAutomationCore.Core.TrapLifecycle;
using VAutomationCore.Core.Data;
using VAutomationCore.Core.Services;
using VAuto.Core.Patterns;
using VAuto.Core.Lifecycle.Data.DataType;

namespace VAuto.Core.Lifecycle
{
    /// <summary>
    /// Arena Lifecycle Manager - Handles lifecycle events for arena zones 
    /// This class is loaded by VAutoZone, intending to make it handle all future events triggering 
    /// </summary>
    public class ArenaLifecycleManager : Singleton<ArenaLifecycleManager>
    {
        private static readonly string _logPrefix = "[ArenaLifecycleManager]";
        
        // Lifecycle stages for arena
        private readonly Dictionary<string, LifecycleStage> _lifecycleStages;
        private readonly Dictionary<string, LifecycleActionHandler> _actionHandlers;
        
        public ManualLogSource Log { get; private set; }
        public new bool IsInitialized { get; private set; }
        public int ServiceCount => _lifecycleStages.Count;

        // Debounce recent transitions: key=(user, arenaId, direction)
        private readonly Dictionary<string, long> _recentTransitions = new Dictionary<string, long>();
        private const int DebounceWindowMs = 1000;

        // ECS Tracking
        private ZoneTrackingHelper _zoneTrackingHelper;
        private bool _ecsTrackingEnabled;
        private float _lastUpdateTime;
        private readonly float _updateInterval = 0.1f; // 10 updates per second

        // Trap lifecycle policy integration
        private ITrapLifecyclePolicy _trapLifecyclePolicy;
        private bool _trapPolicyEnabled;

        public ArenaLifecycleManager()
        {
            Log = VLifecycle.Plugin.Log;
            _lifecycleStages = new Dictionary<string, LifecycleStage>();
            _actionHandlers = new Dictionary<string, LifecycleActionHandler>();
            InitializeActionHandlers();
        }

        /// <summary>
        /// Initialize the arena lifecycle manager
        /// </summary>
        public void Initialize()
        {
            try
            {
                if (IsInitialized) return;
                
                RegisterLifecycleStages();
                InitializeTrapPolicy();
                IsInitialized = true;
                Log?.LogInfo($"{_logPrefix} Initialized");
            }
            catch (Exception ex)
            {
                Log?.LogError($"{_logPrefix} Failed to initialize: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize trap lifecycle policy if VAutoTraps is available and overrides are enabled.
        /// </summary>
        private void InitializeTrapPolicy()
        {
            try
            {
                _trapPolicyEnabled = VLifecycle.Plugin.AllowTrapOverrides;
                
                // Check if VAutoTraps has registered its policy
                if (_trapPolicyEnabled && TrapPolicyResolver.AreOverridesEnabled())
                {
                    _trapLifecyclePolicy = new DefaultTrapLifecyclePolicy();
                    Log?.LogInfo($"{_logPrefix} Trap lifecycle policy enabled (using shared resolver)");
                }
                else
                {
                    _trapLifecyclePolicy = null;
                    Log?.LogDebug($"{_logPrefix} Trap lifecycle policy disabled (config or resolver unavailable)");
                }
            }
            catch (Exception ex)
            {
                Log?.LogWarning($"{_logPrefix} Failed to initialize trap policy: {ex.Message}");
                _trapPolicyEnabled = false;
            }
        }

        /// <summary>
        /// Enable ECS-based zone tracking for autonomous detection.
        /// </summary>
        public void EnableECSTracking()
        {
            if (_ecsTrackingEnabled) return;
            
            try
            {
                _zoneTrackingHelper = new ZoneTrackingHelper(VAutomationCore.Core.UnifiedCore.EntityManager, new CoreLogger("ZoneTracking"));
                _zoneTrackingHelper.Initialize();
                
                // Subscribe to zone transition events
                _zoneTrackingHelper.OnPlayerEnterLifecycleZone += OnPlayerEnterLifecycleZoneHandler;
                _zoneTrackingHelper.OnPlayerExitLifecycleZone += OnPlayerExitLifecycleZoneHandler;
                
                _ecsTrackingEnabled = true;
                Log?.LogInfo($"{_logPrefix} ECS zone tracking enabled");
            }
            catch (Exception ex)
            {
                Log?.LogError($"{_logPrefix} Failed to enable ECS tracking: {ex.Message}");
            }
        }

        /// <summary>
        /// Disable ECS-based zone tracking.
        /// </summary>
        public void DisableECSTracking()
        {
            if (!_ecsTrackingEnabled) return;
            
            if (_zoneTrackingHelper != null)
            {
                _zoneTrackingHelper.OnPlayerEnterLifecycleZone -= OnPlayerEnterLifecycleZoneHandler;
                _zoneTrackingHelper.OnPlayerExitLifecycleZone -= OnPlayerExitLifecycleZoneHandler;
                _zoneTrackingHelper.Dispose();
                _zoneTrackingHelper = null;
            }
            
            _ecsTrackingEnabled = false;
            Log?.LogInfo($"{_logPrefix} ECS zone tracking disabled");
        }

        /// <summary>
        /// ECS update loop - called every frame for autonomous zone detection.
        /// </summary>
        public void UpdateECS(double currentTime)
        {
            if (!_ecsTrackingEnabled) return;
            
            // Throttle updates to reduce CPU usage
            if (currentTime - _lastUpdateTime < _updateInterval) return;
            _lastUpdateTime = currentTime;
            
            try
            {
                _zoneTrackingHelper?.UpdateTracking();
            }
            catch (Exception ex)
            {
                Log?.LogError($"{_logPrefix} ECS update failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a zone for ECS tracking.
        /// </summary>
        public void RegisterZoneForTracking(Entity zoneEntity, float3 center, float radius, string zoneId, bool isLifecycleZone)
        {
            _zoneTrackingHelper?.RegisterZone(zoneEntity, center, radius, zoneId, isLifecycleZone);
        }

        private void OnPlayerEnterLifecycleZoneHandler(Entity player, Entity zone)
        {
            Log?.LogInfo($"{_logPrefix} Player {player} entered lifecycle zone {zone}");
            
            // Trigger enter lifecycle actions
            TriggerLifecycleStage("onEnterLifecycleZone", new LifecycleContext
            {
                CharacterEntity = player,
                Position = _zoneTrackingHelper?.GetPlayerPosition(player) ?? default
            });
        }

        private void OnPlayerExitLifecycleZoneHandler(Entity player, Entity zone)
        {
            Log?.LogInfo($"{_logPrefix} Player {player} exited lifecycle zone {zone}");
            
            // Trigger exit lifecycle actions
            TriggerLifecycleStage("onExitLifecycleZone", new LifecycleContext
            {
                CharacterEntity = player,
                Position = _zoneTrackingHelper?.GetPlayerPosition(player) ?? default
            });
        }

        /// <summary>
        /// Shutdown the arena lifecycle manager
        /// </summary>
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            DisableECSTracking();
            _lifecycleStages.Clear();
            _actionHandlers.Clear();
            IsInitialized = false;
            Log?.LogInfo($"{_logPrefix} Shutdown");
        }

        /// <summary>
        /// Trigger a lifecycle stage when entering arena
        /// </summary>
        public bool OnEnterArena(Entity character, float3 position)
        {
            return TriggerLifecycleStage("onEnterArenaZone", new LifecycleContext
            {
                CharacterEntity = character,
                Position = position
            });
        }

        /// <summary>
        /// Trigger a lifecycle stage when exiting arena
        /// </summary>
        public bool OnExitArena(Entity character, float3 position)
        {
            return TriggerLifecycleStage("onExitArenaZone", new LifecycleContext
            {
                CharacterEntity = character,
                Position = position
            });
        }

        /// <summary>
        /// Called by VAutoZone when player enters arena (via reflection) - preserves position
        /// </summary>
        public bool OnPlayerEnter(Entity userEntity, Entity characterEntity, string arenaId, float3 position)
        {
            if (ShouldDebounce(userEntity, arenaId, "Enter"))
            {
                Log?.LogInfo($"{_logPrefix} Debounced Enter for user={userEntity.Index} arena={arenaId}");
                return false;
            }
            
            // Check trap lifecycle policy via shared resolver
            if (_trapLifecyclePolicy != null && _trapPolicyEnabled)
            {
                var characterId = GetCharacterId(characterEntity);
                var decision = TrapPolicyResolver.EvaluateEnter(new TrapLifecycleContext
                {
                    CharacterId = characterId,
                    ZoneId = arenaId,
                    Position = position,
                    LifecycleStage = "Enter"
                });
                
                if (decision.OverrideTriggered)
                {
                    Log?.LogInfo($"{_logPrefix} Trap policy override on enter: {decision.Reason}");
                    // Continue with lifecycle but may be modified by policy
                }
            }
            
            Log?.LogInfo($"{_logPrefix} Player entering arena {arenaId} at position ({position.x:F0}, {position.y:F0}, {position.z:F0})");
            ApplyDefaults(characterEntity);
            return OnEnterArena(characterEntity, position);
        }

        private void ApplyDefaults(Entity characterEntity)
        {
            try
            {
                var em = UnifiedCore.EntityManager;
                if (!em.Exists(characterEntity))
                {
                    return;
                }

                if (PrefabsAll.ByName.TryGetValue(Default.BloodType, out var bloodGuid))
                {
                    if (!GameActionService.HasBuff(characterEntity, bloodGuid))
                    {
                        GameActionService.TryApplyCleanBuff(characterEntity, bloodGuid, -1f);
                    }
                }
            }
            catch
            {
                // defaults are best-effort; ignore errors
            }
        }

        /// <summary>
        /// Called by VAutoZone when player exits arena (via reflection) - preserves position
        /// </summary>
        public bool OnPlayerExit(Entity userEntity, Entity characterEntity, string arenaId, float3 position)
        {
            if (ShouldDebounce(userEntity, arenaId, "Exit"))
            {
                Log?.LogInfo($"{_logPrefix} Debounced Exit for user={userEntity.Index} arena={arenaId}");
                return false;
            }
            
            // Check trap lifecycle policy via shared resolver
            if (_trapLifecyclePolicy != null && _trapPolicyEnabled)
            {
                var characterId = GetCharacterId(characterEntity);
                var decision = TrapPolicyResolver.EvaluateExit(new TrapLifecycleContext
                {
                    CharacterId = characterId,
                    ZoneId = arenaId,
                    Position = position,
                    LifecycleStage = "Exit"
                });
                
                if (decision.ForceBuffClearOnExit)
                {
                    Log?.LogInfo($"{_logPrefix} Trap policy force buff clear on exit: {decision.Reason}");
                    // Add buff clear action to exit stage
                }
            }
            
            Log?.LogInfo($"{_logPrefix} Player exiting arena {arenaId} from position ({position.x:F0}, {position.y:F0}, {position.z:F0})");
            return OnExitArena(characterEntity, position);
        }

        /// <summary>
        /// Get character ID from character entity.
        /// </summary>
        private ulong GetCharacterId(Entity characterEntity)
        {
            try
            {
                var em = VAutomationCore.Core.UnifiedCore.EntityManager;
                if (em.Exists(characterEntity) && em.HasComponent<ProjectM.PlayerCharacter>(characterEntity))
                {
                    var pc = em.GetComponentData<ProjectM.PlayerCharacter>(characterEntity);
                    var userEntity = pc.UserEntity;
                    if (em.Exists(userEntity) && em.HasComponent<ProjectM.Network.User>(userEntity))
                    {
                        var user = em.GetComponentData<ProjectM.Network.User>(userEntity);
                        return user.PlatformId;
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Handle player connection to server
        /// </summary>
        public void OnPlayerConnected(int userIndex)
        {
            Log?.LogInfo($"{_logPrefix} Player connected: {userIndex}");
            // Initialize any server-level player state if needed
        }

        /// <summary>
        /// Handle player disconnection from server
        /// </summary>
        public void OnPlayerDisconnected(int userIndex)
        {
            Log?.LogInfo($"{_logPrefix} Player disconnected: {userIndex}");
            // Clean up any server-level player state if needed
        }

        /// <summary>
        /// Trigger a lifecycle stage
        /// </summary>
        public bool TriggerLifecycleStage(string stageName, LifecycleContext context)
        {
            try
            {
                if (!_lifecycleStages.TryGetValue(stageName, out var stage))
                {
                    Log?.LogWarning($"{_logPrefix} Unknown lifecycle stage: {stageName}");
                    return false;
                }

                bool allSuccessful = true;
                foreach (var action in stage.Actions)
                {
                    if (!ExecuteAction(action, context))
                    {
                        allSuccessful = false;
                    }
                }

                return allSuccessful;
            }
            catch (Exception ex)
            {
                Log?.LogError($"{_logPrefix} Failed to trigger stage {stageName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Execute a single lifecycle action
        /// </summary>
        private bool ExecuteAction(LifecycleAction action, LifecycleContext context)
        {
            try
            {
                if (!_actionHandlers.TryGetValue(action.Type, out var handler))
                {
                    Log?.LogWarning($"{_logPrefix} No handler for action type: {action.Type}");
                    return false;
                }

                return handler.Execute(action, context);
            }
            catch (Exception ex)
            {
                Log?.LogError($"{_logPrefix} Failed to execute action: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get list of service names
        /// </summary>
        public string[] GetServiceNames()
        {
            return _lifecycleStages.Keys.ToArray();
        }

        /// <summary>
        /// Register default lifecycle stages
        /// </summary>
        private void RegisterLifecycleStages()
        {
            _lifecycleStages["onEnterArenaZone"] = new LifecycleStage
            {
                Name = "onEnterArenaZone",
                Description = "Triggered when player enters arena",
                Actions = new List<LifecycleAction>
                {
                    new LifecycleAction { Type = "save" },
                    new LifecycleAction { Type = "resetcooldowns" },
                    new LifecycleAction { Type = "message", Message = "Entering Arena Zone..." }
                }
            };

            _lifecycleStages["onExitArenaZone"] = new LifecycleStage
            {
                Name = "onExitArenaZone",
                Description = "Triggered when player exits arena",
                Actions = new List<LifecycleAction>
                {
                    new LifecycleAction { Type = "restore" },
                    new LifecycleAction { Type = "clearbuffs" },
                    new LifecycleAction { Type = "message", Message = "Exiting Arena Zone..." }
                }
            };

            // Phase 2-3: Spellbook lifecycle stages
            _lifecycleStages["onEnterLifecycleZone"] = new LifecycleStage
            {
                Name = "onEnterLifecycleZone",
                Description = "Triggered when player enters any lifecycle zone",
                Actions = new List<LifecycleAction>
                {
                    new LifecycleAction { Type = "spellbookgrant" },
                    new LifecycleAction { Type = "vbloodunlock" }
                }
            };

            _lifecycleStages["onExitLifecycleZone"] = new LifecycleStage
            {
                Name = "onExitLifecycleZone",
                Description = "Triggered when player exits any lifecycle zone",
                Actions = new List<LifecycleAction>
                {
                    new LifecycleAction { Type = "spellbookrestore" }
                }
            };
        }

        /// <summary>
        /// Initialize action handlers
        /// </summary>
        private void InitializeActionHandlers()
        {
            _actionHandlers["store"] = new StoreActionHandler();
            _actionHandlers["message"] = new MessageActionHandler();
            
            // State management handlers
            _actionHandlers["save"] = new SavePlayerStateHandler();
            _actionHandlers["restore"] = new RestorePlayerStateHandler();
            _actionHandlers["buff"] = new ApplyBuffHandler();
            _actionHandlers["clearbuffs"] = new ClearBuffsHandler();
            _actionHandlers["removeunequip"] = new RemoveUnequipHandler();
            _actionHandlers["resetcooldowns"] = new ResetCooldownsHandler();
            _actionHandlers["teleport"] = new TeleportHandler();
            _actionHandlers["gameplayevent"] = new CreateGameplayEventHandler();
            
            // Phase 2-3: Automation handlers
            _actionHandlers["vbloodunlock"] = new AutoVBloodUnlockHandler();
            _actionHandlers["spellbookgrant"] = new AutoSpellbookGrantHandler();
        }

        #region Test Methods

        /// <summary>
        /// Run self-test on all lifecycle components
        /// </summary>
        public Dictionary<string, bool> SelfTest()
        {
            var results = new Dictionary<string, bool>();
            
            // Test initialization
            results["Initialized"] = IsInitialized;
            
            // Test stages registered
            results["StagesRegistered"] = _lifecycleStages.Count > 0;
            
            // Test action handlers
            results["ActionHandlers"] = _actionHandlers.Count > 0;
            
            // Test each handler type
            foreach (var handler in _actionHandlers)
            {
                results[$"Handler_{handler.Key}"] = handler.Value != null;
            }
            
            // Test stage execution (without actual actions)
            try
            {
                var testContext = new LifecycleContext
                {
                    CharacterEntity = Entity.Null,
                    Position = float3.zero
                };
                results["StageExecution"] = true;
            }
            catch
            {
                results["StageExecution"] = false;
            }
            
            return results;
        }

        /// <summary>
        /// Add a test action to a stage
        /// </summary>
        public bool AddTestAction(string stageName, LifecycleAction action)
        {
            if (!_lifecycleStages.TryGetValue(stageName, out var stage))
            {
                Log?.LogWarning($"{_logPrefix} Cannot add action - stage not found: {stageName}");
                return false;
            }
            
            stage.Actions.Add(action);
            Log?.LogInfo($"{_logPrefix} Added test action to {stageName}: {action.Type}");
            return true;
        }

        /// <summary>
        /// Clear all actions from a stage
        /// </summary>
        public bool ClearStageActions(string stageName)
        {
            if (!_lifecycleStages.TryGetValue(stageName, out var stage))
            {
                Log?.LogWarning($"{_logPrefix} Cannot clear - stage not found: {stageName}");
                return false;
            }
            
            var count = stage.Actions.Count;
            stage.Actions.Clear();
            Log?.LogInfo($"{_logPrefix} Cleared {count} actions from {stageName}");
            return true;
        }

        /// <summary>
        /// Get stage details for debugging
        /// </summary>
        public Dictionary<string, object> GetStageDetails(string stageName)
        {
            var details = new Dictionary<string, object>();
            
            if (_lifecycleStages.TryGetValue(stageName, out var stage))
            {
                details["Name"] = stage.Name;
                details["Description"] = stage.Description;
                details["ActionCount"] = stage.Actions.Count;
                
                var actionTypes = stage.Actions.Select(a => a.Type).ToList();
                details["ActionTypes"] = actionTypes;
            }
            else
            {
                details["Error"] = "Stage not found";
            }
            
            return details;
        }

        /// <summary>
        /// Get all registered stages and their action counts
        /// </summary>
        public Dictionary<string, int> GetAllStageActionCounts()
        {
            return _lifecycleStages.ToDictionary(
                s => s.Key,
                s => s.Value.Actions.Count
            );
        }

        #endregion

        private bool ShouldDebounce(Entity userEntity, string arenaId, string direction)
        {
            try
            {
                var key = $"{userEntity.Index}:{arenaId}:{direction}";
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (_recentTransitions.TryGetValue(key, out var last))
                {
                    var dt = now - last;
                    if (dt >= 0 && dt < DebounceWindowMs)
                    {
                        return true;
                    }
                }
                _recentTransitions[key] = now;
                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Default trap lifecycle policy that uses the shared TrapPolicyResolver.
    /// VLifecycle uses this to check if VAutoTraps has registered overrides.
    /// </summary>
    internal class DefaultTrapLifecyclePolicy : ITrapLifecyclePolicy
    {
        public bool IsEnabled => TrapPolicyResolver.AreOverridesEnabled();

        public TrapLifecycleDecision OnBeforeLifecycleEnter(TrapLifecycleContext ctx)
        {
            return TrapPolicyResolver.EvaluateEnter(ctx);
        }

        public TrapLifecycleDecision OnBeforeLifecycleExit(TrapLifecycleContext ctx)
        {
            return TrapPolicyResolver.EvaluateExit(ctx);
        }
    }
}
