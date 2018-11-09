using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public AudioSource efxSource;
    public AudioSource musicSource;

    //singleton
    public static SoundManager instance = null;

    //add a bit of variation to spice things up
    public float lowPitchRange = 0.95f; //-5%
    public float highPitchRange = 1.05f; //+5%

    void Awake() {
        //implement singleton
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        //keep sound manager in between level loads
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySingle(AudioClip clip) {
        efxSource.clip = clip;
        efxSource.Play();
    }

    public void RandomizeSfx(params AudioClip[] clips) {
        //add some random variation to spice it up a bit...
        int randomIndex = Random.Range(0, clips.Length);
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);

        efxSource.pitch = randomPitch;
        efxSource.clip = clips[randomIndex];
        efxSource.Play();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
