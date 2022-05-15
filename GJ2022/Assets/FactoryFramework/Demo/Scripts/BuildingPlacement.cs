using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FactoryFramework;

public class BuildingPlacement : MonoBehaviour
{
    [Header("Event Channels to Handle State")]
    public VoidEventChannel_SO startPlacementEvent;
    public VoidEventChannel_SO finishPlacementEvent;
    public VoidEventChannel_SO cancelPlacementEvent;
    
    [Header("Building Prefabs")]
    public GameObject Miner;
    public GameObject Processor;
    public GameObject Factory;
    public GameObject Storage;
    public GameObject Splitter;
    public GameObject Merger;
    public GameObject Assembler;
    private GameObject current;

    [Header("Visual Feedback Building Materials")]
    public Material originalMaterial;
    public Material greenPlacementMaterial;
    public Material redPlacementMaterial;

    // list of valid materials so we don't change any other materials
    private Material[] validMaterials {
        get { 
            return new Material[3] { originalMaterial, greenPlacementMaterial, redPlacementMaterial }; 
        } 
    }

    [Header("Controls")]
    public KeyCode CancelKey = KeyCode.Escape;

    private enum State
    {
        None,
        PlaceBuilding,
        RotateBuilding
    }
    private State state;
    private bool RequiresResourceDepoist = false;

    // building placement variables to track
    private Vector3 mouseDownPos;
    private float mouseHeldTime = 0f;
    private float secondsHoldToRotate = .333f;

    private void OnEnable()
    {
        // listen to the cancel event to force cancel placement from elsewhere in the code
        cancelPlacementEvent.OnEvent += ForceCancel;
    }
    private void OnDisable()
    {
        // stop listening
        cancelPlacementEvent.OnEvent -= ForceCancel;
    }

    private void ForceCancel()
    {
        if (current != null)
        {
            Destroy(current.gameObject);
        }
        current = null;
        this.state = State.None;
    }

    public void PlaceMiner() => StartPlacingBuilding(Miner, true);
    public void PlaceProcessor() => StartPlacingBuilding(Processor);
    public void PlaceFactory() => StartPlacingBuilding(Factory);
    public void PlaceAssembler() => StartPlacingBuilding(Assembler);
    public void PlaceStorage() => StartPlacingBuilding(Storage);
    public void PlaceSplitter() => StartPlacingBuilding(Splitter);
    public void PlaceMerger() => StartPlacingBuilding(Merger);

    public void StartPlacingBuilding(GameObject prefab, bool requireDeposit=false)
    {
        cancelPlacementEvent?.Raise();
        RequiresResourceDepoist = requireDeposit;
        // spawn a prefab and start placement
        if (!TryChangeState(State.PlaceBuilding))
            return;
        
        current = Instantiate(prefab);
        current.name = prefab.name;
        // don't let building "work" until placement is finished
        Building b = current.GetComponent<Building>();
        b.enabled = false;
        // init material to ghost
        ChangeMatrerial(greenPlacementMaterial);
        
    }

    private void ChangeMatrerial(Material mat)
    {
        foreach (MeshRenderer mr in current?.GetComponentsInChildren<MeshRenderer>())
        {
            // dont change materials that shouldn't be changed!
            if (validMaterials.Contains(mr.sharedMaterial))
                mr.sharedMaterial = mat;
        }
    }

