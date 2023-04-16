/// CooldownData.cs
/// 
/// Holds cooldown data for each ability
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CooldownData
{
    private float cooldownTime;
    private float startTime;
    private float endTime;
    private float timePassed;

    public CooldownData(float startTime, float cooldownTime)
    {
        this.startTime = startTime;
        this.cooldownTime = cooldownTime;
        this.endTime = startTime + cooldownTime;
    }

    public float getStartTime()
    {
        return this.startTime;
    }

    public float getEndTime()
    {
        return this.endTime;
    }

    public float getCooldownTime()
    {
        return this.cooldownTime;
    }

    public float getTimePassed()
    {
        return this.timePassed;
    }

    public void calculateTimePassed(float time)
    {
        this.timePassed = (time-this.startTime)/this.cooldownTime;
        Debug.Log("time "+(time-this.startTime));
        Debug.Log("time passed "+this.timePassed);
    }

    public void reset()
    {
        this.startTime = 0;
        this.endTime = 0;
        this.timePassed = 0;
    }

}