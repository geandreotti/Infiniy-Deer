using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ZombieVisuals : MonoBehaviour
{
	[SerializeField] private Animator _animator;
	[SerializeField] private float _blendSmoothSpeed;
	[Space]
	[SerializeField] private GameObject _healthIndicator;

	private bool _active;
	private CharacterController _characterController;
	public ZombieBehaviour zombieBehaviour;

	private void Update()
	{
		UpdateHealthIndicator();
		if (!_active)
			return;
		UpdateAnimator();

	}

	public void Initialize()
	{
		_active = true;
		TryGetComponent<CharacterController>(out _characterController);
		zombieBehaviour.die += Die;
		zombieBehaviour.attack += Attack;
		zombieBehaviour.firstDamage += ShowHealthIndicator;
	}

	private void UpdateAnimator()
	{
		float desiredBlendValue = _characterController.velocity.magnitude > 0 ? 1 : 0;
		float currentBlendValue = _animator.GetFloat(Animator.StringToHash("Blend"));
		_animator.SetFloat("Blend", Mathf.Lerp(currentBlendValue, desiredBlendValue, Time.deltaTime * _blendSmoothSpeed));

	}

	private void ShowHealthIndicator()
	{
		_healthIndicator.transform.DOScale(1, .3f).SetEase(Ease.InOutSine);
	}

	private void UpdateHealthIndicator()
	{
		Transform camera = GameManager.Instance.mainCamera.transform;
		_healthIndicator.transform.rotation = Quaternion.Euler(0, camera.rotation.y + 180, 0);
		_healthIndicator.GetComponent<Slider>().value = zombieBehaviour.health;
	}

	private void Attack()
	{
		_animator.ResetTrigger("Attack");
		_animator.SetTrigger("Attack");
	}

	private void Die()
	{
		_active = false;
		int random = Random.Range(0, 2);
		_animator.SetLayerWeight(_animator.GetLayerIndex("UpperBody"), 0);
		_animator.SetFloat("DeathBlend", random);
		_animator.SetBool(Animator.StringToHash("Dead"), true);

	}
}
