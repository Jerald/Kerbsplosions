//Written by Oscar (Matrixmage)
//Logic help by Lev (CaptainLeviathan)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.IO;

namespace Kerbsplosions
{
	public class KerbsplosionsModule : PartModule
    {
        [KSPField]
        public string ExplosiveType = "LiquidFuel";

        [KSPField]
        public float DetonationVelocity = 0.0f;

        [KSPField]
        public float ExplosiveMass = 0.0f;

        [KSPField]
        public float GasDensity = 0.0f;
        
        [KSPField]
        public float ExplosionRadius = 0.0f;

        //Optional input
        [KSPField]
        public float Dispersion = 1.5f;
        //Optional input

        PartResource fuel;
        PartResource oxidizer;
        float combustibleFuel;
        float combustibleOxidizer;
        float ExplosiveMassLFO;
        float ExplosiveMassLFAtmo;

        bool overrideDetonationVelocity;
        bool overrideExplosiveMass;
        bool overrideExplosionRadius;
        bool overrideGasDensity;
        
        float rayImpulse;

        public override void OnStart(PartModule.StartState state)
        {
            part.OnJustAboutToBeDestroyed += OnExplosion;

            //Value override checks
            if (DetonationVelocity == 0.0f)
            {
                overrideDetonationVelocity = true;
            }

            if (ExplosiveMass == 0.0f)
            {
                overrideExplosiveMass = true;
            }

            if (GasDensity == 0.0f)
            {
                overrideGasDensity = true;
            }

            if (ExplosionRadius == 0.0f)
            {
                overrideExplosionRadius = true;
            }
            //Value override checks

            #region Default value loading
            if (ExplosiveType.ToLower() == "liquidfuel")
            {
                if (overrideDetonationVelocity)
                {
                    DetonationVelocity = 344;
                }

                if (overrideExplosiveMass)
                {
                    fuel = part.Resources.list.Find(testResource => testResource.info.name == "LiquidFuel");
                    oxidizer = part.Resources.list.Find(testResource => testResource.info.name == "Oxidizer");
                }

                if (overrideGasDensity)
                {
                    GasDensity = 0.134756f;
                }
            }
            else if (ExplosiveType.ToLower() == "solidfuel")
            {
                if (overrideDetonationVelocity)
                {
                    DetonationVelocity = 344;
                }

                if (overrideExplosiveMass)
                {
                    ExplosiveMass = part.GetResourceMass() + part.mass;
                }

                if (overrideGasDensity)
                {
                    GasDensity = 0.049172025f;
                }
            }
            else if (ExplosiveType.ToLower() == "tnt")
            {
                if (overrideDetonationVelocity)
                {
                    DetonationVelocity = 6900;
                }

                if (overrideExplosiveMass)
                {
                    ExplosiveMass = part.GetResourceMass() + part.mass;
                }

                if (overrideGasDensity)
                {
                    GasDensity = 0.79865333f;
                }
            }
            #endregion
        }

        float ImpulseCalc(float aExplosiveMass, float aDetonationVelocity, float aGasDensity, float numberOfRays)
        {
            //Singular ray impulse formula
            return ((((aExplosiveMass * aDetonationVelocity) / aGasDensity) *
                (aGasDensity +
                ((float)FlightGlobals.getAtmDensity(FlightGlobals.getStaticPressure(vessel.GetWorldPos3D()),
                FlightGlobals.getExternalTemperature(vessel.GetWorldPos3D()))))) /
                numberOfRays);
            //Singular ray impulse formula
        }

