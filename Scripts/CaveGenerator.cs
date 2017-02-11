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
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generates 
/// </summary>
public sealed class CaveGenerator : MonoBehaviour {
	
	public static GameObject physicalMatrix;
	public static int GenerationalSmoothing { get; set; }
	public static int BorderingWallsAllowance { get; set; }
	public static int PercentageOfWalls { get; set; }
	public static int WallThresholdSize { get; set; }
	public static int mapSize;

	private static int[,] matrix;

	public void Start()
	{
		GenerationalSmoothing = 10;
		BorderingWallsAllowance = 5;
		PercentageOfWalls = 48;
		WallThresholdSize = 100;
	}

	/// <summary>
	/// Converts this object into a prefab
	/// </summary>
	public static void ConvertToPrefab()
	{
		Object emptyObj = PrefabUtility.CreateEmptyPrefab ("Assets/Resources/" + Time.time + ".prefab");
		PrefabUtility.ReplacePrefab(physicalMatrix, emptyObj, ReplacePrefabOptions.Default);
	}

	/// <summary>
	/// Creates the cave.
	/// </summary>
	/// <returns>GameObject version of the cave to be converted to a prefab.</returns>
	/// <param name="size">Size.</param>
	/// <param name="wallSprite">Wall sprite.</param>
	public static GameObject CreateCave(int size)
	{
		//pick random seed and pass to overloaded
		return CreateCave(size, 0);
	}

	/// <summary>
	/// Creates cave with a fixed seed.
	/// </summary>
	/// <returns>GameObject version of the cave to be converted to a prefab.</returns>
	/// <param name="size">Size.</param>
	/// <param name="seed">Seed.</param>
	/// <param name="wallSprite">Wall sprite.</param>
	public static GameObject CreateCave(int size, int seed)
	{
		GameObject wallSprite = (GameObject)Resources.Load ("WallSprite", typeof(GameObject));

		RemoveExistingCavern ();
		
		matrix = new int[size,size];
		mapSize = size;

		//If we have a seed, seed it. If not, generate one.
		print(SeedRandomNumberGenerator(seed));

		//generate initial "logical" matrix to be later converted into physical
		GenerateLogicalMatrix();
		GeneratePhysicalMatrix (wallSprite);

		return physicalMatrix;
	}

	/// <summary>
	/// Seeds the random number generator.
	/// </summary>
	/// <returns>The seed.</returns>
	/// <param name="seed">Seed.</param>
	private static string SeedRandomNumberGenerator(int seed)
	{
		//if the seed is unique - set it
		if (seed == null || seed != 0)
			Random.seed = seed;
		else
			Random.seed = (int)System.DateTime.Now.Ticks;

		return ("Current generated cave seed: " + Random.seed);
	}

