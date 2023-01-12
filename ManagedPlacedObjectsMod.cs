// This is ManagedPlacedObjects mini-framework, written by henpemaz
// and ported to Downpour by Thalber. 
// It is licensed under Creative Commons 0 and is designed
// to be "stackable" at runtime with multiple instances of POM
// in multiple mods. It is not recommended to do that anymore, but
// you still can if you really want to.
namespace ManagedPlacedObjects
{
    [BepInEx.BepInPlugin("rwmodding.coreorg.pom", "PlacedObjectManager", "2.0")]
	public class PomMod : BepInEx.BaseUnityPlugin
    {
        public PomMod()
        {
        }

        //public static PomMod instance;

        public void OnEnable()
        {
            
            // Hooking code goose hre

            //PlacedObjectsManager.Apply(); // no longer necessary, auto applied when anything uses it
            //Examples.PlacedObjectsExample();
        }
    }
}
