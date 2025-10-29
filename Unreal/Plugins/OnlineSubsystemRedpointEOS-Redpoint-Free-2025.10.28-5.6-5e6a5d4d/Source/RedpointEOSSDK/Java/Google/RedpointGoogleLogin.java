package games.redpoint;

import android.app.Activity;
import android.content.Intent;
import android.content.IntentSender;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.android.billingclient.api.AccountIdentifiers;
import com.android.billingclient.api.AcknowledgePurchaseParams;
import com.android.billingclient.api.AcknowledgePurchaseResponseListener;
import com.android.billingclient.api.BillingClient;
import com.android.billingclient.api.BillingClientStateListener;
import com.android.billingclient.api.BillingFlowParams;
import com.android.billingclient.api.BillingResult;
import com.android.billingclient.api.ConsumeParams;
import com.android.billingclient.api.ConsumeResponseListener;
import com.android.billingclient.api.PendingPurchasesParams;
import com.android.billingclient.api.ProductDetails;
import com.android.billingclient.api.ProductDetailsResponseListener;
import com.android.billingclient.api.Purchase;
import com.android.billingclient.api.PurchasesResponseListener;
import com.android.billingclient.api.PurchasesUpdatedListener;
import com.android.billingclient.api.QueryProductDetailsParams;
import com.android.billingclient.api.QueryProductDetailsResult;
import com.android.billingclient.api.QueryPurchasesParams;
import com.google.android.gms.auth.api.identity.BeginSignInRequest;
import com.google.android.gms.auth.api.identity.BeginSignInResult;
import com.google.android.gms.auth.api.identity.Identity;
import com.google.android.gms.auth.api.identity.SignInClient;
import com.google.android.gms.auth.api.identity.SignInCredential;
import com.google.android.gms.common.api.ApiException;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;

import java.util.ArrayList;
import java.util.Base64;
import java.util.Base64.Decoder;
import java.util.HashMap;
import java.util.List;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

public class RedpointGoogleLogin implements PurchasesUpdatedListener, BillingClientStateListener {
	private Activity activity;
	private SignInClient oneTapClient;
	private BeginSignInRequest signInRequest;
	private BillingClient billingClient;
	private boolean billingConnectionInProgress;
	private boolean billingClientDisconnected;
	private List<BillingRequestOperation> delayedBillingOperations;
	private static final int REQ_ONE_TAP = 2;
	private int pendingPurchaseDispatchId;
	private int pendingQueryDispatchId;
	private String pendingPurchaseGuid;
	private OperationMode pendingOperation;

	private enum OperationMode {
		NO_OPERATION,
		PURCHASING,
		QUERYING_RECEIPTS,
	}

	public RedpointGoogleLogin(Activity activity, String inPackageName, String BuildConfiguration) {
		this.activity = activity;
		this.billingConnectionInProgress = false;
		this.billingClientDisconnected = true;
		this.delayedBillingOperations = new ArrayList<BillingRequestOperation>();
		this.pendingPurchaseDispatchId = -1;
		this.pendingQueryDispatchId = -1;
		this.pendingPurchaseGuid = null;
		this.pendingOperation = OperationMode.NO_OPERATION;
	}

	private void printDebug(String message) {
		Log.d("UE", "[RedpointGoogle] " + message);
	}

	private void printError(String message) {
		Log.e("UE", "[RedpointGoogle] " + message);
	}

	private void loginWithFilter(final String inServerClientId, final boolean filterByAuthorizedAccounts) {
		this.oneTapClient = Identity.getSignInClient(activity);
		this.signInRequest =
			BeginSignInRequest.builder()
				.setGoogleIdTokenRequestOptions(BeginSignInRequest.GoogleIdTokenRequestOptions.builder()
					.setSupported(true)
					.setServerClientId(inServerClientId)
					.setFilterByAuthorizedAccounts(filterByAuthorizedAccounts)
					.build())
				.setAutoSelectEnabled(true)
				.build();

		this.oneTapClient.beginSignIn(this.signInRequest)
			.addOnSuccessListener(
				this.activity,
				new OnSuccessListener<BeginSignInResult>() {
					@Override
					public void onSuccess(BeginSignInResult result) {
						try {
							activity.startIntentSenderForResult(
								result.getPendingIntent().getIntentSender(),
								REQ_ONE_TAP,
								null,
								0,
								0,
								0);
						} catch (IntentSender.SendIntentException e) {
							printError(
								"Received SendIntentException exception while calling startIntentSenderForResult: " +
									e.toString());
							e.printStackTrace();

							nativeLoginComplete(false, e.getLocalizedMessage(), "", "", "", "");
						}
					}
				})
			.addOnFailureListener(this.activity, new OnFailureListener() {
				@Override
				public void onFailure(@NonNull Exception e) {
					printDebug("Received onFailure callback while calling oneTapClient beginSignIn with filterByAuthorizedAccounts = " + filterByAuthorizedAccounts + ": " + e.toString());
					e.printStackTrace();
					if (filterByAuthorizedAccounts) {
						// Fallback to sign up flow.
						loginWithFilter(inServerClientId, false);
					} else {
						// Actual login failure.
						nativeLoginComplete(false, e.getLocalizedMessage(), "", "", "", "");
					}
				}
			});
	}

