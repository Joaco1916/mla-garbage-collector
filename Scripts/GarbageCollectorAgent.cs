//Default packages
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ML-Agents packages
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

public class GarbageCollectorAgent : Agent
{
    GarbageCollectorSettings m_GarbageCollectorSettings;

    bool m_Paper;
    bool m_Plastic;
    bool m_Glass;
    bool m_Charged;

    // Speed of agent rotation.
    public float turnSpeed = 150;
    // Speed of agent movement.
    public float moveSpeed = 1;

    //Materiales de los objetos
    public Material normalMaterial;
    public Material paperMaterial;
    public Material plasticMaterial;
    public Material glassMaterial;

    // Buckets
    public GameObject paperBucket;
    public GameObject plasticBucket;
    public GameObject glassBucket;

    // Start is called before the first frame update
    public Rigidbody rBody;
    void Start () {
        rBody = GetComponent<Rigidbody>();
        m_GarbageCollectorSettings = FindObjectOfType<GarbageCollectorSettings>();
        paperBucket = GameObject.FindGameObjectsWithTag("paperBucket")[0];
        plasticBucket = GameObject.FindGameObjectsWithTag("plasticBucket")[0];
        glassBucket = GameObject.FindGameObjectsWithTag("glassBucket")[0];
    }

    //Paper
    void PickUpPaper()
    {
        m_Paper = true;
        m_Charged = true;
        gameObject.GetComponentInChildren<Renderer>().material = paperMaterial;
    }
    void ReleasePaper()
    {
        m_Paper = false;
        m_Charged = false;
        gameObject.GetComponentInChildren<Renderer>().material = normalMaterial;
    }

    //Plastic
    void PickUpPlastic()
    {
        m_Plastic = true;
        m_Charged = true;
        gameObject.GetComponentInChildren<Renderer>().material = plasticMaterial;
    }
    void ReleasePlastic()
    {
        m_Plastic = false;
        m_Charged = false;
        gameObject.GetComponentInChildren<Renderer>().material = normalMaterial;
    }

    //Glass
    void PickUpGlass()
    {
        m_Glass = true;
        m_Charged = true;
        gameObject.GetComponentInChildren<Renderer>().material = glassMaterial;
    }
    void ReleaseGlass()
    {
        m_Glass = false;
        m_Charged = false;
        gameObject.GetComponentInChildren<Renderer>().material = normalMaterial;
    }

    public override void OnEpisodeBegin()
    {
       // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3( 0, 0.5f, 0);

            AddReward(-50f);
        }

        print("OnEpisodeBegin reward: " + GetCumulativeReward());    
    }

    public override void CollectObservations(VectorSensor sensor)
    {     
        // Agent positions
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        // Agent status (charged or not)
        sensor.AddObservation(m_Charged);

        // Agent status (paper, plastic, glass)
        sensor.AddObservation(m_Paper);
        sensor.AddObservation(m_Plastic);
        sensor.AddObservation(m_Glass);

        // Distance to each bucket
        sensor.AddObservation(Vector3.Distance(this.transform.localPosition, paperBucket.transform.localPosition));
        sensor.AddObservation(Vector3.Distance(this.transform.localPosition, plasticBucket.transform.localPosition));
        sensor.AddObservation(Vector3.Distance(this.transform.localPosition, glassBucket.transform.localPosition));
    }

    public Color32 ToColor(int hexVal)
    {
        var r = (byte)((hexVal >> 16) & 0xFF);
        var g = (byte)((hexVal >> 8) & 0xFF);
        var b = (byte)(hexVal & 0xFF);
        return new Color32(r, g, b, 255);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var continuousActions = actionBuffers.ContinuousActions;

        var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
        var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
        var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);

        dirToGo = transform.forward * forward;
        dirToGo += transform.right * right;
        rotateDir = -transform.up * rotate;

        rBody.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);

        // Reduce the velocity when get to a limit of 20f
        if (rBody.velocity.sqrMagnitude > 20f)
        {
            rBody.velocity *= 0.95f;
        }

        // Time penalty
        AddReward(-0.05f);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Colisión con pared
        if (collision.gameObject.CompareTag("wall"))
        {
            AddReward(-1f);
            m_GarbageCollectorSettings.totalScore -= 1;
        }

        //Colisión con papel
        if (collision.gameObject.CompareTag("paper"))
        {
            if( !m_Charged ){
                PickUpPaper();
                collision.gameObject.GetComponent<GarbageLogic>().OnCollected();
                AddReward(100f);
                m_GarbageCollectorSettings.totalScore += 100;
            } else {
                AddReward(-1f);
                m_GarbageCollectorSettings.totalScore -= 1;
            }
        }
        if (collision.gameObject.CompareTag("paperBucket"))
        {
            if( m_Paper ){
                ReleasePaper();
                AddReward(500f);
                m_GarbageCollectorSettings.totalScore += 500;
                EndEpisode();
            } else {
                AddReward(-1f);
                m_GarbageCollectorSettings.totalScore -= 1;
            }
        }

        //Colisión con plástico
        if (collision.gameObject.CompareTag("plastic"))
        {
            if( !m_Charged ){
                PickUpPlastic();
                collision.gameObject.GetComponent<GarbageLogic>().OnCollected();
                AddReward(100f);
                m_GarbageCollectorSettings.totalScore += 100;
            } else {
                AddReward(-1f);
                m_GarbageCollectorSettings.totalScore -= 1;
            }
        }
        if (collision.gameObject.CompareTag("plasticBucket"))
        {
            if( m_Plastic ){
                ReleasePlastic();
                AddReward(500f);
                m_GarbageCollectorSettings.totalScore += 500;
                EndEpisode();
            } else {
                AddReward(-1f);
                m_GarbageCollectorSettings.totalScore -= 1;
            }
        }

        //Colisión con vidrio
        if (collision.gameObject.CompareTag("glass"))
        {
            if( !m_Charged ){
                PickUpGlass();
                collision.gameObject.GetComponent<GarbageLogic>().OnCollected();
                AddReward(100f);
                m_GarbageCollectorSettings.totalScore += 100;
            } else {
                AddReward(-1f);
                m_GarbageCollectorSettings.totalScore -= 1;
            }
        }
        if (collision.gameObject.CompareTag("glassBucket"))
        {
            if( m_Glass ){
                ReleaseGlass();
                AddReward(500f);
                m_GarbageCollectorSettings.totalScore += 500;
                EndEpisode();
            } else {
                AddReward(-1f);
                m_GarbageCollectorSettings.totalScore -= 1;
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.D))
        {
            continuousActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            continuousActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            continuousActionsOut[2] = -1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            continuousActionsOut[0] = -1;
        }
    }
}
