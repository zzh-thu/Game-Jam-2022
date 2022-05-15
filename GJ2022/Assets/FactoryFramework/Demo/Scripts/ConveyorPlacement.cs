using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FactoryFramework;
public class ConveyorPlacement : MonoBehaviour
{
    [Header("Placement Events")]
    public VoidEventChannel_SO startPlacementEvent;
    public VoidEventChannel_SO finishPlacementEvent;
    public VoidEventChannel_SO cancelPlacementEvent;

    [Header("Conveyor Setup")]
    public Conveyor conveyorPrefab;
    private Conveyor current;

    private Vector3 startPos;
    private float startHeight;
    private Vector3 endPos;

    private Socket startSocket;
    private Socket endSocket;

    [Header("Visual Feedback Materials")]
    public Material originalFrameMat;
    public Material originalBeltMat;
    public Material greenGhostMat;
    public Material redGhostMat;

    [Header("Controls")]
    public KeyCode cancelKey = KeyCode.Escape;

    private enum State
    {
        None,
        Start,
        End
    }
    [SerializeField] private State state;

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

    private bool TryChangeState(State desiredState)
    {
        state = desiredState;
        return true;
    }

    public void StartPlacingConveyor() {
        //cancel any placement currently happening
        cancelPlacementEvent?.Raise();
        // instantiate a belt to place
        current = Instantiate(conveyorPrefab);
        if (TryChangeState(State.Start))
        {
            startSocket = null;
            endSocket = null;
        }
        // trigger event
        startPlacementEvent?.Raise();
    }

    private bool ValidLocation()
    {
        if (current == null) return false;
        
            foreach (Collider c in Physics.OverlapSphere(startPos, 1f))
            {
                if (c.tag == "Building" && c.gameObject != current.gameObject)
                {
                    // colliding something!
                    if (ConveyorLogisticsUtils.settings.SHOW_DEBUG_LOGS)
                        Debug.LogWarning($"Invalid placement: {current.gameObject.name} collides with {c.gameObject.name} at the start");
                    //ChangeMatrerial(redPlacementMaterial);
                    return false;
                }
            }
            foreach (Collider c in Physics.OverlapSphere(endPos, 1f))
            {
                if (c.tag == "Building" && c.gameObject != current.gameObject)
                {
                    // colliding something!
                    if (ConveyorLogisticsUtils.settings.SHOW_DEBUG_LOGS)
                        Debug.LogWarning($"Invalid placement: {current.gameObject.name} collides with {c.gameObject.name} at the end");
                    //ChangeMatrerial(redPlacementMaterial);
                    return false;
                }
            }

        //ChangeMatrerial(greenPlacementMaterial);
        return true;
    }
    void HandleStartState()
    {
        Debug.Assert(current != null, "Not currently placing a conveyor.");
        Vector3 worldPos = Vector3.zero;
        Vector3 worldDir = Vector3.forward;
        Ray mousedownRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (RaycastHit hit in Physics.RaycastAll(mousedownRay, 100f))
        {
            if (hit.collider.gameObject.TryGetComponent(out Socket socket))
            {
                if (!socket.IsOpen())
                {
                    // Socket already Occupied
                    break;
                }
                worldPos = hit.collider.transform.position;
                worldDir = hit.collider.transform.forward;
                startSocket = socket;
                break;
            }
            
            if (hit.collider.gameObject.TryGetComponent(out Terrain t))
            {
                startSocket = null;
                worldPos = hit.point;
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0f;
                camForward.Normalize();
                worldDir = camForward;
            }
        }
        startPos = worldPos;
        // setup the start and end vectors to solve for path building
        current.start = worldPos;
        current.startDir = worldDir;
        current.end = worldPos + worldDir;
        current.endDir = worldDir;
        // get the relative height
        startHeight = (startSocket == null) ? 0f :startSocket.transform.position.y - Terrain.activeTerrain.SampleHeight(worldPos);
        //ignore colliders from start and end points
        List<Collider> collidersToIgnore = new List<Collider>();
        // add colliders associated with the connected start socket
        if (startSocket != null)
        {
            collidersToIgnore.AddRange(startSocket.transform.root.GetComponentsInChildren<Collider>());
            collidersToIgnore.Remove(startSocket.transform.root.GetComponent<Collider>());
        }
        
        //if (collidersToIgnore.Count > 0)
        current.UpdateMesh(ignored: collidersToIgnore.ToArray());
        //else
        //    current.UpdateMesh();

        // startSocket != null prevents belt from starting disconnected
        if (current.ValidMesh && startSocket != null)
        {
            current.SetMaterials(greenGhostMat, greenGhostMat);
            if (Input.GetMouseButtonDown(0))
            {
                TryChangeState(State.End);
            }
        }
        else 
            current.SetMaterials(redGhostMat, redGhostMat);

        
    }

