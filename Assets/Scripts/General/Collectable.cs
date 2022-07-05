using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;
using Photon.Pun;

public class Collectable : MonoBehaviour
{

	public Item item;
	public string itemName;

	[BoxGroup("Settings")] [SerializeField] private float _sineSpeed;
	[BoxGroup("Settings")] [SerializeField] private float _sineLenght;
	[BoxGroup("Settings")] [SerializeField] private float _rotationSpeed;

	[BoxGroup("Settings")] [SerializeField] private Item[] _items;

	public PhotonView photonView;
	private Vector3 _startPosition;
	private bool _dropped;

	private void Awake()
	{
	}

	private void Start()
	{
		if (!GameManager.Instance.online)
			Show();
	}

	private void Update()
	{
		Movement();
	}

	public void Show()
	{
		if (GameManager.Instance)
			if (GameManager.Instance.online)
			{
				if (PhotonNetwork.IsMasterClient)
				{
					photonView.RPC("SetCollectableProperties", RpcTarget.All, itemName);
					StartCoroutine(WaitToDestroy());

				}
				else
					return;
			}

		SetCollectableProperties(itemName);
	}

	[PunRPC]
	private void SetCollectableProperties(string itemName)
	{
		foreach (Item item_ in _items)
			if (item_.name == itemName)
				item = item_;

		transform.Find("Model_" + item.name).gameObject.SetActive(true);
		transform.DOScale(1, .2f).SetEase(Ease.InOutSine);
		_startPosition = transform.position;
		_dropped = true;
	}

	private void Movement()
	{
		if (!_dropped)
			return;
		Vector3 newPosition = transform.position;
		newPosition.y += Mathf.Sin(Time.time * _sineSpeed) * _sineLenght * Time.deltaTime;
		transform.position = newPosition;

		transform.Rotate(Vector3.up * Time.deltaTime * _rotationSpeed);
	}

	public void Collect()
	{
		if (GameManager.Instance)
			if (GameManager.Instance.online)
			{
				if (photonView)
					photonView.RPC("CollectPhoton", RpcTarget.All);
				return;
			}

		CollectPhoton();
		transform.DOScale(0, .2f).SetEase(Ease.InOutSine).OnComplete(() => Destroy(gameObject));

	}

	[PunRPC]
	private void CollectPhoton()
	{
		if (!PhotonNetwork.IsMasterClient)
			return;
		transform.DOScale(0, .2f).SetEase(Ease.InOutSine).OnComplete(() => PhotonNetwork.Destroy(gameObject));
	}

	private IEnumerator WaitToDestroy()
	{
		yield return new WaitForSeconds(10);
		if (GameManager.Instance.online)
			PhotonNetwork.Destroy(gameObject);
		else
			Destroy(gameObject);
	}
}
