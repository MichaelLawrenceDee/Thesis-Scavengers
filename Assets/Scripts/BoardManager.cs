using UnityEngine;
using System;                   // For Serializable attribute
//Serializable allows to modify how variables will appear in inspector in the ditor and to show & hide them using a full-bout(?)
using System.Collections.Generic;   // To use lists
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour {

    // Using Serializable allows us to embed a class with sub properties in the inspector.
    [Serializable]
    public class Count
    {
        public int minimum;             //Minimum value for our Count class.
        public int maximum;             //Maximum value for our Count class.


        //Assignment constructor.
        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }


    public int columns = 8;                                         //Number of columns in our game board.
    public int rows = 8;                                            //Number of rows in our game board.
    public Count wallCount = new Count(5, 9);                       //Lower (min of 5) and upper (max of 9) limit for our random number of walls per level.
	public int foodCount = 6;
	//public Count foodCount = new Count(1, 5);                       //Lower and upper limit for our random number of food items per level.
    public GameObject exit;                                         //Prefab to spawn for exit.
    // Other objecs use arrays to pass in multiple objects and choose one of them we want to spawn among the variations
    public GameObject[] floorTiles;                                 //Array of floor prefabs.
    public GameObject[] wallTiles;                                  //Array of wall prefabs.
    public GameObject[] foodTiles;                                  //Array of food prefabs.
    public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
    public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.

    // boardHolder - to keep hierarchy clean, to child the spawning obejcts so to collapse in hierarchy and not fill them all to hierarchy withobjects
    private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
    // Track all diff possible positions on gameboard to keep track wether an object has been spawned in that position or not
    private List<Vector3> gridPositions = new List<Vector3>();  //A list of possible locations to place tiles.


    //Clears our list gridPositions and prepares it to generate a new board.
    void InitialiseList()
    {
        //Clear our list gridPositions.
        gridPositions.Clear();

        // Creating as lists of possible positions to place walls, enemies, or pickups

        //Loop through x axis (columns).
        // Will run as long as x is less than columns - 1: (1,1)...(6,1) -
        for (int x = 1; x < columns - 1; x++)
        {
            //Within each column, loop through y axis (rows).
            // (1,2),(2,6),(3,2),(4,6),(5,2),(6,6) |
            for (int y = 1; y < rows - 1; y++)
            {
                //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }


    //Sets up the outer walls and floor (background) of the game board.
    void BoardSetup()
    {
        //Instantiate Board and set boardHolder to its transform.
        boardHolder = new GameObject("Board").transform;

        //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
        for (int x = -1; x < columns + 1; x++)
        {
            //Loop along y axis, starting from -1 to place floor or outerwall tiles.
            for (int y = -1; y < rows + 1; y++)
            {
                //Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                //Check if we current position is at board edge (outer wall position), if so choose a random outer wall prefab from our array of outer wall tiles. (-1,-1)(8,8)
                if (x == -1 || x == columns || y == -1 || y == rows)
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];

                //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                //Quaternion.Identity - instantiate with no rotation
                GameObject instance =
                    Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                instance.transform.SetParent(boardHolder);
            }
        }
    }


    // Random positions to not duplicate when spawned
    //RandomPosition returns a random position from our list gridPositions.
    Vector3 RandomPosition()        // Place random objects for walls, enemies and powerups
    {
        //Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
        int randomIndex = Random.Range(0, gridPositions.Count);     // From 0 to stored positions of grid lists (which is 36)

        //Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
        Vector3 randomPosition = gridPositions[randomIndex];

        //Remove the entry at randomIndex from the list so that it can't be re-used. For it to not spawn to objects in the same position
        gridPositions.RemoveAt(randomIndex);

        //Return the randomly selected Vector3 position.
        return randomPosition;
    }


    //LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        //Choose a random number of objects to instantiate within the minimum and maximum limits
        // objectCount - control how many of the given object will gonna spawn e.g. # of walls in a level
        int objectCount = Random.Range(minimum, maximum + 1);

        //Instantiate objects until the randomly chosen limit objectCount is reached
        // i < objectCount - spawn # of objects specified by objectCount
        for (int i = 0; i < objectCount; i++)
        {
            //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
            Vector3 randomPosition = RandomPosition();

            //Choose a random tile from tileArray and assign it to tileChoice
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

            //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }


    //SetupScene initializes our level and calls the previous functions to lay out the game board
    public void SetupScene(int level)       // The only single Public function in this class
    {
        //Creates the outer walls and floor.
        BoardSetup();

        //Reset our list of gridpositions.
        InitialiseList();

        //Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);

		// FOOD COUNT FOR EVERY 2 LEVELS DECREASE BY 1
		bool mod = true;

		mod = ((level-1) % 2 == 0);

		if (mod) {
			foodCount--;
		}

		LayoutObjectAtRandom (foodTiles, foodCount, foodCount);

        //Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
        //FOR RANDOMLY GENERATED	
		//LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
		
        //Determine number of enemies based on current level number, based on a logarithmic progression
        int enemyCount = (int)Mathf.Log(level, 2f);

        //Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
        // Doesn't specify random rnge of enemies
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);

        //Instantiate the exit tile in the upper right hand corner of our game board
        // Always placing the exit tile on upper right each level (7,7)
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
    }


    
}
