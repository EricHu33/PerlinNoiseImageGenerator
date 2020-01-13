using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Jobs;

public class Visualize3D : MonoBehaviour
{
    public Renderer textureRender;
    public int m_seed;
    public float Size = 64;
    public bool m_useJobSystem;

    [SerializeField, Range(1, 5)]
    private int m_fractalLevel = 1;

    public GameObject ParticlePrefab;
    public int ParticleAmount = 0;
    public Vector3Int m_gridSize;
    private float[] m_flowFields;
    private NativeArray<float> m_perlins;
    private NativeArray<Vector3Int> m_gridInputs;
    private PerlineNoiseJob m_perlinJob;
    private JobHandle m_JobHandle;
    private TransformAccessArray m_transformsAccessArray;
    private TransformJob m_transJob;
    private JobHandle m_transJobHandle;
    private List<Transform> m_cubes;

    public void Start()
    {
        m_perlins = new NativeArray<float>((m_gridSize.x * m_gridSize.y * m_gridSize.z), Allocator.Persistent);
        m_flowFields = new float[m_gridSize.x * m_gridSize.y * m_gridSize.z];
        m_cubes = new List<Transform>();
        m_gridInputs = new NativeArray<Vector3Int>(m_gridSize.x * m_gridSize.y * m_gridSize.z, Allocator.Persistent);
        var index = 0;
        for (int x = 0; x < m_gridSize.x; x++)
        {
            for (int y = 0; y < m_gridSize.y; y++)
            {
                for (int z = 0; z < m_gridSize.z; z++)
                {
                    m_gridInputs[index] = new Vector3Int(x, y, z);
                    index++;
                }
            }
        }
        SpawnCubes();
        m_transformsAccessArray = new TransformAccessArray(m_cubes.ToArray());

        //SampleNoiseFlowField();
    }
    private void SpawnCubes()
    {
        for (int x = 0; x < ParticleAmount; x++)
        {
            var obj = Instantiate(ParticlePrefab);
            obj.transform.SetParent(transform);
            obj.transform.position = transform.position + new Vector3(UnityEngine.Random.Range(-m_gridSize.x * 0.5f, m_gridSize.x * 0.5f), UnityEngine.Random.Range(-m_gridSize.y * 0.5f, m_gridSize.y * 0.5f), 0);
            obj.transform.localScale = Vector3.one * 0.3f;
            m_cubes.Add(obj.transform);
        }
    }

