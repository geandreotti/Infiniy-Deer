using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyAnimations : MonoBehaviour
{
	public static LobbyAnimations Instance;


	private void Awake()
	{
		if (Instance)
			Destroy(Instance);
		Instance = this;
	}
}