	/// <summary>
	/// Generates the physical matrix.
	/// </summary>
	/// <param name="wallSprite">Wall sprite.</param>
	private static void GeneratePhysicalMatrix(GameObject wallSprite)
	{
		GameObject cellContainer;
		Vector3 spawnLocation = new Vector3(0,0,0);

		//parent object to hold individual cells as collecction
		physicalMatrix = new GameObject ("Cavern");
		physicalMatrix.AddComponent<Cavern> ();

		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				//if it's a wall, make a game object
				if (matrix [i, j] == 0) {
					cellContainer = (GameObject)Instantiate (wallSprite, new Vector3 (i, j, 0), Quaternion.identity);
					cellContainer.name = "[" + i + ", " + j + "]";
					cellContainer.transform.parent = physicalMatrix.transform;
				} else {
					spawnLocation = (new Vector3 (i, j, 0));
				}
			}
		}

		if(GameManager._instance != null)
			GameManager._instance.SpawnCharacter (spawnLocation);
	}

	/// <summary>
	/// Removes the existing cavern.
	/// </summary>
	private static void RemoveExistingCavern()
	{
		foreach (var cave in FindObjectsOfType<Cavern>()) {
			DestroyImmediate (cave.gameObject);
		}
	}

	/// <summary>
	/// Generates the logical matrix.
	/// </summary>
	private static void GenerateLogicalMatrix()
	{		
		for (int i = 0; i < mapSize; i++) {
			for (int j = 0; j < mapSize; j++) {
				//generating arbitrary noise to work with for cellular automata
				if (Random.Range(0,100) > PercentageOfWalls) 
					matrix [i, j] = 0;
				else 
					matrix [i, j] = 1;
			}
		}

		GenerateCaverns ();
		AddUnitBorder ();
		RemoveUndersizedRegions ();
	}

	/// <summary>
	/// Generates caverns within the noise by using a generational smoothing system.
	/// User can specify how many generations of this process should happen by configuring generationalSmoothing variable.
	/// Automaton logic happens here as well.
	/// </summary>
	private static void GenerateCaverns()
	{
		//process of smoothing the caverns
		for (int smoothingIndex = 0; smoothingIndex < GenerationalSmoothing; smoothingIndex++) {
			for (int i = 0; i < mapSize; i++) {
				for (int j = 0; j < mapSize; j++) {

					int numberOfWalls = CountWalls (i, j);

					//Primary automaton logic
					if (numberOfWalls >= BorderingWallsAllowance)
						matrix [i, j] = 1;
					else
						matrix [i, j] = 0;
				}
			}
		}
	}

	/// <summary>
	/// Adds border around the entire matrix to avoid OOB values, and add to cave aesthetic.
	/// </summary>
	private static void AddUnitBorder()
	{
		for (int x = 0; x < mapSize; x++) {
			matrix [0, x] = 0;
			matrix [mapSize-1, x] = 0;
			matrix [x, 0] = 0;
			matrix [x, mapSize - 1] = 0;
		}
	}

	/// <summary>
	/// Counts walls around any given Cell (x,y) values
	/// </summary>
	/// <returns>The walls.</returns>
	/// <param name="cellXCoordinate">Cell X coordinate.</param>
	/// <param name="cellYCoordinate">Cell Y coordinate.</param>
	private static int CountWalls(int cellXCoordinate, int cellYCoordinate)
	{
		int wallCounter = 0;

		for (int i = -1; i <= 1; i++) {
			for (var j = -1; j <= 1; j++) {
				if ( cellXCoordinate + i < 0 || cellXCoordinate + i > mapSize - 1 || cellYCoordinate + j < 0 || cellYCoordinate + j > mapSize - 1 ) 
					continue;

				//Found a wall, increment count
				if ( matrix[cellXCoordinate + i, cellYCoordinate + j] != 0 ) 
					wallCounter++;
			}
		}

		return wallCounter;
	}

	/// <summary>
	/// Removes regions under the acceptable threshold.
	/// Can also be used to eliminate "island" rooms if configured.
	/// </summary>
	private static void RemoveUndersizedRegions()
	{
		//look for all open spaces
		List<Region> wallRegions = GetRegionsOfType (1);

		foreach (Region wallRegion in wallRegions) {
			//if the amount of open spaces is less than the threshold, make them become walls
			if (wallRegion.Cells.Count < WallThresholdSize) {
				foreach (Cell tile in wallRegion.Cells) {
					matrix [tile.x, tile.y] = 0;
				}
			}
		}
	}

	/// <summary>
	/// Returns all regions of a specific tile type.
	/// </summary>
	/// <returns>The regions of type.</returns>
	/// <param name="tileType">Tile type.</param>
	private static List<Region> GetRegionsOfType(int tileType) 
	{
		List<Region> regions = new List<Region> ();
		int[,] matrixFlags = new int[mapSize, mapSize];

		for (int x = 0; x < mapSize; x++) {
			for (int y = 0; y < mapSize; y++) {
				//if it hasn't been checked, and it's the valid tile type we're looking for.. create a new region
				if (matrixFlags [x, y] == 0 && matrix [x, y] == tileType) {
					Region newRegion = GetRegionTiles (x, y);
					regions.Add (newRegion);

					foreach (Cell tile in newRegion.Cells) {
						matrixFlags [tile.x, tile.y] = 1;
					}
				}
			}
		}

		return regions;
	}

	/// <summary>
	/// Performs a flood-fill operation to get all cells of a specific region.
	/// </summary>
	/// <returns>List of tiles in a segmented region.</returns>
	/// <param name="startX">Starting x position</param>
	/// <param name="startY">Starting y position</param>
	private static Region GetRegionTiles(int startX, int startY) 
	{
		Region regionTiles = new Region(new List<Cell>());
		Queue<Cell> queue = new Queue<Cell> ();

		//efficiency addition - updates if a cell has already been checked
		bool[,] matrixFlags = new bool[mapSize, mapSize];
	
		int tileType = matrix [startX, startY];

		queue.Enqueue (new Cell (startX, startY));

		//mark the initial starting point as checked
		matrixFlags [startX, startY] = true;

		//while we have cells to check
		while (queue.Count > 0) {
			Cell tile = queue.Dequeue();
			regionTiles.Cells.Add (tile);

			for (int x = tile.x - 1; x <= tile.x + 1; x++) {
				for (int y = tile.y - 1; y <= tile.y + 1; y++) {
					//if in bounds, and non-diagonal
					if ((x >= 0 && x < mapSize && y >= 0 && y < mapSize) && (y == tile.y || x == tile.x)) {
						if (matrixFlags [x, y] == false && matrix [x, y] == tileType) {

							//mark as checked
							matrixFlags [x, y] = true;
							queue.Enqueue (new Cell (x, y));
						}
					}
				}
			}
		}

		//return the list of cells in the region
		return regionTiles;
	}
}

