using Godot;
using System;
using projeto_lookout.libs;
using System.Collections.Generic;

public partial class InventoryItem : TextureButton
{
	/*
	 *		Shape Strings are used to define the shape of the item.
	 *	They're defined by X and O characters, where X is a filled cell and O is an empty cell.
	 *	
	 *		For instance, a 3x3 L shaped item can be defined as:
	 *				XOO            XXX
	 *				XOO     or     XOO        , among other ways
	 *				XXX            XOO
	 *				
	 *				
	 *	TODO: Consider checking against shapes with empty columns or rows in their edges.
	 */

	[Export(PropertyHint.MultilineText)]
	public string ShapeString
	{
		get => _shapeString;
		set => UpdateShape(value);
	}

	
	private static readonly PackedScene CellScene = (PackedScene)GD.Load("res://ui/inventory/items/item_cell.tscn");


	private string _shapeString;
	private bool[][] _shape;
	private bool _isDragging = false;
	private Vector2 _dragOffset;
	private List<TextureRect> _cells = new();


	public override void _Ready()
	{
		PivotOffset = Size / 2;
	}

	public override void _Input(InputEvent @event)
	{
		if (!Resources.Instance.Inventory.IsEnabled()) return;

		if (@event is InputEventMouseButton mouseEvent)
		{
			OnClick(mouseEvent);
		}
		else if (@event is InputEventMouseMotion motionEvent && _isDragging)
		{
			// Update the position while dragging
			Position = motionEvent.Position + _dragOffset;
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (mouseEvent.Pressed)
			{
				Debug.Log($"{Name} clicked.");
			}
		}
	}

	private void OnClick(InputEventMouseButton mouseEvent)
	{
		if (!Resources.Instance.Inventory.IsEnabled()) return;


		if (mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (mouseEvent.Pressed)
			{
				// Check if the click happened within the button's area
				if (GetRotatedRect().HasPoint(mouseEvent.Position))
				{
					// Start dragging
					_isDragging = true;
					_dragOffset = Position - mouseEvent.Position;
				}
			}
			else
			{
				// Left mouse button released, stop dragging
				_isDragging = false;
			}
		}
		else if (mouseEvent.ButtonIndex == MouseButton.Right)
		{
			
			//					TODO only rotate inside button's area
			
			if (mouseEvent.Pressed)
			{
				RotateShape();
			}
		}
	}
	private void RotateShape()
	{
		RotationDegrees += 90;
		if (RotationDegrees == 360)
			RotationDegrees = 0;
	}

	/// <summary>
	/// Like GetRect(), but compensates for the rotation of the item.
	/// Presumes the rotations are always at 90 degree intervals.
	/// </summary>
	private Rect2 GetRotatedRect()
	{
		Rect2 rect = new(){
			Position = GlobalPosition
		};

		if (RotationDegrees == 0 || RotationDegrees == 180) {
			rect.Size = Size * Scale;
		}
		else if (RotationDegrees == 90 || RotationDegrees == 270)
		{
			rect.Size = new Vector2(Size.Y, Size.X) * Scale;
		}
		else
			throw new InvalidOperationException($"Invalid rotation {RotationDegrees}.");

		Debug.Log($"{Name} rect: {rect}");
		return rect;
	}

	private void UpdateShape(string shapeString)
	{
		string[] lines = shapeString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

		foreach (var line in lines)
		{
			if (line.Length != lines.Length)
				throw new ArgumentException("Non square Item Shape String.");
		}

		_shape = new bool[lines.Length][];
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];

			_shape[i] = new bool[line.Length];
			for (int j = 0; j < line.Length; j++)
			{
				if (line[j] != 'X' && line[j] != 'O')
					throw new ArgumentException($"Invalid character in Item Shape String. Char: {line[j]}, line: {j}.");

				_shape[i][j] = line[j] == 'X';
			}
		}

		_shapeString = shapeString;
		UpdateCells();
	}

	private void UpdateCells()
	{
		foreach (var cell in _cells)
		{
			cell.QueueFree();
		}
		_cells.Clear();

		for (int i = 0; i < _shape.Length; i++)
		{
			for (int j = 0; j < _shape[i].Length; j++)
			{
				if (_shape[i][j])
				{
					TextureRect cell = (TextureRect)CellScene.Instantiate();
					cell.Position = new Vector2(j * cell.Size.X, i * cell.Size.Y);
					AddChild(cell);
					_cells.Add(cell);
				}
			}
		}
	}
}
