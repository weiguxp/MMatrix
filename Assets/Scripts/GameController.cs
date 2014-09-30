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
	private int numTrials;
	private int numCols;
	private int numRows;
	private int numBlacks;
	private int numCorrect;
	private int numIncorrect;
	private int gameLevel;
	private int gameScore;

	//Objects in the Game
	private GameTile[,] objectMatrix;
	private int previousGameLevel;
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
		PlayerPrefs.DeleteAll ();
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
		
		if (gameState == GameState.nextlevel) {
			DeleteAllCells ();
			LoadNextLevelPanel();

		}

		if(gameState == GameState.showcards){
			StartCoroutine(HideCells());}

		if (gameState == GameState.gameover) {
			sumAllGames ++;
			sumAllScores += gameScore;
			HandleTopScores();
		}
	}

	private void StartNewGame(){

		// Used to start a game for the first time.
		if (previousGameLevel > 5) {
			gameLevel = previousGameLevel - 4;		
		} else {
			gameLevel = 1;
		}
		numTrials = 1;

		InitializeGame();

		if (objectMatrix [0, 0].cellState == GameTile.CellState.BlackHidden) {
		};
	}

	private void InitializeGame(){
		// Initializes the game and starts assigns values to the variables.
		//Starts the game by making a 2D integer matrix and assigns "black squares and white squares"
		numCorrect = 0;gameScore = 0; numIncorrect = 0;

		LoadLevelDetails ();

		//objectMatrix = new GameObject[numCols,numRows];

		objectMatrix = new GameTile[numCols, numRows];

	


		InstantiateCells ();


		SetBlackCells ();

		UpdateHUD ();
		ChangeGameState(GameState.showcards);
	}

	private void SetBlackCells()
	{
		int numCells = numCols * numRows;
		HashSet<int> blackSquareList = new HashSet<int>();
		while(blackSquareList.Count < numBlacks){
			blackSquareList.Add(Random.Range(0, numCells-1));
		}
		foreach (int i in blackSquareList){
			int x = i / numRows;
			int y = i % numRows;
			objectMatrix[x,y].cellState = GameTile.CellState.BlackHidden;
			objectMatrix[x,y].Go.GetComponent<Animator>().Play("Blue");
		}
	}

	private Vector2 GetCoords(GameObject go)
	{
		for (int i = 0; i <numCols; i++) 
		{
			for (int j = 0; j<numRows; j++) 
			{
				if(objectMatrix[i,j].Go == go)
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
			GameTile targetTile = objectMatrix[(int)targetCoords.x,(int)targetCoords.y];

			if(targetTile.cellState == GameTile.CellState.BlackHidden)
			{
				targetTile.cellState = GameTile.CellState.BlackShown;
				numCorrect++;
				AddGameScore(10);
				targetTile.Go.GetComponent<Animator> ().Play ("BlueFaster");

			}
			if (targetTile.cellState == GameTile.CellState.WhiteHidden) 
			{
				targetTile.cellState = GameTile.CellState.WhiteShown;
				numIncorrect ++;
				targetTile.Go.GetComponent<Animator> ().Play ("RedFaster");
			}
		}
		UpdateHUD();
		CheckWin ();
	}


	private GameObject[] FindBlackTileObjects()
	{
		List<GameObject> goList = new List<GameObject> ();

		foreach (GameTile tile in objectMatrix) 
		{
			if(tile.cellState == GameTile.CellState.BlackHidden)
				goList.Add(tile.Go);
		}

		return goList.ToArray ();
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

	public void StartGameClick(){
		NGUITools.SetActive (HUDGameOverPanel, false);
		DeleteAllCells ();
		StartNewGame ();
	}

	private IEnumerator TallyGame(bool playerWin){
	//Tallys up the game and handles level up. 
		
		if (numTrials > 1) {
			numTrials --;
			UpdateHUD();
			gameScore += gameLevel*10;

			if (playerWin == true) {gameLevel++;		NGUITools.SetActive(HUDLevelUP,true);} 
			if (playerWin == false) {
				RevealCells();
				if (gameLevel > 1) {if (numBlacks - numCorrect > 1) {gameLevel --;} }
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

	private void HandleTopScores (){

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
		if(objectMatrix.Length > 0){
			foreach(GameTile deleteMe in objectMatrix){
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
				//objectMatrix[i,j] = squareObject;

				objectMatrix[i,j] = new GameTile(squareObject, i , j);



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
						if (objectMatrix[i,j].cellState == GameTile.CellState.BlackHidden) {
						objectMatrix[i,j].Go.GetComponent<Animator>().Play("BlueFaster");
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

		for (int i = 0; i <5; i++) {
			topScoreList.Add(PlayerPrefs.GetInt ("topScore" + i));
		}

		sumAllGames = PlayerPrefs.GetInt ("sumAllGames");
		sumAllScores = PlayerPrefs.GetInt ("sumAllScores");
		previousGameLevel = PlayerPrefs.GetInt ("previousGameLevel");
	}

	private void SaveTopScores(){

		for (int i = 0; i <5; i++) {
			PlayerPrefs.SetInt ("topScore"+i, topScoreList [i]);
		}

		PlayerPrefs.SetInt ("previousGameLevel", gameLevel);
		PlayerPrefs.SetInt ("sumAllGames", sumAllGames);
		PlayerPrefs.SetInt ("sumAllScores", sumAllScores);
	}

//	public void ClearButton(){
//		StartCoroutine (HideCells ());
//	}

}
