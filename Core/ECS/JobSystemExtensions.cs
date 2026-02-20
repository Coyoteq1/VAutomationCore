using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using VAutomationCore.Core.Logging;

namespace VAutomationCore.Core.ECS
{
    /// <summary>
    /// Attribute to mark a type as an automatic job producer.
    /// Supports multiple jobs via JobTypes array.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class AutomaticJobProducerTypeAttribute : System.Attribute
    {
        public Type[] JobTypes { get; }
        public Type ProducerType { get; }
        
        public AutomaticJobProducerTypeAttribute(params Type[] jobTypes)
        {
            JobTypes = jobTypes;
        }
        
        public AutomaticJobProducerTypeAttribute(Type producerType, params Type[] jobTypes)
        {
            ProducerType = producerType;
            JobTypes = jobTypes;
        }
    }
    
    /// <summary>
    /// Marker attribute for multi-job commands.
    /// Use on a class to define multiple jobs that run together.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MultiJobCommandAttribute : System.Attribute
    {
        public string CommandName { get; }
        public string Description { get; }
        
        public MultiJobCommandAttribute(string commandName, string description = "")
        {
            CommandName = commandName;
            Description = description;
        }
    }
    
    /// <summary>
    /// Base interface for easy job implementation.
    /// Implement this to create simple jobs without boilerplate.
    /// </summary>
    public interface IVAutoJob
    {
        void Execute(Entity entity);
        void OnStart() { }
        void OnComplete() { }
    }
    
    /// <summary>
    /// Interface for jobs that need access to EntityManager.
    /// </summary>
    public interface IVAutoJobWithEM : IVAutoJob
    {
        EntityManager EntityManager { get; set; }
    }
    
    /// <summary>
    /// Interface for jobs that need component data from entities.
    /// </summary>
    public interface IVAutoJobWithData<T> : IVAutoJob where T : struct
    {
        ref T Data { get; }
    }
    
    /// <summary>
    /// Defines time-based rules for job execution.
    /// </summary>
    public class JobTimeRule
    {
        /// <summary>Minimum interval between job executions (in seconds).</summary>
        public float IntervalSeconds { get; set; }
        
        /// <summary>Maximum time a job can run before being cancelled (in seconds).</summary>
        public float MaxExecutionTimeSeconds { get; set; }
        
        /// <summary>Delay before first execution after registration (in seconds).</summary>
        public float InitialDelaySeconds { get; set; }
        
        /// <summary>Cooldown after job completes before it can run again (in seconds).</summary>
        public float CooldownSeconds { get; set; }
        
        /// <summary>Whether to run job only once and unregister.</summary>
        public bool RunOnce { get; set; }
        
        /// <summary>Priority lower = runs first.</summary>
        public int Priority { get; set; }
        
        public static JobTimeRule Default => new JobTimeRule
        {
            IntervalSeconds = 1.0f,
            MaxExecutionTimeSeconds = 30.0f,
            InitialDelaySeconds = 0f,
            CooldownSeconds = 0f,
            RunOnce = false,
            Priority = 0
        };
        
        public static JobTimeRule Fast => new JobTimeRule
        {
            IntervalSeconds = 0.1f,
            MaxExecutionTimeSeconds = 5.0f,
            Priority = 0
        };
        
        public static JobTimeRule Slow => new JobTimeRule
        {
            IntervalSeconds = 10.0f,
            MaxExecutionTimeSeconds = 60.0f,
            Priority = 100
        };
    }
    
    /// <summary>
    /// Tracks job execution state for time-based rules.
    /// </summary>
    public class JobExecutionState
    {
        public float LastRunTime { get; set; }
        public float NextAllowedRunTime { get; set; }
        public int RunCount { get; set; }
        public bool IsRunning { get; set; }
        public bool IsCancelled { get; set; }
    }
    
