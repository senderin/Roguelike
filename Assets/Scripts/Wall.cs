using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour
{
	public Sprite dmgSprite;					//Alternate sprite to display after Wall has been attacked by player.
	public int hp = 3;							//hit points for the wall.
    public GameObject[] foodTiles;              // holds the two different food item tiles

	private SpriteRenderer spriteRenderer;		//Store a component reference to the attached SpriteRenderer.

	void Awake ()
	{
		//Get a component reference to the SpriteRenderer.
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}
	
	
	//DamageWall is called when the player attacks a wall.
	public void DamageWall (int loss)
	{
		
		//Set spriteRenderer to the damaged wall sprite.
		spriteRenderer.sprite = dmgSprite;
		
		//Subtract loss from hit point total.
		hp -= loss;
		
		//If hit points are less than or equal to zero:
		if(hp <= 0)
        {
            if(Random.Range(0, 5) == 1)
            {
                GameObject toInstantiate = foodTiles[Random.Range(0, foodTiles.Length)];
                GameObject instance = Instantiate(toInstantiate, new Vector3(transform.position.x, transform.position.y, 0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(transform.parent);

            }

            //Disable the gameObject.
            gameObject.SetActive(false);
        }

	}
}
