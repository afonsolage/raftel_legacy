using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum TerrainType
{
    GRASS,
    ROCK,
    DIRT,
    WOOD,
    LEAF,
}

[System.Serializable]
public struct TerrainSprite
{
    public TerrainType type;
    public Sprite sprite;
}

public class Vector2Comparer : IEqualityComparer<Vector2>
{
    public bool Equals(Vector2 a, Vector2 b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public int GetHashCode(Vector2 obj)
    {
        return obj.GetHashCode();
    }
}

public class TerrainGenerator : MonoBehaviour
{
    public Sprite sprite;

    public Vector2 size;

    public Transform followed;

    public List<TerrainSprite> spriteList;

    public Vector2 spriteOffset;
    private Dictionary<Vector2, GameObject> objectMap;

    private Vector2 followedCurrentPosition;

    public int orderingOffset { get; set; }
    private static float a;
    private bool updateOffset;
    private int mapSize;

    void Awake()
    {
        spriteOffset.x = sprite.bounds.extents.x;
        spriteOffset.y = sprite.bounds.extents.y / 2;

        mapSize = (int)(size.x * 2 * size.y * 2);
        updateOffset = false;
    }

    // Use this for initialization
    void Start()
    {
        objectMap = new Dictionary<Vector2, GameObject>(mapSize, new Vector2Comparer());

        for (int y = (int)-size.y; y < size.y; y++)
        {
            for (int x = (int)-size.x; x < size.x; x++)
            {
                AddVoxel(x, y);
            }
        }

        followedCurrentPosition = FromIsometric(followed.transform.position.x, followed.transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = FromIsometric(followed.transform.position.x, followed.transform.position.y);

        pos -= followedCurrentPosition;

        Move(pos);

        //If sorting layer is near the int limits, let's reorder adding a offset to get away from limit.
        if (updateOffset)
        {
            orderingOffset = (orderingOffset == 0) ? 10000 : 0;

            foreach (KeyValuePair<Vector2, GameObject> pair in objectMap)
            {
                if (pair.Value == null)
                    continue;

                SpriteRenderer r = pair.Value.GetComponent<SpriteRenderer>();

                if (r != null)
                {
                    r.sortingOrder = -((int)pair.Key.x + orderingOffset) * (int)size.y + -((int)pair.Key.y + orderingOffset);
                }
            }

            updateOffset = false;
        }
    }

    private void AddVoxel(int x, int y)
    {
        TerrainType type = TerrainType.GRASS;

        double noise = TerrainNoise.getHeight(x, y) * 10;

        if (noise < 0f)
        {
            type = TerrainType.GRASS;
        }
        else if (noise < 2f)
        {
            type = TerrainType.DIRT;
        }
        else
        {
            type = TerrainType.ROCK;
        }


        if (type == TerrainType.GRASS && UnityEngine.Random.Range(0, 100) < 5)
        {
            bool isNeighborTree = false;

            GameObject go;
            Vector2 p;
            for (int a = -3; a <= 3; a++)
            {
                for (int b = -3; b <= 3; b++)
                {
                    p = new Vector2(x + a, y + b);
                    if (objectMap.TryGetValue(p, out go) && go != null && go.GetComponent<VoxelTree>() != null)
                    {
                        isNeighborTree = true;
                        break;
                    }
                }
            }

            if (!isNeighborTree)
            {
                AddTree(x, y);
                return;
            }
        }



        Vector2 tmp = ToIsometric(x, y);
        objectMap[new Vector2(x, y)] = AddSprite("Terrain " + x + ", " + y, type, tmp.x, tmp.y, gameObject.transform);

    }

    private void AddTree(int x, int y)
    {
        //Add tree wood
        GameObject tree = new GameObject("Terrain Tree " + x + ", " + y);
        objectMap[new Vector2(x, y)] = tree;

        tree.transform.position = new Vector3(x, y, 0);
        tree.transform.parent = gameObject.transform;

        VoxelTree v = tree.AddComponent<VoxelTree>();
        v.trunk_height = 4;
        v.leaf_height = 6;
        v.generator = this;
    }

    public GameObject AddSprite(string name, TerrainType type, float x, float y, float z, int sortingOrder, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.tag = "Voxel";

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = FindSprite(type);
        renderer.sortingLayerName = "Terrain";
        renderer.sortingOrder = 0;
        go.transform.position = new Vector3(x, y, z);
        go.transform.parent = parent;

        return go;
    }

    public GameObject AddSprite(string name, TerrainType type, float x, float y, Transform parent)
    {
        return AddSprite(name, type, x, y, y, 0, parent);
    }

    public void RemoveObject(GameObject go)
    {
        DestroyObject(go);
    }

    private void RemoveVoxel(int x, int y)
    {
        GameObject go;
        Vector2 pos = new Vector2(x, y);
        if (!objectMap.TryGetValue(pos, out go) || go == null)
        {
            return;
        }

        go.transform.parent = null;
        DestroyObject(go);
        objectMap[pos] = null;
    }

    public Vector2 ToIsometric(float x, float y, float z)
    {
        return new Vector3((x - y) * spriteOffset.x, (x + y) * spriteOffset.y, (x + z) * spriteOffset.y);
    }

    public Vector2 ToIsometric(float x, float y)
    {
        return new Vector2((x - y) * spriteOffset.x, (x + y) * spriteOffset.y);
    }

    public Vector2 FromIsometric(float x, float y)
    {
        return new Vector2(floorRound((x / spriteOffset.x + y / spriteOffset.y) / 2),
            floorRound((y / spriteOffset.y - (x / spriteOffset.x)) / 2));
    }

    public Sprite FindSprite(TerrainType type)
    {
        foreach (TerrainSprite sprite in spriteList)
        {
            if (sprite.type == type)
                return sprite.sprite;
        }

        Debug.LogError("Invalid TerrainType: " + type);
        return spriteList[0].sprite;
    }

    private void Move(Vector2 movement)
    {

        if (movement.x == 0 && movement.y == 0)
        {
            return;
        }

        if (movement.x >= 1.0f)
        {
            //Move right

            for (int y = (int)-size.y; y < size.y; y++)
            {
                RemoveVoxel((int)(followedCurrentPosition.x - size.x), (int)(followedCurrentPosition.y + y));
            }

            for (int y = (int)-size.y; y < size.y; y++)
            {
                AddVoxel((int)(followedCurrentPosition.x + size.x), (int)(followedCurrentPosition.y + y));
            }

            followedCurrentPosition.x += movement.x;
        }
        else if (movement.x <= -1.0f)
        {
            //Move left

            followedCurrentPosition.x += movement.x;

            for (int y = (int)-size.y; y < size.y; y++)
            {
                RemoveVoxel((int)(followedCurrentPosition.x + size.x), (int)(followedCurrentPosition.y + y));
            }

            for (int y = (int)-size.y; y < size.y; y++)
            {
                AddVoxel((int)(followedCurrentPosition.x - size.x), (int)(followedCurrentPosition.y + y));
            }
        }

        if (movement.y >= 1.0f)
        {
            //Move up

            for (int x = (int)-size.x; x < size.x; x++)
            {
                RemoveVoxel((int)(followedCurrentPosition.x + x), (int)(followedCurrentPosition.y - size.y));
            }

            for (int x = (int)-size.x; x < size.x; x++)
            {
                AddVoxel((int)(followedCurrentPosition.x + x), (int)(followedCurrentPosition.y + size.y));
            }

            followedCurrentPosition.y += movement.y;
        }
        else if (movement.y <= -1.0f)
        {
            //Move down

            followedCurrentPosition.y += movement.y;

            for (int x = (int)-size.x; x < size.x; x++)
            {
                RemoveVoxel((int)(followedCurrentPosition.x + x), (int)(followedCurrentPosition.y + size.y));
            }

            for (int x = (int)-size.x; x < size.x; x++)
            {
                AddVoxel((int)(followedCurrentPosition.x + x), (int)(followedCurrentPosition.y - size.y));
            }

        }
    }

    public float floorDiv(float x, float y)
    {
        float mod = x / y;

        // Check if x and y have different signs by using a XOR.
        // Also if mod not equals to zero, round down.

        if (((int)x ^ (int)y) < 0 && (mod * y != x))
        {
            mod--;
        }

        return mod;
    }

    public int floorRound(float value)
    {
        if (value < 0)
        {
            return (int)(value - 1);
        }
        else
        {
            return (int)value;
        }
    }
}
