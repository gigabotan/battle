using Unity.Entities;
using Unity.Transforms;

namespace battle
{
    public class DamageSystem: SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _endSimECBSystem;
        
        protected override void OnCreate()
        {
            _endSimECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _endSimECBSystem.CreateCommandBuffer().AsParallelWriter();

            var damageHandle = Entities
                .WithName("DamageSystem")
                .ForEach((
                    Entity unit,
                    int entityInQueryIndex,
                    ref Health health) =>
                {
                    if (health.Value <= 0)
                    {
                        ecb.DestroyEntity(entityInQueryIndex, unit);
                        var children = GetBuffer<Child>(unit);
                        foreach (var child in children)
                        {
                            ecb.DestroyEntity(entityInQueryIndex, child.Value);
                        }

                    }
                }).Schedule(Dependency);
            
            _endSimECBSystem.AddJobHandleForProducer(damageHandle);
            Dependency = damageHandle;
        }
    }
}