using Box3D;
using System;
using System.Collections.Generic;
using System.Text;

namespace KoraGame.Physics
{
    public sealed class MeshCollider : Collider
    {
        // Methods
        protected override Shape CreateShape(Body b3dBody, in ShapeDefinition b3dShapeDef)
        {

            return b3dBody.AddMesh
        }
    }
}
