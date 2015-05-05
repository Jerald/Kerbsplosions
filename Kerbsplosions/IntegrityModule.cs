//Written by Oscar (Matrixmage)

using System;
using UnityEngine;
using KSP.IO;

namespace Kerbsplosions
{
    public class IntegrityModule : PartModule
    {
        [KSPField]
        public float Integrity = 0.0f;

        //Optional input
        [KSPField]
        public float IntegrityModifier = 1;
        //Optional input

        //Non-input KSPFields
        [KSPField(guiActive = true, guiName = "Integrity", guiUnits = "%", guiFormat = "0.00")]
        public float integrityPercentage = 0.0f;

        [KSPField(isPersistant = true)]
        public float currentIntegrity = 0.0f;
        //Non-input KSPFields

        public override void OnStart(PartModule.StartState state)
        {
            if (Integrity == 0.0f)
            {
                Integrity = part.crashTolerance * IntegrityModifier;
            }

            currentIntegrity = Integrity;
        }

        void Update()
        {
            if (Integrity != 0.0f)
            {
                integrityPercentage = ((currentIntegrity / Integrity) * 100);
            }

            //Part death
            if (currentIntegrity <= 0)
            {
                Destroy(part);
            }
            //Part death
        }
    }
}
