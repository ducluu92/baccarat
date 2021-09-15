using Module.Packets.Betting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Baccarrat.Battings
{
    public class BattingBag
    {

        #region SingleTone

        private static readonly Lazy<BattingBag> _instance = new Lazy<BattingBag>(() => new BattingBag());

        public static BattingBag Instance { get { return _instance.Value; } }

        public int innerRound = 0;
        #endregion

        List<BattingRecord> before;
        List<BattingRecord> now;
        RoomDataManager roomData;

        public BattingBag() 
        {
            before = new List<BattingRecord>();
            now = new List<BattingRecord>();
            roomData = RoomDataManager.Instance();
        }

        public void Batting(BETTINGTYPE type, long value) 
        {
            // 라운드 다르면 클리어
            ClearByUpdate();

            // 배팅정보 기록
            var b = now.Where(c => c.Type == type).FirstOrDefault();

            if (b == null)
            {
                b = new BattingRecord
                {
                    Round = innerRound,
                    Type = type,
                    Value = value
                };
                now.Add(b);
            }
            else 
            {
                b.Value += value;
            }

        }

        private void ClearByUpdate() 
        {
            var b = now.FirstOrDefault();

            if (b != null) 
            {
                if (innerRound != b.Round) 
                {
                    now.Clear();
                }
            }
        }

        public void Clear() 
        {
            now.Clear();
        }

        /// <summary>
        /// 쇼다운에서 호출
        /// </summary>
        public void Next() 
        {
            innerRound++; 

            if (now.Count > 0) 
            {
                before.Clear();
                before.AddRange(now);
                now.Clear();
            }
        }


        public List<BattingRecord> GetBefore() 
        {
            // 현재것 클리어
            now.Clear();

            // 기존거에 넣어주기
            var b = Copy(before);
            now.AddRange(b);

            return b;
        }


        private List<BattingRecord> Copy(List<BattingRecord> battings) 
        {
            List<BattingRecord> list = new List<BattingRecord>();

            foreach (var batting in battings)
            {
                var b = new BattingRecord
                {
                    Round = innerRound,
                    Type = batting.Type,
                    Value = batting.Value
                };

                list.Add(b);
            }

            return list;
        }

        public void SetBySp(List<SingleBatting> battings) 
        {
            now.Clear();

            foreach (var b in battings)
            {
                var x = new BattingRecord
                {
                    Round = innerRound,
                    Type = ConvertBettingType(b.BattingType),
                    Value = b.Batting
                };
                now.Add(x);
            }
        }


        private NetworkBettingType ConvertBettingType(BETTINGTYPE type)
        {
            switch (type)
            {
                case BETTINGTYPE.PPAIR:
                    return NetworkBettingType.PlayerPair;
                case BETTINGTYPE.PLAYER:
                    return NetworkBettingType.Player;
                case BETTINGTYPE.TIE:
                    return NetworkBettingType.Tie;
                case BETTINGTYPE.BANKER:
                    return NetworkBettingType.Banker;
                case BETTINGTYPE.BPAIR:
                    return NetworkBettingType.BankerPair;
            }

            return NetworkBettingType.Tie;
        }

        private BETTINGTYPE ConvertBettingType(NetworkBettingType type)
        {
            switch (type)
            {
                case NetworkBettingType.PlayerPair:
                    return BETTINGTYPE.PPAIR;
                case NetworkBettingType.Player:
                    return BETTINGTYPE.PLAYER;
                case NetworkBettingType.Tie:
                    return BETTINGTYPE.TIE;
                case NetworkBettingType.Banker:
                    return BETTINGTYPE.BANKER;
                case NetworkBettingType.BankerPair:
                    return BETTINGTYPE.BPAIR;
            }

            return BETTINGTYPE.NONE;
        }
    }
}
