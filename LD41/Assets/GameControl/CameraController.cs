using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController {

	private WorldManager manager;
	Transform transform;

	public CameraController(WorldManager manager) {
		this.manager = manager;
		this.transform = Camera.main.GetComponent<Transform>();
	}

	public void SnapToFullView() {
		this.ResizeCamera(this.manager.gridSize);
		this.CenterCamera();
	}

	public void SnapToWorld(World world) {
		this.ResizeCamera(this.manager.gridSize * world.scale);
		this.CenterCamera(world);		
	}

	// The size argument here refers to the FULL width or height of the square block
	// the camera should capture.
	private void ResizeCamera(float size) {
		Camera.main.orthographicSize = size / 2f;
	}

	private void CenterCamera(World world = null) {
		if (world == null)
			this.SetCameraPosition(new Vector2(this.manager.gridSize / 2f - 0.5f, this.manager.gridSize / 2f - 0.5f));
		else {
			this.SetCameraPosition(world.Center() - world.scale * new Vector2(0.5f, 0.5f));
		}
	}

	private void SetCameraPosition(Vector2 pos) {
		this.transform.position = new Vector3(pos.x, pos.y, -1);
	}
}
