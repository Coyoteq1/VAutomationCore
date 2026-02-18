using System;
using System.Collections.Generic;
using BepInEx.Logging;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using VAuto.Core.Lifecycle;

namespace VAuto.Core.Lifecycle
{
    public interface IEnhancedArenaSnapshotService
    {
        System.Threading.Tasks.Task CreateSnapshotAsync(Entity user, Entity character, string arenaId, string tag);
        System.Threading.Tasks.Task DeleteSnapshotAsync(string characterId, string arenaId, string tag);
        System.Threading.Tasks.Task CleanupArenaSnapshotsAsync(string arenaId);
    }

    internal sealed class EnhancedArenaSnapshotServiceStub : IEnhancedArenaSnapshotService
    {
        private readonly ManualLogSource _log;
        public EnhancedArenaSnapshotServiceStub(ManualLogSource log) { _log = log; }
        public System.Threading.Tasks.Task CreateSnapshotAsync(Entity user, Entity character, string arenaId, string tag)
        {
            _log?.LogInfo($"[EnhancedArenaSnapshot] (stub) CreateSnapshot char={character.Index} arena={arenaId} tag={tag}");
            return System.Threading.Tasks.Task.CompletedTask;
        }
        public System.Threading.Tasks.Task DeleteSnapshotAsync(string characterId, string arenaId, string tag)
        {
            _log?.LogInfo($"[EnhancedArenaSnapshot] (stub) DeleteSnapshot charId={characterId} arena={arenaId} tag={tag}");
            return System.Threading.Tasks.Task.CompletedTask;
        }
        public System.Threading.Tasks.Task CleanupArenaSnapshotsAsync(string arenaId)
        {
            _log?.LogInfo($"[EnhancedArenaSnapshot] (stub) CleanupSnapshots arena={arenaId}");
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }

    /// <summary>
    /// Lifecycle service for EnhancedArenaSnapshotService.
    /// Handles snapshot creation on arena entry and deletion on arena exit.
    /// </summary>
    public class EnhancedArenaSnapshotLifecycleService : IArenaLifecycleService
    {
        // Exposed for integration tests to assert snapshot calls
        public static EnhancedArenaSnapshotLifecycleService Instance { get; set; }

        public bool IsInitialized { get; private set; }
        public ManualLogSource Log { get; private set; }
        
        private readonly Dictionary<Entity, string> _playerArenaMap = new();
        private readonly Dictionary<Entity, Entity> _userCharacterMap = new();
        private readonly Dictionary<string, string> _activeTags = new(); // key=(characterId,arenaId)
        private IEnhancedArenaSnapshotService _snapshotService;

