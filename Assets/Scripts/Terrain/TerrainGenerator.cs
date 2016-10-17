using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum TerrainType
{
    GRASS,
    ROCK,
    DIRT,
}

[System.Serializable]
public class TerrainSprite
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

    private Vector2 spriteOffset;
    private Dictionary<Vector2, GameObject> objectMap;

    private Vector2 followedCurrentPosition;

    private static float a = 0;

    // Use this for initialization
    void Start()
    {
        spriteOffset.x = sprite.bounds.extents.x;
        spriteOffset.y = sprite.bounds.extents.y / 2;

        objectMap = new Dictionary<Vector2, GameObject>((int)(size.x * 2 * size.y * 2), new Vector2Comparer());

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

        GameObject go = new GameObject("Terrain " + x + ", " + y);
        go.tag = "Voxel";
        objectMap[new Vector2(x, y)] = go;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = FindSprite(type);
        renderer.sortingLayerName = "Terrain";
        Vector3 pos = ToIsometric(x, y);

        pos.z = x + (y / 1.000f);

        go.transform.position = pos;
        go.transform.parent = gameObject.transform;
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

    private Vector2 ToIsometric(float x, float y)
    {
        return new Vector2((x - y) * spriteOffset.x, (x + y) * spriteOffset.y);
    }

    private Vector2 FromIsometric(float x, float y)
    {
        return new Vector2(floorRound((x / spriteOffset.x + y / spriteOffset.y) / 2),
            floorRound((y / spriteOffset.y - (x / spriteOffset.x)) / 2));
    }

    private Sprite FindSprite(TerrainType type)
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
            Debug.Log("Moving terrain " + movement.x + " unit(s) to right");
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
            Debug.Log("Moving terrain " + movement.x + " unit(s) to left");
            //Move right

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
            Debug.Log("Moving terrain " + movement.y + " unit(s) to up");
            //Move right

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
            Debug.Log("Moving terrain " + movement.y + " unit(s) to down");
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
