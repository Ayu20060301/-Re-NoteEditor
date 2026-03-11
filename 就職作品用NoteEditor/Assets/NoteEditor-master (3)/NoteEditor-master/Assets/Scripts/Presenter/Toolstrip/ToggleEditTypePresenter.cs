using NoteEditor.Notes;
using NoteEditor.Model;
using NoteEditor.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NoteEditor.Presenter
{
    public class ToggleEditTypePresenter : MonoBehaviour
    {
        [SerializeField]
        Button editTypeToggleButton = default;
        [SerializeField]
        Sprite iconLongNotes = default; //ロングノーツの画像
        [SerializeField]
        Sprite iconSingleNotes = default; //シングルノーツの画像
        [SerializeField]
        Sprite iconAttackNotes = default; //アタックノーツの画像
        [SerializeField]
        Sprite iconCoinNotes = default; //コインノーツの画像
        [SerializeField]
        Sprite iconJumpNotes = default; //ジャンプノーツの画像
        [SerializeField]
        Color longTypeStateButtonColor = default; //ロングノーツのボタンの色
        [SerializeField]
        Color singleTypeStateButtonColor = default; //シングルノーツのボタンの色
        [SerializeField]
        Color attackTypeStateButtonColor = default; //アタックノーツのボタンの色
        [SerializeField]
        Color coinTypeStateButtonColor = default; //コインノーツのボタンの色
        [SerializeField]
        Color jumpTypeStateButtonColor = default; //ジャンプノーツのボタンの色


        void Awake()
        {
            editTypeToggleButton.OnClickAsObservable()
                .Merge(this.UpdateAsObservable().Where(_ => KeyInput.AltKeyDown()))
                .Select(_ =>
                {
                    //現在のタイプに応じて次のタイプを探す
                    switch (EditState.NoteType.Value)
                    {
                        //シングルノーツアイコンからロングノーツアイコンに切り替え
                        case NoteTypes.Single:
                            return NoteTypes.Long;
                        //ロングノーツアイコンからアタックノーツアイコンに切り替え
                        case NoteTypes.Long:
                            return NoteTypes.Attack;
                        //アタックノーツアイコンからコインノーツアイコンに切り替え
                        case NoteTypes.Attack:
                            return NoteTypes.Coin;
                        //コインノーツアイコンからシングルノーツアイコンに切り替え
                        case NoteTypes.Coin:
                            return NoteTypes.Jump;
                        //ジャンプノーツアイコンからシングルノーツアイコンに切り替え
                        case NoteTypes.Jump:
                        default:
                            return NoteTypes.Single;
                    }
                })
                .Subscribe(editType => EditState.NoteType.Value = editType);

            var buttonImage = editTypeToggleButton.GetComponent<Image>();

            EditState.NoteType.Subscribe(noteType =>
            {
                //ノーツの種類
                switch (noteType)
                {
                    //シングルノーツ
                    case NoteTypes.Single:
                        buttonImage.sprite = iconSingleNotes;
                        buttonImage.color = singleTypeStateButtonColor;
                        break;
                    //ロングノーツ
                    case NoteTypes.Long:
                        buttonImage.sprite = iconLongNotes;
                        buttonImage.color = longTypeStateButtonColor;
                        break;
                    //アタックノーツ
                    case NoteTypes.Attack:
                        buttonImage.sprite = iconAttackNotes;
                        buttonImage.color = attackTypeStateButtonColor;
                        break;
                    //コインノーツ
                    case NoteTypes.Coin:
                        buttonImage.sprite = iconCoinNotes;
                        buttonImage.color = coinTypeStateButtonColor;
                        break;
                    //ジャンプノーツ
                    case NoteTypes.Jump:
                        buttonImage.sprite = iconJumpNotes;
                        buttonImage.color = jumpTypeStateButtonColor;
                        break;

                }
            });
        }
    }
}
