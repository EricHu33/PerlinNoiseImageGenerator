using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualize3D : MonoBehaviour
{
    public Renderer textureRender;
    public int Width;
    public int Height;
    public int Seed;
    public float Size = 64;

    [SerializeField, Range(1, 5)]
    private int FractalLevel = 1;

    public GameObject ParticlePrefab;
    public int ParticleAmount = 0;
    public Vector3Int GridSize;
    private Dictionary<Vector3Int, float> m_flowFields;
    private bool isUpdated = false;


    List<GameObject> m_cubes;

    public void Start()
    {
        m_flowFields = new Dictionary<Vector3Int, float>();
        m_cubes = new List<GameObject>();
        SpawnCubes();
        //SampleNoiseFlowField();
    }
    private void SpawnCubes()
    {
        for (int x = 0; x < 1000; x++)
        {
            var obj = Instantiate(ParticlePrefab);
            obj.transform.SetParent(transform);
            obj.transform.position = transform.position + new Vector3(Random.Range(-GridSize.x * 0.5f, GridSize.x * 0.5f), Random.Range(-GridSize.y * 0.5f, GridSize.y * 0.5f), 0);
            obj.transform.localScale = Vector3.one * 0.3f;
            m_cubes.Add(obj);
        }
    }

    public void Draw3DNosieTexture()
    {
        var map = new float[GridSize.x, GridSize.y];
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                map[GridSize.x - 1 - x, y] = m_flowFields[new Vector3Int(x, y, 0)];
            }
        }
        var texture = TextureGenerator.GenerateTextureFromNoiseMap(map);
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void OnValidate()
    {
        if (Width < 1)
        {
            Width = 1;
        }
        if (Height < 1)
        {
            Height = 1;
        }
    }

    public float PerlinNoise3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);

        return (xy + xz + yz + yx + zx + zy) / 6;
    }

    private void Update()
    {
        var scale = 1.0f / Size;
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    var f = PerlinNoise3D(x, y, z);
                    m_flowFields[new Vector3Int(x, y, z)] = (Perlin.Fbm((Seed + x * scale), Seed + y * scale, (z + Time.time) * scale, FractalLevel) + 1) * 0.5f;
                }
            }
        }
        isUpdated = true;
        foreach (var cube in m_cubes)
        {
            if (cube.transform.position.x > transform.position.x + GridSize.x * 0.5f)
            {
                cube.transform.position = new Vector3(transform.position.x - GridSize.x * 0.5f, cube.transform.position.y, cube.transform.position.z);
            }
            if (cube.transform.position.x < transform.position.x - GridSize.x * 0.5f)
            {
                cube.transform.position = new Vector3(transform.position.x + GridSize.x * 0.5f, cube.transform.position.y, cube.transform.position.z);
            }
            if (cube.transform.position.y > transform.position.y + GridSize.y * 0.5f)
            {
                cube.transform.position = new Vector3(cube.transform.position.x, transform.position.y - GridSize.y * 0.5f, cube.transform.position.z);
            }
            if (cube.transform.position.y < transform.position.y - GridSize.y * 0.5f)
            {
                cube.transform.position = new Vector3(cube.transform.position.x, transform.position.y + GridSize.y * 0.5f, cube.transform.position.z);
            }
            if (cube.transform.position.z > transform.position.z + GridSize.z * 0.5f)
            {
                cube.transform.position = new Vector3(cube.transform.position.x, cube.transform.position.y, transform.position.z - GridSize.z * 0.5f);
            }
            if (cube.transform.position.z < transform.position.z - GridSize.z * 0.5f)
            {
                cube.transform.position = new Vector3(cube.transform.position.x, cube.transform.position.y, transform.position.z + GridSize.z * 0.5f);
            }
            //var bounds = new Bounds(transform.position, new Vector3(GridSize.x, GridSize.y, GridSize.z));
            var x = Mathf.Clamp(Mathf.RoundToInt(transform.position.x + cube.transform.position.x + GridSize.x * 0.5f), 0, GridSize.x - 1);
            var y = Mathf.Clamp(Mathf.RoundToInt(transform.position.y + cube.transform.position.y + GridSize.y * 0.5f), 0, GridSize.y - 1);
            var z = Mathf.Clamp(Mathf.RoundToInt(transform.position.z + cube.transform.position.z + GridSize.y * 0.5f), 0, GridSize.z - 1);
            var noiseValue = m_flowFields[new Vector3Int(x, y, z)];
            var noiseDir = new Vector3(Mathf.Cos(noiseValue * 2 * Mathf.PI), Mathf.Sin(noiseValue * 2 * Mathf.PI), Mathf.Cos(noiseValue * Mathf.PI));
            //cube.transform.position += noiseDir * Time.deltaTime * 5f;
            var newDir = Vector3.RotateTowards(cube.transform.forward, noiseDir, 5 * Time.deltaTime, 0.0f);
            cube.transform.rotation = Quaternion.LookRotation(newDir);
            cube.transform.position += cube.transform.forward * 5 * Time.deltaTime;
            //var bounds = new Bounds(transform.position, new Vector3(GridSize.x, GridSize.y, GridSize.z));
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x, GridSize.y, GridSize.z));
        var scale = 1.0f / Size;
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    var noiseValue = m_flowFields[new Vector3Int(x, y, z)];
                    var noiseDir = new Vector3(Mathf.Cos(noiseValue * 2 * Mathf.PI), Mathf.Sin(noiseValue * 2 * Mathf.PI), Mathf.Cos(noiseValue * 2 * Mathf.PI));
                    var colorVal = noiseDir.normalized;
                    Gizmos.color = new Color(colorVal.x, colorVal.y, colorVal.z, 1);
                    var pos = (new Vector3(x - GridSize.x * 0.5f, y - GridSize.y * 0.5f, z - GridSize.z * 0.5f) + transform.position);
                    var endPos = pos + noiseDir.normalized;
                    Gizmos.DrawLine(pos, endPos);
                    // m_flowFields[x * GridSize.x + y * GridSize.y + z] = noiseValue;
                }
            }
        }
        Draw3DNosieTexture();
    }
}
