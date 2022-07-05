using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using NaughtyAttributes;

public class PlayerVisuals : MonoBehaviour
{
	public PlayerBehaviour playerBehaviour;

	[BoxGroup("Components")] [SerializeField] private Animator _animator;
	[BoxGroup("Components")] [SerializeField] private GameObject _nickName;
	[BoxGroup("Components")] [SerializeField] private PlayerIKHandler _IKHandler;
	[BoxGroup("Components")] [SerializeField] private Transform _weaponsParent;
	[Space]
	[BoxGroup("Components")] [SerializeField] private Transform _aimIndicator;

	[BoxGroup("Settings")] [SerializeField] private float _blendSmoothSpeed;

	private bool _active;
	private float _runThreshold;

	private Joystick _movementJoystick;
	private PlayerMovement _playerMovement;
	public PhotonView photonView;

	void Start()
	{

	}

	void Update()
	{
		UpdateNickName();

		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (photonView)
					if (!photonView.IsMine)
						return;

		if (!_active)
			return;

		UpdateAnimator();
	}

	public void Initialize()
	{

		_aimIndicator.gameObject.SetActive(true);

		_active = true;

		_movementJoystick = UIManager.Instance.movementJoystick;

		TryGetComponent<PlayerMovement>(out _playerMovement);
		TryGetComponent<PhotonView>(out photonView);

		playerBehaviour.attack += Attack;
		playerBehaviour.die += Die;
		playerBehaviour.showWeapon += ShowWeapon;

		UIManager.Instance.shootButtonClicked += SetGunIK;
		UIManager.Instance.shootButtoReleased += SetGunIK;

		_runThreshold = _playerMovement ? _playerMovement.runThreshold : .5f;
	}

	private void UpdateAnimator()
	{
		if (_animator == null)
			return;
		Vector2 inputVector = new Vector2(_movementJoystick.Horizontal, _movementJoystick.Vertical);
		float blendTarget = inputVector.magnitude != 0 && inputVector.magnitude > _runThreshold ? 1 : inputVector.magnitude != 0 ? .5f : 0;
		float currentBlend = _animator.GetFloat("Blend");
		float desiredBlend = Mathf.Lerp(currentBlend, blendTarget, Time.deltaTime * (inputVector.magnitude == 0 ? _blendSmoothSpeed / 2 : _blendSmoothSpeed));

		_animator.SetFloat("Blend", desiredBlend);
	}

	private void UpdateNickName()
	{
		Transform camera = GameManager.Instance.mainCamera.transform;
		_nickName.transform.rotation = Quaternion.Euler(0, camera.rotation.y, 0);
	}

	private void Attack()
	{
		if (_animator.GetCurrentAnimatorStateInfo(1).IsName("AttackBlend"))
			return;

		_animator.SetFloat("AttackBlend", 0);

		Vector3 raycastOrigin = new Vector3(transform.position.x, transform.position.y - .5f, transform.position.z);
		RaycastHit hit;
		if (Physics.Raycast(raycastOrigin, transform.forward, out hit))
		{
			Debug.Log(hit.transform.gameObject);
			if (hit.transform.name.Contains("LootCrate"))
				_animator.SetFloat("AttackBlend", 1);
		}

		_animator.SetTrigger(Animator.StringToHash("Attack"));
	}

	private void Die()
	{
		_active = false;
		int random = Random.Range(0, 2);
		_animator.SetLayerWeight(_animator.GetLayerIndex("UpperBody"), 0);
		_animator.SetFloat("DeathBlend", random);
		_animator.SetBool(Animator.StringToHash("Dead"), true);

	}

	private string _weaponToShow;
	private void ShowWeapon(bool show, GameObject weapon)
	{
		Guns gun;
		weapon.TryGetComponent<Guns>(out gun);

		if (UIManager.Instance)
		{
			if (gun)
				UIManager.Instance.ChangeweaponUI(true);
			else
				UIManager.Instance.ChangeweaponUI(false);
		}


		if (show)
		{
			if (gun)
				ShowAimIndicator(true);
			else
				ShowAimIndicator(false);


			if (GameManager.Instance)
				if (GameManager.Instance.online)
					if (photonView)
						if (photonView.IsMine)
						{
							photonView.RPC("ShowWeapon_Photon", RpcTarget.AllBuffered);
							return;
						}

			foreach (Transform children in _weaponsParent)
			{
				children.DOScale(0, 0);
				children.gameObject.SetActive(false);
			}

			weapon.gameObject.SetActive(true);
			weapon.transform.DOScale(0, 0);
			weapon.transform.DOScale(1, .2f).SetEase(Ease.InOutSine);
		}
	}

	private void ShowAimIndicator(bool show)
	{
		Debug.Log(_aimIndicator.childCount);
		if (show)
			for (int i = 0; i < _aimIndicator.childCount; i++)
				_aimIndicator.GetChild(i).GetComponent<CanvasGroup>().DOFade(1, .4f).SetEase(Ease.InOutSine);
		else
			for (int i = _aimIndicator.childCount - 1; i > 2; i--)
				_aimIndicator.GetChild(i).GetComponent<CanvasGroup>().DOFade(0, .4f).SetEase(Ease.InOutSine);
	}

	private void SetGunIK(bool hasIK)
	{
		if (!playerBehaviour.activeWeapon)
			return;

		Guns gun;
		playerBehaviour.activeWeapon.TryGetComponent<Guns>(out gun);
		if (gun)
			_IKHandler.ikWeight = hasIK ? 1 : 0;
	}

	[PunRPC]
	private void ShowWeapon_Photon()
	{
		foreach (Transform children in _weaponsParent)
		{
			children.DOScale(0, 0);
			children.gameObject.SetActive(false);
		}

		Transform weapon = playerBehaviour.activeWeapon.transform;
		weapon.gameObject.SetActive(true);
		weapon.DOScale(0, 0);
		weapon.DOScale(1, .2f).SetEase(Ease.InOutSine);
	}

}