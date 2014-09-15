using UnityEngine;
using System.Collections;

public class ReInitializeButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown() {
		GameController.CS.ClearButton();
	}
}
