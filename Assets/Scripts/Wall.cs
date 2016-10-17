using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour
{

    // The sprite displayed once the player hit the wall so the player can see if they are succesful in attacking the wall
    public Sprite dmgSprite;					    //Alternate sprite to display after Wall has been attacked by player.
    public int hp = 4;                              // hp for hit points for the wall and initialize to 4 

    private SpriteRenderer spriteRenderer;          //Store a component reference to the attached SpriteRenderer.

    // Use this for initialization
    void Awake()
    {

        //Get a component reference to the SpriteRenderer.
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //DamageWall is called when the player attacks a wall.
    public void DamageWall(int loss)
    {
        // 1
        //Set spriteRenderer to the damaged wall sprite.
        // Gives the player visual feedback if the player successfully attacked the wall
        spriteRenderer.sprite = dmgSprite;

        // 2
        //Subtract loss from hit point total.
        hp -= loss;

        //If hit points are less than or equal to zero:
        if (hp <= 0)
            //Disable the gameObject. And save all the gameObject
            gameObject.SetActive(false);
    }
}
