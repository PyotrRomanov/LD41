using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAnimator {
	Interpolator I;
	private float speed = 1f;
	private float end = 0.75f;

	public void SetSpeed(float speed) {
		this.speed = speed;
	}

	public void SetEnd(float end) {
		this.end = end;
	}

	public MovementAnimator(Interpolator I) {
		this.I = I;
	}

	public IEnumerator Animate(Transform T, Vector3 newPos, Lock locker = null) {
		if (locker != null)
			locker.SignalStart();

		Vector3 oldPos = T.position;
		for (float t = 0f; t < this.end; t += Time.deltaTime * speed * Options.Animation.movementSpeed) {
			T.position = I.Interpolate(t) * newPos + (1 - I.Interpolate(t)) * oldPos; 
			yield return new WaitForEndOfFrame();
		}

		T.position = newPos;

		if (locker != null) 
			locker.SignalStop();
	}
}

// A SpriteColorAnimator performs an animation on a given SpriteRenderer component 
// that changes its color depending on some color given as an argument.
public abstract class SpriteColorAnimator {
	protected class AnimationData {
		public float t = 0;
		public float speed = 1;
		public bool completed = false;
		public Color originalColor;
		public Color newColor;
		public SpriteRenderer R;

		public AnimationData(SpriteRenderer R, Color originalColor, Color newColor, float speed) {
			this.originalColor = originalColor;
			this.newColor = newColor;
			this.speed = speed;
			this.R = R;
		}
	}

	public IEnumerator Animate(SpriteRenderer R, Color color, Lock locker = null, float speed = 1) {
		if (locker != null) {
			locker.SignalStart();
		}

		AnimationData data = new AnimationData(R, R.color, color, speed);

		while(!data.completed) {
			data.t += Time.deltaTime * data.speed * Options.Animation.spriteSpeed;
			this.NextFrame(data);
			yield return new WaitForEndOfFrame();
		}

		if (locker != null) {
			locker.SignalStop();
		}
	}

	protected abstract void NextFrame(AnimationData data);
}

public class Blinker : SpriteColorAnimator {
	override protected void NextFrame(AnimationData data) {		
		if (data.t >= 1) {
			data.completed = true;
			data.R.color = data.originalColor;
			return;
		}

		float theta = Mathf.Sin(2 * Mathf.PI * data.t);
	 	data.R.color = Color.Lerp(data.originalColor, data.newColor, theta);
	}
}



public class Fader : SpriteColorAnimator {
	override protected void NextFrame(AnimationData data) {
		if (data.t >= 1) {
			data.completed = true;
			data.R.color = data.newColor;
			return;
		}

		data.R.color = Color.Lerp(data.originalColor, data.newColor, data.t);
	}
}


public class HueSine : SpriteColorAnimator {
	private const float cutoff = 0.35f;
	private int x0 = Random.Range(0, 10);
	private int x1 = Random.Range(0, 10);
	private int x2 = Random.Range(0, 10);

	override protected void NextFrame(AnimationData data){
		if (data.t >= 1) {
			data.completed = true;
			data.R.color = data.newColor;
			return;
		}

		// First do some wacky random color shifts.
		if (data.t < 1 - cutoff) {
			data.R.color = new Color(data.newColor.r + Mathf.Sin(x0 * data.t) + Mathf.Sin(2 * x0 * data.t),
									 data.newColor.g + Mathf.Sin(x1 * data.t) + Mathf.Sin(2 * x1 * data.t),
									 data.newColor.b + Mathf.Sin(x2 * data.t) + Mathf.Sin(2 * x2 * data.t));
		}
		// Then linearly interpolate to the final color.
		else {
			data.R.color = Color.Lerp(data.originalColor, data.newColor, (data.t - (1 - cutoff)) / cutoff);	
		}
	}
}

public class Lock {
	private int lockCnt = 0;
	public bool Locked() { return this.lockCnt > 0; }
	public void SignalStart () { this.lockCnt ++; }
	public void SignalStop  () { this.lockCnt --; }
}
