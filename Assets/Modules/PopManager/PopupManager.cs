using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using TMPro;

using UnityEngine.UI;

using System;
using System.Linq;

using static Modules.Utility.Utility;
namespace ViitorCloud.Utility.PopupManager {
    public enum PopupType {
        OneButton,
        TwoButton,
        ThreeButton,
        ToastMassage
    };
    public enum MessageType {
        Info,
        Warning,
        Error
    };

    public class ButtonProperties {
        public Action buttonAction_;
        public string buttonName;
    }

    public class PopupManager : MonoBehaviour {

        public static PopupManager Instance { set; get; }

        [SerializeField] GameObject loadingPopup_;
        [SerializeField] TextMeshProUGUI loadingText;
        [SerializeField] int maxLoadingDot = 5;
        [SerializeField] GameObject popup_;
        [SerializeField] TextMeshProUGUI text_TextMeshProUGUI_;
        [SerializeField] Button leftButton_;
        [SerializeField] Button rightButton_;
        [SerializeField] Button thirdButton_;
        [SerializeField] Image typeMsgImage_;

        [SerializeField] Sprite infoSprite_;
        [SerializeField] Sprite warningSprite_;
        [SerializeField] Sprite errorSprite_;

        [SerializeField] float padding_;

        [SerializeField] TextMeshProUGUI toastMessage_;
        [SerializeField] int duration_ = 5;
        [SerializeField] GameObject toastPanel_;

        private List<Sprite> loadingImages;
        [SerializeField] Image loading;

        private void Awake() {
            Instance = this;
            loadingImages = Resources.LoadAll<Sprite>("LoadingImage/").ToList();
            if (loadingImages == null) {
                loadingImages = Resources.LoadAll<Sprite>("LoadingImageBase/").ToList();
            }
        }

        public void ShowLoading() {
            ClosePopup();
            loadingPopup_.SetActive(true);
            //StartCoroutine(LoadingTextAnimation());
        }

        public void HideLoading() {
            loadingPopup_.SetActive(false);
            //StopCoroutine(LoadingTextAnimation());
        }
        IEnumerator LoadingTextAnimation() {
            //loadingText.text = "Loading";
            while (true) {
                for (int i = 0; i < loadingImages.Count; i++) {
                    yield return new WaitForSecondsRealtime(30 * Time.deltaTime);
                    loading.sprite = loadingImages[i];
                }
            }
        }
        public void ShowPopup(string msg, MessageType messageType, PopupType popupType, ButtonProperties leftbuttonProperties = null, ButtonProperties rightbuttonProperties = null, ButtonProperties thirdbuttonProperties = null, Sprite customSprite = null) {
            popup_.SetActive(true);
            HideLoading();
            if (msg.Length > 255) {
                msg = msg.Substring(0, 255) + "...";
                LogError("Message too long truncating");
            }

            text_TextMeshProUGUI_.text = msg;
            switch (popupType) {
                case PopupType.OneButton:
                    ButtonLogic(leftButton_, "Okay", leftbuttonProperties);
                    rightButton_.gameObject.SetActive(false);
                    thirdButton_.gameObject.SetActive(false);
                    break;
                case PopupType.TwoButton:
                    ButtonLogic(leftButton_, "Okay", leftbuttonProperties);
                    ButtonLogic(rightButton_, "Okay", rightbuttonProperties);
                    thirdButton_.gameObject.SetActive(false);
                    break;
                case PopupType.ThreeButton:
                    ButtonLogic(leftButton_, "Okay", leftbuttonProperties);
                    ButtonLogic(rightButton_, "Okay", rightbuttonProperties);
                    ButtonLogic(thirdButton_, "Cancel", thirdbuttonProperties);
                    break;
                case PopupType.ToastMassage:
                    ToastMassageLogic(msg);
                    break;
            }

            switch (messageType) {
                case MessageType.Info:
                    typeMsgImage_.sprite = infoSprite_;
                    break;
                case MessageType.Warning:
                    typeMsgImage_.sprite = warningSprite_;
                    break;
                case MessageType.Error:
                    typeMsgImage_.sprite = errorSprite_;
                    break;
            }

            if (customSprite != null) {
                typeMsgImage_.sprite = customSprite;
            }

            StartCoroutine(SetSize());
        }

        IEnumerator SetSize() {
            popup_.transform.GetChild(0).GetComponent<ContentSizeFitter>().enabled = false;
            yield return new WaitForSecondsRealtime(0.01f);
            popup_.transform.GetChild(0).GetComponent<ContentSizeFitter>().enabled = true;
            if (popup_.GetComponent<RectTransform>().sizeDelta.y + padding_ > Screen.height) {
                text_TextMeshProUGUI_.GetComponent<LayoutElement>().preferredHeight = 700;
            } else {
                Log("Reset LayoutGroup", text_TextMeshProUGUI_.gameObject);
            }
        }

        private void ButtonLogic(Button button, string defaultName, ButtonProperties buttonCallback) {

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ClosePopup);

            if (buttonCallback == null) {
                button.GetComponentInChildren<TextMeshProUGUI>().text = defaultName;
            } else {
                button.GetComponentInChildren<TextMeshProUGUI>().text = buttonCallback.buttonName;
                if (buttonCallback.buttonAction_ != null) {
                    button.onClick.AddListener(buttonCallback.buttonAction_.Invoke);
                }
            }
            button.gameObject.SetActive(true);

        }

        public void ClosePopup() {
            popup_.SetActive(false);
        }

        private void ToastMassageLogic(string msg) {
            ShowToast(msg);
        }

        public void ShowToast(string text) {
            StartCoroutine(ShowToastCOR(text));
        }

        private IEnumerator ShowToastCOR(string text) {
            toastPanel_.SetActive(true);
            Color originalColor = toastMessage_.color;

            toastMessage_.text = text;
            toastMessage_.enabled = true;

            //Fade in
            yield return FadeInAndOut(toastMessage_, true, 0.5f);

            //Wait for the duration_
            float counter = 0;
            while (counter < duration_) {
                counter += Time.deltaTime;
                yield return null;
            }

            //Fade out
            yield return FadeInAndOut(toastMessage_, false, 0.5f);

            toastMessage_.enabled = false;
            toastMessage_.color = originalColor;
            toastPanel_.SetActive(false);
        }

        IEnumerator FadeInAndOut(TextMeshProUGUI targetText, bool fadeIn, float duration) {
            //Set Values depending on if fadeIn or fadeOut
            float a, b;
            if (fadeIn) {
                a = 0f;
                b = 1f;
            } else {
                a = 1f;
                b = 0f;
            }

            Color currentColor = Color.white;
            float counter = 0f;

            while (counter < duration) {
                counter += Time.deltaTime;
                float alpha = Mathf.Lerp(a, b, counter / duration);

                targetText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                yield return null;
            }
        }
    }
}