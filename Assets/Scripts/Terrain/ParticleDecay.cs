using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleDecay : MonoBehaviour {

    private ParticleSystem system;

	// Use this for initialization
	void Start () {
        system = gameObject.GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (system && !system.IsAlive())
        {
            Destroy(gameObject);
        }
	}
}
