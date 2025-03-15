
#if KHFC_IAP
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;
using Unity.Services.Core;
using Unity.Services.Core.Environments;


#if KHFC_UNITASK
using AsyncVoid = Cysharp.Threading.Tasks.UniTaskVoid;
#else
using AsyncVoid = System.Threading.Tasks.Task;
#endif

namespace KHFC.IAP {
	public class IAPProduct {
		public string ID;
		public ProductType Type;
	}

	public delegate void IAPCallback(Product product);
	public delegate void IAPFailedCallback(string productID, PurchaseFailureReason reason, string message);

	public class InAppPurchaser : IDetailedStoreListener {
		const string ENV_NAME = "production";

		static IStoreController m_StoreController;		// The Unity Purchasing system
		static IExtensionProvider m_ExtensionProvider;	// The store-specific Purchasing subsystems

		public List<IAPProduct> m_ListProduct;
		public bool initialized => m_StoreController != null && m_ExtensionProvider != null;

		public IAPCallback m_OnPurchaseSucceed;
		public IAPFailedCallback m_OnPurchaseFailed;

		bool m_PurchaseProcess = false;

		/// <summary> 상품을 등록한다. Init 이전에 호출해서 리스트를 채워야 함 </summary>
		public void SetProduct(List<IAPProduct> list) {
			m_ListProduct = list;
		}

		public void Init() {
			if (initialized)
				return;

			InitializeProcess().Forget();
			//InitializeProcessSync();
		}

		/// <summary> 스토어에 등록된 가격 정보를 리턴 </summary>
		public string GetLocalePrice(string id) {
			if (!initialized)
				return "null";
			Product product = m_StoreController.products.WithID(id);
			return product.metadata.localizedPriceString;
		}

		async AsyncVoid InitializeProcess() {
			try {
				var options = new InitializationOptions().SetEnvironmentName(ENV_NAME);
				await UnityServices.InitializeAsync(options);
			} catch (Exception e) {
				// An error occurred during initialization.
				UnityEngine.Debug.LogError($"UnityServices Initialize Failed : {e}");
			}
			InitializePurchasing();
		}

		void InitializeProcessSync() {
			try {
				var options = new InitializationOptions().SetEnvironmentName(ENV_NAME);
				UnityServices.InitializeAsync(options).Wait();
			} catch (Exception e) {
				// An error occurred during initialization.
				UnityEngine.Debug.LogError($"UnityServices Initialize Failed : {e}");
			}
			InitializePurchasing();
		}

		public void InitializePurchasing() {
			if (initialized)
				return;

			ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
			foreach (IAPProduct shopData in m_ListProduct) {
				builder.AddProduct(shopData.ID, shopData.Type);
				UnityEngine.Debug.Log($"IAP AddProduct:{shopData.ID}, {shopData.Type}");
			}

			UnityPurchasing.Initialize(this, builder);
		}

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
			UnityEngine.Debug.Log("IAP OnInitialized: Success");

			m_StoreController = controller;
			m_ExtensionProvider = extensions;
		}

		public void OnInitializeFailed(InitializationFailureReason error) {
			UnityEngine.Debug.Log($"IAP OnInitializeFailed: {error}");
		}
		public void OnInitializeFailed(InitializationFailureReason error, string message) {
			UnityEngine.Debug.Log($"IAP OnInitializeFailed: {error}\nmessage: {message}");
		}


		public bool BuyProduct(string productID) {
			if (m_PurchaseProcess)
				return false;

#if UNITY_EDITOR
			m_OnPurchaseSucceed?.Invoke(null);
			return true;
#endif
			if (!initialized) {
				UnityEngine.Debug.Log($"BuyProductID {productID}  FAIL. Not initialized.");
				m_PurchaseProcess = false;
				return false;
			}

			Product product = m_StoreController.products.WithID(productID);
			m_PurchaseProcess = true;

			if (product != null && product.availableToPurchase) {
				UnityEngine.Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				m_StoreController.InitiatePurchase(product);
			} else {
				UnityEngine.Debug.Log($"BuyProductID {productID} : FAIL. Not purchasing product, either is not found or is not available for purchase");
				m_PurchaseProcess = false;
			}

			return m_PurchaseProcess;
		}

