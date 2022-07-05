using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{

	public event Action zombieAttackCallback;
	public event Action<bool> playerEnableCollider;

	public ZombieBehaviour zombieBehaviour;
	public PlayerBehaviour playerBehaviour;

	private void Start()
	{
		if (zombieBehaviour)
			zombieAttackCallback += zombieBehaviour.AttackCallBack;
		if (playerBehaviour)
			playerEnableCollider += playerBehaviour.ActivateMeleeCollider;
	}

	public void ZombieAttackCallback()
	{
		zombieAttackCallback?.Invoke();
	}

	public void ActivateMeleeCollider()
	{
		playerEnableCollider?.Invoke(true);
	}

	public void DeactivateMeleeCollider()
	{
		playerEnableCollider?.Invoke(false);
	}
}
