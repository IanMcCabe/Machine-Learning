using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Genes
{
	// Static Variables
	public static float MIN_SIZE = 0.1f;
	public static float MAX_SIZE = 0.5f;

	public Color colorGene = Color.black;
	public float sizeGene = 0;

	// Constructor
	public Genes(Color color, float size)
	{
		colorGene = color;
		sizeGene = size;
	}

	public Genes() { }
}

public class Animal : MonoBehaviour
{
	// Life
	public bool isAlive = true;
	public float lifeSpan = 0;

	public Genes genes = new Genes();

	// Private
	private Collider2D thisCollider;
	private SpriteRenderer thisRenderer;
	private Transform thisTransform;

	// Use this for initialization
	void Start()
	{
		Init();
		ApplyGenes();
	}

	private void Init()
	{
		thisCollider = GetComponent<Collider2D>();
		thisRenderer = GetComponent<SpriteRenderer>();
		thisTransform = GetComponent<Transform>();
	}
	
	private void ApplyGenes()
	{
		thisRenderer.color = genes.colorGene;
		thisTransform.localScale = Vector3.one * genes.sizeGene;
	}

	private void OnMouseDown()
	{
		Kill();
		thisCollider.enabled = false;
		thisRenderer.enabled = false;
	}

	public void Kill()
	{
		isAlive = false;
		lifeSpan = PopulationManager.ElapsedTime;
	}
}
