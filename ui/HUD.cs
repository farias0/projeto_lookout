using Godot;
using System;
using projeto_lookout.libs;

public partial class HUD : CanvasLayer
{
	private TextureRect _healthBar;
	private Vector2 _healthBarSize;

	private TextureRect _staminaBar;
	private Vector2 _staminaBarSize;

	private Label _healthPotionAmoutLabel;

	private Label _goldLabel;
	private String _goldLabelPrefix; // The yene symbol is unicode and Godot doesn't like it


	public override void _Ready()
	{
		Resources.HUD = this;

		_healthBar = GetNode<TextureRect>("Gauges/Health/Bar");
		_healthBarSize = _healthBar.Size;

		_staminaBar = GetNode<TextureRect>("Gauges/Stamina/Bar");
		_staminaBarSize = _staminaBar.Size;

		_goldLabel = GetNode<Label>("GoldLabel");
		_goldLabelPrefix = _goldLabel.Text;

		_healthPotionAmoutLabel = GetNode<Label>("HealthPotions/Amount");
	}

	/// <param name="pct">Between 0 and 1</param>
	public void SetHealth(float pct)
	{
		if (pct < 0 || pct > 1)
			throw new ArgumentOutOfRangeException(nameof(pct), "Value must be between 0 and 1.");

		_healthBar.SetSize(new Vector2(_healthBarSize.X * pct, _healthBarSize.Y));
	}

	public void SetHealthBarVisible(bool visible)
	{
		_healthBar.Visible = visible;
	}

	/// <param name="pct">Between 0 and 1</param>
	public void SetStamina(float pct)
	{
		if (pct < 0 || pct > 1)
			throw new ArgumentOutOfRangeException(nameof(pct), "Value must be between 0 and 1.");

		_staminaBar.SetSize(new Vector2(_staminaBarSize.X * pct, _staminaBarSize.Y));
	}
	public void SetStaminaBarVisible(bool visible)
	{
		_staminaBar.Visible = visible;
	}

	public void SetGoldAmount(int amount)
	{
		_goldLabel.Text = _goldLabelPrefix + amount;
	}

	public void SetHealthPotionAmount(int amount)
	{
		_healthPotionAmoutLabel.Text = amount.ToString();
	}
}
