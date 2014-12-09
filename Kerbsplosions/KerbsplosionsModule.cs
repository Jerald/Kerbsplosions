using System;
using System.Collections;
using UnityEngine;
using KSP.IO;

namespace Kerbsplosions
{
	public class KerbsplosionsModule : PartModule
	{
		[KSPField]
		public float explosionForce;

		[KSPField]
		public float explosionRadius;

		[KSPField(isPersistant = true, guiActive = true, guiName = "Integrity")]
		public float integrity;

		void Update()
		{
			if (integrity <= 0)
			{
				part.explode();
				
			}
		}
	}
}

