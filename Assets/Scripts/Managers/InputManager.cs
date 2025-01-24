using UnityEngine;

public class InputManager : ManagerBase<InputManager>
{
    public override bool Initialize()
    {
        Application.targetFrameRate = 60;
        return base.Initialize();
    }
}
