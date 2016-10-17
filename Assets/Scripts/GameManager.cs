using UnityEngine;
using System.Collections;
using System.Collections.Generic;		//Allows us to use Lists. // Lists to keep track of enemies
using UnityEngine.UI;					//Allows us to use UI.

public class GameManager : MonoBehaviour {
	public float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
	public float turnDelay = 0.1f;							//Delay between each Player turn.

	// Can access public funtion to the var of our GameManager from any script in the game
    public static GameManager instance = null;  			//Public - accessible outside the class
                                                			// Static - variable will belong to the class itself as opposed to the instance of the class
	public BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
	public int playerFoodPoints = 100;						//Starting value for Player food points.
	[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.

	private Text levelText;									//Text to display current level number.
	private GameObject levelImage;							//Image to block out level as levels are being set up, background for levelText.
	public int level = 1;									//Current level number, expressed in game as "Day 1". 
	private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
	private bool enemiesMoving;								//Boolean to check if enemies are moving.
	private bool doingSetup = true;							//Boolean to check if we're setting up board, prevent Player from moving during setup.

    //Awake is always called before any Start functions
    void Awake () {

        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            // Keeps the scores and not destroyed on hierarchy
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

		// Added
		//Assign enemies to a new List of Enemy objects.
		enemies = new List<Enemy>();

        // Get and store component reference to the attached BoardManagaer Script
        boardScript = GetComponent<BoardManager>();

        // Call InitGame function to initialize the first level
        InitGame();
	}

	//This is called each time a scene is loaded. THIS IS AN API, and is called every scene is loaded
	void OnLevelWasLoaded(int index)
	{
		//Add one to our level number.		
		level++;

		//Call InitGame to initialize our level.
		InitGame ();
	}

    void InitGame()
    {
		doingSetup = true;									//While doingSetup is true the player can't move, prevent player from moving while title card is up.

		levelImage = GameObject.Find("LevelImage");			//Get a reference to our image LevelImage by finding it by name.
		levelText = GameObject.Find("LevelText").GetComponent<Text>();		//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
		levelText.text = "Day " + level;					//Set the text of levelText to the string "Day" and append the current level number.
		levelImage.SetActive(true);							//Set levelImage to active blocking player's view of the game board during setup.
		if (level > 1 && level <= 10)
			playerFoodPoints += 5;
		
		if (level > 10) {
			levelText.text = "Victory! You won with " + playerFoodPoints  + " Foods left.\n\n"
				+ "Enemies Killed: " + Player.enemyct + "\n\n"
				+ "Food Obtained: " + Player.foodct + "\n\n"
				+ "Soda Obtained: " + Player.sodact + "\n\n"
				+ "Rank " + RankPercentage(Player.enemyct, Player.foodct + Player.sodact, level);
		} else {
			enemies.Clear();					//Clear any Enemy objects in our List to prepare for next level. // CLear lists of enemies when game starts
			Invoke("HideLevelImage", levelStartDelay); //Call the HideLevelImage function with a delay in seconds of levelStartDelay. If title card is displaued, wait two seconds, before turning off (start the game)
        	boardScript.SetupScene(level);      // Tells boardScript what level of the scene is setting up to determine the number of enemies
		}
    }


	string RankPercentage(int enemy, int foods, int level)
	{
		enemy = Player.enemyct;
		foods = Player.foodct + Player.sodact;

		int rank = ((enemy / 19 * 100) 
			+ (foods / 30 * 100) 
			+ (level /10 *100)) /3;

		if(rank > 90 && rank <= 100) 
			return "S";
		else if(rank > 70 && rank <= 90) 
			return "A";
		else if(rank > 50 && rank <= 70) 
			return "B";
		else if(rank > 30 && rank <= 50) 
			return "C";
		else if(rank > 10 && rank <= 30) 
			return "D";
		else if(rank >= 0 && rank <= 10) 
			return "F";

		return "F";
	}

	string RankFixed(int enemy, int foods, int level)
	{
		enemy = Player.enemyct = 19;
		foods = Player.foodct + Player.sodact;
		foods = 30;
		level = 10;

		if (level == 10 && enemy == 19 && foods == 30) {
			return "S";
		}
		else if (level >= 7 && level <= 9 && enemy >= 12 && enemy <= 18 && foods >= 20 && foods <= 29) {
			return "A";
		}
		else if (level >= 5 && level <= 6 && enemy >= 6 && enemy <= 11 && foods >= 12 && foods <= 19) {
			return "B";
		}
		else if (level >= 3 && level <= 4 && enemy >= 3 && enemy <= 5 && foods >= 6 && foods <= 11) {
			return "C";
		}
		else if (level == 2 && enemy >= 1 && enemy <= 2 && foods >= 3 && foods <= 5) {
			return "D";
		}
		else if (level == 1 && enemy == 0 && foods >= 0 && foods <= 2) {
			return "F";
		}

		return "F";

	}


	//Hides black image used between levels
	void HideLevelImage()
	{
		//Disable the levelImage gameObject.
		levelImage.SetActive(false);

		//Set doingSetup to false allowing player to move again.
		doingSetup = false;
	}

	// PLAYER's SCORE HERE
	//GameOver is called when the player reaches 0 food points
	public void GameOver()
	{
		//Set levelText to display number of levels passed and game over message
		levelText.text = "After " + level + " days, you starved.\n\n"
			+ "Enemies Killed: " + Player.enemyct + "\n\n"
			+ "Food Obtained: " + Player.foodct + "\n\n"
			+ "Soda Obtained: " + Player.sodact + "\n\n"
			+ "Rank " + RankPercentage(Player.enemyct, Player.foodct + Player.sodact, level);

		//Enable black background image gameObject.
		levelImage.SetActive(true);

		//Disable this GameManager.
		enabled = false;
	}
		

	//Update is called every frame.
	void Update()
	{
		//Check that playersTurn or enemiesMoving or doingSetup are not currently true. IF BOTH ARE NOT MOVING/ FALSE
		if(playersTurn || enemiesMoving || doingSetup)

			//If any of these are true, return and do not start MoveEnemies. IF MOVING, MOVE PLAYER
			return;

		// If neither are ture. Start moving enemies. MOVE ENEMIES
		StartCoroutine (MoveEnemies ());
	}

	//Call this to add the passed in Enemy to the List of Enemy objects.
	public void AddEnemyToList(Enemy script)
	{
		//Add Enemy to List enemies.
		enemies.Add(script);
	}


	// DELAY MOVEMENT FOR ENEMY AND PLAYER
	//Coroutine to move enemies one at a time in sequence.
	IEnumerator MoveEnemies()
	{
		//While enemiesMoving is true player is unable to move.
		enemiesMoving = true;

		//Wait for turnDelay seconds, defaults to .1 (100 ms).
		yield return new WaitForSeconds(turnDelay);

		//If there are no enemies spawned (IE in first level):
		if (enemies.Count == 0) 
		{
			//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
			yield return new WaitForSeconds(turnDelay);
		}

		//Loop through List of Enemy objects.
		for (int i = 0; i < enemies.Count; i++)
		{
			//Call the MoveEnemy function of Enemy at index i in the enemies List.
			enemies[i].MoveEnemy ();

			//Wait for Enemy's moveTime before moving next Enemy, 
			yield return new WaitForSeconds(enemies[i].moveTime);
		}
		//Once Enemies are done moving, set playersTurn to true so player can move.
		playersTurn = true;

		//Enemies are done moving, set enemiesMoving to false.
		enemiesMoving = false;
	}
}
