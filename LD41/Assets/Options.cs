using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Options {
	public static class Animation {
		public static bool animateMovement    = true;
		public static bool animateBump 		  = true;
		public static bool lockDuringMovement = true;
		public static bool lockDuringSprite   = true;

		public static float movementSpeed 	  = 10f;
		public static float spriteSpeed 	  = 2f;
	}

	public static class KeyBinds {
		public static KeyCode up    		= KeyCode.W;
		public static KeyCode down  		= KeyCode.S;
		public static KeyCode left  		= KeyCode.A;
		public static KeyCode right 		= KeyCode.D;

		public static KeyCode action0 		= KeyCode.Space;
		public static KeyCode action1 		= KeyCode.Alpha1;
		public static KeyCode action2 		= KeyCode.Alpha2;
		public static KeyCode action3 		= KeyCode.Alpha3;
		public static KeyCode action4 		= KeyCode.Alpha4;

		public static KeyCode showMovements = KeyCode.Tab;
	}
}