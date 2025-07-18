﻿using Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

namespace Walk.TextEditor.RazorLib;

public partial class TextEditorService
{
	/// <summary>
	/// Map an 'extensionNoPeriod' to a 'Type' that inherits 'ComponentBase'.
	/// </summary>
	private readonly Dictionary<string, Type?> _map = new();

	public Type? GetHeader(string extensionNoPeriod)
	{
		if (_map.TryGetValue(extensionNoPeriod, out var type))
			return type;
		else
			return typeof(TextEditorDefaultHeaderDisplay);
	}

	public void UpsertHeader(string extensionNoPeriod, Type? type)
	{
		if (_map.ContainsKey(extensionNoPeriod))
			_map[extensionNoPeriod] = type;
		else
			_map.Add(extensionNoPeriod, type);
	}

	public void RemoveHeader(string extensionNoPeriod)
	{
		_map.Remove(extensionNoPeriod);
	}
}