    void HandleEndState()
    {
        Debug.Assert(current != null, "Not currently placing a conveyor.");
        Vector3 worldPos = Vector3.zero;
        Vector3 worldDir = Vector3.forward;
        Ray mousedownRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        foreach (RaycastHit hit in Physics.RaycastAll(mousedownRay, 100f))
        {
            if (hit.collider.transform.root == current.transform) continue;
            // want to specifically connect to a conveyor socket, not a belt bridge
            if (hit.collider.gameObject.TryGetComponent<ConveyorSocket>(out ConveyorSocket socket))
            {
                if (!socket.IsOpen())
                {
                    // Socket already Occupied
                    break;
                }
                worldPos = hit.collider.transform.position;
                worldDir = hit.collider.transform.forward;
                endSocket = socket;

                current.DisableBridge();

                break;
            }
            if (hit.collider.gameObject.TryGetComponent<Terrain>(out Terrain t))
            {
                worldPos = hit.point;
                // stay same level if this is the terrain

                worldPos.y = Terrain.activeTerrain.SampleHeight(worldPos) + startHeight;
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0f;
                camForward.Normalize();
                worldDir = camForward;
                // reset socket
                endSocket = null;

                // enable the belt bridge connection
                current.EnableBridge();

            }
            
        }
        endPos = worldPos;
        current.end = worldPos;
        current.endDir = worldDir;
        List<Collider> collidersToIgnore = new List<Collider>();
        //add colliders associated with the connected start and end sockets
        //THIS IS NOT A GREAT WAY TO DO THIS - CONSIDER USING LAYERMASKS
        collidersToIgnore.AddRange(FindObjectsOfType<TerrainCollider>());
        if (startSocket != null)
            collidersToIgnore.AddRange(startSocket.GetComponentsInChildren<Collider>());
        if (endSocket != null)
            collidersToIgnore.AddRange(endSocket.GetComponentsInChildren<Collider>());
        ConveyorBridge bridge = current.GetComponentInChildren<ConveyorBridge>();
        if (bridge != null)
            collidersToIgnore.Add(bridge.GetComponent<Collider>());

        current.UpdateMesh(
            startskip: startSocket != null ? 2 : 0, 
            endskip: 1, 
            ignored: collidersToIgnore.Count > 0 ? collidersToIgnore.ToArray() : null
        );

        if (current.ValidMesh)
            current.SetMaterials(greenGhostMat, greenGhostMat);
        else
            current.SetMaterials(redGhostMat, redGhostMat);

        if (Input.GetMouseButtonDown(0) && current.ValidMesh)
        {

            // change the sockets!
            if (startSocket != null)
            {
                startSocket.Connect(current);
            }
            if (endSocket != null)
            {
                endSocket.Connect(current);
            }
            // finalize the conveyor
            current.UpdateMesh(true);
            current.SetMaterials(originalFrameMat, originalBeltMat);
            current.AddCollider();

            // stop placing conveyor
            current = null;
            startSocket = null;
            endSocket = null;

            TryChangeState(State.None);
            finishPlacementEvent?.Raise();
        }
    }

    void HandleNoneState()
    {
        // right click to delete
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            foreach (RaycastHit hit in Physics.RaycastAll(ray, 100f))
            {
                if (hit.collider.transform.root.TryGetComponent<Conveyor>(out Conveyor conveyor))
                {
                    //foreach (Socket socket in building.gameObject.GetComponentsInChildren<Socket>())
                    //{
                    //    // do nothing
                    //}
                    Destroy(conveyor.gameObject);
                    return;
                }
            }
        }
        return;
        
    }

    public void Update()
    {
        if (Input.GetKeyDown(cancelKey))
        {
            if (current != null)
                Destroy(current.gameObject);
            current = null;
            startSocket = null;
            endSocket = null;
            state = State.None;
        }
        switch (state)
        {
            case State.None:
                HandleNoneState();
                break;
            case State.Start:
                HandleStartState();
                break;
            case State.End:
                HandleEndState();
                break;
        }
    }

}
