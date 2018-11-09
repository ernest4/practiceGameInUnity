using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

    public int playerDamage;

    private Animator animator;

    //players position, telling the enemy where to move towards.
    private Transform target;

    //cause the enemy to move every other turn
    private bool skipMove;

    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;

    // Use this for initialization
    protected override void Start () {
        GameManager.instance.AddEnemyToList(this); //add itself to game managers list of managed enemies
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    protected override void AttmeptMove<T>(int xDir, int yDir) {
        if (skipMove) { //skip this turn...
            skipMove = false; //prepare to not skip the next turn
            return;
        }

        //otherwise, attempt to move and prepare to skip next turn
        base.AttmeptMove<T>(xDir, yDir);
        skipMove = true;
    }

    public void MoveEnemy() {
        //called by game manager, will move the enemies
        int xDir = 0;
        int yDir = 0;

        //if the enemay is x aligned with player
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon) {
            //chose y direction as to reduce y distance to player
            yDir = target.position.y > transform.position.y ? 1 : -1;
        } else {
            //otherwise chose x direction as to reduce x distance to player
            xDir = target.position.x > transform.position.x ? 1 : -1;
        }

        AttmeptMove<Player>(xDir, yDir);
    }

    protected override void OnCantMove<T>(T component) {
        Player hitPlayer = component as Player;

        animator.SetTrigger("enemyAttack");

        SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);

        hitPlayer.LoseFood(playerDamage);
    }
}
