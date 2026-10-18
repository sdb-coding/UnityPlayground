
public interface ISaveableReceiver 
{
    void OnBeforeLoadGlobal(); //Called before any load happne, used to clean static things
    void OnBeforeSaveInstance(); //Called right before saving the current ISaveableReceiver Instance, used to pull static data
    void OnAfterLoadInstance(); //Called after loading the current ISaveableReceiver Instance, used to setup the loaded data
}
