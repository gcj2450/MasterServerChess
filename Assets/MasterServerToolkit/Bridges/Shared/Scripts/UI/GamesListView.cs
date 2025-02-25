﻿using Aevien.UI;
using MasterServerToolkit.Logging;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MasterServerToolkit.Games
{
    public class GamesListView : UIView
    {
        [Header("Components"), SerializeField]
        private UILable uiLablePrefab;
        [SerializeField]
        private UILable uiColLablePrefab;
        [SerializeField]
        private UIButton uiButtonPrefab;
        [SerializeField]
        private RectTransform listContainer;
        [SerializeField]
        private TMP_Text statusInfoText;

        public UnityEvent OnStartGameEvent;

        protected override void Awake()
        {
            base.Awake();

            // Listen to show/hide events
            Mst.Events.AddEventListener(MstEventKeys.showGamesListView, OnShowGamesListEventHandler);
            Mst.Events.AddEventListener(MstEventKeys.hideGamesListView, OnHideGamesListEventHandler);
        }

        protected override void Start()
        {
            base.Start();

            if (listContainer)
            {
                foreach (Transform t in listContainer)
                {
                    Destroy(t.gameObject);
                }
            }
        }

        private void OnShowGamesListEventHandler(EventMessage message)
        {
            Show();
        }

        private void OnHideGamesListEventHandler(EventMessage message)
        {
            Hide();
        }

        protected override void OnShow()
        {
            FindGames();
        }

        /// <summary>
        /// Sends request to master server to find games list
        /// </summary>
        public void FindGames()
        {
            ClearGamesList();

            canvasGroup.interactable = false;

            if (statusInfoText)
            {
                statusInfoText.text = "Finding rooms... Please wait!";
                statusInfoText.gameObject.SetActive(true);
            }

            MstTimer.WaitForSeconds(0.2f, () =>
            {
                Mst.Client.Matchmaker.FindGames((games) =>
                {
                    canvasGroup.interactable = true;

                    if (games.Count == 0)
                    {
                        statusInfoText.text = "No games found! Try to create your own.";
                        return;
                    }

                    statusInfoText.gameObject.SetActive(false);
                    DrawGamesList(games);
                });
            });
        }

        private void DrawGamesList(IEnumerable<GameInfoPacket> games)
        {
            if (listContainer)
            {
                int index = 0;

                var gameNameCol = Instantiate(uiColLablePrefab, listContainer, false);
                gameNameCol.Lable = "Name";

                var gameAddressCol = Instantiate(uiColLablePrefab, listContainer, false);
                gameAddressCol.Lable = "Address";

                var gameRegionCol = Instantiate(uiColLablePrefab, listContainer, false);
                gameRegionCol.Lable = "Region";

                var pingRegionCol = Instantiate(uiColLablePrefab, listContainer, false);
                pingRegionCol.Lable = "Ping";

                var gamePlayersCol = Instantiate(uiColLablePrefab, listContainer, false);
                gamePlayersCol.Lable = "Players";

                var ConnectBtnCol = Instantiate(uiColLablePrefab, listContainer, false);
                ConnectBtnCol.Lable = "#";

                foreach (GameInfoPacket gameInfo in games)
                {
                    var gameNameLable = Instantiate(uiLablePrefab, listContainer, false);
                    gameNameLable.Lable = gameInfo.IsPasswordProtected ? $"{gameInfo.Name} <color=yellow>[Password]</color>" : gameInfo.Name;
                    gameNameLable.name = $"gameNameLable_{index}";

                    var gameAddressLable = Instantiate(uiLablePrefab, listContainer, false);
                    gameAddressLable.Lable = gameInfo.Address;
                    gameAddressLable.name = $"gameAddressLable_{index}";

                    var gameRegionLable = Instantiate(uiLablePrefab, listContainer, false);
                    string region = string.IsNullOrEmpty(gameInfo.Region) ? "International" : gameInfo.Region;
                    gameRegionLable.Lable = region;
                    gameRegionLable.name = $"gameRegionLable_{index}";

                    var pingRegionLable = Instantiate(uiLablePrefab, listContainer, false);
                    pingRegionLable.Lable = $"...";

                    var rx = new Regex(@":\d+");
                    string ip = rx.Replace(gameInfo.Address.Trim(), "");

                    MstTimer.WaitPing(ip, (time) => {
                        pingRegionLable.Lable = $"{time} ms.";
                    });

                    pingRegionLable.name = $"pingRegionLable_{index}";

                    var gamePlayersBtn = Instantiate(uiButtonPrefab, listContainer, false);
                    string maxPleyers = gameInfo.MaxPlayers <= 0 ? "∞" : gameInfo.MaxPlayers.ToString();
                    gamePlayersBtn.SetLable($"{gameInfo.OnlinePlayers} / {maxPleyers} [Show]");
                    gamePlayersBtn.name = $"gamePlayersLable_{index}";
                    gamePlayersBtn.AddOnClickListener(() => {
                        Mst.Events.Invoke(MstEventKeys.showPlayersListView, new EventMessage(gameInfo.Id));
                        Hide();
                    });

                    var gameConnectBtn = Instantiate(uiButtonPrefab, listContainer, false);
                    gameConnectBtn.SetLable("Join");
                    gameConnectBtn.AddOnClickListener(() => {
                        MatchmakingBehaviour.Instance.StartMatch(gameInfo);
                    });
                    gameConnectBtn.name = $"gameConnectBtn_{index}";

                    index++;

                    Logs.Info(gameInfo);
                }
            }
            else
            {
                Logs.Error("Not all components are setup");
            }
        }

        private void ClearGamesList()
        {
            if (listContainer)
            {
                foreach (Transform tr in listContainer)
                {
                    Destroy(tr.gameObject);
                }
            }
        }
    }
}