using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Test();
    }

    private async void Test()
    {
        GraphLoader loader = new GraphLoader("neo4j+s://d6906551.databases.neo4j.io:7687", "neo4j", "k2SPtzle4z6jd6n1JqmS49pqqEC9b1Vy3lKfuTbP9Vs");
        
    }


}
