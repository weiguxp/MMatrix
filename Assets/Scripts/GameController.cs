using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	public GameObject WhiteSquare;
	public GameObject BlackSquare;
	private GameObject SquareObject;
	private MatrixBlockScript matrixBlockScript;

	//Settings for the game
	private int numCols;
	private int numRows;
	private int numBlacks;
	private int[] blacksquares;
	private int[,] memoryMatrix;
	private GameObject[,] objectMatrix;


	// Use this for initialization
	void Start () {

		// Establishes that this is the controller, don't destroy it
		CS = this;
		DontDestroyOnLoad(this);

		gameState = GameState.initiate;
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if(gameState == GameState.initiate){
			InitiateGame (4,4,5);
			gameState = GameState.showcards;
		}

		if(gameState == GameState.showcards){
			instantiateSquares ();
			gameState = GameState.choosecards;
		}

	}

	public void InitiateGame(int x_cols, int y_rows, int num_blacks){
		//Starts the game by making a 2D integer matrix and assigns "black squares and white squares"

		numCols = x_cols;
		numRows = y_rows;
		numBlacks = num_blacks;
		int numSquares = numCols * numRows;
		memoryMatrix  = new int[numCols,numRows];
		objectMatrix = new GameObject[numCols,numRows];

//		Puts the numbers into as HashSet. 
		HashSet<int> blackSquareList = new HashSet<int>();
		while(blackSquareList.Count < num_blacks){
			blackSquareList.Add(Random.Range(0, numSquares-1));
		}

		foreach (int i in blackSquareList){
			int x = i / numRows;
			int y = i % numRows;
			memoryMatrix[x,y] = 1;
		}

	}

	public void ClearButton(){
		ClearCells ();
	}

	public void ClearCells(){
		foreach(GameObject deleteMe in objectMatrix){
			deleteMe.GetComponent<SpriteRenderer>().color = Color.white;
		}
	}	

	public void instantiateSquares(){
//Instantiates the squares and assigns their x and y cordinate values relative to the memory matrix. 
		for (int i = 0; i <numCols; i++){
			for (int j = 0; j<numRows; j++){

				if (memoryMatrix[i,j] == 1){
					SquareObject = (GameObject)Instantiate(BlackSquare, new Vector3(i,j,0), Quaternion.identity);
					matrixBlockScript = SquareObject.GetComponent<MatrixBlockScript>();
					matrixBlockScript.x_coord = i;
					matrixBlockScript.y_coord = j;
					objectMatrix[i,j] = SquareObject;
				}
				if (memoryMatrix[i,j] == 0){
						SquareObject = (GameObject)Instantiate(WhiteSquare, new Vector3(i,j,0), Quaternion.identity);
						objectMatrix[i,j] = SquareObject;
						matrixBlockScript = SquareObject.GetComponent<MatrixBlockScript>();
						matrixBlockScript.x_coord = i;
						matrixBlockScript.y_coord = j;
					}

			}
		}
	}
}