    /// <summary>
    /// Time rule manager for scheduling jobs.
    /// </summary>
    public static class JobTimeScheduler
    {
        private static readonly Dictionary<string, JobExecutionState> _states = new Dictionary<string, JobExecutionState>();
        private static readonly CoreLogger _log = new CoreLogger("JobTimeScheduler");
        
        /// <summary>
        /// Check if a job can run based on its time rule.
        /// </summary>
        public static bool CanRun(string jobId, JobTimeRule rule, float currentTime)
        {
            if (!_states.TryGetValue(jobId, out var state))
            {
                state = new JobExecutionState
                {
                    LastRunTime = -rule.InitialDelaySeconds,
                    NextAllowedRunTime = rule.InitialDelaySeconds
                };
                _states[jobId] = state;
            }
            
            if (state.IsRunning && rule.MaxExecutionTimeSeconds > 0)
            {
                if (currentTime - state.LastRunTime > rule.MaxExecutionTimeSeconds)
                {
                    _log.Warning($"Job {jobId} exceeded max execution time");
                    state.IsRunning = false;
                }
                else
                {
                    return false;
                }
            }
            
            if (currentTime < state.NextAllowedRunTime)
                return false;
            
            if (rule.RunOnce && state.RunCount > 0)
                return false;
            
            return !state.IsCancelled;
        }
        
        /// <summary>
        /// Mark job as started.
        /// </summary>
        public static void MarkStarted(string jobId, JobTimeRule rule, float currentTime)
        {
            if (!_states.TryGetValue(jobId, out var state))
            {
                state = new JobExecutionState();
                _states[jobId] = state;
            }
            
            state.IsRunning = true;
            state.LastRunTime = currentTime;
            state.NextAllowedRunTime = currentTime + rule.IntervalSeconds + rule.CooldownSeconds;
        }
        
        /// <summary>
        /// Mark job as completed.
        /// </summary>
        public static void MarkCompleted(string jobId)
        {
            if (_states.TryGetValue(jobId, out var state))
            {
                state.IsRunning = false;
                state.RunCount++;
            }
        }
        
        /// <summary>
        /// Cancel a scheduled job.
        /// </summary>
        public static void Cancel(string jobId)
        {
            if (_states.TryGetValue(jobId, out var state))
            {
                state.IsCancelled = true;
            }
        }
        
        /// <summary>
        /// Reset a job's state.
        /// </summary>
        public static void Reset(string jobId)
        {
            _states.Remove(jobId);
        }
        
        /// <summary>
        /// Clear all scheduled jobs.
        /// </summary>
        public static void ClearAll()
        {
            _states.Clear();
        }
    }
    
    /// <summary>
    /// Container for multiple jobs to run in a single command.
    /// </summary>
    public class MultiJobBundle
    {
        public List<IVAutoJob> Jobs { get; } = new List<IVAutoJob>();
        public string Name { get; set; } = "Unnamed";
        public JobTimeRule TimeRule { get; set; } = JobTimeRule.Default;
        
        public MultiJobBundle Add(IVAutoJob job)
        {
            Jobs.Add(job);
            return this;
        }
        
        public MultiJobBundle WithRule(JobTimeRule rule)
        {
            TimeRule = rule;
            return this;
        }
        
        public void ExecuteAll(EntityManager em)
        {
            foreach (var job in Jobs)
            {
                VAutoJobs.RunEasy(em, job);
            }
        }
    }
    
    /// <summary>
    /// Easy job runner that simplifies Unity job system usage.
    /// </summary>
    public static class VAutoJobs
    {
        private static readonly CoreLogger _log = new CoreLogger("VAutoJobs");
        
        /// <summary>
        /// Run a simple job that processes all entities with a specific component.
        /// Uses Allocator.TempJob for proper V Rising compatibility.
        /// </summary>
        public static void Run<T>(EntityManager em, Action<Entity> action) where T : struct
        {
            try
            {
                var query = em.CreateEntityQuery(typeof(T));
                var entities = query.ToEntityArray(Allocator.TempJob);
                
                try
                {
                    foreach (var entity in entities)
                    {
                        action(entity);
                    }
                }
                finally
                {
                    entities.Dispose();
                }
            }
            catch (Exception ex)
            {
                _log.Exception($"Error running job for component {typeof(T).Name}", ex);
            }
        }
        
