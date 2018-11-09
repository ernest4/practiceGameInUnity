using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    //Game manager is important as it manages a single playthrough and 
    //single score for single player. Thus it's important to ensure that
    //only one game manager ever exists. Therefore, the game manager will
    //be made a Singleton.
    public static GameManager instance = null;

    public BoardManager boardScript;

    public int playerFoodPoints = 100;
    [HideInInspector] public bool playersTurn = true;

    public float levelStartDelay = 2f;
    private int level = 1;

    private Text levelText;
    private GameObject levelImage;
    private bool doingSetup;
    public float turnDelay = .1f;
    private List<Enemy> enemies;
    private bool enemiesMoving;

    //called once the script is loaded
    void Awake() {
        //Singleto check
        if (instance == null) instance = this; //if first ever - OK.
        else if (instance != this) Destroy(gameObject); //if another board manager tries to get created - destroy it!
        //ALSO!!!
        //Since we need to preserve game manager between scene loads (where
        //generally everything else in the scene hierarchy get destroyed)
        //we need to make sure that the game manager survives!
        DontDestroyOnLoad(gameObject);

        enemies = new List<Enemy>();

        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    private void OnLevelWasLoaded(int index) {
        level++;

        InitGame();
    }

    private void InitGame() {
        //prevent the player from moving while showing the title card
        doingSetup = true;

        //deal with UI title card
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Day " + level;
        levelImage.SetActive(true);
        //once title card is displayed, wait a bit before taking it off
        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();
        boardScript.SetupScene(level);
    }

    private void HideLevelImage() {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    public void GameOver() {
        //game over text and title card
        levelText.text = "After " + level + " days, you starved.";
        levelImage.SetActive(true);
        //deactivate the game manager
        enabled = false;
    }

    //called once the script is enabled. Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if players turn or enemies already moving, do nothing yet...
        if (playersTurn || enemiesMoving || doingSetup) return;

        //otherwise, it's not players turn and enemies haven't started moving
        //so start off the enemy movements with coroutine.
        StartCoroutine(MoveEnemies());
	}

    public void AddEnemyToList(Enemy script) {
        enemies.Add(script);
    }

    IEnumerator MoveEnemies() {
        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay);

        if (enemies.Count == 0) yield return new WaitForSeconds(turnDelay);


        for (int i = 0; i < enemies.Count; i++) {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;
        enemiesMoving = false;
    }
}
