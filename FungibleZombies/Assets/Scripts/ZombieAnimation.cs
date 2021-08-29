using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Animator))]
public class ZombieAnimation : MonoBehaviour
{

	private Animator animator;

	void Awake () {
		animator = GetComponent<Animator>();
    animator.speed = 1f;
	}

	public void Idle () {
    animator.speed = 1f;
    animator.Play("zombie_idle");
  }

	public void Walk () {
    animator.speed = 1f;
    animator.Play("zombie_walk");
	}

	public void Run () {
    animator.speed = 2f;
    animator.Play("zombie_walk");
	}

	public void Attack () {
    animator.speed = 1f;
    animator.Play("zombie_attack");
	}

	public void Death () {
    animator.speed = 1f;
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("zombie_death_standing"))
			animator.Play("zombie_idle", 0);
		else
      animator.Play("zombie_death_standing");
	}

	public void Jump () {
    // TODO
	}

}
