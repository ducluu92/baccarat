using Assets.Scripts.Exception;
using Assets.Scripts.Model.Charge;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Services.Lobby
{
    public class CashChargeService : ICashChargeService
    {
        private const string ROOT = "CASH";
        private const string CASH_REQUEST_LIST = ROOT + "/REQUEST_LIST";

        public void CreateList(CashRequestList list)
        {
            if (!PlayerPrefs.HasKey(CASH_REQUEST_LIST)) 
            {
                PlayerPrefs.SetString(CASH_REQUEST_LIST, JsonConvert.SerializeObject(list));
            }
        }

        public bool GetChargeIsRead(int SelectedIdx)
        {
            if (PlayerPrefs.HasKey(CASH_REQUEST_LIST))
            {
                var jsonData = PlayerPrefs.GetString(CASH_REQUEST_LIST);
                var data = JsonConvert.DeserializeObject<CashRequestList>(jsonData);

                foreach (var item in data.CashList)
                {
                    if (item.Idx == SelectedIdx)
                    {
                        return item.IsRead;
                    }
                }

                return false;
            }
            else throw new ChargeException($"Could not find key '{CASH_REQUEST_LIST}'");
        }

        public void InsertItem(CashRequest insertItem)
        {
            if (PlayerPrefs.HasKey(CASH_REQUEST_LIST))
            {
                var jsonData = PlayerPrefs.GetString(CASH_REQUEST_LIST);
                var data = JsonConvert.DeserializeObject<CashRequestList>(jsonData);

                var updateList = new List<CashRequest>();

                bool isExist = false;

                foreach(var item in data.CashList)
                {
                    if(item.Idx == insertItem.Idx)
                    {
                        isExist = true;
                        break;
                    }
                }

                updateList = data.CashList;

                if (!isExist)
                {    
                    updateList.Add(insertItem);
                }

                var updateJson = JsonConvert.SerializeObject(new CashRequestList()
                {
                    CashList = updateList
                });

                PlayerPrefs.SetString(CASH_REQUEST_LIST, updateJson);
            }
            else throw new ChargeException($"Could not find key '{CASH_REQUEST_LIST}'");
        }

        public CashRequestList ReadList()
        {
            if (PlayerPrefs.HasKey(CASH_REQUEST_LIST))
            {
                var prefsData = PlayerPrefs.GetString(CASH_REQUEST_LIST);

                return JsonConvert.DeserializeObject<CashRequestList>(prefsData);
            }
            else throw new ChargeException($"Could not find key '{CASH_REQUEST_LIST}'");
        }

        public void RemoveItem(CashRequest removeItem)
        {
            if (PlayerPrefs.HasKey(CASH_REQUEST_LIST))
            {
                var json_data = PlayerPrefs.GetString(CASH_REQUEST_LIST);
                var data = JsonConvert.DeserializeObject<CashRequestList>(json_data);

                var updateList = new List<CashRequest>();

                foreach (var item in data.CashList)
                {
                    if (item.Idx != removeItem.Idx)
                    {
                        updateList.Add(item);
                    }
                }

                var update_json = JsonConvert.SerializeObject(new CashRequestList()
                {
                    CashList = updateList
                });

                PlayerPrefs.SetString(CASH_REQUEST_LIST, update_json);
            }
            else throw new ChargeException($"Could not find key '{CASH_REQUEST_LIST}'");
        }

        public void ShowList()
        {
            Debug.Log(PlayerPrefs.GetString(CASH_REQUEST_LIST));
        }

        public void UpdateChargeRead(int selectedIdx)
        {
            if (PlayerPrefs.HasKey(CASH_REQUEST_LIST))
            {
                var json_data = PlayerPrefs.GetString(CASH_REQUEST_LIST);
                var data = JsonConvert.DeserializeObject<CashRequestList>(json_data);

                var updateList = new List<CashRequest>();

                foreach (var item in data.CashList)
                {
                    if (item.Idx == selectedIdx)
                    {
                        var UpdateItem = new CashRequest()
                        {
                            Idx = item.Idx,
                            Status = item.Status,
                            IsRead = true,
                        };

                        updateList.Add(UpdateItem);
                    }
                    else updateList.Add(item);
                }

                var updateJson = JsonConvert.SerializeObject(new CashRequestList()
                {
                    CashList = updateList
                });

                PlayerPrefs.SetString(CASH_REQUEST_LIST, updateJson);
            }
            else throw new ChargeException($"Could not find key '{CASH_REQUEST_LIST}'");
        }

        public void UpdateItem(CashRequest updateItem)
        {
            if (PlayerPrefs.HasKey(CASH_REQUEST_LIST))
            {
                var jsonData = PlayerPrefs.GetString(CASH_REQUEST_LIST);
                var data = JsonConvert.DeserializeObject<CashRequestList>(jsonData);

                var updateList = new List<CashRequest>();

                foreach (var item in data.CashList)
                {
                    if (item.Idx == updateItem.Idx)
                    {
                        var UpdateItem = new CashRequest()
                        {
                            Idx = item.Idx,
                            Status = item.Status,
                            IsRead = true,
                        };

                        updateList.Add(UpdateItem);
                    }
                    else updateList.Add(item);
                }

                var updateJson = JsonConvert.SerializeObject(new CashRequestList()
                {
                    CashList = updateList
                });

                PlayerPrefs.SetString(CASH_REQUEST_LIST, updateJson);
            }
            else throw new ChargeException($"Could not find key '{CASH_REQUEST_LIST}'");
        }
    }
}
