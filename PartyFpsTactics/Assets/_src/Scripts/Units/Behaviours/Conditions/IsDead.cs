using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units")]
    public class IsDead : BaseUnitConditional
    {
        public override TaskStatus OnUpdate()
        {
            if (selfUnit.HealthController.IsDead)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}