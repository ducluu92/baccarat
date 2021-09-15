using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public class RememberMeLocalStorageService : IRememberMeService
    {
        private const string idKey = "REMEMBER_ID";
        private const string passwordKey = "REMEMBER_PASSWORD"; 

        public string GetID() 
        {
            return PlayerPrefs.GetString(idKey);
        }

        public void SetID(string id) 
        {
            PlayerPrefs.SetString(idKey, id);
        }

        public string GetPassword() 
        {
            return PlayerPrefs.GetString(passwordKey);
        }

        public void SetPassword(string password) 
        {
            PlayerPrefs.SetString(passwordKey, password);
        }
    }
}