    public void Draw3DNosieTexture()
    {
        var map = new float[m_gridSize.x, m_gridSize.y];
        for (int x = 0; x < m_gridSize.x; x++)
        {
            for (int y = 0; y < m_gridSize.y; y++)
            {
                map[m_gridSize.x - 1 - x, y] = m_flowFields[m_gridSize.x + (x + m_gridSize.y * y)];
            }
        }
        var texture = TextureGenerator.GenerateTextureFromNoiseMap(map);
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    [BurstCompile]
    public struct PerlineNoiseJob : IJobParallelFor
    {
        public float Timestamp;
        public int FractalLevel;
        public float Scale;
        public int Seed;
        [ReadOnly]
        public NativeArray<Vector3Int> inputs;
        public NativeArray<float> output;

        public void Execute(int index)
        {
            output[index] = (Perlin.Fbm((Seed + inputs[index].x * Scale), Seed + inputs[index].y * Scale, (inputs[index].z + Timestamp) * Scale, FractalLevel) + 1) * 0.5f;
        }
    }

    [BurstCompile]
    public struct TransformJob : IJobParallelForTransform
    {
        [ReadOnly]
        public Vector3 OwnerPos;
        [ReadOnly]
        public float DeltaTime;
        [ReadOnly]
        public Vector3Int GridSize;
        [ReadOnly]
        public NativeArray<float> noiseArray;

        public void Execute(int index, TransformAccess transform)
        {
            if (transform.position.x > OwnerPos.x + GridSize.x * 0.5f)
            {
                transform.position = new Vector3(OwnerPos.x - GridSize.x * 0.5f, transform.position.y, transform.position.z);
            }
            if (transform.position.x < OwnerPos.x - GridSize.x * 0.5f)
            {
                transform.position = new Vector3(OwnerPos.x + GridSize.x * 0.5f, transform.position.y, transform.position.z);
            }
            if (transform.position.y > OwnerPos.y + GridSize.y * 0.5f)
            {
                transform.position = new Vector3(transform.position.x, OwnerPos.y - GridSize.y * 0.5f, transform.position.z);
            }
            if (transform.position.y < OwnerPos.y - GridSize.y * 0.5f)
            {
                transform.position = new Vector3(transform.position.x, OwnerPos.y + GridSize.y * 0.5f, transform.position.z);
            }
            if (transform.position.z > OwnerPos.z + GridSize.z * 0.5f)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, OwnerPos.z - GridSize.z * 0.5f);
            }
            if (transform.position.z < OwnerPos.z - GridSize.z * 0.5f)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, OwnerPos.z + GridSize.z * 0.5f);
            }

            var x = Mathf.Clamp(Mathf.RoundToInt(OwnerPos.x + transform.position.x + GridSize.x * 0.5f), 0, GridSize.x - 1);
            var y = Mathf.Clamp(Mathf.RoundToInt(OwnerPos.y + transform.position.y + GridSize.y * 0.5f), 0, GridSize.y - 1);
            var z = Mathf.Clamp(Mathf.RoundToInt(OwnerPos.z + transform.position.z + GridSize.y * 0.5f), 0, GridSize.z - 1);
            var noiseValue = noiseArray[x + GridSize.y * (y + GridSize.z * z)];
            var noiseDir = new Vector3(Mathf.Cos(noiseValue * 2 * Mathf.PI), Mathf.Sin(noiseValue * 2 * Mathf.PI), Mathf.Cos(noiseValue * Mathf.PI));
            var newDir = Vector3.RotateTowards(transform.rotation * Vector3.forward, noiseDir, 5 * DeltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);
            transform.position += (transform.rotation * Vector3.forward) * 15 * DeltaTime;
        }
    }

    public struct CopyArrayAndAddJob : IJob
    {
        [ReadOnly]
        public NativeArray<float> InArray;
        public NativeArray<float> OutArray;

        public void Execute()
        {
            for (int i = 0; i < InArray.Length; i++)
            {
                var temp = InArray[i] + 1;
                OutArray[i] = temp;
            }
        }
    }

    // Job adding two floating point values together
    public struct SumJob : IJob
    {
        public float a;
        public float b;
        public NativeArray<float> result;

        public void Execute()
        {
            result[0] = a + b;
        }
    }

    public struct ApplyMutilNumJob : IJob
    {
        public float a;
        public NativeArray<float> result;

        public void Execute()
        {
            result[0] = result[0] * a;
        }
    }

    public void BurstTriggerClick()
    {
        m_useJobSystem = !m_useJobSystem;
    }

    private void Update()
    {
        var scale = 1.0f / Size;
        if (m_useJobSystem)
        {
            m_perlinJob = new PerlineNoiseJob()
            {
                inputs = m_gridInputs,
                output = m_perlins,
                Timestamp = Time.time,
                FractalLevel = m_fractalLevel,
                Scale = scale,
                Seed = m_seed,
            };
            m_transJob = new TransformJob()
            {
                OwnerPos = transform.position,
                GridSize = m_gridSize,
                noiseArray = m_perlins,
                DeltaTime = Time.deltaTime,
            };
            m_JobHandle = m_perlinJob.Schedule(m_gridSize.x * m_gridSize.y * m_gridSize.z, 35);
            m_transJobHandle = m_transJob.Schedule(m_transformsAccessArray, m_JobHandle);

        }
        else
        {
            for (int x = 0; x < m_gridSize.x; x++)
            {
                for (int y = 0; y < m_gridSize.y; y++)
                {
                    for (int z = 0; z < m_gridSize.z; z++)
                    {
                        m_flowFields[x + m_gridSize.y * (y + m_gridSize.z * z)] = (Perlin.Fbm((m_seed + x * scale), m_seed + y * scale, (z + Time.time) * scale, m_fractalLevel) + 1) * 0.5f;
                    }
                }
            }
            foreach (var cube in m_cubes)
            {
                if (cube.position.x > transform.position.x + m_gridSize.x * 0.5f)
                {
                    cube.position = new Vector3(transform.position.x - m_gridSize.x * 0.5f, cube.position.y, cube.position.z);
                }
                if (cube.position.x < transform.position.x - m_gridSize.x * 0.5f)
                {
                    cube.position = new Vector3(transform.position.x + m_gridSize.x * 0.5f, cube.position.y, cube.position.z);
                }
                if (cube.position.y > transform.position.y + m_gridSize.y * 0.5f)
                {
                    cube.position = new Vector3(cube.position.x, transform.position.y - m_gridSize.y * 0.5f, cube.position.z);
                }
                if (cube.position.y < transform.position.y - m_gridSize.y * 0.5f)
                {
                    cube.position = new Vector3(cube.position.x, transform.position.y + m_gridSize.y * 0.5f, cube.position.z);
                }
                if (cube.position.z > transform.position.z + m_gridSize.z * 0.5f)
                {
                    cube.position = new Vector3(cube.position.x, cube.position.y, transform.position.z - m_gridSize.z * 0.5f);
                }
                if (cube.position.z < transform.position.z - m_gridSize.z * 0.5f)
                {
                    cube.position = new Vector3(cube.position.x, cube.position.y, transform.position.z + m_gridSize.z * 0.5f);
                }

                var x = Mathf.Clamp(Mathf.RoundToInt(transform.position.x + cube.position.x + m_gridSize.x * 0.5f), 0, m_gridSize.x - 1);
                var y = Mathf.Clamp(Mathf.RoundToInt(transform.position.y + cube.position.y + m_gridSize.y * 0.5f), 0, m_gridSize.y - 1);
                var z = Mathf.Clamp(Mathf.RoundToInt(transform.position.z + cube.position.z + m_gridSize.y * 0.5f), 0, m_gridSize.z - 1);
                var noiseValue = m_flowFields[x + m_gridSize.y * (y + m_gridSize.z * z)];
                var noiseDir = new Vector3(Mathf.Cos(noiseValue * 2 * Mathf.PI), Mathf.Sin(noiseValue * 2 * Mathf.PI), Mathf.Cos(noiseValue * Mathf.PI));
                var newDir = Vector3.RotateTowards(cube.forward, noiseDir, 5 * Time.deltaTime, 0.0f);
                cube.rotation = Quaternion.LookRotation(newDir);
                cube.position += cube.forward * 15 * Time.deltaTime;
            }
        }
    }

    public void LateUpdate()
    {
        if (m_useJobSystem)
        {
            m_transJobHandle.Complete();
            m_perlins.CopyTo(m_flowFields);
        }
    }



    private void OnDisable()
    {
        m_perlins.Dispose();
        m_gridInputs.Dispose();
        m_transformsAccessArray.Dispose();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(m_gridSize.x, m_gridSize.y, m_gridSize.z));
        var scale = 1.0f / Size;
        for (int x = 0; x < m_gridSize.x; x++)
        {
            for (int y = 0; y < m_gridSize.y; y++)
            {
                for (int z = 0; z < m_gridSize.z; z++)
                {
                    var noiseValue = m_flowFields[x + m_gridSize.y * (y + m_gridSize.z * z)];
                    var noiseDir = new Vector3(Mathf.Cos(noiseValue * 2 * Mathf.PI), Mathf.Sin(noiseValue * 2 * Mathf.PI), Mathf.Cos(noiseValue * 2 * Mathf.PI));
                    var colorVal = noiseDir.normalized;
                    Gizmos.color = new Color(colorVal.x, colorVal.y, colorVal.z, 1);
                    var pos = (new Vector3(x - m_gridSize.x * 0.5f, y - m_gridSize.y * 0.5f, z - m_gridSize.z * 0.5f) + transform.position);
                    var endPos = pos + noiseDir.normalized;
                    Gizmos.DrawLine(pos, endPos);
                    // m_flowFields[x * GridSize.x + y * GridSize.y + z] = noiseValue;
                }
            }
        }
        Draw3DNosieTexture();
    }
}
