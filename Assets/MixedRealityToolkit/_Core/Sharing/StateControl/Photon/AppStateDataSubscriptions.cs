﻿using Photon.Pun;
using System;
using System.Collections.Generic;
#if BINARY_SERIALIZATION
using System.Runtime.Serialization.Formatters.Binary;
#else
#endif
using UnityEngine;

namespace Pixie.StateControl.Photon
{
    public class AppStateDataSubscriptions : MonoBehaviour, IAppStateDataSubscriptions
    {
        private Dictionary<string, HashSet<string>> subscribedTypes = new Dictionary<string, HashSet<string>>();
        private Dictionary<string, SubscriptionModeEnum> subscriptionModes = new Dictionary<string, SubscriptionModeEnum>();
        private object[] rpcReceiveSubscriptionMode = new object[2];
        // Photon-specific component
        private PhotonView photonView;

        public void SetSubscriptionMode(SubscriptionModeEnum subscriptionMode, IEnumerable<Type> subscriptionTypes = null)
        {
            Debug.Log("Setting subscription mode: " + subscriptionMode);

            photonView = GetComponent<PhotonView>();
            if (photonView == null)
                throw new Exception("This component can't operate without a photon view");

            HashSet<string> subscriptionTypeNames = null;

            switch (subscriptionMode)
            {
                case SubscriptionModeEnum.All:
                default:
                    break;

                case SubscriptionModeEnum.Manual:
                    if (subscriptionTypes == null)
                        throw new Exception("Subscription types cannot be null when subscription is manual");

                    subscriptionTypeNames = new HashSet<string>();
                    foreach (Type type in subscriptionTypes)
                        subscriptionTypeNames.Add(type.FullName);

                    // If we have NO subscriptions, that's not okay
                    if (subscriptionTypeNames.Count == 0)
                        throw new Exception("Subscription types cannot be empty when subscription is manual");
                    break;
            }

            // Let everyone know we've changed
            // This will update our own subscription info as well
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
            {
                rpcReceiveSubscriptionMode[0] = subscriptionMode;
                rpcReceiveSubscriptionMode[1] = subscriptionTypeNames;

                photonView.RPC("ReceiveSubscriptionSettings", RpcTarget.AllBuffered, rpcReceiveSubscriptionMode);
            }
            else
            {
                Debug.LogWarning("Not connected - not sending subscription type.");
            }
        }

        [PunRPC]
        private void ReceiveSubscriptionMode(SubscriptionModeEnum newSubscriptionMode, HashSet<string> newStateTypes, PhotonMessageInfo info)
        {
            Debug.Log("Receiving subscription mode: " + newSubscriptionMode);

            if (!subscriptionModes.ContainsKey(info.Sender.UserId))
                subscriptionModes.Add(info.Sender.UserId, newSubscriptionMode);
            else
                subscriptionModes[info.Sender.UserId] = newSubscriptionMode;

            HashSet<string> stateTypes;
            if (!subscribedTypes.TryGetValue(info.Sender.UserId, out stateTypes))
            {
                stateTypes = new HashSet<string>();
                subscribedTypes.Add(info.Sender.UserId, stateTypes);
            }

            // Clear the state array types regardless of mode
            stateTypes.Clear();

            switch (newSubscriptionMode)
            {
                case SubscriptionModeEnum.All:
                default:
                    break;

                case SubscriptionModeEnum.Manual:
                    if (newStateTypes == null)
                        throw new Exception("Subscription types cannot be null when subscription is manual");

                    foreach (string newStateType in newStateTypes)
                        stateTypes.Add(newStateType);

                    if (stateTypes.Count == 0)
                        throw new Exception("Subscription types cannot be empty when subscription is manual");
                    break;
            }
        }

        public bool IsUserSubscribedToType(string photonPlayerID, Type stateArrayType)
        {
            SubscriptionModeEnum modeForPlayer = SubscriptionModeEnum.All;
            // If we don't have a subscription entry for this player then they're subscribed by default
            if (!subscriptionModes.TryGetValue(photonPlayerID, out modeForPlayer))
                return true;

            switch (modeForPlayer)
            {
                case SubscriptionModeEnum.All:
                default:
                    return true;

                case SubscriptionModeEnum.Manual:
                    HashSet<string> stateTypes = subscribedTypes[photonPlayerID];
                    return stateTypes.Contains(stateArrayType.FullName);
            }
        }
    }
}