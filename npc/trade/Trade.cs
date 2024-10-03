using Godot;
using System;
using projeto_lookout.libs;
using System.Linq;

public partial class Trade : Resource
{
	[Export]
	public Godot.Collections.Array<Sell> Sells { get; set; }

	public void StartTrade(Node screenNodeParent)
	{
		var tradeScreen = Resources.Instance.TradeScreen.Instantiate() as TradeScreen;
		tradeScreen.AddSells(Sells.ToList());
		screenNodeParent.AddChild(tradeScreen);
	}
}
