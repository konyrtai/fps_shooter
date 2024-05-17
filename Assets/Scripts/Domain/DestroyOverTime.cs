using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    /// <summary>
    /// Время жизни
    /// </summary>
    public float lifetime = .5f;

    private void Start()
    {
        Destroy(this.gameObject, lifetime);
    }
}
