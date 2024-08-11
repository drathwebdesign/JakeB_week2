using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerSpin : MonoBehaviour {

    public float rotationSpeed = 2000f;

    // Update is called once per frame
    void Update() {
        //spin propeller
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}