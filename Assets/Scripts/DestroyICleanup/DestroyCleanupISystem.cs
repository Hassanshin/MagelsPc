using Unity.Entities;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Transforms;
using Hash.Util;

namespace Tertle.DestroyCleanup
{
	[UpdateInGroup(typeof(DestroySystemGroup), OrderLast = true)]
	public partial struct DestroyCleanupISystem : ISystem
	{
		private EntityQuery _query;
		private EntityQuery _destroyAndSpawningQuery;

		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			// state.RequireForUpdate<InGameRunningComponent>();
			state.RequireForUpdate<DestroyTag>();
			
#if UNITY_NETCODE
			// Client doesn't destroy ghosts, instead we'll disable them in
			this.query = Unity.NetCode.ClientServerWorldExtensions.IsClient(state.WorldUnmanaged)
				? SystemAPI.QueryBuilder().WithAll<DestroyEntity>().WithNone<Unity.NetCode.GhostInstance>().Build()
				: SystemAPI.QueryBuilder().WithAll<DestroyEntity>().Build();
#else
			this._query = SystemAPI.QueryBuilder().WithAll<DestroyTag>().Build();
#endif
			this._query.SetChangedVersionFilter(ComponentType.ReadOnly<DestroyTag>());
		}

		/// <inheritdoc />
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var bufferSingleton = SystemAPI.GetSingleton<DestroyEntityCommandBufferSystem.Singleton>();
			
			var jobHandleDestroyDuration = new DecreaseDestroyDurationJob
			{
				CommandBuffer = bufferSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
				DeltaTime = SystemAPI.Time.DeltaTime
			}.ScheduleParallel(state.Dependency);
			
			jobHandleDestroyDuration.Complete();
			
			new DestroyJob 
			{
				CommandBuffer = bufferSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
			}.ScheduleParallel(this._query);
			
		}

		[WithChangeFilter(typeof(DestroyTag))]
		[WithAll(typeof(DestroyTag))]
		[BurstCompile]
		private partial struct DestroyJob : IJobEntity
		{
			public EntityCommandBuffer.ParallelWriter CommandBuffer;
			public void Execute([ChunkIndexInQuery] int chunkIndexInQuery, Entity entity)
			{
				this.CommandBuffer.DestroyEntity(chunkIndexInQuery, entity);
			}
		}

		[BurstCompile]
		private partial struct DecreaseDestroyDurationJob : IJobEntity
		{
			public EntityCommandBuffer.ParallelWriter CommandBuffer;

			[ReadOnly]
			public float DeltaTime;
			
			public void Execute([ChunkIndexInQuery] int chunkIndexInQuery, Entity entity, ref DestroyByDurationComponent destroyByDurationComponent)
			{
				destroyByDurationComponent.Duration -= DeltaTime;
				
				if (destroyByDurationComponent.Duration < 0)
				{
					CommandBuffer.SetComponentEnabled<DestroyTag>(chunkIndexInQuery, entity, true);
				}
			}
		}
	}
}

