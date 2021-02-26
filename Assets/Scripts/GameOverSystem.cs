using System;
using Unity.Collections;
using Unity.Entities;

namespace battle
{

    public struct BattleComponent: IComponentData
    {
        public bool IsOver;
    }

    public struct GameOverMarker : IComponentData
    {
        
    }
    
    [UpdateAfter(typeof(AttackSystem))]
    public class GameOverSystem: SystemBase
    {

        private EntityQuery _entityQuery;
        private EndSimulationEntityCommandBufferSystem _endSimECBSystem;

        protected override void OnCreate()
        {
            _endSimECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate()
        {
            NativeHashSet<int> aliveTeams = new NativeHashSet<int>(Enum.GetNames(typeof(Team)).Length, Allocator.TempJob);
            var ecb = _endSimECBSystem.CreateCommandBuffer().AsParallelWriter();

            var calculate = Entities
                .WithName("CheckTeams")
                .WithStoreEntityQueryInField(ref _entityQuery)
                .ForEach((
                    Entity unit,
                    int entityInQueryIndex,
                    in TeamTag teamTag) =>
                {
                    aliveTeams.Add((int)teamTag.Value);
                }).Schedule(Dependency);
            
            var check = Entities
                .WithName("CheckGameIsOver")
                .WithReadOnly(aliveTeams)
                .WithDisposeOnCompletion(aliveTeams)
                .WithoutBurst()
                .WithAll<BattleComponent>()
                .WithNone<GameOverMarker>()
                .ForEach((Entity e, int entityInQueryIndex) =>
                {
                    if (aliveTeams.Count() <= 1)
                    {
                        ecb.AddComponent(entityInQueryIndex, e, new GameOverMarker());
                    }
                })
                .Schedule(calculate);
            
            _endSimECBSystem.AddJobHandleForProducer(check);
            Dependency = check;
        }
    }
}