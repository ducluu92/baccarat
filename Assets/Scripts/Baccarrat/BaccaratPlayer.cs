using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Module.Utils.Currency;
using System;

namespace Assets.Scripts.Baccarrat
{
	public class BaccaratPlayer : MonoBehaviour
	{
		public Room CurrentRoom; //플레이어가 위치하고 있는 룸 데이터입니다.
		public bool isPlayer = false; //이 플레이어가 유저인지 판별하는 불리언데이터입니다.

		public BETTINGTYPE Bettingtype; //플레이어가 배팅하고자 하는 배팅타입입니다.
		public int BettingAmount;       //플레이어가 배팅하고자 하는 배팅액수입니다.
		public int CurrentBetting;		//플레이어의 현재 금액입니다.
		public int CurrentBettingTotal; //플레이어의 현재 배팅한 총 금액입니다.
		public int DoNotBettingCount;		//플레이어의 총 배팅 횟수를 체크(만약에 5번이상 배팅안하면 아웃처리)

		public bool isBatting = false; //이동 애니메이션이 플레이중인지의 여부를 담고 있는 불리언입니다.
		public TrailRenderer chip; // 칩 이동시 표현되는 이펙트 게임오브젝트입니다. 트레일을 조절하기 위해 트레일렌더러 클래스 형식으로 호출합니다.

		public int BattingAmountPPair; //플레이어가 플레이어 페어에 배팅한 값입니다.
		public int BattingAmountPlayer; //플레이어가 플레이어에 배팅한 값입니다.
		public int BattingAmountTie; //플레이어가 타이에 배팅한 값입니다.
		public int BattingAmountBanker; //플레이어가 뱅커에 배팅한 값입니다.
		public int BattingAmountBPair; //플레이어가 뱅커 페어에 배팅한 값입니다.

		public int Lose_BattingAmountPPair; //플레이어가 플레이어 페어에 배팅한 값입니다.	(패배 카운트 체크 용)
		public int Lose_BattingAmountPlayer; //플레이어가 플레이어에 배팅한 값입니다.		(패배 카운트 체크 용)
		public int Lose_BattingAmountTie; //플레이어가 타이에 배팅한 값입니다.				(패배 카운트 체크 용)
		public int Lose_BattingAmountBanker; //플레이어가 뱅커에 배팅한 값입니다.			(패배 카운트 체크 용)
		public int Lose_BattingAmountBPair; //플레이어가 뱅커 페어에 배팅한 값입니다.		(패배 카운트 체크 용)

		public SpriteRenderer ChipSprite; //칩 스프라이트 렌더러 입니다.
		public Sprite[] ChipSprites; //금액별 칩 스프라이트 배열입니다.
		public Transform[] BettingPositions; //플레이어가 배팅하고자 하는 위치의 배열입니다.
		public ChipContainer[] ChipContainers; //플레이어가 가지고 있는 칩 스택 컨테이너들의 배열입니다.
		public ChipContainer StackEffectContainer; //칩이 쌓이는 이펙트를 연출하기 위한 칩 스택 컨테이너 입니다.
		public ChipContainer PPairStackEffectContainer; //PPair에 칩이 쌓이는 이펙트를 연출하기 위한 칩 스택 컨테이너 입니다.
		public ChipContainer BPairStackEffectContainer; //PPair에 칩이 쌓이는 이펙트를 연출하기 위한 칩 스택 컨테이너 입니다.
		public Transform MAXChip; //맥스벳 칩 객체입니다.

		public Text Text_UserCurrency;				//플레이어의 배팅 총액 텍스트
		public GameObject Obj_UserCurrentCurrency;	//플레이어의 현재 잔액 게임 오브젝트
		RectTransform Rect; //플레이어의 배팅 총액 텍스트에 대한 렉트트랜스폼 (UI 정렬을 위한 컴포넌트입니다.)
		RectTransform Rect_obj; //플레이어의 현재 잔액 게임 오브젝트에 대한 렉트트랜스폼 (UI 정렬을 위한 컴포넌트입니다.)

