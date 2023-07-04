namespace Eff;

public class Cached<T> where T : IEquatable<T>
{
	public T val
	{
		set
		{
			LastError = null;
			if (!Object.Equals(value, oldval))
			{
				try
				{
					OnValueChange?.Invoke(value);
				}
				catch (Exception ex)
				{
					LastError = ex;
				}
			}
			oldval = currentval;
			currentval = value;
		}
		get => currentval;
	}
	private T currentval;
	private T oldval;
	public Exception? LastError { get; private set; } = null;

	public Cached(T value, Action<T> onValueChange)
	{
		this.oldval = value;
		this.currentval = value;
		OnValueChange = onValueChange;
	}

	public Action<T>? OnValueChange { get; set; }
}

#pragma warning restore 1591