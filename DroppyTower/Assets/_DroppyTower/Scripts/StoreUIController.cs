using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using System.Collections.Generic;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace _DroppyTower
{
    public class StoreUIController : MonoBehaviour
    {
        public GameObject coinPackPrefab;
        public Transform productList;
        public string clickid;
        private StarkAdManager starkAdManager;

        // Use this for initialization
        void Start()
        {
            //var purchaser = InAppPurchaser.Instance;
            //for (int i = 0; i < purchaser.coinPacks.Length; i++)
            //{
            //    InAppPurchaser.CoinPack pack = purchaser.coinPacks[i];
            //    GameObject newPack = Instantiate(coinPackPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            //    Transform newPackTf = newPack.transform;
            //    newPackTf.Find("CoinValue").GetComponent<Text>().text = pack.coinValue.ToString();
            //    newPackTf.Find("Button/PriceString").GetComponent<Text>().text = pack.priceString;
            //    newPackTf.SetParent(productList, true);
            //    newPackTf.localScale = Vector3.one;

            //    // Add button listener
            //    newPackTf.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
            //        {
            //            Utilities.ButtonClickSound();

            //            #if EASY_MOBILE
            //            purchaser.Purchase(pack.productName);
            //            #endif
            //        });
            //}
        }
        public void AddCoins()
        {
            ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {

                    CoinManager.Instance.AddCoins(100);


                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
            
        }
        public void getClickid()
        {
            var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
            if (launchOpt.Query != null)
            {
                foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                    if (kv.Value != null)
                    {
                        Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                        if (kv.Key.ToString() == "clickid")
                        {
                            clickid = kv.Value.ToString();
                        }
                    }
                    else
                    {
                        Debug.Log(kv.Key + "<-参数-> " + "null ");
                    }
            }
        }

        public void apiSend(string eventname, string clickid)
        {
            TTRequest.InnerOptions options = new TTRequest.InnerOptions();
            options.Header["content-type"] = "application/json";
            options.Method = "POST";

            JsonData data1 = new JsonData();

            data1["event_type"] = eventname;
            data1["context"] = new JsonData();
            data1["context"]["ad"] = new JsonData();
            data1["context"]["ad"]["callback"] = clickid;

            Debug.Log("<-data1-> " + data1.ToJson());

            options.Data = data1.ToJson();

            TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
               response => { Debug.Log(response); },
               response => { Debug.Log(response); });
        }


        /// <summary>
        /// </summary>
        /// <param name="adId"></param>
        /// <param name="closeCallBack"></param>
        /// <param name="errorCallBack"></param>
        public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
        {
            starkAdManager = StarkSDK.API.GetStarkAdManager();
            if (starkAdManager != null)
            {
                starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
            }
        }
    }
}
