using System.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Hash.HashMap;

public class ShotgunWeapon : BaseWeapon
{
	[Header("shotgun")]
	[SerializeField]
	private int _totalShot = 10;
	
	[SerializeField]
	private float _maxAngle = 10;
	
	public override void Reload()
	{
		_currentAmmo = 0;
		
		StartCoroutine(reloadDelay(_maxAmmoReloadTime));
	}
	
	// public void OnDrawGizmos()
	// {
	//     quaternion playerQuaternion = quaternion.LookRotation(_playerDirection, math.up());
	//     quaternion debugDir = math.mul(playerQuaternion, quaternion.Euler(0, math.radians(_maxAngle), 0));
	//     Debug.DrawRay(transform.position, math.mul(debugDir, math.forward()) * 5, Color.green);
	// }
	
	private IEnumerator reloadDelay(float delay)
	{
		_currentReloadTime = delay;
		while (_currentReloadTime > 0)
		{
			_currentReloadTime -= 0.1f;
			yield return new WaitForSeconds(0.1f);
		}
		_currentAmmo = _maxAmmo;
	}

	public override void Shoot()
	{
		float angleOffset = _maxAngle / (_totalShot - 1);
		for (int i = 0; i < _totalShot; i++)
		{
			float angle = -_maxAngle / 2 + i * angleOffset;
			
			var bullet = SpawnEntityBullet(new LocalTransform
			{
				Position = _playerPos,
				Rotation = math.mul(
					quaternion.LookRotation(_playerDirection, math.up()), 
					quaternion.Euler(new float3(0, math.radians(angle), 0))),
				Scale = 0.25f,
			});
			
		}


	}

	public override void OnHit(HitData data)
	{
		Vector3 pos = new Vector3(data.Pos.x, 0, data.Pos.y);
		GameObject vfx = data.IsKilling ? 
			SpawnVfxKill(pos, Quaternion.identity):
			SpawnVfxHit(pos, Quaternion.identity);
	}
}
