namespace EffExt;
/// <summary>
/// Factory callbacks that are used to create an UpdatableAndDeletable for a given effect.
/// </summary>
/// <param name="room">Current room</param>
/// <param name="data">Object carrying additional data for your effect's custom fields</param>
/// <param name="firstTimeRealized">Whether it's the first time the room is being realized this session</param>
/// <returns>An UpdatableAndDeletable that should be added to the room. Null if none.</returns>
public delegate UpdatableAndDeletable? UADFactory(Room room, EffectExtraData data, bool firstTimeRealized);

/// <summary>
/// Callbacks that are used when you need your effect to do something on initialize, without spawning an UpdatableAndDeletable.
/// </summary>
/// <param name="room">Current room</param>
/// <param name="data">Object carrying additional data for your effect's custom fields</param>
/// <param name="firstTimeRealized">Whether it's the first time the room is being realized this session</param>
public delegate void EffectInitializer(Room room, EffectExtraData data, bool firstTimeRealized);