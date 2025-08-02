using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hitfx : MonoBehaviour
{

    public GameObject explosion; // drag your explosion prefab here

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Player")
        {
            GameObject expl = Instantiate(explosion, transform.position, Quaternion.identity) as GameObject;
            Destroy(expl, 3); // delete the explosion after 3 seconds
        }

    }

}