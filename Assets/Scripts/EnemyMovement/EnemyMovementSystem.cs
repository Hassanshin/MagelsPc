using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Hash.Util;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
	public partial struct EnemyMovementSystem : ISystem
	{
		private GridSingleton _gridSingleton;
		private Entity _player;
		private float3 _playerPos;
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<EnemyTag>();
			state.RequireForUpdate<GridSingleton>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var deltaTime = SystemAPI.Time.DeltaTime;
			SystemAPI.TryGetSingleton(out _gridSingleton);
			SystemAPI.TryGetSingletonEntity<PlayerTag>(out _player);
			_playerPos = SystemAPI.GetComponent<LocalTransform>(_player).Position;
			
			JobHandle EnemyMovementSystemJobHandle = new EnemyMovementSystemJob
			{
				DeltaTime = deltaTime,
				GridSingleton = _gridSingleton,
				PlayerPos = _playerPos,
			}.ScheduleParallel(state.Dependency);
			
			EnemyMovementSystemJobHandle.Complete();
		}
	}

	[BurstCompile]
	public partial struct EnemyMovementSystemJob : IJobEntity
	{
		public float DeltaTime;
		public GridSingleton GridSingleton;
		public float3 PlayerPos;
		
		[BurstCompile]
		public void Execute(Entity owner, [ChunkIndexInQuery] int chunkIndex, ref LocalTransform localTransform,
			DynamicBuffer<AgentPathBuffer> buffers, ref AgentMoveComponent moveComponent, in AgentPathComponent pathComponent)
		{
			if (moveComponent.IsStopped) { return; }

            float2 destination = pathComponent.Destination;
            
            if (GridSingleton.GetIdFromPos(localTransform.Position.xz) == GridSingleton.GetIdFromPos(destination))
			{
				moveComponent.DistanceSqLeft = math.distancesq(localTransform.Position, pathComponent.Destination3D);
				if (moveComponent.DistanceSqLeft <= 1E-2)
				{
					moveComponent.IsStopped = true;
				}
			}
			
			float3 next;
			if (buffers.Length >= 1)
			{
				var nextPath = buffers[buffers.Length - 2];
				next = new float3(nextPath.Value.x, 0, nextPath.Value.y);
			}
			else
			{
				next = pathComponent.Destination3D;
			}
			
			moveComponent.Direction = math.normalize(next - localTransform.Position);
			localTransform.Position += DeltaTime * moveComponent.Speed * moveComponent.Direction;
			
			localTransform.Rotation = quaternion.LookRotation(moveComponent.Direction, math.up());
		}
	}

