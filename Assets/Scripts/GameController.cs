﻿using UnityEngine;
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
	public GameState gameState;
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
	private int gameLevel=1;
	private int gameScore = 0;


	//HUD items linked here
	public GameObject HUDAttempts;
	public GameObject HUDTiles;
	public GameObject HUDScore;
	public GameObject HUDLevel;
	public GameObject HUDLevelChangePanel;
	public GameObject HUDLevelUP;
	public GameObject HUDGameOverPanel;

	//Top Scores
	private List<int> topScoreList;

	// Use this for initialization
	void Start () {
		// Establishes that this is the controller, don't destroy it
		CS = this;
		DontDestroyOnLoad(this);
		ChangeGameState (GameState.firststart);

	}

	void OnGUI(){
//		GUI.Box(new Rect(10,700,200,50),numTrials.ToString()+"correct " + numCorrect +"incorrect " + numIncorrect);
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void ChangeGameState(GameState newState){

		Debug.Log (newState.ToString ());
		gameState = newState;

		if(newState == GameState.firststart){
			numTrials = 15;
			LoadTopScores ();
			InitializeGame();
		}
		
		if (newState == GameState.nextlevel) {
			DeleteCells ();
			LoadNextLevelPanel();
		}

		if(newState == GameState.showcards){
			StartCoroutine(HideCells());}

		if (newState == GameState.gameover) {
				
		}
	}

	public void InitializeGame(){
		// Initializes the game and starts assigns values to the variables.


		//Starts the game by making a 2D integer matrix and assigns "black squares and white squares"
		blackObjects = new List<GameObject>();
		numCorrect = 0;
		numIncorrect = 0;
		LoadLevelDetails ();
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

	public void CheckWin (){
	// Checcks if the number of tries is up
		if (numCorrect + numIncorrect >= numBlacks) {
			gameState = GameState.idle;
			if(numIncorrect > 0){ 
				SaveTopScores ();
				StartCoroutine (TallyGame (false));
			}
			else{
				StartCoroutine(TallyGame (true));
			}
		}
	}
	
	public IEnumerator TallyGame(bool playerWin){
	//Tallys up the game and handles level up. 
		
		if (numTrials > 1) {
			numTrials --;
			if (playerWin == true) {gameLevel++;		NGUITools.SetActive(HUDLevelUP,true);} 
			if (playerWin ==false) {
				RevealCells();
				if (gameLevel > 1) {if (numBlacks - numCorrect > 1) {gameLevel --;} }
			}
			
			yield return new WaitForSeconds(1);
			ChangeGameState(GameState.nextlevel);
		}
		else{
			ChangeGameState(GameState.idle);
		}
	}

	
	public void ClearButton(){
		StartCoroutine (HideCells ());
	}
	
	public void DeleteCells(){
		if(objectMatrix.Length > 0){
			foreach(GameObject deleteMe in objectMatrix){
				Destroy (deleteMe);
			}
		}
	}
	
	public IEnumerator HideCells(){
		yield return new WaitForSeconds (2);
		foreach(GameObject colorMe in blackObjects){
			colorMe.GetComponent<Animator>().Play("White");
		}
		yield return new WaitForSeconds (0.8f);
		ChangeGameState(GameState.choosecards);
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
	
	public void InstantiateCells(){
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
		topScoreList.Add(gameScore);

	}
	public void UpdateHUD(){
		HUDAttempts.GetComponent<UILabel>().text = numTrials.ToString();
		HUDScore.GetComponent<UILabel>().text = gameScore.ToString();
		HUDTiles.GetComponent<UILabel>().text = (numBlacks - numCorrect - numIncorrect).ToString();
		HUDLevel.GetComponent<UILabel>().text = (gameLevel).ToString();
	}

	public void RevealCells(){
		for (int i = 0; i <numCols; i++) {
				for (int j = 0; j<numRows; j++) {
						if (memoryMatrix [i, j] == 1) {
						objectMatrix[i,j].GetComponent<Animator>().Play("BlueFaster");
						}
				}
		}
	}

	public void LoadLevelDetails(){
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
	}

	private void SaveTopScores(){

		topScoreList.Sort ();
		topScoreList.Reverse ();
		PlayerPrefs.SetInt ("topScore1", topScoreList [0]);
		PlayerPrefs.SetInt ("topScore2", topScoreList [1]);
		PlayerPrefs.SetInt ("topScore3", topScoreList [2]);
		PlayerPrefs.SetInt ("topScore4", topScoreList [3]);
		PlayerPrefs.SetInt ("topScore5", topScoreList [4]);
	}

}
