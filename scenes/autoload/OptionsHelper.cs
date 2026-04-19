using Godot;

namespace Game.AutoLoad;

public partial class OptionsHelper : Node
{
	public override void _Ready()
	{
	}

	public static void SetBusVolumePercent(string busName, float volumePercent)
	{
		var busIndex = AudioServer.GetBusIndex(busName);
		AudioServer.SetBusVolumeLinear(busIndex, volumePercent);
	}

	public static float GetBusVolumePercent(string busName)
	{
		var busIndex = AudioServer.GetBusIndex(busName);
		return AudioServer.GetBusVolumeLinear(busIndex);
	}

	public static void ToggleWindowMode()
	{
		if (IsFullscreen())
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
		}
	}

	public static bool IsFullscreen()
	{
		var windowMode = DisplayServer.WindowGetMode();
		return windowMode == DisplayServer.WindowMode.ExclusiveFullscreen || windowMode == DisplayServer.WindowMode.Fullscreen;
	}
}
