using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class DebugSpawner : MonoBehaviour
{
#if UNITY_EDITOR
    [CustomEditor(typeof(DebugSpawner))]
    public class DebugSpawner_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Spawn Card Collection"))
            {
                var cards = CardViewFactory.Instance.ConstructCardModels(((DebugSpawner)target).cardCollection);
                int cardNumber = 0;
                int row = 0;
                foreach (var card in cards)
                {
                    var newCardDisplayer = CardViewFactory.Instance.GetCardPresenter(card.cardType, card);
                    newCardDisplayer.Transform.localPosition = new Vector3(cardNumber * 5.1f, row * 7.1f, 0);
                    cardNumber++;
                    if (cardNumber == 13)
                    {
                        row++;
                        cardNumber = 0;
                    }

                }
            }
            base.OnInspectorGUI();
        }
    }
#endif
    public CardCollection cardCollection;

}
