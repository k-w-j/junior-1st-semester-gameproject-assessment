using UnityEngine;

namespace Code.Blocks
{
    [CreateAssetMenu(fileName = "BlockListDataSO", menuName = "SO/BlockListDataSO", order = 0)]
    public class BlockListDataSO : ScriptableObject
    {
        public GameObject[] blockPrefabs;
    }
}