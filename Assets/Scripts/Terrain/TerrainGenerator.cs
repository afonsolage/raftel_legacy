using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum VoxelType
{
    GRASS,
    ROCK,
    DIRT,
    WOOD,
    LEAF,
}

[System.Serializable]
public struct TerrainVoxel
{
    public VoxelType type;
    public GameObject prefab;
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
    public Vector2 size;

    public Transform followed;

    public GameObject destroyEffect;
    public List<TerrainVoxel> voxelList;
    private Dictionary<Vector2, GameObject> objectMap;
    private Vector3 followedCurrentPosition;

    private int mapSize;

    void Awake()
    {
        mapSize = (int)(size.x * 2 * size.y * 2);
        objectMap = new Dictionary<Vector2, GameObject>(mapSize, new Vector2Comparer());
    }

    // Use this for initialization
    void Start()
    {

        for (int y = (int)-size.y; y < size.y; y++)
        {
            for (int x = (int)-size.x; x < size.x; x++)
            {
                AddVoxel(x, y);
            }
        }

        followedCurrentPosition = followed.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = followed.transform.position;

        pos -= followedCurrentPosition;

        pos.x = floorRound(pos.x);
        pos.y = floorRound(pos.y);
        pos.z = floorRound(pos.z);

        Move(pos);
    }

    private void AddVoxel(int x, int y)
    {
        VoxelType type = VoxelType.GRASS;

        double noise = TerrainNoise.getHeight(x, y) * 10;

        if (noise < 0f)
        {
            type = VoxelType.GRASS;
        }
        else if (noise < 2f)
        {
            type = VoxelType.DIRT;
        }
        else
        {
            type = VoxelType.ROCK;
        }

        if (type == VoxelType.GRASS && UnityEngine.Random.Range(0, 100) < 5)
        {
            bool isNeighborTree = false;

            int r = UnityEngine.Random.Range(0, 100);

            int trunkHeight = (r > 90) ? 6 : (r > 70) ? 5 : (r > 50) ? 4 : 3;

            r = UnityEngine.Random.Range(0, 100);

            int leafHeight = (r > 90) ? 5 : (r > 70) ? 4 : (r > 50) ? 3 : 2;

            GameObject go;
            Vector2 p;
            for (int a = -leafHeight; a <= leafHeight; a++)
            {
                for (int b = -leafHeight; b <= leafHeight; b++)
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
                AddTree(x, y, trunkHeight, trunkHeight + leafHeight);
                return;
            }
        }

        objectMap[new Vector2(x, y)] = AddCube("Terrain " + x + ", " + y, type, x, 0, y, gameObject.transform);
    }

    private void AddTree(int x, int y, int trunkHeight, int leafHeight)
    {
        //Add tree wood
        GameObject tree = new GameObject("Terrain Tree " + x + ", " + y);
        objectMap[new Vector2(x, y)] = tree;

        tree.transform.position = new Vector3(x, 0, y);
        tree.transform.parent = gameObject.transform;

        VoxelTree v = tree.AddComponent<VoxelTree>();
        v.trunk_height = trunkHeight;
        v.leaf_height = leafHeight;
        v.generator = this;
    }

    public GameObject AddCube(string name, VoxelType type, float x, float y, float z, Transform parent)
    {
        GameObject go = Instantiate(FindVoxel(type));
        go.tag = "Voxel";
        go.name = name;
        go.transform.parent = parent;
        go.transform.localPosition = new Vector3(x, y, z);

        return go;
    }

    public void RemoveObject(GameObject go)
    {
        DestroyObject(go);
    }

    private void RemoveVoxel(int x, int y)
    {
        GameObject go = null;
        Vector2 pos = new Vector2(x, y);

        if (objectMap == null)
        {
            Debug.LogError("Object Map is null!!");
            return;
        }

        if (!objectMap.TryGetValue(pos, out go) || go == null)
        {
            return;
        }

        go.transform.parent = null;
        DestroyObject(go);
        objectMap[pos] = null;
    }

    public GameObject FindVoxel(VoxelType type)
    {
        foreach (TerrainVoxel voxel in voxelList)
        {
            if (voxel.type == type)
                return voxel.prefab;
        }

        Debug.LogError("Invalid TerrainType: " + type);
        return voxelList[0].prefab;
    }

    public void AddDestroyCubeEffect(GameObject go)
    {
        GameObject effect = Instantiate(destroyEffect);

        Renderer renderer = go.GetComponent<Renderer>();
        Renderer effectRenderer = effect.GetComponent<Renderer>();

        if (renderer == null || effectRenderer == null)
            return;

        effectRenderer.material = renderer.material;

        effect.transform.position = go.transform.position;
    }

    private void Move(Vector3 movement)
    {

        if (movement.x == 0 && movement.z == 0)
        {
            return;
        }

        if (movement.x >= 1.0f)
        {
            //Move right
            for (int a = 0; a < movement.x; movement.x--)
            {
                for (int y = (int)-size.y; y < size.y; y++)
                {
                    RemoveVoxel((int)(followedCurrentPosition.x - size.x), (int)(followedCurrentPosition.z + y));
                }

                for (int y = (int)-size.y; y < size.y; y++)
                {
                    AddVoxel((int)(followedCurrentPosition.x + size.x), (int)(followedCurrentPosition.z + y));
                }

                followedCurrentPosition.x += 1;
            }
        }
        else if (movement.x <= -1.0f)
        {
            //Move left

            for (int a = 0; a > movement.x; movement.x++)
            {
                followedCurrentPosition.x -= 1;

                for (int y = (int)-size.y; y < size.y; y++)
                {
                    RemoveVoxel((int)(followedCurrentPosition.x + size.x), (int)(followedCurrentPosition.z + y));
                }

                for (int y = (int)-size.y; y < size.y; y++)
                {
                    AddVoxel((int)(followedCurrentPosition.x - size.x), (int)(followedCurrentPosition.z + y));
                }
            }
        }

        if (movement.z >= 1.0f)
        {
            //Move up

            for (int a = 0; a < movement.z; movement.z--)
            {

                for (int x = (int)-size.x; x < size.x; x++)
                {
                    RemoveVoxel((int)(followedCurrentPosition.x + x), (int)(followedCurrentPosition.z - size.y));
                }

                for (int x = (int)-size.x; x < size.x; x++)
                {
                    AddVoxel((int)(followedCurrentPosition.x + x), (int)(followedCurrentPosition.z + size.y));
                }

                followedCurrentPosition.z += 1;
            }
        }
        else if (movement.z <= -1.0f)
        {
            //Move down
            for (int a = 0; a > movement.z; movement.z++)
            {
                followedCurrentPosition.z -= 1;

                for (int x = (int)-size.x; x < size.x; x++)
                {
                    RemoveVoxel((int)(followedCurrentPosition.x + x), (int)(followedCurrentPosition.z + size.y));
                }

                for (int x = (int)-size.x; x < size.x; x++)
                {
                    AddVoxel((int)(followedCurrentPosition.x + x), (int)(followedCurrentPosition.z - size.y));
                }
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
