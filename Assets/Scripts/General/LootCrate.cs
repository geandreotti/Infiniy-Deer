using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using DG.Tweening;
using Photon.Pun;

public class LootCrate : MonoBehaviour
{
	public Transform spawnPoint;

	[BoxGroup("Settings")] [SerializeField] private LayerMask _colliders;
	[BoxGroup("Settings")] public CrateLoot lootType;
	[BoxGroup("Settings")] [SerializeField] private int _spawnQuantity;
	[BoxGroup("Settings")] [SerializeField] private GameObject _collectable;
	[Space]
	[BoxGroup("Settings")] [SerializeField] private float _randomVariationMax;


	[BoxGroup("Loot")] [SerializeField] private List<Item> _weapons = new List<Item>();
	[BoxGroup("Loot")] [SerializeField] private List<Item> _melees = new List<Item>();
	[BoxGroup("Loot")] [SerializeField] private List<Item> _resources = new List<Item>();
	[Space]
	[SerializeField] private GameObject[] _crateParts;

	private PhotonView _photonView;

	private event Action _destroyCrate;

	private void Start()
	{
		TryGetComponent<PhotonView>(out _photonView);
	}

	private void Update()
	{
	}

	public void SpawnItems()
	{
		for (int i = 0; i < _spawnQuantity; i++)
		{
			Vector3 randomPosition = transform.position + new Vector3(UnityEngine.Random.Range(-_randomVariationMax, _randomVariationMax), transform.position.y + .5f, UnityEngine.Random.Range(-_randomVariationMax, _randomVariationMax));

			Item randomItem = GetRandomItems();

			Collectable collectable = Instantiate(_collectable, randomPosition, Quaternion.identity).GetComponent<Collectable>();
			collectable.itemName = randomItem.name;
			collectable.gameObject.transform.localScale = Vector3.zero;
			collectable.enabled = true;
			_destroyCrate += collectable.Show;
		}
	}

	Collectable collectable;
	public void SpawnItems_Photon()
	{
		for (int i = 0; i < _spawnQuantity; i++)
		{
			Vector3 randomPosition = transform.position + new Vector3(UnityEngine.Random.Range(-_randomVariationMax, _randomVariationMax), transform.position.y + .5f, UnityEngine.Random.Range(-_randomVariationMax, _randomVariationMax));
			Item randomItem = GetRandomItems();

			collectable = PhotonNetwork.Instantiate("Collectable_Item", randomPosition, Quaternion.identity).GetComponent<Collectable>();
			collectable.itemName = randomItem.name;
			collectable.enabled = true;
			_destroyCrate += collectable.Show;
		}
	}

	private Item GetRandomItems()
	{
		List<Item> possibleItems = lootType == (CrateLoot)0 ? _weapons : lootType == (CrateLoot)1 ? _melees : _resources;
		Item randomItem = possibleItems[UnityEngine.Random.Range(0, possibleItems.Count)];
		return randomItem;
	}

	[PunRPC]
	public IEnumerator DestroyCrate()
	{

		if (GameManager.Instance)
			if (GameManager.Instance.online)
			{
				if (PhotonNetwork.IsMasterClient)
				{
					SpawnItems_Photon();
					if (GameRoutineManager.Instance)
						GameRoutineManager.Instance.cratesSpawnsOccupied.Remove(spawnPoint);
				}
			}
			else
				SpawnItems();


		GetComponent<MeshRenderer>().enabled = false;

		_destroyCrate?.Invoke();

		foreach (GameObject cratePart in _crateParts)
		{
			cratePart.SetActive(true);
			cratePart.GetComponent<Rigidbody>().isKinematic = false;
		}

		GetComponent<Collider>().enabled = false;

		yield return new WaitForSeconds(.5f);

		foreach (GameObject cratePart in _crateParts)
		{
			cratePart.transform.DOScale(Vector3.zero, .2f);
			cratePart.GetComponent<Collider>().isTrigger = true;
		}

		yield return new WaitForSeconds(.2f);

		if (GameManager.Instance)
		{
			if (GameManager.Instance.online)
			{
				if (PhotonNetwork.IsMasterClient)
				{
					PhotonNetwork.Destroy(gameObject);
					yield break;
				}

			}
			else
				Destroy(gameObject);

		}
		else
			Destroy(gameObject);

	}

	public void OnTriggerEnter(Collider other)
	{

		if (_colliders != (_colliders | (1 << other.gameObject.layer)))
			return;

		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (PhotonNetwork.IsMasterClient)
				{
					_photonView.RPC("DestroyCrate", RpcTarget.AllBuffered);
					return;
				}

		StartCoroutine(DestroyCrate());
	}

	
}

public enum CrateLoot
{
	weapon,
	melee,
	resource
}
