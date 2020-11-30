using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjetoAbelhas.WorldData;

namespace ProjetoAbelhas
{
    /// <summary>
    /// Code based on https://github.com/RonenNess/Unity-2d-pathfinding by RonenNess
    /// Added support to hexagon tiles and optmized for world acess. Non-Obsolete. No known problems.
    /// </summary>
    namespace PathFind
    {   
        /// <summary>
        /// Main pathfind class. Used to pathfind a path in hexagonal grid. Non-Obsolete. No known problems.
        /// </summary>
        public class Pathfinding
        {
            /// <summary>
            /// Find a path in world from point A to B. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="world">Current world</param>
            /// <param name="A">Start tile</param>
            /// <param name="B">Target tile</param>
            /// <returns></returns>
            public static List<TilePos> FindPath(World world,TilePos A,TilePos B)
            {
                return FindPath(new Grid(world),A,B);
            }

            /// <summary>
            /// Find a path in world from point A to B supporint coroutines. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="world">Current world</param>
            /// <param name="A">Start tile</param>
            /// <param name="B">Target tile</param>
            /// <param name="callback"></param>
            /// <returns></returns>
            public static IEnumerator FindPathAsync(World world, TilePos A, TilePos B,System.Action<List<TilePos>> callback)
            {
                float lastYield = Time.time;
                float yieldTime = 1/60f;
                
                Grid grid = new Grid(world);
                
                Node startNode = grid.GetNode(A);
                Node targetNode = grid.GetNode(B);

                List<Node> openSet = new List<Node>();
                HashSet<Node> closedSet = new HashSet<Node>();
                openSet.Add(startNode);

                while (openSet.Count > 0)
                {
                    Node currentNode = openSet[0];
                    for (int i = 1; i < openSet.Count; i++)
                    {
                        if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                        {
                            currentNode = openSet[i];
                        }
                    }

                    openSet.Remove(currentNode);
                    closedSet.Add(currentNode);

                    if (currentNode == targetNode)
                    {
                        List<Node> nodes_path = RetracePath(grid, startNode, targetNode);

                        // convert to a list of points and return
                        List<TilePos> ret = new List<TilePos>();
                        if (nodes_path != null)
                        {
                            foreach (Node node in nodes_path)
                            {
                                ret.Add(new TilePos(node.gridX, node.gridY));
                            }
                        }

               
                        callback(ret);
                        yield break;
                    }

                    foreach (Node neighbour in grid.GetNeighbours(currentNode))
                    {   
                        if (/*!neighbour.walkable || */closedSet.Contains(neighbour))
                        {
                            continue;
                        }

                        int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) * (int)(10.0f * neighbour.penalty);
                        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetNode);
                            neighbour.parent = currentNode;

                            if (!openSet.Contains(neighbour))
                                openSet.Add(neighbour);
                        }
                    }

                    if(Time.time - lastYield >= yieldTime)
                    {
                        lastYield = Time.time;
                        yield return 0;
                    }
                }

