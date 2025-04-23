using System.Collections.Generic;
using UnityEngine;

namespace VNWGame
{
    public class MeshCreater
    {
        /// <summary>
        /// sequence为true为顺时针,false为逆时针, center为false网格00在左下角
        /// </summary>
        /// <param name="sequence"></param>
        public static Mesh GeneratorQuadMesh(bool sequence = true, bool center = false, float scale = 1)
        {
            Mesh tempMesh = new Mesh();
            Vector3[] vertices = new Vector3[4];
            Vector2[] uv = new Vector2[4];
            int[] indices;

            float offset = center ? 0.5f : 0;
            vertices[0] = new Vector3(0 - offset, 0 - offset, 0) * scale;
            vertices[1] = new Vector3(1 - offset, 0 - offset, 0) * scale;
            vertices[2] = new Vector3(0 - offset, 1 - offset, 0) * scale;
            vertices[3] = new Vector3(1 - offset, 1 - offset, 0) * scale;

            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(0, 1);
            uv[3] = new Vector2(1, 1);

            if (sequence)
            {
                indices = new int[6]{ 0, 3, 1, 0, 2, 3 };
            }
            else
            {
                indices = new int[6]{ 0, 3, 2, 0, 1, 3 };
            }

            tempMesh.vertices = vertices;
            tempMesh.triangles = indices;
            tempMesh.uv = uv;

            return tempMesh;
        }

        public static List<Vector3> ChangeMeshFaceCamera(Mesh mesh)
        {
            Vector3 normal = Camera.main.transform.forward;
            Vector3 right = Vector3.Normalize(Vector3.Cross(normal, Vector3.up));
            Vector3 up = Vector3.Normalize(Vector3.Cross(right, normal));
            List<Vector3> vertices = new List<Vector3>();
            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                vertices.Add(right * mesh.vertices[i].x + up * mesh.vertices[i].y + normal * mesh.vertices[i].z);
            }
            return vertices;
        }
    }
}