        void OnExplosion()
        {
            //Debug.Log("ExplosiveType: " + ExplosiveType);
            //Debug.Log("DetonationVelocity: " + DetonationVelocity);
            //Debug.Log("ExplosiveMass: " + ExplosiveMass);
            //Debug.Log("GasDensity: " + GasDensity);
            //Debug.Log(overrideDetonationVelocity);
            //Debug.Log(overrideExplosionRadius);
            //Debug.Log(overrideExplosiveMass);
            //Debug.Log(overrideGasDensity);

            //ExplosionMass logic for Liquid Fuel
            if (ExplosiveType == "LiquidFuel" && overrideExplosiveMass)
            {
                if (fuel != null)
                {
                    ExplosiveMassLFAtmo = (float)fuel.amount * 5;

                    if (oxidizer != null)
                    {
                        combustibleFuel = Mathf.Min((float)fuel.amount, ((float)oxidizer.amount * (0.9f / 1.1f)));
                        combustibleOxidizer = combustibleFuel * (1.1f / 0.9f);

                        ExplosiveMassLFO = (combustibleFuel + combustibleOxidizer) * 5;
                        ExplosiveMassLFAtmo = ((float)fuel.amount - combustibleFuel) * 5;
                    }
                }
            }
            //ExplosionMass logic for Liquid Fuel

            //Default explosionRadius calculation
            if (overrideExplosionRadius)
            {
                ExplosionRadius = Mathf.Pow((ExplosiveMass / GasDensity) / ((4 / 3) * Mathf.PI), 1 / 3) * 10;
            }
            //Default explosionRadius calculation

            //Collider List definition and null check
            List<Collider> collidersToBoom = new List<Collider>(Physics.OverlapSphere(part.transform.position, ExplosionRadius));

            collidersToBoom = collidersToBoom.Where(testCollider => testCollider != null &&
                testCollider.gameObject.GetComponentInParent<Part>() != null &&
                testCollider.gameObject.GetComponentInParent<Part>().vessel != null).ToList();
            //Collider List definition and null check

            //Vessel array definition and filling
            VesselBox[] vesselBoxesToBoom = new VesselBox[collidersToBoom.Count];
            for (int i = 0; i < collidersToBoom.Count; ++i)
            {
                vesselBoxesToBoom[i] = Boxifier.GetVesselBoundingBox(collidersToBoom[i].gameObject.GetComponentInParent<Part>().vessel);
            }
            //Vessel array definition and filling

            //Ray generation
            Vector3[] ray360Output = RayGo.RayThreeSixty(Dispersion, ExplosionRadius);
            Ray[] rays = RayGo.getRays(part.transform.position, vesselBoxesToBoom, ray360Output);
            //Ray generation

            //RaycastHit layered array definition and ray casting
            RaycastHit[][] soManyRaycastHits = new RaycastHit[rays.Length][];
            for (int i = 0; i < rays.Length; ++i)
            {
                soManyRaycastHits[i] = Physics.RaycastAll(rays[i], ExplosionRadius);
            }
            //RaycastHit layered array definition and ray casting

            //Impulse calculation
            if (ExplosiveMass == 0.0F && (ExplosiveMassLFAtmo > 0.0F || ExplosiveMassLFO > 0.0F))
            {
                rayImpulse = ImpulseCalc(ExplosiveMassLFAtmo, DetonationVelocity * 0.2f, GasDensity, ray360Output.Length) + ImpulseCalc(ExplosiveMassLFO, DetonationVelocity, GasDensity, ray360Output.Length);
            }
            else if (ExplosiveMass > 0.0F)
            {
                rayImpulse = ImpulseCalc(ExplosiveMass, DetonationVelocity, GasDensity, ray360Output.Length);
            }
            else
            {
                rayImpulse = 0f;
                Debug.Log("[Kerbsplosions] Part " + part.name + " has no ray impulse");
            }
            //Impulse calculation

            Debug.Log("[Kerbsplosions] Singular ray impulse of part " + part.name + " is " + rayImpulse);

            foreach (RaycastHit[] explosiveRay in soManyRaycastHits)
            {
                for (int i = 0; i < explosiveRay.Length; ++i)
                {
                    //IntegrityModule grabber and null checker
                    IntegrityModule moduleReference = explosiveRay[i].transform.gameObject.GetComponentInParent<IntegrityModule>();
                    if (moduleReference == null || explosiveRay[i].rigidbody == null)
                    {
                        return;
                    }
                    //IntegrityModule grabber and null checker

                    explosiveRay[i].rigidbody.AddForceAtPosition(rays[i].direction * rayImpulse, explosiveRay[i].point, ForceMode.Impulse);

                    //Damage dealing and out-of-energy check
                    if (moduleReference.currentIntegrity < rayImpulse)
                    {
                        rayImpulse -= moduleReference.currentIntegrity;
                        moduleReference.currentIntegrity = 0;
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