		public GameObject Obj_UserWinLoseContainer;         // 플레이어의 승리, 패배 (+,-)를 표시하는 컨테이너
		public Text Text_Winner;							// 승리 텍스트
		public Text Text_Lose;								// 패배 텍스트
		public Animation Anim_Winner;						// 승리 애니메이션
		public Animation Anim_Lose;							// 패배 애니메이션

		public AudioSource StackingAudio; //칩 스택을 실행할때의 사운드입니다.
		public AudioSource BettingAudio; //칩 베팅을 실핼할때의 사운드입니다.

		public GameObject UI_MaxEffect; //맥스 배팅 시 플레이어 위에 디스플레이되는 이펙트입니다.
		public GameObject[] UI_HightlightPanel = new GameObject[5]; //베팅 시 해당 패널이 하이라이팅되는 이펙트입니다.

		public GameObject Obj_IsPlayerEffect; // 플레이어인지 아닌지 체크하는 이펙트
		private bool OnPlayerEffect = false;

		private void Awake()
		{
			Rect =  Obj_UserCurrentCurrency.transform.parent.GetComponent<RectTransform>();
			Rect_obj =  Text_UserCurrency.transform.parent.GetComponent<RectTransform>();
		}

		void Update()
		{
			Text_UserCurrency.text = CurrencyConverter.Kor(this.CurrentBettingTotal);
			LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);

			if (CurrentBetting >= 0)
			{
				if (!Obj_UserCurrentCurrency.activeSelf)
					Obj_UserCurrentCurrency.SetActive(true);

				Obj_UserCurrentCurrency.GetComponent<Text>().text = CurrencyConverter.Kor(this.CurrentBetting);
				LayoutRebuilder.ForceRebuildLayoutImmediate(Rect_obj);
			}
			else
			{
				if (Obj_UserCurrentCurrency.activeSelf) Obj_UserCurrentCurrency.SetActive(false);
			}

			if (!OnPlayerEffect)
			{
				if (isPlayer)
				{
					if (!Obj_IsPlayerEffect.activeSelf)
					{
						Obj_IsPlayerEffect.SetActive(true);
						OnPlayerEffect = true;
					}
				}
			}
		}

		public void UpdateCurrentBetting()
		{
			var userSelectedRatingLimitProfit = UserInfoManager.selectedRating.UpperLimitBankerPair;

			var playerWinAmount = 0;
			var playerLoseAmount = 0;

			playerLoseAmount += Lose_BattingAmountPPair;

			playerLoseAmount += Lose_BattingAmountPlayer;

			playerLoseAmount += Lose_BattingAmountTie;

			playerLoseAmount += Lose_BattingAmountBanker;

			playerLoseAmount += Lose_BattingAmountBPair;
      
			// 상한가 체크 (상한가는 P.P., Tie, B.P. 3가지 만 체크함)
			var totalCurrentBetting = BattingAmountPPair + BattingAmountTie + BattingAmountBPair;

			if(totalCurrentBetting >= userSelectedRatingLimitProfit)
            {
				// 만약 상한가 보다 크다면 totalbetting = 상한가 + 배팅한 금액 (200만원 배팅해서 P.P. 이기면 1200만원 돌려줌) => 기존에는 1300만원을 돌려줘야했었음 ( 1100 당첨 + 200 배팅 )

				// 배팅 했던 금액 계산 후 더해줌
				CurrentBetting += BattingAmountPPair / 12;
				CurrentBetting += BattingAmountTie / 9;
				CurrentBetting += BattingAmountBPair / 12;

				// 상한가 1000만원 더해줌
				CurrentBetting += Convert.ToInt32(userSelectedRatingLimitProfit);

				// 클라이언트 표시 변수 금액 추가
				playerWinAmount += BattingAmountPPair / 12;
				playerWinAmount += BattingAmountTie / 9;
				playerWinAmount += BattingAmountBPair / 12;

				playerWinAmount += Convert.ToInt32(userSelectedRatingLimitProfit);
			}
			else
            {
				// 아니면 원래 대로 진행
				CurrentBetting += BattingAmountPPair;
				CurrentBetting += BattingAmountPlayer;
				CurrentBetting += BattingAmountTie;
				CurrentBetting += BattingAmountBanker;
				CurrentBetting += BattingAmountBPair;

				// 클라이언트 표시 변수 금액 추가
				playerWinAmount += BattingAmountPPair;
				playerWinAmount += BattingAmountPlayer;
				playerWinAmount += BattingAmountTie;
				playerWinAmount += BattingAmountBanker;
				playerWinAmount += BattingAmountBPair;
			}

			var resultCost = playerWinAmount - playerLoseAmount;

			if (resultCost > 0)
            {
				Obj_UserWinLoseContainer.SetActive(true);

				Text_Winner.text = "+ " + CurrencyConverter.Kor(resultCost);
				Anim_Winner.Play();
			}
			else if(resultCost < 0)
            {
				Obj_UserWinLoseContainer.SetActive(true);

				Text_Lose.text = "- " + CurrencyConverter.Kor(resultCost * -1);
				Anim_Lose.Play();
			}

			Lose_BattingAmountPPair = 0;
			Lose_BattingAmountPlayer = 0;
			Lose_BattingAmountTie = 0;
			Lose_BattingAmountBanker = 0;
			Lose_BattingAmountBPair = 0;

			StartCoroutine(Co_CloseWinLoseContainer());
		}

