using DevInterface;
using System.Text.RegularExpressions;
using UnityEngine;

using System.Reflection;
namespace Pom;

public static partial class Pom
{
	/// <summary>
	/// An interface for a <see cref="ManagedFieldWithPanel"/> that can be controlled through a <see cref="ManagedSlider"/>.
	/// </summary>
	public interface IInterpolablePanelField // sliders
	{
		float FactorOf(PositionedDevUINode node, ManagedData data);
		void NewFactor(PositionedDevUINode node, ManagedData data, float factor);
	}

	/// <summary>
	/// An interface for a <see cref="ManagedFieldWithPanel"/> that can be controlled through a <see cref="ManagedButton"/> or <see cref="ManagedArrowSelector"/>.
	/// </summary>
	public interface IIterablePanelField // buttons, arrows
	{
		void Next(PositionedDevUINode node, ManagedData data);
		void Prev(PositionedDevUINode node, ManagedData data);
	}

	/// <summary>
	/// A <see cref="ManagedField"/> that stores a <see cref="float"/> value.
	/// </summary>
	public class FloatField : ManagedFieldWithPanel, IInterpolablePanelField, IIterablePanelField
	{
		protected readonly float min;
		protected readonly float max;
		protected readonly float increment;

		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores a <see cref="float"/>. Can be used as an Attribute for a field in your data class derived from <see cref="ManagedData"/>.
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="min">the minimum allowed value</param>
		/// <param name="max">the maximum allowed value</param>
		/// <param name="defaultValue">the value a new data object is generated with</param>
		/// <param name="increment">controls digits when displaying the value, also behavior with buttons and arrows</param>
		/// <param name="control">the type of UI for this field</param>
		/// <param name="displayName">a display name for the panel, defaults to <paramref name="key"/></param>
		public FloatField(
			string key,
			float min,
			float max,
			float defaultValue,
			float increment = 0.1f,
			ControlType control = ControlType.slider,
			string? displayName = null) : base(
				key,
				Mathf.Clamp(defaultValue, min, max),
				control,
				displayName)
		{
			this.min = Math.Min(min, max);
			this.max = Math.Max(min, max);
			this.increment = increment;
		}

		public override object FromString(string str)
		{
			return Mathf.Clamp(float.Parse(str), min, max);
		}

		protected virtual int NumberOfDecimals()
		{
			// fix decimals from https://stackoverflow.com/a/30205131
			decimal dec = new decimal(increment);
			dec = Math.Abs(dec); //make sure it is positive.
			dec -= (int)dec;     //remove the integer part of the number.
			int decimalPlaces = 0;
			while (dec > 0)
			{
				decimalPlaces++;
				dec *= 10;
				dec -= (int)dec;
			}
			return decimalPlaces;
		}

		public override float SizeOfLargestDisplayValue()
		{
			return
			HUD.DialogBox.meanCharWidth
			* (Mathf.FloorToInt(Mathf.Max(Mathf.Abs(min), Mathf.Abs(max))).ToString().Length
			+ 2
			+ NumberOfDecimals());
		}

