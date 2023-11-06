using System;
using TaleWorlds.Library;

namespace TaleWorlds.Library
{
    public static class Vec3Extensions
    {
        public static string GetDirectionString(this Vec3 sourcePosition, Vec3 targetPosition)
        {
            float playerX = sourcePosition.X;
            float playerY = sourcePosition.Y;
			float yDif = targetPosition.Y - playerY;
			float xDif = targetPosition.X - playerX;
			double directionDegrees = Math.Atan2(xDif, yDif) / Math.PI * 180;
			if (directionDegrees < 0)
			{
				directionDegrees += 360;
			}
			string direction;
			switch (directionDegrees)
			{
				case double n when n > 348.75 || n <= 11.25: //0 degrees
					direction = "N";
					break;
				case double n when n <= 33.75: //22.5 degrees
					direction = "NNE";
					break;
				case double n when n <= 56.25: //45 degrees
					direction = "NE";
					break;
				case double n when n <= 78.75: //67.5 degrees
					direction = "ENE";
					break;
				case double n when n <= 101.25: //90 degrees
					direction = "E";
					break;
				case double n when n <= 123.75: //112.5 degrees
					direction = "ESE";
					break;
				case double n when n <= 146.25: //135 degrees
					direction = "SE";
					break;
				case double n when n <= 168.75: //157.5 degrees
					direction = "SSE";
					break;
				case double n when n <= 191.25: //180 degrees
					direction = "S";
					break;
				case double n when n <= 213.75: //202.5 degrees
					direction = "SSW";
					break;
				case double n when n <= 236.25: //225 degrees
					direction = "SW";
					break;
				case double n when n <= 258.75: //247.5 degrees
					direction = "WSW";
					break;
				case double n when n <= 281.25: //270 degrees
					direction = "W";
					break;
				case double n when n <= 303.75: //292.5 degrees
					direction = "WNW";
					break;
				case double n when n <= 326.25: //315 degrees
					direction = "NW";
					break;
				case double n when n <= 348.75: //337.5 degrees
					direction = "NNW";
					break;
				default:
					direction = $"UNKNOWN direction from {sourcePosition} to {targetPosition}";
					break;
			}
			return direction;
		}
    }
}
