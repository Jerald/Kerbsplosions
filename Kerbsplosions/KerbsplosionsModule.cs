using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

namespace Kerbsplosions
{
	public class KerbsplosionsModule : PartModule
    {
        #region Config input KSPFields
        //Where applicable, default values have been set to those for kerosene
        [KSPField]
        public float detonationVelocity = 400;

        [KSPField]
        public float massOfExplosive;
        
        [KSPField]
        public float explosionRadius;

		[KSPField]
		public float integrity;

        //Optional input
        [KSPField]
        public float dispersion = 1.5f;

        [KSPField]
        public float gasDensityFudgeNumber = 0.134756f;
        //Optional input
        //Where applicable, default values have been set to those for kerosene
        #endregion

        #region Non-input KSPFields
        [KSPField(guiActive = true, guiName = "Integrity", guiUnits = "%", guiFormat = "0.00")]
        public float integrityPercentage;
        
        [KSPField(isPersistant = true)]
        float currentIntegrity;
        #endregion

        PartResource fuel;
        PartResource oxidizer;
        bool lockedExplosiveMass;
        float rayImpulse;
        float combustibleFuel;
        float combustibleOxidizer;

        void Start()
        {
            part.OnJustAboutToBeDestroyed += OnExplosion;

            if (massOfExplosive == 0.0F)
            {
                //Thanks to darklight for the linq stuff!
                fuel = part.Resources.list.Find(testResource => testResource.info.name == "LiquidFuel");
                oxidizer = part.Resources.list.Find(testResource => testResource.info.name == "Oxidizer");
                //Thanks to darklight for the linq stuff!

                if (fuel == null)
                {
                    lockedExplosiveMass = true;
                }
            }
            else
            {
                lockedExplosiveMass = true;
            }

            if (integrity == 0.0F)
            {
                integrity = part.crashTolerance;
            }
        }

        void Update()
		{
            integrityPercentage = ((currentIntegrity / integrity) * 100);

            if (!lockedExplosiveMass)
            {
                massOfExplosive = ((float)part.Resources["LiquidFuel"].amount * 0.005f) / 1000;
            }

            //Part death
			if (currentIntegrity <= 0)
			{
				part.explode();
			}
            //Part death
		}

        void OnExplosion()
        {
            if (fuel != null)
            {
                if (oxidizer != null)
                {
                    combustibleFuel = Mathf.Min((float)fuel.amount, ((float)oxidizer.amount * (0.9f / 1.1f)));
                    combustibleOxidizer = combustibleFuel * (1.1f / 0.9f);

                    massOfExplosive = (combustibleFuel + combustibleOxidizer) * 5;
                }
            }

            //Collider List definition and null check
            List<Collider> collidersToBoom = new List<Collider>(Physics.OverlapSphere(part.transform.position, explosionRadius));
            for (int i = collidersToBoom.Count - 1; i >= 0; i--)
            {
                if (collidersToBoom[i].gameObject.GetComponentInParent<Part>() == null)
                {
                    collidersToBoom.Remove(collider);
                }
            }
            //Collider List definition and null check

            //Vessel array definition and filling
            VesselBox[] vesselBoxesToBoom = new VesselBox[collidersToBoom.Count];
            for (int i = 0; i < collidersToBoom.Count; ++i)
            {
                vesselBoxesToBoom[i] = Boxifier.GetVesselBoundingBox(collidersToBoom[i].gameObject.GetComponentInParent<Part>().vessel);
            }
            //Vessel array definition and filling

            Vector3[] ray360Output = RayGo.RayThreeSixty(dispersion, explosionRadius);
            Ray[] rays = RayGo.getRays(part.transform.position, vesselBoxesToBoom, ray360Output);

            //RaycastHit layered array definition and ray casting
            RaycastHit[][] soManyRaycastHits = new RaycastHit[rays.Length][];
            for (int i = 0; i < rays.Length; ++i)
            {
                soManyRaycastHits[i] = Physics.RaycastAll(rays[i], explosionRadius);
            }
            //RaycastHit layered array definition and ray casting

            
            //Formula for singular ray impulse
            if (massOfExplosive > 0f)
            {
                rayImpulse = ((((massOfExplosive * detonationVelocity) / gasDensityFudgeNumber) *
                    (gasDensityFudgeNumber +
                    ((float)FlightGlobals.getAtmDensity(FlightGlobals.getStaticPressure(vessel.GetWorldPos3D()), FlightGlobals.getExternalTemperature(vessel.GetWorldPos3D())) *
                    (float)FlightGlobals.getStaticPressure(vessel.GetWorldPos3D())))) / ray360Output.Length);
            }
            else
            {
                rayImpulse = 0f;
            }
            //Formula for singular ray impulse

            Debug.Log("[Kerbsplosions] Singular ray impulse: " + rayImpulse);

            foreach (RaycastHit[] explosiveRay in soManyRaycastHits)
            {
                for (int i = 0; i < explosiveRay.Length; ++i)
                {
                    //KerbsplosionsModule grabber and null checker
                    var moduleReference = explosiveRay[i].transform.gameObject.GetComponentInParent<KerbsplosionsModule>();
                    if (moduleReference == null || explosiveRay[i].rigidbody == null)
                    {
                        return;
                    }
                    //KerbsplosionsModule grabber and null checker

                    explosiveRay[i].rigidbody.AddForceAtPosition(rays[i].direction * rayImpulse, explosiveRay[i].point, ForceMode.Impulse);

                    //Damage dealing and out-of-energy check
                    if (moduleReference.currentIntegrity < rayImpulse)
                    {
                        rayImpulse -= moduleReference.currentIntegrity;
                        currentIntegrity = 0;
                    }
                    else
                    {
                        moduleReference.currentIntegrity -= rayImpulse;
                        break;
                    }
                    //Damage dealing and out-of-energy check
                }
            }

            part.OnJustAboutToBeDestroyed -= OnExplosion;
        }
	}
}

