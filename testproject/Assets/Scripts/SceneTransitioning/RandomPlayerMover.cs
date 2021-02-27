using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;

public class RandomPlayerMover : NetworkedBehaviour
{
    public Vector3 targetLocation;
    public float speed = 1;

    private Rigidbody m_Rigidbody;

    [HideInInspector]
    private bool IsPaused;

    [SerializeField]
    private GameObject m_CameraRoot;

    private MeshRenderer m_MeshRenderer;

    [HideInInspector]
    [SerializeField]
    private RigidbodyConstraints m_DefaultConstraints;

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();


        if(m_Rigidbody)
        {
            if(m_DefaultConstraints == RigidbodyConstraints.None)
            {
                m_DefaultConstraints = m_Rigidbody.constraints;
            }
            else
            {
                m_Rigidbody.constraints = m_DefaultConstraints;
            }
        }

        m_MeshRenderer = GetComponent<MeshRenderer>();

        var temp = transform.position;
        temp.y = 0.5f;
        transform.position = temp;
        targetLocation = GetRandomLocation();
    }

    public void SetPlayerCamera()
    {
        if (m_CameraRoot && IsLocalPlayer)
        {
            if (Camera.main && Camera.main.transform.parent == null)
            {
                Camera.main.transform.parent = m_CameraRoot.transform;
                Camera.main.transform.localPosition = Vector3.zero;
                Camera.main.transform.localRotation = Quaternion.identity;
            }
        }
    }

    public void OnIsHidden(bool isHidden)
    {
        if (!m_MeshRenderer)
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
        }

        if (m_MeshRenderer)
        {
            m_MeshRenderer.enabled = !isHidden;
        }
    }

    public void OnPaused(bool isPaused)
    {
        IsPaused = isPaused;
        if(!m_Rigidbody)
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            if(m_Rigidbody)
            {
                if(m_DefaultConstraints == RigidbodyConstraints.None)
                {
                    m_DefaultConstraints = m_Rigidbody.constraints;
                }
                else
                {
                    m_Rigidbody.constraints = m_DefaultConstraints;
                }
            }
        }

        if(m_Rigidbody)
        {
            if (IsPaused)
            {
                m_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                m_Rigidbody.velocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                m_Rigidbody.constraints = m_DefaultConstraints;
            }
        }
    }

    private void FixedUpdate()
    {
        if(!IsPaused && IsOwner)
        {
            float distance = Vector3.Distance(transform.position, targetLocation);
            if (distance > 0.5f)
            {
                var stepPosition = Vector3.MoveTowards(transform.position, targetLocation, speed * Time.fixedDeltaTime);
                transform.rotation = Quaternion.LookRotation(stepPosition.normalized);
                m_Rigidbody.MovePosition(stepPosition);
            }
            else
            {
                targetLocation = GetRandomLocation();
            }
        }
    }

    private Vector3 GetRandomLocation()
    {
        return new Vector3(Random.Range(-15.0f, 15.0f), transform.position.y, Random.Range(-15.0f, 15.0f));
    }
}