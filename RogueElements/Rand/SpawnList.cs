﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace RogueElements
{

    [Serializable]
    public class SpawnList<T> : IRandPicker<T>
    {
        [Serializable]
        private class SpawnRate
        {
            public T Spawn;
            public int Rate;

            public SpawnRate(T item, int rate)
            {
                Spawn = item;
                Rate = rate;
            }
        }

        private List<SpawnRate> spawns;
        private int spawnTotal;

        public int Count { get { return spawns.Count; } }
        public int SpawnTotal { get { return spawnTotal; } }
        public bool CanPick { get { return spawnTotal > 0; } }
        public bool ChangesState { get { return false; } }

        public SpawnList()
        {
            spawns = new List<SpawnRate>();
        }

        public void Add(T spawn)
        {
            Add(spawn, 10);
        }
        public void Add(T spawn, int rate)
        {
            if (rate < 0)
                throw new ArgumentException("Spawn rate must be 0 or higher.");
            spawns.Add(new SpawnRate(spawn, rate));
            spawnTotal += rate;
        }

        public void Clear()
        {
            spawns.Clear();
            spawnTotal = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (SpawnRate element in spawns)
                yield return element.Spawn;
        }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public T Pick(IRandom random)
        {
            int ii = PickIndex(random);
            return spawns[ii].Spawn;
        }

        public int PickIndex(IRandom random)
        {
            if (spawnTotal > 0)
            {
                int rand = random.Next(spawnTotal);
                int total = 0;
                for (int ii = 0; ii < spawns.Count; ii++)
                {
                    total += spawns[ii].Rate;
                    if (rand < total)
                        return ii;
                }
            }
            throw new InvalidOperationException("Cannot spawn from a spawnlist of total rate 0!");
        }
        
        public T GetSpawn(int index)
        {
            return spawns[index].Spawn;
        }

        public int GetSpawnRate(T spawn)
        {
            for (int ii = 0; ii < spawns.Count; ii++)
            {
                if (spawns[ii].Spawn.Equals(spawn))
                    return spawns[ii].Rate;
            }
            return 0;
        }

        public int GetSpawnRate(int index)
        {
            return spawns[index].Rate;
        }

        public void SetSpawn(int index, T spawn)
        {
            spawns[index].Spawn = spawn;
        }

        public void SetSpawnRate(int index, int rate)
        {
            if (rate < 0)
                throw new ArgumentException("Spawn rate must be 0 or higher.");
            spawnTotal = spawnTotal - spawns[index].Rate + rate;
            spawns[index].Rate = rate;
        }
        
        public void RemoveAt(int index)
        {
            spawnTotal -= spawns[index].Rate;
            spawns.RemoveAt(index);
        }

        public override bool Equals(object obj)
        {
            SpawnList<T> other = obj as SpawnList<T>;
            if (other == null)
                return false;
            if (spawns.Count != other.spawns.Count)
                return false;
            for (int ii = 0; ii < spawns.Count; ii++)
            {
                if (!spawns[ii].Spawn.Equals(other.spawns[ii].Spawn))
                    return false;
                if (spawns[ii].Rate != other.spawns[ii].Rate)
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int code = 0;
            for (int ii = 0; ii < spawns.Count; ii++)
                code ^= spawns[ii].Spawn.GetHashCode() ^ spawns[ii].Rate;
            return code;
        }
    }
}