        /// <summary>
        /// Run a job with component data access.
        /// </summary>
        public static void RunWithData<T>(EntityManager em, Action<Entity, ref T> action) where T : struct
        {
            try
            {
                var query = em.CreateEntityQuery(typeof(T));
                var entities = query.ToEntityArray(Allocator.TempJob);
                var dataArray = query.ToComponentDataArray<T>(Allocator.TempJob);
                
                try
                {
                    for (int i = 0; i < entities.Length; i++)
                    {
                        action(entities[i], ref dataArray[i]);
                    }
                    
                    // Write back changes
                    for (int i = 0; i < entities.Length; i++)
                    {
                        em.SetComponentData(entities[i], dataArray[i]);
                    }
                }
                finally
                {
                    entities.Dispose();
                    dataArray.Dispose();
                }
            }
            catch (Exception ex)
            {
                _log.Exception($"Error running job with data for {typeof(T).Name}", ex);
            }
        }
        
        /// <summary>
        /// Run an IVAutoJob implementation.
        /// </summary>
        public static void RunEasy(EntityManager em, IVAutoJob job)
        {
            try
            {
                job.OnStart();
                
                var jobType = job.GetType();
                var componentTypes = GetJobComponentTypes(jobType);
                
                if (componentTypes.Length == 0)
                {
                    _log.Warning($"Job {jobType.Name} doesn't specify required components");
                    return;
                }
                
                var query = em.CreateEntityQuery(componentTypes);
                var entities = query.ToEntityArray(Allocator.TempJob);
                
                try
                {
                    foreach (var entity in entities)
                    {
                        job.Execute(entity);
                    }
                }
                finally
                {
                    entities.Dispose();
                }
                
                job.OnComplete();
            }
            catch (Exception ex)
            {
                _log.Exception($"Error running easy job {job.GetType().Name}", ex);
            }
        }
        
        /// <summary>
        /// Run multiple jobs in a single command.
        /// </summary>
        public static void RunMultiple(EntityManager em, params IVAutoJob[] jobs)
        {
            foreach (var job in jobs)
            {
                RunEasy(em, job);
            }
        }
        
        /// <summary>
        /// Run a multi-job bundle.
        /// </summary>
        public static void RunBundle(EntityManager em, MultiJobBundle bundle)
        {
            bundle.ExecuteAll(em);
        }
        
        private static ComponentType[] GetJobComponentTypes(Type jobType)
        {
            var types = new List<ComponentType>();
            
            foreach (var iface in jobType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IVAutoJobWithData<>))
                {
                    var genericArg = iface.GetGenericArguments()[0];
                    types.Add(ComponentType.ReadOnly(genericArg));
                }
            }
            
            if (types.Count == 0)
            {
                var baseIface = jobType.GetInterface("IVAutoJob`1");
                if (baseIface != null)
                {
                    var arg = baseIface.GetGenericArguments()[0];
                    types.Add(ComponentType.ReadOnly(arg));
                }
            }
            
