using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cerulean : MonoBehaviour
{
	public LayerMask platformMask;
	public float platformDist;

	public int moveDir = 1;
	public float moveSpeed;

	void Start ()
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, platformMask);
		platformDist = hit.distance;
	}
	
	void Update () 
	{
		RaycastHit2D bot = Physics2D.Raycast(transform.position, Vector2.down, platformDist * 1.1f, platformMask);
		RaycastHit2D left = Physics2D.Raycast(transform.position, Vector2.left, (0.20f + moveSpeed * Time.deltaTime), platformMask);
		RaycastHit2D right = Physics2D.Raycast(transform.position, Vector2.right, (0.20f + moveSpeed * Time.deltaTime), platformMask);

		if (!bot || left || right)
		{
			ChangeDirection();
		}

		if(!MapManager.instance.inMapEdit && !MapManager.instance.isCleared)
			transform.Translate(Vector3.right * moveSpeed * Time.deltaTime * moveDir);
	}

	void ChangeDirection()
	{
		moveDir = -moveDir;
	}
}
