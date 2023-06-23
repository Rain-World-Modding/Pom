namespace Pom.Eff;

public sealed class FnOnce
{
	private object sync = new();
	private bool done = false;
	public Exception? error { get; private set; } = null;
	private Action action;
	public FnOnce(Action action)
	{
		this.action = action;
	}
	public void Invoke()
	{
		lock (sync)
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
}
