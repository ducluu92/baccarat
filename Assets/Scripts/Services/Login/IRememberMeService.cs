using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Services
{
    public interface IRememberMeService
    {
        string GetID();
        void SetID(string id);
        string GetPassword();
        void SetPassword(string password);
    }
}
