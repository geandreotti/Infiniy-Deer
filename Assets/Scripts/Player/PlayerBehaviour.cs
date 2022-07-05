using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;
using DG.Tweening;

public class PlayerBehaviour : MonoBehaviour
{

	public bool active;
	[ProgressBar("Health", 100, EColor.Red)]
	public float health;

	[BoxGroup("Settings")] [SerializeField] LayerMask _checkColliders;

	[SerializeField] private GameObject _nickName;
	[SerializeField] public Weapon activeWeapon;
	[SerializeField] private Transform _weaponsParent;
	[Space]
	[SerializeField] private Transform _IKTarget;
	[SerializeField] private GameObject _bulletPrefab;

	[BoxGroup("Components")] [SerializeField] private PlayerMovement _playerMovement;
	[BoxGroup("Components")] [SerializeField] private PlayerVisuals _playerVisuals;


	private Collectable _avaiableCollectable;

	private bool _shooting;
	private PhotonView _photonView;
	private Coroutine _shootRoutine;

	private Item _toSwap;
	private Item _toCollect;

	private GameObject _tempActiveWeapon;

	public event Action initialize;
	public event Action<float> takeDamage;
	public event Action attack;
	public event Action die;
	public event Action<Item, bool> collectableAvaiable;
	public event Action<bool, GameObject> showWeapon;

	private void Start()
	{
		TryGetComponent<PhotonView>(out _photonView);
		Initialize();
	}

	private void Update()
	{
		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (!_photonView.IsMine)
					return;

		if (active && activeWeapon)
			Shoot();
	}

	public void Initialize()
	{
		if (active)
			return;

		if (UIManager.Instance)
		{
			UIManager.Instance.attackButtonClicked += Attack;
			UIManager.Instance.shootButtonClicked += Shooting;
			UIManager.Instance.shootButtoReleased += Shooting;
			UIManager.Instance.swapWeapomButtonClicked += SwapWeapon;
			UIManager.Instance.collectWeapomButtonClicked += CollectWeapon;

			collectableAvaiable += UIManager.Instance.CollectableAvaiable;
		}


		if (_playerMovement != null)
		{
			_playerMovement.playerBehaviour = this;
			initialize += _playerMovement.Initialize;
		}
		if (_playerVisuals != null)
		{
			_playerVisuals.playerBehaviour = this;
			initialize += _playerVisuals.Initialize;
		}

		_nickName.SetActive(false);

		initialize?.Invoke();
		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (_photonView)
					if (_photonView.IsMine)
					{
						_photonView.RPC("ActivatePlayer", RpcTarget.AllBuffered);
						return;
					}

		ActivatePlayer();
	}

	public void TakeDamage(float damageValue)
	{
		if (!active)
			return;
		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (_photonView)
				{

					_photonView.RPC("Damage", RpcTarget.AllBuffered, damageValue);
					return;
				}



		Damage(damageValue);
	}

	public void Attack()
	{
		attack?.Invoke();
	}

	private void SwapWeapon(Item toSwap)
	{
		if (activeWeapon)
			if (activeWeapon.name == toSwap.name)
				return;

		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (_photonView)
				{
					_photonView.RPC("SwapWeapon_Photon", RpcTarget.AllBuffered, toSwap.name);
					return;
				}

		if (activeWeapon)
			if (activeWeapon.name == toSwap.name)
				return;

		_toSwap = toSwap;

		if (activeWeapon)
			showWeapon?.Invoke(false, activeWeapon.gameObject);

		SetActiveWeapon(toSwap.name);

		showWeapon?.Invoke(true, activeWeapon.gameObject);

	}

	private void CollectWeapon(Item toCollect)
	{
		_avaiableCollectable.Collect();

		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (_photonView)
				{
					_photonView.RPC("SwapWeapon_Photon", RpcTarget.AllBuffered, toCollect.name);
					return;
				}

		_toSwap = toCollect;

		if (activeWeapon)
			showWeapon?.Invoke(false, activeWeapon.gameObject);

		SetActiveWeapon(toCollect.name);

		showWeapon?.Invoke(true, activeWeapon.gameObject);
	}

