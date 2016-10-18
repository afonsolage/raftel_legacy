using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelTree : MonoBehaviour
{
    private Vector2[] DIRECTIONS = { new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1) };

    public TerrainGenerator generator;
    public int trunk_height;
    public int leaf_height;

    private Dictionary<Vector3, GameObject> objectMap;

    // Use this for initialization
    void Start()
    {
        //The leaft_height will create the same for all sizes but down.
        objectMap = new Dictionary<Vector3, GameObject>(trunk_height + (leaf_height * 5));

        //Render Central Leaf
        for (int i = leaf_height - 1; i >= trunk_height; i--)
        {
            objectMap[new Vector3(0, i, 0)] = generator.AddCube("Tree Leaf " + i, VoxelType.LEAF, 0, i, 0, gameObject.transform);
        }

        //Render Trunk
        for (int i = trunk_height - 1; i >= 0; i--)
        {
            objectMap[new Vector3(0, i, 0)] = generator.AddCube("Tree Trunk " + i, VoxelType.WOOD, 0, i, 0, gameObject.transform);
        }

        int leafCount = leaf_height - trunk_height;
        int zBase = trunk_height;

        //For each direction axis.
        for (int dirIdx = 0; dirIdx < DIRECTIONS.Length; dirIdx++)
        {
            Vector2 dir = DIRECTIONS[dirIdx];

            //Since in each direction we have to go keep reducing one leaf each unit in axis, let's start from -1.
            for (int i = 1; i < leafCount; i++)
            {
                //Advance n unit in given direction.
                Vector2 move = (dir * i);
                //Add leaf from the base to the top 
                for (int k = 0; k < leafCount - i; k++)
                {
                    Vector3 v = new Vector3(move.x, zBase + k, move.y);
                    objectMap[v] = generator.AddCube("Tree Leaf " + move , VoxelType.LEAF, v.x, v.y, v.z, gameObject.transform);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}