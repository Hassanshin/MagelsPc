using System.Collections;
using UnityEngine;
using Unity.Transforms;

public class ShotgunWeapon : BaseWeapon
{
	public override void Reload()
	{
		_currentAmmo = 0;
		
		StartCoroutine(reloadDelay(_maxAmmoReloadTime));
	}
	
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
		var bullet = SpawnEntityBullet(new LocalTransform
		{
			Position = _playerPos,
			Rotation = Quaternion.LookRotation(_playerDirection),
			Scale = 0.25f,
		});
		
		_entityManager.SetComponentData(bullet, new MoveForwardComponent(_bulletSpeed, 0));
	}
}