		/// <summary> 구매 성공 이후 자동으로 호출 </summary>
		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {
			bool validPurchase = true;
#if UNITY_ANDROID
			UnityEngine.Debug.Log("IAP receipt: " + args.purchasedProduct.receipt);
			//if (isAllowedFormat(args.purchasedProduct.transactionID) == false) {
			//	//영수증 포맷이 일치하지 않음
			//	Debug.Log("TransactionID is Not Matched.");
			//	return PurchaseProcessingResult.Complete;
			//}

			CrossPlatformValidator validator
				= new(GooglePlayTangle.Data(), AppleTangle.Data(), UnityEngine.Application.identifier);
			try {
				// On Google Play, result has a single product ID.
				// On Apple stores, receipts contain multiple products.
				var result = validator.Validate(args.purchasedProduct.receipt);
				// For informational purposes, we list the receipt(s)
				UnityEngine.Debug.Log("Receipt is valid. Contents:");
				foreach (IPurchaseReceipt productReceipt in result) {
					UnityEngine.Debug.Log(productReceipt.productID);
					UnityEngine.Debug.Log(productReceipt.purchaseDate);
					UnityEngine.Debug.Log(productReceipt.transactionID);
				}
			} catch (IAPSecurityException e) {
				UnityEngine.Debug.Log("Invalid receipt, not unlocking content : " + e);
				validPurchase = false;
			}
#endif

			Product product = args.purchasedProduct;
			string productId = product.definition.id;
			if (!validPurchase) {
				m_OnPurchaseFailed?.Invoke(productId, PurchaseFailureReason.Unknown, "Invalid Receipt");
				return PurchaseProcessingResult.Pending;
			}

			m_OnPurchaseSucceed?.Invoke(product);
			m_PurchaseProcess = false;

			return PurchaseProcessingResult.Complete;
		}

		public string GetGooglePurchaseToken(Product purchasedProduct) {
#if UNITY_ANDROID
			if (!purchasedProduct.hasReceipt)
				return string.Empty;

			CrossPlatformValidator validator
				= new(GooglePlayTangle.Data(), AppleTangle.Data(), UnityEngine.Application.identifier);
			try {
				IPurchaseReceipt[] result = validator.Validate(purchasedProduct.receipt);
				foreach (IPurchaseReceipt productReceipt in result) {
					if (productReceipt is GooglePlayReceipt google)
						return google.purchaseToken;
				}
			} catch (IAPSecurityException) {
				UnityEngine.Debug.Log("Invalid receipt, not unlocking content");
			}
#endif
			return string.Empty;
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureDescription desc) {
			m_PurchaseProcess = false;
			UnityEngine.Debug.Log(string.Format("IAP OnPurchaseFailed: '{0}'\nreason: {1}\nmessage: {2}", product.definition.storeSpecificId, desc.reason, desc.message));
			m_OnPurchaseFailed?.Invoke(product.definition.id, desc.reason, desc.message);
		}

		public void OnPurchaseFailed(Product product, PurchaseFailureReason reason) {
			m_PurchaseProcess = false;
			UnityEngine.Debug.Log(string.Format("IAP OnPurchaseFailed: '{0}'\nreason: {1}", product.definition.storeSpecificId, reason));
			m_OnPurchaseFailed?.Invoke(product.definition.id, reason, string.Empty);
		}

		/// <summary> 구매 복구, IOS에서만 지원하는 기능 </summary>
		public void RestorePurchases() {
#if UNITY_IOS
			// If Purchasing has not yet been set up ...
			if (!initialized) {
				// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
				UnityEngine.Debug.Log("RestorePurchases FAIL. Not initialized.");
				return;
			}

			// If we are running on an Apple device ...
			if (UnityEngine.Application.platform != UnityEngine.RuntimePlatform.IPhonePlayer &&
				UnityEngine.Application.platform != UnityEngine.RuntimePlatform.OSXPlayer) {
				// We are not running on an Apple device. No work is necessary to restore purchases.
				UnityEngine.Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
				return;
			}

			// ... begin restoring purchases
			UnityEngine.Debug.Log("RestorePurchases started ...");

			// Fetch the Apple store-specific subsystem.
			IAppleExtensions apple = m_ExtensionProvider.GetExtension<IAppleExtensions>();
			// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
			// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
			apple.RestoreTransactions((result, code) => {
				// The first phase of restoration. If no more responses are received on ProcessPurchase then 
				// no purchases are available to be restored.
				UnityEngine.Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
			});
#endif
		}
	}
}
#endif