using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace battle
{
    public class GameManager : MonoBehaviour
    {
        
        private EntityManager _entityManager;
        
        public Text FinalText = null;
        public GameObject Panel = null;
        public BattleController BattleController = null;

        private void Awake()
        {
            BattleController.GameOverEvent.AddListener(GameOver);
        }


        public void GameOver(float finalTime)
        {
            Panel.SetActive(true);
            FinalText.text += finalTime;
        }
        
    }
}
