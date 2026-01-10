using UnityEngine;

namespace Map
{
    public static class MeshGenerator
    {
        public static MeshData GenerateTerrainMesh(float[,] heightmap,float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
        {
            AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
            int width = heightmap.GetLength(0);
            int height = heightmap.GetLength(1);
            
            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;
            
            int simplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
            int verticesPerLine = (width - 1) / simplificationIncrement + 1;
            
            MeshData meshData = new MeshData(width, height);
            int vertexIndex = 0;

            for (int y = 0; y < height; y+= simplificationIncrement)
            {
                for (int x = 0; x < width; x+= simplificationIncrement)
                {
                    lock (heightCurve)
                    {
                       meshData.vertices[vertexIndex] = new Vector3(topLeftX+x, heightCurve.Evaluate(heightmap[x, y])* heightMultiplier, topLeftZ - y);
                    }
                    meshData.uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height);
                    if (x < width - 1 && y < height - 1)
                    {
                        meshData.AddTriangle(vertexIndex , vertexIndex +verticesPerLine+ 1, vertexIndex + verticesPerLine);
                        meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex , vertexIndex + 1);
                    }
                    vertexIndex++;
                }
            }

            return meshData;

        }
    }

    public class MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        int triangleIndex;
        public Vector2[] uvs;
        public MeshData(int meshWidth, int meshHeight)
        {
            vertices = new Vector3[meshWidth * meshHeight];
            uvs = new Vector2[meshWidth * meshHeight];
            triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        }

        public void AddTriangle(int a, int b, int c)
        {
            triangles[triangleIndex ]= b;
            triangles[triangleIndex+1] = c;
            triangles[triangleIndex+2] = a;
            triangleIndex += 3;
        }
        
        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