		private IEnumerator Co_CloseWinLoseContainer()
        {
			yield return new WaitForSeconds(1.5f);
			Obj_UserWinLoseContainer.SetActive(false);
		}

		public void changebettingtype(int type) //플레이어가 배팅하고자 하는 배팅 타입을 변경합니다.
		{
			this.Bettingtype = (BETTINGTYPE)type;
		}

		public void changebettingamount(int amount) //플레이어가 배팅하고자 하는 배팅 수량을 변경합니다.
		{
			this.BettingAmount = amount;
		}

		public void ExecuteBetting() //베팅함수를 호출할때 해당 메서드를 사용해주세요! (맥스는 아래에 있습니다)
		{
			DoNotBettingCount = 0;

			if (this.Bettingtype == BETTINGTYPE.NONE)
				return;

			CurrentBettingTotal += BettingAmount;
			CurrentBetting -= BettingAmount;
			BettingAudio.Play();

			switch (this.Bettingtype)
			{
				case BETTINGTYPE.PPAIR:
					BattingAmountPPair += BettingAmount;
					Lose_BattingAmountPPair += BettingAmount;
					this.Bet(this.BattingAmountPPair, bettingtypeindex: (int)this.Bettingtype - 1);
					break;
				case BETTINGTYPE.PLAYER:
					BattingAmountPlayer += BettingAmount;
					Lose_BattingAmountPlayer += BettingAmount;
					this.Bet(this.BattingAmountPlayer, bettingtypeindex: (int)this.Bettingtype - 1);
					break;
				case BETTINGTYPE.TIE:
					BattingAmountTie += BettingAmount;
					Lose_BattingAmountTie += BettingAmount;
					this.Bet(this.BattingAmountTie, bettingtypeindex: (int)this.Bettingtype - 1);
					break;
				case BETTINGTYPE.BANKER:
					BattingAmountBanker += BettingAmount;
					Lose_BattingAmountBanker += BettingAmount;
					this.Bet(this.BattingAmountBanker, bettingtypeindex: (int)this.Bettingtype - 1);
					break;
				case BETTINGTYPE.BPAIR:
					BattingAmountBPair += BettingAmount;
					Lose_BattingAmountBPair += BettingAmount;
					this.Bet(this.BattingAmountBPair, bettingtypeindex: (int)this.Bettingtype - 1);
					break;
			}
	
		}

