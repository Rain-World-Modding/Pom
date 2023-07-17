namespace Eff;

/// <summary>
/// Contains a value (<see cref="val"/>) and a callback (<see cref="OnValueChange"/>).
/// The callback is raised when <see cref="val"/> is changed to a NEW value.
/// Contained value must be <see cref="IEquatable{T}"/>.
/// </summary>
/// <typeparam name="T">Type of contained value.</typeparam>
public class Cached<T> where T : IEquatable<T>
{
	/// <summary>
	/// Contained value. Setter invokes the <see cref="OnValueChange"/> when new value is different from previous one.
	/// </summary>
	/// <value></value>
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
	/// <summary>
	/// Exception envountered last time there was an attempt to invoke setter <see cref="OnValueChange"/>, if there was one.
	/// </summary>
	/// <value></value>
	public Exception? LastError { get; private set; } = null;

	/// <summary>
	/// Creates a new instance.
	/// </summary>
	/// <param name="value">Starting value</param>
	/// <param name="onValueChange">Action to take when value changes.</param>
	public Cached(T value, Action<T> onValueChange)
	{
		this.oldval = value;
		this.currentval = value;
		OnValueChange = onValueChange;
	}
	/// <summary>
	/// The callback that is called each time contained value is changed.
	/// </summary>
	/// <value></value>
	public Action<T> OnValueChange { get; private set; }
}

#pragma warning restore 1591