using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIKHandler : MonoBehaviour
{
	public float ikWeight;

	[SerializeField] private Animator _animator;
	[SerializeField] private Transform _IKTarget;

	private void OnAnimatorIK(int layerIndex)
	{
		_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
		_animator.SetIKPosition(AvatarIKGoal.RightHand, _IKTarget.position);
	}
}
