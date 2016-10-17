using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

    public GameObject gameManager;


	void Awake () {
        // Uses static var used in the script and accessing it from the loader script
        if (GameManager.instance == null)
            Instantiate(gameManager);
	}

}
