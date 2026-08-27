using System.Runtime.InteropServices;

namespace KoraGame.Graphics
{
    public sealed class Mesh : GameElement
    {
        // Properties
        public bool HasIndices
        {
            get
            {
                binding.CheckNative();
                return Mesh_GetHasIndices(binding.NativePtr);
            }
        }

        public bool HasVertices
        {
            get
            {
                binding.CheckNative();
                return Mesh_GetHasVertices(binding.NativePtr);
            }
        }

        public uint SubmeshCount
        {
            get
            {
                binding.CheckNative();
                return Mesh_GetSubmeshCount(binding.NativePtr);
            }
        }

        public GraphicsBuffer IndexBuffer
        {
            get
            {
                binding.CheckNative();
                IntPtr bufferPtr = Mesh_GetIndexBuffer(binding.NativePtr);
                return NativeBinding.GetManagedObject<GraphicsBuffer>(bufferPtr);
            }
        }

        public GraphicsBuffer VertexBuffer
        {
            get
            {
                binding.CheckNative();
                IntPtr bufferPtr = Mesh_GetVertexBuffer(binding.NativePtr);
                return NativeBinding.GetManagedObject<GraphicsBuffer>(bufferPtr);
            }
        }

        // Constructor
        private Mesh(IntPtr nativePtr)
        {
            binding.NativePtr = nativePtr;
        }

        public Mesh(uint submeshCount = 0)
        {
            binding.NativePtr = Mesh_CreateNative(binding.ManagedPtr, submeshCount);
        }

        // Methods
        public uint GetElementCount(uint submesh = 0)
        {
            binding.CheckNative();
            return Mesh_GetElementCount(binding.NativePtr, submesh);
        }

        // Bindings
        [UnmanagedCallersOnly]
        internal static IntPtr Mesh_CreateManaged(IntPtr nativePtr)
        {
            // Create new managed mesh
            Mesh mesh = new(nativePtr);

            // Get managed
            return mesh.binding.ManagedPtr;
        }

        [DllImport("KoraGame")]
        internal static extern IntPtr Mesh_CreateNative(IntPtr managedPtr, uint submeshCount);

        [DllImport("KoraGame")]
        internal static extern bool Mesh_GetHasIndices(IntPtr nativedPtr);

        [DllImport("KoraGame")]
        internal static extern bool Mesh_GetHasVertices(IntPtr nativedPtr);

        [DllImport("KoraGame")]
        internal static extern uint Mesh_GetSubmeshCount(IntPtr nativedPtr);

        [DllImport("KoraGame")]
        internal static extern IntPtr Mesh_GetIndexBuffer(IntPtr nativedPtr);

        [DllImport("KoraGame")]
        internal static extern IntPtr Mesh_GetVertexBuffer(IntPtr nativedPtr);

        [DllImport("KoraGame")]
        internal static extern uint Mesh_GetElementCount(IntPtr nativedPtr, uint submesh);
    }
}
