using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Handles the spin button, bet selection, money, and popup flow.</summary>
public class SlotHandle : MonoBehaviour
{
    [Header("Core Game")]
    [SerializeField] private SlotMachineGame slotMachine;

    [Header("Button Press Objects")]
    [SerializeField] private GameObject objectToHide;
    [SerializeField] private GameObject objectToShow;
    [SerializeField, Min(0f)] private float objectSwapDuration = 2f;

    [Header("Betting")]
    [SerializeField] private GameObject betPopup;
    [SerializeField] private Button bet100Button;
    [SerializeField] private Button bet500Button;
    [SerializeField] private Button bet1000Button;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField, Min(0)] private int startingMoney = 5000;
    [SerializeField, Min(1)] private int winMultiplier = 10;

    [Header("Win")]
    [SerializeField] private GameObject winPopup;
    [SerializeField] private TMP_Text winAmountText;

    private int money;
    private int currentBet;
    private bool spinning;
    private Coroutine objectSwapCoroutine;
    private Button spinButton;

    private void Awake()
    {
        money = startingMoney;
        if (betPopup != null) betPopup.SetActive(false);
        if (winPopup != null) winPopup.SetActive(false);

        spinButton = GetComponent<Button>();
        if (spinButton != null) spinButton.onClick.AddListener(Spin);
        if (bet100Button != null) bet100Button.onClick.AddListener(Bet100);
        if (bet500Button != null) bet500Button.onClick.AddListener(Bet500);
        if (bet1000Button != null) bet1000Button.onClick.AddListener(Bet1000);
        UpdateMoneyDisplay();
    }

    private void OnDestroy()
    {
        if (spinButton != null) spinButton.onClick.RemoveListener(Spin);
        if (bet100Button != null) bet100Button.onClick.RemoveListener(Bet100);
        if (bet500Button != null) bet500Button.onClick.RemoveListener(Bet500);
        if (bet1000Button != null) bet1000Button.onClick.RemoveListener(Bet1000);
    }

    public void Spin()
    {
        if (spinning) return;
        if (slotMachine == null || !slotMachine.IsConfigured)
        {
            Debug.LogError("Assign a configured SlotMachineGame component in the Inspector.", this);
            return;
        }
        if (betPopup == null)
        {
            Debug.LogError("Assign the Bet Popup in the Inspector.", this);
            return;
        }

        betPopup.SetActive(true);
    }

    public void Bet100() { PlaceBet(100); }
    public void Bet500() { PlaceBet(500); }
    public void Bet1000() { PlaceBet(1000); }

    private void PlaceBet(int amount)
    {
        if (spinning) return;
        if (money < amount)
        {
            Debug.Log("Not enough money to place the " + amount + " bet.", this);
            return;
        }

        currentBet = amount;
        money -= currentBet;
        UpdateMoneyDisplay();
        betPopup.SetActive(false);

        if (objectSwapCoroutine != null)
            StopCoroutine(objectSwapCoroutine);
        objectSwapCoroutine = StartCoroutine(SwapObjectsTemporarily());
        StartCoroutine(PlaySpin());
    }

    private IEnumerator SwapObjectsTemporarily()
    {
        if (objectToHide != null) objectToHide.SetActive(false);
        if (objectToShow != null) objectToShow.SetActive(true);

        yield return new WaitForSeconds(objectSwapDuration);

        if (objectToShow != null) objectToShow.SetActive(false);
        if (objectToHide != null) objectToHide.SetActive(true);
        objectSwapCoroutine = null;
    }

    private IEnumerator PlaySpin()
    {
        spinning = true;
        if (spinButton != null) spinButton.interactable = false;

        yield return StartCoroutine(slotMachine.Spin());

        int[] results = slotMachine.LastResults;
        if (results == null || results.Length == 0)
        {
            // If the core could not spin, refund the wager instead of consuming it.
            money += currentBet;
            UpdateMoneyDisplay();
            Debug.LogError("Spin did not produce a result; the bet was refunded.", this);
        }
        else
        {
            int payout = slotMachine.LastSpinWon ? currentBet * winMultiplier : 0;
            money += payout;
            UpdateMoneyDisplay();

            string outcome = slotMachine.LastSpinWon
                ? "WIN - all reels match. Payout: " + payout
                : "No win. Bet lost: " + currentBet;
            string resultDetails = "Spin result: ";
            for (int i = 0; i < results.Length; i++)
            {
                if (i > 0) resultDetails += ", ";
                resultDetails += "Reel " + (i + 1) + " = " + slotMachine.SymbolNameAt(results[i]);
            }
            Debug.Log(resultDetails + " | " + outcome, this);
            Debug.Log("Money remaining: " + money, this);

            if (slotMachine.LastSpinWon && winPopup != null)
            {
                if (winAmountText != null)
                    winAmountText.text = "You Won $" + payout;
                winPopup.SetActive(true);
            }
        }

        spinning = false;
        if (spinButton != null) spinButton.interactable = true;
    }

    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
            moneyText.text = "$ " + money;
    }
}
