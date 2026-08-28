
namespace KoraGame
{
    public abstract class ScriptableAsset : GameElement
    {
        // Methods
        protected virtual void OnLoaded() { }

        internal void DoLoaded()
        {
            try
            {
                OnLoaded();
            }
            catch(Exception e)
            {
                Debug.LogException(e, LogFilter.Script);
            }
        }
    }
}
