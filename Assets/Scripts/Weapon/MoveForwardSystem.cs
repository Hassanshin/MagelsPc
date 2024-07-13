using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

	public partial struct MoveForwardSystem : ISystem
	{
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<MoveForwardComponent>();
			// state.RequireForUpdate<YOUR_DATA_COMPONENT>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var deltaTime = SystemAPI.Time.DeltaTime;
			
			JobHandle MoveForwardSystemJobHandle = new MoveForwardSystemJob
			{
				DeltaTime = deltaTime,
				
			}.ScheduleParallel(state.Dependency);
			
			MoveForwardSystemJobHandle.Complete();
		}
	}

	[BurstCompile]
	public partial struct MoveForwardSystemJob : IJobEntity
	{
		public float DeltaTime;
		
		[BurstCompile]
		public void Execute(Entity owner, [ChunkIndexInQuery] int chunkIndex,
			ref MoveForwardComponent moveForward, ref LocalTransform localTransform)
		{
			localTransform.Position += DeltaTime * moveForward.Speed * localTransform.Forward();
		}
	}

