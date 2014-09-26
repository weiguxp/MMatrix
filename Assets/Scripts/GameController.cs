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


	//Variables in the game
	private Game currentGame;
	private Game previousGame;

	//Objects in the Game
	private GameTile[,] gameTileMatrix;
	private UILabel avgScoreLabel;
	private List<UILabel> labelList = new List<UILabel> ();
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
	private List<int> topScoreList = new List<int> ();
	private int sumAllScores;
	private int sumAllGames;

	// Use this for initialization
	void Start () {

		// Establishes that this is the controller, don't destroy it
		CS = this;
		DontDestroyOnLoad(this);
		LoadGame ();
		InitializeGame();

	}

	void OnGUI(){
//		
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	private void ChangeGameState(GameState newState){
		Debug.Log (newState.ToString ());
		gameState = newState;
		
		if (gameState == GameState.nextlevel) {
			DeleteAllCells ();
			LoadNextLevelPanel();

		}

		if(gameState == GameState.showcards){
			StartCoroutine(HideCells());}

		if (gameState == GameState.gameover) {
			DeleteAllCells ();
			sumAllGames ++;
			sumAllScores += currentGame.gameScore;
			HandleTopScores();
		}
	}


	private void InitializeGame(){
		currentGame = new Game();

		if (previousGame.gameLevel > 5) {
			currentGame.gameLevel = previousGame.gameLevel - 4;		
		} else {
			currentGame.gameLevel = 1;
		}

		// Initializes the game and starts assigns values to the variables.
		//Starts the game by making a 2D integer matrix and assigns "black squares and white squares"
		currentGame.numCorrect = 0; currentGame.gameScore = 0; currentGame.numIncorrect = 0; currentGame.numTrials = 1;

		GenerateLevelSize ();

		gameTileMatrix = new GameTile[currentGame.numCols, currentGame.numRows];
	
		InstantiateCells ();
		SetBlackCells ();
		UpdateHUD ();
		ChangeGameState(GameState.showcards);
	}

	private void SetBlackCells()
	{
		int numCells = currentGame.numCols * currentGame.numRows;
		HashSet<int> blackSquareList = new HashSet<int>();
		while(blackSquareList.Count < currentGame.numBlacks){
			blackSquareList.Add(Random.Range(0, numCells-1));
		}
		foreach (int i in blackSquareList){
			int x = i / currentGame.numRows;
			int y = i % currentGame.numRows;
			gameTileMatrix[x,y].cellState = GameTile.CellState.BlackHidden;
			gameTileMatrix[x,y].Go.GetComponent<Animator>().Play("Blue");
		}
	}

	private Vector2 GetCoords(GameObject go)
	{
		for (int i = 0; i <currentGame.numCols; i++) 
		{
			for (int j = 0; j<currentGame.numRows; j++) 
			{
				if(gameTileMatrix[i,j].Go == go)
				{
					return new Vector2(i,j);
				}
			}
		}
		return Vector2.zero;
	}


	public void CellClick (GameObject go){
//  Registers clicks when game is at choose cards phase.
	if (gameState == GameState.choosecards) {

			Vector2 targetCoords = GetCoords(go);
			GameTile targetTile = gameTileMatrix[(int)targetCoords.x,(int)targetCoords.y];

			if(targetTile.cellState == GameTile.CellState.BlackHidden)
			{
				targetTile.cellState = GameTile.CellState.BlackShown;
				currentGame.numCorrect++;
				AddGameScore(10);
				targetTile.Go.GetComponent<Animator> ().Play ("BlueFaster");

			}
			if (targetTile.cellState == GameTile.CellState.WhiteHidden) 
			{
				targetTile.cellState = GameTile.CellState.WhiteShown;
				currentGame.numIncorrect ++;
				targetTile.Go.GetComponent<Animator> ().Play ("RedFaster");
			}
			UpdateHUD();
			CheckWin ();
		}
	}


	private GameObject[] FindBlackTileObjects()
	{
		List<GameObject> goList = new List<GameObject> ();

		foreach (GameTile tile in gameTileMatrix) 
		{
			if(tile.cellState == GameTile.CellState.BlackHidden)
				goList.Add(tile.Go);
		}

		return goList.ToArray ();
	}

	private void CheckWin (){
	// Checcks if the number of tries is up
		if (currentGame.numCorrect + currentGame.numIncorrect >= currentGame.numBlacks) {
			gameState = GameState.idle;
			if(currentGame.numIncorrect > 0){ 
				StartCoroutine (HandleGameWin (false));
			}
			else{
				StartCoroutine(HandleGameWin (true));
			}
		}
	}

	public void StartGameClick(){
		NGUITools.SetActive (HUDGameOverPanel, false);
		InitializeGame ();
	}

	private IEnumerator HandleGameWin(bool playerWin){
	//Tallys up the game and handles level up. 

		//score for completing level
		currentGame.gameScore += CurrentLevelScore(currentGame.gameLevel);


		if (currentGame.numTrials > 1) {
			currentGame.numTrials --;
			UpdateHUD();

			if (playerWin == true) {currentGame.gameLevel++;		NGUITools.SetActive(HUDLevelUP,true);} 
			if (playerWin == false) {
				RevealCells();
				if (currentGame.gameLevel > 1) {if (currentGame.numBlacks - currentGame.numCorrect > 1) {currentGame.gameLevel --;} }
			}
			yield return new WaitForSeconds(1);
			ChangeGameState(GameState.nextlevel);
		}
		else{
			RevealCells();
			yield return new WaitForSeconds(1);
			ChangeGameState(GameState.gameover);
		}
	}

	private int CurrentLevelScore(int gameLevel){
		return gameLevel * 10;
	}

	private void HandleTopScores (){
		previousGame = currentGame;
		previousGame.gameLevel = currentGame.gameLevel;
		topScoreList.Add (currentGame.gameScore);
		topScoreList.Sort ();
		topScoreList.Reverse ();
		SaveGame ();

		int newHighScore = 10;

		if (currentGame.gameScore > topScoreList [5]) {
				newHighScore = topScoreList.IndexOf (currentGame.gameScore);		
				Debug.Log ("New High Score " + newHighScore);
		}
		
		// Instantiate Score Items

		if (labelList.Count < 1) {
				for (int i = 0; i <5; i++) {
						GameObject t = NGUITools.AddChild (HUDGameOverPanel, HUDScoreLabel);
						t.transform.localPosition = new Vector3 (-115, 196 - i * 118, 0);
						labelList.Add (t.GetComponent<UILabel> ()); 
				}
			GameObject averageScoreLabel = NGUITools.AddChild (HUDGameOverPanel, HUDScoreLabel);
			avgScoreLabel = averageScoreLabel.GetComponent<UILabel>();
			averageScoreLabel.transform.localPosition = new Vector3 (-115, -382, 0);
		}

		for (int i = 0; i <5; i++) {
				labelList[i].text = (i + 1).ToString () + ". " + topScoreList [i].ToString ();
				if (i == newHighScore) {
					labelList[i].text = (i + 1).ToString () + ". " + topScoreList [i].ToString () + " (New)";
				}
		}


		int averageScore = sumAllScores / sumAllGames;
		avgScoreLabel.text = "Average:" + averageScore.ToString ();
		LoadGameOverPanel ();
	}
	
	private void DeleteAllCells(){
		if(gameTileMatrix.Length > 0){
			foreach(GameTile deleteMe in gameTileMatrix){
				Destroy (deleteMe.Go);
			}
		}
	}
	
	private IEnumerator HideCells(){
		yield return new WaitForSeconds (2);
		foreach(GameObject colorMe in FindBlackTileObjects()){
			colorMe.GetComponent<Animator>().Play("White");
		}
		yield return new WaitForSeconds (0.8f);
		ChangeGameState(GameState.choosecards);
	}	

	public void LoadGameOverPanel(){

		HUDGameOverPanel.GetComponent<TweenAlpha> ().enabled = true;
		NGUITools.SetActive (HUDGameOverPanel, true);

		HUDGameOverScore.GetComponent<UILabel> ().text = currentGame.gameScore.ToString();
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
		for (int i = 0; i <currentGame.numCols; i++){
			for (int j = 0; j<currentGame.numRows; j++){
				squareObject = (GameObject)Instantiate(whiteSquare, new Vector3((float)i-(float)currentGame.numCols/2,(float)j-(float)currentGame.numRows/2,0), Quaternion.identity);
				//objectMatrix[i,j] = squareObject;

				gameTileMatrix[i,j] = new GameTile(squareObject, i , j);

			}
		}
	}

	private void AddGameScore(int addScore){
		currentGame.gameScore += addScore;

	}
	private void UpdateHUD(){
		HUDAttempts.GetComponent<UILabel>().text = currentGame.numTrials.ToString();
		HUDScore.GetComponent<UILabel>().text = currentGame.gameScore.ToString();
		HUDTiles.GetComponent<UILabel>().text = (currentGame.numBlacks - currentGame.numCorrect - currentGame.numIncorrect).ToString();
		HUDLevel.GetComponent<UILabel>().text = (currentGame.gameLevel).ToString();
	}

	private void RevealCells(){
		for (int i = 0; i <currentGame.numCols; i++) {
				for (int j = 0; j<currentGame.numRows; j++) {
						if (gameTileMatrix[i,j].cellState == GameTile.CellState.BlackHidden) {
						gameTileMatrix[i,j].Go.GetComponent<Animator>().Play("BlueFaster");
						}
				}
		}
	}

	private void GenerateLevelSize(){
		currentGame.numBlacks = currentGame.gameLevel + 2;
		currentGame.numCols = (currentGame.gameLevel -1 )/ 4 + 3;
		currentGame.numRows = (currentGame.gameLevel) / 2 + 3;
	}

	private void  LoadGame(){
		//Loads previous high scores if any. 
		for (int i = 0; i <5; i++) {
			topScoreList.Add(PlayerPrefs.GetInt ("topScore" + i));
		}

		sumAllGames = PlayerPrefs.GetInt ("sumAllGames");
		sumAllScores = PlayerPrefs.GetInt ("sumAllScores");

		//Allows saving of previous game
		previousGame = new Game ();
		previousGame.gameLevel = PlayerPrefs.GetInt ("previousGameLevel");
	}

	private void SaveGame(){

		for (int i = 0; i <5; i++) {
			PlayerPrefs.SetInt ("topScore"+i, topScoreList [i]);
		}

		PlayerPrefs.SetInt ("previousGameLevel", currentGame.gameLevel);
		PlayerPrefs.SetInt ("sumAllGames", sumAllGames);
		PlayerPrefs.SetInt ("sumAllScores", sumAllScores);
	}

//	public void ClearButton(){
//		StartCoroutine (HideCells ());
//	}

}
