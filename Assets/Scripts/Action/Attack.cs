using UnityEngine;

public class Attack : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonUp("Fire1"))
        {
            Vector3 pos = transform.position;
            //pos.y += 0.5f;
            RaycastHit hit;

            if (Physics.Raycast(pos, transform.forward, out hit, 1f))
            {
                GameObject go = hit.transform.gameObject;
                if (go != null)
                {
                    Debug.Log("Attacking: " + go.name);
                }
            }
        }
    }
}