		public void ExecuteBettingMAX() // 맥스 베팅을 실행합니다.
		{
			DoNotBettingCount = 0;

			if (!isBatting && this.Bettingtype != BETTINGTYPE.NONE)
			{
				var managerInterceptor = RoomDataManager.Instance();

				int sum = (int)this.Bettingtype;

				BettingAudio.Play();
				UI_MaxEffect.SetActive(true);

				if (CurrentBetting < managerInterceptor.MaxBetting)
                {
					CurrentBettingTotal += CurrentBetting;

					switch (this.Bettingtype)
					{
						case BETTINGTYPE.PPAIR:
							BattingAmountPPair += CurrentBetting;
							Lose_BattingAmountPPair += CurrentBetting;
							break;
						case BETTINGTYPE.PLAYER:
							BattingAmountPlayer += CurrentBetting;
							Lose_BattingAmountPlayer += CurrentBetting;
							break;
						case BETTINGTYPE.TIE:
							BattingAmountTie += CurrentBetting;
							Lose_BattingAmountTie += CurrentBetting;
							break;
						case BETTINGTYPE.BANKER:
							BattingAmountBanker += CurrentBetting;
							Lose_BattingAmountBanker += CurrentBetting;
							break;
						case BETTINGTYPE.BPAIR:
							BattingAmountBPair += CurrentBetting;
							Lose_BattingAmountBPair += CurrentBetting;
							break;
					}

					CurrentBetting = 0;
				}
                else
                {
					CurrentBettingTotal += managerInterceptor.MaxBetting;

					switch (this.Bettingtype)
					{
						case BETTINGTYPE.PPAIR:
							BattingAmountPPair += managerInterceptor.MaxBetting;
							Lose_BattingAmountPPair += managerInterceptor.MaxBetting;
							break;
						case BETTINGTYPE.PLAYER:
							BattingAmountPlayer += managerInterceptor.MaxBetting;
							Lose_BattingAmountPlayer += managerInterceptor.MaxBetting;
							break;
						case BETTINGTYPE.TIE:
							BattingAmountTie += managerInterceptor.MaxBetting;
							Lose_BattingAmountTie += managerInterceptor.MaxBetting;
							break;
						case BETTINGTYPE.BANKER:
							BattingAmountBanker += managerInterceptor.MaxBetting;
							Lose_BattingAmountBanker += managerInterceptor.MaxBetting;
							break;
						case BETTINGTYPE.BPAIR:
							BattingAmountBPair += managerInterceptor.MaxBetting;
							Lose_BattingAmountBPair += managerInterceptor.MaxBetting;
							break;
					}

					CurrentBetting -= managerInterceptor.MaxBetting;
				}
				
				MAXChip.transform.DOMove(BettingPositions[sum - 1].position,0.3f)
				.SetEase(Ease.OutQuint);
			}

		}

		private void Bet(int amount,int bettingtypeindex) // 배팅한 값과 배팅 타입에 따라 플레이어 페어에 배팅액을 더합니다.
		{
			if(!isBatting) // 애니메이션 재생여부 불리언 조건을 판단합니다.
			{
				switch(amount) //배팅 금액에 따라 칩 스프라이트를 변경합니다.
				{
					case 1000:
						ChipSprite.sprite = ChipSprites[0];
						break;
					case 10000:
						ChipSprite.sprite = ChipSprites[1];
						break;
					case 100000:
						ChipSprite.sprite = ChipSprites[2];
						break;
					case 1000000:
						ChipSprite.sprite = ChipSprites[3];
						break;

				}

				isBatting = true; // 애니메이션 재생여부 불리언을 켭니다.

				chip.gameObject.SetActive(true);
				chip.transform.DOMove(BettingPositions[bettingtypeindex].position,0.3f) // 이펙트 게임오브젝트를 트윈을 이용해 움직입니다.
				.SetEase(Ease.OutQuint)
				.OnComplete( () => done(ChipContainers[bettingtypeindex], BettingPositions[bettingtypeindex].position, amount) ); // 트윈이 완료되면 자동으로 컨테이너 업데이트를 실행하는 콜백함수를 실행합니다.
			}

		}

