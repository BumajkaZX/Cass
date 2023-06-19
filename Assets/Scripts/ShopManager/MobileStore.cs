namespace Cass.StoreManager
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using System.Threading.Tasks;
    using System;
    using System.Threading;
    using Unity.Services.Core;
    using UnityEngine.Purchasing.Extension;
    using Cass.Items;
    using UnityEngine.Purchasing.Security;

    public class MobileStore : IDetailedStoreListener, IStore
    {
        private IStoreController _storeController = default;

        private IExtensionProvider _extensionProvider = default;

        public async Task Init(CancellationToken token)
        {
            InitializationOptions options = new InitializationOptions()

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                .SetOption("Test", true);
#else
                .SetOption("Test", false);
#endif

            await UnityServices.InitializeAsync(options);

            ResourceRequest operation = Resources.LoadAsync<TextAsset>("IAPProductCatalog");

            while (!operation.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await Task.Yield();
            }

            OnComplete(operation);
        }

        private void OnComplete(AsyncOperation operation)
        {
            ResourceRequest request = operation as ResourceRequest;

#if UNITY_EDITOR

            Debug.Log($"Loaded Asset: {request.asset}");

#endif
            ProductCatalog catalog = JsonUtility.FromJson<ProductCatalog>((request.asset as TextAsset).text);

#if UNITY_EDITOR

            StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.DeveloperUser;

            StandardPurchasingModule.Instance().useFakeStoreAlways = true;

            Debug.Log($"Loaded catalog with {catalog.allProducts.Count} items");

#endif
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.GooglePlay));

            foreach(ProductCatalogItem item in catalog.allProducts)
            {
                builder.AddProduct(item.id, item.type);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;

            foreach (Product product in _storeController.products.all)
            {
                if (product.hasReceipt)
                {
                    ItemsContainer.Instance.Items.Find(_ => _.ItemId == product.definition.id).IsBought = true;
                }
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {

#if UNITY_EDITOR

            Debug.LogError($"Error initializing IAP because of {error}");

#endif

        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {

#if UNITY_EDITOR

            Debug.LogError($"Error initializing IAP because of {error} with message {message}");

#endif

        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            throw new System.NotImplementedException();
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
          

#if UNITY_EDITOR

            Debug.Log($"Successfully purchased {purchaseEvent.purchasedProduct.definition.id}");
            ItemsContainer.Instance.Items.Find(_ => _.ItemId == purchaseEvent.purchasedProduct.definition.id).IsBought = true;

#elif RECEIPT_VALIDATION
            bool isValidPurchase = true;

            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);

            try
            {
                var result = validator.Validate(purchaseEvent.purchasedProduct.receipt);

                Debug.Log("Receipt is valid. Contents:");

                foreach (IPurchaseReceipt productReceipt in result)
                {
                    Debug.Log(productReceipt.productID);
                    Debug.Log(productReceipt.purchaseDate);
                    Debug.Log(productReceipt.transactionID);
                }
            }
            catch(IAPSecurityException ex)
            {
                Debug.LogError("Invalid receipt " + ex);
                isValidPurchase = false;
            }

            if (isValidPurchase)
            {
                ItemsContainer.Instance.Items.Find(_ => _.ItemId == purchaseEvent.purchasedProduct.definition.id).IsBought = true;
            }
#endif

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
        }

        public void Purchase(PlayerItem item) => _storeController.InitiatePurchase(_storeController.products.WithID(item.ItemId));
    }
}
