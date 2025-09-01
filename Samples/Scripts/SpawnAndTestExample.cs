using UnityEngine;
using UnityEngine.UI;

namespace SpellBound.Controller.Samples {
    public class SpawnAndTestExample : MonoBehaviour {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Button spawnPlayerBtn;
        [SerializeField] private Transform spawnPoint;

        private void Awake() {
            if (playerPrefab == null) 
                Debug.LogError("Player prefab is null", this);
            
            if (spawnPlayerBtn == null)
                spawnPlayerBtn = GetComponent<Button>();
        }

        private void OnEnable() {
            spawnPlayerBtn.onClick.AddListener(SpawnPlayer);
        }

        private void OnDisable() {
            spawnPlayerBtn.onClick.RemoveListener(SpawnPlayer);
        }

        private void SpawnPlayer() {
            playerPrefab = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}