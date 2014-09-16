using UnityEngine;
using System.Collections;

public class MatrixBlockScript : MonoBehaviour {
	public int y_coord;
	public int x_coord;
	public bool isBlackSquare;


	// Use this for initialization
	void Start () {
//		Debug.Log (y_coord + "," + x_coord);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown(){
			GameController.CS.CellClick(x_coord, y_coord);
	}
	
}
