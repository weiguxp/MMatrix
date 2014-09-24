using UnityEngine;
using System.Collections;

public class MatrixBlockScript : MonoBehaviour {
	// Use this for initialization
	void Start () {
//		Debug.Log (y_coord + "," + x_coord);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown(){
			GameController.CS.CellClick(this.gameObject);

	}
	
}
