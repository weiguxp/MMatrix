using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	public  enum GameState
	{
		initiate,
		showcards,
		choosecards,
		playerdeath,
		playerwin,
		idle,
	}

	public static GameController CS;
	public GameState gameState;
	private MatrixBlockScript matrixBlockScript;


	public GameObject WhiteSquare;
	private GameObject SquareObject;

	//Settings for the game
	private int numCols;
	private int numRows;
	private int numBlacks;
	private int numCorrect;
	private int[] blacksquares;
	private int[,] memoryMatrix;
	private GameObject[,] objectMatrix;
	private GameObject[] blackObjects;
	private int gameLevel=1;
	private int gameScore = 0;


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
			InitiateGame();
			gameState = GameState.showcards;
		}

		if(gameState == GameState.showcards){
			gameState = GameState.idle;
			StartCoroutine(HideCells());
		}

		if (gameState == GameState.playerdeath) {
			StartCoroutine(PlayerLose());
			gameState = GameState.idle;
		}

		if (gameState == GameState.playerwin) {
			StartCoroutine(PlayerWin ());
			gameState = GameState.idle;
		}

	}


	public void InitiateGame(){
		//Starts the game by making a 2D integer matrix and assigns "black squares and white squares"
		numCorrect = 0;
		Debug.Log ("CurrentLevel:" + gameLevel+ " CurrentScore:" + gameScore);
		LevelInitiate ();
		int numCells = numCols * numRows;
		memoryMatrix  = new int[numCols,numRows];
		objectMatrix = new GameObject[numCols,numRows];

//		Puts the numbers into as HashSet. 
		HashSet<int> blackSquareList = new HashSet<int>();
		while(blackSquareList.Count < numBlacks){
			blackSquareList.Add(Random.Range(0, numCells-1));
		}

		foreach (int i in blackSquareList){
			int x = i / numRows;
			int y = i % numRows;
			memoryMatrix[x,y] = 1;
		InstantiateCells ();
		}

	}

	public void CellClick (int x, int y){
//  Registers clicks when game is at choose cards phase.
	if (gameState == GameState.choosecards) {
			if (memoryMatrix [x, y] == 1) {
				memoryMatrix [x, y] = 3;
				numCorrect++;
				gameScore += 10;
				CheckWin ();
				objectMatrix [x, y].GetComponent<SpriteRenderer> ().color = Color.blue;

			}

			if (memoryMatrix [x, y] == 0) {
				memoryMatrix [x, y] = 2;
				objectMatrix [x, y].GetComponent<SpriteRenderer> ().color = Color.red;
				gameState = GameState.playerdeath;
			}
		}
	}

	public void LevelInitiate(){
		numBlacks = gameLevel + 2;
		numCols = gameLevel / 2 + 3;
		numRows = (gameLevel - 1) / 2 + 3;
	}


	public void ClearButton(){
		StartCoroutine (HideCells ());
	}



	public void DeleteCells(){
		foreach(GameObject deleteMe in objectMatrix){
			Destroy (deleteMe);
		}
	}

	public void CheckWin (){

		if (numCorrect >= numBlacks) {
			gameState = GameState.playerwin;
		}
	}

	public IEnumerator HideCells(){
		yield return new WaitForSeconds (2);
		foreach(GameObject colorMe in objectMatrix){
			colorMe.GetComponent<Animator>().Play("White");
		}
		
		gameState = GameState.choosecards;
	}	

	public IEnumerator PlayerWin(){

		yield return new WaitForSeconds (1);
		gameLevel++;
		DeleteCells ();
		gameState = GameState.initiate;
	}

	public IEnumerator PlayerLose(){
		RevealCells ();
		yield return new WaitForSeconds (1);

		if (gameLevel > 1) {
			if (numBlacks-numCorrect >1){
			gameLevel --;
			}
		}
		DeleteCells ();
		gameState = GameState.initiate;
	}

	public void InstantiateCells(){
//	Instantiates the squares and assigns their x and y cordinate values relative to the memory matrix. 
		for (int i = 0; i <numCols; i++){
			for (int j = 0; j<numRows; j++){

				SquareObject = (GameObject)Instantiate(WhiteSquare, new Vector3((float)i-(float)numCols/2,(float)j-(float)numCols/2,0), Quaternion.identity);
				matrixBlockScript = SquareObject.GetComponent<MatrixBlockScript>();
				matrixBlockScript.x_coord = i;
				matrixBlockScript.y_coord = j;

				objectMatrix[i,j] = SquareObject;

				if (memoryMatrix[i,j] == 1){
					matrixBlockScript.isBlackSquare = true;
//					matrixBlockScript.GetComponent<SpriteRenderer>().color = Color.blue;
//					SquareObject.GetComponent<Animator>().Play("Blue");
				}

				if (memoryMatrix[i,j] == 0){
					matrixBlockScript.isBlackSquare = false;
//					matrixBlockScript.GetComponent<SpriteRenderer>().color = Color.white;
				}

			}
		}

		SelectBlack ();
		foreach (GameObject tempa in blackObjects) {
//			tempa.GetComponent<Animator>().Play("Blue");
		}
	}

	public void SelectBlack(){
		blackObjects = new GameObject[numBlacks] ;
		int elo= 0;

		for (int i = 0; i <numCols; i++) {
			for (int j = 0; j<numRows; j++) {
				if (memoryMatrix[i,j] == 1){
					GameObject temp = objectMatrix[i,j];
					blackObjects[elo]=(temp);
					elo++;
                 }
			}
		}
	}


	public void RevealCells(){

		for (int i = 0; i <numCols; i++) {
				for (int j = 0; j<numRows; j++) {
						if (memoryMatrix [i, j] == 1) {
								objectMatrix[i,j].GetComponent<SpriteRenderer> ().color = Color.blue;
						}
				}
		}
	}
}
