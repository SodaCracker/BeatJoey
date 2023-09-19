public abstract class StagingDirection : MusicalElement
{
    public virtual void OnBegin()
    {
    }

    public virtual void OnEnd()
    {
        
    }

    public virtual void Update()
    {
    }
    
    public virtual bool IsFinished { get; protected set; }
}