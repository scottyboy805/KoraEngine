using Box3D;
using System.Numerics;
using System.Runtime.InteropServices;

namespace KoraGame.Physics
{
    public sealed class PhysicsMesh : GameElement
    {
        // Private
        private bool isConvex = false;
        private Vector3F[] vertices;
        private int[] indices;

        // Internal
        internal CollisionMesh b3dMesh;
        internal ConvexHull b3dHull;

        // Properties
        public ReadOnlySpan<Vector3F> Vertices => vertices;
        public ReadOnlySpan<int> Indices => indices;

        // Constructor
        internal PhysicsMesh(string name, Vector3F[] vertices, int[] indices)
            : base(name)
        {
        }

        // Methods
        protected override void OnCreate()
        {
            // Get converted vertices
            ReadOnlySpan<Vector3> vertices = MemoryMarshal.Cast<Vector3F, Vector3>(this.vertices);
            
            // Create the mesh
            b3dMesh = CollisionMesh.FromTriangles(vertices, indices);
        }

        public void SetVertices(Vector3F[] vertices)
        {
        }
    }
}
