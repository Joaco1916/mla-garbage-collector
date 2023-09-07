using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;

public class GarbageCollectorSettings : MonoBehaviour
{
    [HideInInspector]
    public GameObject[] agents;
    [HideInInspector]
    public GarbageCollectorArea[] listArea;

    public float totalScore;
    //public Text scoreText;

    StatsRecorder m_Recorder;

    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        m_Recorder = Academy.Instance.StatsRecorder;
    }

    void EnvironmentReset()
    {
        ClearObjects(GameObject.FindGameObjectsWithTag("paper"));
        ClearObjects(GameObject.FindGameObjectsWithTag("plastic"));
        ClearObjects(GameObject.FindGameObjectsWithTag("glass"));

        agents = GameObject.FindGameObjectsWithTag("agent");
        listArea = FindObjectsOfType<GarbageCollectorArea>();
        foreach (var gca in listArea)
        {
            gca.ResetGarbageArea(agents);
        }

        totalScore = 0;
    }

    void ClearObjects(GameObject[] objects)
    {
        foreach (var garbage in objects)
        {
            Destroy(garbage);
        }
    }

    public void Update()
    {
        //scoreText.text = $"Score: {totalScore}";

        // Send stats via SideChannel so that they'll appear in TensorBoard.
        // These values get averaged every summary_frequency steps, so we don't
        // need to send every Update() call.
        if ((Time.frameCount % 100) == 0)
        {
            m_Recorder.Add("TotalScore", totalScore);
        }
    }
}