	public void login(String[] inClientIds, String inServerClientId) {
		printDebug("Using server client ID for authentication: " + inServerClientId);
		this.loginWithFilter(inServerClientId, true);
	}

	public native void nativeLoginComplete(
		boolean wasSuccessful,
		String errorMessage,
		String idToken,
		String userId,
		String userEmail,
		String displayName);

	public void onActivityResult(int requestCode, int resultCode, @Nullable Intent data) {
		switch (requestCode) {
			case REQ_ONE_TAP:
				try {
					SignInCredential credential = oneTapClient.getSignInCredentialFromIntent(data);
					final String idToken = credential.getGoogleIdToken();
					final String userEmail = credential.getId();
					final String displayName = credential.getDisplayName();

					// We have to parse the ID token to get the subject out.
					String[] chunks = idToken.split("\\.");
					Decoder decoder = Base64.getUrlDecoder();
					String payload = new String(decoder.decode(chunks[1]));
					JSONObject json = new JSONObject(payload);
					final String userId = json.getString("sub");

					printDebug("Received ID token from authentication: " + idToken);

					// Initialize the billing client at this point, and wait for it to
					// successfully connect to Google Play.
					this.billingClient =
						BillingClient.newBuilder(this.activity)
							.setListener(this)
							.enablePendingPurchases(PendingPurchasesParams.newBuilder().enableOneTimeProducts().build())
							.enableAutoServiceReconnection()
							.build();
					
					this.makeBillingRequest(new BillingRequestOperation() {
						@Override
						public void performOperation(boolean didConnect, String connectionError) {
							if (!didConnect) {
								printDebug("Google Play Billing connection failed with response: " + connectionError);
								RedpointGoogleLogin.this.nativeLoginComplete(
									false,
									"Failed to connect to Google Play Billing: " + connectionError,
									"",
									"",
									"",
									"");
								return;
							}

							RedpointGoogleLogin.this.nativeLoginComplete(true, "", idToken, userId, userEmail, displayName);
						}
					});
				} catch (JSONException e) {
					printError("Received JSON exception while handling onActivityResult: " + e.toString());
					e.printStackTrace();

					this.nativeLoginComplete(false, e.getLocalizedMessage(), "", "", "", "");
				} catch (ApiException e) {
					printError("Received API exception while handling onActivityResult: " + e.toString());
					e.printStackTrace();

					this.nativeLoginComplete(false, e.getLocalizedMessage(), "", "", "", "");
				}
				break;
		}
	}

	public native void nativeLogoutComplete(
		boolean wasSuccessful,
		String errorMessage);

	public void logout() {
        printDebug("logout requested");
		if (this.oneTapClient != null) {
            printDebug("logout starting on One Tap client");
			this.oneTapClient.signOut()
				.addOnSuccessListener(
					this.activity,
					new OnSuccessListener<Void>() {
						@Override
						public void onSuccess(Void unused) {
                            printDebug("logout successful, saved credentials have been cleared");
							nativeLogoutComplete(true, "");
						}
					}
				)
				.addOnFailureListener(
					this.activity, new OnFailureListener() {
						@Override
						public void onFailure(@NonNull Exception e) {
                            printDebug("logout failed: " + e.getLocalizedMessage());
							nativeLogoutComplete(false, "Failed to sign out: " + e.getLocalizedMessage());
						}
					}
				);
		}
	}

	public void onDestroy() {
	}

	public void onStart() {
	}

	public void onResume() {
		if (this.billingClient != null) {
			this.makeBillingRequest(new BillingRequestOperation() {
				@Override
				public void performOperation(boolean didConnect, String connectionError) {
					if (!didConnect) {
						printDebug(
							"Failed to queryPurchasesAsync during onResume due to connection failure: " +
								connectionError);
						return;
					}

					// Query for the latest purchases status, as per Google Play Billing requirements.
					RedpointGoogleLogin.this.billingClient.queryPurchasesAsync(
						QueryPurchasesParams.newBuilder().setProductType(BillingClient.ProductType.INAPP).build(),
						new PurchasesResponseListener() {
							@Override
							public void onQueryPurchasesResponse(
								@NonNull BillingResult billingResult,
								@NonNull List<Purchase> purchases) {
								RedpointGoogleLogin.this.onPurchasesUpdated(billingResult, purchases);
							}
						});
				}
			});
		}
	}

	public void onStop() {
	}

