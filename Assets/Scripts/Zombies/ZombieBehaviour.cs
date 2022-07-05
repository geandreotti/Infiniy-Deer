using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using Photon.Pun;

public class ZombieBehaviour : MonoBehaviour
{
	public bool active;
	[ProgressBar("Health", 100, EColor.Red)]
	public float health;

	[BoxGroup("Settings")] [SerializeField] private LayerMask _colliders;

	[BoxGroup("Settings")] [SerializeField] private float _attackDistance;
	[BoxGroup("Settings")] [SerializeField] private float _attackDamage;
	[Space]
	[BoxGroup("Settings")] [SerializeField] private float _searchAngle;
	[BoxGroup("Settings")] [SerializeField] private float _searchDistance;
	[BoxGroup("Settings")] [SerializeField] private LayerMask _searchLayers;

	[Space]
	[SerializeField] private ZombieMovement _zombieMovement;
	[SerializeField] private ZombieVisuals _zombieVisuals;

	public GameObject _target;
	private bool _canAttack;
	private PhotonView _photonView;
	public GameObject _tempTarget;

	public event Action initialize;

	public event Action attack;
	public event Action die;
	public event Action<float> doDamage;
	public event Action<float> takeDamage;
	public event Action firstDamage;
	public event Action<GameObject> targetFound;

	private void Start()
	{
		Initialize();
	}

	private void Update()
	{
		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (!PhotonNetwork.IsMasterClient)
					return;

		if (!active)
			return;

		SearchForTarget();
		CheckForAttack();
	}

	public void Initialize()
	{
		if (_zombieMovement != null)
		{
			_zombieMovement.zombieBehaviour = this;
			initialize += _zombieMovement.Initialize;
		}
		if (_zombieVisuals != null)
		{
			_zombieVisuals.zombieBehaviour = this;
			initialize += _zombieVisuals.Initialize;
		}

		TryGetComponent<PhotonView>(out _photonView);

		if (GameRoutineManager.Instance)
			die += GameRoutineManager.Instance.ZombieKilled;

		active = true;
		_canAttack = true;
		health = 100;

		initialize?.Invoke();
	}

	private void SearchForTarget()
	{
		if (_target != null)
			return;

		Vector3 forward = transform.forward;
		Vector3 origin = transform.position;

		foreach (PlayerBehaviour player in GameManager.Instance.players)
		{
			Vector3 playerPosition = player.transform.position;
			Vector3 direction = playerPosition - origin;

			float angle = Vector3.Angle(forward, direction);
			RaycastHit hit;
			if (angle < _searchAngle)
			{
				if (Physics.Raycast(origin, direction, out hit, _searchDistance, _searchLayers))
					if (hit.transform.gameObject == player.gameObject)
					{
						_tempTarget = player.gameObject;

						if (GameManager.Instance)
						{
							if (GameManager.Instance.online)
							{
								if (_photonView)
									if (_photonView.IsMine)
										_photonView.RPC("TargetFound", RpcTarget.AllBuffered);
							}
							else
								TargetFound();
						}
						else
							TargetFound();

					}
			}

		}
	}

	[PunRPC]
	private void TargetFound()
	{
		_target = _tempTarget;
		doDamage += _target.GetComponent<PlayerBehaviour>().TakeDamage;
		targetFound?.Invoke(_tempTarget);
	}

	private void CheckForAttack()
	{

		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (!PhotonNetwork.IsMasterClient)
					return;

		if (_target == null)
			return;

		float distanceToTarget = Vector3.Distance(transform.position, _target.transform.position);
		if (distanceToTarget < _attackDistance)
			Attack();
	}

	private void Attack()
	{
		if (!_canAttack)
			return;

		_canAttack = false;
		attack?.Invoke();
	}

	public void AttackCallBack()
	{
		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (!PhotonNetwork.IsMasterClient)
					return;

		float distanceToTarget = Vector3.Distance(transform.position, _target.transform.position);
		if (distanceToTarget < _attackDistance)
			doDamage?.Invoke(_attackDamage);
		_canAttack = true;
	}

	[PunRPC]
	public void Damage(float damageValue)
	{
		health -= damageValue;

		if (health + damageValue == 100)
			firstDamage?.Invoke();

		if (GameManager.Instance)
		{
			if (GameManager.Instance.online)
			{
				if (PhotonNetwork.IsMasterClient)
					if (health <= 0)
						_photonView.RPC("Die", RpcTarget.AllBuffered);
				return;
			}
		}

		if (health <= 0)
			Die();
	}

	[PunRPC]
	private void Die()
	{
		active = false;
		die?.Invoke();
	}

	public void OnTriggerEnter(Collider other)
	{
		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (!PhotonNetwork.IsMasterClient)
					return;

		if (_colliders != (_colliders | (1 << other.gameObject.layer)) || !active)
			return;

		Bullet bullet;
		Weapon weapon;
		other.transform.parent.TryGetComponent<Bullet>(out bullet);
		other.TryGetComponent<Weapon>(out weapon);

		float damage = 0;

		if (bullet)
			damage = bullet.damage;

		if (weapon)
			damage = weapon.damage;

		if (GameManager.Instance)
			if (GameManager.Instance.online)
			{
				_photonView.RPC("Damage", RpcTarget.AllBuffered, damage);
				if (bullet)
					PhotonNetwork.Destroy(bullet.gameObject);
				return;
			}

		Damage(damage);
		if (bullet)
			Destroy(bullet.gameObject);
	}

	private void OnDrawGizmos()
	{
		float rayRange = 5f;
		Quaternion leftRayRotation = Quaternion.AngleAxis(-_searchAngle, transform.up);
		Quaternion rightRayRotation = Quaternion.AngleAxis(_searchAngle, transform.up);
		Vector3 leftRayDirection = leftRayRotation * transform.forward;
		Vector3 rightRayDirection = rightRayRotation * transform.forward;
		Gizmos.DrawRay(transform.position, leftRayDirection * rayRange);
		Gizmos.DrawRay(transform.position, rightRayDirection * rayRange);
	}
}
