using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public Sprite dmgSprite;
    public int hp = 4;

    public AudioClip chopSound1;
    public AudioClip chopSound2;

    private SpriteRenderer spriteRenderer;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DamageWall(int loss) {
        SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);
        //show damage to wall
        spriteRenderer.sprite = dmgSprite;
        hp -= loss;
        if (hp <= 0) gameObject.SetActive(false); //if destroyed, deactivate it
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