	private String responseCodeToString(int responseCode) {
		switch (responseCode) {
			case BillingClient.BillingResponseCode.SERVICE_TIMEOUT:
				return "SERVICE_TIMEOUT";
			case BillingClient.BillingResponseCode.FEATURE_NOT_SUPPORTED:
				return "FEATURE_NOT_SUPPORTED";
			case BillingClient.BillingResponseCode.SERVICE_DISCONNECTED:
				return "SERVICE_DISCONNECTED";
			case BillingClient.BillingResponseCode.OK:
				return "OK";
			case BillingClient.BillingResponseCode.USER_CANCELED:
				return "USER_CANCELED";
			case BillingClient.BillingResponseCode.SERVICE_UNAVAILABLE:
				return "SERVICE_UNAVAILABLE";
			case BillingClient.BillingResponseCode.BILLING_UNAVAILABLE:
				return "BILLING_UNAVAILABLE";
			case BillingClient.BillingResponseCode.ITEM_UNAVAILABLE:
				return "ITEM_UNAVAILABLE";
			case BillingClient.BillingResponseCode.DEVELOPER_ERROR:
				return "DEVELOPER_ERROR";
			case BillingClient.BillingResponseCode.ERROR:
				return "ERROR";
			case BillingClient.BillingResponseCode.ITEM_ALREADY_OWNED:
				return "ITEM_ALREADY_OWNED";
			case BillingClient.BillingResponseCode.ITEM_NOT_OWNED:
				return "ITEM_NOT_OWNED";
		}
		return "UNKNOWN";
	}

	private String getMessageForBillingResult(BillingResult billingResult) {
		StringBuilder sb = new StringBuilder();
		sb.append("(billing code) ");
		sb.append(responseCodeToString(billingResult.getResponseCode()));
		sb.append(" (billing debug message) ");
		String debugMessage = billingResult.getDebugMessage();
		if (debugMessage == null || debugMessage.equals("")) {
			sb.append("<empty>");
		} else {
			sb.append("'");
			sb.append(debugMessage);
			sb.append("'");
		}
		return sb.toString();
	}

	public native void nativeQueryOffersByIdComplete(
		int dispatchId,
		boolean wasSuccessful,
		String errorMessage,
		String jsonData);

	public native void nativeInitiatePurchaseComplete(
		int dispatchId,
		boolean wasSuccessful,
		boolean wasCancelled,
		String errorMessage,
		String jsonData);

	public native void nativeNotifyOutOfBandPurchases(
		String jsonData,
		boolean wasSuccessfulReceipts,
		String errorMessage);

	public native void nativeAcknowledgePurchaseComplete(int dispatchId, boolean wasSuccessful, String errorMessage);

	public native void nativeConsumePurchaseComplete(int dispatchId, boolean wasSuccessful, String errorMessage);

	public native void nativeQueryReceiptsComplete(int dispatchId, boolean wasSuccessful, String errorMessage);

	private void finalizeInitiatePurchaseComplete(
		int dispatchId,
		boolean wasSuccessful,
		boolean wasCancelled,
		String errorMessage,
		String jsonData) {
		if (this.pendingPurchaseDispatchId == dispatchId) {
			this.pendingPurchaseDispatchId = -1;
		}
		if (this.pendingOperation == OperationMode.PURCHASING) {
			this.pendingOperation = OperationMode.NO_OPERATION;
		}
		nativeInitiatePurchaseComplete(dispatchId, wasSuccessful, wasCancelled, errorMessage, jsonData);
	}

	private void finalizeQueryReceiptsComplete(int dispatchId, boolean wasSuccessful, String errorMessage) {
		if (this.pendingQueryDispatchId == dispatchId) {
			this.pendingQueryDispatchId = -1;
		}
		if (this.pendingOperation == OperationMode.QUERYING_RECEIPTS) {
			this.pendingOperation = OperationMode.NO_OPERATION;
		}
		nativeQueryReceiptsComplete(dispatchId, wasSuccessful, errorMessage);
	}

	private int getAndClearPendingDispatchId(OperationMode operationMode) {
		if (operationMode == OperationMode.PURCHASING && this.pendingOperation == OperationMode.PURCHASING) {
			int inDispatchId = this.pendingPurchaseDispatchId;
			this.pendingPurchaseDispatchId = -1;
			printDebug("Obtained dispatch ID " + inDispatchId + " (purchase) and cleared.");
			return inDispatchId;
		} else if (
			operationMode == OperationMode.QUERYING_RECEIPTS &&
				this.pendingOperation == OperationMode.QUERYING_RECEIPTS) {
			int inDispatchId = this.pendingQueryDispatchId;
			this.pendingQueryDispatchId = -1;
			printDebug("Obtained dispatch ID " + inDispatchId + " (query) and cleared.");
			return inDispatchId;
		}
		throw new RuntimeException("Invalid call to getAndClearPendingDispatchId!");
	}

