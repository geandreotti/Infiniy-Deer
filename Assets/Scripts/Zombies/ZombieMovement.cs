using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;
using Photon.Pun;

public class ZombieMovement : MonoBehaviour
{

	[BoxGroup("Settings")] [SerializeField] private float _moveSpeed;
	[BoxGroup("Settings")] [SerializeField] private float _gravitySpeed;
	[Space]
	[BoxGroup("Settings")] [SerializeField] private float _turnSpeed;

	private GameObject _target;
	private bool _active;
	private CharacterController _characterController;
	private NavMeshAgent _navMeshAgent;
	private PhotonView _photonView;

	public ZombieBehaviour zombieBehaviour;

	public void Initialize()
	{
		_active = true;
		TryGetComponent<CharacterController>(out _characterController);
		TryGetComponent<NavMeshAgent>(out _navMeshAgent);
		TryGetComponent<PhotonView>(out _photonView);

		_navMeshAgent.SetDestination(Vector3.zero);

		zombieBehaviour.targetFound += TargetFound;
		zombieBehaviour.die += Die;

		if (!_navMeshAgent)
			return;
		_navMeshAgent.updatePosition = false;
		_navMeshAgent.updateRotation = false;
	}

	private void Update()
	{
		if (!_active)
			return;

		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (!PhotonNetwork.IsMasterClient)
					return;

		Move();
		Rotate();
		FollowTarget();
	}

	private void Move()
	{
		Vector3 desiredVelocity = _navMeshAgent.desiredVelocity;
		desiredVelocity.Normalize();
		desiredVelocity = desiredVelocity * _moveSpeed;
		desiredVelocity.y -= _gravitySpeed;

		_characterController.Move(desiredVelocity * Time.deltaTime);
		_navMeshAgent.velocity = _characterController.velocity;
	}

	private void Rotate()
	{
		Vector3 lookDirection = _navMeshAgent.steeringTarget - transform.position;
		lookDirection.y = 0;
		if (lookDirection != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _turnSpeed);
		}
			
	}

	private void TargetFound(GameObject target)
	{
		_target = target;
	}

	private void FollowTarget()
	{
		if (_target == null)
			return;
		_navMeshAgent.SetDestination(_target.transform.position);
	}

	private void Die()
	{
		_active = false;
		_characterController.enabled = false;
	}
}
