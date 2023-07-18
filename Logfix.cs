namespace PomCore;

internal static class Logfix
{
	internal static LevelLogCallback LogDebug = (data) => __DefaultImpl_Log(BepInEx.Logging.LogLevel.Debug, data);
	internal static LevelLogCallback LogInfo = (data) => __DefaultImpl_Log(BepInEx.Logging.LogLevel.Info, data);
	internal static LevelLogCallback LogMessage = (data) => __DefaultImpl_Log(BepInEx.Logging.LogLevel.Message, data);
	internal static LevelLogCallback LogWarning = (data) => __DefaultImpl_Log(BepInEx.Logging.LogLevel.Warning, data);
	internal static LevelLogCallback LogError = (data) => __DefaultImpl_Log(BepInEx.Logging.LogLevel.Error, data);
	internal static LevelLogCallback LogFatal = (data) => __DefaultImpl_Log(BepInEx.Logging.LogLevel.Fatal, data);
	private static void __DefaultImpl_Log(BepInEx.Logging.LogLevel level, object data)
	{
		LevelLogCallback action = level switch
		{
			BepInEx.Logging.LogLevel.Error
			| BepInEx.Logging.LogLevel.Fatal
			=> UnityEngine.Debug.LogError,
			BepInEx.Logging.LogLevel.Warning
			=> UnityEngine.Debug.LogWarning,
			_ => UnityEngine.Debug.Log
		};
		action($"[POM/{level}] {data}");
	}
	internal delegate void LevelLogCallback(object data);
	internal delegate void GeneralLogCallback(BepInEx.Logging.LogLevel level, object data);
}