	public void queryOffersById(final int inDispatchId, final String[] inOfferIds) {
		this.makeBillingRequest(new BillingRequestOperation() {
			@Override
			public void performOperation(boolean didConnect, String connectionError) {
				if (!didConnect) {
					printDebug(
						"Calling back into nativeQueryOffersByIdComplete due to connection failure: " +
							connectionError);
					RedpointGoogleLogin.this.nativeQueryOffersByIdComplete(
						inDispatchId,
						false,
						"Failed to re-connect to Google Play Billing: " + connectionError,
						"");
					return;
				}

				List<QueryProductDetailsParams.Product> productIds = new ArrayList<QueryProductDetailsParams.Product>();
				for (String offerId : inOfferIds) {
					productIds.add(QueryProductDetailsParams.Product.newBuilder()
						.setProductId(offerId)
						.setProductType(BillingClient.ProductType.INAPP)
						.build());
				}

				QueryProductDetailsParams query =
					QueryProductDetailsParams.newBuilder().setProductList(productIds).build();

				RedpointGoogleLogin.this.billingClient.queryProductDetailsAsync(
					query,
					new ProductDetailsResponseListener() {
						@Override
						public void onProductDetailsResponse(@NonNull BillingResult billingResult, @NonNull QueryProductDetailsResult queryProductDetailsResult) {
							List<ProductDetails> list = queryProductDetailsResult.getProductDetailsList();
							
							if (billingResult.getResponseCode() != BillingClient.BillingResponseCode.OK) {
								printDebug(
									"Calling back into nativeQueryOffersByIdComplete due to billing failure: " +
										RedpointGoogleLogin.this.getMessageForBillingResult(billingResult));
								RedpointGoogleLogin.this.nativeQueryOffersByIdComplete(
									inDispatchId,
									false,
									"Failed to query products from Google Play: " +
										RedpointGoogleLogin.this.getMessageForBillingResult(billingResult),
									"");
								return;
							}

							try {
								JSONArray results = new JSONArray();

								for (ProductDetails product : list) {
									ProductDetails.OneTimePurchaseOfferDetails priceDetails =
										product.getOneTimePurchaseOfferDetails();

									JSONObject offerObject = new JSONObject();
									offerObject.put("offerId", product.getProductId());
									offerObject.put("title", product.getTitle());
									offerObject.put("description", product.getDescription());
									offerObject.put("longDescription", "");
									offerObject.put("regularPriceText", "");
									offerObject.put("regularPrice", "-1");
									offerObject.put("priceText", priceDetails.getFormattedPrice());
									offerObject.put("price", Long.toString(priceDetails.getPriceAmountMicros()));
									offerObject.put("currencyCode", priceDetails.getPriceCurrencyCode());
									offerObject.put("releaseDateUnixTimestamp", "");
									offerObject.put("expirationDateUnixTimestamp", "");
									offerObject.put("offerDiscountType", "NotOnSale");
									results.put(offerObject);
								}

								printDebug("Calling back into nativeQueryOffersByIdComplete due to success");
								RedpointGoogleLogin.this
									.nativeQueryOffersByIdComplete(inDispatchId, true, "", results.toString());
							} catch (JSONException e) {
								printError("Calling back into nativeQueryOffersByIdComplete due to exception");
								e.printStackTrace();
								RedpointGoogleLogin.this
									.nativeQueryOffersByIdComplete(inDispatchId, false, e.toString(), "");
							}
						}
					});
			}
		});
	}

	private String getPurchaseStateString(int purchaseCode) {
		switch (purchaseCode) {
			case Purchase.PurchaseState.PENDING:
				return "PENDING";
			case Purchase.PurchaseState.PURCHASED:
				return "PURCHASED";
			case Purchase.PurchaseState.UNSPECIFIED_STATE:
				return "UNSPECIFIED_STATE";
		}
		return "UNKNOWN";
	}

