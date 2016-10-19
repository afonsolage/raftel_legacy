using UnityEngine;

public class Attack : MonoBehaviour
{
    public TerrainGenerator generator;

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
            pos.y += 1f;
            RaycastHit hit;

            Debug.Log("Raycasting from: " + pos + " into: " + transform.forward);

            if (Physics.Raycast(pos, transform.forward, out hit, 2f))
            {
                GameObject go = hit.transform.gameObject;
                if (go != null)
                {
                    generator.AddDestroyCubeEffect(go);
                }
            }
        }
    }
}
