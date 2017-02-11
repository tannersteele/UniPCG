/*
	The MIT License (MIT)
	Copyright (c) 2016 Tanner Steele

	Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
	(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
	publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
	subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
	MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
	WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using UnityEditor;

/// <summary>
/// UnityEditor Script for creating caves
/// </summary>
public class MyTools : MonoBehaviour {

	[MenuItem("MyTools/Generate Cavern Network")]
	static void Create () {
		Remove ();

		CaveGenerator.BorderingWallsAllowance = 5;
		CaveGenerator.GenerationalSmoothing = 10;
		CaveGenerator.PercentageOfWalls = 48;
		CaveGenerator.WallThresholdSize = 100;

		print(CaveGenerator.CreateCave (100));
	}

	[MenuItem("MyTools/Remove Cavern Network")]
	static void Remove() {
		foreach (var item in FindObjectsOfType<Cavern>()) {
			DestroyImmediate (item.gameObject);
		}
	}

	[MenuItem("MyTools/Save As Prefab")]
	static void Save() {
		CaveGenerator.ConvertToPrefab ();
	}
}
