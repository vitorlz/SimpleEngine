using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace SimpleEngine.Types
{
    public struct Transform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}
