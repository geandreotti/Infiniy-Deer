using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;

public class Guns : Weapon
{
	[BoxGroup("Weapon Settings")] public float shotDelay;
	[BoxGroup("Weapon Settings")] [SerializeField] private float _bulletSpreadModifierMax;
	[Space]
	[BoxGroup("Weapon Settings")] [SerializeField] private Transform _bulletSpawn;
	[BoxGroup("Weapon Settings")] [SerializeField] private GameObject _bulletPrefab;
	[Space]
	public Transform IKTarget;

	public void Shoot()
	{
		float randomX = Random.Range(-_bulletSpreadModifierMax, _bulletSpreadModifierMax);
		float randomY = Random.Range(-_bulletSpreadModifierMax, _bulletSpreadModifierMax);
		float randomZ = Random.Range(-_bulletSpreadModifierMax, _bulletSpreadModifierMax);

		Quaternion desiredRotation = Quaternion.LookRotation(_bulletSpawn.right);

		GameObject bullet;
		if (GameManager.Instance)
			if (GameManager.Instance.online)
			{

				bullet = PhotonNetwork.Instantiate(_bulletPrefab.name, _bulletSpawn.position, desiredRotation);
				bullet.gameObject.SetActive(true);
				bullet.transform.Rotate(randomX, randomY, randomZ);
				bullet.GetComponent<Bullet>().damage = damage;
				return;
			}

		bullet = Instantiate(_bulletPrefab, _bulletSpawn.position, desiredRotation);
		bullet.gameObject.SetActive(true);
		bullet.transform.Rotate(randomX, randomY, randomZ);
		bullet.GetComponent<Bullet>().damage = damage;

	}
}
