using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class GameRunner : MonoBehaviour, IModelContainer, ISaveableReceiver, ISceneLifecycle
{
#if UNITY_EDITOR
    [CustomEditor(typeof(GameRunner))]
    public class GameRunner_Editor : Editor
    {
        GameRunner myTarget;
        private void OnEnable()
        {
            myTarget = (GameRunner)target;
        }
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Start Debug Game"))
                {
                    myTarget.StartNewGame(myTarget.debugBlueprint);
                }
                if (GUILayout.Button("Debug Out cards"))
                {
                    foreach (var item in myTarget.gameModel.Value.cardRegistry.Values)
                    {
                        Debug.Log(item.GetCardTitleAsync());
                    }
                }
            }
            base.OnInspectorGUI();
        }
    }
#endif
    [Header("BaseEvents")]
    public BaseEvent startAntePart;
    public BaseEvent startStorePart;

    [Header("ProjectReferences")]
    [SerializeField] StoreRunner storeRunner;
    [SerializeField] JokerHandPresenter jokerHandPresenter;

    [Header("Random")]
    public uint startingRandomSeed = 1234567;
    public bool startingSeedIsRandom = false;

#if DEBUG_ENABLED
    [Header("Debug")]
    public GameBlueprint debugBlueprint;
    [SerializeField] DebugStarter debugStartOnStart;
    enum DebugStarter {None, NormalStart, LoadOnStart };

    public RefEvent<GameModel> gameModel { get; private set; }
    private void Awake()
    {
        gameModel = new RefEvent<GameModel>();
    }
    private void Start()
    {
        switch (debugStartOnStart)
        {
            case DebugStarter.None:
                break;
            case DebugStarter.NormalStart:
                if (debugBlueprint != null)
                {
                    StartNewGame(debugBlueprint);
                }
                break;
            case DebugStarter.LoadOnStart:
                Services.SaveSystem.SaveLoadManager.LoadActiveRun();
                break;
            default:
                break;
        }
    }
#endif
    public void StartNewGame(GameBlueprint gameBlueprint)
    {
        SetupRandom(startingSeedIsRandom ? 0 : startingRandomSeed);
        SetupGameModel(gameBlueprint);
        SetupUniversalPresenters();
        if (storeRunner == null)
            storeRunner = FindAnyObjectByType<StoreRunner>();
        RunContext.Current.StartNewAnte();
        BaseEvent.Raise(startAntePart);
    }
    void SetupRandom(uint seed)
    {
        if (seed != 0)
            RandomUtility.InitRandom(seed);
        else
            RandomUtility.InitRandom((uint)(System.DateTime.Now.Ticks * Mathf.PI));
    }
    void SetupGameModel(GameBlueprint gameBlueprint)
    {
        gameModel.Value = new GameModel(gameBlueprint);
        gameModel.Value.onAnteComplete += AnteComplete;
        gameModel.Value.onStoreComplete += StoreComplete;
    }
    void SetupUniversalPresenters()
    {
        jokerHandPresenter.DisplayPlayerHand(gameModel.Value.jokerHandModel);
    }
    void AnteComplete()
    {
        storeRunner.StartNewStore(gameModel);
        BaseEvent.Raise(startStorePart);
    }
    void StoreComplete()
    {
        BaseEvent.Raise(startAntePart);
        gameModel.Value.StartNewAnte();
    }

    public void OnAfterLoadInstance()
    {
        switch (gameModel.Value.currentGameState)
        {
            case GameState.None:
                break;
            case GameState.Ante:
                BaseEvent.Raise(startAntePart);
                break;
            case GameState.Store:
                BaseEvent.Raise(startStorePart);
                break;
            case GameState.AnteSelection:
                break;
            default:
                break;
        }
    }

    public void OnBeforeSaveInstance()
    {
        gameModel.Value.randomState = RandomUtility.GetCurrentStage();
    }
    public void OnBeforeLoadGlobal()
    {
        if (gameModel.Value != null)
            gameModel.Value.ClearCardRegistry();
    }

    public object GetModel() => gameModel.Value;

    public void ModelRestored()
    {
        RandomUtility.SetState(gameModel.Value.randomState);
    }
    #region ISceneLifecycle
    public async Task OnPrepareUnloadAsync()
    {
        await Task.Delay(1000); // Temp test
    }
    public void ReceiveLoadPayload(object loadPayload)
    {
        if (loadPayload is GameBlueprint gameBlueprint)
        {
            StartNewGame(gameBlueprint);
        } 
    }
    public async Task OnSceneLoadedAsync()
    {
        await Task.Delay(1000); // Temp test
    }
    #endregion
}
