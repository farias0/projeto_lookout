using Godot;
using System;
using projeto_lookout.libs;

public partial class Subtitles : Control
{
	private RichTextLabel _label;

	public override void _Ready()
	{
		Resources.Subtitles = this;

		_label = GetNode<RichTextLabel>("RichTextLabel");

		Visible = false;
	}

	public void Show(string text)
	{
		Visible = true;
		_label.Text = WrapText(text);
	}

	public void Stop()
	{
		Visible = false;
		_label.Text = "";
	}

	private static string WrapText(string text)
	{
		return $"[center]{text}[/center]";
	}
}
