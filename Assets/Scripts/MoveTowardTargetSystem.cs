using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace battle
{
    struct MovementSpeed : IComponentData
    {
        public float MetersPerSecond;
    }

    struct MoveMarker : IComponentData
    {
        
    }
    
    public partial class MoveTowardTargetSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _endSimECBSystem;
        private EntityQuery _entityQuery;
        
        protected override void OnCreate()
        {
            _endSimECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var ecb = _endSimECBSystem.CreateCommandBuffer().AsParallelWriter();
            NativeArray<float3> positions = new NativeArray<float3>(_entityQuery.CalculateEntityCount(), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);


            var calculate = Entities
                .WithName("MoveTowardTarget")
                .WithStoreEntityQueryInField(ref _entityQuery)
                .WithAll<MoveMarker>()
                .ForEach((
                    Entity unit,
                    int entityInQueryIndex,
                    ref Rotation rot,
                    in Translation pos,
                    in Target target,
                    in AttackRange attackRange,
                    in MovementSpeed movementSpeed) =>
                {
                    if (!HasComponent<TeamTag>(target.Value))
                    {
                        positions[entityInQueryIndex] = pos.Value;
                        UnitAIUtility.TransitionFromMove(ecb, unit, entityInQueryIndex);
                        return;
                    }

                    var targetPos = GetComponent<Translation>(target.Value);
                    var vectorToTarget = targetPos.Value - pos.Value;
                    if (math.length(vectorToTarget) < attackRange.Value)
                    {
                        positions[entityInQueryIndex] = pos.Value;
                        UnitAIUtility.TransitionFromMove(ecb, unit, entityInQueryIndex);
                        return;
                    }

                    var moveDirection = math.normalize(vectorToTarget);
                    rot.Value = quaternion.LookRotation(new float3(moveDirection.x, 0.0f, moveDirection.z), math.up());
                    positions[entityInQueryIndex] = pos.Value + moveDirection * movementSpeed.MetersPerSecond * deltaTime;

                }).Schedule(Dependency);
            
            var moveHandle = Entities
                .WithName("Move")
                .WithReadOnly(positions)
                .WithDisposeOnCompletion(positions)
                .WithAll<MoveMarker>()
                .ForEach((int entityInQueryIndex, ref Translation translation) =>
                {
            
                    translation.Value = positions[entityInQueryIndex];
                })
                .Schedule(calculate);
            
            
            _endSimECBSystem.AddJobHandleForProducer(moveHandle);
            Dependency = moveHandle;
        }
    }
}