	public void initiatePurchase(
		final int inDispatchId,
		final String purchaseGuid,
		final String[] inOfferIds,
		final int[] inQuantity) {
		if (this.pendingOperation != OperationMode.NO_OPERATION) {
			String reason;
			switch (this.pendingOperation) {
				case PURCHASING:
					reason = "Another purchase is already in-progress, so another purchase can not be started.";
					break;
				case QUERYING_RECEIPTS:
					reason = "A QueryReceipts operation is in-progress, so a purchase can not be started.";
					break;
				default:
					reason = "An unknown operation is in-progress, so a purchase can not be started.";
					break;
			}

			printDebug(reason);
			this.nativeInitiatePurchaseComplete(inDispatchId, false, false, reason, "");
			return;
		}

		this.pendingOperation = OperationMode.PURCHASING;
		try {
			this.makeBillingRequest(new BillingRequestOperation() {
				@Override
				public void performOperation(boolean didConnect, String connectionError) {
					if (!didConnect) {
						printDebug(
							"Calling back into nativeInitiatePurchaseComplete due to connection failure: " +
								connectionError);
						RedpointGoogleLogin.this.finalizeInitiatePurchaseComplete(
							inDispatchId,
							false,
							false,
							"Failed to re-connect to Google Play Billing: " + connectionError,
							"");
						return;
					}

					List<QueryProductDetailsParams.Product> productIds =
						new ArrayList<QueryProductDetailsParams.Product>();
					for (String offerId : inOfferIds) {
						productIds.add(QueryProductDetailsParams.Product.newBuilder()
							.setProductId(offerId)
							.setProductType(BillingClient.ProductType.INAPP)
							.build());
					}

					if (productIds.size() == 0) {
						printDebug(
							"Calling back into nativeInitiatePurchaseComplete because there are no products to purchase: " +
								connectionError);
						RedpointGoogleLogin.this.finalizeInitiatePurchaseComplete(
							inDispatchId,
							false,
							false,
							"There were no products to purchase in the checkout operation.",
							"");
						return;
					}

					QueryProductDetailsParams query =
						QueryProductDetailsParams.newBuilder().setProductList(productIds).build();

					RedpointGoogleLogin.this.billingClient.queryProductDetailsAsync(
						query,
						new ProductDetailsResponseListener() {
							@Override
							public void onProductDetailsResponse(@NonNull BillingResult billingResult, @NonNull QueryProductDetailsResult queryProductDetailsResult) {
								List<ProductDetails> list = queryProductDetailsResult.getProductDetailsList();

								if (billingResult.getResponseCode() != BillingClient.BillingResponseCode.OK) {
									printDebug(
										"Calling back into nativeInitiatePurchaseComplete due to billing failure: " +
											RedpointGoogleLogin.this.getMessageForBillingResult(billingResult));
									RedpointGoogleLogin.this.finalizeInitiatePurchaseComplete(
										inDispatchId,
										false,
										false,
										"Failed to query products from Google Play: " +
											RedpointGoogleLogin.this.getMessageForBillingResult(billingResult),
										"");
									return;
								}

								HashMap<String, ProductDetails> productLookup = new HashMap<String, ProductDetails>();
								for (ProductDetails product : list) {
									productLookup.put(product.getProductId(), product);
								}

								List<BillingFlowParams.ProductDetailsParams> productDetailsParamList =
									new ArrayList<BillingFlowParams.ProductDetailsParams>();
								for (int i = 0; i < inOfferIds.length; i++) {
									ProductDetails productDetails = productLookup.get(inOfferIds[i]);
									for (int q = 0; q < inQuantity[i]; q++) {
										productDetailsParamList.add(BillingFlowParams.ProductDetailsParams.newBuilder()
											.setProductDetails(productDetails)
											.build());
									}
								}

								// @note: We use the obfuscated account ID to track whether a purchase is for a
								// specific invocation of the initiatePurchase callback.
								BillingFlowParams billingFlowParams =
									BillingFlowParams.newBuilder()
										.setProductDetailsParamsList(productDetailsParamList)
										.setObfuscatedAccountId(purchaseGuid)
										.build();

								// Causes purchase results to route to the correct delegate on the C++ side.
								RedpointGoogleLogin.this.pendingPurchaseDispatchId = inDispatchId;
								RedpointGoogleLogin.this.pendingPurchaseGuid = purchaseGuid;

								BillingResult initiateBillingResult =
									billingClient.launchBillingFlow(activity, billingFlowParams);
								if (initiateBillingResult.getResponseCode() != BillingClient.BillingResponseCode.OK) {
									printDebug(
										"Calling back into nativeInitiatePurchaseComplete due to billing failure: " +
											RedpointGoogleLogin.this.getMessageForBillingResult(initiateBillingResult));
									RedpointGoogleLogin.this.finalizeInitiatePurchaseComplete(
										inDispatchId,
										false,
										false,
										"Failed to start purchase on Google Play: " +
											RedpointGoogleLogin.this.getMessageForBillingResult(initiateBillingResult),
										"");
								}
							}
						});
				}
			});
		} catch (Exception e) {
			this.pendingOperation = OperationMode.NO_OPERATION;
			printError("Unexpected exception during initiatePurchase: " + e.toString());
			e.printStackTrace();
			throw e;
		}
	}

