namespace Eff;

public class ExampleEffectUAD : UpdatableAndDeletable
{
	public ExampleEffectUAD()
	{
		init = new(() =>
		{
			plog.LogWarning($"Example effect go in room {room.abstractRoom.name}");
		});

	}
	FnOnce init;
	public override void Update(bool eu)
	{
		init.Invoke();
		base.Update(eu);
	}
}