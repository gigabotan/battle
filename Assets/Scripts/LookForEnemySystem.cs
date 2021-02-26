using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace battle
{
    public class LookForEnemySystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _endSimECBSystem;
        private EntityQuery _targetsQuery;
        private EntityQuery _battleQuery;
    
        protected override void OnCreate()
        {
            _endSimECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

            _targetsQuery = GetEntityQuery(ComponentType.ReadOnly<TeamTag>(), 
                ComponentType.ReadOnly<Translation>());
            _battleQuery = GetEntityQuery(ComponentType.ReadOnly<BattleComponent>());

        }
    

        protected override void OnUpdate()
        {
            if (_battleQuery.CalculateEntityCount() == 0)
            {
                return;
            }
            
            var ecb = _endSimECBSystem.CreateCommandBuffer().AsParallelWriter();

            var units = _targetsQuery.ToEntityArray(Allocator.TempJob);

            var lookHandle = Entities
                .WithName("LookEnemy")
                .WithNone<Target>() // looking for idle units
                .WithDisposeOnCompletion(units)
                .ForEach((
                    Entity unitEntity,
                    int entityInQueryIndex,
                    in Translation unitPosition,
                    in TeamTag teamTag
                ) =>
                {
                    var closestDistance = float.PositiveInfinity;
                    var target = Entity.Null;
                    
                    foreach (var unit in units)
                    {
                        if (GetComponent<TeamTag>(unit).Value == teamTag.Value)
                        {
                            continue;
                        }

                        var distance = math.length(GetComponent<Translation>(unit).Value - unitPosition.Value);
                        if (!(distance < closestDistance)) continue;
                        closestDistance = distance;
                        target = unit;
                    }

                    if (target == Entity.Null) return;
                    UnitAIUtility.TransitionToAttack(ecb, unitEntity, entityInQueryIndex, target);
                    UnitAIUtility.TransitionToMove(ecb, unitEntity, entityInQueryIndex);

                }).Schedule(Dependency);
                
            _endSimECBSystem.AddJobHandleForProducer(lookHandle);
            Dependency = lookHandle;
        }
    }
}

