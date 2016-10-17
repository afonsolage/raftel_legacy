using UnityEngine;
using System.Collections;

public class SimpleController : MonoBehaviour {

    public float speed = 2f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        move *= Time.deltaTime * speed;

        transform.position += move;
	}
}
