using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AnimatorParam
{
    public string name { get; }
    public bool val { get; }

    public AnimatorParam(string name, bool val)
    {
        this.name = name;
        this.val = val;
    }
}
