namespace EffExt;

/// <summary>
/// Type of data in the effect field.
/// </summary>
public enum DataType
{
	/// <summary>
	/// Data is an integer
	/// </summary>
	Int,
	/// <summary>
	/// Data is a float
	/// </summary>
	Float,
	/// <summary>
	/// Data is a boolean
	/// </summary>
	Bool,
	/// <summary>
	/// Data is a string
	/// </summary>
	String,
	/// <summary>
	/// Data is unknown
	/// </summary>
	Unknown = -1
}