                 yield break;
            }

            /// <summary>
            /// Find a path in grid from point A to B. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="grid">Current world grid</param>
            /// <param name="A">Start tile</param>
            /// <param name="B">Target tile</param>
            /// <returns></returns>
            public static List<TilePos> FindPath(Grid grid, TilePos A, TilePos B)
            {
                //Get path
                List<Node> nodes_path = _ImpFindPath(grid, A, B);

                //Convert nodes to points
                List<TilePos> ret = new List<TilePos>();
                if (nodes_path != null)
                {
                    foreach (Node node in nodes_path)
                    {
                        ret.Add(new TilePos(node.gridX, node.gridY));
                    }
                }
                return ret;
            }

            /// <summary>
            /// Internal function to find path in a grid. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="grid">Current world grid</param>
            /// <param name="A">Start tile</param>
            /// <param name="B">Target tile</param>
            /// <returns></returns>
            private static List<Node> _ImpFindPath(Grid grid, TilePos A, TilePos B)
            {
                Node startNode = grid.GetNode(A);
                Node targetNode = grid.GetNode(B);

                List<Node> openSet = new List<Node>();
                HashSet<Node> closedSet = new HashSet<Node>();
                openSet.Add(startNode);

                while (openSet.Count > 0)
                {
                    Node currentNode = openSet[0];
                    for (int i = 1; i < openSet.Count; i++)
                    {
                        if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                        {
                            currentNode = openSet[i];
                        }
                    }

                    openSet.Remove(currentNode);
                    closedSet.Add(currentNode);

                    if (currentNode == targetNode)
                    {
                        return RetracePath(grid, startNode, targetNode);
                    }

                    foreach (Node neighbour in grid.GetNeighbours(currentNode))
                    {   
                        if (/*!neighbour.walkable || */closedSet.Contains(neighbour))
                        {
                            continue;
                        }

                        int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) * (int)(10.0f * neighbour.penalty);
                        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetNode);
                            neighbour.parent = currentNode;

                            if (!openSet.Contains(neighbour))
                                openSet.Add(neighbour);
                        }
                    }
                }

                return null;
            }

            /// <summary>
            /// Retrace ending path to check. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="grid">Current grid</param>
            /// <param name="A">Start node</param>
            /// <param name="B">Target node</param>
            /// <returns></returns>
            private static List<Node> RetracePath(Grid grid, Node A, Node B)
            {
                List<Node> path = new List<Node>();
                Node currentNode = B;

                while (currentNode != A)
                {
                    path.Add(currentNode);
                    currentNode = currentNode.parent;
                }
                path.Reverse();
                return path;
            }

            /// <summary>
            /// Get distance between two nodes. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="nodeA">First node</param>
            /// <param name="nodeB">Seconde node</param>
            /// <returns></returns>
            private static int GetDistance(Node nodeA, Node nodeB)
            {
                int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
                int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

                if (dstX > dstY)
                    return 14 * dstY + 10 * (dstX - dstY);
                return 14 * dstX + 10 * (dstY - dstX);
            }
        }

        /// <summary>
        /// Current path node. Non-Obsolete. No known problems.
        /// </summary>
        public class Node
        {
            //Node data
            public int gridX;
            public int gridY;
            public float penalty;

            //Calculate while tracing path. Non-Obsolete. No known problems.
            public int gCost;
            public int hCost;
            public Node parent;

           /// <summary>
           /// Create node with penalty and positions. Non-Obsolete. No known problems.
           /// </summary>
           /// <param name="price">Price</param>
           /// <param name="x">Pos X</param>
           /// <param name="y">Pos Y</param>
            public Node(float price, int x, int y)
            {
                penalty = price;
                gridX = x;
                gridY = y;
            }

            /// <summary>
            /// Get final cost of a tile. Non-Obsolete. No known problems.
            /// </summary>
            /// <value></value>
            public int fCost
            {
                get
                {
                    return gCost + hCost;
                }
            }
        }
        
        /// <summary>
        /// Grid of nodes used to trace a path. Non-Obsolete. No known problems.
        /// </summary>
        public class Grid
        {
            private Node[,] nodes;
            int gridSizeX, gridSizeY;
            World world;
            
            /// <summary>
            /// Creates a grid from a world. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="world"></param>
            public Grid(World world)
            {
                this.world = world;
                gridSizeX = world.GetWidth();
                gridSizeY = world.GetHeight();
                nodes = new Node[world.GetWidth(), world.GetHeight()];
            }

            /// <summary>
            /// Get a node at tile position. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="pos">Current Tile</param>
            /// <returns></returns>
            public Node GetNode(TilePos pos)
            {
                if(nodes[pos.x,pos.z] != null)
                    return nodes[pos.x,pos.z];

                return nodes[pos.x,pos.z] = new Node(1.0f,pos.x,pos.z);
            }

            /// <summary>
            /// Get accessible neighbours hexagons/node from a node. Non-Obsolete. No known problems.
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            public List<Node> GetNeighbours(Node node)
            {
                List<Node> neighbours = new List<Node>();

                for(int i = 0; i < 6; i ++)
                {
                    TilePos ps = WorldUtils.GetAdjascentTileFromFace(new TilePos(node.gridX,node.gridY),(HexFace)i);
                    if(ps[0] >= 0 && ps[0] < gridSizeX && ps[1] >= 0 && ps[1] < gridSizeY)
                        if(world.CanWalkAboveTile(new TilePos(ps[0],ps[1]),HexFaceMethods.Inverse((HexFace)i),false))
                            neighbours.Add(GetNode(ps));
                }

                return neighbours;
            }
        }
    }

}