		public void done(ChipContainer container,Vector3 position, int amount) //특정 칩컨테이너의 위치를 직접 정하고 업데이트 합니다.
		{
			isBatting = false;

			// 이펙트 칩을 원위치로 순간 이동하면서 칩 트레일 렌더러를 초기화 합니다. (원 위치로 돌아가면서 잔상이 남지 않기 위함.)
			chip.enabled = false;
			chip.transform.position = transform.position;
			chip.Clear();
			chip.enabled = true;

			float x = position.x;
			float y = position.y;

			container.ChipContainer_1000.position = new Vector3(x, y, 0);
			container.ChipContainer_10000.position = new Vector3(x + 0.35f, y, 0);
			container.ChipContainer_100000.position = new Vector3(x, y - 0.17f, 0);
			container.ChipContainer_1000000.position = new Vector3(x + 0.35f, y - 0.17f, 0);
			StartCoroutine(container.UpdateChipContainer(amount)); // 해당 칩 컨테이너에 대한 업데이트 코루틴을 실행합니다.
		}

		public void doneMAX() //맥스 베팅 트윈이 끝나면 실행되는 콜백함수입니다.
		{
			isBatting = false;

			chip.enabled = false;
			chip.transform.position = transform.position;
			chip.Clear();
			chip.enabled = true;
		}


		public void RevertAllWithoutEffect() // 모든 컨테이너에 대해 리버트를 실행합니다.
		{
			this.Bettingtype = BETTINGTYPE.NONE;

			UI_MaxEffect.SetActive(false);

			MAXChip.DOMove(transform.position, 0.1f)
			.SetEase(Ease.OutQuint);

			CurrentBetting += BattingAmountPPair;
			CurrentBetting += BattingAmountPlayer;
			CurrentBetting += BattingAmountTie;
			CurrentBetting += BattingAmountBanker;
			CurrentBetting += BattingAmountBPair;

			BattingAmountPPair = 0;
			BattingAmountPlayer = 0;
			BattingAmountTie = 0;
			BattingAmountBanker = 0;
			BattingAmountBPair = 0;

			Lose_BattingAmountPPair = 0;
			Lose_BattingAmountBPair = 0;
			Lose_BattingAmountBanker = 0;
			Lose_BattingAmountTie = 0;
			Lose_BattingAmountPlayer = 0;

			CurrentBettingTotal = 0;

			for (int i = 0; i < 5; i++)
				revert(ChipContainers[i]);
			this.revert(StackEffectContainer);
			this.revert(BPairStackEffectContainer);
			this.revert(PPairStackEffectContainer);
		}

		public void RevertAll() // 모든 컨테이너에 대해 리버트를 실행합니다.
		{
			this.Bettingtype = BETTINGTYPE.NONE;

			UI_MaxEffect.SetActive(false);

			MAXChip.DOMove(transform.position,0.1f)
			.SetEase(Ease.OutQuint);

			UpdateCurrentBetting();

			BattingAmountPPair = 0;
			BattingAmountPlayer = 0;
			BattingAmountTie = 0;
			BattingAmountBanker = 0;
			BattingAmountBPair = 0;

			Lose_BattingAmountPPair = 0;
			Lose_BattingAmountBPair = 0;
			Lose_BattingAmountBanker = 0;
			Lose_BattingAmountTie = 0;
			Lose_BattingAmountPlayer = 0;

			CurrentBettingTotal = 0;

			if(isPlayer)
				CurrentRoom.UpdateBoardAtOnce();

			for (int i = 0; i < 5; i++)
			{
				revert(ChipContainers[i]);
			}
			this.revert(StackEffectContainer);
			this.revert(BPairStackEffectContainer);
			this.revert(PPairStackEffectContainer);
		}

