using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour {

	private bool locker = false;

	void Start () {
		AudioSource player = (AudioSource) this.GetComponent<AudioSource>();
		player.pitch *= 1;
	}

	public void PlaySound (string name, float volume) {
		AudioSource player = (AudioSource) this.GetComponent<AudioSource>();
		if (!this.locker) {
			StartCoroutine(this.SetLock());
			if (Resources.Load("Music/" + name) is AudioClip) {
				player.PlayOneShot((AudioClip) Resources.Load("Music/" + name), volume);
			}
		}
	}

	private IEnumerator SetLock() {
		this.locker = true;
		yield return new WaitForSeconds(0.10f);
		this.locker = false;
	}



}
