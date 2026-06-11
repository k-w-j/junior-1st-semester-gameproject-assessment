using System;
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
        [SerializeField] private Transform moveSpawnPoint;
        [Space] 
        [SerializeField] private float speed;
        [SerializeField] private float spawnInterval = 2.0f;
		[SerializeField] private bool isAuto = true;

        public Transform SpawnHeight => spawnHeight;
        public List<GameObject> SpawnedBlocks { get; private set; }

        private Rigidbody _currentBlock;

        private bool _canSpawn = true;
        private float _timer;
        private float _timer2;

        private void Awake()
        {
            SpawnedBlocks =  new List<GameObject>();
        }

        private void Start()
        {
            if (!isAuto)
            {
                SpawnRandomBlockNotAuto();
            }
        }

        private void Update()
        {
            float minRangeX = minRange.position.x;
            float maxRangeX = maxRange.position.x;
            float moveSpawnPointX = moveSpawnPoint.transform.position.x;
                
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_currentBlock != null)
                {
                    _currentBlock.constraints = RigidbodyConstraints.None;
                    _currentBlock = null;
                }
                _canSpawn = false;
            }
                    
            if (Input.GetKey(KeyCode.LeftArrow) && minRangeX < moveSpawnPointX)
            {
                moveSpawnPoint.transform.position += Vector3.left * (Time.deltaTime * speed);
            }
                    
            if (Input.GetKey(KeyCode.RightArrow) && maxRangeX > moveSpawnPointX)
            {
                moveSpawnPoint.transform.position += Vector3.right * (Time.deltaTime * speed);
            }

            if (_currentBlock != null)
            {
                _currentBlock.transform.position = moveSpawnPoint.transform.position;
            }
            
            _timer += Time.deltaTime;
            
            if (_timer >= spawnInterval)
            {
                if (isAuto)
                {
                    SpawnRandomBlock();
                }

                _timer = 0f;
            }

            if (_canSpawn == false)
            {
                _timer2 += Time.deltaTime;
                
                if (_timer2 >= spawnInterval)
                {
                    if (isAuto == false)
                    {
                        SpawnRandomBlockNotAuto();
                    }

                    _canSpawn = true;
                    _timer2 = 0f;
                }
            }
        }
        
        private void SpawnRandomBlockNotAuto()
        {
            if (blockListData.blockPrefabs.Length == 0) return;
            
            int randomIndex = Random.Range(0, blockListData.blockPrefabs.Length);
            _currentBlock = Instantiate(blockListData.blockPrefabs[randomIndex]).GetComponent<Rigidbody>();
            _currentBlock.constraints = RigidbodyConstraints.FreezeAll;
            
            SpawnedBlocks.Add(_currentBlock.gameObject);
        }

        private void SpawnRandomBlock()
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