    private void HandleIdleState()
    {
        // right click to delete
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            foreach (RaycastHit hit in Physics.RaycastAll(ray, 100f))
            {
                if (hit.collider.gameObject.TryGetComponent<Building>(out Building building))
                {
                    foreach(Socket socket in building.gameObject.GetComponentsInChildren<Socket>())
                    {
                        // FIXME remove this from sockets
                    }
                    Destroy(building.gameObject);
                    return;
                }
            }
        }
        return;
    }
    private void HandlePlaceBuildingState()
    {
        // move building with mouse pos
        Vector3 groundPos = transform.position;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        foreach (RaycastHit hit in Physics.RaycastAll(ray, 100f))
        {
            // this will only place buildings on terrain. feel free to change this!
            if (hit.collider.TryGetComponent<Terrain>(out Terrain terrain))
            {
                groundPos = hit.point;
            }
        }


        current.transform.position = groundPos;
        bool valid = ValidLocation();
        // left mouse button to try to place building
        if (Input.GetMouseButtonDown(0) && valid)
        {
            // try to change state to rotate the building
            if (TryChangeState(State.RotateBuilding))
                mouseDownPos = groundPos;
        }

    }
    private void HandleRotateBuildingState()
    {
        // wait for mouse to be held for X seconds until building rotation is allowed
        // this prevents quick clicks resulting in seemingly random building rotations
        mouseHeldTime += Time.deltaTime;
        if (mouseHeldTime > secondsHoldToRotate)
        {
            bool valid = ValidLocation();
            // get new ground position to rotate towards
            Vector3 dir = current.transform.forward;
            // rotate the building!
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            foreach (RaycastHit hit in Physics.RaycastAll(mouseRay, 100f))
            {
                // thios demo script will only use a terrain object as the "ground"
                if (hit.collider.TryGetComponent<Terrain>(out Terrain terrain))
                    current.transform.forward = (mouseDownPos - hit.point).normalized;
            }
            current.transform.position = mouseDownPos;
        }

        if (Input.GetMouseButtonUp(0))
        {
            TryChangeState(State.None);
        }
    }

    private bool ValidLocation()
    {
        if (current == null) return false;
        // this only works with box xcolliders because thats an assumption we made with the demo prefabs!
        if (current.TryGetComponent<BoxCollider>(out BoxCollider col))
        {
            bool onResourceDeposit = false;
            foreach (Collider c in Physics.OverlapBox(col.transform.TransformPoint(col.center), col.size/2f, col.transform.rotation))
            {
                if (c.tag == "Building" && c.gameObject != current.gameObject)
                {
                    // colliding something!
                    if (ConveyorLogisticsUtils.settings.SHOW_DEBUG_LOGS)
                        Debug.LogWarning($"Invalid placement: {current.gameObject.name} collides with {c.gameObject.name}");
                    ChangeMatrerial(redPlacementMaterial);
                    return false;
                }
                // check for resources
                if (c.tag == "Resources")
                {
                    onResourceDeposit = true;
                }
            }
            if (RequiresResourceDepoist && !onResourceDeposit)
            {
                if (ConveyorLogisticsUtils.settings.SHOW_DEBUG_LOGS)
                    Debug.LogWarning($"Invalid placement: {current.gameObject.name} requries placement near Resource Deposit");
                ChangeMatrerial(redPlacementMaterial);
                return false;
            }
        }
        ChangeMatrerial(greenPlacementMaterial);
        return true;
    }

    private bool TryChangeState(State desiredState)
    {
        if (desiredState == State.PlaceBuilding)
        {
            if (state != State.None || current != null)
            {
                // if currently placing a building, cancel it
                Destroy(current);
                state = State.None;
                cancelPlacementEvent?.Raise();
            }
            mouseHeldTime = 0f;
            this.state = desiredState;
            // trigger event
            startPlacementEvent?.Raise();
            return true;
        }
        if (desiredState == State.RotateBuilding)
        {
            this.state = desiredState;
            return true;
        }
        if (desiredState == State.None)
        {   
            // if we weren't placing a building, ignore
            if (current == null)
            {
                this.state = desiredState;
                return true;
            }

            // make sure building placement and rotation is valid
            if (ValidLocation())
            {
                // finish placing building and enable it
                this.state = desiredState;
                ChangeMatrerial(originalMaterial);
                Building b = current.GetComponent<Building>();
                b.enabled = true;
                current = null;
                // trigger event
                finishPlacementEvent?.Raise();
                return true;
            }
            else
            {
                this.state = State.PlaceBuilding;
                mouseHeldTime = 0f;
                return false;
            }
        }
        return false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(CancelKey))
        {
            if (current != null)
            {
                Destroy(current.gameObject);
                cancelPlacementEvent?.Raise();
            }
            current = null;
            state = State.None;
        }

        switch (state)
        {
            case State.RotateBuilding:
                HandleRotateBuildingState();
                break;
            case State.None:
                HandleIdleState();
                break;
            case State.PlaceBuilding:
                HandlePlaceBuildingState();
                break;
        }

    }

}
