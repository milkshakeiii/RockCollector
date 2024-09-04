using UnityEngine;

public class ReturnModuleButton : MonoBehaviour
{
    public Equipment module;
    private SetupScreen setupScreen;

    public void Initialize(Equipment module, SetupScreen setupScreen)
    {
        this.module = module;
        this.setupScreen = setupScreen;
        GetComponentInChildren<TMPro.TMP_Text>().text = module.name;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Clicked()
    {
        setupScreen.ReturnModule(this);
    }
}
