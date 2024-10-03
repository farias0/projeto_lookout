using Godot;
using System;
using System.Collections.Generic;
using projeto_lookout.libs;

public partial class TradeScreen : Control
{
	private readonly List<TradeItem> _tradeItems = new();

	private static readonly PackedScene TradeItem =
		(PackedScene)GD.Load("res://ui/trade/trade_item.tscn");

	public void AddSells(List<Sell> sells)
	{
		foreach (var sell in sells)
		{
			var tradeItem = TradeItem.Instantiate() as TradeItem;
			tradeItem.Sell = sell;
			_tradeItems.Add(tradeItem);
			GetNode<Control>("Panel/Frame").AddChild(tradeItem);
		}
	}

	public override void _Ready()
	{
		StartTrade();
	}

	public override void _Process(double delta)
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("back"))
			FinishTrade();
	}

	private void StartTrade()
	{
		Resources.Instance.OngroingTrade = this;
		Input.MouseMode = Input.MouseModeEnum.Visible;
		Resources.Instance.HUD.SetCrosshairEnabled(false);
	}

	private void FinishTrade()
	{
		Resources.Instance.OngroingTrade = null;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Resources.Instance.HUD.SetCrosshairEnabled(true);
		QueueFree();
	}
}
