using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace battle
{
    struct Damage : IComponentData
    {
        public float Value;
    }

    struct AttackRange : IComponentData
    {
        public float Value;
    }

    struct Health: IComponentData
    {
        public float Value;
    }
    

    public class AttackSystem: SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _endSimECBSystem;
        
        protected override void OnCreate()
        {
            _endSimECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _endSimECBSystem.CreateCommandBuffer().AsParallelWriter();
            var deltaTime = Time.DeltaTime;
            
            var attackHandle = Entities
                .WithName("AttackSystem")
                .WithNone<MoveMarker>()
                    .ForEach((
                        Entity unit,
                        int entityInQueryIndex,
                        ref Rotation rot,
                        in Translation pos,
                        in Target target,
                        in AttackRange attackRange,
                        in Damage damage,
                        in TeamTag teamTag) =>
                    {
            
                        if (HasComponent<TeamTag>(target.Value))
                        {
                            var targetPos = GetComponent<Translation>(target.Value);
                            var vectorToTarget = targetPos.Value - pos.Value;
                            if (math.length(vectorToTarget) > attackRange.Value)
                            {
                                UnitAIUtility.TransitionToMove(ecb, unit, entityInQueryIndex);
                                return;
                            }
                            
                            var moveDirection = math.normalize(vectorToTarget);
                            rot.Value = quaternion.LookRotation(new float3(moveDirection.x, 0.0f, moveDirection.z),
                                math.up());
            
                            var color = teamTag.Value == Team.Blue ? Color.blue : Color.red;
                            Debug.DrawLine(new Vector3(pos.Value.x, pos.Value.y, pos.Value.z),
                                new Vector3(targetPos.Value.x, targetPos.Value.y, targetPos.Value.z),
                                color); // sorry for this
            
                            var health = GetComponent<Health>(target.Value);
                            SetComponent(target.Value, new Health() {Value = health.Value - damage.Value * deltaTime});
                            return;
            
                        }
                        UnitAIUtility.TransitionFromAttack(ecb, unit, entityInQueryIndex);
            
                    }).Schedule(Dependency);
            
            _endSimECBSystem.AddJobHandleForProducer(attackHandle);
            Dependency = attackHandle;
        }
    }
}