        /// <summary>
        /// Initialize the snapshot lifecycle service
        /// </summary>
        public void Initialize(ManualLogSource logger)
        {
            try
            {
                Log = logger;
                _snapshotService ??= new EnhancedArenaSnapshotServiceStub(Log);
                IsInitialized = true;
                Instance = this;
                Log?.LogInfo("[EnhancedArenaSnapshotLifecycleService] Initialized snapshot lifecycle service");
            }
            catch (Exception ex)
            {
                Log?.LogError($"[EnhancedArenaSnapshotLifecycleService] Failed to initialize: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cleanup the snapshot lifecycle service
        /// </summary>
        public void Cleanup()
        {
            try
            {
                _playerArenaMap.Clear();
                _userCharacterMap.Clear();
                IsInitialized = false;
                Instance = null;
                Log?.LogInfo("[EnhancedArenaSnapshotLifecycleService] Cleaned up snapshot lifecycle service");
            }
            catch (Exception ex)
            {
                Log?.LogError($"[EnhancedArenaSnapshotLifecycleService] Failed to cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when player enters arena - CREATE SNAPSHOT
        /// </summary>
        public virtual bool OnPlayerEnter(Entity user, Entity character, string arenaId)
        {
            try
            {
                if (!ValidateState())
                {
                    Log?.LogWarning("[EnhancedArenaSnapshotLifecycleService] Service not initialized, cannot handle player enter");
                    return false;
                }

                Log?.LogInfo($"[EnhancedArenaSnapshotLifecycleService] Player entering arena {arenaId} - creating snapshot");

                // Store arena mapping for exit
                _playerArenaMap[user] = arenaId;
                _userCharacterMap[user] = character;

                // Build CharacterId and Tag
                var characterId = GetCharacterId(user);
                var tag = BuildSnapshotTag(characterId, arenaId);
                _activeTags[$"{characterId}:{arenaId}"] = tag;
                Log?.LogInfo($"[EnhancedArenaSnapshot] Tag generated tag={tag} char={characterId} arena={arenaId}");

                // Create snapshot via service
                _snapshotService.CreateSnapshotAsync(user, character, arenaId, tag).GetAwaiter().GetResult();
                Log?.LogInfo($"[EnhancedArenaSnapshotLifecycleService] ✅ Snapshot created successfully for arena entry");

                // Emit CreateGameplayEvent (Enter)
                Log?.LogInfo($"[EnhancedArenaSnapshot] CreateGameplayEvent Enter arena={arenaId} char={characterId} tag={tag}");
                return true;
            }
            catch (Exception ex)
            {
                Log?.LogError($"[EnhancedArenaSnapshotLifecycleService] Error in OnPlayerEnter: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Called when player exits arena - DELETE SNAPSHOT
        /// </summary>
        public virtual bool OnPlayerExit(Entity user, Entity character, string arenaId)
        {
            try
            {
                if (!ValidateState())
                {
                    Log?.LogWarning("[EnhancedArenaSnapshotLifecycleService] Service not initialized, cannot handle player exit");
                    return true; // Exit should not fail
                }

                // Use stored arenaId if available
                if (_playerArenaMap.TryGetValue(user, out var storedArenaId))
                {
                    arenaId = storedArenaId;
                    _playerArenaMap.Remove(user);
                }

                // Get character from stored mapping if not provided
                if (character == Entity.Null && _userCharacterMap.TryGetValue(user, out var storedCharacter))
                {
                    character = storedCharacter;
                    _userCharacterMap.Remove(user);
                }

                Log?.LogInfo($"[EnhancedArenaSnapshotLifecycleService] Player exiting arena {arenaId} - deleting snapshot");

                // Get character ID for snapshot deletion
                var characterId = GetCharacterId(user);

                // Determine Tag
                var key = $"{characterId}:{arenaId}";
                if (!_activeTags.TryGetValue(key, out var tag) || string.IsNullOrEmpty(tag))
                {
                    tag = BuildSnapshotTag(characterId, arenaId);
                    Log?.LogWarning($"[EnhancedArenaSnapshot] No prior Tag on Exit; generated tag={tag} char={characterId} arena={arenaId}");
                }
                else
                {
                    _activeTags.Remove(key);
                }

                // Delete/restore snapshot via service
                _snapshotService.DeleteSnapshotAsync(characterId, arenaId, tag).GetAwaiter().GetResult();
                Log?.LogInfo($"[EnhancedArenaSnapshotLifecycleService] ✅ Snapshot deletion/restoration completed for arena exit");

                // Emit CreateGameplayEvent (Exit)
                Log?.LogInfo($"[EnhancedArenaSnapshot] CreateGameplayEvent Exit arena={arenaId} char={characterId} tag={tag}");
                return true;
            }
            catch (Exception ex)
            {
                Log?.LogError($"[EnhancedArenaSnapshotLifecycleService] Error in OnPlayerExit: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Called when arena lifecycle starts
        /// </summary>
        public bool OnArenaStart(string arenaId)
        {
            try
            {
                if (!ValidateState()) return true;
                
                Log?.LogInfo($"[EnhancedArenaSnapshotLifecycleService] Arena {arenaId} started - no action needed for snapshots");
                return true;
            }
            catch (Exception ex)
            {
                Log?.LogError($"[EnhancedArenaSnapshotLifecycleService] Error in OnArenaStart: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Called when arena lifecycle ends
        /// </summary>
        public bool OnArenaEnd(string arenaId)
        {
            try
            {
                if (!ValidateState()) return true;
                
                Log?.LogInfo($"[EnhancedArenaSnapshotLifecycleService] Arena {arenaId} ended - cleaning up all snapshots");
                
                // Clean up all snapshots for this arena
                CleanupArenaSnapshots(arenaId);
                
                return true;
            }
            catch (Exception ex)
            {
                Log?.LogError($"[EnhancedArenaSnapshotLifecycleService] Error in OnArenaEnd: {ex.Message}");
                return false;
            }
        }

        #region Private Helper Methods

        private bool ValidateState()
        {
            return IsInitialized && Log != null;
        }

        private string GetCharacterId(Entity user)
        {
            try
            {
                var em = VAutomationCore.Core.UnifiedCore.EntityManager;
                if (em == null)
                {
                    Log?.LogWarning("[EnhancedArenaSnapshotLifecycleService] EntityManager not available");
                    return user.Index.ToString();
                }

                if (em.TryGetComponentData(user, out User userData))
                {
                    return userData.PlatformId.ToString();
                }

                Log?.LogWarning("[EnhancedArenaSnapshotLifecycleService] Could not get user data");
                return user.Index.ToString();
            }
            catch (Exception ex)
            {
                Log?.LogError($"[EnhancedArenaSnapshotLifecycleService] Error getting character ID: {ex.Message}");
                return user.Index.ToString();
            }
        }

        private bool AsyncCreateSnapshot(Entity user, Entity character, string arenaId)
        {
            // TODO: Replace with actual EnhancedArenaSnapshotService.CreateSnapshot call
            // This is a placeholder for the async implementation
            try
            {
                // Example: return EnhancedArenaSnapshotService.CreateSnapshotAsync(user, character, arenaId).Result;
                
                // For now, create a PlayerLifecycleEvent and log
                var lifecycleEvent = new PlayerLifecycleEvent
                {
                    UserEntity = user,
                    CharacterEntity = character,
                    ArenaId = arenaId,
                    EventType = PlayerLifecycleEventType.Enter,
                    Timestamp = DateTime.UtcNow,
                    EventData = { { "Method", "AsyncCreateSnapshot" } }
                };
                
                Log?.LogInfo($"[EnhancedArenaSnapshotLifecycleService] Created event for arena entry snapshot");
                return true;
            }
            catch (Exception ex)
            {
                Log?.LogError($"[EnhancedArenaSnapshotLifecycleService] AsyncCreateSnapshot failed: {ex.Message}");
                return false;
            }
        }

        private void AsyncDeleteSnapshot(string characterId, string arenaId)
        {
            // TODO: Replace with actual EnhancedArenaSnapshotService.DeleteSnapshotAsync call
            // This is a placeholder for the async implementation
            try
            {
                // Example: EnhancedArenaSnapshotService.DeleteSnapshotAsync(characterId, arenaId);
                
                Log?.LogInfo($"[EnhancedArenaSnapshotLifecycleService] Initiated async delete for snapshot: character={characterId}, arena={arenaId}");
            }
            catch (Exception ex)
            {
                Log?.LogError($"[EnhancedArenaSnapshotLifecycleService] AsyncDeleteSnapshot failed: {ex.Message}");
            }
        }

        private string BuildSnapshotTag(string characterId, string arenaId)
        {
            return $"{characterId}:{arenaId}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        }

        #endregion
    }
}
