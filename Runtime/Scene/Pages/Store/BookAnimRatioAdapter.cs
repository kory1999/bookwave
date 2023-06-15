
using BeWild.AIBook.Runtime.Manager;
using UnityEngine;

public class BookAnimRatioAdapter : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        float scale = 0.8f;
        if (GameManager.IsPadDevice)
        {
            scale = 1.0f;
        }
        transform.localScale = new Vector3(scale, scale, 1.0f);
    }

        
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
