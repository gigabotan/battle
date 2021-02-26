using Unity.Entities;
using UnityEngine;

namespace battle
{
    public struct Target : IComponentData
    {
        public Entity Value;
    }

    public enum Team
    {
        Red,
        Blue
    }
    
    public struct TeamTag : IComponentData
    {
        public Team Value;
    }
    

    [DisallowMultipleComponent]
    public class UnitAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float AttackRange = 5.0f;
        public float Speed = 3.0f;
        public float Damage = 1.0f;
        public float Health = 10f;
        public Team Team;
    
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponents(entity, new ComponentTypes(
                new ComponentType[]
                {
                    typeof(MovementSpeed),
                    typeof(Damage),
                    typeof(AttackRange),
                    typeof(Health),
                    typeof(TeamTag)
                }));
            
            dstManager.SetComponentData(entity, new MovementSpeed {MetersPerSecond = Speed});
            dstManager.SetComponentData(entity, new Damage() {Value = Damage});
            dstManager.SetComponentData(entity, new AttackRange() {Value = AttackRange});
            dstManager.SetComponentData(entity, new TeamTag() {Value = Team});
            dstManager.SetComponentData(entity, new Health() {Value = Health});
        }
    }
}

