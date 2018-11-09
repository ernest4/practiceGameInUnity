using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {
    //Creates the game manager when starting the game for the first time.
    public GameManager gameManager;

    void Awake() {
        if (GameManager.instance == null) Instantiate(gameManager);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
