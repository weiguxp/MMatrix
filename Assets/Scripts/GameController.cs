using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	public  enum GameState
	{
		initiate,
		showcards,
		flipwait,
		choosecards,
		playerdeath,
	}

	public static GameController CS;
	public GameState gameState;

	//Settings for the game
	private int numCols;
	private int numRows;
	private int numBlacks;
	private int[] blacksquares;


	// Use this for initialization
	void Start () {

		// Establishes that this is the controller, don't destroy it
		CS = this;
		DontDestroyOnLoad(this);

		gameState = GameState.initiate;

		InitiateGame (4, 4, 4);
		Debug.Log (5 % 3);

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void InitiateGame(int x_cols, int y_rows, int num_blacks){
		numCols = x_cols;
		numRows = y_rows;
		numBlacks = num_blacks;
		int numSquares = numCols * numRows;
		int[] blackSquareList = new int[num_blacks];
		int[,] memoryMatrix  = new int[numCols,numRows];

		for (int j = 0; j <num_blacks; j++) {
			int isNewNum = 0;
			int i = -1;

			while(isNewNum == 0){
				i = Random.Range (0, numSquares);
				isNewNum =1;
				foreach (int k in blackSquareList){
					if (k == i){ isNewNum=0;}
				}
			}

			blackSquareList [j] = i;
		}

//		foreach (int i in blackSquareList){
//			Debug.Log (i);
//		}

	}
}
