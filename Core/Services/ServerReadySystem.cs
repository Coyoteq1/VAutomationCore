using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace VAutomationCore.Core.Services
{
    /// <summary>
    /// Server ready system - handles message and shard taken events.
    /// Use this to initialize systems when server is ready.
    /// </summary>
    public class ServerReadySystem
    {
        private static ServerReadySystem _instance;
        public static ServerReadySystem Instance => _instance ??= new ServerReadySystem();
        
        private bool _isServerReady;
        private bool _isShardTaken;
        private readonly List<Action> _onReadyCallbacks = new List<Action>();
        private readonly List<Action> _onShardTakenCallbacks = new List<Action>();
        private readonly List<Action<string>> _messageHandlers = new List<Action<string>>();
        
        /// <summary>
        /// Mark server as ready.
        /// </summary>
        public void SetServerReady()
        {
            if (_isServerReady) return;
            
            _isServerReady = true;
            Debug.Log("[ServerReady] Server is ready!");
            
            // Execute all pending callbacks
            foreach (var callback in _onReadyCallbacks)
            {
                try { callback(); } catch (Exception ex) { Debug.LogError(ex); }
            }
            _onReadyCallbacks.Clear();
        }
        
        /// <summary>
        /// Mark shard as taken (player joined/connected).
        /// </summary>
        public void SetShardTaken()
        {
            if (_isShardTaken) return;
            
            _isShardTaken = true;
            Debug.Log("[ServerReady] Shard taken by player!");
            
            foreach (var callback in _onShardTakenCallbacks)
            {
                try { callback(); } catch (Exception ex) { Debug.LogError(ex); }
            }
            _onShardTakenCallbacks.Clear();
            
            // Notify other mods
            ModAPI.Broadcast("shard_taken", DateTime.UtcNow);
        }
        
        /// <summary>
        /// Register callback for server ready.
        /// </summary>
        public void OnReady(Action callback)
        {
            if (_isServerReady) callback();
            else _onReadyCallbacks.Add(callback);
        }
        
        /// <summary>
        /// Register callback for shard taken.
        /// </summary>
        public void OnShardTaken(Action callback)
        {
            if (_isShardTaken) callback();
            else _onShardTakenCallbacks.Add(callback);
        }
        
        /// <summary>
        /// Register message handler.
        /// </summary>
        public void OnMessage(Action<string> handler)
        {
            _messageHandlers.Add(handler);
        }
        
        /// <summary>
        /// Send a message to all handlers.
        /// </summary>
        public void SendMessage(string message)
        {
            Debug.Log($"[ServerReady] Message: {message}");
            
            foreach (var handler in _messageHandlers)
            {
                try { handler(message); } catch (Exception ex) { Debug.LogError(ex); }
            }
            
            // Also broadcast to mods
            ModAPI.Broadcast("message", message);
        }
        
        /// <summary>
        /// Check if server is ready.
        /// </summary>
        public bool IsServerReady => _isServerReady;
        
        /// <summary>
        /// Check if shard is taken.
        /// </summary>
        public bool IsShardTaken => _isShardTaken;
        
        /// <summary>
        /// Reset state (for testing/restart).
        /// </summary>
        public void Reset()
        {
            _isServerReady = false;
            _isShardTaken = false;
            _onReadyCallbacks.Clear();
            _onShardTakenCallbacks.Clear();
        }
    }
    
    /// <summary>
    /// Ready helper for quick access.
    /// </summary>
    public static class Ready
    {
        private static ServerReadySystem _sys => ServerReadySystem.Instance;
        
        public static void ServerReady() => _sys.SetServerReady();
        public static void ShardTaken() => _sys.SetShardTaken();
        public static void OnReady(Action cb) => _sys.OnReady(cb);
        public static void OnShardTaken(Action cb) => _sys.OnShardTaken(cb);
        public static void OnMessage(Action<string> h) => _sys.OnMessage(h);
        public static void Message(string msg) => _sys.SendMessage(msg);
        public static bool IsReady => _sys.IsServerReady;
        public static bool IsTaken => _sys.IsShardTaken;
    }
    
    /// <summary>
    /// Integration with existing job system.
    /// </summary>
    public static class ReadyJobs
    {
        /// <summary>
        /// Run job when server is ready.
        /// </summary>
        public static void WhenReady(Action job)
        {
            Ready.OnReady(job);
        }
        
        /// <summary>
        /// Run job when shard is taken.
        /// </summary>
        public static void WhenShardTaken(Action job)
        {
            Ready.OnShardTaken(job);
        }
        
        /// <summary>
        /// Run job when both ready and shard taken.
        /// </summary>
        public static void WhenReadyAndTaken(Action job)
        {
            Ready.OnReady(() => 
            {
                Ready.OnShardTaken(job);
            });
        }
    }
    
    /// <summary>
    /// Admin approval system for commands.
    /// </summary>
    public static class AdminApproval
    {
        private static readonly HashSet<string> _approvedAdmins = new HashSet<string>();
        private static readonly List<Action<string>> _approvalCallbacks = new List<Action<string>>();
        
        /// <summary>
        /// Add admin to approved list.
        /// </summary>
        public static void Approve(string adminName)
        {
            _approvedAdmins.Add(adminName.ToLower());
            Debug.Log($"[Admin] Approved admin: {adminName}");
            
            foreach (var cb in _approvalCallbacks)
            {
                try { cb(adminName); } catch { }
            }
        }
        
        /// <summary>
        /// Remove admin from approved list.
        /// </summary>
        /// </summary>
        public static void Remove(string adminName)
        {
            _approvedAdmins.Remove(adminName.ToLower());
            Debug.Log($"[Admin] Removed admin: {adminName}");
        }
        
        /// <summary>
        /// Check if player is approved admin.
        /// </summary>
        public static bool IsApproved(string adminName)
        {
            return _approvedAdmins.Contains(adminName.ToLower());
        }
        
        /// <summary>
        /// Check and execute if admin approved.
        /// </summary>
        public static bool TryExecute(string adminName, Action action)
        {
            if (IsApproved(adminName))
            {
                action();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Register callback for admin approval.
        /// </summary>
        public static void OnApproved(Action<string> callback)
        {
            _approvalCallbacks.Add(callback);
        }
        
        /// <summary>
        /// Get all approved admins.
        /// </summary>
        public static IReadOnlyCollection<string> GetAll() => _approvedAdmins;
        
        /// <summary>
        /// Clear all approvals.
        /// </summary>
        public static void ClearAll()
        {
            _approvedAdmins.Clear();
        }
        
        /// <summary>
        /// Approve multiple admins at once.
        /// </summary>
        public static void ApproveAll(params string[] admins)
        {
            foreach (var admin in admins)
            {
                Approve(admin);
            }
        }
    }
}
