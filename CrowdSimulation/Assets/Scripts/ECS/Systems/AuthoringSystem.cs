using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AuthoringSystem : MonoBehaviour
{
    private bool didStartExecute = false;

    protected virtual void Start()
    {
        didStartExecute = true;

        SetVariables();
    }

    private void OnValidate()
    {
        if (!didStartExecute) return;

        SetVariables();
    }

    protected virtual void SetVariables() {}
}