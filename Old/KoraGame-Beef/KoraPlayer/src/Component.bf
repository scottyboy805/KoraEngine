namespace KoraPlayer;

public abstract class Component : GameElement
{
	// Private
	private bool active = true;

	// Internal
	internal GameObject gameObject;

	// Properties
	public bool Active => active;
	public bool ActiveInScene => active == true && gameObject != null && gameObject.ActiveInScene == true;
	public GameObject GameObject => gameObject;
	public Scene Scene => gameObject.Scene;

	// Methods
	protected virtual void OnEnable(){}
	protected virtual void OnDisable(){}

	public void SetActive(bool on)
	{
		// Check for no change
		if(active == on)
			return;

		this.active = on;
		DoComponentEnabledEvent(this, on);
	}

	internal static void DoComponentEnabledEvent(Component component, bool on)
	{
		// Trigger event
		if(on == true)
		{
			// Do enable
			component.OnEnable();
		}
		else
		{
			// Do disable
			component.OnDisable();
		}
	}
}