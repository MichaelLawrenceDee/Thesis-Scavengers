using UnityEngine;
using System.Collections;

// Abstract - enables us to create classes and class members that are incomplete and must be implemented in the derived class
public abstract class MovingObject : MonoBehaviour {

    public float moveTime = 0.1f;           //Time it will take object to move, in seconds.

    // A layer on checking collisions as we're moving to determine if a space is open to be moved in to 
    // Players, enemies, walls on this layer when creating a prefab
    public LayerMask blockingLayer; 		//Layer on which collision will be checked.


    private BoxCollider2D boxCollider;      //The BoxCollider2D component attached to this object.
    private Rigidbody2D rb2D;               // rb2D to storecomponent reference to the rigidbody2d component of the unity moving
    private float inverseMoveTime;          // inversemovetime for movement calculations to be more efficient

	//experimental code for check enemy
	public const int MOVE_ATTEMPT_NO_HIT  = 0;
	public const int MOVE_ATTEMPT_HIT     = 1;

    // 1
    // Protected virtual function can be overriden by their inheriting classes, it's useful for potentially we want for the inheriting classes to have a different implementation of start
	protected virtual void Start () {
        
        //Get a component reference to this object's BoxCollider2D
        boxCollider = GetComponent<BoxCollider2D>();

        //Get a component reference to this object's Rigidbody2D
        rb2D = GetComponent<Rigidbody2D>();

        //By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
        inverseMoveTime = 1f / moveTime;

    }


    // 4
    //Move returns true if it is able to move and false if not. 
    //Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        //Store start position to move from, based on objects current transform position.
        Vector2 start = transform.position;
		Vector2 end1;
		Vector2 end2;
		// Calculate end position based on the direction parameters passed in when calling Move.
		if (transform.gameObject.CompareTag ("Player")) {
			end1 = start + new Vector2 (xDir, yDir);
			end2 = start + new Vector2 (xDir, yDir);
		} else {
			end1 = start + new Vector2(xDir, 0);
			end2 = start + new Vector2(0, yDir);
			//Debug.Log ("Start: " + start + " end1: " + end1 + " end2: " + end2);
		}

        //Disable the boxCollider so that linecast doesn't hit this object's own collider.
        boxCollider.enabled = false;

        //Cast a line from start point to end point checking collision on blockingLayer.
        hit = Physics2D.Linecast(start, end1, blockingLayer);

        //Re-enable boxCollider after linecast
        boxCollider.enabled = true;

        //Check if anything was hit
		if (start!=end1 && hit.transform == null) {
			//If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
			StartCoroutine (SmoothMovement (end1));

			//Return true to say that Move was successful
			return true;
		} else {
			boxCollider.enabled = false;
			hit = Physics2D.Linecast(start, end2, blockingLayer);
			boxCollider.enabled = true;
			if (start!=end2 && hit.transform == null) {
				StartCoroutine (SmoothMovement (end2));
				return true;
			}
			//If something was hit, return false, Move was unsuccesful.
			return false;
		}
    }


    // 2
    //Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
        //Square magnitude is used instead of magnitude because it's computationally cheaper.
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        //While that distance is greater than a very small amount (Epsilon, almost zero):
        while (sqrRemainingDistance > float.Epsilon)
        {
            //Find a new position proportionally closer to the end, based on the moveTime
            // Vector3.MoveTowards - moves a point in a straight line towads a target point
            // We are moving on a new position that will be moved from rb2D (from our current position), the end (destintion), 
                // value returned by Vector3.MoveTowards is gonna be a point iMT*T.dT closer to the end destination
            Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

            //Call MovePosition on attached Rigidbody2D and move it to the (new position found) calculated position.
            rb2D.MovePosition(newPostion);

            //Recalculate the remaining distance after moving.
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            //Return and loop until sqrRemainingDistance is close enough to zero to end the function
            // Null - wait for a frame before reevaluating it the condition of the loop
            yield return null;
        }
    }


    // 5 Generic parameter is used is for the player and enemies inherit from moving object, so that player can intercat with walls while enemies can interact with player, this means we don't know in advance what type of hitComponent is there gonna be interacting with
        // by making it generic, we can get a reference to it and pass that into on canMove which will then act accordingly based on the implementation of onCanMove in the inheriting classes
    //The virtual keyword means AttemptMove can be overridden by inheriting classes using the override keyword.
    //AttemptMove takes a generic parameter T to specify the type of component we expect our unit to interact with if blocked (Player for Enemies, Wall for Player).
    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component         // Where keyword to specify that T is a component
    {
        //Hit will store whatever our linecast hits when Move is called.
        RaycastHit2D hit;

        //Set canMove to true if Move was successful, false if failed.
        bool canMove = Move(xDir, yDir, out hit);

        //Check if nothing was hit by linecast
        if (hit.transform == null)
            //If nothing was hit, return and don't execute further code.
            return;
		
        // If Something was hit
        //Get a component reference to the component of type T attached to the object that was hit
        T hitComponent = hit.transform.GetComponent<T>();

        //If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
        if (!canMove && hitComponent != null)

            //Call the OnCantMove function and pass it hitComponent as a parameter.
            OnCantMove(hitComponent);
    }

	//experimental code
	/*protected virtual int AttemptMovex <T> (int xDir, int yDir)
		where T : Component
	{
		RaycastHit2D hit;
		bool canMove = Move(xDir, yDir, out hit);

		if (hit.transform == null)
			return MOVE_ATTEMPT_NO_HIT;

		T hitComponent = hit.transform.GetComponent<T>();

		if (!canMove && hitComponent != null)
		{
			OnCantMove(hitComponent);
			return MOVE_ATTEMPT_HIT;
		}

		return MOVE_ATTEMPT_NO_HIT;
	}*/

    // 3
    //The abstract modifier indicates that the thing being modified has a missing or incomplete implementation.
    //OnCantMove will be overriden by functions in the inheriting classes.
    protected abstract void OnCantMove<T>(T component)
        where T : Component;


}
