using Godot;
using System;
using projeto_lookout.libs;

public partial class TradeItem : Control
{
	public Sell Sell { get => _sell; set => SetSell(value); }

	private Control _iconContainer;
	private Label _itemName;
	private Label _price;
	private Godot.Button _button;

	private Sell _sell;


	public override void _Ready()
	{
		_iconContainer = GetNode<Control>("IconContainer");
		_itemName = GetNode<Label>("LabelName");
		_price = GetNode<Label>("LabelPrice");
		_button = GetNode<Godot.Button>("Button");

		_button.Connect("pressed", new(this, nameof(ButtonPressed)));

		if (Sell != null) SetSell(Sell);
	}

	private void ButtonPressed()
	{
		if (Sell.SellItem())
		{
			//QueueFree();
			// TODO control stock
		}
	}

	private void SetSell(Sell sell)
	{
		_sell = sell;

		if (IsNodeReady())
		{
			_iconContainer.AddChild(new TextureRect()
			{
				Texture = sell.Icon,
				ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
				Size = _iconContainer.Size
			});

			_itemName.Text = Sell.ItemName;
			_price.Text = $"{HUD.GoldLabelPrefix} {Sell.Price}";
		}
	}
}
