using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEngine.Voxels
{
    public class Cube
    {
        public static float[] vertices = 
        {
            // Front face (+Z)
            -0.5f, -0.5f,  0.5f,   0f, 0f, 1f,   0f, 0f,  // bottom-left
            0.5f, -0.5f,  0.5f,   0f, 0f, 1f,   1f, 0f,  // bottom-right
            0.5f,  0.5f,  0.5f,   0f, 0f, 1f,   1f, 1f,  // top-right
            -0.5f,  0.5f,  0.5f,   0f, 0f, 1f,   0f, 1f,  // top-left

            // Back face (-Z)
            0.5f, -0.5f, -0.5f,   0f, 0f, -1f,   0f, 0f,  // bottom-left
            -0.5f, -0.5f, -0.5f,   0f, 0f, -1f,   1f, 0f,  // bottom-right
            -0.5f,  0.5f, -0.5f,   0f, 0f, -1f,   1f, 1f,  // top-right
            0.5f,  0.5f, -0.5f,   0f, 0f, -1f,   0f, 1f,  // top-left

            // Left face (-X)
            -0.5f, -0.5f, -0.5f,  -1f, 0f, 0f,   0f, 0f,  // bottom-left
            -0.5f, -0.5f,  0.5f,  -1f, 0f, 0f,   1f, 0f,  // bottom-right
            -0.5f,  0.5f,  0.5f,  -1f, 0f, 0f,   1f, 1f,  // top-right
            -0.5f,  0.5f, -0.5f,  -1f, 0f, 0f,   0f, 1f,  // top-left

            // Right face (+X)
            0.5f, -0.5f,  0.5f,   1f, 0f, 0f,   0f, 0f,  // bottom-left
            0.5f, -0.5f, -0.5f,   1f, 0f, 0f,   1f, 0f,  // bottom-right
            0.5f,  0.5f, -0.5f,   1f, 0f, 0f,   1f, 1f,  // top-right
            0.5f,  0.5f,  0.5f,   1f, 0f, 0f,   0f, 1f,  // top-left

            // Top face (+Y)
            -0.5f,  0.5f,  0.5f,   0f, 1f, 0f,   0f, 0f,  // bottom-left
            0.5f,  0.5f,  0.5f,   0f, 1f, 0f,   1f, 0f,  // bottom-right
            0.5f,  0.5f, -0.5f,   0f, 1f, 0f,   1f, 1f,  // top-right
            -0.5f,  0.5f, -0.5f,   0f, 1f, 0f,   0f, 1f,  // top-left

            // Bottom face (-Y)
            -0.5f, -0.5f, -0.5f,   0f, -1f, 0f,   0f, 0f,  // bottom-left
            0.5f, -0.5f, -0.5f,   0f, -1f, 0f,   1f, 0f,  // bottom-right
            0.5f, -0.5f,  0.5f,   0f, -1f, 0f,   1f, 1f,  // top-right
            -0.5f, -0.5f,  0.5f,   0f, -1f, 0f,   0f, 1f   // top-left
        };
        public static int[] indices = 
        {
            // Front face
            0, 1, 2,
            2, 3, 0,

            // Back face
            4, 5, 6,
            6, 7, 4,

            // Left face
            8, 9,10,
            10,11, 8,

            // Right face
            12,13,14,
            14,15,12,

            // Top face
            16,17,18,
            18,19,16,

            // Bottom face
            20,21,22,
            22,23,20
        };
    }
}
