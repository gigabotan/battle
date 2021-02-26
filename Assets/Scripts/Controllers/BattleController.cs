using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace battle
{
    public class BattleController : MonoBehaviour
    {
        private float _startTime;
        
        public UnityEvent<float> GameOverEvent;

        public void Update()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly(typeof(GameOverMarker)));
            if (query.CalculateEntityCount() == 1)
            {
                entityManager.DestroyEntity(query);
                GameOver();
            }
        }

        
        public void StartBattle()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var missionGoalsEntity = entityManager.CreateEntity(typeof(BattleComponent));
            entityManager.AddComponentObject(missionGoalsEntity, this);

            _startTime = Time.time;
        }
        
        public void GameOver()
        {
            Debug.Log("Game over");
            GameOverEvent.Invoke(Time.time - _startTime);
        }
    }
}