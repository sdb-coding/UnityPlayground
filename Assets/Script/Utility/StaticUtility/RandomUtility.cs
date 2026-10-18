using Unity.Mathematics;

public static class RandomUtility
{
    private static Random random = new Random(1);
    public static void InitRandom(uint seed)
    {
        if (seed == 0) seed = 1;
        random.InitState(seed);
    }
    public static void InitRandom(Random savedState)
    {
        random = savedState;
    }
    public static int GetRandomInt(int min, int max)
    {
        return random.NextInt(min, max);
    }
    public static float GetRandomFloat(float min, float max)
    {
        return random.NextFloat(min, max);
    }
    public static uint GetCurrentStage()
    {
        return random.state;
    }
    public static void SetState(uint state)
    {
        if (state == 0) state = 1;
        random.state = state;
    }
}
