using UnityEngine;

namespace Eff;

internal static class Examples
{
	internal static void __RegisterExamples()
	{
		try
		{
			new EffectDefinitionBuilder("testeffect")
				.AddIntField("intfield", 0, 10, 5, "An integer")
				.AddFloatField("floatfield", 0f, 10f, 0.1f, 1f, "A float")
				.AddBoolField("boolfield", true, "A bool")
				.AddStringField("stringfield", "example_string%-%", "A string")
				.SetUADFactory((room, data, firstTimeRealized) => new ExampleEffectUAD(data))
				.SetCategory("POMEffectsExamples")
				.Register();
		}
		catch (Exception ex)
		{
			LogWarning($"Error on eff examples init {ex}");
		}
	}
}

internal class ExampleEffectUAD : UpdatableAndDeletable
{
	private ExampleDrawable[] _drawables;
	private bool _setupRan;

	public EffectExtraData EffectData { get; }

	public ExampleEffectUAD(EffectExtraData effectData)
	{
		_drawables = new ExampleDrawable[0];
		EffectData = effectData;
	}

	public override void Update(bool eu)
	{
		int drawableCount = EffectData.GetInt("intfield");
		if (!_setupRan)
		{
			LogWarning($"Example effect go in room {room.abstractRoom.name} : {this.EffectData.GetString("stringfield")}");
			_setupRan = true;
		}
		if (drawableCount != _drawables.Length)
		{
			foreach (var drawable in _drawables)
			{
				room.RemoveObject(drawable);
			}
			_drawables = new ExampleDrawable[drawableCount];
			for (int i = 0; i < drawableCount; i++)
			{
				_drawables[i] = new(this);
				room.AddObject(_drawables[i]);
			}
		}
	}

	private class ExampleDrawable : UpdatableAndDeletable, IDrawable
	{
		private Vector2 pos;
		private Vector2 vel;
		private Vector2 acceleration;
		private Vector2 screenspace;
		private readonly ExampleEffectUAD owner;

		public ExampleDrawable(ExampleEffectUAD owner)
		{
			this.owner = owner;
		}
		public override void Update(bool eu)
		{
			base.Update(eu);
			float acceleration_intensity = this.owner.EffectData.GetFloat("floatfield");
			acceleration = new Vector2(UnityEngine.Random.value * 2f - 1, UnityEngine.Random.value * 2f - 1) * acceleration_intensity;
			vel += acceleration;
			pos += vel;
			Rect bounds = new(new(), screenspace);
			if (!bounds.Contains(pos))
			{
				Reset(screenspace);
			}
		}
		private void Reset(Vector2 sSize)
		{
			vel = new();
			screenspace = sSize;
			pos = screenspace / 2f;
			if (owner.EffectData.GetBool("boolfield"))
			{
				room.AddObject(new Spark(
					((Player)room.updateList.FirstOrDefault(x => x is Player))?.firstChunk.pos ?? new(),
					UnityEngine.Random.insideUnitCircle * 5f,
					new(1f, 0f, 0f),
					null,
					15,
					30));
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
		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].SetPosition(pos);
			if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || this.room != rCam.room))
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{

		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
		{
			foreach (var sprite in sLeaser.sprites) sprite?.RemoveFromContainer();
			newContatiner ??= rCam.ReturnFContainer("Foreground");
			sLeaser.sprites[0].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[0]);
		}
	}
}