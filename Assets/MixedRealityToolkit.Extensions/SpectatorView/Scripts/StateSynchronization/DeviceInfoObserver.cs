﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.MixedReality.Toolkit.Extensions.Experimental.Socketer;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions.Experimental.SpectatorView
{
    public class DeviceInfoObserver : MonoBehaviour
    {
        private const float trackingStalledReceiveDelay = 1.0f;

        public const string CreateSharedSpatialCoordinateCommand = "CreateSharedSpatialCoordinate";
        public const string DeviceInfoCommand = "DeviceInfo";
        public const string StatusCommand = "Status";
        public const float arUcoMarkerSizeInMeters = 0.1f;

        private INetworkManager networkManager;
        private SocketEndpoint connectedEndpoint;
        private float lastReceivedPoseTime = -1;
        private string deviceName;
        private string deviceIPAddress;

        /// <summary>
        /// Gets the network manager associated with the device.
        /// </summary>
        public INetworkManager NetworkManager => networkManager;

        /// <summary>
        /// Gets the SocketEndpoint for the currently-connected device.
        /// </summary>
        public SocketEndpoint ConnectedEndpoint => connectedEndpoint;

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        public string DeviceName => deviceName;

        /// <summary>
        /// Gets the IP address reported by the device itself.
        /// </summary>
        public string DeviceIPAddress => deviceIPAddress;

        /// <summary>
        /// Gets whether or not the receipt of new poses from the device has stalled for an unexpectedly-large time.
        /// </summary>
        public bool IsTrackingStalled => networkManager.IsConnected && (Time.time - lastReceivedPoseTime) > trackingStalledReceiveDelay;

        private void Awake()
        {
            networkManager = GetComponent<INetworkManager>();
            networkManager.Connected += OnConnected;
            networkManager.Disconnected += OnDisconnected;
            networkManager.RegisterCommandHandler(DeviceInfoCommand, HandleDeviceInfoCommand);
            networkManager.RegisterCommandHandler(StateSynchronizationObserver.CameraCommand, HandleCameraCommand);
        }

        private void OnDestroy()
        {
            if (networkManager != null)
            {
                networkManager.Connected -= OnConnected;
                networkManager.Disconnected -= OnDisconnected;
                networkManager.UnregisterCommandHandler(DeviceInfoCommand, HandleDeviceInfoCommand);
                networkManager.UnregisterCommandHandler(StateSynchronizationObserver.CameraCommand, HandleCameraCommand);
            }
        }

        private void OnConnected(SocketEndpoint endpoint)
        {
            connectedEndpoint = endpoint;
            lastReceivedPoseTime = Time.time;
        }

        private void OnDisconnected(SocketEndpoint endpoint)
        {
            if (connectedEndpoint == endpoint)
            {
                connectedEndpoint = null;
            }
        }

        private void HandleDeviceInfoCommand(SocketEndpoint endpoint, string command, BinaryReader reader, int remainingDataSize)
        {
            deviceName = reader.ReadString();
            deviceIPAddress = reader.ReadString();
        }

        private void HandleCameraCommand(SocketEndpoint endpoint, string command, BinaryReader reader, int remainingDataSize)
        {
            lastReceivedPoseTime = Time.time;
        }
    }
}