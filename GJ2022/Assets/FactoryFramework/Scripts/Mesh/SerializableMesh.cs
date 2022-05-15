using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace FactoryFramework
{
    [Serializable]
    public class SerializableMesh : ISerializationCallbackReceiver
    {
        [NonSerialized]
        private Mesh mMesh;

        [SerializeField]
        int[] tris;
        [SerializeField]
        Vector3[] verts;
        [SerializeField]
        Vector3[] normals;
        [SerializeField]
        Vector2[] uvs;

        public SerializableMesh()
        {

        }
        public SerializableMesh(Mesh m)
        {
            SetMesh(m);
        }
        public void SetMesh(Mesh m)
        {
            mMesh = m;
            tris = m.triangles;
            verts = m.vertices;
            normals = m.normals;
            uvs = m.uv;
        }
        public Mesh GetMesh()
        {
            if (mMesh == null)
            {
                mMesh = new Mesh();
                mMesh.vertices = verts;
                mMesh.normals = normals;
                mMesh.uv = uvs;
                mMesh.triangles = tris;
            }
            return mMesh;
        }

        public void OnBeforeSerialize()
        {
            if (!mMesh)
                return;
            tris = mMesh.triangles;
            verts = mMesh.vertices;
            normals = mMesh.normals;
            uvs = mMesh.uv;
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
