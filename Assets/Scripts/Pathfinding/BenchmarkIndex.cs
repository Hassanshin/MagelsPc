using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

public class BenchmarkSystem : MonoBehaviour
{
	private const int INITIAL_CAPACITY = 10000;
	private NativeParallelHashMap<int2, PathNodeTest> _int2Map;
	private NativeParallelHashMap<int, PathNodeTest> _intMap;
	private NativeParallelHashSet<PathNodeTest> _hashSet;
	private NativeList<PathNodeTest> _list;
	
	/// <summary>
	/// ReadHashMapInt2 Job 	0.55ms
	/// ReadHashSet Job         0.44ms
	/// ReadHashMapInt Job		0.36ms
	/// ReadArray Job 	        0.08ms
	/// </summary>
	
	public void Start()
	{
		_int2Map = new NativeParallelHashMap<int2, PathNodeTest>(INITIAL_CAPACITY, Allocator.Persistent);
		_intMap = new NativeParallelHashMap<int, PathNodeTest>(INITIAL_CAPACITY, Allocator.Persistent);
		_list = new NativeList<PathNodeTest>(INITIAL_CAPACITY, Allocator.Persistent);
		_hashSet = new NativeParallelHashSet<PathNodeTest>(INITIAL_CAPACITY, Allocator.Persistent);

		PopulateMaps();

		BenchmarkReadOperations();
	}

	public void OnDestroy()
	{
		_int2Map.Dispose();
		_intMap.Dispose();
	}

	public void PopulateMaps()
	{
		for (int x = 0; x < 100; x++)
		{
			for (int y = 0; y < 100; y++)
			{
				int2 position = new int2(x, y);
				PathNodeTest pathNode = new()
				{
					Pos = position,
					HCost = UnityEngine.Random.Range(0.0f, 10.0f),
					GCost = UnityEngine.Random.Range(0.0f, 10.0f),
					FCost = UnityEngine.Random.Range(0.0f, 20.0f),
					ComeFromIndex = new int2(-1, -1),
					IsWalkable = true
				};
				_int2Map.Add(position, pathNode);
				int index = x * 100 + y;
				_intMap.Add(index, pathNode);
				_list.AddNoResize(pathNode);
				_hashSet.Add(pathNode);
			}
		}
	}
	
	[System.Serializable]
	public struct PathNodeTest : System.IEquatable<PathNodeTest>
	{
		public float2 Pos;
		
		public int Index;
		public float GCost;
		public float HCost;
		public float FCost;
		
		public bool IsWalkable;
		public int2 ComeFromIndex;
		
		public void CalculateFCost()
		{
			FCost = GCost + FCost;
		}

		public bool Equals(PathNodeTest other)
		{
			return Pos.Equals(other.Pos);
		}

		public override string ToString()
		{
			return Pos.ToString() + "." + Index;
		}
	
	}

	public void BenchmarkReadOperations()
	{
		NativeArray<float> int2ReadTimes = new NativeArray<float>(1, Allocator.TempJob);
		NativeArray<float> intReadTimes = new NativeArray<float>(1, Allocator.TempJob);

		var int2Job = new ReadInt2Job
		{
			Map = _int2Map.AsReadOnly(),
			ReadTimes = int2ReadTimes,
			StartTime = UnityEngine.Time.realtimeSinceStartup,
		};

		var intJob = new ReadIntJob
		{
			Map = _intMap.AsReadOnly(),
			ReadTimes = intReadTimes,
			StartTime = UnityEngine.Time.realtimeSinceStartup,
		};
		
		var hashSetJob = new ReadHashSetJob
		{
			Set = _hashSet.AsReadOnly(),
		};
		
		var listJob = new ReadListJob
		{
			List = _list.AsReadOnly(),
		};

		JobHandle int2Handle = int2Job.Schedule();
		JobHandle intHandle = intJob.Schedule();
		JobHandle hashSetHandle = hashSetJob.Schedule();
		JobHandle listJobHandle = listJob.Schedule();
		
		JobHandle.CompleteAll(ref int2Handle, ref intHandle);
		
		int2ReadTimes.Dispose();
		intReadTimes.Dispose();
	}

	private struct ReadHashSetJob : IJob
	{
		[ReadOnly]
		public NativeParallelHashSet<PathNodeTest>.ReadOnly Set;
		public void Execute()
		{
			foreach (var node in Set)
			{
				// Simulate read operation
				float gCost = node.GCost;
			}
		}
	}

	private struct ReadListJob : IJob
	{
		[ReadOnly]
		public NativeArray<PathNodeTest>.ReadOnly List;
		
		public void Execute()
		{
			for (int i = 0; i < List.Length; i++)
			{
				float gCost = List[i].GCost;
			}
		}
	}

	private struct ReadInt2Job : IJob
	{
		[ReadOnly]
		public NativeParallelHashMap<int2, PathNodeTest>.ReadOnly Map;
		public NativeArray<float> ReadTimes;
		public float StartTime;
		
		public void Execute()
		{
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					int2 key = new int2(x, y);
					if (Map.TryGetValue(key, out PathNodeTest node))
					{
						float gCost = node.GCost;
					}
				}
			}

		}
	}

	private struct ReadIntJob : IJob
	{
		[ReadOnly]
		public NativeParallelHashMap<int, PathNodeTest>.ReadOnly Map;
		public NativeArray<float> ReadTimes;
		public float StartTime;

		public void Execute()
		{
			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					int key = x * 100 + y;
					if (Map.TryGetValue(key, out PathNodeTest node))
					{
						float gCost = node.GCost;
					}
				}
			}

		}
	}
}
