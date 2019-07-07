using UnityEngine;
using System.Collections;
using UnityEngine.UI;   //Allows us to use UI.
using System.Collections.Generic;

//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
public class Player : MovingObject
{
    public bool onWorldBoard; 
    public bool dungeonTransition; 

	public int wallDamage = 1;					//How much damage a player does to a wall when chopping it.
	public Text healthText;						//UI Text to display current player health total.
	private Animator animator;					//Used to store a reference to the Player's animator component.
	private int health;                         //Used to store player health points total during level.
    public static Vector2 position;

    public Image glove;
    public Image boot;
    public int attackMod = 0, defenseMod = 0;
    private Dictionary<string, Item> inventory;

    private Weapon weapon;
    public Image weaponComponent1, weaponComponent2, weaponComponent3;

    public static bool isFacingRight;

    //Start overrides the Start function of MovingObject
    protected override void Start ()
	{
		//Get a component reference to the Player's animator component
		animator = GetComponent<Animator>();
		
		//Get the current health point total stored in GameManager.instance between levels.
		health = GameManager.instance.healthPoints;
		
		//Set the healthText to reflect the current player health total.
		healthText.text = "Health: " + health;

        position.x = position.y = 2;

        onWorldBoard = true;
        dungeonTransition = false;

        inventory = new Dictionary<string, Item>();
		
		//Call the Start function of the MovingObject base class.
		base.Start ();
	}
	
	private void Update ()
	{
		//If it's not the player's turn, exit the function.
		if(!GameManager.instance.playersTurn) return;
		
		int horizontal = 0;  	//Used to store the horizontal move direction.
		int vertical = 0;		//Used to store the vertical move direction.

        // holds whether the player is blocked from moving or not
        bool canMove = false;
		
		//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
        // GetAxisRaw returns 1 if player moves in positive direction or -1 in negative direction
		horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
		
		//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
		vertical = (int) (Input.GetAxisRaw ("Vertical"));
		
		//Check if moving horizontally, if so set vertical to zero.
		if(horizontal != 0)
		{
			vertical = 0;
		}

		//Check if we have a non-zero value for horizontal or vertical
		if(horizontal != 0 || vertical != 0)
		{
            // if it is, turns off the movement
            if(!dungeonTransition)
            {
                Vector2 start = transform.position;
                Vector2 end = start + new Vector2(horizontal, vertical);
                GetComponent<BoxCollider2D>().enabled = false;
                RaycastHit2D hit = Physics2D.Linecast(start, end, base.blockingLayer);
                GetComponent<BoxCollider2D>().enabled = true;

                if(hit.transform != null)
                {
                    switch(hit.transform.gameObject.tag)
                    {
                        case "Wall":
                            canMove = AttemptMove<Wall>(horizontal, vertical);
                            break;

                        case "Chest":
                            canMove = AttemptMove<Chest>(horizontal, vertical);
                            break;

                        case "Enemy":
                            canMove = AttemptMove<Enemy>(horizontal, vertical);
                            break;
                    }
                }

                else
                {
                    canMove = AttemptMove<Wall>(horizontal, vertical);
                }


                ////Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
                ////Pass in horizontal and vertical as parameters to specify the direction to move Player in.
                //if (onWorldBoard)
                //    canMove = AttemptMove<Wall>(horizontal, vertical);
                //else
                    //canMove = AttemptMove<Chest>(horizontal, vertical);

                // if it is not, not track the movement
                if (canMove && onWorldBoard)
                {
                    // position is updated
                    position.x += horizontal;
                    position.y += vertical;

                    // board is updated
                    GameManager.instance.UpdateBoard(horizontal, vertical);
                }
            }
        }
	}
	
	//AttemptMove overrides the AttemptMove function in the base class MovingObject
	//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
	protected override bool AttemptMove <T> (int xDir, int yDir)
	{
        if (xDir == 1 && !isFacingRight)
            isFacingRight = true;
        else if (xDir == -1 && isFacingRight)
            isFacingRight = false;

		//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
		bool hit = base.AttemptMove <T> (xDir, yDir);
		
		//Set the playersTurn boolean of GameManager to false now that players turn is over.
		GameManager.instance.playersTurn = false;

		return hit;
	}
	
	
	//OnCantMove overrides the abstract function OnCantMove in MovingObject.
	//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
	protected override void OnCantMove <T> (T component)
	{
        if(typeof(T) == typeof(Wall))
        {
            //Set hitWall to equal the component passed in as a parameter.
            Wall blockingObject = component as Wall;
            //Call the DamageWall function of the Wall we are hitting.
            blockingObject.DamageWall(wallDamage);
        }

        else if(typeof(T) == typeof(Chest))
        {
            Chest blockingObject = component as Chest;
            blockingObject.Open();
        }

        else if (typeof(T) == typeof(Enemy))
        {
            Enemy blockingObject = component as Enemy;
            blockingObject.DamageEnemy(wallDamage);
        }

        //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
        animator.SetTrigger ("playerChop");

        if(weapon)
        {
            weapon.UseWeapon();
        }
    }
	
