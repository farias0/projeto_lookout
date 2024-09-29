using Godot;
using System;
using projeto_lookout.libs;
using System.Collections.Generic;
using System.Linq;


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

	[Export]
	public string ID { get; set; }
	[Export]
	public string Label {  get; set; }
	[Export]
	public bool IsBowItem { get; set; }
	[Export(PropertyHint.MultilineText)]
	public string ShapeString
	{
		get => _shapeString;
		set => UpdateShape(value);
	}
	[Export]
	public PackedScene SpawnsItem { get; set; }


	public List<ItemCell> Cells
	{
		get => GetCells();
	}


	private static readonly PackedScene CellScene = (PackedScene)GD.Load("res://ui/inventory/items/cell/item_cell.tscn");


	private string _shapeString;
	private ItemCell[][] _cells = Array.Empty<ItemCell[]>();
	private bool _isDragging = false;
	private Vector2 _preDragPosition;
	private float _preDragRotation;
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

	public void OnClick(ItemCell clickedCell, InputEventMouseButton mouseEvent)
	{
		if (mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (mouseEvent.Pressed)
			{
				BeginDragging(mouseEvent.GlobalPosition);
			}
			else // Released mouse button
			{
				FinishDragging();
			}
		}
		else if (mouseEvent.ButtonIndex == MouseButton.Right)
		{
			if (mouseEvent.Pressed && _isDragging)
			{
				_dragOffset += RotateAroundCell(clickedCell);
				Position = mouseEvent.GlobalPosition + _dragOffset;
				Resources.Instance.Inventory.Audio.PlayItemRotated();
			}
		}
	}

	public void ResetDraggingPosition()
	{
		Position = _preDragPosition;
		RotationDegrees = _preDragRotation;
	}

	private void BeginDragging(Vector2 fromPosition)
	{
		_isDragging = true;
		_preDragPosition = Position;
		_preDragRotation = RotationDegrees;
		_dragOffset = Position - fromPosition;
		Resources.Instance.Inventory.StartDraggingItem(this);
	}

	private void FinishDragging()
	{
		var inventory = Resources.Instance.Inventory;

		_isDragging = false;
		if (!inventory.AttemptItemDrag(this))
		{
			ResetDraggingPosition();
			Resources.Instance.Inventory.CancelDraggingItem(this);
		}
	}

	/// <summary>
	/// Rotates the item 90 degrees clockwise.
	/// Also returns the 2D offset so to apply so it rotates around the given cell.
	/// </summary>
	private Vector2 RotateAroundCell(ItemCell cell) // TODO break this function down
	{
		RotationDegrees += 90;
		if (RotationDegrees == 450) // Including 360 degrees allows us to detect a full rotation
			RotationDegrees = 90;


		var cellSize = Cells[0].GetSize();

		int x = 0, y = 0;
		bool foundCell = false;
		for (x = 0; x < _cells.Length; x++)
		{
			for (y = 0; y < _cells[x].Length; y++)
			{
				if (_cells[x][y] == cell)
				{
					foundCell = true;
					break;
				}
			}
			if (foundCell) break;
		}
		if (!foundCell)
			throw new InvalidOperationException("Could not find cell to rotate around.");


		return RotationDegrees switch
		{
			0 => new Vector2(0, 0),
			90 => new Vector2((x + y - 1) * cellSize.X, (x - y) * cellSize.X),
			180 => new Vector2((-x + y) * cellSize.X, (x + y - 1) * cellSize.X),
			270 => new Vector2((-x - y + 1) * cellSize.X, (-x + y) * cellSize.X),
			360 => new Vector2((x - y) * cellSize.X, (-x - y + 1) * cellSize.X),
			_ => throw new InvalidOperationException($"Invalid Item rotation {RotationDegrees}."),
		};
	}

	///// <returns>The dimensions of this item's shape (based on its cells).</returns>
	//private Vector2 ShapeDimensions()
	//{
	//	var cellSize = Cells[0].GetSize();
	//	return new Vector2(_shape[0].Length * cellSize.X, _shape.Length * cellSize.Y);
	//}

	private void UpdateShape(string shapeString)
	{
		string[] lines = shapeString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		bool[][] shape;

		foreach (var line in lines)
		{
			if (line.Length != lines[0].Length)
				throw new ArgumentException("Non regular Item Shape String.");
		}

		shape = new bool[lines.Length][];
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];

			shape[i] = new bool[line.Length];
			for (int j = 0; j < line.Length; j++)
			{
				if (line[j] != 'X' && line[j] != 'O')
					throw new ArgumentException($"Invalid character in Item Shape String. Char: {line[j]}, line: {j}.");

				shape[i][j] = line[j] == 'X';
			}
		}

		_shapeString = shapeString;
		UpdateCells(shape);
	}

	private List<ItemCell> GetCells()
	{
		return _cells.SelectMany(_cellLine => _cellLine).Where(cell => cell != null).ToList();
	}

	private void UpdateCells(bool[][] shape)
	{
		foreach (var cell in GetCells())
		{
			cell.QueueFree();
		}

		_cells = new ItemCell[shape.Length][];

		for (int i = 0; i < shape.Length; i++)
		{
			_cells[i] = new ItemCell[shape[i].Length];

			for (int j = 0; j < shape[i].Length; j++)
			{
				if (shape[i][j])
				{
					ItemCell cell = (ItemCell)CellScene.Instantiate();
					cell.Type = CellType.ItemCell;
					cell.Item = this;
					cell.Position = new Vector2(j * cell.Size.X, i * cell.Size.Y);
					AddChild(cell);
					_cells[i][j] = cell;
				}
			}
		}
	}
}
