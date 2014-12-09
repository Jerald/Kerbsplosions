using System;
using System.Collections;
using UnityEngine;
using KSP.IO;


namespace Kerbsplosions
{
	public class KerbsplosionsPart : Part
	{
		Ray ray = new Ray();
		RaycastHit[] raycastHits;
		
		Vector3 direction;
		Quaternion xRotation;
		Quaternion zRotation;

		float degreesToMove;
		
		float explosionForce;
		float explosionRadius = 20;
		
		int steps = 36;

		protected override void onFlightStart()
		{
			//MonoBehaviour.print("onFlightStart");

			ray = new Ray();

			raycastHits = new RaycastHit[0]; 
			
			degreesToMove = Mathf.Floor(360/steps);
			
			direction = Vector3.right;
			xRotation = Quaternion.Euler(Vector3.right * degreesToMove);
			zRotation = Quaternion.Euler(Vector3.forward * degreesToMove);
			
			if(Modules.Contains("KerbsplosionsModule"))
			{
				explosionForce = Modules["KerbsplosionsModule"].Fields.GetValue<float>("explosionForce");
				explosionRadius = Modules["KerbsplosionsModule"].Fields.GetValue<float>("explosionRadius");
			}
		}

		protected override void onPartExplode()
		{
			ray.origin = transform.position;
			
			for (int i = 0; i < (steps/2)-1; ++i)
			{
				direction = zRotation * direction;
				for (int j =0; j < steps; ++j)
				{
					direction = xRotation * direction;
					raycastHits = Physics.RaycastAll(ray.origin, direction, explosionRadius);
					
					for (int k = 0; k < raycastHits.Length; ++k)
					{
						float localExplosionForce = explosionForce;

						try
						{
							raycastHits[k].rigidbody.AddForceAtPosition(direction.normalized * localExplosionForce, raycastHits[k].point, ForceMode.Impulse);

							if (raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity") < localExplosionForce)
							{
								raycastHits[k].collider.gameObject.GetComponentInParent<Part>().explode();
								localExplosionForce -= raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity");
							}
							else
							{

								float newIntegrity = raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity") - localExplosionForce;
								localExplosionForce -= raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity");
								raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.SetValue<float>("integrity", newIntegrity);
							}

							if (localExplosionForce <=  0f) { break; }
						}
						catch (Exception e)
						{
							//Debug.Log("Kerbsplosions has made an exception in onPartExplode. Exception: " + e);
						}
					}
				}
			}

			#region Right ray (needed becuase sphere casting code doesn't send a ray right or left)
			direction = Vector3.right;
			raycastHits = Physics.RaycastAll(ray.origin, direction, explosionRadius);
			for (int k = 0; k < raycastHits.Length; ++k)
			{
				float localExplosionForce = explosionForce;
				
				try
				{
					raycastHits[k].rigidbody.AddForceAtPosition(direction.normalized * localExplosionForce, raycastHits[k].point, ForceMode.Impulse);
					
					if (raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity") < localExplosionForce)
					{
						raycastHits[k].collider.gameObject.GetComponentInParent<Part>().explode();
						localExplosionForce -= raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity");
					}
					else
					{
						
						float newIntegrity = raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity") - localExplosionForce;
						localExplosionForce -= raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity");
						raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.SetValue<float>("integrity", newIntegrity);
					}
					
					if (localExplosionForce <=  0f) { break; }
				}
				catch (Exception e)
				{
					//Debug.Log("Kerbsplosions has made an exception in onPartExplode. Exception: " + e);
				}
			}
			#endregion

			#region Left ray (needed becuase sphere casting code doesn't send a ray right or left)
			direction = Vector3.left;
			raycastHits = Physics.RaycastAll(ray.origin, direction, explosionRadius);
			for (int k = 0; k < raycastHits.Length; ++k)
			{
				float localExplosionForce = explosionForce;
				
				try
				{
					raycastHits[k].rigidbody.AddForceAtPosition(direction.normalized * localExplosionForce, raycastHits[k].point, ForceMode.Impulse);
					
					if (raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity") < localExplosionForce)
					{
						raycastHits[k].collider.gameObject.GetComponentInParent<Part>().explode();
						localExplosionForce -= raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity");
					}
					else
					{
						
						float newIntegrity = raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity") - localExplosionForce;
						localExplosionForce -= raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.GetValue<float>("integrity");
						raycastHits[k].collider.gameObject.GetComponentInParent<Part>().Modules["KerbsplosionsModule"].Fields.SetValue<float>("integrity", newIntegrity);
					}
					
					if (localExplosionForce <=  0f) { break; }
				}
				catch (Exception e)
				{
					//Debug.Log("Kerbsplosions has made an exception in onPartExplode. Exception: " + e);
				}
			}
			#endregion
		}
	}
	
}


