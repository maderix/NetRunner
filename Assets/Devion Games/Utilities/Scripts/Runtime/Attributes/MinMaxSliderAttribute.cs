﻿using System;
using UnityEngine;

namespace DevionGames
{
	[System.Serializable]
	public class MinMaxSliderAttribute : PropertyAttribute
	{

		public readonly float max;
		public readonly float min;

		public MinMaxSliderAttribute (float min, float max)
		{
			this.min = min;
			this.max = max;
		}
	}
}