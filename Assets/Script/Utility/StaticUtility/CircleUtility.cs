using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{

    public struct CirclePoint
    {
        public Vector3 thePoint;
        public Vector3 prevPoint;

        public CirclePoint(Vector3 _thePoint, Vector3 _prevPoint)
        {
            thePoint = _thePoint;
            prevPoint = _prevPoint;
        }
    }

    public class CircleUtility
    {
        public static CirclePoint FindPointOnCircle(Vector3 c, float r, Vector3[] path)
        {

            Vector3 Start = c;
            Vector3 End = c;
            float dist = 0;

            // Find segement that intersect circle
            foreach (Vector3 point in path)
            {
                End = point;
                    
                dist = Vector3.Distance(c, End);

                if(dist > r)
                {
                    break;
                }
                Start = point; 
            }

            if(dist < r)
            {
                return new CirclePoint(End, c);
            }

            // Calculate the determinate
            var _dX = Start.x - End.x;
            var _dZ = Start.z - End.z;
            
            var A = _dX * _dX + _dZ * _dZ;
            var B = 2* (_dX * (Start.x - c.x) + _dZ * ( Start.z - c.z));
            var C = (Start.x - c.x) * (Start.x - c.x) +
                    (Start.z - c.z) * (Start.z - c.z) -
                    r * r;
            
            var determinate = B * B - 4 * A * C;


            // We know that the line intersects the circle so
            // there will always be a solution.
            if(determinate == 0) {
                return new CirclePoint(Start, End);
            } else {
                var det_sqrt = Math.Sqrt(determinate);
                var t = (-B + det_sqrt) / (2 * A);
                var point1 = new Vector3((float)(Start.x + t * _dX), Start.y, (float)(Start.z + t * _dZ));
                t = (-B - det_sqrt) / (2 * A);
                var point2 = new Vector3((float)(Start.x + t * _dX), Start.y, (float)(Start.z + t * _dZ));

                var dist1 = Vector3.Distance(point1, End);
                var dist2 = Vector3.Distance(point2, End);

                var CloserPoint = (dist1 < dist2) ? point2 : point1;
                return new CirclePoint(CloserPoint, Start);
            }
        }

    }
}