using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace battle
{
    public static class UnitAIUtility
    {
        public static void TransitionFromMove(EntityCommandBuffer.ParallelWriter ecb, Entity e, int index)
        {
            ecb.RemoveComponent<MoveMarker>(index, e);
        }
        
        public static void TransitionFromAttack(EntityCommandBuffer.ParallelWriter ecb, Entity e, int index)
        {
            ecb.RemoveComponent<Target>(index, e);
        }
        
        public static void TransitionToMove(EntityCommandBuffer.ParallelWriter ecb, Entity e, int index)
        {
            ecb.AddComponent<MoveMarker>(index, e); ;
        }
        
        public static void TransitionToAttack(EntityCommandBuffer.ParallelWriter ecb, Entity e, int index, Entity target)
        {
            ecb.AddComponent(index, e, new Target() {Value = target});
        }
    }
}