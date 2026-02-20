using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VAutomationCore.Core.ECS;

namespace VAutomationCore.Examples
{
    /// <summary>
    /// Example jobs demonstrating the JobSystemExtensions API.
    /// </summary>
    public class ExampleJobs
    {
        // Example 1: Simple entity iteration
        public static void Example_SimpleIteration(EntityManager em)
        {
            // Run action on all entities with LocalTransform component
            em.ForEach<LocalTransform>(entity => 
            {
                UnityEngine.Debug.Log($"Found entity: {entity}");
            });
        }
        
        // Example 2: With component data access
        public static void Example_WithData(EntityManager em)
        {
            em.ForEach<LocalTransform>((entity, ref LocalTransform transform) => 
            {
                transform.Position += new float3(0, 1, 0);
                UnityEngine.Debug.Log($"Moved entity {entity} to {transform.Position}");
            });
        }
        
        // Example 3: Using JobBuilder fluent API
        public static void Example_JobBuilder(EntityManager em)
        {
            em.Jobs()
                .Job1(new LogJob<LocalTransform>())
                .Int(42)
                .WithRule(JobTimeRule.Fast)
                .Named("ExampleJob")
                .Execute();
        }
        
        // Example 4: Using templates
        public static void Example_Templates(EntityManager em)
        {
            em.RunJob(JobTemplates.LogAll<LocalTransform>());
            em.RunJob(JobTemplates.DestroyAll<EmptyComponent>());
        }
        
        // Example 5: JobFlow for chaining
        public static void Example_JobFlow(EntityManager em)
        {
            new JobFlow()
                .Then(new LogJob<LocalTransform>())
                .Then(new LogJob<LocalTransform>())
                .Run(em);
        }
        
        // Example 6: Using LocalExecutor
        public static void Example_LocalExecutor(EntityManager em)
        {
            em.Exec()
                .Run(new LogJob<LocalTransform>());
        }
        
        // Example 7: Run scheduled job with time rule
        public static void Example_Scheduled(EntityManager em)
        {
            em.RunScheduled("myJob", JobTimeRule.Slow, new LogJob<LocalTransform>());
        }
        
        // Example 8: Multiple jobs at once
        public static void Example_MultipleJobs(EntityManager em)
        {
            em.RunJobs(
                new LogJob<LocalTransform>(),
                new LogJob<LocalTransform>()
            );
        }
        
        // Example 9: Custom IVAutoJob implementation
        public static void Example_CustomJob(EntityManager em)
        {
            var customJob = new CustomPositionJob { Offset = new float3(10, 0, 0) };
            em.RunJob(customJob);
        }
        
        // Example 10: Send results via email after job
        public static async void Example_WithEmail(EntityManager em)
        {
            int count = 0;
            em.ForEach<LocalTransform>(e => count++);
            
            var results = $"Processed {count} entities";
            await Email.SendJobResults("ExampleJob", results);
        }
    }
    
    /// <summary>
    /// Custom job example.
    /// </summary>
    public struct CustomPositionJob : IVAutoJob
    {
        public float3 Offset;
        
        public void Execute(Entity entity)
        {
            UnityEngine.Debug.Log($"Custom job for entity {entity} with offset {Offset}");
        }
        
        public void OnStart() => UnityEngine.Debug.Log("Custom job started");
        public void OnComplete() => UnityEngine.Debug.Log("Custom job completed");
    }
    
    /// <summary>
    /// Combined example showing full workflow.
    /// </summary>
    public static class FullExample
    {
        public static void RunCompleteExample(EntityManager em)
        {
            // Configure email (optional)
            Email.Configure("https://discord.com/api/webhooks/your-webhook");
            
            // Build job chain with time rule
            em.Jobs()
                .Job1(new CustomPositionJob { Offset = new float3(1, 2, 3) })
                .Job2(new LogJob<LocalTransform>())
                .WithRule(JobTimeRule.Default)
                .Named("CompleteExample")
                .Execute();
            
            // Send results
            _ = Email.SendJobResults("CompleteExample", "Job chain executed successfully");
        }
    }
}
