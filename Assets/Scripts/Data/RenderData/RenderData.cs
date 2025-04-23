using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VNWGame.Game
{
    public class RenderData
    {
        protected MaterialPropertyBlock mpb;
        protected Material mat;
        protected RenderParams rp;
        protected Mesh mesh;

        public RenderData(int MaxCount, Mesh mesh, Material material)
        {
            mpb = new MaterialPropertyBlock();
            mat = Material.Instantiate(material);
            this.mesh = mesh;
            rp = new RenderParams(material)
            {
                worldBounds = new Bounds(Vector3.zero, 100000 * Vector3.one), // use tighter bounds
                matProps = mpb,
            };
        }

        public void Render(int count)
        {
            if (count > 0)
                Graphics.DrawMeshInstancedProcedural(mesh, 0, mat, rp.worldBounds, count, mpb);
        }

        public virtual void Clear()
        {
            Material.DestroyImmediate(mat);
            mat = null;
        }
    }
}
