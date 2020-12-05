using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace ProjetoAbelhas
{
    namespace WorldData
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
            LowerL = 5
        }

        /// <summary>
        /// Used to help manipulation of HexFaces. Non-Obsolete. No known problems.
        /// </summary>
        public static class HexFaceMethods
        {
            public static HexFace Inverse(this HexFace cur)
            {
                if(cur == HexFace.LowerL)
                    return HexFace.UpperR;
                else if(cur == HexFace.LowerR)
                    return HexFace.UpperL;
                if(cur == HexFace.UpperL)
                    return HexFace.LowerR;
                else if(cur == HexFace.UpperR)
                    return HexFace.LowerL;
                else if(cur == HexFace.UpperM)
                    return HexFace.LowerM;
                else
                    return HexFace.UpperM;
            }
        }


        /// <summary>
        /// Used as util for conversions and to store constants. Non-Obsolete. No known problems.
        /// </summary>
        public class WorldUtils
        {
            #region Field Declarations
            public static readonly int CHUNK_SIZE_X = 8;
            public static readonly int CHUNK_SIZE_Z = 8;

            public static readonly float HEX_SIZE_X = 0.866025f; //Holds half of x offset
            public static readonly float HEX_SIZE_Z = 1.5f; //Holds z offset
            public static readonly float HEX_DISTANCE = 0.866025f * 2; //Distance between two hexagons

            public static readonly Color[] WATER_COLORS = {
                new Color(0.5f,0.5f,1f),
                new Color(0.25f,0.25f,0.9f),
                new Color(0f,0f,0.75f),
                new Color(0f,0f,0.5f),
            };

            #endregion

            /// <summary>
            /// Converts a grid position to hex position. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="pos">Tile Pos</param>
            /// <returns></returns>
            public static Vector2 TilePosToHex(TilePos pos)
            {
                return new Vector2(pos.z%2 == 0 
                ? (pos.x * HEX_SIZE_X * 2) : (pos.x * HEX_SIZE_X * 2 + HEX_SIZE_X),pos.z * HEX_SIZE_Z);
            }

            /// <summary>
            /// Converts a hex position to grid position (Assumes that position is in center of hexagon). Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x">Hex X</param>
            /// <param name="z">Hex Z</param>
            /// <returns></returns>
            public static TilePos HexPosToGrid(float x,float z)
            {  
                int gz = (int) ((z + 0.5f) / HEX_SIZE_Z);
                int gx = ((int) ((x + (gz%2 == 0 ? HEX_SIZE_X : 0)) / HEX_SIZE_X))/2;

                return new TilePos(gx,gz);
                
            }

            /// <summary>
            /// Get hexagon for a world point (Don't assumes that position is in center of hexagon). Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x">Pos X</param>
            /// <param name="z">Pos Z</param>
            /// <returns></returns>
            public static Vector2 WorldPosToHex(float x,float z)
            {
                TilePos pos = HexPosToGrid(x,z);
                Vector2 point = new Vector2(x,z);
                Vector2 current_point = TilePosToHex(pos);

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

            /// <summary>
            /// Get adjascent tile at a hexagon face. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="pos">Tile Pos</param>
            /// <param name="face"></param>
            /// <returns></returns>
            public static TilePos GetAdjascentTileFromFace(TilePos pos,HexFace face)
            {
                if(face == HexFace.UpperM) 
                    return new TilePos(pos[0] + 1,pos[1]);
                else if(face == HexFace.LowerM) 
                    return new TilePos(pos[0] - 1,pos[1]);
                else if(pos[1]%2 == 0)
                {
                    if(face == HexFace.LowerL)
                        return new TilePos(pos[0] - 1,pos[1] + 1); 
                    else if(face == HexFace.LowerR)
                        return new TilePos(pos[0] - 1,pos[1] - 1); 
                    else if(face == HexFace.UpperL)
                        return new TilePos(pos[0],pos[1] + 1); 
                    else if(face == HexFace.UpperR)
                        return new TilePos(pos[0],pos[1] - 1); 
                }
                else
                {
                    if(face == HexFace.LowerL)
                        return new TilePos(pos[0],pos[1] + 1); 
                    else if(face == HexFace.LowerR)
                        return new TilePos(pos[0],pos[1] - 1); 
                    else if(face == HexFace.UpperL)
                        return new TilePos(pos[0] + 1,pos[1] + 1); 
                    else if(face == HexFace.UpperR)
                        return new TilePos(pos[0] + 1,pos[1] - 1); 
                }

                return null;
            }
            
            /// <summary>
            /// Check if a tile is accessible by another tile (Not assuste that tiles are neighbours). Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="tile">Tile to Acess</param>
            /// <param name="other">Source Tile</param>
            /// <returns></returns>
            public static bool TileIsAcessibleFrom(TilePos tile,TilePos other)
            {
                return Vector2.Distance(TilePosToHex(tile),TilePosToHex(other)) <= HEX_DISTANCE + 0.02f;
            }
            
        }

        /// <summary>
        /// Holds world info. Non-Obsolete. No known problems.
        /// </summary>
        public class World
        {
            public readonly int chunksX = 16;
            public readonly int chunksZ = 16;
            public float waterLevel = 6;

            //public ChunkData[,] chunks;
            public byte[,] block_data; //Temporary block storage

            /// <summary>
            /// Main world constructor
            /// </summary>
            public World()
            {
                //ChunkData[,] chunks = new ChunkData[chunksX,chunksZ];
                block_data = new byte[GetWidth(),GetHeight()];
            }

            /// <summary>
            /// Get width of world in tiles. Non-Obsolete. No known problems.
            /// </summary>
            /// <returns></returns>
            public int GetWidth()
            {
                return chunksX * WorldUtils.CHUNK_SIZE_X;
            }
            
            /// <summary>
            /// Get height of world in tiles. Non-Obsolete. No known problems.
            /// </summary>
            /// <returns></returns>
            public int GetHeight()
            {
                return chunksZ * WorldUtils.CHUNK_SIZE_Z;
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
            public int GetFluidPointDistanceFromGround(float x,float z,float fluid_height,int max_distance)
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

            /// <summary>
            /// Check if tile is safe for walk. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public bool CanWalkAboveTile(TilePos tile,HexFace from_face,bool can_swim)
            {
               
                if(block_data[tile[0],tile[1]] == 1)
                    return false;

                if(can_swim)
                    return true;

                Vector2 pos = WorldUtils.TilePosToHex(tile);
                return GetTerrainHeightAtPoint(pos.x,pos.y) < waterLevel ? false : true;
            }
        }

        /// <summary>
        /// Represents a tile position in world. Non-Obsolete. No known problems.
        /// </summary>
        public class TilePos
        {
            #region Field Declarations
            public int x;
            public int z;
            #endregion
            
            /// <summary>
            /// Index acess for position. Non-Obsolete. No known problems.
            /// </summary>
            /// <value></value>
            public int this[int key]
            {
                get => key == 0 ? x : z;
                set { 
                    if(key == 0)
                        x = value;
                    else
                        z = value;
                }  
            }

            /// <summary>
            /// Default constructor. Non-Obsolete. No known problems.
            /// </summary>
            public TilePos()
            {
                x = 0;
                z = 0;
            }

            /// <summary>
            /// Default constructor give coordinates.  Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x">Tile X</param>
            /// <param name="z">Tile Z</param>
            public TilePos(int x, int z)
            {
                this.x = x;
                this.z = z;
            }
            
            /// <summary>
            /// Default constructor cloning other TilePos. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="pos">Tile Pos</param>
            public TilePos(TilePos pos)
            {
                x = pos.x;
                z = pos.z;
            }

            /// <summary>
            /// Used in key comparisions. Non-Obsolete. No known problems.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return x ^ z;
            }
            
            /// <summary>
            /// Check if pos is equals to a object. Non-Obsolete. No known problems. (Assumes that obj type is TilePos)
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(System.Object obj)
            {
                TilePos p = (TilePos)obj;

                if (ReferenceEquals(null, p))
                {
                    return false;
                }

                return (x == p.x) && (z == p.z);
            }

            /// <summary>
            /// Check if pos is equals to another pos. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public bool Equals(TilePos p)
            {
                if (ReferenceEquals(null, p))
                {
                    return false;
                }

                return (x == p.x) && (z == p.z);
            }

            /// <summary>
            /// Check if positions are equals using operator ==. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static bool operator ==(TilePos a, TilePos b)
            {
                if (System.Object.ReferenceEquals(a, b))
                {
                    return true;
                }
                /*
                if (ReferenceEquals(null, a))
                {
                    return false;
                }
                if (ReferenceEquals(null, b))
                {
                    return false;
                }
                */

                return a.x == b.x && a.z == b.z;
            }

            /// <summary>
            /// Check if positions are not equal using operator !=. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static bool operator !=(TilePos a, TilePos b)
            {
                return !(a == b);
            }

            /// <summary>
            /// Set values of pos. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="x">Tile X</param>
            /// <param name="z">Tile Z</param>
            /// <returns></returns>
            public TilePos Set(int x, int z)
            {
                this.x = x;
                this.z = z;
                return this;
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