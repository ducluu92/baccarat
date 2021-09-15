using Assets.Scripts.Model.Charge;
using Assets.Scripts.Services.Lobby;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Settings
{
    public class UIChargeListItem : MonoBehaviour
    {
        private ICashChargeService _cashChargeService;

        public Text number;
        public Text Amount;
        public Text Status;
        public Text Date;
        public GameObject AccountButton;
        public GameObject AccountButtonReddot;
        public GameObject CashChargeReddot;
        public GameObject CashChargeListReddot;
        public GameObject RecordButton;
        public GameObject AccountButton_Exchange;
        public GameObject RecordButtton_Exchange;
        
        public string CompanyAccountOwner;
        public string CompanyAccountName;
        public string CompanyAccountNumber;
        public long Money;
        public long transferredMoney;
        public int Point;
        public int Bonus;
        public string ProcessedDate;

        void Start()
        {
            _cashChargeService = new CashChargeService();
        }

        public void ToggleOnAccountPopUp()
        {
            AccountButtonReddot.SetActive(false);
            CashChargeReddot.SetActive(false);
            CashChargeListReddot.SetActive(false);

            _cashChargeService.UpdateChargeRead(Convert.ToInt32(this.number.text));

            Lobby_AccountPopUp.instance.index.text = "No." + this.number.text;
            Lobby_AccountPopUp.instance.CompanyAccountOwner.text = this.CompanyAccountOwner;
            Lobby_AccountPopUp.instance.CompanyAccountName.text = this.CompanyAccountName;
            Lobby_AccountPopUp.instance.CompanyAccountNumber.text = this.CompanyAccountNumber;

            Lobby_AccountPopUp.instance.gameObject.SetActive(true);
        }

        public void ToggleOnRecordPopUp()
        {
            Lobby_RecordPopUp.instance.index.text = "No." + this.number.text;
            Lobby_RecordPopUp.instance.Money.text = string.Format("{0:n0}", this.Money);
            Lobby_RecordPopUp.instance.TransferredMoney.text = string.Format("{0:n0}", this.transferredMoney);
            Lobby_RecordPopUp.instance.Point.text = string.Format("{0:n0}", this.Point);
            Lobby_RecordPopUp.instance.Bonus.text = string.Format("{0:n0}", this.Bonus);
            Lobby_RecordPopUp.instance.ProcessedDate.text = this.ProcessedDate;

            Lobby_RecordPopUp.instance.gameObject.SetActive(true);
        }

        public void ToggleOnAccountPopUpExchange()
        {
            Lobby_AccountPopUp_Exchange.instance.index.text = "No." + this.number.text;
            Lobby_AccountPopUp_Exchange.instance.CompanyAccountOwner.text = this.CompanyAccountOwner;
            Lobby_AccountPopUp_Exchange.instance.CompanyAccountName.text = this.CompanyAccountName;
            Lobby_AccountPopUp_Exchange.instance.CompanyAccountNumber.text = this.CompanyAccountNumber;

            Lobby_AccountPopUp_Exchange.instance.gameObject.SetActive(true);
        }

        public void ToggleOnRecordPopUpExchange()
        {
            Lobby_RecordPopUp_Exchange.instance.index.text = "No." + this.number.text;
            Lobby_RecordPopUp_Exchange.instance.Money.text = string.Format("{0:n0}", this.Money);
            Lobby_RecordPopUp_Exchange.instance.TransferredMoney.text = string.Format("{0:n0}", this.transferredMoney);
            Lobby_RecordPopUp_Exchange.instance.ProcessedDate.text = this.ProcessedDate;

            Lobby_RecordPopUp_Exchange.instance.gameObject.SetActive(true);
        }
    }
}
