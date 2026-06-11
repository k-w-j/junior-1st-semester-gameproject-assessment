using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Blocks
{
    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private BlockListDataSO blockListData;
        [Space]
        [SerializeField] private Transform spawnHeight;
        [SerializeField] private Transform maxRange;
        [SerializeField] private Transform minRange;
        [Space]
        [SerializeField] private float spawnInterval = 2.0f;
		[SerializeField] private bool isAuto = true;

        public Transform SpawnHeight => spawnHeight;
        public List<GameObject> SpawnedBlocks { get; private set; }
        
        private float _timer;

        private void Awake()
        {
            SpawnedBlocks =  new List<GameObject>();
        }

        private void Update()
        {
			if(isAuto == false)
				return;

            _timer += Time.deltaTime;
            if (_timer >= spawnInterval)
            {
                SpawnRandomBlock();
                _timer = 0f;
            }
        }

        public void SpawnRandomBlock()
        {
            if (blockListData.blockPrefabs.Length == 0) return;
            
            float randomX = Random.Range(minRange.position.x, maxRange.position.x);
            Vector3 spawnPos = new Vector3(randomX, spawnHeight.position.y, spawnHeight.position.z);
            
            int randomIndex = Random.Range(0, blockListData.blockPrefabs.Length);
            
            GameObject newBlock = Instantiate(blockListData.blockPrefabs[randomIndex],
                spawnPos, Quaternion.identity);
            
            SpawnedBlocks.Add(newBlock);
        }
        
        public void ResetEnvironment()
        {
            foreach (GameObject block in SpawnedBlocks)
            {
                if (block != null) Destroy(block);
            }
            SpawnedBlocks.Clear();
            _timer = 0f;
        }
    }
}