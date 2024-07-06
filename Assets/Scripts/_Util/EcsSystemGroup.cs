using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Scenes;
using Unity.Transforms;

namespace Hash.Util
{
	public class EcsSystemGroup
	{
	   
	}
	
	// [UpdateInGroup(typeof(InitializationSystemGroup))]
	// public partial class TomomiWeaponInitializationSystemGroup : ComponentSystemGroup 
	// {
		
	// }
	
	public partial class TomomiWeaponSystemGroup : ComponentSystemGroup 
	{
		
	}
	
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(SceneSystemGroup))]
	public partial class TomomiInitializationSystemGroup : ComponentSystemGroup
	{
		
	}
	
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateBefore(typeof(TransformSystemGroup))]
	public partial class TomomiCoreSystemGroup : ComponentSystemGroup
	{
		
	}
	
	[UpdateInGroup(typeof(TransformSystemGroup), OrderFirst = true)]
	public partial class TomomiTransformSystemGroup : ComponentSystemGroup
	{
		
	}
	
	public partial class TomomiTriggerSystemGroup : ComponentSystemGroup
	{
		
	}
	
	[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
	[UpdateBefore(typeof(BeginSimulationSystemGroup))]
	public partial class DestroySystemGroup : ComponentSystemGroup
	{
	}
	
	[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
	[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
	[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
	[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
	public partial class BeginSimulationSystemGroup : ComponentSystemGroup
	{
	}

	[UpdateAfter(typeof(TransformSystemGroup))]
	public partial class EndSimulationSystemGroup : ComponentSystemGroup
	{
	}
}
