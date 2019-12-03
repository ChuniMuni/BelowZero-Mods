using System;
using System.Reflection;
using Harmony;

namespace NoOxygenWarnings
{
	public class Entry
	{
        public static void Patch()
        {
            try
            {
                HarmonyInstance.Create("MrPurple6411.NoOxygenWarnings").PatchAll(Assembly.GetExecutingAssembly());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
