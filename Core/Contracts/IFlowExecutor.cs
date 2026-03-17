using Unity.Entities;
using VAutomationCore.Core.Lifecycle;

namespace VAutomationCore.Core.Contracts
{
    public interface IFlowExecutor
    {
        LifecycleExecutionResult ExecuteEnterFlow(string flowId, Entity player);
        LifecycleExecutionResult ExecuteExitFlow(string flowId, Entity player);
    }
}
