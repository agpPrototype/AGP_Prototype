using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCritical;
using Player;

public class EndGameSwitch : MonoBehaviour {

	void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerControl>())
        {
            GameController.Instance.GameState = Utility.EnumService.GameState.Win_BellRing;
        }
    }
}
