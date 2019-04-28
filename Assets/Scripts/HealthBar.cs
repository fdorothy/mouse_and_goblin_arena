using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour {
    public Game game;
    public PieceType piece;
    RectTransform rect;
    RectTransform parentRect;
    float maxWidth = 1.0f;
    int maxHealth;
    float currentHealth = 0;
    int targetHealth = 0;
    float healRate = 5.0f;
    float damageRate = 10.0f;
	// Use this for initialization
	void Start () {
        game = FindObjectOfType<Game>();
        Setup();
        rect = GetComponent<RectTransform>();
        maxWidth = rect.rect.width;
	}

    public void Setup() {
        if (game != null)
        {
            currentHealth = game.getWizardHealth(piece);
            maxHealth = (int)currentHealth;
            targetHealth = (int)currentHealth;
        }
    }

    // Update is called once per frame
    void Update () {
        if (currentHealth < 0)
            Setup();
        float oldTargetHealth = targetHealth;
        if (game != null)
            targetHealth = game.getWizardHealth(piece);
        else
            targetHealth = 0;
        if (currentHealth < targetHealth) {
            currentHealth += Time.deltaTime * healRate;
            if (currentHealth > targetHealth)
                currentHealth = targetHealth;
        } else if (currentHealth > targetHealth) {
            currentHealth -= Time.deltaTime * damageRate;
            if (currentHealth < targetHealth)
                currentHealth = targetHealth;
        }

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth * currentHealth / maxHealth);
	}
}