		public override string DisplayValueForNode(PositionedDevUINode node, ManagedData data)
		{
			//return base.DisplayValueForNode(node, data);
			// fix too many decimals
			return data.GetValue<float>(key).ToString("N" + NumberOfDecimals());
		}

		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual float FactorOf(
			PositionedDevUINode node,
			ManagedData data)
		{
			return ((max - min) == 0) ? 0f : (((float)data.GetValue<float>(key) - min) / (max - min));
		}
		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual void NewFactor(
			PositionedDevUINode node,
			ManagedData data,
			float factor)
		{
			data.SetValue<float>(key, min + factor * (max - min));
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Next(
			PositionedDevUINode node,
			ManagedData data)
		{
			float val = data.GetValue<float>(key) + increment;
			if (val > max) val = min;
			data.SetValue<float>(key, val);
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Prev(
			PositionedDevUINode node,
			ManagedData data)
		{
			float val = data.GetValue<float>(key) - increment;
			if (val < min) val = max;
			data.SetValue<float>(key, val);
		}

		public override void ParseFromText(
			PositionedDevUINode node,
			ManagedData data,
			string newValue)
		{
			float val = float.Parse(newValue);
			if (val < min || val > max) throw new ArgumentException();
			base.ParseFromText(node, data, newValue);
		}
	}

	/// <summary>
	/// A <see cref="ManagedField"/> that stores a <see cref="bool"/> value.
	/// </summary>
	public class BooleanField : ManagedFieldWithPanel, IIterablePanelField, IInterpolablePanelField
	{
		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores a <see cref="bool"/>. Can be used as an Attribute for a field in your data class derived from <see cref="ManagedData"/>.
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="defaultValue">the value a new data object is generated with</param>
		/// <param name="control">the type of UI for this field</param>
		/// <param name="displayName">a display name for the panel, defaults to <paramref name="key"/></param>
		public BooleanField(
			string key,
			bool defaultValue,
			ControlType control = ControlType.button,
			string? displayName = null) : base(
				key,
				defaultValue,
				control,
				displayName)
		{
		}

		public override object FromString(string str)
		{
			return bool.Parse(str);
		}

		public override float SizeOfLargestDisplayValue()
		{
			return HUD.DialogBox.meanCharWidth * 5;
		}
		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual float FactorOf(
			PositionedDevUINode node,
			ManagedData data)
		{
			return data.GetValue<bool>(key) ? 1f : 0f;
		}
		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual void NewFactor(
			PositionedDevUINode node,
			ManagedData data,
			float factor)
		{
			data.SetValue(key, factor > 0.5f);
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Next(
			PositionedDevUINode node,
			ManagedData data)
		{
			data.SetValue(key, !data.GetValue<bool>(key));
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Prev(
			PositionedDevUINode node,
			ManagedData data)
		{
			data.SetValue(key, !data.GetValue<bool>(key));
		}
	}

	/// <summary>
	/// A <see cref="ManagedField"/> that stores an <see cref="Enum"/> value.
	/// </summary>

	public class EnumField<E> : ManagedFieldWithPanel, IIterablePanelField, IInterpolablePanelField
		where E : struct, Enum
	{
		[EnumField<BindingFlags>("A", BindingFlags.IgnoreCase, new[] { BindingFlags.IgnoreCase })]
		protected E[] _possibleValues;
		protected int _c_enumlen = -1;
		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores an <see cref="Enum"/> of the specified type.
		/// Can be used as Attribute, thanks generics!
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="defaultValue">the value a new data object is generated with</param>
		/// <param name="possibleValues">the acceptable values for this field, defaults to a deferred call to <see cref="Enum.GetValues"/> for the type</param>
		/// <param name="control">the type of UI for this field</param>
		/// <param name="displayName">a display name for the panel, defaults to <paramref name="key"/></param>
		public EnumField(string key, E defaultValue, E[]? possibleValues = null, ControlType control = ControlType.arrows, string? displayName = null) : base(key, (possibleValues != null && !possibleValues.Contains(defaultValue)) ? possibleValues[0] : defaultValue, control, displayName)
		{
			//this.type = typeof(E);
			//this.type = type;
			this._possibleValues = possibleValues!;
		}

		protected virtual E[] PossibleValues // We defer this listing so enumextend can do its magic.
		{
			get
			{

				var entries = Enum.GetValues(typeof(E));
				if (_possibleValues is null || entries.Length != _c_enumlen) _possibleValues = Enum.GetValues(typeof(E)).Cast<E>().ToArray();
				return _possibleValues!;
			}
		}


		public override object FromString(string str)
		{
			if (Enum.TryParse<E>(str, out E fromstring))
				return PossibleValues.Contains(fromstring) ? fromstring : PossibleValues[0];
			return default!;
		}

		public override float SizeOfLargestDisplayValue()
		{
			int longestEnum = PossibleValues.Aggregate<E, int>(0, (longest, next) =>
					Mathf.Max(longest, next.ToString().Length));
			return HUD.DialogBox.meanCharWidth * longestEnum + 2;
		}
		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual float FactorOf(PositionedDevUINode node, ManagedData data)
		{
			return (float)Array.IndexOf(PossibleValues, data.GetValue<E>(key)) / (float)(PossibleValues.Length - 1);
		}
		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual void NewFactor(PositionedDevUINode node, ManagedData data, float factor)
		{
			data.SetValue<Enum>(key, PossibleValues[Mathf.RoundToInt(factor * (PossibleValues.Length - 1))]);
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Next(PositionedDevUINode node, ManagedData data)
		{
			data.SetValue<Enum>(key, PossibleValues[(Array.IndexOf(PossibleValues, data.GetValue<Enum>(key)) + 1) % PossibleValues.Length]);
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Prev(PositionedDevUINode node, ManagedData data)
		{
			data.SetValue<Enum>(key, PossibleValues[(Array.IndexOf(PossibleValues, data.GetValue<Enum>(key)) - 1 + PossibleValues.Length) % PossibleValues.Length]);
		}

		public override void ParseFromText(PositionedDevUINode node, ManagedData data, string newValue)
		{
			E fromstring = default;
			try
			{
				fromstring = (E)Enum.Parse(typeof(E), newValue);
			}
			catch (Exception)
			{
				foreach (Enum val in PossibleValues)
				{
					if (val.ToString().ToLowerInvariant() == newValue.ToLowerInvariant())
					{
						// This check is flawed if we have for instance "aa" and "AAA" and "aaa" it becomes impossible to type in the second one
						// But honestly who would name their enums like that...
						data.SetValue(key, val);
						return;
					}
				}
			}
			if (!PossibleValues.Contains(fromstring)) throw new ArgumentException();

			data.SetValue(key, fromstring);
			//base.ParseFromText(node, data, newValue);
		}
	}

	/// <summary>
	/// A <see cref="ManagedField"/> that stores an <see cref="ExtEnumBase"/> value.
	/// </summary>
	public class ExtEnumField<XE> : ManagedFieldWithPanel, IIterablePanelField, IInterpolablePanelField
		where XE : ExtEnum<XE>
	{
		//[ExtEnumField("A", typeof(Ext), BindingFlags.DeclaredOnly, new Enum[] {BindingFlags.CreateInstance})]
		protected readonly ExtEnumType type;

		[ExtEnumField<AbstractPhysicalObject.AbstractObjectType>("A", "", null)]
		protected XE[] _possibleValues;
		protected int _c_enumver = -1;
		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores an <see cref="ExtEnumBase"/> of the specified type.
		/// Cannot be used as Attribute, instead you should pass this object to <see cref="ManagedData.ManagedData(PlacedObject, ManagedField[])"/> and mark your field with the <see cref="ManagedData.BackedByField"/> attribute.
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="defaultValue">the value a new data object is generated with</param>
		/// <param name="possibleValues">the acceptable values for this field, defaults to a deferred call to <see cref="Enum.GetValues"/> for the type</param>
		/// <param name="control">the type of UI for this field</param>
		/// <param name="displayName">a display name for the panel, defaults to <paramref name="key"/></param>
		public ExtEnumField(
			string key,
			XE defaultValue,
			XE[]? possibleValues = null,
			ControlType control = ControlType.arrows,
			string? displayName = null) : base(
				key,
				(possibleValues?.Contains(defaultValue) ?? false) ? possibleValues[0] : defaultValue,
				control,
				displayName)
		{
			this.type = ExtEnumBase.GetExtEnumType(typeof(XE));
			this._possibleValues = possibleValues!;
		}
		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores an <see cref="ExtEnumBase"/> of the specified type.
		/// Can be used as an attribute! Make sure to pass the correct string values, otherwise
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="defaultValue">the value a new data object is generated with</param>
		/// <param name="possibleValues">the acceptable values for this field, defaults to a deferred call to <see cref="Enum.GetValues"/> for the type</param>
		/// <param name="control">the type of UI for this field</param>
		/// <param name="displayName">a display name for the panel, defaults to <paramref name="key"/></param>
		public ExtEnumField(
			string key,
			string defaultValue,
			string[]? possibleValues = null,
			ControlType control = ControlType.arrows,
			string? displayName = null) : this(
				key,
				(XE)ExtEnumBase.Parse(typeof(XE), defaultValue, false),
				possibleValues.Select(x => (XE)(ExtEnumBase.Parse(typeof(XE), x, false))).ToArray(),
				control,
				displayName
			)
		{

		}

		protected virtual XE[] PossibleValues
		{
			get
			{
				var entries = ExtEnumBase.GetExtEnumType(typeof(XE)).entries;
				if (_c_enumver != ExtEnum<XE>.valuesVersion)
				{
					_possibleValues = entries.Select(entry => (XE)ExtEnumBase.Parse(typeof(XE), entry, false)).ToArray();
				}
				_c_enumver = ExtEnum<XE>.valuesVersion;
				return _possibleValues;
			}
		}

		public override object FromString(string str)
		{
			ExtEnumBase.TryParse(typeof(XE), str, false, out ExtEnumBase res);
			return PossibleValues.Contains(res) ? res : PossibleValues[0];
		}

		public override float SizeOfLargestDisplayValue()
		{
			int longestExtEnumBase = PossibleValues.Aggregate<ExtEnumBase, int>(0, (longest, next) =>
					Mathf.Max(next.value.Length, longest));
			return HUD.DialogBox.meanCharWidth * longestExtEnumBase + 2;
		}
		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual float FactorOf(
			PositionedDevUINode node,
			ManagedData data)
		{
			return (float)Array.IndexOf(PossibleValues, data.GetValue<ExtEnumBase>(key)) / (float)(PossibleValues.Length - 1);
		}
		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual void NewFactor(
			PositionedDevUINode node,
			ManagedData data,
			float factor)
		{
			data.SetValue<ExtEnumBase>(key, PossibleValues[Mathf.RoundToInt(factor * (PossibleValues.Length - 1))]);
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Next(
			PositionedDevUINode node,
			ManagedData data)
		{
			data.SetValue<ExtEnumBase>(key, PossibleValues[(Array.IndexOf(PossibleValues, data.GetValue<ExtEnumBase>(key)) + 1) % PossibleValues.Length]);
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Prev(
			PositionedDevUINode node,
			ManagedData data)
		{
			data.SetValue<ExtEnumBase>(key, PossibleValues[(Array.IndexOf(PossibleValues, data.GetValue<ExtEnumBase>(key)) - 1 + PossibleValues.Length) % PossibleValues.Length]);
		}

		public override void ParseFromText(
			PositionedDevUINode node,
			ManagedData data,
			string newValue)
		{
			if (!ExtEnumBase.TryParse(typeof(XE), newValue, false, out ExtEnumBase o_nv))
			{
				foreach (XE val in PossibleValues)
				{
					if (val.ToString().ToLowerInvariant() == newValue.ToLowerInvariant())
					{
						// This check is flawed if we have for instance "aa" and "AAA" and "aaa" it becomes impossible to type in the second one
						// But honestly who would name their ExtEnumBases like that...
						data.SetValue(key, val);
						return;
					}
				}
				throw new ArgumentException($"Cannot parse {newValue} as {typeof(XE).FullName}");
			}
			XE fromstring = (XE)o_nv;
			if (!PossibleValues.Contains(fromstring)) throw new ArgumentException($"Value {fromstring} not present in PossibleValues");
			data.SetValue(key, fromstring);
			//base.ParseFromText(node, data, newValue);
		}
	}

	/// <summary>
	/// A <see cref="ManagedField"/> that stores an <see cref="int"/> value.
	/// </summary>
	public class IntegerField : ManagedFieldWithPanel, IIterablePanelField, IInterpolablePanelField
	{
		protected readonly int min;
		protected readonly int max;
		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores an <see cref="int"/>. Can be used as an Attribute for a field in your data class derived from <see cref="ManagedData"/>.
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="min">the minimum allowed value (inclusive)</param>
		/// <param name="max">the maximum allowed value (inclusive)</param>
		/// <param name="defaultValue">the value a new data object is generated with</param>
		/// <param name="control">the type of UI for this field</param>
		/// <param name="displayName">a display name for the panel, defaults to <paramref name="key"/></param>
		public IntegerField(
			string key,
			int min,
			int max,
			int defaultValue,
			ControlType control = ControlType.arrows,
			string? displayName = null) : base(
				key,
				Mathf.Clamp(defaultValue, min, max),
				control,
				displayName)
		{
			this.min = Math.Min(min, max); // trust nobody
			this.max = Math.Max(min, max);
		}

		public override object FromString(string str)
		{
			return Mathf.Clamp(int.Parse(str), min, max);
		}

		public override float SizeOfLargestDisplayValue()
		{
			return HUD.DialogBox.meanCharWidth * ((Mathf.Max(Mathf.Abs(min), Mathf.Abs(max))).ToString().Length + 2);
		}
		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual float FactorOf(
			PositionedDevUINode node,
			ManagedData data)
		{
			return (max - min == 0) ? 0f : (data.GetValue<int>(key) - min) / (float)(max - min);
		}
		/// <summary>
		/// Implements <see cref="IInterpolablePanelField"/>. Called from UI sliders.
		/// </summary>
		public virtual void NewFactor(
			PositionedDevUINode node,
			ManagedData data,
			float factor)
		{
			data.SetValue<int>(key, Mathf.RoundToInt(min + factor * (max - min)));
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Next(
			PositionedDevUINode node,
			ManagedData data)
		{
			int val = data.GetValue<int>(key) + 1;
			if (val > max) val = min;
			data.SetValue<int>(key, val);
		}
		/// <summary>
		/// Implements <see cref="IIterablePanelField"/>. Called from UI buttons and arrows.
		/// </summary>
		public virtual void Prev(
			PositionedDevUINode node,
			ManagedData data)
		{
			int val = data.GetValue<int>(key) - 1;
			if (val < min) val = max;
			data.SetValue<int>(key, val);
		}

		public override void ParseFromText(
			PositionedDevUINode node,
			ManagedData data,
			string newValue)
		{
			int val = int.Parse(newValue);
			if (val < min || val > max) throw new ArgumentException();
			base.ParseFromText(node, data, newValue);
		}
	}

	/// <summary>
	/// A <see cref="ManagedField"/> that stores a <see cref="string"/> value.
	/// </summary>
	public class StringField : ManagedFieldWithPanel
	{
		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores a <see cref="string"/>. Can be used as an Attribute for a field in your data class derived from <see cref="ManagedData"/>.
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="defaultValue">the value a new data object is generated with</param>
		/// <param name="displayName">a display name for the panel, defaults to <paramref name="key"/></param>
		public StringField(
			string key,
			string defaultValue,
			string? displayName = null) : base(
				key,
				defaultValue,
				ControlType.text,
				displayName)
		{

		}

		protected readonly static List<KeyValuePair<string, string>> replacements = new()
		{
			new(": ","%1"),
			new( ", ","%2" ),
			new("><","%3"),
			new("~","%4"),
			new("%","%0"), // this goes last, very important
		};

		public override object FromString(string str)
		{
			//return str[0];
			return replacements.Aggregate(str, (current, value) =>
				current.Replace(value.Value, value.Key));
		}

		public override string ToString(object value)
		{
			//return new string[] { value.ToString() };
			return System.Linq.Enumerable.Reverse(replacements).Aggregate
				(
					value.ToString(),
					(current, val) => current.Replace(val.Key, val.Value)
				);
		}

		public override float SizeOfLargestDisplayValue()
		{
			return HUD.DialogBox.meanCharWidth * 25; // No character limit but this is the expected reasonable max anyone would be using ?
		}

		public override string? DisplayValueForNode(
			PositionedDevUINode node,
			ManagedData data) // bypass replacements
		{
			return data.GetValue<string>(key);
		}

		public override void ParseFromText(
			PositionedDevUINode node,
			ManagedData data,
			string newValue) // no replacements
		{
			data.SetValue(key, newValue);
		}
	}

	/// <summary>
	/// A <see cref="ManagedField"/> for a <see cref="Vector2"/> value.
	/// </summary>
	public class Vector2Field : ManagedField
	{
		protected readonly VectorReprType controlType;
		protected readonly string label;

		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores a <see cref="Vector2"/>.
		/// Cannot be used as Attribute, instead you should pass this object to <see cref="ManagedData.ManagedData(PlacedObject, ManagedField[])"/> and mark your field with the <see cref="ManagedData.BackedByField"/> attribute.
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="defaultValue">the value a new data object is generated with</param>
		/// <param name="controlType">the type of UI for this field, from <see cref="Vector2Field.VectorReprType"/></param>
		public Vector2Field(
			string key,
			Vector2 defaultValue,
			VectorReprType controlType = VectorReprType.line,
			string? label = null) : base(
				key,
				defaultValue)
		{
			this.controlType = controlType;
			this.label = label ?? "";
		}

		public Vector2Field(
			string key,
			float defX,
			float defY,
			VectorReprType ct = VectorReprType.line,
			string? label = null) : this(
				  key,
				  new Vector2(defX, defY),
				  ct,
				  label)
		{

		}
		public enum VectorReprType
		{
			none,
			line,
			circle,
			rect,
		}

		public override object FromString(string str)
		{
			string[] arr = Regex.Split(str, "\\^");
			return new Vector2(float.Parse(arr[0]), float.Parse(arr[1]));
		}

		public override string ToString(object value)
		{
			Vector2 vec = (Vector2)value;
			return string.Join("^", new string[] { vec.x.ToString(), vec.y.ToString() });
		}

		public override DevUINode? MakeAditionalNodes(ManagedData managedData, ManagedRepresentation managedRepresentation)
		{
			return new ManagedVectorHandle(this, managedData, managedRepresentation, controlType);
		}
	}

	/// <summary>
	/// A <see cref="ManagedField"/> for a <see cref="RWCustom.IntVector2"/> value.
	/// </summary>
	public class IntVector2Field : ManagedField
	{
		protected readonly IntVectorReprType controlType;
		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores a <see cref="RWCustom.IntVector2"/>.
		/// Cannot be used as Attribute, use the other constructor instead you should pass this object to <see cref="ManagedData.ManagedData(PlacedObject, ManagedField[])"/> and mark your field with the <see cref="ManagedData.BackedByField"/> attribute.
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="defaultValue">the value a new data object is generated with</param>
		/// <param name="controlType">the type of UI for this field, from <see cref="IntVector2Field.IntVectorReprType"/></param>
		public IntVector2Field(
			string key,
			RWCustom.IntVector2 defaultValue,
			IntVectorReprType controlType = IntVectorReprType.line) : base(
				key,
				defaultValue)
		{
			this.controlType = controlType;
		}
		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores a <see cref="RWCustom.IntVector2"/>.
		/// Cannot be used as Attribute, use the other constructor instead you should pass this object to <see cref="ManagedData.ManagedData(PlacedObject, ManagedField[])"/> and mark your field with the <see cref="ManagedData.BackedByField"/> attribute.
		/// </summary>
		/// <param name="key">The key to access field with</param>
		/// <param name="defX">Default X value</param>
		/// <param name="defY">Default Y value</param>
		/// <param name="controlType">The way control should be represented</param>
		public IntVector2Field(
			string key,
			int defX,
			int defY,
			IntVectorReprType controlType = IntVectorReprType.line) : this(
				key,
				new RWCustom.IntVector2(defX, defY),
				controlType)
		{

		}
		///<summary>
		///Different ways an IntVector2 managedfield can be represented. For all options, the value scale is 1 per tile (20px) 
		///</summary>
		public enum IntVectorReprType
		{
			///<summary>
			///No representation
			///</summary>
			none,
			///<summary>
			///A line, same as Vector2Field
			///</summary>
			line,
			///<summary>
			///The targeted tile will be highlighted
			///</summary>
			tile,
			///<summary>
			///An arrow that can point in 4 cardinal directions
			///</summary>
			fourdir,
			///<summary>
			///An arrow that can point in 8 directions (cardinal and diagonal)
			///</summary>
			eightdir,
			///<summary>
			///A rectangle spanning between main handle and intvector2 handle
			///</summary>
			rect,
		}

		public override object FromString(string str)
		{
			string[] arr = Regex.Split(str, "\\^");
			return new RWCustom.IntVector2(int.Parse(arr[0]), int.Parse(arr[1]));
		}

		public override string ToString(object value)
		{
			RWCustom.IntVector2 vec = (RWCustom.IntVector2)value;
			return string.Join("^", new string[] { vec.x.ToString(), vec.y.ToString() });
		}

		public override DevUINode MakeAditionalNodes(
			ManagedData managedData,
			ManagedRepresentation managedRepresentation)
		{
			return new ManagedIntHandle(this, managedData, managedRepresentation, controlType);
		}
	}

	/// <summary>
	/// A <see cref="ManagedField"/> for a <see cref="UnityEngine.Color"/> value.
	/// </summary>
	public class ColorField : ManagedFieldWithPanel, IInterpolablePanelField
	{
		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores a <see cref="UnityEngine.Color"/>.
		/// Cannot be used as Attribute, use the other constructor, instead you should pass this object to <see cref="ManagedData.ManagedData(PlacedObject, ManagedField[])"/> and mark your field with the <see cref="ManagedData.BackedByField"/> attribute.
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="defaultColor">the value a new data object is generated with</param>
		/// <param name="controlType">one of <see cref="ManagedFieldWithPanel.ControlType.text"/> or <see cref="ManagedFieldWithPanel.ControlType.slider"/></param>
		public ColorField(
			string key,
			Color defaultColor,
			ControlType controlType = ControlType.text,
			string? displayName = null) : base(
				key,
				defaultColor,
				controlType,
				displayName)
		{


		}

		/// <summary>
		/// Creates a <see cref="ManagedField"/> that stores a <see cref="UnityEngine.Color"/>.
		/// Can be used as an attribute
		/// </summary>
		/// <param name="key">The key to access that field with</param>
		/// <param name="defaultColor">the value a new data object is generated with</param>
		/// <param name="controlType">one of <see cref="ManagedFieldWithPanel.ControlType.text"/> or <see cref="ManagedFieldWithPanel.ControlType.slider"/></param>
		public ColorField(
			string key,
			float defR,
			float defG,
			float defB,
			float defA = 1f,
			ControlType ct = ControlType.text,
			string? DisplayName = null) : this(
				key,
				new Color(defR, defG, defB, defA),
				ct,
				DisplayName)
		{ }

		public override object FromString(string str)
		{
			return new Color(
					Convert.ToInt32(str.Substring(0, 2), 16) / 255f,
					Convert.ToInt32(str.Substring(2, 2), 16) / 255f,
					Convert.ToInt32(str.Substring(4, 2), 16) / 255f);
		}

		public override string ToString(object value)
		{
			Color color = (Color)value;
			return string.Join("", new string[] {Mathf.RoundToInt(color.r * 255).ToString("X2"),
					Mathf.RoundToInt(color.g * 255).ToString("X2"),
					Mathf.RoundToInt(color.b * 255).ToString("X2")});
		}

		public override void ParseFromText(
			PositionedDevUINode node,
			ManagedData data,
			string newValue)
		{
			if (newValue.StartsWith("#")) newValue = newValue.Substring(1);
			if (newValue.Length != 6) throw new ArgumentException();
			data.SetValue(key, this.FromString(newValue));
		}

		public override string? DisplayValueForNode(
			PositionedDevUINode node,
			ManagedData data)
		{
			switch (this.control)
			{
			case ControlType.slider:
				Color color = data.GetValue<Color>(key);
				ColorSliderControl control = (ColorSliderControl)node.parentNode;
				if (node == control.rslider) return Mathf.RoundToInt(color.r * 255).ToString();
				if (node == control.gslider) return Mathf.RoundToInt(color.g * 255).ToString();
				return Mathf.RoundToInt(color.b * 255).ToString();
			case ControlType.text:
				return "#" + ToString(data.GetValue<object>(key) ?? "FFFFFF");
			default:
				return null;
			}
		}
		public override float SizeOfLargestDisplayValue()
		{
			switch (this.control)
			{
			case ControlType.slider:
				return HUD.DialogBox.meanCharWidth * (4); ;
			case ControlType.text:
				return HUD.DialogBox.meanCharWidth * (9); ;
			default:
				return 0;
			}
		}

		public override float SizeOfDisplayname()
		{
			switch (this.control)
			{
			case ControlType.slider:
				return HUD.DialogBox.meanCharWidth * (displayName.Length + 6);
			default:
				return base.SizeOfDisplayname();
			}
		}

		public override Vector2 SizeOfPanelUiMinusName()
		{
			switch (control)
			{
			case ControlType.slider:
				Vector2 size = base.SizeOfPanelUiMinusName();
				size.y = 60;
				return size;
			default:
				return base.SizeOfPanelUiMinusName();
			}

		}

		public override PositionedDevUINode? MakeControlPanelNode(
			ManagedData managedData,
			ManagedControlPanel panel,
			float sizeOfDisplayname)
		{
			switch (control)
			{
			case ControlType.slider:
				return new ColorSliderControl(this, managedData, panel, sizeOfDisplayname);
			case ControlType.text:
				return base.MakeControlPanelNode(managedData, panel, sizeOfDisplayname);
			case ControlType.arrows:
			case ControlType.button:
				throw new NotImplementedException();
			default:
				break;
			}
			return null;
		}
		public float FactorOf(
			PositionedDevUINode node,
			ManagedData data)
		{
			Color color = data.GetValue<Color>(key);
			ColorSliderControl control = (ColorSliderControl)node.parentNode;
			if (node == control.rslider) return color.r;
			if (node == control.gslider) return color.g;
			return color.b;

		}

		public void NewFactor(
			PositionedDevUINode node,
			ManagedData data,
			float factor)
		{
			Color color = data.GetValue<Color>(key);
			ColorSliderControl control = (ColorSliderControl)node.parentNode;
			if (node == control.rslider) color.r = factor;
			else if (node == control.gslider) color.g = factor;
			else color.b = factor;
			data.SetValue<Color>(key, color);
		}

		private class ColorSliderControl : PositionedDevUINode
		{
			public ManagedSlider rslider;
			public ManagedSlider gslider;
			public ManagedSlider bslider;

			public ColorSliderControl(
				ColorField field,
				ManagedData managedData,
				DevUINode parent,
				float sizeOfDisplayname) : base(
					parent.owner,
					field.key,
					parent,
					Vector2.zero)
			{
				this.rslider = new ManagedSlider(field, managedData, this, sizeOfDisplayname);
				rslider.pos = new(0, 40);
				(rslider.subNodes[0] as DevUILabel)!.Text += " - R";
				this.subNodes.Add(rslider);
				this.gslider = new ManagedSlider(field, managedData, this, sizeOfDisplayname);
				(gslider.subNodes[0] as DevUILabel)!.Text += " - G";
				gslider.pos = new(0, 20);
				this.subNodes.Add(gslider);
				this.bslider = new ManagedSlider(field, managedData, this, sizeOfDisplayname);
				(bslider.subNodes[0] as DevUILabel)!.Text += " - B";
				this.subNodes.Add(bslider);
			}
		}
	}

	public class DrivenVector2Field : Vector2Field
	{
		private readonly string keyOfOther;
		private readonly DrivenControlType drivenControlType;

		public enum DrivenControlType
		{
			relativeLine,
			perpendicularLine,
			perpendicularOval,
			rectangle,
		}

		public DrivenVector2Field(
			string keyofSelf,
			string keyOfOther,
			Vector2 defaultValue,
			DrivenControlType controlType = DrivenControlType.perpendicularLine,
			string? label = null) : base(
				keyofSelf,
				defaultValue,
				VectorReprType.none,
				label)
		{
			this.keyOfOther = keyOfOther;
			this.drivenControlType = controlType;
		}

		public override DevUINode? MakeAditionalNodes(ManagedData managedData, ManagedRepresentation managedRepresentation)
		{
			switch (drivenControlType)
			{
			case DrivenControlType.relativeLine:
				return new DrivenVectorControl(this, managedData, (managedRepresentation.managedNodes[keyOfOther] as PositionedDevUINode)!, drivenControlType, label);
			case DrivenControlType.perpendicularLine:
			case DrivenControlType.perpendicularOval:
			case DrivenControlType.rectangle:
				return new DrivenVectorControl(this, managedData, managedRepresentation, drivenControlType, label);
			default:
				return null;
			}
		}

		public class DrivenVectorControl : PositionedDevUINode
		{
			protected readonly DrivenVector2Field control;
			protected readonly ManagedData data;
			protected readonly DrivenControlType controlType;
			protected Handle handleB;
			protected FSprite? circleSprite;
			protected FSprite? lineBSprite;
			private int[]? rect;

			public DrivenVectorControl(DrivenVector2Field control,
						   ManagedData data,
						   PositionedDevUINode repr,
						   DrivenControlType controlType,
						   string label) : base(repr.owner, control.key, repr, Vector2.zero)
			{
				circleSprite = null;
				lineBSprite = null;
				this.control = control;
				this.data = data;
				this.controlType = controlType;

				handleB = new Handle(owner, "V_Handle", this, new(100f, 0f));
				handleB.subNodes.Add(new DevUILabel(owner, "hbl", handleB, new(-3.5f, -7.5f), 16, label) { spriteColor = Color.clear });
				this.subNodes.Add(handleB);

				this.handleB.pos = data.GetValue<Vector2>(control.key);

				switch (controlType)
				{
				case DrivenControlType.perpendicularLine:
				case DrivenControlType.relativeLine:
					this.fSprites.Add(this.lineBSprite = new FSprite("pixel", true) { anchorY = 0f });
					owner.placedObjectsContainer.AddChild(this.lineBSprite);
					break;
				case DrivenControlType.perpendicularOval:
					this.fSprites.Add(this.circleSprite = new FSprite("Futile_White", true)
					{
						shader = owner.room.game.rainWorld.Shaders["VectorCircle"],
						alpha = 0.02f
					});
					owner.placedObjectsContainer.AddChild(this.circleSprite);
					break;
				case DrivenControlType.rectangle:
					this.rect = new int[5];
					for (int i = 0; i < 5; i++)
					{
						this.rect[i] = this.fSprites.Count;
						this.fSprites.Add(new FSprite("pixel", true));
						owner.placedObjectsContainer.AddChild(this.fSprites[rect[i]]);
						this.fSprites[rect[i]].anchorX = 0f;
						this.fSprites[rect[i]].anchorY = 0f;
					}
					this.fSprites[rect[4]].alpha = 0.05f;
					break;

				default:
					break;
				}
			}

			public override void Refresh()
			{
				base.Refresh();
				Vector2 drivingPos = data.GetValue<Vector2>(control.keyOfOther);
				switch (controlType)
				{
				case DrivenControlType.relativeLine:
					// ??? nothing to do here
					break;
				case DrivenControlType.perpendicularLine:
				case DrivenControlType.perpendicularOval:
				case DrivenControlType.rectangle:
					Vector2 perp = RWCustom.Custom.PerpendicularVector(drivingPos);
					handleB.pos = perp * handleB.pos.magnitude;// * handleB.pos.magnitude;
					break;
				}
				switch (controlType)
				{
				case DrivenControlType.perpendicularLine:
				case DrivenControlType.relativeLine:
					lineBSprite!.SetPosition(absPos);
					lineBSprite.scaleY = handleB.pos.magnitude;
					lineBSprite.rotation = RWCustom.Custom.VecToDeg(handleB.pos);
					break;
				case DrivenControlType.perpendicularOval:
					circleSprite!.SetPosition(absPos);
					circleSprite.scaleY = drivingPos.magnitude / 8f;
					circleSprite.scaleX = handleB.pos.magnitude / 8f;
					circleSprite.rotation = RWCustom.Custom.VecToDeg(drivingPos);
					break;
				case DrivenControlType.rectangle:
					Vector2 leftbottom;// = Vector2.zero;
					Vector2 topright;// = Vector2.zero;
					Vector2 bottomright;
					Vector2 topleft;

					leftbottom = (parentNode as PositionedDevUINode)!.absPos;
					bottomright = leftbottom + drivingPos;
					topleft = handleB.absPos;
					topright = leftbottom + drivingPos + handleB.pos;//absPos;
																	 //Vector2 size = (topright - leftbottom);

					base.MoveSprite(rect![0], leftbottom);
					this.fSprites[rect[0]].scaleY = drivingPos.magnitude;
					this.fSprites[rect[0]].rotation = RWCustom.Custom.VecToDeg(drivingPos);
					base.MoveSprite(rect[1], leftbottom);
					this.fSprites[rect[1]].scaleY = handleB.pos.magnitude;
					this.fSprites[rect[1]].rotation = RWCustom.Custom.VecToDeg(handleB.pos);
					base.MoveSprite(rect[2], (topright));
					this.fSprites[rect[2]].scaleY = drivingPos.magnitude;
					this.fSprites[rect[2]].rotation = RWCustom.Custom.VecToDeg(drivingPos) + 180f;
					base.MoveSprite(rect[3], (topright));
					this.fSprites[rect[3]].scaleY = handleB.pos.magnitude;
					this.fSprites[rect[3]].rotation = RWCustom.Custom.VecToDeg(handleB.pos) + 180f;
					base.MoveSprite(rect[4], leftbottom);
					this.fSprites[rect[4]].scaleX = drivingPos.magnitude;
					this.fSprites[rect[4]].scaleY = handleB.pos.magnitude;
					this.fSprites[rect[4]].rotation = RWCustom.Custom.VecToDeg(handleB.pos);
					break;
				default:
					break;
				}
				data.SetValue<Vector2>(control.key, handleB.pos);
			}
		}
	}

}
