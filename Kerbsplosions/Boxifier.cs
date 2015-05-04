//Written by Oscar (Matrixmage)
//Semi-inspired by some code from Ferram4

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

namespace Kerbsplosions
{
	public static class Boxifier
	{
		public static Vector3Pair GetPartBoundingBox(Part part)
		{
            Vector3 maxBounds = new Vector3();
            Vector3 minBounds = new Vector3();

            foreach (Collider collider in part.GetComponentsInChildren<Collider>())
            {
                //Null collider check
                if (collider == null)
                {
                    continue;
                }
                //Null collider check

                //Part bound calculation
                maxBounds.x = Mathf.Max(maxBounds.x, collider.bounds.max.x);
                maxBounds.y = Mathf.Max(maxBounds.y, collider.bounds.max.y);
                maxBounds.z = Mathf.Max(maxBounds.z, collider.bounds.max.z);

                minBounds.x = Mathf.Min(minBounds.x, collider.bounds.min.x);
                minBounds.y = Mathf.Min(minBounds.y, collider.bounds.min.y);
                minBounds.z = Mathf.Min(minBounds.z, collider.bounds.min.z);
                //Part bound calculation
            }

            return new Vector3Pair(maxBounds, minBounds);
		}

		public static VesselBox GetVesselBoundingBox(Vessel vessel)
		{
			Vector3[] outputVertices = new Vector3[8];
            Box3D[] partBoxArray = new Box3D[vessel.Parts.Count];

			Vector3 maxBounds = new Vector3();
			Vector3 minBounds = new Vector3();

			for (int i = 0; i < vessel.parts.Count; ++i)
			{
                //Null part check
                if (vessel.parts[i] == null)
                {
                    continue;
                }
                //Null part check

				Vector3Pair partBoundingBox = GetPartBoundingBox(vessel.parts[i]);

                //Vessel bound computation
				maxBounds.x = Mathf.Max(maxBounds.x, partBoundingBox.p1.x);
				maxBounds.y = Mathf.Max(maxBounds.y, partBoundingBox.p1.y);
				maxBounds.z = Mathf.Max(maxBounds.z, partBoundingBox.p1.z);

				minBounds.x = Mathf.Min(minBounds.x, partBoundingBox.p2.x);
				minBounds.y = Mathf.Min(minBounds.y, partBoundingBox.p2.y);
				minBounds.z = Mathf.Min(minBounds.z, partBoundingBox.p2.z);
                //Vessel bound computation

                //Part vertices computation
                partBoxArray[i].vertices[0] = new Vector3(partBoundingBox.p1.x, partBoundingBox.p1.y, partBoundingBox.p1.z);
                partBoxArray[i].vertices[1] = new Vector3(partBoundingBox.p1.x, partBoundingBox.p1.y, partBoundingBox.p2.z);
                partBoxArray[i].vertices[2] = new Vector3(partBoundingBox.p1.x, partBoundingBox.p2.y, partBoundingBox.p2.z);

                partBoxArray[i].vertices[3] = new Vector3(partBoundingBox.p2.x, partBoundingBox.p2.y, partBoundingBox.p2.z);
                partBoxArray[i].vertices[4] = new Vector3(partBoundingBox.p2.x, partBoundingBox.p2.y, partBoundingBox.p1.z);
                partBoxArray[i].vertices[5] = new Vector3(partBoundingBox.p2.x, partBoundingBox.p1.y, partBoundingBox.p1.z);

                partBoxArray[i].vertices[6] = new Vector3(partBoundingBox.p1.x, partBoundingBox.p2.y, partBoundingBox.p1.z);
                partBoxArray[i].vertices[7] = new Vector3(partBoundingBox.p2.x, partBoundingBox.p1.y, partBoundingBox.p2.z);
                //Part vertices computation
			}

            //Vessel vertices computation
			outputVertices[0] = new Vector3(maxBounds.x, maxBounds.y, maxBounds.z);
			outputVertices[1] = new Vector3(maxBounds.x, maxBounds.y, minBounds.z);
			outputVertices[2] = new Vector3(maxBounds.x, minBounds.y, minBounds.z);
			
			outputVertices[3] = new Vector3(minBounds.x, minBounds.y, minBounds.z);
			outputVertices[4] = new Vector3(minBounds.x, minBounds.y, maxBounds.z);
			outputVertices[5] = new Vector3(minBounds.x, maxBounds.y, maxBounds.z);
			
			outputVertices[6] = new Vector3(maxBounds.x, minBounds.y, maxBounds.z);
			outputVertices[7] = new Vector3(minBounds.x, maxBounds.y, minBounds.z);
            //Vessel vertices computation
			
            return new VesselBox(new Box3D(outputVertices), partBoxArray);
		}

	}
}