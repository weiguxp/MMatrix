using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
	public  enum GameState
	{
		firststart,
		nextlevel,
		showcards,
		choosecards,
		idle,
		gameover,
	}

	public static GameController CS;
	private GameState gameState;
	public GameObject whiteSquare;
	private MatrixBlockScript matrixBlockScript;
	private GameObject squareObject;


	//Settings for the game
	private int numTrials;
	private int numCols;
	private int numRows;
	private int numBlacks;
	private int numCorrect;
	private int numIncorrect;
	private int[] blacksquares;
	private int[,] memoryMatrix;
	private GameObject[,] objectMatrix;
	private List<GameObject> blackObjects;
	private int gameLevel;
	private int gameScore;
	private int previousGameLevel;
	private List<GameObject> deleteScores = new List<GameObject>();


	//HUD items linked here
	public GameObject HUDAttempts;
	public GameObject HUDTiles;
	public GameObject HUDScore;
	public GameObject HUDLevel;
	public GameObject HUDLevelChangePanel;
	public GameObject HUDLevelUP;
	public GameObject HUDGameOverPanel;
	public GameObject HUDScoreLabel;
	public GameObject HUDGameOverScore;

	//Top Scores
	private List<int> topScoreList;
	private int sumAllScores;
	private int sumAllGames;

	// Use this for initialization
	void Start () {
		// Establishes that this is the controller, don't destroy it
		CS = this;
		DontDestroyOnLoad(this);
		LoadTopScores ();
		StartNewGame();


	}

	void OnGUI(){
//		GUI.Box(new Rect(10,700,200,50),numTrials.ToString()+"correct " + numCorrect +"incorrect " + numIncorrect);
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	private void ChangeGameState(GameState newState){
		Debug.Log (newState.ToString ());
		gameState = newState;
		
		if (newState == GameState.nextlevel) {
			DeleteAllCells ();
			LoadNextLevelPanel();
		}

		if(newState == GameState.showcards){
			StartCoroutine(HideCells());}

		if (newState == GameState.gameover) {
			sumAllGames ++;
			sumAllScores += gameScore;
			HandleTopScores();
		}
	}

	private void StartNewGame(){
		gameScore = 0;
		// Used to start a game for the first time.
		if (previousGameLevel > 5) {
			gameLevel = previousGameLevel - 4;		
		} else {
			gameLevel = 1;
		}
		numTrials = 1;

		InitializeGame();
	}


	private void InitializeGame(){
		// Initializes the game and starts assigns values to the variables.
		//Starts the game by making a 2D integer matrix and assigns "black squares and white squares"
		blackObjects = new List<GameObject>();
		numCorrect = 0;
		numIncorrect = 0;
		LoadLevelDetails ();
		int numCells = numCols * numRows;
		memoryMatrix  = new int[numCols,numRows];
		objectMatrix = new GameObject[numCols,numRows];
		HashSet<int> blackSquareList = new HashSet<int>();
		while(blackSquareList.Count < numBlacks){
			blackSquareList.Add(Random.Range(0, numCells-1));
		}

		foreach (int i in blackSquareList){
			int x = i / numRows;
			int y = i % numRows;
			memoryMatrix[x,y] = 1;
		}
		InstantiateCells ();
		UpdateHUD ();
		ChangeGameState(GameState.showcards);
	}

	public void CellClick (int x, int y){
//  Registers clicks when game is at choose cards phase.
		if (gameState == GameState.choosecards) {

				if (memoryMatrix [x, y] == 1) {
					memoryMatrix [x, y] = 3;
					numCorrect++;
					AddGameScore(10);
					objectMatrix [x, y].GetComponent<Animator> ().Play ("BlueFaster");

				}
				if (memoryMatrix [x, y] == 0) {
					memoryMatrix [x, y] = 2;
					numIncorrect ++;
					objectMatrix [x, y].GetComponent<Animator> ().Play ("RedFaster");
				}
			}
		UpdateHUD();
		CheckWin ();
	}

	public void StartGameClick(){
		NGUITools.SetActive (HUDGameOverPanel, false);
		DeleteAllCells ();
		StartNewGame ();
	}


	private void CheckWin (){
	// Checcks if the number of tries is up
		if (numCorrect + numIncorrect >= numBlacks) {
			gameState = GameState.idle;
			if(numIncorrect > 0){ 
				StartCoroutine (TallyGame (false));
			}
			else{
				StartCoroutine(TallyGame (true));
			}
		}
	}
	
	private IEnumerator TallyGame(bool playerWin){
	//Tallys up the game and handles level up. 
		
		if (numTrials > 1) {
			numTrials --;
			UpdateHUD();
			gameScore += gameLevel*10;

			if (playerWin == true) {gameLevel++;		NGUITools.SetActive(HUDLevelUP,true);} 
			if (playerWin ==false) {
				RevealCells();
				if (gameLevel > 1) {if (numBlacks - numCorrect > 1) {gameLevel --;} }
			}
			yield return new WaitForSeconds(1);
			ChangeGameState(GameState.nextlevel);
		}
		else{
			ChangeGameState(GameState.gameover);
		}
	}

	private void HandleTopScores (){

		foreach (GameObject i in deleteScores) {
			Destroy(i);		
		}

		previousGameLevel = gameLevel;

		topScoreList.Add (gameScore);
		topScoreList.Sort ();
		topScoreList.Reverse ();
		SaveTopScores ();

		int newHighScore = 10;

		if (gameScore > topScoreList [5]) {
				newHighScore = topScoreList.IndexOf (gameScore);		
				Debug.Log ("New High Score " + newHighScore);
		}

		for (int i = 0; i <5; i++) {
				GameObject t = NGUITools.AddChild (HUDGameOverPanel, HUDScoreLabel);
				t.GetComponent<UILabel> ().text = (i + 1).ToString () + ". " + topScoreList [i].ToString ();
				t.transform.localPosition = new Vector3 (-115, 196 - i * 118, 0);

				if (i == newHighScore) {
						t.GetComponent<UILabel> ().text = (i + 1).ToString () + ". " + topScoreList [i].ToString () + " (New)";
				}
				deleteScores.Add(t);
		}

		int averageScore = sumAllScores / sumAllGames;
		GameObject averageScoreLabel = NGUITools.AddChild (HUDGameOverPanel, HUDScoreLabel);
		averageScoreLabel.GetComponent<UILabel> ().text = "Average:" + averageScore.ToString ();
		averageScoreLabel.transform.localPosition = new Vector3 (-115, -382, 0);
		deleteScores.Add(averageScoreLabel);
		LoadGameOverPanel ();
	}
	
	private void DeleteAllCells(){
		if(objectMatrix.Length > 0){
			foreach(GameObject deleteMe in objectMatrix){
				Destroy (deleteMe);
			}
		}
	}
	
	private IEnumerator HideCells(){
		yield return new WaitForSeconds (2);
		foreach(GameObject colorMe in blackObjects){
			colorMe.GetComponent<Animator>().Play("White");
		}
		yield return new WaitForSeconds (0.8f);
		ChangeGameState(GameState.choosecards);
	}	

	public void LoadGameOverPanel(){


		HUDGameOverPanel.GetComponent<TweenAlpha> ().enabled = true;
		NGUITools.SetActive (HUDGameOverPanel, true);

		HUDGameOverScore.GetComponent<UILabel> ().text = gameScore.ToString();
	}


	public void LoadNextLevelPanel(){
		HUDLevelChangePanel.GetComponent<TweenAlpha> ().enabled = true;
		NGUITools.SetActive (HUDLevelChangePanel, true);
	}

	public void FinishedLevelPanelAnimation(){
		NGUITools.SetActive (HUDLevelChangePanel, false);
		NGUITools.SetActive (HUDLevelUP, false);
		InitializeGame ();
	}
	
	private void InstantiateCells(){
//	Instantiates the squares and assigns their x and y cordinate values relative to the memory matrix. 
		for (int i = 0; i <numCols; i++){
			for (int j = 0; j<numRows; j++){

				squareObject = (GameObject)Instantiate(whiteSquare, new Vector3((float)i-(float)numCols/2,(float)j-(float)numRows/2,0), Quaternion.identity);
				matrixBlockScript = squareObject.GetComponent<MatrixBlockScript>();
				matrixBlockScript.x_coord = i;
				matrixBlockScript.y_coord = j;

				objectMatrix[i,j] = squareObject;

				if (memoryMatrix[i,j] == 1){
					matrixBlockScript.isBlackSquare = true;
					blackObjects.Add(squareObject);
					squareObject.GetComponent<Animator>().Play("Blue");
				}

				if (memoryMatrix[i,j] == 0){
					matrixBlockScript.isBlackSquare = false;
				}
			}
		}
	}

	private void AddGameScore(int addScore){
		gameScore += addScore;

	}
	private void UpdateHUD(){
		HUDAttempts.GetComponent<UILabel>().text = numTrials.ToString();
		HUDScore.GetComponent<UILabel>().text = gameScore.ToString();
		HUDTiles.GetComponent<UILabel>().text = (numBlacks - numCorrect - numIncorrect).ToString();
		HUDLevel.GetComponent<UILabel>().text = (gameLevel).ToString();
	}

	private void RevealCells(){
		for (int i = 0; i <numCols; i++) {
				for (int j = 0; j<numRows; j++) {
						if (memoryMatrix [i, j] == 1) {
						objectMatrix[i,j].GetComponent<Animator>().Play("BlueFaster");
						}
				}
		}
	}

	private void LoadLevelDetails(){
		numBlacks = gameLevel + 2;
		numCols = gameLevel / 4 + 3;
		numRows = (gameLevel - 1) / 2 + 3;
	}

	private void  LoadTopScores(){
		topScoreList = new List<int> ();
		topScoreList.Add(PlayerPrefs.GetInt ("topScore1"));
		topScoreList.Add(PlayerPrefs.GetInt ("topScore2"));
		topScoreList.Add(PlayerPrefs.GetInt ("topScore3"));
		topScoreList.Add(PlayerPrefs.GetInt ("topScore4"));
		topScoreList.Add(PlayerPrefs.GetInt ("topScore5"));

		sumAllGames = PlayerPrefs.GetInt ("sumAllGames");
		sumAllScores = PlayerPrefs.GetInt ("sumAllScores");
		previousGameLevel = PlayerPrefs.GetInt ("previousGameLevel");


	}

	private void SaveTopScores(){

		PlayerPrefs.SetInt ("topScore1", topScoreList [0]);
		PlayerPrefs.SetInt ("topScore2", topScoreList [1]);
		PlayerPrefs.SetInt ("topScore3", topScoreList [2]);
		PlayerPrefs.SetInt ("topScore4", topScoreList [3]);
		PlayerPrefs.SetInt ("topScore5", topScoreList [4]);

		PlayerPrefs.SetInt ("previousGameLevel", gameLevel);
		PlayerPrefs.SetInt ("sumAllGames", sumAllGames);
		PlayerPrefs.SetInt ("sumAllScores", sumAllScores);


	}

//	public void ClearButton(){
//		StartCoroutine (HideCells ());
//	}

}
