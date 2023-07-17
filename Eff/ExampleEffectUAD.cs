using UnityEngine;

namespace Eff;

internal class ExampleEffectUAD : UpdatableAndDeletable, IDrawable
{
	private FnOnce init;
	private Cached<float> acceleration_intensity;
	private Vector2 pos;
	private Vector2 vel;
	private Vector2 acceleration;
	private Vector2 screenspace;
	public EffectExtraData EffectData { get; }
	public ExampleEffectUAD(EffectExtraData effectData)
	{
		acceleration_intensity = effectData._floats["floatfield"].valueCache;
		init = new(() =>
		{
			plog.LogWarning($"Example effect go in room {room.abstractRoom.name}");
		});
		EffectData = effectData;
	}

	public override void Update(bool eu)
	{
		init.Invoke();
		base.Update(eu);
		acceleration = new Vector2(UnityEngine.Random.value * 2f - 1, UnityEngine.Random.value * 2f - 1) * acceleration_intensity.val;
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