		public void RevertAllWithWinner(ChipContainer container) // 모든 컨테이너에 대해 리버트를 실행합니다. (container => winner에 대한 컨테이너)
		{
			this.Bettingtype = BETTINGTYPE.NONE;

			UI_MaxEffect.SetActive(false);

			MAXChip.DOMove(transform.position, 0.1f)
			.SetEase(Ease.OutQuint);

			UpdateCurrentBetting();

			BattingAmountPPair = 0;
			BattingAmountPlayer = 0;
			BattingAmountTie = 0;
			BattingAmountBanker = 0;
			BattingAmountBPair = 0;

			Lose_BattingAmountPPair = 0;
			Lose_BattingAmountBPair = 0;
			Lose_BattingAmountBanker = 0;
			Lose_BattingAmountTie = 0;
			Lose_BattingAmountPlayer = 0;

			CurrentBettingTotal = 0;

			if(isPlayer)
			CurrentRoom.UpdateBoardAtOnce();

			for (int i = 0; i < 5; i++)
			{
				if (ChipContainers[i] != container) fadeOut(container);
				else revert(ChipContainers[i]);
			}
			this.revert(StackEffectContainer);
			this.revert(BPairStackEffectContainer);
			this.revert(PPairStackEffectContainer);
		}

		public void RevertAllWithWinners(List<ChipContainer> containerList) // 모든 컨테이너에 대해 리버트를 실행합니다. (container => winner에 대한 컨테이너)
		{
			this.Bettingtype = BETTINGTYPE.NONE;

			MAXChip.DOMove(transform.position, 0.1f)
			.SetEase(Ease.OutQuint);

			UI_MaxEffect.SetActive(false);

			UpdateCurrentBetting();

			BattingAmountPPair = 0;
			BattingAmountPlayer = 0;
			BattingAmountTie = 0;
			BattingAmountBanker = 0;
			BattingAmountBPair = 0;

			CurrentBettingTotal = 0;

			if(isPlayer)
			CurrentRoom.UpdateBoardAtOnce();

			for (int i = 0; i < 5; i++)
			{
				if (!containerList.Contains(ChipContainers[i]))
				{
					fadeOut(ChipContainers[i]);
				}
				else
				{
					revert(ChipContainers[i]);
				}
			}
			this.revert(StackEffectContainer);
			this.revert(BPairStackEffectContainer);
			this.revert(PPairStackEffectContainer);
		}

		public void revert(ChipContainer container) //컨테이너 한개 단위마다 리버트를 실행합니다.
		{
			container.ChipContainer_1000.DOMove(transform.position,0.1f)
			.SetEase(Ease.OutQuint);

			container.ChipContainer_10000.DOMove(transform.position,0.1f)
			.SetEase(Ease.OutQuint);

			container.ChipContainer_100000.DOMove(transform.position,0.1f)
			.SetEase(Ease.OutQuint);

			container.ChipContainer_1000000.DOMove(transform.position,0.1f)
			.SetEase(Ease.OutQuint)
			.OnComplete(() => container.ClearAllChips(chip));
		}

		public void fadeOut(ChipContainer container)
        {
			chip.enabled = false;
			chip.transform.position = transform.position;
			chip.Clear();

			for(int i=0; i<4;i++)
            {
                container.PlayerEffect[i].GetComponent<SpriteRenderer>().DOFade(0, 0.5f);
            }

			for(int i=0;i<9;i++)
			{
				container.ChipContainer_chips_1000[i].GetComponent<SpriteRenderer>().DOFade(0, 0.5f)
				.SetEase(Ease.OutQuint);
				container.ChipContainer_chips_10000[i].GetComponent<SpriteRenderer>().DOFade(0, 0.5f)
				.SetEase(Ease.OutQuint);
				container.ChipContainer_chips_100000[i].GetComponent<SpriteRenderer>().DOFade(0, 0.5f)
				.SetEase(Ease.OutQuint);
				container.ChipContainer_chips_1000000[i].GetComponent<SpriteRenderer>().DOFade(0, 0.5f)
				.SetEase(Ease.OutQuint);
			}

			container.ChipContainer_chips_1000[9].GetComponent<SpriteRenderer>().DOFade(0, 0.5f)
				.SetEase(Ease.OutQuint);
			container.ChipContainer_chips_10000[9].GetComponent<SpriteRenderer>().DOFade(0, 0.5f)
				.SetEase(Ease.OutQuint);
			container.ChipContainer_chips_100000[9].GetComponent<SpriteRenderer>().DOFade(0, 0.5f)
				.SetEase(Ease.OutQuint);
			container.ChipContainer_chips_1000000[9].GetComponent<SpriteRenderer>().DOFade(0, 0.5f)
				.SetEase(Ease.OutQuint)
				.OnComplete(() => revert(container));
		}

