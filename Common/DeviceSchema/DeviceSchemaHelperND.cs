﻿using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Schema;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.DeviceSchema
{
    /// <summary>
    /// Helper class to encapsulate interactions with the device schema.
    ///
    /// Elsewhere in the app we try to always deal with this flexible schema as dynamic,
    /// but here we take a dependency on Json.Net where necessary to populate the objects
    /// behind the schema.
    /// </summary>
    public static class DeviceSchemaHelperND
    {
        /// <summary>
        /// Gets a DeviceProperties instance from a Device.
        /// </summary>
        /// <param name="device">
        /// The Device from which to extract a DeviceProperties instance.
        /// </param>
        /// <returns>
        /// A DeviceProperties instance, extracted from <paramref name="device"/>.
        /// </returns>
        public static DeviceProperties GetDeviceProperties(DeviceND device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = device.DeviceProperties;

            if (props == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceProperties' property is missing");
            }

            return props;
        }

        /// <summary>
        /// Gets a IoTHubProperties instance from a device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static IoTHub GetIoTHubProperties(DeviceND device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = device.IoTHub;

            if (props == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'IoTHubProperties' property is missing");
            }

            return props;
        }

        /// <summary>
        /// Gets a Device instance's Device ID.
        /// </summary>
        /// <param name="device">
        /// The Device instance from which to extract a Device ID.
        /// </param>
        /// <returns>
        /// The Device ID, extracted from <paramref name="device" />.
        /// </returns>
        public static string GetDeviceID(DeviceND device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetDeviceProperties(device);

            string deviceID = props.DeviceID;

            if (deviceID == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceID' property is missing");
            }

            return deviceID;
        }

        /// <summary>
        /// Get connection device id 
        /// </summary>
        /// <param name="device">Device instance from message</param>
        /// <returns>Connection device id from IoTHub</returns>
        public static string GetConnectionDeviceId(DeviceND device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetIoTHubProperties(device);

            string deviceID = props.ConnectionDeviceId;

            if (deviceID == null)
            {
                throw new DeviceRequiredPropertyNotFoundException("'DeviceID' property is missing");
            }

            return deviceID;
        }

        /// <summary>
        /// Extract's a Device instance's Created Time value.
        /// </summary>
        /// <param name="device">
        /// The Device instance from which to extract a Created Time value.
        /// </param>
        /// <returns>
        /// A Created Time value, extracted from <paramref name="device" />.
        /// </returns>
        public static DateTime GetCreatedTime(DeviceND device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetDeviceProperties(device);

            DateTime? createdTime = props.CreatedTime;

            if (!createdTime.HasValue || createdTime.Equals(DateTime.MinValue))
            {
                throw new DeviceRequiredPropertyNotFoundException("'CreatedTime' property is missing");
            }

            return createdTime.Value;
        }

        /// <summary>
        /// Extracts an Updated Time value from a Device instance.
        /// </summary>
        /// <param name="device">
        /// The Device instance from which to extract an Updated Time value.
        /// </param>
        /// <returns>
        /// The Updated Time value, extracted from <paramref name="device" />, or
        /// null if it is null or does not exist.
        /// </returns>
        public static DateTime? GetUpdatedTime(DeviceND device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetDeviceProperties(device);

            // note that since null is a valid value, don't try to test if the actual UpdateTime is there

            return props.UpdatedTime;
        }

        /// <summary>
        /// Set the current time (in UTC) to the device's UpdatedTime Device Property
        /// </summary>
        /// <param name="device"></param>
        public static void UpdateUpdatedTime(DeviceND device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetDeviceProperties(device);

            props.UpdatedTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Extracts a Hub Enabled State value from a Device instance.
        /// </summary>
        /// <param name="device">
        /// The Device instance from which to extract a Hub Enabled State
        /// value.
        /// </param>
        /// <returns>
        /// The Hub Enabled State value extracted from <paramref name="device"/>,
        /// or null if the value is missing or null.
        /// </returns>
        public static bool? GetHubEnabledState(DeviceND device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            var props = GetDeviceProperties(device);

            // note that since null is a valid value, don't try to test if the actual HubEnabledState is there

            return props.HubEnabledState;
        }

        /// <summary>
        /// _rid is used internally by the DocDB and is required for use with DocDB.
        /// (_rid is resource id)
        /// </summary>
        /// <param name="device">Device data</param>
        /// <returns>_rid property value as string, or empty string if not found</returns>
        public static string GetDocDbRid(DeviceND device)
        {
            return SchemaHelperND.GetDocDbRid<DeviceND>(device);
        }

        /// <summary>
        /// id is used internally by the DocDB and is sometimes required.
        /// </summary>
        /// <param name="device">Device data</param>
        /// <returns>Value of the id, or empty string if not found</returns>
        public static string GetDocDbId(DeviceND device)
        {
            return SchemaHelperND.GetDocDbId<DeviceND>(device);
        }

        /// <summary>
        /// Build a valid device representation in the dynamic format used throughout the app.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="isSimulated"></param>
        /// <param name="iccid"></param>
        /// <returns></returns>
        public static DeviceND BuildDeviceStructure(string deviceId, bool isSimulated, string iccid)
        {
            DeviceND device = new DeviceND();

            InitializeDeviceProperties(device, deviceId, isSimulated);
            InitializeSystemProperties(device, iccid);

            device.Commands = new List<Command>();
            device.CommandHistory = new List<CommandHistoryND>();
            device.IsSimulatedDevice = isSimulated;

            return device;
        }

        /// <summary>
        /// Initialize the device properties for a new device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="deviceId"></param>
        /// <param name="isSimulated"></param>
        /// <returns></returns>
        public static void InitializeDeviceProperties(DeviceND device, string deviceId, bool isSimulated)
        {
            DeviceProperties deviceProps = new DeviceProperties();
            deviceProps.DeviceID = deviceId;
            deviceProps.HubEnabledState = null;
            deviceProps.CreatedTime = DateTime.UtcNow;
            deviceProps.DeviceState = "normal";
            deviceProps.UpdatedTime = null;

            device.DeviceProperties = deviceProps;
        }

        /// <summary>
        /// Initialize the system properties for a new device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="iccid"></param>
        /// <returns></returns>
        public static void InitializeSystemProperties(DeviceND device, string iccid)
        {
            SystemProperties systemProps = new SystemProperties();
            systemProps.ICCID = iccid;

            device.SystemProperties = systemProps;
        }

        /// <summary>
        /// Remove the system properties from a device, to better emulate the behavior of real devices when sending device info messages.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="iccid"></param>
        /// <param name="isSimulated"></param>
        /// <returns></returns>
        public static void RemoveSystemPropertiesForSimulatedDeviceInfo(DeviceND device)
        {
            // Our simulated devices share the structure code with the rest of the system,
            // so we need to explicitly handle this case; since this is only an issue when
            // the code is shared in this way, this special case is kept separate from the
            // rest of the initialization code which would be present in a non-simulated system
            device.SystemProperties = null;
        }
    }
}
