using Godot;
using System;
using projeto_lookout.libs;

public class Dialogue
{
	public bool IsFinished = false;

	private const float AutoSkipTime = 4f;

	private string[] _lines;
	private int currentLine;
	private float _autoSkipCountdown = -1;

	public Dialogue(string[] lines) // More like a monologue, am I right?
	{
		_lines = lines;
	}

	public void Start()
	{
		IsFinished = false;
		currentLine = -1;
		NextLine();
	}

	public void Process(float delta)
	{
		_autoSkipCountdown -= delta;
		if (_autoSkipCountdown <= 0)
		{
			NextLine();
		}
	}

	public void NextLine()
	{
		currentLine++;

		if (currentLine >= _lines.Length)
		{
			Stop();
			return;
		}

		_autoSkipCountdown = AutoSkipTime;
		Resources.Subtitles.Show(_lines[currentLine]);
	}

	public void Stop()
	{
		Resources.Subtitles.Hide();
		IsFinished = true;
	}
}
