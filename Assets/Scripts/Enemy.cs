using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.

//Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
public class Enemy : MovingObject
{
	public int playerDamage; 							//The amount of food points to subtract from the player when attacking.


	private Animator animator;							//Variable of type Animator to store a reference to the enemy's Animator component.
	private Transform target;							//Transform to attempt to move toward each turn.
	private bool skipMove;								//Boolean to determine whether or not enemy should skip a turn or move this turn.
	public int hp;									//hp for enemy
	public int hp10;
	public int turn;
	public int div=11;
	//Start overrides the virtual Start function of the base class.
	protected override void Start ()
	{
		
		// ADDED //Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
		//This allows the GameManager to issue movement commands.
		GameManager.instance.AddEnemyToList (this);

		//Get and store a reference to the attached Animator component.
		animator = GetComponent<Animator> ();

		//Find the Player GameObject using it's tag and store a reference to its transform component.
		target = GameObject.FindGameObjectWithTag ("Player").transform;

		turn = 0;
		div -= 1;

		//Call the start function of our base class MovingObject.
		base.Start ();
	}


	// AI FOR ENEMY
	//Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
	//See comments in MovingObject for more on how base AttemptMove function works.
	protected override void AttemptMove <T> (int xDir, int yDir)
	{
		turn += 1;
		//base.AttemptMove <T> (xDir, yDir);
		//Check if skipMove is true, if so set it to false and skip this turn.
		// ENEMY MOVE EVERY OTHER TURN
		/*switch (GameManager.instance.level) {
			case 2:
				//if (turn % div == 4 || turn % div == 8)
					//base.AttemptMove <T> (xDir, yDir);
			//StartCoroutine (Wait(xDir,yDir));
					//base.AttemptMove <T> (xDir, yDir);
					break;
			case 3:
				int temp = turn % div;
				if (temp%2==1)
					base.AttemptMove <T> (xDir, yDir);
				break;
			case 4:
				if (turn % div != 3)
					base.AttemptMove <T> (xDir, yDir);
				break;
			case 5:
				if (turn % div != 3 && turn % div != 7)
					base.AttemptMove <T> (xDir, yDir);
				else {
					base.AttemptMove <T> (xDir, yDir);
					StartCoroutine ("Wait");
					base.AttemptMove <T> (xDir, yDir);
				}
				break;
			case 6:
				base.AttemptMove <T> (xDir, yDir);
				StartCoroutine ("Wait");
				base.AttemptMove <T> (xDir, yDir);
				break;
			case 7:
				base.AttemptMove <T> (xDir, yDir);
				StartCoroutine ("Wait");
				base.AttemptMove <T> (xDir, yDir);
				StartCoroutine ("Wait");
				base.AttemptMove <T> (xDir, yDir);
				break;
			default:*/
				if (skipMove) {
					skipMove = false;
					return;
				}
				//Call the AttemptMove function from MovingObject.
				base.AttemptMove <T> (xDir, yDir);
				//Now that Enemy has moved, set skipMove to true to skip next move.
				skipMove = true;
				//break;
		//}
	}
	// A* ALGORITHM HERE
	//MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
	public void MoveEnemy ()
	{
		//Declare variables for X and Y axis move directions, these range from -1 to 1.
		//These values allow us to choose between the cardinal directions: up, down, left and right.
		int xDir = 0;
		int yDir = 0;

		// Check the target and transform position and figure out which direction to move them; checks if enemy and player are in the same column
		//If the difference in positions is approximately zero (Epsilon) do the following:
		if (Mathf.Abs (target.position.y - transform.position.y) > 0)

			// If they are in the same column, check y coordinate
			//If the y coordinate of the target's (player) position is greater than the y coordinate of this enemy's position set y direction 1 (to move up). If not, set it to -1 (to move down).
			yDir = target.position.y > transform.position.y ? 1 : -1;
		

		//If the difference in positions is not approximately zero (Epsilon) do the following:
		if (Mathf.Abs (target.position.x - transform.position.x) > 0)
			//Check if target x position is greater than enemy's x position, if so set x direction to 1 (move right), if not set to -1 (move left).
			xDir = target.position.x > transform.position.x ? 1 : -1;
		

		//Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
		AttemptMove <Player> (xDir, yDir);
	}


	//OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
	//and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
	protected override void OnCantMove <T> (T component)
	{
		//Declare hitPlayer and set it to equal the encountered component.
		Player hitPlayer = component as Player;

		//Set the attack trigger of animator to trigger Enemy attack animation.

		animator.SetTrigger ("enemyAttack");

		//Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
		hitPlayer.LoseFood (playerDamage);
	}
	public void LoseHP(int loss)
	{
		hp -= loss;
		hp10 -= loss;
		//If hit points are less than or equal to zero:
		if (GameManager.instance.level<10 && hp <= 0)
			//Disable the gameObject. And save all the gameObject
			gameObject.SetActive (false);

		if (GameManager.instance.level==10 && hp10 <= 0)
			//Disable the gameObject. And save all the gameObject
			gameObject.SetActive (false);
		
	}
}