using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class GamesManager : StateMachine
{
    // _     __________    _    ______   __   ____ ___  ____  _____           
    //| |   | ____/ ___|  / \  / ___\ \ / /  / ___/ _ \|  _ \| ____|          
    //| |   |  _|| |  _  / _ \| |    \ V /  | |  | | | | | | |  _|            
    //| |___| |__| |_| |/ ___ | |___  | |   | |__| |_| | |_| | |___           
    //|_____|_____\____/_/   \_\____| |_|    \____\___/|____/|_____|          


    // ____   ___    _   _  ___ _____    ____ _   _    _    _   _  ____ _____ 
    //|  _ \ / _ \  | \ | |/ _ |_   _|  / ___| | | |  / \  | \ | |/ ___| ____|
    //| | | | | | | |  \| | | | || |   | |   | |_| | / _ \ |  \| | |  _|  _|  
    //| |_| | |_| | | |\  | |_| || |   | |___|  _  |/ ___ \| |\  | |_| | |___ 
    //|____/ \___/  |_| \_|\___/ |_|    \____|_| |_/_/   \_|_| \_|\____|_____|

    #region Singleton
    public static GamesManager Instance;

    #endregion

    public GameObject myKim;
    private Kim myKimScript;

    float TotalGameTime;

    int BurgerCount;
    int CollectedBurgers;

    [SerializeField] bool SkipIntro = false;
    [SerializeField] private float currentTimeStep;
    
    [FormerlySerializedAs("Burgers")] public List<Burger> BurgersInScene = new List<Burger>();

    private void Awake()
    {
        Instance = this;
        currentTimeStep = 1f;
    }

    private void Start()
    {
        InitializeStateMachine();
        BurgersInScene = FindObjectsOfType<Burger>().ToList();
        BurgerCount = BurgersInScene.Count;
        myKimScript = myKim.GetComponent<Kim>();
        if (!SkipIntro)
        {
            ChangeState<IntroState>();
        }
        else
        {
            ChangeState<PlayingState>();
        }
    }

    private void Update()
    {
        UpdateStateMachine();
        Time.timeScale = currentTimeStep;
    }

    public Vector3 GetKimPos()
    {
        return myKim.transform.position;
    }

    public void SetTotalGameTime(float aTime)
    {
        TotalGameTime = aTime;
    }
    public float GetTotalGameTime()
    {
        return TotalGameTime;
    }

    public void CollectBurger()
    {
        CollectedBurgers++;
        InitiateActionsAfterBurgerCollection();
    }

    private async void InitiateActionsAfterBurgerCollection()
    {
        await Task.Delay(1000);
        myKimScript.OnBurgerCollected();
    }

    public int GetBurgerCount()
    {
        return BurgerCount;
    }
    public int GetCollectedBurgers()
    {
        return CollectedBurgers;
    }
}
