using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryFramework
{
    public class Producer : Building, IOutput
    {
        public float resourcesPerSecond;
        private float secondsPerResource { get { return 1f / resourcesPerSecond; } }
        public LocalStorage resource;
        private float _t;

        public void SetOutputResource(Item item)
        {
            if (item == resource.itemStack.item) return;
            resource.itemStack.amount = 0;
            resource.itemStack.item = item;
        }

        private void OnEnable()
        {
            // use mesh to calculate bounds
            Mesh m = GetComponent<MeshFilter>()?.mesh;
            Vector3 center = (m != null) ? m.bounds.center : transform.position;
            Vector3 size = (m != null) ? m.bounds.extents : Vector3.one;
            foreach (Collider c in Physics.OverlapBox(transform.TransformPoint(center), size))
            {
                if (c.TryGetComponent<Resource>(out Resource r)){
                    this.SetOutputResource(r.item);
                    break;
                }
            }
        }

        public override void ProcessLoop()
        {
            if (resource.itemStack.item == null) return;
            // maybe move this to coroutine or async
            if (resource.itemStack.amount == resource.itemStack.item.itemData.maxStack)
                return;
            _t += Time.deltaTime; // FIXME maybe
            if (_t > secondsPerResource)
            {
                resource.itemStack.amount += 1;
                _t = _t % secondsPerResource;
            }

        }

        public bool CanGiveOutput(Item filter = null)
        {
            if (filter != null) Debug.LogWarning("Producer Does not Implement Item Filter Output");
            return resource.itemStack.item != null && resource.itemStack.amount > 0;
        }
        public Item OutputType() { return resource.itemStack.item; }
        public Item GiveOutput(Item filter = null)
        {
            if (filter != null) Debug.LogWarning("Producer Does not Implement Item Filter Output");
            if (resource.itemStack.item == null || resource.itemStack.amount == 0) return null;
            resource.itemStack.amount -= 1;
            return resource.itemStack.item;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawWireSphere(Vector3.zero, 1f);
        }
    }
}