	//LoseHealth is called when an enemy attacks the player.
	//It takes a parameter loss which specifies how many points to lose.
	public void LoseHealth (int loss)
	{
		//Set the trigger for the player animator to transition to the playerHit animation.
		animator.SetTrigger ("playerHit");
		
		//Subtract lost health points from the players total.
		health -= loss;
		
		//Update the health display with the new total.
		healthText.text = "-"+ loss + " Health: " + health;
		
		//Check to see if game has ended.
		CheckIfGameOver ();
	}
	
	
	//CheckIfGameOver checks if the player is out of health points and if so, ends the game.
	private void CheckIfGameOver ()
	{
		//Check if health point total is less than or equal to zero.
		if (health <= 0) 
		{	
			//Call the GameOver function of GameManager.
			GameManager.instance.GameOver ();
		}
	}

    // manages the effect of the interactiong with exit tile
    private void GoDungeonPortal()
    {
        if(onWorldBoard)
        {
            onWorldBoard = false;
            GameManager.instance.EnterDungeon();
            transform.position = DungeonManager.startPosition;
        }
        else
        {
            onWorldBoard = true;
            GameManager.instance.ExitDungeon();
            transform.position = position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Exit")
        {
            dungeonTransition = true;
            Invoke("GoDungeonPortal", 0.5f);
            Destroy(collision.gameObject);
        }

        else if(collision.tag == "Food" || collision.tag == "Soda")
        {
            UpdateHealth(collision);
            Destroy(collision.gameObject);
        }

        else if(collision.tag == "Item")
        {
            UpdateInventory(collision);
            Destroy(collision.gameObject);
            AdaptDifficulty();
        }

        else if(collision.tag == "Weapon")
        {
            if(weapon)
            {
                Destroy(transform.GetChild(0).gameObject);
            }

            collision.enabled = false;
            collision.transform.parent = transform;
            weapon = collision.GetComponent<Weapon>();
            weapon.AcquireWeapon();
            weapon.inPlayerInventory = true;
            weapon.EnableSpriteRender(false);
            wallDamage = attackMod + 3;
            weaponComponent1.sprite = weapon.GetComponentImage(0);
            weaponComponent2.sprite = weapon.GetComponentImage(1);
            weaponComponent3.sprite = weapon.GetComponentImage(2);
            AdaptDifficulty();
        }
    }

    private void UpdateHealth(Collider2D item)
    {
        // not to exceed maximum health
        if (!(health < 100))
            return;

        if(item.tag == "Food")
        {
            health += Random.Range(1, 4);
        }
        else
        {
            health += Random.Range(4, 11);
        }
        GameManager.instance.healthPoints = health;
        healthText.text = "Health: " + health;
    }

    private void UpdateInventory(Collider2D item)
    {
        Item itemData = item.GetComponent<Item>();
        switch(itemData.type)
        {
            case ItemType.GLOVE:
                if (!inventory.ContainsKey("glove"))
                    inventory.Add("glove", itemData);
                else
                    inventory["glove"] = itemData;
                glove.color = itemData.level;
                break;
            case ItemType.BOOT:
                if (!inventory.ContainsKey("boot"))
                    inventory.Add("boot", itemData);
                else
                    inventory["boot"] = itemData;
                boot.color = itemData.level;
                break;
        }

        attackMod = 0;
        defenseMod = 0;

        foreach(KeyValuePair<string, Item> gear in inventory)
        {
            attackMod += gear.Value.attackMod;
            defenseMod += gear.Value.defenseMod;
        }

        if (weapon)
            wallDamage = attackMod + 3;
    }

    private void AdaptDifficulty()
    {
        if (wallDamage >= 10)
            GameManager.instance.enemiesSmarter = true;
        if (wallDamage >= 15)
            GameManager.instance.enemiesFaster = true;
        if (wallDamage >= 20)
            GameManager.instance.enemySpawnRatio = 10;
    }
}

