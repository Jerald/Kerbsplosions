//Written by Oscar (Matrixmage)

using System;
using UnityEngine;
using KSP.IO;

namespace Kerbsplosions
{
    public class IntegrityModule : PartModule
    {
        [KSPField]
        public float Integrity;

        //Non-input KSPFields
        [KSPField(guiActive = true, guiName = "Integrity", guiUnits = "%", guiFormat = "0.00")]
        public float integrityPercentage;

        [KSPField(isPersistant = true)]
        public float currentIntegrity = 0f;
        //Non-input KSPFields

        void Start()
        {
            if (Integrity == 0.0F)
            {
                Integrity = part.crashTolerance;
            }

            currentIntegrity = Integrity;
        }

        void Update()
        {
            if (Integrity != 0.0F)
            {
                integrityPercentage = ((currentIntegrity / Integrity) * 100);
            }

            //Debug.Log("[Kerbsplosions] Integrity: " + Integrity);
            //Debug.Log("[Kerbsplosions] Current Integrity: " + currentIntegrity);

            //Part death
            if (currentIntegrity <= 0)
            {
                //part.explode();
                Destroy(part);
            }
            //Part death
        }
    }
}
