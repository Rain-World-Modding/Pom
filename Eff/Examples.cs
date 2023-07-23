using UnityEngine;

namespace Eff;

internal static class Examples
{
	internal static void __RegisterExamples()
	{
		try
		{
			new EffectDefinitionBuilder("testeffect")
				.AddIntField("intfield", 0, 10, 5)
				.AddFloatField("floatfield", 0f, 10f, 0.1f, 1f)
				.AddBoolField("boolfield", true)
				.AddStringField("stringfield", "example_string%-%")
				.SetUADFactory((room, data, firstTimeRealized) => new ExampleEffectUAD(data))
				.SetCategory("POMEffects Examples")
				.Register();
		}
		catch (Exception ex)
		{

		}
	}
}

internal class ExampleEffectUAD : UpdatableAndDeletable, IDrawable
{
	//private FnOnce init;
	private bool setupRan;
	//private Cached<float> acceleration_intensity;
	private Vector2 pos;
	private Vector2 vel;
	private Vector2 acceleration;
	private Vector2 screenspace;
	public EffectExtraData EffectData { get; }
	public ExampleEffectUAD(EffectExtraData effectData)
	{
		//acceleration_intensity = effectData._floats["floatfield"].valueCache;
		EffectData = effectData;
	}

	public override void Update(bool eu)
	{
		if (!setupRan)
		{
			LogWarning($"Example effect go in room {room.abstractRoom.name}");
			setupRan = true;
		}
		base.Update(eu);
		float acceleration_intensity = this.EffectData.GetFloat("floatfield");
		acceleration = new Vector2(UnityEngine.Random.value * 2f - 1, UnityEngine.Random.value * 2f - 1) * acceleration_intensity;
		vel += acceleration;
		pos += vel;
		Rect bounds = new(new(), screenspace);
		if (!bounds.Contains(pos))
		{
			Reset(screenspace);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];

		Reset(rCam.sSize);
		sLeaser.sprites[0] = new FSprite("FoodCircleA")
		{
			color = Color.red,
			scale = 2f
		};
		AddToContainer(sLeaser, rCam, null);
	}

	private void Reset(Vector2 sSize)
	{
		vel = new();
		screenspace = sSize;
		pos = screenspace / 2f;
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].SetPosition(pos);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{

	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
	{
		newContatiner ??= rCam.ReturnFContainer("Foreground");
		sLeaser.sprites[0].RemoveFromContainer();
		newContatiner.AddChild(sLeaser.sprites[0]);
	}
}