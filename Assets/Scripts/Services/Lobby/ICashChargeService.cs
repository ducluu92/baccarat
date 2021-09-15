using Assets.Scripts.Model.Charge;

namespace Assets.Scripts.Services.Lobby
{
    public interface ICashChargeService
    {
        void CreateList(CashRequestList list);
        CashRequestList ReadList();

        void RemoveItem(CashRequest item);
        void UpdateItem(CashRequest item);
        void InsertItem(CashRequest item);

        void UpdateChargeRead(int idx);
        bool GetChargeIsRead(int idx);

        void ShowList();
    }
}
