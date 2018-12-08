using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class Player : MonoBehaviour
{
	int moveDir;
	public Sprite[] sprites;
	public bool isJumping;
	public float speed;
	public float jumpForce;

	Vector3 velocity;
	public float gravityScale;

	Animator animator;
	SpriteRenderer renderer;
	CharacterController2D cc2D;

	public LayerMask platformMask;
	public LayerMask groundMask;

	public bool dead;
	bool deadAnimationDone;

	public bool onLadder;
	public bool onClimb;

	GameObject lastLadder;
	GameObject targetPlatform;
	float ladderStartY;
	float ladderEndY;
	int ladderDir;

	public AudioClip walkSound;
	public AudioClip jumpSound;
	public AudioClip pointSound;
	public AudioClip clearSound;
	public AudioClip fallSound;
	public AudioClip dieSound;
	public AudioClip ceruleanSound;

	int score;

	void Awake () 
	{
		renderer = GetComponent<SpriteRenderer>();
		cc2D = GetComponent<CharacterController2D>();
		animator = GetComponent<Animator>();

		cc2D.onTriggerEnterEvent += (Collider2D coll) =>
		{
			if(coll.tag == "Danger")
				Die();

			if (coll.tag == "JapariPan")
			{
				GetComponent<AudioSource>().PlayOneShot(pointSound);
				coll.tag = "Untagged";
				float x = Screen.width - 48;
				float y = Screen.height / 2 - (MapManager.instance.currentMap.targetScore - score) * 24;
				coll.gameObject.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 0)) + Vector3.forward * 10;
				score++;

				if (score == MapManager.instance.currentMap.targetScore)
				{
					StartCoroutine(Clear());
				}
			}

			if(coll.tag == "Ladder")
			{
				lastLadder = coll.gameObject;
			}
		};
	}


	void Update ()
	{
		if (MapManager.instance.inMapEdit || MapManager.instance.isCleared)
			return;

		if (dead)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), 0.2f);

			if (!cc2D.isGrounded && !deadAnimationDone)
			{
				cc2D.move(Vector3.down * speed * Time.deltaTime);
			}
			else
			{
				if (!deadAnimationDone)
				{
					GetComponent<AudioSource>().Stop();
					GetComponent<AudioSource>().PlayOneShot(dieSound);
				}
				deadAnimationDone = true;
			}

			return;
		}

		Bounds bounds = GetComponent<BoxCollider2D>().bounds;

		Collider2D[] overlapped = Physics2D.OverlapAreaAll(bounds.min, bounds.max);

		onLadder = false;

		foreach(Collider2D coll in overlapped)
		{
			if(coll.tag == "Ladder")
			{
				onLadder = true;
			}
		}

		if (onLadder && !onClimb && !isJumping)
		{
			Vector3 top = new Vector3(transform.position.x, bounds.max.y);
			Vector3 bot = new Vector3(transform.position.x, bounds.min.y);

			ladderDir = (int)Mathf.Sign(lastLadder.transform.position.y - bot.y);

			if ((Input.GetKey(KeyCode.W) && ladderDir == 1) || (Input.GetKey(KeyCode.S) && ladderDir == -1))
			{
				ladderStartY = bot.y;

				RaycastHit2D[] hits = null;

				if (ladderDir == 1)
				{
					hits = Physics2D.RaycastAll(top, Vector2.up, Mathf.Infinity, platformMask);
					if (hits.Length > 0)
					{
						targetPlatform = hits[0].transform.gameObject;
						ladderEndY = targetPlatform.GetComponent<BoxCollider2D>().bounds.max.y;
						cc2D.platformMask = 0;
					}
				}
				else
				{
					hits = Physics2D.RaycastAll(bot, Vector2.down, Mathf.Infinity, platformMask);

					if (hits.Length > 1)
					{
						targetPlatform = hits[1].transform.gameObject;
						ladderEndY = targetPlatform.GetComponent<BoxCollider2D>().bounds.max.y;
						cc2D.platformMask = 0;
					}
				}

				onClimb = true;
			}
		}

		if (onClimb)
		{

			Vector3 top = new Vector3(transform.position.x, GetComponent<BoxCollider2D>().bounds.max.y);
			Vector3 bot = new Vector3(transform.position.x, GetComponent<BoxCollider2D>().bounds.min.y);

			moveDir = 0;

			Vector3 vel = new Vector3();

			if (Input.GetKey(KeyCode.W))
			{
				vel = Vector3.up * speed * Time.deltaTime;
				animator.Play(Animator.StringToHash("Climb"));
			}

			else if (Input.GetKey(KeyCode.S))
			{
				vel = Vector3.down * speed * Time.deltaTime;
				animator.Play(Animator.StringToHash("Climb"));
			}
			else
			{
				animator.Play(Animator.StringToHash("Climb_Idle"));
			}

			if (ladderDir == 1)
			{
				if (bot.y + vel.y >= ladderEndY)
				{
					onClimb = false;
					cc2D.move(Vector3.up * ((bot.y + vel.y) - ladderEndY));
					vel = Vector3.zero;
					cc2D.platformMask = platformMask;
					animator.Play(Animator.StringToHash("Idle"));
				}

				if(bot.y + vel.y <= ladderStartY)
				{
					onClimb = false;
					cc2D.move(Vector3.up * (ladderStartY - (bot.y + vel.y)));
					vel = Vector3.zero;
					cc2D.platformMask = platformMask;
					animator.Play(Animator.StringToHash("Idle"));
				}
			}
			else
			{
				if (bot.y + vel.y <= ladderEndY)
				{
					onClimb = false;
					cc2D.move(Vector3.up * (ladderEndY -(bot.y + vel.y)));
					vel = Vector3.zero;
					cc2D.platformMask = platformMask;
					animator.Play(Animator.StringToHash("Idle"));
				}

				if (bot.y + vel.y >= ladderStartY)
				{
					onClimb = false;
					cc2D.move(Vector3.up * ((bot.y + vel.y) - ladderStartY));
					vel = Vector3.zero;
					cc2D.platformMask = platformMask;
					animator.Play(Animator.StringToHash("Idle"));
				}
			}

			cc2D.move(vel);
		}

		if (cc2D.isGrounded)
		{
			velocity.y = 0;

			if (Input.GetKey(KeyCode.D))
			{
				moveDir = 1;
				renderer.flipX = false;
				animator.Play(Animator.StringToHash("Move"));
			}
			else if (Input.GetKey(KeyCode.A))
			{
				moveDir = -1;
				renderer.flipX = true;
				animator.Play(Animator.StringToHash("Move"));
			}
			else
			{
				moveDir = 0;
				animator.Play(Animator.StringToHash("Idle"));
			}
		}

		if (Input.GetKey(KeyCode.Space))
		{
			if (!isJumping && cc2D.isGrounded)
			{
				GetComponent<AudioSource>().PlayOneShot(jumpSound);

				isJumping = true;
				velocity.y = jumpForce;
				cc2D.move(velocity * Time.deltaTime);
			}
		}

		if (isJumping)
		{
			velocity.x = moveDir * speed;

			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 30 * moveDir * Mathf.Sign(velocity.y)), 0.2f);

			if (cc2D.isGrounded)
				isJumping = false;
		}
		else
		{
			velocity.x = moveDir * speed;

			transform.eulerAngles = Vector3.zero;
		}

		if (!onClimb)
		{
			velocity.y += gravityScale * Time.deltaTime;
			cc2D.move(velocity * Time.deltaTime);
		}
	}
	
	public void WalkSound()
	{
		if(!isJumping)
		GetComponent<AudioSource>().PlayOneShot(walkSound);
	}

	IEnumerator Clear()
	{
		animator.Play(Animator.StringToHash("Idle"));
		MapManager.instance.isCleared = true;
		yield return new WaitForSeconds(1);
		MapManager.instance.clearPanel.SetActive(true);
		GetComponent<AudioSource>().PlayOneShot(clearSound);
		yield return new WaitForSeconds(3);
		MapManager.instance.clearPanel.SetActive(false);
		MapManager.instance.UpdateMap(MapManager.instance.currentMap);
	}

	void Die()
	{
		if (!dead)
		{
			dead = true;
			animator.Play(Animator.StringToHash("Die"));
			cc2D.platformMask = groundMask;

			cc2D.move(Vector3.up * speed * Time.deltaTime);

			GetComponent<AudioSource>().PlayOneShot(fallSound);
		}
	}
}
