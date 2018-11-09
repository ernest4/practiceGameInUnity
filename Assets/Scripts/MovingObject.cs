using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//abstract, cannot be instantiated, only inherited from.
public abstract class MovingObject : MonoBehaviour {

    public float moveTime = .1f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxColider;
    private Rigidbody2D rb2D;
    private float inverseMoveTime;

	// Use this for initialization
    // Protected as we don't want the unity engine to execute this method
    // when this script is awoken, as at that point it might not be attached
    // to anything yet. The script which will inherit from this script will
    // take care of running the Start method.
	protected virtual void Start () {
        boxColider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
	}

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit) {
        //implicit cast between transform.position (Vector3) to
        //Vector2 so we can ignore the z axis.
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        //before doing a ray cast, disable the colider of the object
        //itself so the ray doesn't hit and detect collision with the
        //object itself. Reenable afterwards.
        boxColider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxColider.enabled = true;

        //if no collision, execture the smoothe movemenet as coroutine
        //(over series of frames). Otherwise, return false. The 'hit'
        //variable which was passed by refernce 'out' will be set and
        //available after this function call for any further processing
        //such as identifying location of obstacle.
        if (hit.transform == null) {
            StartCoroutine(SmoothMovement(end));
            return true;
        }
        return false;
    }

    protected IEnumerator SmoothMovement(Vector3 end) {
        //get sqaured (positive) distance between current position
        //of object and destination. Sqauring is more efficient than
        //using modulo magnitude.
        //ALSO!!!
        //We user transform.position to calculate distance because
        //transform is updated on every Update (every frame) while
        //we apply and move the object using rigidbody position
        //as it runs on every FixedUpdate every few frames or so.
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon) {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            //returning null means that this coroutine will run on the next frame
            yield return null;
            //if you want the corouting to run after certain period of time
            //regardless of how many frames have passed, use:
            //yield return new WaitForSeconds(...float seconds...);
        }
    }

    protected virtual void AttmeptMove<T> (int xDir, int yDir) where T : Component {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null) return; //Can move, not blocked.

        //get the component attached to the object that was hit with the ray
        T hitComponent = hit.transform.GetComponent<T>();

        //if blocked and we have hit something with the ray, pass that hit
        //object (wall? enemy?...) to the OnCantMove for further processing
        //that is customized by each class that inherits MovingObject.
        //e.g. Enemy: 
        // OnCantMove(player) ----> attack!!
        // OnCantMove(wall) -----> wait.
        //e.g. Player:
        // OnCantMove(enemy) -----> shout for help!!
        // OnCantMove(wall) -----> break it down.
        // etc.
        if (!canMove && hitComponent != null) OnCantMove(hitComponent);
    }

    //this will be implemented by the script that will inherit from this script
    protected abstract void OnCantMove<T>(T component) where T : Component;
	
	// Update is called once per frame
	void Update () {
		
	}
}