	public void queryReceipts(final int inDispatchId) {
		if (this.pendingOperation != OperationMode.NO_OPERATION) {
			String reason;
			switch (this.pendingOperation) {
				case PURCHASING:
					reason = "A purchase is in-progress, so receipts can not be queried at this time.";
					break;
				case QUERYING_RECEIPTS:
					reason = "Another QueryReceipts operation is in-progress, so receipts can not be queried at this time.";
					break;
				default:
					reason = "An unknown operation is in-progress, so receipts can not be queried at this time.";
					break;
			}

			printDebug(reason);
			this.nativeQueryReceiptsComplete(inDispatchId, false, reason);
			return;
		}

		this.pendingOperation = OperationMode.QUERYING_RECEIPTS;
		try {
			this.makeBillingRequest(new BillingRequestOperation() {
				@Override
				public void performOperation(boolean didConnect, String connectionError) {
					if (!didConnect) {
						printDebug(
							"Calling back into nativeQueryReceiptsComplete due to connection failure: " +
								connectionError);
						RedpointGoogleLogin.this.finalizeQueryReceiptsComplete(
							inDispatchId,
							false,
							"Failed to re-connect to Google Play Billing: " + connectionError);
						return;
					}

					// Causes query results to route to the correct delegate on the C++ side.
					RedpointGoogleLogin.this.pendingQueryDispatchId = inDispatchId;

					RedpointGoogleLogin.this.billingClient.queryPurchasesAsync(
						QueryPurchasesParams.newBuilder().setProductType(BillingClient.ProductType.INAPP).build(),
						new PurchasesResponseListener() {
							@Override
							public void onQueryPurchasesResponse(
								@NonNull BillingResult billingResult,
								@NonNull List<Purchase> purchases) {
								RedpointGoogleLogin.this.onPurchasesUpdated(billingResult, purchases);
							}
						});
				}
			});
		} catch (Exception e) {
			this.pendingOperation = OperationMode.NO_OPERATION;
			printError("Unexpected exception during queryReceipts: " + e.toString());
			e.printStackTrace();
			throw e;
		}
	}
	
