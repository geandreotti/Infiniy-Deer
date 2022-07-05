using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bullet : MonoBehaviour
{
	public float damage;

	[SerializeField] private float _waitToDestroy;
	private void Start()
	{
		StartCoroutine(DestroyBullet());
	}

	private void Update()
	{
		transform.Translate(new Vector3(0, 0, 20 * Time.deltaTime));
	}

	private IEnumerator DestroyBullet()
	{
		yield return new WaitForSeconds(_waitToDestroy);
		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (PhotonNetwork.IsMasterClient)
				{
					transform.DOScale(0, .2f).SetEase(Ease.InOutSine).OnComplete(() => PhotonNetwork.Destroy(gameObject));
					yield break;

				}
		transform.DOScale(0, .2f).SetEase(Ease.InOutSine).OnComplete(() => Destroy(gameObject));

	}
}