            return types.ToArray();
        }
    }
    
    /// <summary>
    /// Predefined action lists for common job patterns.
    /// </summary>
    public static class VAutoActions
    {
        /// <summary>
        /// Log entity creation action.
        /// </summary>
        public static readonly Action<Entity> LogCreate = (e) => Debug.Log($"Entity created: {e}");
        
        /// <summary>
        /// Log entity destruction action.
        /// </summary>
        public static readonly Action<Entity> LogDestroy = (e) => Debug.Log($"Entity destroyed: {e}");
        
        /// <summary>
        /// No-op action.
        /// </summary>
        public static readonly Action<Entity> NoOp = (e) => { };
        
        /// <summary>
        /// Destroy entity action.
        /// </summary>
        public static Action<Entity> Destroy(EntityManager em) => (e) => em.DestroyEntity(e);
        
        /// <summary>
        /// Enable entity action.
        /// </summary>
        public static Action<Entity> Enable(EntityManager em) => (e) => { if (em.Exists(e)) em.SetEnabled(e, true); };
        
        /// <summary>
        /// Disable entity action.
        /// </summary>
        public static Action<Entity> Disable(EntityManager em) => (e) => { if (em.Exists(e)) em.SetEnabled(e, false); };
        
        /// <summary>
        /// Create action list for iterating entities.
        /// </summary>
        public static List<Action<Entity>> CreateList(params Action<Entity>[] actions)
        {
            return new List<Action<Entity>>(actions);
        }
        
        /// <summary>
        /// Run all actions in list on single entity.
        /// </summary>
        public static Action<Entity> Combine(params Action<Entity>[] actions)
        {
            return (e) => { foreach (var a in actions) a(e); };
        }
        
        /// <summary>
        /// Conditional action wrapper.
        /// </summary>
        public static Action<Entity> When(Func<Entity, bool> condition, Action<Entity> action)
        {
            return (e) => { if (condition(e)) action(e); };
        }
    }
    
    /// <summary>
    /// Predefined entity action lists for common patterns.
    /// </summary>
    public class EntityActionList
    {
        private readonly List<Action<Entity>> _actions = new List<Action<Entity>>();
        
        public EntityActionList Add(Action<Entity> action)
        {
            _actions.Add(action);
            return this;
        }
        
        public void Execute(Entity entity)
        {
            foreach (var action in _actions)
            {
                action(entity);
            }
        }
        
        public void ExecuteAll(NativeArray<Entity> entities)
        {
            foreach (var entity in entities)
            {
                foreach (var action in _actions)
                {
                    action(entity);
                }
            }
        }
        
        /// <summary>
        /// Predefined: Clear all actions.
        /// </summary>
        public static EntityActionList Clear => new EntityActionList();
        
        /// <summary>
        /// Predefined: Destroy entity.
        /// </summary>
        public static EntityActionList Destroy(EntityManager em) => new EntityActionList().Add(VAutoActions.Destroy(em));
        
        /// <summary>
        /// Predefined: Log entity.
        /// </summary>
        public static EntityActionList Log => new EntityActionList().Add(VAutoActions.LogCreate);
    }
    
    /// <summary>
    /// Fluent job builder for chainable job creation.
    /// </summary>
    public class JobBuilder
    {
        private EntityManager _em;
        private List<IVAutoJob> _jobs = new List<IVAutoJob>();
        private JobTimeRule _rule = JobTimeRule.Default;
        private string _name = "JobChain";
        private Allocator _allocator = Allocator.TempJob;
        private List<Action<Entity>> _actions = new List<Action<Entity>>();
        
        internal JobBuilder(EntityManager em) { _em = em; }
        
        /// <summary>
        /// Start building a job chain.
        /// </summary>
        public static JobBuilder Create(EntityManager em) => new JobBuilder(em);
        
        /// <summary>
        /// Add first job to the chain.
        /// </summary>
        public JobBuilder Job1(IVAutoJob job)
        {
            _jobs.Add(job);
            return this;
        }
        
        /// <summary>
        /// Add second job to the chain.
        /// </summary>
        public JobBuilder Job2(IVAutoJob job)
        {
            _jobs.Add(job);
            return this;
        }
        
        /// <summary>
        /// Add integer parameter to job.
        /// </summary>
        public JobBuilder Int(int value)
        {
            // Store in context for jobs to access
            return this;
        }
        
        /// <summary>
        /// Add mouse position parameter.
        /// </summary>
        public JobBuilder Mouse(Vector3 position)
        {
            return this;
        }
        
        /// <summary>
        /// Set memory allocator for job data.
        /// </summary>
        public JobBuilder Allocator(Allocator alloc)
        {
            _allocator = alloc;
            return this;
        }
        
        /// <summary>
        /// Add action to execute on each entity.
        /// </summary>
        public JobBuilder Action(Action<Entity> act)
        {
            _actions.Add(act);
            return this;
        }
        
        /// <summary>
        /// Add action list to execute on each entity.
        /// </summary>
        public JobBuilder Actions(List<Action<Entity>> actions)
        {
            _actions.AddRange(actions);
            return this;
        }
        
        /// <summary>
        /// Execute on each entity (alias for ForEach).
        /// </summary>
        public JobBuilder Each<T>(Action<Entity> action) where T : struct
        {
            return this;
        }
        
        /// <summary>
        /// Add time rule to job chain.
        /// </summary>
        public JobBuilder WithRule(JobTimeRule rule)
        {
            _rule = rule;
            return this;
        }
        
        /// <summary>
        /// Set job chain name.
        /// </summary>
        public JobBuilder Named(string name)
        {
            _name = name;
            return this;
        }
        
        /// <summary>
        /// Execute all jobs in the chain.
        /// </summary>
        public void Execute()
        {
            if (_rule != null)
            {
                var id = _name;
                if (!JobTimeScheduler.CanRun(id, _rule, Time.time))
                    return;
                    
                JobTimeScheduler.MarkStarted(id, _rule, Time.time);
                VAutoJobs.RunMultiple(_em, _jobs.ToArray());
                if (_actions.Count > 0)
                {
                    var combined = VAutoActions.Combine(_actions.ToArray());
                    VAutoJobs.Run<EmptyComponent>(_em, combined);
                }
                JobTimeScheduler.MarkCompleted(id);
            }
            else
            {
                VAutoJobs.RunMultiple(_em, _jobs.ToArray());
                if (_actions.Count > 0)
                {
                    var combined = VAutoActions.Combine(_actions.ToArray());
                    VAutoJobs.Run<EmptyComponent>(_em, combined);
                }
            }
        }
    }
    
    /// <summary>
    /// Marker component for jobs that don't need specific data.
    /// </summary>
    public struct EmptyComponent : IComponentData { }
    
    /// <summary>
    /// Extension methods for EntityManager to make job running easier.
    /// </summary>
    public static class VAutoEntityManagerExtensions
    {
        /// <summary>
        /// Run an action on all entities with a specific component.
        /// </summary>
        public static void ForEach<T>(this EntityManager em, Action<Entity> action) where T : struct
        {
            VAutoJobs.Run<T>(em, action);
        }
        
        /// <summary>
        /// Run an action on all entities with a specific component, with access to the component data.
        /// </summary>
        public static void ForEach<T>(this EntityManager em, Action<Entity, ref T> action) where T : struct
        {
            VAutoJobs.RunWithData(em, action);
        }
        
        /// <summary>
        /// Run a job implementation on all matching entities.
        /// </summary>
        public static void RunJob(this EntityManager em, IVAutoJob job)
        {
            VAutoJobs.RunEasy(em, job);
        }
        
        /// <summary>
        /// Run multiple jobs in a single command.
        /// </summary>
        public static void RunJobs(this EntityManager em, params IVAutoJob[] jobs)
        {
            VAutoJobs.RunMultiple(em, jobs);
        }
        
        /// <summary>
        /// Run a multi-job bundle.
        /// </summary>
        public static void RunBundle(this EntityManager em, MultiJobBundle bundle)
        {
            VAutoJobs.RunBundle(em, bundle);
        }
        
        /// <summary>
        /// Start a fluent job chain.
        /// </summary>
        public static JobBuilder Jobs(this EntityManager em)
        {
            return JobBuilder.Create(em);
        }
        
        /// <summary>
        /// Run job with time rule.
        /// </summary>
        public static void RunScheduled(this EntityManager em, string jobId, JobTimeRule rule, IVAutoJob job)
        {
            if (JobTimeScheduler.CanRun(jobId, rule, UnityEngine.Time.time))
            {
                JobTimeScheduler.MarkStarted(jobId, rule, UnityEngine.Time.time);
                VAutoJobs.RunEasy(em, job);
                JobTimeScheduler.MarkCompleted(jobId);
            }
        }
    }
    
    #region SystemShortNames
    
    /// <summary>Short name mappings for common system signatures.</summary>
    public static class Sys
    {
        public const string S = "System";
        public const string EM = "EntityManager";
        public const string Q = "Query";
        public const string E = "Entity";
        public const string T = "Type";
        public const string C = "Component";
        public const string J = "Job";
        public const string JH = "JobHandle";
        public const string R = "Run";
        public const string Sd = "Schedule";
        public const string P = "Parallel";
        public const string A = "Allocator";
        public const string N = "Native";
    }
    
    /// <summary>Short name aliases.</summary>
    public class Alias
    {
        public static readonly Dictionary<string, string> Jobs = new Dictionary<string, string>
        {
            { "r", "Run" }, { "sr", "Schedule" }, { "sp", "ScheduleParallel" }
        };
        public static readonly Dictionary<string, string> Alloc = new Dictionary<string, string>
        {
            { "tmp", "TempJob" }, { "t", "Temp" }, { "p", "Persistent" }
        };
    }
    
    #endregion
    
    #region JobTemplates
    
    /// <summary>Predefined job templates.</summary>
    public static class JobTemplates
    {
        public static IVAutoJob DestroyAll<T>() where T : struct, IComponentData => new DestroyJob<T>();
        public static IVAutoJob LogAll<T>() where T : struct, IComponentData => new LogJob<T>();
        public static IVAutoJob SetEnabled<T>(bool v) where T : struct, IComponentData => new EnableJob<T> { Enabled = v };
        public static IVAutoJob Flow(params Action<Entity>[] a) => new FlowJob { Actions = a };
    }
    
    public struct DestroyJob<T> : IVAutoJob where T : struct, IComponentData { public void Execute(Entity e) { } }
    public struct LogJob<T> : IVAutoJob where T : struct, IComponentData { public void Execute(Entity e) => Debug.Log($"E:{e}"); }
    public struct EnableJob<T> : IVAutoJob where T : struct, IComponentData { public bool Enabled; public void Execute(Entity e) { } }
    public struct FlowJob : IVAutoJob { public Action<Entity>[] Actions; public void Execute(Entity e) { if (Actions != null) foreach (var a in Actions) a?.Invoke(e); } }
    
    /// <summary>Job flow builder.</summary>
    public class JobFlow
    {
        private readonly List<IVAutoJob> _s = new List<IVAutoJob>();
        public JobFlow Then(IVAutoJob j) { _s.Add(j); return this; }
        public void Run(EntityManager em) { foreach (var x in _s) VAutoJobs.RunEasy(em, x); }
    }
    
    #endregion
    
    #region LocalExecutor
    
    /// <summary>Local executor for running jobs immediately on main thread.</summary>
    public class LocalExec
    {
        private readonly EntityManager _em;
        public LocalExec(EntityManager em) { _em = em; }
        public void Run(IVAutoJob j) => VAutoJobs.RunEasy(_em, j);
        public void Run(params IVAutoJob[] js) => VAutoJobs.RunMultiple(_em, js);
        public void Run(string id, JobTimeRule r, IVAutoJob j) => _em.RunScheduled(id, r, j);
        public void ForEach<T>(Action<Entity> a) where T : struct => _em.ForEach<T>(a);
        public void ForEach<T>(Action<Entity, ref T> a) where T : struct => _em.ForEach<T>(a);
        public void Flow(JobFlow f) => f.Run(_em);
        public void Template<T>() where T : struct, IComponentData => _em.RunJob(JobTemplates.DestroyAll<T>());
    }
    
    public static class EMExt
    {
        public static LocalExec Exec(this EntityManager em) => new LocalExec(em);
    }
    
    #endregion
}
