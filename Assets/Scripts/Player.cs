using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MovingObject {

    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1;
    public Text foodText;
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    private Animator animator;
    private int food;

    //mobile controlls
    //set initial touch point off screen to indicate 'no touch yet'...
    private Vector2 touchOrigin = -Vector2.one; // (-1,-1)

	// Use this for initialization
	protected override void Start () {
        animator = GetComponent<Animator>();

        //during each level the player will manage the food points
        //in between level transitions, the game manager will remember
        //player's food points.
        food = GameManager.instance.playerFoodPoints;

        foodText.text = "Food: " + food;

        base.Start();
	}

    private void OnDisable() {
        //when player is disabled in between scene loads,
        //store the food in the game manager which persists
        //between levels.
        GameManager.instance.playerFoodPoints = food;
    }

    // Update is called once per frame
    void Update () {
        //if it's not the player's turn, nothing more to do here,
        //return prematurely. Otherwise execute player instructions.
        if (!GameManager.instance.playersTurn) return;

        int horizontal = 0;
        int vertical = 0;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        //movement code for standalone build controls (PC, controller)
        //returns in float range [-1,1], we int cast to force the numbers
        //to be -1, 0 or 1.
        horizontal = (int) Input.GetAxisRaw("Horizontal");
        vertical = (int) Input.GetAxisRaw("Vertical");

        //prevent the player from being able to move diagonally by
        //forcing movement in only one axis if buttons in both x and y
        //directions are pressed. Here, prefer the horizontal movement
        //to override vertical movement.
        if (horizontal != 0) vertical = 0;
#else
        //Mobile controlls
        if (Input.touchCount > 0) { //if any touches detected
            //get the first touch, ignoring other possible touches
            Touch myTouch = Input.touches[0];

            if (myTouch.phase == TouchPhase.Began) { //beginning of a touch
                touchOrigin = myTouch.position;
            } else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) {
                Vector2 touchEnd = myTouch.position;

                //get direction to move in based on origin and destination of the touch
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;

                //reset x for next round
                touchOrigin.x = -1;

                //as touches arent in perfect alignment with the axes, we need
                //to infer the likely direction based on which axis recieved most input
                if (Mathf.Abs(x) > Mathf.Abs(y)) horizontal = x > 0 ? 1 : -1;
                else vertical = y > 0 ? 1 : -1;
            }
        }
#endif

        //if we have a desired movement direction, attempt to move in that
        //direction - we the expectation that we migth hit a wall!
        if (horizontal != 0 || vertical != 0) AttmeptMove<Wall>(horizontal, vertical);
	}

    protected override void AttmeptMove<T>(int xDir, int yDir) {
        food--; //movement costs food points for the player
        foodText.text = "Food: " + food;
        base.AttmeptMove<T>(xDir, yDir);

        RaycastHit2D hit;
        if (Move(xDir, yDir, out hit)) {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }

        CheckIfGameOver(); //after food update, see if game ends

        //if all good, notify game manager that players turn has ended.
        GameManager.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        //this function will handle interactions between player
        //and other items such as food, soda, exit etc.
        
        if (other.tag == "Exit") {
            //reload the scene (regenerate the level) and disable the player
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        } else if (other.tag == "Food") {
            food += pointsPerFood;
            foodText.text = "+" + pointsPerFood + " Food: " + food;
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false); //deactivate the other object as it was "consumed".
        } else if (other.tag == "Soda") {
            food += pointsPerSoda;
            foodText.text = "+" + pointsPerSoda + " Food: " + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false); //deactivate the other object as it was "consumed".
        }
    }

    protected override void OnCantMove<T>(T component) {
        //Player walks into a wall, then damage the wall.
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);

        //tell the player animation state machine to execute
        //state transitions and subsequent animations based
        //on activation of the trikker 'playerChop'
        animator.SetTrigger("PlayerChop");
    }

    private void Restart() {
        //Normally, you would load a new scene with new level,
        //however, our game is procedurally generated. Therefore
        //we simply reload the current secene we are on and our
        //level will be generated again.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoseFood(int loss) {
        //called when attacked by enemy
        //transition to player hit animation state
        animator.SetTrigger("PlayerHit");
        food -= loss;
        foodText.text = "-" + loss + " Food: " + food;
        CheckIfGameOver();
    }

    private void CheckIfGameOver() {
        if (food <= 0) {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.GameOver(); //will disable game manager
        }
    }
}
