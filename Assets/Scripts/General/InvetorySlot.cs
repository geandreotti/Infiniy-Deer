using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class InvetorySlot : MonoBehaviour
{
	public Item currentItem;
	public Item itemToSwap;
	public bool selected;

	[SerializeField] private RawImage _icon;
	[SerializeField] private RawImage _swapIcon;
	[Space]
	[SerializeField] private CanvasGroup _swapButton;

	private void Start()
	{
		UpdateIcon();
	}

	public void ShowSwapButton(bool show)
	{
		if (show)
			_swapIcon.texture = itemToSwap.icon;

		_swapButton.interactable = show;
		_swapButton.blocksRaycasts = show;
		_swapButton.DOFade(show ? 1 : 0, .2f).SetEase(Ease.InOutSine);

		GetComponent<Button>().interactable = !show;
	}

	public void UpdateIcon()
	{
		if (currentItem != null)
			_icon.texture = currentItem.icon;
		if (itemToSwap == null)
			ShowSwapButton(false);
	}

	public void Selected(bool select)
	{
		selected = select;
		transform.DOScale(select ? 1.1f : 1, .2f).SetEase(Ease.InOutSine);

	}
}
