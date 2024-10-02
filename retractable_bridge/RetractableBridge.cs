using Godot;
using System;

public partial class RetractableBridge : Node3D, IButtonActionable
{
	[Export]
	public float Speed = 8f;


	private Node3D _meshNode;
	private MeshInstance3D _mesh;
	private StaticBody3D _collider;
	private BoxShape3D _collisionShape;
	private RetractableBridgeAudio _audio;
	private float _extendedSize;
	private bool _isExtending = false;


	public override void _Ready()
	{
		_meshNode = GetNode<Node3D>("BridgeMesh");
		_mesh = _meshNode.GetNode<MeshInstance3D>("MeshInstance3D");
		_collider = GetNode<StaticBody3D>("BridgeCollider");
		_collisionShape = _collider.GetNode<CollisionShape3D>("CollisionShape3D").Shape as BoxShape3D;
		_audio = GetNode<RetractableBridgeAudio>("AudioStreamPlayer3D");

		_extendedSize = GetSize();
		SetSize(0);
	}

	public override void _Process(double delta)
	{
		if (_isExtending)
		{
			SetSize(GetSize() + (Speed * (float)delta));
			if (IsExtended())
			{
				SetSize(_extendedSize);
				_isExtending = false;
				_audio.StopActivate();
			}
		}
	}

	public void ButtonActivate()
	{
		if (!IsExtended())
		{
			_isExtending = true;
			_audio.PlayActivate();
		}
	}

	private bool IsExtended()
	{
		return GetSize() >= _extendedSize;
	}

	private float GetSize()
	{

		/// !! The size of the bridge mesh defines the size of the bridge
		
		return _mesh.Scale.Z;
	}

	private void SetSize(float size)
	{
		{ // Adjust collider
			var pos = _collider.Position;
			pos.Z = (GetSize() / _extendedSize) * _extendedSize / 2;
			_collider.Position = pos;

			var shapeSize = _collisionShape.Size;
			shapeSize.Z = size;
			_collisionShape.Size = shapeSize;
		}

		{ // Adjust mesh
			var pos = _meshNode.Position;
			pos.Z = _collider.Position.Z - (_extendedSize / 2);
			_meshNode.Position = pos;

			var scale = _mesh.Scale;
			scale.Z = size;
			_mesh.Scale = scale;
		}
	}
}