	private void Shooting(bool enable)
	{
		if (!activeWeapon)
			return;

		if (!activeWeapon.GetComponent<Guns>())
			return;

		activeWeapon.GetComponent<Guns>().IKTarget = _IKTarget.transform;

		if (!enable)
		{
			if (_shootRoutine != null)
				StopCoroutine(_shootRoutine);
			//SHOW aim Indicator
			_shooting = enable;

		}
		else
		{
			_shooting = true;
			//HIDE aim Indicator
		}

	}

	private float _shotTimeStamp;
	private void Shoot()
	{
		if (!_shooting)
			return;

		if (Time.time > _shotTimeStamp)
		{
			_shotTimeStamp = Time.time + activeWeapon.GetComponent<Guns>().shotDelay;
			activeWeapon.GetComponent<Guns>().Shoot();
		}
	}

	public void ActivateMeleeCollider(bool activate)
	{
		if (!activeWeapon)
			return;

		if (!activeWeapon.GetComponent<Melees>())
			return;

		if (GameManager.Instance)
			if (GameManager.Instance.online)
			{
				if (_photonView)
					if (_photonView.IsMine)
						_photonView.RPC("ActivateMeleeCollider_Photon", RpcTarget.All, activate);

				return;
			}

		activeWeapon.GetComponent<BoxCollider>().enabled = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_checkColliders != (_checkColliders | (1 << other.gameObject.layer)) || !active)
			return;

		other.transform.TryGetComponent<Collectable>(out _avaiableCollectable);
		if (_avaiableCollectable)
			collectableAvaiable?.Invoke(_avaiableCollectable.item, true);

	}

	private void OnTriggerExit(Collider other)
	{
		if (_checkColliders != (_checkColliders | (1 << other.gameObject.layer)) || !active)
			return;

		Collectable exitCollectable;
		other.transform.TryGetComponent<Collectable>(out exitCollectable);

		if (_avaiableCollectable)
			if (_avaiableCollectable == exitCollectable)
			{
				_avaiableCollectable = null;
				collectableAvaiable?.Invoke(null, false);
			}
	}

	[PunRPC]
	private void ActivatePlayer()
	{
		Debug.Log(PhotonNetwork.NickName);
		if (GameManager.Instance)
			if (GameManager.Instance.online)
				_nickName.GetComponent<TMPro.TextMeshProUGUI>().text = PhotonNetwork.NickName;

		active = true;
		health = 100;

		_playerMovement.playerBehaviour = this;
		_playerVisuals.playerBehaviour = this;

		TryGetComponent<PhotonView>(out _playerMovement.photonView);
		TryGetComponent<PhotonView>(out _playerVisuals.photonView);

		if (!GameManager.Instance.players.Contains(this))
			GameManager.Instance.players.Add(this);
	}

	[PunRPC]
	private void ActivateMeleeCollider_Photon(bool activate)
	{
		activeWeapon.GetComponent<Collider>().enabled = activate;
	}

	[PunRPC]
	private void SwapWeapon_Photon(string weaponName)
	{
		if (activeWeapon)
			showWeapon?.Invoke(false, activeWeapon.gameObject);
		_photonView.RPC("SetActiveWeapon", RpcTarget.AllBuffered, weaponName);
		showWeapon?.Invoke(true, activeWeapon.gameObject);
	}

	[PunRPC]
	private void SetActiveWeapon(string weaponName)
	{
		activeWeapon = _weaponsParent.Find(weaponName).GetComponent<Weapon>();


	}

	[PunRPC]
	private void Damage(float damageValue)
	{
		health -= damageValue;

		if (GameManager.Instance)
		{
			if (GameManager.Instance.online)
			{
				if (_photonView.IsMine)
					UIManager.Instance.UpdateLifeBar(health);

				if (health <= 0)
					_photonView.RPC("Die", RpcTarget.AllBuffered);
				return;
			}
			else if (health <= 0){ }
				Die();
		}
		else if (health <= 0)
			Die();

		if (UIManager.Instance)
			UIManager.Instance.UpdateLifeBar(health);

		takeDamage?.Invoke(damageValue);
	}

	[PunRPC]
	private void Die()
	{
		active = false;
		die?.Invoke();

		if (_photonView.IsMine)
			UIManager.Instance.ShowRetryScreen();
	}
}
