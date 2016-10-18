using UnityEngine;
using System.Collections;

public class VoxelTree : MonoBehaviour
{

    private Vector2[] DIRECTIONS = { new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1) };

    public TerrainGenerator generator;
    public int trunk_height;
    public int leaf_height;

    private GameObject[] data;

    // Use this for initialization
    void Start()
    {
        //The leaft_height will create the same for all sizes but down.
        data = new GameObject[trunk_height + (leaf_height * 5)];

        int x = (int)transform.position.x;
        int y = (int)transform.position.y;

        //Render Trunk
        for (int i = 0; i < trunk_height; i++)
        {
            data[i] = generator.AddSprite("Terrain " + x + ", " + y + " - Tree Trunk " + i, TerrainType.WOOD, 100, x + i, y + i, gameObject.transform, true);
        }

        //Render Central Leaf
        for (int i = trunk_height; i < leaf_height; i++)
        {
            data[i] = generator.AddSprite("Terrain " + x + ", " + y + " - Tree Trunk " + i, TerrainType.LEAF, 100, x + i, y + i, gameObject.transform, true);
        }

        Vector2 basePos = new Vector2(x + trunk_height, y + trunk_height);
        int leafCount = leaf_height - trunk_height;

        //For each direction axis.
        for (int dirIdx = 0; dirIdx < DIRECTIONS.Length; dirIdx++)
        {
            Vector2 dir = DIRECTIONS[dirIdx];

            //Compute index base for given direction.
            int baseIdx = leaf_height + (dirIdx * leafCount);

            //Since in each direction we have to go keep reducing one leaf each unit in axis, let's start from -1.
            for (int i = baseIdx, a = 0; i < baseIdx + leafCount - 1; i++, a++)
            {
                //Advance n unit in given direction.
                Vector2 pos = basePos + (dir * (a + 1));

                //Add leaf from the base to the top 
                for (int k = 0; k < leafCount - (a + 1); k++)
                {
                    data[i] = generator.AddSprite("Terrain " + x + ", " + y + " - Tree Trunk " + i, TerrainType.LEAF, 100 - (int)((pos.x * leafCount) + pos.y), (int)pos.x + k, (int)pos.y + k, gameObject.transform, true);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Remove()
    {
        foreach (GameObject go in data)
        {
            if (go != null)
            {
                generator.RemoveObject(go);
            }
        }
    }
}