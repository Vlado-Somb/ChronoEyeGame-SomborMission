// TimelineBlockDatabase.cs
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Timeline
{
    [CreateAssetMenu(menuName = "GameData/Timeline Block Database")]
    public class TimelineBlockDatabase : ScriptableObject
    {
        public List<TimelineBlockSO> blocks = new();

        public string[] GetBlockLabels()
        {
            if (blocks == null || blocks.Count == 0) return new string[] { "<none>" };

            var labels = new List<string>();
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                if (block == null)
                    labels.Add($"(Null Block {i})");
                else
                    labels.Add(string.IsNullOrEmpty(block.blockId) ? block.name : block.blockId);
            }
            return labels.ToArray();
        }

        public TimelineBlockSO GetBlockByIndex(int index)
        {
            if (index < 0 || index >= blocks.Count) return null;
            return blocks[index];
        }

        public int GetBlockCount() => blocks?.Count ?? 0;

        public int FindIndexByBlockId(string id)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                if (block != null && block.blockId == id)
                    return i;
            }
            return -1;
        }
<<<<<<< Updated upstream

        public string[] GetBlockIds()
        {
            if (blocks == null || blocks.Count == 0) return new string[0];

            var ids = new List<string>();
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                ids.Add(block != null && !string.IsNullOrEmpty(block.blockId) ? block.blockId : $"Unnamed_{i}");
            }
            return ids.ToArray();
        }

=======
>>>>>>> Stashed changes
    }
}