		public void ExecuteChipStackEffect(ChipContainer container) //칩컨테이너 위에 칩이 쌓이는 효과를 연출합니다.
		{
			StackingAudio.Play();

			StackEffectContainer.gameObject.SetActive(false);

			StackEffectContainer.UpdateChipContainerNonCo(container.amount);

			var Pos1000 =  container.TopChipPosition(ref container.ChipContainer_chips_1000);
			var Pos10000 =  container.TopChipPosition(ref container.ChipContainer_chips_10000);
			var Pos100000 =  container.TopChipPosition(ref container.ChipContainer_chips_100000);
			var Pos1000000 =  container.TopChipPosition(ref container.ChipContainer_chips_1000000);

			StackEffectContainer.ChipContainer_1000.position = new Vector3(Pos1000.x,Pos1000.y + 0.6f);
			StackEffectContainer.ChipContainer_10000.position = new Vector3(Pos10000.x,Pos10000.y + 0.6f);
			StackEffectContainer.ChipContainer_100000.position = new Vector3(Pos100000.x,Pos100000.y + 0.6f);
			StackEffectContainer.ChipContainer_1000000.position = new Vector3(Pos1000000.x,Pos1000000.y + 0.6f);

			StackEffectContainer.gameObject.SetActive(true);

			StackEffectContainer.ChipContainer_1000.DOMove(new Vector3(Pos1000.x,Pos1000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);
			StackEffectContainer.ChipContainer_10000.DOMove(new Vector3(Pos10000.x,Pos10000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);
			StackEffectContainer.ChipContainer_100000.DOMove(new Vector3(Pos100000.x,Pos100000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);
			StackEffectContainer.ChipContainer_1000000.DOMove(new Vector3(Pos1000000.x,Pos1000000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);

			//StackEffectContainer.transform.DOMove( new Vector3(container.ChipContainer_1000.transform.position.x, container.ChipContainer_1000.transform.position.y + 0.5f, 0),0.5f );

		}

		public void ExecuteChipStackEffect_BPair(ChipContainer container) //칩컨테이너 위에 칩이 쌓이는 효과를 연출합니다.
		{
			StackingAudio.Play();

			BPairStackEffectContainer.gameObject.SetActive(false);

			BPairStackEffectContainer.UpdateChipContainerNonCo(container.amount);

			var Pos1000 =  container.TopChipPosition(ref container.ChipContainer_chips_1000);
			var Pos10000 =  container.TopChipPosition(ref container.ChipContainer_chips_10000);
			var Pos100000 =  container.TopChipPosition(ref container.ChipContainer_chips_100000);
			var Pos1000000 =  container.TopChipPosition(ref container.ChipContainer_chips_1000000);

			BPairStackEffectContainer.ChipContainer_1000.position = new Vector3(Pos1000.x,Pos1000.y + 0.6f);
			BPairStackEffectContainer.ChipContainer_10000.position = new Vector3(Pos10000.x,Pos10000.y + 0.6f);
			BPairStackEffectContainer.ChipContainer_100000.position = new Vector3(Pos100000.x,Pos100000.y + 0.6f);
			BPairStackEffectContainer.ChipContainer_1000000.position = new Vector3(Pos1000000.x,Pos1000000.y + 0.6f);

			BPairStackEffectContainer.gameObject.SetActive(true);

			BPairStackEffectContainer.ChipContainer_1000.DOMove(new Vector3(Pos1000.x,Pos1000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);
			BPairStackEffectContainer.ChipContainer_10000.DOMove(new Vector3(Pos10000.x,Pos10000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);
			BPairStackEffectContainer.ChipContainer_100000.DOMove(new Vector3(Pos100000.x,Pos100000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);
			BPairStackEffectContainer.ChipContainer_1000000.DOMove(new Vector3(Pos1000000.x,Pos1000000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);

			//StackEffectContainer.transform.DOMove( new Vector3(container.ChipContainer_1000.transform.position.x, container.ChipContainer_1000.transform.position.y + 0.5f, 0),0.5f );

		}

		public void ExecuteChipStackEffect_PPair(ChipContainer container) //칩컨테이너 위에 칩이 쌓이는 효과를 연출합니다.
		{
			StackingAudio.Play();

			PPairStackEffectContainer.gameObject.SetActive(false);

			PPairStackEffectContainer.UpdateChipContainerNonCo(container.amount);

			var Pos1000 =  container.TopChipPosition(ref container.ChipContainer_chips_1000);
			var Pos10000 =  container.TopChipPosition(ref container.ChipContainer_chips_10000);
			var Pos100000 =  container.TopChipPosition(ref container.ChipContainer_chips_100000);
			var Pos1000000 =  container.TopChipPosition(ref container.ChipContainer_chips_1000000);

			PPairStackEffectContainer.ChipContainer_1000.position = new Vector3(Pos1000.x,Pos1000.y + 0.6f);
			PPairStackEffectContainer.ChipContainer_10000.position = new Vector3(Pos10000.x,Pos10000.y + 0.6f);
			PPairStackEffectContainer.ChipContainer_100000.position = new Vector3(Pos100000.x,Pos100000.y + 0.6f);
			PPairStackEffectContainer.ChipContainer_1000000.position = new Vector3(Pos1000000.x,Pos1000000.y + 0.6f);

			PPairStackEffectContainer.gameObject.SetActive(true);

			PPairStackEffectContainer.ChipContainer_1000.DOMove(new Vector3(Pos1000.x,Pos1000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);
			PPairStackEffectContainer.ChipContainer_10000.DOMove(new Vector3(Pos10000.x,Pos10000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);
			PPairStackEffectContainer.ChipContainer_100000.DOMove(new Vector3(Pos100000.x,Pos100000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);
			PPairStackEffectContainer.ChipContainer_1000000.DOMove(new Vector3(Pos1000000.x,Pos1000000.y + 0.05f),0.3f).
				SetEase(Ease.OutBounce);

			//StackEffectContainer.transform.DOMove( new Vector3(container.ChipContainer_1000.transform.position.x, container.ChipContainer_1000.transform.position.y + 0.5f, 0),0.5f );

		}

		public IEnumerator WinnerEffect(List<ChipContainer> containerList,ChipContainer containerListBP = null,ChipContainer containerListPP = null)
        {
			foreach(var container in containerList )
            {
				yield return new WaitForSeconds(0.1f);
				
				ExecuteChipStackEffect(container);
			}

			if(containerListBP != null)
			ExecuteChipStackEffect_BPair(containerListBP);
			if(containerListPP != null)
			ExecuteChipStackEffect_PPair(containerListPP);
			
			yield return new WaitForSeconds(2.0f);

			RevertAllWithWinners(containerList);
		}

		public int GetBettingAmountWithIndex(int index)
        {
			// 0 = PPair
			// 1 = Player
			// 2 = Tie
			// 3 = Banker
			// 4 = BPair

			switch (index)
            {
				case 0:
					return BattingAmountPPair;
				case 1:
					return BattingAmountPlayer;
				case 2:
					return BattingAmountTie;
				case 3:
					return BattingAmountBanker;
				case 4:
					return BattingAmountBPair;
			}

			return -1;
		}

		public void Clear()
		{
			UI_MaxEffect.SetActive(false);

			CurrentBetting = -1;
			DoNotBettingCount = 0;

			BattingAmountPPair = 0;
			BattingAmountPlayer = 0;
			BattingAmountTie = 0;
			BattingAmountBanker = 0;
			BattingAmountBPair = 0;

			Lose_BattingAmountPPair = 0;
			Lose_BattingAmountBPair = 0;
			Lose_BattingAmountBanker = 0;
			Lose_BattingAmountTie = 0;
			Lose_BattingAmountPlayer = 0;

			CurrentBettingTotal = 0;
		}

		public void ShowData()
        {
			Debug.Log("CurrentBetting > " + CurrentBetting);
        }
	}
}

