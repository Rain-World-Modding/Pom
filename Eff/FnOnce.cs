namespace Eff;
/// <summary>
/// Contains a callback that is only allowed to be raised once
/// </summary>
public sealed class FnOnce
{
	private object sync = new();
	private bool done = false;
	/// <summary>
	/// Error from invokation, if there is one
	/// </summary>
	/// <value></value>
	public Exception? error { get; private set; } = null;
	private Action action;
	/// <summary>
	/// Creates a new instance.
	/// </summary>
	public FnOnce(Action action)
	{
		this.action = action;
	}
	/// <summary>
	/// Attempts to invoke the contained callback. Only works once.
	/// </summary>
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
