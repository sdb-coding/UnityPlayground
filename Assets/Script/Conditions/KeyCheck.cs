using UnityEngine;

public class KeyCheck : MonoBehaviour
{
    public enum KeyCheckCondition { IsTrue, IsEqualToValue, IsBiggerThanValue, IsSmallerThanValue, IsEqualToAnotherKey, IsBiggerThanAnotherKey, IsState, IsNotState, IsFalse   }
    public KeyVariable keyVariable;
    public KeyCheckCondition condition;
    public int checkValue;

    public KeyVariable otherKeyVariable;

    public bool IsTrue()
    {
        int savedValue = 0;
        int otherSavedValue = 0;

        //savedValue = Managers.SaveManager.Instance.saveDataManager.keyDataManager.GetValue(keyVariable);
        if (condition == KeyCheckCondition.IsBiggerThanAnotherKey || condition == KeyCheckCondition.IsEqualToAnotherKey)
        {
            //otherSavedValue = Managers.SaveManager.Instance.saveDataManager.keyDataManager.GetValue(otherKeyVariable);
        }
        return CheckCondition(this, savedValue, otherSavedValue);
    }
    private static bool CheckCondition(KeyCheck keyCheck, int savedValue, int otherSavedValue)
    {
        switch (keyCheck.condition)
        {
            case KeyCheckCondition.IsTrue:
                return savedValue > 0;
            case KeyCheckCondition.IsFalse:
                return savedValue < 1;
            case KeyCheckCondition.IsEqualToValue:
                return savedValue == keyCheck.checkValue;
            case KeyCheckCondition.IsBiggerThanValue:
                return savedValue > keyCheck.checkValue;
            case KeyCheckCondition.IsEqualToAnotherKey:
                return savedValue == otherSavedValue;
            case KeyCheckCondition.IsBiggerThanAnotherKey:
                return savedValue > otherSavedValue;
            case KeyCheckCondition.IsState:
                return savedValue == keyCheck.checkValue;
            case KeyCheckCondition.IsSmallerThanValue:
                return savedValue < keyCheck.checkValue;
            case KeyCheckCondition.IsNotState:
                return savedValue != keyCheck.checkValue;
        }

        return false;
    }
}
