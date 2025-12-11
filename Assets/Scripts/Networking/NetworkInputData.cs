using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

enum SpaceshipButtons
{
    Fire = 0,
}

public struct NetworkInputData : INetworkInput
{   
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public NetworkButtons Buttons;
}
