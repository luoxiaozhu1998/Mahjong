using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using Photon.Pun;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public Transform gaze;

    public Transform gaze2;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = gaze.position;
        transform.rotation = gaze2.rotation;
    }
}