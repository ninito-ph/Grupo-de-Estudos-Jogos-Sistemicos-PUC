using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace ProjetoAbelhas
{
    namespace WorldGeneration
    {   
        /// <summary>
        /// Used to navigate between hexagons by faces. Non-Obsolete. No known problems.
        /// </summary>
        public enum HexFace
        {
            UpperL = 0,
            UpperM = 1, //X+
            UpperR = 2,
            LowerR = 3,
            LowerM = 4, //X-
            LowerL = 5,
        }

        /// <summary>
        /// Used as util for convertions and to store constants. Non-Obsolete. No known problems.
        /// </summary>
        public class WorldUtils
        {
            public static readonly int CHUNK_SIZE_X = 16;
            public static readonly int CHUNK_SIZE_Z = 4;

            public static readonly float HEX_SIZE_X = 0.866025f; //Holds half of x offset
            public static readonly float HEX_SIZE_Z = 1.5f; //Holds z offset
            public static readonly float HEX_DISTANCE = 0.866025f * 2; //Distance between two hexagons

            public static readonly Color[] WATER_COLORS = {
                new Color(0.5f,0.5f,1f),
                new Color(0.25f,0.25f,0.9f),
                new Color(0f,0f,0.75f),
                new Color(0f,0f,0.5f),
            };

            /// <summary>
            /// Converts a grid position to hex position. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x">Grid X</param>
            /// <param name="z">Grid Z</param>
            /// <returns></returns>
            public static Vector2 GridPosToHex(int x,int z)
            {
                return new Vector2(z%2 == 0 
                ? (x * HEX_SIZE_X * 2) : (x * HEX_SIZE_X * 2 + HEX_SIZE_X),z * HEX_SIZE_Z);
            }

            /// <summary>
            /// Converts a hex position to grid position (Assumes that position is in center of hexagon). Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x">Hex X</param>
            /// <param name="z">Hex Z</param>
            /// <returns></returns>
            public static int[] HexPosToGrid(float x,float z)
            {  
                int gz = (int) ((z + 0.5f) / HEX_SIZE_Z);
                int gx = ((int) ((x + (gz%2 == 0 ? HEX_SIZE_X : 0)) / HEX_SIZE_X))/2;

                return new int[]{gx,gz};
                
            }

            /// <summary>
            /// Get hexagon for a world point (Don't assumes that position is in center of hexagon). Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x">Pos X</param>
            /// <param name="z">Pos Z</param>
            /// <returns></returns>
            public static Vector2 WorldPosToHex(float x,float z)
            {
                int[] pos = HexPosToGrid(x,z);
                Vector2 point = new Vector2(x,z);
                Vector2 current_point = GridPosToHex(pos[0],pos[1]);

                float max_distance = Vector2.Distance(point,current_point);
                Vector2 cur = current_point;
                //Check adjascent tiles
                for(int angle = 0; angle < 6; angle ++)
                {
                    float angle_f = 30 + angle * 60;
                    float fx = current_point.x + math.sin(math.PI/180 * angle_f) * HEX_DISTANCE;
                    float fz = current_point.y + math.cos(math.PI/180 * angle_f) * HEX_DISTANCE;

                    float ds = Vector2.Distance(point,new Vector2(fx,fz));

                    if(ds < max_distance)
                    {
                        max_distance = ds;
                        cur = new Vector2(fx,fz);
                    }
                }

                return cur;        
            }
            
            /// <summary>
            /// Get adjascent hexagon at a hexagon face. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="pos">Hex pos</param>
            /// <param name="face">Hex face</param>
            /// <returns></returns>
            public static Vector2 GetAdjascentHexFromFace(Vector2 pos,HexFace face)
            {
                float angle_f = 30 + ((int)face) * 60;
                return pos + new Vector2(math.sin(math.PI/180 * angle_f) * HEX_DISTANCE,math.cos(math.PI/180 * angle_f) * HEX_DISTANCE);
            }
        }

        /// <summary>
        /// Holds world info. Non-Obsolete. No known problems.
        /// </summary>
        public class World
        {
            public int chunksX = 16;
            public int chunksZ = 16;

            public float waterLevel = 6;

            public ChunkData[,] chunks;

            public World()
            {
                ChunkData[,] chunks = new ChunkData[chunksX,chunksZ];
            }

            /// <summary>
            /// Gets the terrain height for a certain point in world. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="z"></param>
            /// <returns></returns>
            public float GetTerrainHeightAtPoint(float x,float z)
            {
                return Mathf.PerlinNoise(x * 1/32f,z * 1/32f) * 18f;  
            }

            /// <summary>
            /// Gets height for a certain point in world, including water level. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="z"></param>
            /// <returns></returns>
            public float GetHeightAtPoint(float x,float z)
            {
                return Mathf.Max(waterLevel,GetTerrainHeightAtPoint(x,z));
            }

            /// <summary>
            /// Get closest terrain distance from a water tile. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x">Water X</param>
            /// <param name="z">Water Z</param>
            /// <param name="fluid_height">Current Fluid Height</param>
            /// <param name="max_distance">Max Search Distance</param>
            /// <returns></returns>
            public int GetFluidDistanceFromGround(float x,float z,float fluid_height,int max_distance)
            {
                for(int distance = 1; distance < max_distance; distance ++)
                {
                    for(int angle = 0; angle < 6; angle ++)
                    {
                        float angle_f = 30 + angle * 60;
                        if(GetTerrainHeightAtPoint(x + math.sin(math.PI/180 * angle_f) * WorldUtils.HEX_DISTANCE * distance,z + math.cos(math.PI/180 * angle_f) * WorldUtils.HEX_DISTANCE * distance) > fluid_height)
                        {
                            return distance;
                        }
                    }
                }

                return max_distance;
            }
        }

        /// <summary>
        /// Holds chunk tiles info. Non-Obsolete. No known problems.
        /// </summary>
        public class ChunkData
        {
            #region Field Declarations
            public byte[,] tiles; //For now 0 = no tile, 1 = tile
            #endregion


            public ChunkData()
            {
                tiles = new byte[WorldUtils.CHUNK_SIZE_X,WorldUtils.CHUNK_SIZE_Z];

                for(int x = 0; x < WorldUtils.CHUNK_SIZE_X; x++)
                    for(int z = 0; z < WorldUtils.CHUNK_SIZE_Z; z ++)
                        tiles[x,z] = 1;
            }
        }
    }
}