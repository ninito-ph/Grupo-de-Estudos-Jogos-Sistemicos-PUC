using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjetoAbelhas.WorldData;
using ProjetoAbelhas.PathFind;

/// <summary>
/// Testing path tracing in hexagonal grid. Non-Obsolete. No known problems.
/// </summary>
public class NavigationTest : MonoBehaviour
{
    public static NavigationTest CURRENT_TEST;

    void Start()
    {
        CURRENT_TEST = this; 
        path_filter = path_object.GetComponent<MeshFilter>();
    }

    public GameObject path_object;
    MeshFilter path_filter;

    List<Vector3> path = new List<Vector3>();
    float moveSpeed = 8;

    void Update()
    {
        if(path.Count > 0)
        {
            //Go to next
            transform.position = Vector3.MoveTowards(transform.position, path[0], moveSpeed * Time.deltaTime);
            if(Vector3.Distance(transform.position,path[0]) < 0.05f)
            {
                path.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Walk to hexagon
    /// </summary>
    /// <param name="hex">Target hexagon</param>
    public void CalculatePathTo(Vector2 hex)
    {
        //Get current hex
        Vector2 current = WorldUtils.WorldPosToHex(gameObject.transform.position.x,gameObject.transform.position.z);
        
        TilePos start = WorldUtils.HexPosToGrid(current.x,current.y);
        //TilePos cur = WorldUtils.HexPosToGrid(current.x,current.y);
        TilePos end = WorldUtils.HexPosToGrid(hex.x,hex.y);

        float startTime = Time.time;
        //Get path 
        StartCoroutine(Pathfinding.FindPathAsync(TestWorldGenerator.world,start,end,delegate(List<TilePos> tiles){

            float delta = Time.time - startTime;
            Debug.Log("Found path: " + tiles.Count + " in " + delta + " second(s)!");

            path.Clear();
            path.Add(transform.position);

            foreach(TilePos pos in tiles)
            {
                path.Add(PosToWorld(WorldUtils.TilePosToHex(pos)));
            }

            ProjetoAbelhas.WorldMeshBuilder builder = new ProjetoAbelhas.WorldMeshBuilder(8192,999,999);

            for(int i = 0; i < path.Count; i ++)
            {
                //Debug.DrawLine(path[i],path[i + 1], Color.black,((path.Count - 1) * WorldUtils.HEX_DISTANCE)/moveSpeed,false); 
                Vector3 pos = path[i];
                builder.AddFluidHexagon(pos.x,TestWorldGenerator.world.GetHeightAtPoint(pos.x,pos.z) + 0.05f,pos.z,Color.white);
            }

            path_filter.mesh = builder.Create();

        }));


        /*
        //Walk to position when there is no obstacles
        while(end[0] != cur[0] || end[1] != cur[1])
        {
            HexFace face = HexFace.LowerL;

            if(end[1] == cur[1])
            {
                if(end[0] > cur[0])
                    face = HexFace.UpperM;
                else
                    face = HexFace.LowerM;
            }
            else if(end[0] == cur[0])
            {   
                if(end[1] > cur[1])
                    face = (cur[1]%2 == 0) ? HexFace.UpperL : HexFace.LowerL;
                else
                    face = (cur[1]%2 == 0) ? HexFace.UpperR : HexFace.LowerR;
            }
            else
            {
                if(end[0] > cur[0])
                    face = (end[1] > cur[1]) ? HexFace.UpperL : HexFace.UpperR;
                else
                    face = (end[1] > cur[1]) ? HexFace.LowerL : HexFace.LowerR;
            }

            
            Vector2 vt = WorldUtils.GetAdjascentHexFromFace(current,face);
            TilePos ps = WorldUtils.HexPosToGrid(vt.x,vt.y);
            

            if(TestWorldGenerator.world.CanWalkAboveTile(new TilePos(ps[0],ps[1]),HexFaceMethods.Inverse(face),false))
            {
                 current = vt;
                cur = ps;
                path.Add(PosToWorld(vt));
            }
            else
            { 
                path.Clear();
                
                //Source and target points
                TilePos from = new TilePos(start[0], start[1]);
                TilePos to = new TilePos(end[0], end[1]);

                //Get path using A*
                foreach(TilePos pos in Pathfinding.FindPath(TestWorldGenerator.world, from, to))
                {
                    path.Add(PosToWorld(WorldUtils.TilePosToHex(pos)));
                }

                break;
            }
        }   
        
        /*
        ProjetoAbelhas.WorldMeshBuilder builder = new ProjetoAbelhas.WorldMeshBuilder(8192,999,999);

        for(int i = 0; i < path.Count; i ++)
        {
            //Debug.DrawLine(path[i],path[i + 1], Color.black,((path.Count - 1) * WorldUtils.HEX_DISTANCE)/moveSpeed,false); 
            Vector3 pos = path[i] - new Vector3(0,0.495f,0);
            builder.AddFluidHexagon(pos.x,pos.y,pos.z,Color.white);
        }

        path_filter.mesh = builder.Create();
        */


    }
    
    /*
                //Remove default path
                path.Clear();

                int tries = 0;
                //Try to pathfind to dodge from obstacle
                List<PathNode> current_path = new List<PathNode>();

                int map_size_x = 15000;
                List<int> already_tested = new List<int>();
                
                current_path.Add(new PathNode(start,end));
                already_tested.Add(-1);
                
                while(current_path[current_path.Count - 1].pos[0] != end[0] || current_path[current_path.Count -1].pos[1] != end[1])
                {
                    int i = current_path.Count - 1;

                    PathNode p = current_path[i];

                    if(p.tryN == 6)
                    {
                        current_path.RemoveAt(i);
                        already_tested.RemoveAt(i);

                        if(current_path.Count == 0)
                        {
                            Debug.Log("Path not found!");
                            break;
                        }

                        continue;
                    }
                    
                    int[] pn = WorldUtils.GetAdjascentTileFromFace(p.pos,p.best[p.tryN]);
                    
                    if(!already_tested.Contains(pn[1] * map_size_x + pn[0]) && TestWorldGenerator.world.CanWalkAboveTile(pn[0],pn[1],HexFaceMethods.Inverse(p.best[p.tryN]),false))
                    {
                        //Cut repeating moves
                        bool cut = false;
                        for(int j = 0; j < current_path.Count; j ++)
                        {
                            if(current_path[j].pos[0] == pn[0] && current_path[j].pos[1] == pn[1])
                            {
                                //Debug.Log("Cut out!");
                                cut = true;
                                current_path.RemoveRange(j,current_path.Count - j);
                                already_tested.RemoveRange(j,current_path.Count - j);
                                break;
                            }
                            else if(WorldUtils.TileIsAcessibleFrom(current_path[j].pos,pn))
                            {
                                //Debug.Log("Cut out by side!");
                                cut = true;
                                current_path.RemoveRange(j + 1,current_path.Count - (j + 1));
                                already_tested.RemoveRange(j + 1,current_path.Count - (j + 1));
                                break;
                            }
                        }

                        //Cut sided moves
                        current_path.Add(new PathNode(pn,end));
                        already_tested.Add(pn[1] * map_size_x + pn[0]);
                        
                        p.tryN ++;  
                    }
                    else
                        p.tryN ++; //Rotate to next side  

                    tries ++;
                    if(tries >= 50000)
                    {
                        Debug.Log("Path not found: Too many tries");
                        break;
                    }
                   
                }

                Debug.Log("Path found: " + current_path.Count);
                foreach(PathNode nd in current_path)
                {
                    path.Add(PosToWorld(WorldUtils.GridPosToHex(nd.pos[0],nd.pos[1])));
                }
                */

    /*
    public class PathNode
    {
        public HexFace[] best;
        public int[] pos;
        public int tryN = 0;

        public PathNode(int[] cur,int[] end)
        {
            this.pos = cur;

            this.best = NavigationTest.GetBestTilesFor(cur,end);
        }
    }

    public static HexFace[] GetBestTilesFor(int[] cur,int[] end)
    {
        HexFace face = HexFace.LowerL;

        if(end[1] == cur[1])
        {
            if(end[0] > cur[0])
                face = HexFace.UpperM;
            else
                face = HexFace.LowerM;
        }
        else if(end[0] == cur[0])
        {   
            if(end[1] > cur[1])
                face = (cur[1]%2 == 0) ? HexFace.UpperL : HexFace.LowerL;
            else
                face = (cur[1]%2 == 0) ? HexFace.UpperR : HexFace.LowerR;
        }
        else
        {
            if(end[0] > cur[0])
                face = (end[1] > cur[1]) ? HexFace.UpperL : HexFace.UpperR;
            else
                face = (end[1] > cur[1]) ? HexFace.LowerL : HexFace.LowerR;
        }

        if(face == HexFace.UpperM)
            return new HexFace[]{
                HexFace.UpperM,
                HexFace.UpperL,
                HexFace.UpperR,
                HexFace.LowerL,
                HexFace.LowerR,
                HexFace.LowerM,
            };
        if(face == HexFace.LowerM)
            return new HexFace[]{
                HexFace.LowerM,
                HexFace.LowerL,
                HexFace.LowerR,
                HexFace.UpperL,
                HexFace.UpperR,
                HexFace.UpperM,
            };
        if(face == HexFace.UpperL)
            return new HexFace[]{
                HexFace.UpperL,
                HexFace.UpperM,
                HexFace.LowerL,
                HexFace.UpperR,
                HexFace.LowerM,
                HexFace.LowerR,
            };
        if(face == HexFace.UpperR)
            return new HexFace[]{
                HexFace.UpperR,
                HexFace.UpperM,
                HexFace.LowerR,
                HexFace.UpperL,
                HexFace.LowerM,
                HexFace.LowerL,
            };
        if(face == HexFace.LowerL)
            return new HexFace[]{
                HexFace.LowerL,
                HexFace.LowerM,
                HexFace.UpperL,
                HexFace.LowerR,
                HexFace.UpperM,
                HexFace.UpperR,
            };
        
        return new HexFace[]{
            HexFace.LowerR,
            HexFace.LowerM,
            HexFace.UpperR,
            HexFace.LowerL,
            HexFace.UpperM,
            HexFace.UpperL,
        };
    }
    */

    /// <summary>
    /// Convert a 2D position to 3D using world height
    /// </summary>
    /// <param name="pos">World 2D position</param>
    /// <returns></returns>
    public Vector3 PosToWorld(Vector2 pos)
    {
        return new Vector3(pos.x,TestWorldGenerator.world.GetHeightAtPoint(pos.x,pos.y) + 0.5f,pos.y);
    }
}
