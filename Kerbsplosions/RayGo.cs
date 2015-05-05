//Written by Lev (CaptainLeviathan)
//Mostly edited by Oscar (Matrixmage)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kerbsplosions
{
	public struct Box3D
	{
		public Vector3[] vertices;
		public Vector3 center;

		public Box3D(Vector3[] aVertices)
		{
			vertices = aVertices;
            center = (aVertices[0] + aVertices[1] + aVertices[2] + aVertices[3] + aVertices[4] + aVertices[5] + aVertices[6] + aVertices[7]) / 8;
		}
	}
	
	public struct RayGoZone
	{
		public Vector3 direction;
		public float angle;
		
		public RayGoZone(Vector3 aDirection, float aAngle)
		{
			direction = aDirection;
			angle = aAngle;
		}
	}
	
	public struct VesselBox
	{
		public Box3D vesselBox;
		public Box3D[] partBoxes;
		
		public VesselBox(Box3D aVessleBox, Box3D[] aPartBoxes)
		{
			vesselBox = aVessleBox;
			partBoxes = aPartBoxes;
		}
	}
	
	public static class RayGo
	{

		static RayGoZone Box3DToRayGoZone(Box3D box3D, Vector3 origin)
		{
			return new RayGoZone(Vector3.Normalize(box3D.center - origin), BiggestAngle(origin, box3D.center, box3D.vertices));
		}
		
		static float BiggestAngle(Vector3 a, Vector3 b, Vector3[] c)
		{
			float[] angles = new float[c.Length];
			
			for (int i = 0; i < c.Length; ++i)
			{
				angles[i] = CosLawAngleCalc(a, b, c[i]);
			}
			
			return Mathf.Max(angles);
		}
		
		static float CosLawAngleCalc(Vector3 pointA, Vector3 pointB, Vector3 pointC)
		{
			float sideA = Vector3.Distance(pointB, pointC);
			float sideB = Vector3.Distance(pointA, pointC);
			float sideC = Vector3.Distance(pointA, pointB);
			
			float angleA = Mathf.Acos((Mathf.Pow(sideB, 2) + Mathf.Pow(sideC, 2) - Mathf.Pow(sideA, 2)) / (2 * sideB * sideC));
			return angleA;
		}
		
		public static Vector3[] RayThreeSixty(float rayDispersion, float radius)
		{
			Matrix4x4 rotaterY;
			Matrix4x4 rotaterZ;
			
			List<Vector3> outPut = new List<Vector3>();
			Vector3 newLayerPoint;
			
			float rotateY = (rayDispersion / radius);
			float rotateZ = 0f;

			float rotationY = rotateY;
			float rotationZ = 0f;
			outPut.Add(Vector3.forward);
			
			for (int i = 1; rotationY < Mathf.PI; ++i)
			{
				rotateY = (rayDispersion / radius);
				rotaterY = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, rotateY * 57.2957795f * i, 0f), new Vector3(1f, 1f, 1f));
				newLayerPoint = rotaterY.MultiplyPoint3x4(Vector3.forward);
				outPut.Add(newLayerPoint);
				rotateZ = ((rayDispersion / radius)/Mathf.Sin(rotateY * i));
				for (int k = 1; rotationZ < 2 * Mathf.PI; ++k)
				{
					rotaterZ = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, rotateZ * 57.2957795f * k), new Vector3(1f, 1f, 1f));
					outPut.Add(rotaterZ.MultiplyPoint3x4(newLayerPoint));
					rotationZ += rotateZ;
				}
				rotationZ = rotateZ;
				rotationY += rotateY;
			}
			
			return outPut.ToArray();
		}
		
		static Ray[] RayPruner(RayGoZone[] rayGoZones, Vector3[] directions, Vector3 origin)
		{
			Ray[] output;
			List<Vector3> prunedDirections = new List<Vector3>();
			
			for (int i = 0; i < directions.Length; ++i)
			{
				for (int k = 0; k < rayGoZones.Length; ++k)
				{
					if ((2f * Mathf.Asin(Vector3.Distance(directions[i], rayGoZones[k].direction) / 2) < rayGoZones[k].angle))
					{
						prunedDirections.Add(directions[i]);
					}
				}
			}
			
			output = new Ray[prunedDirections.Count];
			for (int i = 0; i < prunedDirections.Count; ++i)
			{
				output[i].direction = prunedDirections[i];
				output[i].origin = origin;
			}
			
			return output;
		}
		
		public static Ray[] getRays(Vector3 origin, VesselBox[] vesselBoxs, Vector3[] rayThreeSixtyOutput)
		{
			List<RayGoZone> rayGoZones = new List<RayGoZone>();
			RayGoZone vesselRayGoZone;
			
			for (int i = 0; i < vesselBoxs.Length; ++i)
			{
				vesselRayGoZone = Box3DToRayGoZone(vesselBoxs[i].vesselBox, origin);
				
				if (vesselRayGoZone.angle > Mathf.PI / 2f)
				{
					for (int k = 0; k < vesselBoxs[i].partBoxes.Length; ++k)
					{
						rayGoZones.Add(Box3DToRayGoZone(vesselBoxs[i].partBoxes[k], origin));
					}
				}
				else
				{
					rayGoZones.Add(vesselRayGoZone);
				}
			}
			
			return RayPruner(rayGoZones.ToArray(), rayThreeSixtyOutput, origin);
		}
	}
}

