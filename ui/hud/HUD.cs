using Godot;
using System;
using projeto_lookout.libs;

public partial class HUD : CanvasLayer
{
	public static string GoldLabelPrefix; // The yene symbol is unicode and Godot doesn't like it


	private TextureRect _healthBar;
	private Vector2 _healthBarSize;

	private TextureRect _staminaBar;
	private Vector2 _staminaBarSize;

	private TextureRect _bowItemIcon;
	private TextureRect _bootsItemIcon;

	private Label _healthPotionAmoutLabel;

	private Label _staminaPotionAmonutLabel;

	private Label _goldLabel;

	private TextureRect _crosshair;

	public override void _Ready()
	{
		Resources.Instance.HUD = this;

		_healthBar = GetNode<TextureRect>("Gauges/Health/Bar");
		_healthBarSize = _healthBar.Size;

		_staminaBar = GetNode<TextureRect>("Gauges/Stamina/Bar");
		_staminaBarSize = _staminaBar.Size;

		_bowItemIcon = GetNode<TextureRect>("Items/BowItem/Icon");
		_bootsItemIcon = GetNode<TextureRect>("Items/BootsItem/Icon");

		_goldLabel = GetNode<Label>("GoldLabel");
		GoldLabelPrefix = _goldLabel.Text;

		_healthPotionAmoutLabel = GetNode<Label>("Items/HealthPotions/Amount");

		_staminaPotionAmonutLabel = GetNode<Label>("Items/StaminaPotions/Amount");

		_crosshair = GetNode<TextureRect>("Crosshair");
	}

	public void SetCrosshairEnabled(bool enabled)
	{
		_crosshair.Visible = enabled;
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
		_goldLabel.Text = GoldLabelPrefix + amount;
	}

	public void SetHealthPotionAmount(int amount)
	{
		_healthPotionAmoutLabel.Text = amount.ToString();
	}

	public void SetStaminaPotionAmount(int amount)
	{
		_staminaPotionAmonutLabel.Text = amount.ToString();
	}

	public void SetBowItemIcon(Texture2D texture)
	{
		_bowItemIcon.Texture = texture;
	}

	public void SetBootsItemIcon(Texture2D texture)
	{
		_bootsItemIcon.Texture = texture;
	}
}
