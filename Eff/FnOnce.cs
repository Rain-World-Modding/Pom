namespace Pom.Eff;

public sealed class FnOnce
{
	private bool done = false;
	public Exception? error { get; private set; } = null;
	private Action action;
	public FnOnce(Action action)
	{
		this.action = action;
	}
	public void Invoke()
	{
		if (done)
		{
			return;
		}
		try
		{
			action.Invoke();
		}
		catch (Exception ex)
		{
			error = ex;
		}
		done = true;
	}
}
