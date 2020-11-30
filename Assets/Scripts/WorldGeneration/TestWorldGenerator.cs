using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjetoAbelhas.WorldData;
using Unity.Mathematics;

/// <summary>
/// Test world generator system. Non-Obsolete. No known problems.
/// </summary>
/// 
public class TestWorldGenerator : MonoBehaviour
{
    #region Field Declarations
    public static World world;

    public Material defaultMaterial;
    public GameObject cube_pivot;
    public GameObject cube_pivot_display;

    #endregion
    
    #region Unity Callbacks 
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        world = new World();

        for(int i = 0; i < world.chunksX; i ++)
            for(int j = 0; j < world.chunksZ; j ++)
                CreateChunkMesh(i,j);

        cube_pivot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube_pivot_display = GameObject.CreatePrimitive(PrimitiveType.Cube);

        cube_pivot.transform.localScale = cube_pivot_display.transform.localScale = Vector3.one * 0.2f;
        
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        Vector2 pos = WorldUtils.WorldPosToHex(cube_pivot.transform.position.x,cube_pivot.transform.position.z);
        pos = WorldUtils.GetAdjascentHexFromFace(pos,HexFace.LowerR);
        cube_pivot_display.transform.position = new Vector3(pos.x,world.GetHeightAtPoint(pos.x,pos.y),pos.y);
    }

    /// <summary>
    /// Generate a mesh for a chunk with given position. Non-Obsolete. No known problems.
    /// </summary>
    /// <param name="cx">ChunkPos X</param>
    /// <param name="cz">ChunkPos Z</param>
    public void CreateChunkMesh(int cx,int cz)
    {
        //Get units for generation
        int x_size = WorldUtils.CHUNK_SIZE_X * 2;
        int z_size = WorldUtils.CHUNK_SIZE_Z / 2;

        GameObject empty = new GameObject();
        empty.transform.position = new Vector3(cx * WorldUtils.HEX_SIZE_X * x_size,0,cz * WorldUtils.HEX_SIZE_Z * z_size * 2);
        MeshFilter filter = empty.AddComponent<MeshFilter>();
        empty.AddComponent<MeshRenderer>().material = defaultMaterial;

        GameObject empty_water = new GameObject();
        empty_water.transform.parent = empty.transform;
        empty_water.transform.localPosition = Vector3.zero;
        MeshFilter water_filter = empty_water.AddComponent<MeshFilter>();
        empty_water.AddComponent<MeshRenderer>().material = defaultMaterial;

        //Terrain mesh builder (Solid blocks)
        ProjetoAbelhas.WorldMeshBuilder terrain_builder = new ProjetoAbelhas.WorldMeshBuilder(256000,100,100);
        
        //For now make this way, and after try to join adjascent waters to create one mesh per lake
        ProjetoAbelhas.WorldMeshBuilder water_builder = new ProjetoAbelhas.WorldMeshBuilder(8192,100,100);

        for(int bx = 0; bx < x_size; bx++)
        {
            for(int bz = 0; bz < z_size; bz ++)
            {
                #region Current Hex

                float x = cx * WorldUtils.HEX_SIZE_X * x_size + (bx * WorldUtils.HEX_SIZE_X);
                float z = cz * WorldUtils.HEX_SIZE_Z * z_size * 2 + (WorldUtils.HEX_SIZE_Z * ((bz) * 2 + bx%2));

                float ch = world.GetTerrainHeightAtPoint(x,z);
            
                #endregion
               
                //Check if ground is bellow water level
                if(ch < world.waterLevel)
                {
                    int distance = world.GetFluidPointDistanceFromGround(x,z,world.waterLevel,4);
      
                    water_builder.AddFluidHexagon((bx * WorldUtils.HEX_SIZE_X),world.waterLevel,(WorldUtils.HEX_SIZE_Z * ((bz) * 2 + bx%2)),
                        WorldUtils.WATER_COLORS[distance - 1]);
                }
                else
                {
                     #region Check Neighbour Hex

                    //Used to retrieve adjascent heights 
                    float[] h = {
                        ch - world.GetTerrainHeightAtPoint(x + math.sin(math.PI/180 * 30) * WorldUtils.HEX_DISTANCE,z + math.cos(math.PI/180 * 30) * WorldUtils.HEX_DISTANCE),
                        ch - world.GetTerrainHeightAtPoint(x + math.sin(math.PI/180 * 90) * WorldUtils.HEX_DISTANCE,z + math.cos(math.PI/180 * 90) * WorldUtils.HEX_DISTANCE),
                        ch - world.GetTerrainHeightAtPoint(x + math.sin(math.PI/180 * 150) * WorldUtils.HEX_DISTANCE,z + math.cos(math.PI/180 * 150) * WorldUtils.HEX_DISTANCE),
                        ch - world.GetTerrainHeightAtPoint(x + math.sin(math.PI/180 * 210) * WorldUtils.HEX_DISTANCE,z + math.cos(math.PI/180 * 210) * WorldUtils.HEX_DISTANCE),
                        ch - world.GetTerrainHeightAtPoint(x + math.sin(math.PI/180 * 270) * WorldUtils.HEX_DISTANCE,z + math.cos(math.PI/180 * 270) * WorldUtils.HEX_DISTANCE),
                        ch - world.GetTerrainHeightAtPoint(x + math.sin(math.PI/180 * 330) * WorldUtils.HEX_DISTANCE,z + math.cos(math.PI/180 * 330) * WorldUtils.HEX_DISTANCE)
                    };
                    terrain_builder.AddStackedHexagon((bx * WorldUtils.HEX_SIZE_X),0,(WorldUtils.HEX_SIZE_Z * ((bz) * 2 + bx%2)),ch,h,
                        Color.Lerp(new Color(0,0.1f,0),Color.green,(ch-5)/8f),new Color(0.75f,0.4f,0f));
                    #endregion 
                }              
            }
        }

        filter.mesh = terrain_builder.Create();
        water_filter.mesh = water_builder.Create();

        empty.AddComponent<MeshCollider>().sharedMesh = filter.mesh;
        empty_water.AddComponent<MeshCollider>().sharedMesh = water_filter.mesh;

        terrain_builder.Clear();
        water_builder.Clear();
    }

    #endregion
}

