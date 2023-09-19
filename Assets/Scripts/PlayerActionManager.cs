public enum EPlayerAction
{
    None,
    Wink,
    Jump,
}

public class PlayerActionManager : ManagerBase<PlayerActionManager>
{
    public void DoAction(EPlayerAction playerActionType)
    {
        throw new System.NotImplementedException();
    }
    public EPlayerAction CurrentPlayerActionType { get; private set; }

    public OnBeatActionInfo LastActionInfo { get; private set; } = new();

}