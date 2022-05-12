using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.State;
using Nekoyume.UI;
using UnityEngine;
using UnityEngine.UI;
using Bencodex.Types;
using Nekoyume.Helper;
using TMPro;

namespace Nekoyume
{
    using UniRx;

    public class ArenaTestPopup : PopupWidget
    {
        [SerializeField]
        private Button joinArena;

        [SerializeField]
        private Button getList;

        [SerializeField]
        private Button close;

        [SerializeField]
        private InputField inputField;

        [SerializeField]
        private TextMeshProUGUI board;

        private readonly List<Guid> equipments = new List<Guid>();
        private readonly List<Guid> costumes = new List<Guid>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private void Start()
        {
            joinArena.onClick.AddListener(() =>
            {
                Debug.Log("ONCLICK joinArena");
                Game.Game.instance.ActionManager.JoinArena(costumes, equipments);
            });

            getList.onClick.AddListener(() =>
            {
                Debug.Log("ONCLICK getList");
                var index = int.Parse(inputField.text);
                GetList(index);
            });

            close.onClick.AddListener(() =>
            {
                Close();
            });
        }

        private async void GetList(long blockIndex)
        {
            var state = await Util.GetArenaState(blockIndex);
            var sb = new StringBuilder();
            var index = 1;
            sb.Append($"[Arena State Address] : {state.Address}\n");
            sb.Append($"[my avatar address] : {States.Instance.CurrentAvatarState.address}\n");
            foreach (var address in state.AvatarAddresses)
            {
                sb.Append($"[joined address - {index}] {address}\n");

                var avatarState = await Util.GetArenaAvatarState(address);
                foreach (var guid in avatarState.Costumes)
                {
                    sb.Append($"[COSTUMES] {guid}\n");
                }

                foreach (var guid in avatarState.Equipments)
                {
                    sb.Append($"[EQUIPMENTS] {guid}\n");
                }

                sb.Append($"[PROFILE] " +
                          $"name({avatarState.NameWithHash}) - " +
                          $"Level({avatarState.Level}) -" +
                          $"CharacterId({avatarState.CharacterId}) -" +
                          $"\n");

                sb.Append($"[TICKETS] " +
                          $"Ticket({avatarState.Ticket}) -" +
                          $"NcgTicket({avatarState.NcgTicket}) -" +
                          $"\n");

                sb.Append($"[LOOK] " +
                          $"HairIndex({avatarState.HairIndex}) -" +
                          $"LensIndex({avatarState.LensIndex}) -" +
                          $"EarIndex({avatarState.EarIndex}) -" +
                          $"TailIndex({avatarState.TailIndex}) -" +
                          $"\n");

                if (avatarState.Records.TryGetRecord(blockIndex, out var record))
                {
                    sb.Append($"[RECORD] win({record.Win}) - win({record.Lose}) - score({record.Score}) \n");
                }

                sb.Append("------------------------------------------------\n");
                index++;
            }

            board.text = sb.ToString();
            StartCoroutine(OffOn());
        }

        private IEnumerator OffOn()
        {
            yield return null;
            board.gameObject.SetActive(false);
            yield return null;
            board.gameObject.SetActive(true);

        }


        public override void Show(bool ignoreShowAnimation = false)
        {
            base.Show(true);
            SubscribeInventory();
            board.text = string.Empty;
            inputField.text = string.Empty;
        }

        private void SubscribeInventory()
        {
            _disposables.DisposeAllAndClear();
            ReactiveAvatarState.Inventory.Subscribe(inventory =>
            {
                if (inventory is null)
                {
                    return;
                }

                costumes.Clear();
                equipments.Clear();

                foreach (var item in inventory.Items)
                {
                    if (item.Locked)
                    {
                        continue;
                    }

                    switch (item.item.ItemType)
                    {
                        case ItemType.Costume:
                            var costume = (Costume)item.item;
                            if (costume.equipped)
                            {
                                costumes.Add(costume.ItemId);
                            }

                            break;

                        case ItemType.Equipment:
                            var equipment = (Equipment)item.item;
                            if (equipment.equipped)
                            {
                                equipments.Add(equipment.ItemId);
                            }

                            break;
                    }
                }
            }).AddTo(_disposables);
        }

    }
}
