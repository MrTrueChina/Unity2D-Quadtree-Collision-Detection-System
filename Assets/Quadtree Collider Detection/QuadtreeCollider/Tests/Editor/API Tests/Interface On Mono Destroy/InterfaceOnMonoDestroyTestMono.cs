using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceOnMonoDestroyTestMono : MonoBehaviour, IInterfaceOnMonoDestroyTestInterface
{
    public void SayHello()
    {
        Debug.Log("Hello");
    }
}
