using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgentsExamples;

public class GarbageCollectorArea : Area
{
    //Basuras
    public GameObject paper;
    public GameObject plactic;
    public GameObject glass;

    //Nro de cada uno
    public int numPaper;
    public int numPlastic;
    public int numGlass;

    //Config
    public bool respawnGarbage;
    public float range;

    void CreateGarbage(int num, GameObject type)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject f = Instantiate(type, new Vector3(Random.Range(-range, range), 1f,
                Random.Range(-range, range*0.2f)) + transform.position,
                Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 90f)));
            f.GetComponent<GarbageLogic>().respawn = respawnGarbage;
            f.GetComponent<GarbageLogic>().myArea = this;
        }
    }

    public void ResetGarbageArea(GameObject[] agents)
    {
        foreach (GameObject agent in agents)
        {
            if (agent.transform.parent == gameObject.transform)
            {
                agent.transform.position = new Vector3(Random.Range(-range*0.4f, range*0.4f), 2f,
                    Random.Range(range*0.25f, range*0.5f))
                    + transform.position;
                agent.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            }
        }

        CreateGarbage(numPaper, paper);
        CreateGarbage(numPlastic, plactic);
        CreateGarbage(numGlass, glass);
    }

    public override void ResetArea()
    {
    }
}

