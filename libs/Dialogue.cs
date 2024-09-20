using Godot;
using System;
using projeto_lookout.libs;

public class Dialogue
{
	/*
	 *		This is a wrapper class for the dialogue.gd script.
	 *		
	 *		This module was written in GDScript so we could export an array of multiline strings
	 *	as an Inspector property, which makes it much more convient to write dialogues.
	 */

	readonly GodotObject _dialogue;

	public Dialogue(string path)
	{
		_dialogue = GD.Load<GodotObject>(path);
	}

	public void Start()
	{
		_dialogue.Call("start");
	}

	public void Process(float delta)
	{
		_dialogue.Call("process", delta);
	}

	public void NextLine()
	{
		_dialogue.Call("next_line");
	}

	public void Stop()
	{
		_dialogue.Call("stop");
	}

	public bool IsFinished => (bool)_dialogue.Get("is_finished");
}
