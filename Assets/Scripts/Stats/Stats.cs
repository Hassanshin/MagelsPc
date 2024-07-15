using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Hash.Stats
{
	[System.Serializable]
	public struct Stats 
	{
		[SerializeField]
		private float _value;
		public float Value => _value;
		
		[SerializeField]
		private float _defaultValue;
		public float DefaultValue => _defaultValue;
		
		[SerializeField]
		private float2 _minMax;
		public float Max => _minMax.y;
		public float Min => _minMax.x;
		
		[SerializeField]
		public float PercentageValue => _value / Max;
		
		private const float MAX_NUMBER = 1e+6f;
		
		/// <summary>
		/// Add value to stats
		/// </summary>
		/// <param name="amount"></param>
		public float Add(float amount)
		{
			_value += amount;
			_value = math.clamp(_value, _minMax.x, _minMax.y);
			
			return _value;
		}
		
		/// <summary>
		/// Set stats value
		/// </summary>
		/// <param name="_amount"></param>
		public float Set(float _amount)
		{
			_value = _amount;
			_value = math.clamp(_value, _minMax.x, _minMax.y);
			
			return _value;
		}
		
		/// <summary>
		/// Multiply stats value from current amount
		/// </summary>
		/// <param name="_amount"></param>
		/// <returns></returns>
		public float Multiply(float _amount)
		{
			_value *= _amount;
			_value = math.clamp(_value, _minMax.x, _minMax.y);
			
			return _value;
		}
		
		/// <summary>
		/// Extend the clamp value
		/// </summary>
		/// <param name="minMaxAmount"></param>
		/// <returns></returns>
		public float ExtendMinMax(float2 minMaxAmount)
		{
			_minMax += minMaxAmount;
			// multiplier = 1;
			
			return _value;
		}

		/// <summary>
		/// Reset the stats to its default
		/// </summary>
		public float Reset()
		{
			// multiplier = 1;
			_value = _defaultValue;
			
			return _value;
		}
		
		/// <summary>
		/// Add value and extending the clamp value
		/// </summary>
		/// <param name="amount"></param>
		/// <param name="extendMinMax"></param>
		public float AddAndExtend(float _amount, float2 _extendMinMax)
		{
			_minMax += _extendMinMax;
			
			_value += _amount;
			_value = math.clamp(_value, _minMax.x, _minMax.y);
			
			return _value;
		}
		
		#region Static Methods
		
		public static Stats InitHealth(float v)
		{
			return new Stats
			{
				_defaultValue = v,
				_minMax = new float2(0, v),
				// multiplier = 1,
				_value = v,
			};
		}
		
		public static Stats InitHealthEmpty(float v)
		{
			return new Stats
			{
				_defaultValue = v,
				_minMax = new float2(0, v),
				// multiplier = 1,
			};
		}
		
		public static Stats InitNullifyLayer(float v)
		{
			return new Stats
			{
				_defaultValue = 0,
				_minMax = new float2(0, MAX_NUMBER),
				// multiplier = 1,
				_value = v,
			};
		}
		
		public static Stats InitMultiplier(float v)
		{
			return new Stats
			{
				_defaultValue = v,
				_minMax = new float2(0, MAX_NUMBER),
				// multiplier = 1,
				_value = v,
			};
		}
		
		public static Stats Init(float v)
		{
			return new Stats
			{
				_value = v,
				_defaultValue = v,
				_minMax = new float2(-MAX_NUMBER, MAX_NUMBER),
				// multiplier = 1,
			};
		}
		
		#endregion
	}
}
