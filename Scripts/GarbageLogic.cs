using UnityEngine;

public class GarbageLogic : MonoBehaviour
{
    public bool respawn;
    public GarbageCollectorArea myArea;

    public void OnCollected()
    {
        if (respawn)
        {
            transform.position = new Vector3(Random.Range(-myArea.range, myArea.range),
                3f,
                Random.Range(-myArea.range, myArea.range*0.2f)) + myArea.transform.position;
            //set the velocity and angular velocity to zero
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Colisión con pared
        if (collision.gameObject.CompareTag("wall"))
        {
            if (respawn)
            {
                transform.position = new Vector3(Random.Range(-myArea.range, myArea.range),
                    3f,
                    Random.Range(-myArea.range, myArea.range*0.2f)) + myArea.transform.position;
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
