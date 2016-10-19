using UnityEngine;
using System.Collections;

public class SimpleController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotateSpeed = 10f;
    public TerrainGenerator generator;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float rot = Input.GetAxis("Horizontal") * Time.deltaTime * rotateSpeed;
        float move = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        transform.Rotate(Vector3.up, rot, Space.Self);
        transform.position += transform.forward * move;
    }
}
