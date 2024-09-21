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

	
	private static readonly PackedScene CellScene = (PackedScene)GD.Load("res://ui/inventory/items/cell/item_cell.tscn");


	public List<ItemCell> Cells = new();


	private string _shapeString;
	private bool[][] _shape;
	private bool _isDragging = false;
	private Vector2 _preDragPosition;
	private Vector2 _dragOffset;


	public override void _Ready()
	{
		PivotOffset = Size / 2;
	}

	//public override void _Input(InputEvent @event)
	//{
	//	if (!Resources.Instance.Inventory.IsEnabled()) return;	
	//}

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

	public void OnMove(InputEventMouseMotion motionEvent)
	{
		if (!_isDragging) return;
		Position = motionEvent.Position + _dragOffset;
	}

	public void OnClick(InputEventMouseButton mouseEvent)
	{
		var inventory = Resources.Instance.Inventory;

		if (mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (mouseEvent.Pressed)
			{
				_isDragging = true;
				_preDragPosition = Position;
				_dragOffset = Position - mouseEvent.Position;
			}
			else // Released mouse button
			{
				_isDragging = false;
				if (!inventory.AttemptItemDrag(this))
				{
					Position = _preDragPosition;
				}
			}
		}
		else if (mouseEvent.ButtonIndex == MouseButton.Right)
		{
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

	private void UpdateShape(string shapeString)
	{
		string[] lines = shapeString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

		foreach (var line in lines)
		{
			if (line.Length != lines[0].Length)
				throw new ArgumentException("Non regular Item Shape String.");
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
		foreach (var cell in Cells)
		{
			cell.QueueFree();
		}
		Cells.Clear();

		for (int i = 0; i < _shape.Length; i++)
		{
			for (int j = 0; j < _shape[i].Length; j++)
			{
				if (_shape[i][j])
				{
					ItemCell cell = (ItemCell)CellScene.Instantiate();
					cell.Type = CellType.ItemCell;
					cell.Item = this;
					cell.Position = new Vector2(j * cell.Size.X, i * cell.Size.Y);
					AddChild(cell);
					Cells.Add(cell);
				}
			}
		}
	}
}
