using Game.Resources.Level;
using Godot;
using System;
using System.Text.Json;

namespace Game.AutoLoad;

public partial class SaveManager : Node
{

	public static SaveManager Instance { get; private set; }

	private static SaveData saveData = new();
	private const string SAVE_DATA_FILE_PATH = "user://save.json";

	public override void _EnterTree()
	{
		Instance = this;
		LoadSaveData();
	}

	public static bool IsLevelCompleted(string levelId)
	{
		saveData.LevelCompletionStatus.TryGetValue(levelId, out var data);

		return data?.IsCompleted == true;
	}

	public static void SaveLevelCompletion(LevelDefinitionResource levelDefinitionResource)
	{
		saveData.SaveLevelCompletion(levelDefinitionResource.Id, true);
		WriteSaveData();
		GD.Print(levelDefinitionResource.Id);
	}

	private static void LoadSaveData()
	{
		if (!FileAccess.FileExists(SAVE_DATA_FILE_PATH)) return;

		using var saveFile = FileAccess.Open(SAVE_DATA_FILE_PATH, FileAccess.ModeFlags.Read);
		var stringData = saveFile.GetLine();
		try
		{
			var loadedSaveData = JsonSerializer.Deserialize<SaveData>(stringData);
			saveData = loadedSaveData;
		}
		catch (Exception e)
		{
			GD.PushWarning("Save file is correct, cant load data: ", e.Message);
		}
	}

	private static void WriteSaveData()
	{
		var dataString = JsonSerializer.Serialize(saveData);
		using var saveFile = FileAccess.Open(SAVE_DATA_FILE_PATH, FileAccess.ModeFlags.Write);
		saveFile.StoreLine(dataString);
	}
}
