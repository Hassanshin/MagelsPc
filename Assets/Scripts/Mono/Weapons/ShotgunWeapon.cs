using System.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Hash.HashMap;

public class ShotgunWeapon : BaseWeapon
{
	
	
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
		float angleOffset = MaxAngle / (TotalShot > 1 ? TotalShot - 1 : 1); // Prevent division by zero

        for (int i = 0; i < TotalShot; i++)
        {
            float angle = -MaxAngle / 2 + i * angleOffset;
            quaternion rotation = math.mul(
                quaternion.LookRotation(_playerDirection, math.up()),
                quaternion.Euler(new float3(0, math.radians(angle), 0))
            );

            // Normalize the quaternion to ensure it's always valid
            rotation = math.normalize(rotation);

            var bullet = SpawnEntityBullet(new LocalTransform
            {
                Position = _playerPos,
                Rotation = rotation,
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
