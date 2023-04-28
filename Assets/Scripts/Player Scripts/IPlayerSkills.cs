/// IPlayerSkills.cs
///
/// Interface for unique character skills implementation.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerSkills
{
    void ActivateBasicAttack();
    void ActivateSkill();
    void ActivateUltimate();
    float[] GetCooldown(); //0 - primary, 1 - secondary, 2 - ultimate
}
