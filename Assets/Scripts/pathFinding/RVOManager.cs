using UnityEngine;
using RVO;

public class RVOManager : MonoBehaviour
{
    [Header("RVO Global Settings")]
    [SerializeField] float timeStep = 0.02f;
    [SerializeField] float neighborDist = 3f;
    [SerializeField] int maxNeighbors = 10;
    [SerializeField] float timeHorizon = 1.0f;
    [SerializeField] float timeHorizonObst = 1.0f;
    [SerializeField] float radius = 0.5f;
    [SerializeField] float maxSpeed = 5f;

    private static RVOManager instance;
    public static RVOManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<RVOManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("RVOManager");
                    instance = go.AddComponent<RVOManager>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRVO();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeRVO()
    {
        Simulator.Instance.setTimeStep(timeStep);
        Simulator.Instance.setAgentDefaults(
            neighborDist, 
            maxNeighbors, 
            timeHorizon, 
            timeHorizonObst, 
            radius, 
            maxSpeed, 
            new RVO.Vector2(0f, 0f)
        );
        
        Debug.Log("RVO Manager initialized with timeStep: " + timeStep);
    }

    void LateUpdate()
    {
        Simulator.Instance.doStep();
    }
    
    public float GetTimeStep() => timeStep;
    public float GetNeighborDist() => neighborDist;
    public float GetTimeHorizon() => timeHorizon;
    public float GetDefaultRadius() => radius;
}