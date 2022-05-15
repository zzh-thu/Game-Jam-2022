using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FactoryFramework
{
    public class Conveyor : LogisticComponent, IInput, IOutput
    {
        private float Length { get { return (p == null) ? 0f : p.GetTotalLength(); } }
        public Vector3 start = Vector3.zero;
        public Vector3 end = Vector3.forward;
        public Vector3 startDir = Vector3.forward;
        public Vector3 endDir = Vector3.forward;
        public IPath p;
        public BeltMeshSO frameBM;
        public BeltMeshSO beltBM;
        private bool _validMesh = true;
        public bool ValidMesh { get { return _validMesh; } }

        // pool for gameobjects (models) on belts
        protected Pool<Transform> beltObjectPool;

        [SerializeField] private MeshFilter frameFilter;
        [SerializeField] private MeshFilter beltFilter;
        private ConveyorBridge bridge;
        
        public float speed = 1f;
        private MeshRenderer beltMeshRenderer;

        private int capacity;
        public List<ItemOnBelt> items = new List<ItemOnBelt>();
        //private List<Transform> _transforms = new List<Transform>();
        private ItemOnBelt LastItem { get { return items[items.Count - 1]; } }
        // private int currentLoad; 

        private ConveyorJob _conveyorJob;
        private JobHandle _jobHandle;
        private NativeArray<float> _itemPositionsArray;
        private TransformAccessArray _transformsArray;

        // events
        public UnityEvent<Conveyor> OnConveyorDestroyed;

        public void CalculateCapacity()
        {
            // this function also serves as a sort of Init()

            // capcity is a simple calculation that depends on belt length and belt_spacing
            capacity = Mathf.FloorToInt(Length / settings.BELT_SPACING);
            
            // create a pool of gameobjects with rendering capabilities
            // pool => CreateFunc, DestroyFunc, GetFunc, ReleaseFunc, capacity
            beltObjectPool = new Pool<Transform>(
                () => { 
                    GameObject item = new GameObject();
                    item.transform.parent = transform;
                    item.AddComponent<MeshFilter>();
                    item.AddComponent<MeshRenderer>();
                    return item.transform;
                },
                (Transform t) => { Destroy(t.gameObject); },
                (Transform t) => { t.gameObject.SetActive(true); },
                (Transform t) => { t.gameObject.SetActive(false); },
                capacity);

            
            //_itemPositionsArray = new NativeArray<float>(capacity, Allocator.Persistent);
        }
        private void Awake() { 
            capacity = Mathf.FloorToInt(Length / settings.BELT_SPACING); //CalculateCapacity();
            bridge = GetComponentInChildren<ConveyorBridge>();
            bridge.gameObject.SetActive(false);
        }

        public void EnableBridge() => bridge.gameObject.SetActive(true);
        public void DisableBridge() => bridge.gameObject.SetActive(false);
        public bool CanGiveOutput(Item filter = null)
        {
            if (filter != null) Debug.LogWarning("Conveyor Belt Does not Implement Item Filter Output");
            if (items.Count == 0) return false;
            return items[0].position == Length;
        }
        public bool CanTakeInput(Item item)
        {
            if (items.Count == 0) return true;
            else if (capacity == 0) return false;
            // make sure the previous item on the belt is far enough away to leave room for a new item
            return LastItem.position >= settings.BELT_SPACING;
        }
        public void TakeInput(Item item)
        {
            if (!CanTakeInput(item))
                Debug.LogError($"Belt is trying to accept input {item} when it is unable to.");
            
          
            ItemOnBelt iob = new ItemOnBelt()
            {
                item = item,
                position = 0f
            };
         
            Transform newItemModel = beltObjectPool.GetItem();
            if (newItemModel == null)
            {
                Debug.LogError($"error with {gameObject.name} no items left to grab");
            }

            newItemModel.GetComponent<MeshFilter>().sharedMesh = iob.item.prefab.GetComponent<MeshFilter>().sharedMesh;
            newItemModel.GetComponent<MeshRenderer>().sharedMaterial = iob.item.prefab.GetComponent<MeshRenderer>().sharedMaterial;
            iob.model = newItemModel;
            items.Add(iob);
            capacity -= 1;
            return;
        }

        public Item OutputType()
        {
            if (items.Count == 0) return null;
            if (items[0].position < Length) return null;
            return items[0].item;
        }
        public Item GiveOutput(Item filter = null)
        {
            if (!CanGiveOutput())
                Debug.LogError($"Belt is trying to GiveOutput when it is unable to.");
            if (filter != null) Debug.LogWarning("Conveyor Belt Does not Implement Item Filter Output");

            ItemOnBelt firstItem = items[0];
            // return item model to pool
            Transform model = firstItem.model;
            firstItem.model = null;
            beltObjectPool.ReleaseItem(model);
            // actually remove this item
            items.RemoveAt(0);
            // add 1 to  remaining capacity
            capacity += 1;
            return firstItem.item;
        }
        
        public override void ProcessLoop()
        {
            MoveItems();
            // FIXME because IPath is not a struct
            //MoveItemsJob(); // this will be the Jobs/Burst way to do things
        }

        public void MoveItems()
        {
            // this can be done with Jobs
            float cumulativeMaxPos = Length;

            for (int x = 0; x < items.Count; x++)
            {
                float position = items[x].position;
                position += speed * Time.deltaTime;
                position = math.clamp(position, 0f, cumulativeMaxPos);


                ItemOnBelt item = items[x];
                item.position = position;
                items[x] = item;


                Transform t = item.model;
                float pos = item.position;
                float percent = pos / p.GetTotalLength();
                Vector3 worldPos = p.GetWorldPointFromPathSpace(percent);
                Quaternion worldRotation = p.GetRotationAtPoint(percent);
                t.SetPositionAndRotation(worldPos, worldRotation);
                

                // update max cumulative position
                cumulativeMaxPos -= 1f * settings.BELT_SPACING;
            }
        }

        private void Update()
        { 
            ProcessLoop();

            // FIXME maybe don't need to do this every update
            beltMeshRenderer?.material.SetFloat("_Speed", speed);
        }

        private void LateUpdate()
        {
            _jobHandle.Complete();

            for (int i = 0; i < _conveyorJob.itemPositions.Length; i++)
            {
                ItemOnBelt iob = items[i];
                iob.position = _conveyorJob.itemPositions[i];
                items[i] = iob;
            }
        }

        void MoveItemsJob()
        {
            if (items.Count == 0) return;
            
     
            Transform[] _transforms = new Transform[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                _transforms[i] = items[i].model;
                _itemPositionsArray[i] = items[i].position;
            }
            _transformsArray = new TransformAccessArray(_transforms);


            _conveyorJob = new ConveyorJob()
            {
                //path=p,
                itemPositions = _itemPositionsArray,
                speed = speed,
                spacing = settings.BELT_SPACING,
                length = Length,
                deltatime = Time.deltaTime
            };
            _jobHandle = _conveyorJob.Schedule(_transformsArray);
        }

        public void UpdateMesh(bool finalize = false, Collider[] ignored = null, int startskip = 0, int endskip = 0)
        {
            _validMesh = true;
            p?.CleanUp();
            p = PathFactory.GeneratePathOfType(start, startDir, end, endDir, settings.PATHTYPE);

            if (!p.IsValid) _validMesh = false;
            int length = Mathf.Max(4,(int)(p.GetTotalLength() * settings.BELT_SEGMENTS_PER_UNIT));

            bool collision = PathFactory.CollisionAlongPath(p, 0.5f, ConveyorLogisticsUtils.settings.BELT_SCALE/2f, ~0, ignored, startskip: startskip, endskip: endskip); //only collide belt collideable layer
            if (collision)
            {
                _validMesh = false;
            }

            frameFilter.mesh = BeltMeshGenerator.Generate(p, frameBM, length, ConveyorLogisticsUtils.settings.BELT_SCALE);
            beltFilter.mesh = BeltMeshGenerator.Generate(p, beltBM, length, ConveyorLogisticsUtils.settings.BELT_SCALE, 1f, true);

            beltMeshRenderer = beltFilter.gameObject.GetComponent<MeshRenderer>();

            if (finalize)
                CalculateCapacity();
        }

        public void SetMaterials(Material frameMat, Material beltMat)
        {
            frameFilter.gameObject.GetComponent<MeshRenderer>().material = frameMat;
            beltFilter.gameObject.GetComponent<MeshRenderer>().material = beltMat;
        }
        public void AddCollider()
        {
            frameFilter.gameObject.AddComponent(typeof(MeshCollider));
        }

        private void OnDestroy()
        {
            //_itemPositionsArray.Dispose();
            //_transformsArray.Dispose();
            p?.CleanUp();
            OnConveyorDestroyed?.Invoke(this);
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            // doesnt matter item type
            if (!CanGiveOutput() && CanTakeInput(null))
            {
                Gizmos.color = Color.green;
            }
            else if (CanGiveOutput() && !CanTakeInput(null))
            {
                Gizmos.color = Color.red;
            }
            else if (!CanGiveOutput() && !CanTakeInput(null))
            {
                Gizmos.color = Color.yellow;
            }

            Gizmos.matrix = transform.localToWorldMatrix;
            Handles.matrix = transform.localToWorldMatrix;

            foreach (ItemOnBelt i in items)
            {
                Gizmos.color = i.item.DebugColor;

                float pos = i.position;
                float percent = pos / p.GetTotalLength();
                Vector3 worldPos = p.GetWorldPointFromPathSpace(percent);

                Gizmos.DrawWireSphere(worldPos, settings.BELT_SPACING / 2f);

            }
        }
#endif
    }

    [BurstCompile]
    public struct ConveyorJob : IJobParallelForTransform
    {
        public NativeArray<float> itemPositions;
        //public IPath path;
        public float speed;
        public float spacing;
        public float length;
        public float deltatime;

        public void Execute(int index, TransformAccess transform)
        {
            float maxPos = length - ((float)index * spacing);
            float position = itemPositions[index];
            position = math.clamp(position + speed * deltatime, 0f, maxPos);
            itemPositions[index] = position;

            //float percent = position / path.GetTotalLength();
            //Vector3 worldPos = path.GetWorldPointFromPathSpace(percent);
            //Quaternion worldRotation = path.GetRotationAtPoint(percent);
            //transform.position = worldPos;
            //transform.rotation = worldRotation;
        }
    }
}