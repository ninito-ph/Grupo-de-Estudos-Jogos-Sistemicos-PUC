using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// Test world generator system. Obsolete. No known problems.
/// </summary>
/// 
public class TestWorldGenerator : MonoBehaviour
{
    #region Field Declarations
    public readonly int CHUNK_SIZE_X = 16;
    public readonly int CHUNK_SIZE_Z = 4;

    public readonly float HEX_SIZE_X = 0.866025f; //Holds half of x offset
    public readonly float HEX_SIZE_Z = 1.5f; //Holds z offset
    
    #endregion
    
    #region Unity Callbacks 
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        CreateChunkMesh(0,0);
        CreateChunkMesh(0,1);
        CreateChunkMesh(1,0);
        CreateChunkMesh(1,1);
    }

    /// <summary>
    /// Generate a mesh for a chunk with given position
    /// </summary>
    /// <param name="cx">ChunkPos X</param>
    /// <param name="cz">ChunkPos Z</param>
    public void CreateChunkMesh(int cx,int cz)
    {
        GameObject empty = new GameObject();
        empty.transform.position = new Vector3(cx * HEX_SIZE_X * CHUNK_SIZE_X,0,cz * HEX_SIZE_Z * CHUNK_SIZE_Z * 2);
        MeshFilter filter = empty.AddComponent<MeshFilter>();
        empty.AddComponent<MeshRenderer>()/*.material = mat*/;

        ProjetoAbelhas.WorldMeshBuilder builder = new ProjetoAbelhas.WorldMeshBuilder(256000,100,100);

        for(int bx = 0; bx < CHUNK_SIZE_X; bx++)
        {
            for(int bz = 0; bz < CHUNK_SIZE_Z; bz ++)
            {
                #region Current Hex

                float x = cx * HEX_SIZE_X * 16 + (bx * HEX_SIZE_X);
                float z = cz * HEX_SIZE_Z * 4 * 2 + (HEX_SIZE_Z * ((bz) * 2 + bx%2));

                float ch = GetPointAtPerlin(x,z);
            
                #endregion
                #region Check Neighbour Hex

                float off = 0.866025f * 2; //Used to retrieve adjascent heights 

                float[] h = {
                    ch - GetPointAtPerlin(x + math.sin(math.PI/180 * 30) * off,z + math.cos(math.PI/180 * 30) * off),
                    ch - GetPointAtPerlin(x + math.sin(math.PI/180 * 90) * off,z + math.cos(math.PI/180 * 90) * off),
                    ch - GetPointAtPerlin(x + math.sin(math.PI/180 * 150) * off,z + math.cos(math.PI/180 * 150) * off),
                    ch - GetPointAtPerlin(x + math.sin(math.PI/180 * 210) * off,z + math.cos(math.PI/180 * 210) * off),
                    ch - GetPointAtPerlin(x + math.sin(math.PI/180 * 270) * off,z + math.cos(math.PI/180 * 270) * off),
                    ch - GetPointAtPerlin(x + math.sin(math.PI/180 * 330) * off,z + math.cos(math.PI/180 * 330) * off)
                };
                builder.AddStackedHexagon((bx * HEX_SIZE_X),0,(HEX_SIZE_Z * ((bz) * 2 + bx%2)),ch,h,
                Color.white);

                #endregion  
            }
        }

        filter.mesh = builder.Create();
        empty.AddComponent<MeshCollider>().sharedMesh = filter.mesh;

        builder.Clear();
    }

    /// <summary>
    /// Used to calculate heights for all vertices of an hexagon
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static float[] GetHeightsFor(float x,float z)
    {
        return new float[]{
            GetPointAtPerlin(x + 0.866025f,z + 0.5f),
            GetPointAtPerlin(x + 0.866025f,z - 0.5f),
            GetPointAtPerlin(x,z - 1f),
            GetPointAtPerlin(x - 0.866025f,z - 0.5f),
            GetPointAtPerlin(x - 0.866025f,z + 0.5f),
            GetPointAtPerlin(x,z + 1f)
        };
    }

    /// <summary>
    /// Get point height at perlin noise
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public static float GetPointAtPerlin(float x,float z)
    {
        return Mathf.PerlinNoise(x * 1/64f,z * 1/64f) * 18F;
    }
    #endregion
}

