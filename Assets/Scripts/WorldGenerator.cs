using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ProjetoAbelhas
{
    /// <summary>
    /// Used as helper to create meshs for chunk renderer. Non-obsolete. No known problems.
    /// </summary>
    public class WorldMeshBuilder
    {
        #region Field Declarations
        private Vector3[] vertices; 
        private Color[] colors;
        private int[] triangles;

        private int vertices_count;
        private int triangles_count;

        private float bound_x,bound_z; //Limit map size
        #endregion
        

        /// <summary>
        /// Default standard-size constructor. Non-obsolete. No known problems.
        /// </summary>
        public WorldMeshBuilder(float bound_x,float bound_z)
        {
            this.bound_x = bound_x;
            this.bound_z = bound_z;
          
            Clear();
        }

        /// <summary>
        /// Default constructor with custom size support. Non-obsolete. No known problems.
        /// </summary>
        /// <param name="size"></param>
        public WorldMeshBuilder(int size,float bound_x,float bound_z)
        {
            this.bound_x = bound_x;
            this.bound_z = bound_z;

            Clear(size);
        }

        /// <summary>
        /// Used to init/clear builder values with custom size. Non-obsolete. No known problems.
        /// </summary>
        public void Clear(int size)
        {
            vertices = new Vector3[size];
            colors = new Color[size];
            triangles = new int[size * 2];

            vertices_count = 0;
            triangles_count = 0;
        }

        /// <summary>
        /// Used to init/clear builder values with default size. Non-obsolete. No known problems.
        /// </summary>
        public void Clear()
        {
            Clear(4096);
        }

        /// <summary>
        /// Add hexagon with lateral faces
        /// </summary>
        /// <param name="x">Pos X</param>
        /// <param name="y">Pos Y</param>
        /// <param name="z">Pos Z</param>
        /// <param name="h">Plane Height</param>
        /// <param name="nh">Side Faces Heights</param>
        /// <param name="color">Color</param>
        public void AddStackedHexagon(float x,float y,float z,float h,float[] nh,Color color)
        {
            vertices[vertices_count] = new Vector3(x + 0.866025f,y + h,z + 0.5f);
            vertices[vertices_count + 1] = new Vector3(x + 0.866025f,y + h,z - 0.5f);
            vertices[vertices_count + 2] = new Vector3(x,y + h,z - 1);
            vertices[vertices_count + 3] = new Vector3(x - 0.866025f,y + h,z - 0.5f);
            vertices[vertices_count + 4] = new Vector3(x - 0.866025f,y + h,z + 0.5f);
            vertices[vertices_count + 5] = new Vector3(x,y + h,z + 1);

            colors[vertices_count] = color;
            colors[vertices_count + 1] = color;
            colors[vertices_count + 2] = color;
            colors[vertices_count + 3] = color;
            colors[vertices_count + 4] = color;
            colors[vertices_count + 5] = color;
            
            triangles[triangles_count] = vertices_count;
            triangles[triangles_count + 1] = vertices_count + 1;
            triangles[triangles_count + 2] = vertices_count + 2;
            triangles[triangles_count + 3] = vertices_count;
            triangles[triangles_count + 4] = vertices_count + 2;
            triangles[triangles_count + 5] = vertices_count + 3;
            triangles[triangles_count + 6] = vertices_count;
            triangles[triangles_count + 7] = vertices_count + 3;
            triangles[triangles_count + 8] = vertices_count + 5;
            triangles[triangles_count + 9] = vertices_count + 3;
            triangles[triangles_count + 10] = vertices_count + 4;
            triangles[triangles_count + 11] = vertices_count + 5;

            triangles_count += 12;
            vertices_count += 6;

            //Add lateral faces
            AddLateralFacesHex(new Vector3(x + 0.866025f,y + h,z + 0.5f),new Vector3(x + 0.866025f,y + h,z - 0.5f),nh[1],color);
            AddLateralFacesHex(new Vector3(x + 0.866025f,y + h,z - 0.5f),new Vector3(x,y + h,z - 1),nh[2],color);
            AddLateralFacesHex(new Vector3(x,y + h,z - 1),new Vector3(x - 0.866025f,y + h,z - 0.5f),nh[3],color);
            AddLateralFacesHex(new Vector3(x - 0.866025f,y + h,z - 0.5f),new Vector3(x - 0.866025f,y + h,z + 0.5f),nh[4],color);
            AddLateralFacesHex(new Vector3(x - 0.866025f,y + h,z + 0.5f),new Vector3(x,y + h,z + 1),nh[5],color);
            AddLateralFacesHex(new Vector3(x,y + h,z + 1),new Vector3(x + 0.866025f,y + h,z + 0.5f),nh[0],color);
        }

        /// <summary>
        /// Add lateral face for a hexagon
        /// </summary>
        /// <param name="a">Vertex A</param>
        /// <param name="b">Vertex B</param>
        /// <param name="h">Face Height</param>
        /// <param name="color">Face Color</param>
        private void AddLateralFacesHex(Vector3 a,Vector3 b,float h,Color color)
        {
            if(h <= 0)
                return;

            vertices[vertices_count] = a;
            vertices[vertices_count + 1] = b;

            vertices[vertices_count + 2] = a + new Vector3(0,-h,0);
            vertices[vertices_count + 3] = b + new Vector3(0,-h,0);

            triangles[triangles_count] = vertices_count + 2;
            triangles[triangles_count + 1] = vertices_count + 1;
            triangles[triangles_count + 2] = vertices_count;

            triangles[triangles_count + 3] = vertices_count + 1;
            triangles[triangles_count + 4] = vertices_count + 2;
            triangles[triangles_count + 5] = vertices_count + 3;

            colors[vertices_count] = color;
            colors[vertices_count + 1] = color;
            colors[vertices_count + 2] = color;
            colors[vertices_count + 3] = color;

            vertices_count += 4;
            triangles_count += 6;
        }

        /// <summary>
        /// Generate mesh from builder
        /// </summary>
        /// <returns></returns>
        public Mesh Create()
        {
            Mesh mh = new Mesh();

            mh.vertices = vertices.Take(vertices_count).ToArray();
            mh.triangles = triangles.Take(triangles_count).ToArray();
            mh.colors = colors.Take(vertices_count).ToArray();
            
            mh.RecalculateNormals();
            return mh;
        }
    }
}