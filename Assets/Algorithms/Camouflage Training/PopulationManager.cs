using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class PopulationManager : MonoBehaviour
{
	[Header("Population Settings")]
	[SerializeField] private GameObject animalPrefab;
	[SerializeField] private int populationSize = 20;
	[SerializeField] private int generation = 1;
	[SerializeField] private List<Animal> population = new List<Animal>();

	[Range(0.1f,1f)]
	[SerializeField] private float selectionPercent = 0.5f;

	[Header("Trial Settings")]
	[SerializeField] private int _fontSize = 20;
	[SerializeField] private bool _autoSelect = false;
	[SerializeField] private int _trialTime = 10;
	private static float _elapsed = 0;
	public static float ElapsedTime
	{
		get { return _elapsed; }
	}

	[Header("Mutation Settings")]
	public float mutationChance = 0.25f;
	public float mutationFactor = 0.1f;

	// Objects spawning settings
	private Vector2 topRightCorner = new Vector2(1f, 1f);
	private Vector2 edgeVector;

	private Genes progenitor1 = new Genes(new Color(0, 0, 0, 1), Genes.MIN_SIZE);
	private Genes progenitor2 = new Genes(new Color(1, 1, 1, 1), Genes.MAX_SIZE);

	void Awake ()
	{
		// Ensures that the gameobjects always spawn within the screen.
		edgeVector = Camera.main.ScreenToWorldPoint(topRightCorner);
		edgeVector = new Vector2(Mathf.Abs(edgeVector.x), Mathf.Abs(edgeVector.y)) - Vector2.one;
		
		for (int i = 0; i < populationSize; i++)
		{
			// Create the inital animals
			Animal animal = Breed(progenitor1, progenitor2);
			population.Add(animal);
		}
	}
	
	GUIStyle guiStyle = new GUIStyle();
	private void OnGUI()
	{
		guiStyle.fontSize = _fontSize;
		guiStyle.normal.textColor = Color.white;
		GUI.Label(new Rect(10, 10, 100, 20), "Generation: " + generation, guiStyle);
		GUI.Label(new Rect(10, 15 + _fontSize, 100, 20), "Trial Countdown: " + (_trialTime - (int)_elapsed), guiStyle);
		_autoSelect = GUI.Toggle(new Rect(10, 40 + _fontSize, 200, 20), _autoSelect, "Auto Color Fitness");
	}

	private void Update()
	{
		_elapsed += Time.deltaTime;
		if (_elapsed >= _trialTime)
		{
			BreedNewPopulation();
			_elapsed = 0;
		}
	}

	private void BreedNewPopulation()
	{
		if (population.Count > 0)
		{
			Animal[] orderedPopulation;

			

			if (_autoSelect)
			{
				// Determines fitness based on how closely they match the background color.
				Vector4 background = Camera.main.backgroundColor;
				orderedPopulation = population.OrderBy(o => Mathf.Abs((background - (Vector4)o.genes.colorGene).magnitude)).ToArray();
				Array.Reverse(orderedPopulation);
			}
			else
			{	
				// Kills off any remaining animals that survived the last generation.
				for (int i = 0; i < population.Count; i++)
				{
					if (population[i].isAlive) { population[i].Kill(); }
				}

				// Determines fitness based on the lifespan of each animal though user selection.
				orderedPopulation = population.OrderBy(o => o.lifeSpan).ToArray();
			}

			population.Clear();

			int startIndex = (int)(orderedPopulation.Length * (1 - selectionPercent));
			int endIndex = orderedPopulation.Length;
			int offspringCount = populationSize / (endIndex - startIndex);

			// Breed a portion of the population.
			for (int i = startIndex; i < endIndex; i++)
			{
				int nextIndex = i + 1;
				if (nextIndex >= endIndex) { nextIndex = startIndex + 1; }
				if (nextIndex >= endIndex) { break; }

				for (int j = 0; j < offspringCount; j++)
				{
					population.Add(Breed(orderedPopulation[i].genes, orderedPopulation[nextIndex].genes));
				}
			}

			// Destory and remove the previous generation.
			for (int i = 0; i < orderedPopulation.Length; i++)
			{
				Destroy(orderedPopulation[i].gameObject);
			}

			generation++;
		}
		else
		{
			Debug.LogWarning("The animal went extinct");
		}
	}
	
	private Animal Breed(Genes parent1, Genes parent2)
	{
		// Spawn the animal	
		Animal animal = SpawnAnimal();

		// Generate color from genes
		Color animalColor = new Color(Random.Range(parent1.colorGene.r, parent2.colorGene.r),
										Random.Range(parent1.colorGene.g, parent2.colorGene.g),
										Random.Range(parent1.colorGene.b, parent2.colorGene.b),
										1);

		float animalSize = Random.Range(parent1.sizeGene, parent2.sizeGene);

		// Mutate and set the animals DNA
		MutateColor(animal, animalColor);
		MutateSize(animal, animalSize);
		return animal;
	}

	private Animal SpawnAnimal()
	{
		Vector3 pos = new Vector3(Random.Range(-edgeVector.x, edgeVector.x), Random.Range(-edgeVector.y, edgeVector.y), 0);
		GameObject animalObj = Instantiate(animalPrefab, pos, Quaternion.identity);
		Animal animal = animalObj.GetComponent<Animal>();
		return animal;
	}


	private void MutateColor(Animal animal, Color color)
	{
		// Calculates the chance of a mutation and adds a factor.
		float mutation = Random.Range(0f, 1f);
		if (mutation > (1 - mutationChance))
		{
			color.r = LimitToRange(color.r + Random.Range(-mutationFactor, mutationFactor), 0f, 1f);
			color.g = LimitToRange(color.g + Random.Range(-mutationFactor, mutationFactor), 0f, 1f);
			color.b = LimitToRange(color.b + Random.Range(-mutationFactor, mutationFactor), 0f, 1f);
		}

		// Apply final color
		animal.genes.colorGene = color;
	}

	private void MutateSize(Animal animal, float size)
	{
		// Calculates the chance of a mutation and adds a factor.
		float mutation = Random.Range(0f, 1f);
		if (mutation > (1 - mutationChance))
		{
			size = LimitToRange(size + Random.Range(-mutationFactor, mutationFactor), Genes.MIN_SIZE, Genes.MAX_SIZE);
		}

		// Apply final size;
		animal.genes.sizeGene = size;
	}

#region - Helper Functions
	private float LimitToRange(float value, float inclusiveMinimum, float inclusiveMaximum)
	{
		if (value < inclusiveMinimum) { return inclusiveMinimum; }
		if (value > inclusiveMaximum) { return inclusiveMaximum; }
		return value;
	}
#endregion
}