	public void acknowledgePurchase(final int inDispatchId, final String purchaseToken) {
		this.makeBillingRequest(new BillingRequestOperation() {
			@Override
			public void performOperation(boolean didConnect, String connectionError) {
				if (!didConnect) {
					printDebug(
						"Calling back into nativeAcknowledgePurchaseComplete due to connection failure: " +
							connectionError);
					RedpointGoogleLogin.this.nativeAcknowledgePurchaseComplete(
						inDispatchId,
						false,
						"Failed to re-connect to Google Play Billing: " + connectionError);
					return;
				}

				RedpointGoogleLogin.this.billingClient.acknowledgePurchase(
					AcknowledgePurchaseParams.newBuilder().setPurchaseToken(purchaseToken).build(),
					new AcknowledgePurchaseResponseListener() {
						@Override
						public void onAcknowledgePurchaseResponse(@NonNull BillingResult billingResult) {
							printDebug("Calling back into nativeQueryReceiptsComplete due to acknowledge result");
							if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
								RedpointGoogleLogin.this.nativeAcknowledgePurchaseComplete(inDispatchId, true, "");
							} else {
								RedpointGoogleLogin.this.nativeAcknowledgePurchaseComplete(
									inDispatchId,
									false,
									"Failed to acknowledge purchase on Google Play: " +
										RedpointGoogleLogin.this.getMessageForBillingResult(billingResult));
							}
							return;
						}
					});
			}
		});
	}

	public void consumePurchase(final int inDispatchId, final String purchaseToken) {
		this.makeBillingRequest(new BillingRequestOperation() {
			@Override
			public void performOperation(boolean didConnect, String connectionError) {
				if (!didConnect) {
					printDebug(
						"Calling back into nativeConsumePurchaseComplete due to connection failure: " +
							connectionError);
					RedpointGoogleLogin.this.nativeConsumePurchaseComplete(
						inDispatchId,
						false,
						"Failed to re-connect to Google Play Billing: " + connectionError);
					return;
				}

				RedpointGoogleLogin.this.billingClient.consumeAsync(
					ConsumeParams.newBuilder().setPurchaseToken(purchaseToken).build(),
					new ConsumeResponseListener() {
						@Override
						public void onConsumeResponse(@NonNull BillingResult billingResult, @NonNull String s) {
							printDebug("Calling back into nativeQueryReceiptsComplete due to consumption result");
							if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
								RedpointGoogleLogin.this.nativeConsumePurchaseComplete(inDispatchId, true, "");
							} else {
								RedpointGoogleLogin.this.nativeConsumePurchaseComplete(
									inDispatchId,
									false,
									"Failed to consume purchase on Google Play: " +
										RedpointGoogleLogin.this.getMessageForBillingResult(billingResult));
							}
							return;
						}
					});
			}
		});
	}

	@Override
	public void onPurchasesUpdated(@NonNull BillingResult purchaseBillingResult, @Nullable List<Purchase> purchaseList) {
		if (purchaseBillingResult.getResponseCode() == BillingClient.BillingResponseCode.OK && purchaseList != null) {
			try {
				JSONObject purchaseForPendingOperation = null;

				JSONArray results = new JSONArray();

				for (Purchase purchase : purchaseList) {
					JSONArray productsArray = new JSONArray();
					for (String productId : purchase.getProducts()) {
						productsArray.put(productId);
					}

					JSONObject purchaseObject = new JSONObject();
					purchaseObject.put("purchaseGuid", purchase.getAccountIdentifiers().getObfuscatedAccountId());
					purchaseObject.put("developerPayload", purchase.getDeveloperPayload());
					purchaseObject.put("orderId", purchase.getOrderId());
					purchaseObject.put("originalJson", purchase.getOriginalJson());
					purchaseObject.put("packageName", purchase.getPackageName());
					purchaseObject.put("productIds", productsArray);
					purchaseObject.put("purchaseState", getPurchaseStateString(purchase.getPurchaseState()));
					purchaseObject.put("purchaseTime", Long.toString(purchase.getPurchaseTime()));
					purchaseObject.put("purchaseToken", purchase.getPurchaseToken());
					purchaseObject.put("quantity", purchase.getQuantity());
					purchaseObject.put("signature", purchase.getSignature());
					purchaseObject.put("isAcknowledged", purchase.isAcknowledged());
					purchaseObject.put("isAutoRenewing", purchase.isAutoRenewing());

					if (purchase.getAccountIdentifiers().getObfuscatedAccountId() != null &&
						purchase.getAccountIdentifiers().getObfuscatedAccountId().equals(this.pendingPurchaseGuid) &&
						this.pendingOperation == OperationMode.PURCHASING) {
						purchaseForPendingOperation = purchaseObject;
					} else {
						results.put(purchaseObject);
					}
				}

				if (purchaseForPendingOperation != null) {
					// Notify the specific purchase operation of the purchase result that it was interested in.
					int inDispatchId = this.getAndClearPendingDispatchId(OperationMode.PURCHASING);
					printDebug("Calling back into nativeInitiatePurchaseComplete due to purchase success.");
					finalizeInitiatePurchaseComplete(
						inDispatchId,
						true,
						false,
						"",
						purchaseForPendingOperation.toString());
				}

				// @note: We intentionally do this prior to QUERYING_RECEIPTS callback so that the
				// purchases will get loaded into the KnownReceipts array on the C++ side before the
				// QueryReceipts operation returns. The callback on QueryReceipts does not include
				// the queried receipts, so nativeNotifyOutOfBandPurchases is the only way to get receipt
				// data into the system outside of a purchase.
				if (results.length() > 0) {
					// Notify the out-of-band purchase notification system that we received purchase information
					// that wasn't associated with an in-progress purchase.
					nativeNotifyOutOfBandPurchases(results.toString(), true, "");
				}

				if (pendingOperation == OperationMode.QUERYING_RECEIPTS) {
					// Notify the specific query operation of the query results.
					int inDispatchId = this.getAndClearPendingDispatchId(OperationMode.QUERYING_RECEIPTS);
					printDebug("Calling back into nativeQueryReceiptsComplete due to query success.");
					finalizeQueryReceiptsComplete(inDispatchId, true, "");
				}
			} catch (JSONException e) {
				switch (this.pendingOperation) {
					case PURCHASING: {
						int inDispatchId = this.getAndClearPendingDispatchId(OperationMode.PURCHASING);
						printDebug("Calling back into nativeInitiatePurchaseComplete due to exception");
						finalizeInitiatePurchaseComplete(inDispatchId, false, false, e.toString(), "");
					}
					break;
					case QUERYING_RECEIPTS: {
						int inDispatchId = this.getAndClearPendingDispatchId(OperationMode.QUERYING_RECEIPTS);
						printDebug("Calling back into nativeQueryReceiptsComplete due to exception");
						finalizeQueryReceiptsComplete(inDispatchId, false, e.toString());
					}
					break;
					default: {
						printError(
							"Failed to serialize purchases due to JSON exception for unknown operation: " + e.toString());
						e.printStackTrace();
						break;
					}
				}
			}
		} else {
			if (this.pendingOperation == OperationMode.PURCHASING) {
				int inDispatchId = this.getAndClearPendingDispatchId(OperationMode.PURCHASING);

				if (purchaseBillingResult.getResponseCode() == BillingClient.BillingResponseCode.USER_CANCELED) {
					printDebug("Calling back into nativeInitiatePurchaseComplete due to user cancellation.");
					finalizeInitiatePurchaseComplete(inDispatchId, false, true, "", "");
				} else if (
					purchaseBillingResult.getResponseCode() == BillingClient.BillingResponseCode.OK &&
						purchaseList == null) {
					printDebug(
						"Calling back into nativeInitiatePurchaseComplete due to unexpected null purchase list.");
					finalizeInitiatePurchaseComplete(
						inDispatchId,
						false,
						false,
						"The purchase list was unexpectedly null in the Java code.",
						"");
				} else {
					printDebug(
						"Calling back into nativeInitiatePurchaseComplete due to billing failure: " +
							getMessageForBillingResult(purchaseBillingResult));
					finalizeInitiatePurchaseComplete(
						inDispatchId,
						false,
						false,
						"Failed to finish purchase on Google Play: " +
							getMessageForBillingResult(purchaseBillingResult),
						"");
				}
			} else if (this.pendingOperation == OperationMode.QUERYING_RECEIPTS) {
				int inDispatchId = this.getAndClearPendingDispatchId(OperationMode.QUERYING_RECEIPTS);
				printDebug("Calling back into nativeQueryReceiptsComplete due to operation failure");
				finalizeQueryReceiptsComplete(
					inDispatchId,
					false,
					"Failed to query receipts on Google Play: " + getMessageForBillingResult(purchaseBillingResult));
			} else {
				// @note: This code path is not expected to fire at the moment, due to a bug in Google Play Billing
				// which does not notify applications when slow cards fail. See the following issues for information:
				// - https://github.com/android/play-billing-samples/issues/272
				// - https://issuetracker.google.com/issues/149668311

				printDebug("Received failing onPurchasesUpdated outside of operation; this is a deferred failure.");

				try {
					JSONArray results = new JSONArray();

					if (purchaseList != null) {
						for (Purchase purchase : purchaseList) {
							JSONArray productsArray = new JSONArray();
							for (String productId : purchase.getProducts()) {
								productsArray.put(productId);
							}

							JSONObject purchaseObject = new JSONObject();
							purchaseObject.put(
								"purchaseGuid",
								purchase.getAccountIdentifiers().getObfuscatedAccountId());
							purchaseObject.put("developerPayload", purchase.getDeveloperPayload());
							purchaseObject.put("orderId", purchase.getOrderId());
							purchaseObject.put("originalJson", purchase.getOriginalJson());
							purchaseObject.put("packageName", purchase.getPackageName());
							purchaseObject.put("productIds", productsArray);
							purchaseObject.put("purchaseState", getPurchaseStateString(purchase.getPurchaseState()));
							purchaseObject.put("purchaseTime", Long.toString(purchase.getPurchaseTime()));
							purchaseObject.put("purchaseToken", purchase.getPurchaseToken());
							purchaseObject.put("quantity", purchase.getQuantity());
							purchaseObject.put("signature", purchase.getSignature());
							purchaseObject.put("isAcknowledged", purchase.isAcknowledged());
							purchaseObject.put("isAutoRenewing", purchase.isAutoRenewing());

							results.put(purchaseObject);
						}
					}

					// Notify the out-of-band purchase notification system that we received failing out-of-band
					// receipts. We fire the event even if there are no purchases attached, so we can at least log the
					// error in Unreal.
					nativeNotifyOutOfBandPurchases(
						results.toString(),
						false,
						"Received out-of-band receipts with a failure state: " +
							getMessageForBillingResult(purchaseBillingResult));
				} catch (JSONException e) {
					printError(

						"Failed to serialize purchases due to JSON exception for out-of-band failure: " + e.toString());
					e.printStackTrace();
				}
			}
		}
	}

	interface BillingRequestOperation {
		public void performOperation(boolean didConnect, String connectionError);
	}

	private void makeBillingRequest(BillingRequestOperation operation) {
		if (this.billingClientDisconnected) {
			// Add the operation to the delay list. Whether we start the connection, or
			// whether it's already in-progress, the operation will always get
			// invoked with the result.
			this.delayedBillingOperations.add(operation);

			if (!this.billingConnectionInProgress) {
				// We need to start the reconnection.
				this.billingConnectionInProgress = true;
				this.billingClient.startConnection(this);
			}
		} else {
			operation.performOperation(true, "");
		}
	}

	@Override
	public void onBillingSetupFinished(@NonNull BillingResult billingResult) {
		this.billingConnectionInProgress = false;
		if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
			this.billingClientDisconnected = false;
		}
		List<BillingRequestOperation> pendingOperations = this.delayedBillingOperations;
		this.delayedBillingOperations = new ArrayList<BillingRequestOperation>();
		if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK) {
			for (BillingRequestOperation op : pendingOperations) {
				op.performOperation(true, "");
			}
		} else {
			for (BillingRequestOperation op : pendingOperations) {
				op.performOperation(false, getMessageForBillingResult(billingResult));
			}
		}
	}

	@Override
	public void onBillingServiceDisconnected() {
		if (this.pendingOperation == OperationMode.PURCHASING) {
			printDebug("Calling back into nativeInitiatePurchaseComplete due to service disconnection.");
			finalizeInitiatePurchaseComplete(
				this.pendingPurchaseDispatchId,
				false,
				false,
				"Disconnected from Google Play during purchase",
				"");
		}

		if (this.pendingOperation == OperationMode.QUERYING_RECEIPTS) {
			printDebug("Calling back into nativeQueryReceiptsComplete due to service disconnection.");
			finalizeQueryReceiptsComplete(
				this.pendingQueryDispatchId,
				false,
				"Disconnected from Google Play during purchase");
		}

		printDebug("Cleared dispatch IDs because the billing service disconnected.");
		this.pendingPurchaseDispatchId = -1;
		this.pendingPurchaseGuid = null;
		this.pendingQueryDispatchId = -1;
		this.billingClientDisconnected = true;
	}
}