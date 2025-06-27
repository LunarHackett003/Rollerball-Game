using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FizzBuzz : MonoBehaviour
{
    int number;
    [SerializeField] int fizzNumber, buzzNumber;
    [SerializeField] int range;

    [SerializeField] GameObject floatyText;
    [SerializeField] float spawnRange;
    [SerializeField] float floatyTextSpeed;
    [SerializeField] float floatyTextLifetime;

    [SerializeField] Rigidbody rb;
    [SerializeField] float angularVelocity;
    enum ShowcaseType
    {
        none,
        particle,
        bounce,
    }
    [SerializeField] ShowcaseType showcase;
    private void Start()
    {
    }
    private void Update()
    {
        switch (showcase)
        {
            case ShowcaseType.none:
                if (number < range)
                {
                    Check();
                    number++;
                }
                break;
            case ShowcaseType.particle:
                if (number < range)
                {
                    Check();
                    number++;
                }
                break;
            case ShowcaseType.bounce:

                break;
            default:
                break;
        }
        rb.isKinematic = showcase != ShowcaseType.bounce;

    }
    private void OnGUI()
    {
        if(GUILayout.Button("Lets go again!", GUILayout.Width(300), GUILayout.Height(50)))
        {
            print("Yay! Lets do it again!");
            number = 0;
        }
    }
    void Check()
    {
        string output = $"{number}: " + (number % fizzNumber == 0 ? "Fizz" : "") + (number % buzzNumber == 0 ? "Buzz" : "");
        print(output);
        Vector3 random = Random.onUnitSphere;
        if (showcase == ShowcaseType.particle)
        {
            GameObject go = Instantiate(floatyText, transform.position + (random * spawnRange), Quaternion.identity);
            go.AddComponent<Rigidbody>().velocity = random * floatyTextSpeed;
            var tmp = go.GetComponent<TextMeshPro>();
            tmp.text = output;
            tmp.color = Random.ColorHSV();
            Destroy(go, floatyTextLifetime);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(showcase == ShowcaseType.bounce)
        {
            string output = $"{number}: " + (number % fizzNumber == 0 ? "Fizz" : "") + (number % buzzNumber == 0 ? "Buzz" : "");
            Vector3 random = Random.onUnitSphere;
            GameObject go = Instantiate(floatyText, transform.position + (random * spawnRange), Quaternion.identity);
            go.AddComponent<Rigidbody>().velocity = random * floatyTextSpeed;
            var tmp = go.GetComponent<TextMeshPro>();
            tmp.text = output;
            tmp.color = Random.ColorHSV();
            Destroy(go, floatyTextLifetime);
            number++;
            rb.angularVelocity += Random.onUnitSphere * angularVelocity;
        }
    }
}
