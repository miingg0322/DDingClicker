using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UserRegisterController : MonoBehaviour
{
    public int charLimit = 12;

    [Header("Register")]
    public InputField nameField;
    public InputField pwField;
    public InputField pwConfirmField;
  
    public GameObject registerPanel;
    public GameObject cannotRegisterPanel;
    public GameObject registerSuccessPanel;
    public Button registerCloseBtn;
    public Button registerSubmitBtn;
    public Button cannotRegisterCloseBtn;
    public Button registerSuccessCloseBtn;
    public Text cannotRegisterReasonText;

    [Header("Login")]
    public InputField loginName;
    public InputField loginPw;

    public GameObject loginPanel;
    public GameObject cannotLoginPanel;
    public GameObject loginSuccessPanel;
    public Button loginCloseBtn;
    public Button loginSubmitBtn;
    public Button cannotLoginCloseBtn;
    public Button loginSuccessCloseBtn;
    public Text cannotLoginReasonText;

    string nickName;
    string pw;
    Regex krRegex;
    Regex noRegex;
    int limit;
    void Start()
    {
        nameField.characterLimit = charLimit;
        registerCloseBtn.onClick.AddListener(() => UIManager.Instance.OnCancelBtnClicked(registerPanel));
        cannotRegisterCloseBtn.onClick.AddListener(() => UIManager.Instance.OnCancelBtnClicked(cannotRegisterPanel));
        registerSubmitBtn.onClick.AddListener(OnRegisterSubmit);
        registerSuccessCloseBtn.onClick.AddListener(() => UIManager.Instance.OnCancelBtnClicked(registerSuccessPanel));

        loginCloseBtn.onClick.AddListener(() => UIManager.Instance.OnCancelBtnClicked(loginPanel));
        cannotLoginCloseBtn.onClick.AddListener(() => UIManager.Instance.OnCancelBtnClicked(cannotLoginPanel));
        loginSubmitBtn.onClick.AddListener(()=>DataManager.Instance.Login(loginName.text, loginPw.text));
        registerSuccessCloseBtn.onClick.AddListener(() => UIManager.Instance.OnCancelBtnClicked(loginSuccessPanel));

        krRegex = new Regex(@"[°¡-ÆR]");
        noRegex = new Regex(@"[^a-zA-Z0-9°¡-ÆR]");
        limit = charLimit;
        nameField.onValueChanged.AddListener(NickNameRule);
        nameField.onEndEdit.AddListener(CheckRegex);
        //nameField.onValidateInput += delegate (string input, int charIndex, char addedChar) { return CheckNameValidation(addedChar); };
    }
    public void NickNameRule(string input)
    {

        int length = input.Length;
        int krCount = 0;

        for (int i = 0; i < length; i++)
        {
            if (krRegex.IsMatch(input[i].ToString()))
            {
                krCount++;
            }
        }
        nameField.characterLimit = charLimit - krCount;

        if(nameField.characterLimit - length < 0)
        {
            if (krRegex.IsMatch(input[length - 1].ToString()))
            {
                input = input.Substring(0, length - 1);
                nameField.textComponent.text = input;
                nameField.text = input;
            }                         
        }
    }
    public void CheckRegex(string input)
    {
        if(noRegex.IsMatch(input))
        {
            cannotRegisterReasonText.text = "´Ð³×ÀÓ¿¡ Æ¯¼ö¹®ÀÚ »ç¿ëÀÌ ºÒ°¡ÇÕ´Ï´Ù.";
            cannotRegisterPanel.SetActive(true);
            registerSubmitBtn.interactable = false;
        }
        else
        {
            registerSubmitBtn.interactable = true;
        }
    }
    private char CheckNameValidation(char charToValidate)
    {
        if (noRegex.IsMatch(charToValidate.ToString()))
        {
            charToValidate = '\0';
        }
        return charToValidate;
    }


    public async void OnRegisterSubmit()
    {
        pw = pwField.text;
        if (pw.Length != 6)
        {
            cannotRegisterReasonText.text = "ºñ¹Ð¹øÈ£ 6±ÛÀÚ";
            cannotRegisterPanel.SetActive(true);
            return;
        }

        if (!pw.Equals(pwConfirmField.text))
        {
            cannotRegisterReasonText.text = "ºñ¹Ð¹øÈ£ ºÒÀÏÄ¡";
            cannotRegisterPanel.SetActive(true);
            return;
        }

        nickName = nameField.text;
        if (nickName.Length < 4)
        {
            int krLengthConvert = 0;
            for (int i = 0; i < nickName.Length; i++)
            {
                if (krRegex.IsMatch(nickName[i].ToString()))
                {
                    krLengthConvert += 2;
                }
                else
                {
                    krLengthConvert++;
                }
            }

            if (krLengthConvert < 4)
            {
                cannotRegisterReasonText.text = "´Ð³×ÀÓ ÇÑ±Û 2±ÛÀÚ ÀÌ»ó, ¿µ¾î 4±ÛÀÚ ÀÌ»ó";
                cannotRegisterPanel.SetActive(true);
                return;
            }
        }
        bool isExists = await DataManager.Instance.CheckUserNameExists(nickName);
        if (isExists)
        {
            cannotRegisterReasonText.text = "´Ð³×ÀÓ Áßº¹";
            cannotRegisterPanel.SetActive(true);
            return;
        }

        string pwSHA256 = Encryptor.SHA256Encryt(pw);

        bool registerCompleted = await DataManager.Instance.RegisterNewUser(nickName, pwSHA256);
        if (registerCompleted)
        {
            PlayerPrefs.SetString("NickName", nickName);
            PlayerPrefs.SetString("Pw", pwSHA256);
            registerSuccessPanel.SetActive(true);
        }
    }

    public void LoginFailed()
    {
        cannotLoginReasonText.text = "·Î±×ÀÎ ½ÇÆÐ\n´Ð³×ÀÓ°ú ºñ¹Ð¹øÈ£¸¦ È®ÀÎÇØÁÖ¼¼¿ä";
        cannotLoginPanel.SetActive(true);
    }
    public void LoginSuccess()
    {
        loginSuccessPanel.SetActive(true);
    }

}

