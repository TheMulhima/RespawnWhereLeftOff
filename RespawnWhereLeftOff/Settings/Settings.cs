using Modding.Converters;
namespace RespawnWhereLeftOff;

public class SaveSettings
{
	[JsonConverter(typeof(Modding.Converters.Vector3Converter))]
	public Vector3 RespawnPoint;

	public string respawnScene = null;
}