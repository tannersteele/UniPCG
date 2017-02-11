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
using System.Collections;

/// <summary>
/// A Game Manager class that acts as a point-of-entry for this application.
/// It includes test cases within the Start() method which can be modified to one's heart's content.
/// </summary>
public class GameManager : MonoBehaviour {
	public GameObject playerCharacter;
	private Rigidbody2D playerRigidbody;

	public static GameManager _instance;

	void Awake() 
	{
		_instance = this;
	}

	void Start () 
	{
		//initialize the generator with some values
		CaveGenerator.BorderingWallsAllowance = 5;
		CaveGenerator.GenerationalSmoothing = 10;
		CaveGenerator.PercentageOfWalls = 48;
		CaveGenerator.WallThresholdSize = 100;

		//create a cave with a fixed seed
		CaveGenerator.CreateCave (100, 1337);

		playerRigidbody = playerCharacter.GetComponent<Rigidbody2D> ();
	}

	public void SpawnCharacter(Vector3 location)
	{
		Instantiate(playerCharacter, location, Quaternion.identity);
	}

	void Update() 
	{
		if (Input.GetKey (KeyCode.A)) {
			playerRigidbody.AddForce (Vector2.left, ForceMode2D.Impulse);
		} else if (Input.GetKey (KeyCode.D)) {
			playerRigidbody.AddForce (Vector2.right, ForceMode2D.Impulse);
		} else if (Input.GetKey (KeyCode.Space)) {
			playerRigidbody.AddForce (Vector2.up, ForceMode2D.Impulse);
		}
	}
}
