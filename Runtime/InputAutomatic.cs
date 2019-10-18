using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using UnityEngine;
using UnityEngine.XR;

public class InputAutomatic
{
    bool ContainsFeatureWithName(List<InputFeatureUsage> features, string name)
    {
        for (int i = 0; i < features.Count; i++)
        {
            if (features[i].name == name)
                return true;
        }

        return false;
    }

    struct InputAutoConfiguration
    {
        public int FramesToDelayForTests;
        public string [] DeviceNames;
    }

    IEnumerator WaitForFrames()
    {
        TextAsset FileText = Resources.Load<TextAsset>("XRInputProviderAutomatedTestConfig");

        if (FileText == null)
        {
            Debug.Log("No configuration file has been found. WaitForFrames is skipping.");
            yield break;
        }

        InputAutoConfiguration Config = JsonUtility.FromJson<InputAutoConfiguration>(FileText.text);

        for (int i = 0; i < Config.FramesToDelayForTests; i++)
            yield return null;
    }

    [UnityTest]
    [Description("This test will run if there is a Resources/XRInputProviderAutomatedTestConfig.json defined.  It verifies a particular configuration for test.  If the configuration file is not defined, then tests will run assuming the specific configuration is correct for the purposes of running tests.")]
    public IEnumerator VerifyConfiguration()
    {
        TextAsset FileText = Resources.Load<TextAsset>("XRInputProviderAutomatedTestConfig");

        if (FileText == null)
        {
            Debug.Log("No configuration file has been found");
            yield break;
        }

        InputAutoConfiguration Config = JsonUtility.FromJson<InputAutoConfiguration>(FileText.text);
        
        Debug.Log("Expected Configuration: FramesToDelayForTests = " + Config.FramesToDelayForTests + ". DeviceCount = " + Config.DeviceNames.Length + ". DeviceNames: ");
        for (int i = 0; i < Config.DeviceNames.Length; i++)
        {
            Debug.Log("[" + i + "] " + Config.DeviceNames[i]);
        }

        yield return WaitForFrames();

        List<InputDevice> Devices = new List<InputDevice>();
        InputDevices.GetDevices(Devices);

        Debug.Log("\nObserved Configuration: DeviceCount = " + Devices.Count + ". DeviceNames: ");
        for (int i = 0; i < Devices.Count; i++)
        {
            Debug.Log("[" + i + "] " + Devices[i].name);
        }

        Assert.AreEqual(Config.DeviceNames.Length, Devices.Count, "Comparing expected number of devices to observed number of devices");

        for (int i = 0; i < Config.DeviceNames.Length; i++)
        {
            for (int j = 0; j < Devices.Count; j++)
            {
                if (Config.DeviceNames[i] == Devices[j].name)
                {
                    Devices.RemoveAt(j);
                    break;
                }
            }
        }
        Assert.AreEqual(0, Devices.Count, "Error: expected device names did not match observed device names.");
    }

    [UnityTest]
    [Description("This test verifies the Related Usage Definitions Section of the Input Rules document.")]
    public IEnumerator RelatedUsageDefinitions()
    {
        yield return WaitForFrames();

        List<InputDevice> Devices = new List<InputDevice>();
        InputDevices.GetDevices(Devices);

        Debug.Log(Devices.Count + " devices Found.");
        Assert.AreNotEqual(0, Devices.Count, "No devices found. This test applies to devices reported by this Input Provider. Please complete a full device setup before rerunning this test.");

        for (int i = 0; i < Devices.Count; i++)
        {
            Debug.Log("Testing device: " + Devices[i].name);
            List<InputFeatureUsage> Features = new List<InputFeatureUsage>();
            Devices[i].TryGetFeatureUsages(Features);
            if (Features.Count == 0)
                break;

            // If either PrimaryTouch or SecondaryButton exists
            // then PrimaryButton must exist
            if (ContainsFeatureWithName(Features, "PrimaryTouch") 
                || ContainsFeatureWithName(Features, "SecondaryButton")
                )
                Assert.IsTrue(ContainsFeatureWithName(Features, "PrimaryButton"), "If a PrimaryTouch usage or a SecondaryButton usage exist, then a PrimaryButton usage must exist.");

            // If SecondaryTouch exists
            // then PrimaryTouch must exist
            if (ContainsFeatureWithName(Features, "SecondaryTouch"))
                Assert.IsTrue(ContainsFeatureWithName(Features, "PrimaryTouch"), "If a SecondaryTouch usage exists, then a PrimaryTouch usage must exist");

            // If SecondaryTouch exists
            // then SecondaryButton must exist
            if (ContainsFeatureWithName(Features, "SecondaryTouch"))
                Assert.IsTrue(ContainsFeatureWithName(Features, "SecondaryButton"), "If a SecondaryTouch usage exists, then a SecondaryButton usage must exist");

            // If Primary2DAxisTouch, Primary2DAxisClick, or Secondary2DAxis exist
            // then Primary2DAxis must exist
            if (ContainsFeatureWithName(Features, "Primary2DAxisTouch") 
                || ContainsFeatureWithName(Features, "Primary2DAxisClick")
                || ContainsFeatureWithName(Features, "Secondary2DAxis")
                )
                Assert.IsTrue(ContainsFeatureWithName(Features, "Primary2DAxis"), "If a Primary2DAxisTouch, Primary2DAxisClick, or Secondary2DAxis usage exist, then a Primary2DAxis usage must exist.");

            // If either Trigger or TriggerButton exist then both must exist.
            Assert.IsTrue(!(ContainsFeatureWithName(Features, "Trigger") ^ ContainsFeatureWithName(Features, "TriggerButton")), "If either a Trigger or TriggerButton usage exists, then both must exist.");

            // If either Grip or GripButton exist then both must exist.
            Assert.IsTrue(!(ContainsFeatureWithName(Features, "Grip") ^ ContainsFeatureWithName(Features, "GripButton")), "If either a Grip or GripButton usage exists, then both must exist.");

        }
    }

    [UnityTest]
    [Description("This test verifies that a HMD/Generic role device has the correct tracking usages.")]
    public IEnumerator TrackingUsagesRoleGeneric()
    {
        yield return WaitForFrames();

        List<InputDevice> Devices = new List<InputDevice>();
        InputDevices.GetDevices(Devices);

        Debug.Log(Devices.Count + " devices Found.");
        Assert.AreNotEqual(0, Devices.Count, "No devices found. This test applies to devices reported by this Input Provider. Please complete a full device setup before rerunning this test.");


        int HMDCount = 0;
        for (int i = 0; i < Devices.Count; i++)
        {
            if (Devices[i].role == InputDeviceRole.Generic)
            {
                Debug.Log(Devices[i].name + " is a HMD. Running checks...");
                List<InputFeatureUsage> Features = new List<InputFeatureUsage>();
                Devices[i].TryGetFeatureUsages(Features);

                Assert.IsTrue(ContainsFeatureWithName(Features, "DeviceRotation"), "A DeviceRotation usage must exist.");
                Assert.IsTrue(ContainsFeatureWithName(Features, "LeftEyeRotation"), "A LeftEyeRotation usage must exist.");
                Assert.IsTrue(ContainsFeatureWithName(Features, "RightEyeRotation"), "A RightEyeRotation usage must exist.");
                Assert.IsTrue(ContainsFeatureWithName(Features, "CenterEyeRotation"), "A CenterEyeRotation usage must exist.");
                HMDCount++;
            }
            else {
                Debug.Log(Devices[i].name + " is NOT a HMD. skipping to next device...");
            }
        }

        Debug.Log(HMDCount + " HMDs found.");
    }

    [UnityTest]
    [Description("This test verifies that a TrackingReference device has the correct tracking usages.")]
    public IEnumerator TrackingUsagesRoleTrackingReference()
    {
        yield return WaitForFrames();

        List<InputDevice> Devices = new List<InputDevice>();
        InputDevices.GetDevices(Devices);

        Debug.Log(Devices.Count + " devices Found.");
        Assert.AreNotEqual(0, Devices.Count, "No devices found. This test applies to devices reported by this Input Provider. Please complete a full device setup before rerunning this test.");

        int TrackingReferenceCount = 0;
        for (int i = 0; i < Devices.Count; i++)
        {
            if (Devices[i].role == InputDeviceRole.TrackingReference)
            {
                Debug.Log(Devices[i].name + " is a TrackingReference. Running checks...");
                List<InputFeatureUsage> Features = new List<InputFeatureUsage>();
                Devices[i].TryGetFeatureUsages(Features);

                Assert.IsTrue(ContainsFeatureWithName(Features, "DevicePosition"), "A DevicePosition usage must exist.");
                Assert.IsTrue(ContainsFeatureWithName(Features, "DeviceRotation"), "A DeviceRotation usage must exist.");

                TrackingReferenceCount++;
            }
            else {
                Debug.Log(Devices[i].name + " is NOT a TrackingReference. Skipping to next device...");
            }
        }

        Debug.Log(TrackingReferenceCount + " TrackingReferences found.");
    }

    [UnityTest]
    [Description("This test verifies that a HardwareTracker device has the correct tracking usages.")]
    public IEnumerator TrackingUsagesRoleHardwareTracker()
    {
        yield return WaitForFrames();

        List<InputDevice> Devices = new List<InputDevice>();
        InputDevices.GetDevices(Devices);

        Debug.Log(Devices.Count + " devices Found.");
        Assert.AreNotEqual(0, Devices.Count, "No devices found. This test applies to devices reported by this Input Provider. Please complete a full device setup before rerunning this test.");

        int HardwareTrackerCount = 0;
        for (int i = 0; i < Devices.Count; i++)
        {
            if (Devices[i].role == InputDeviceRole.HardwareTracker)
            {
                Debug.Log(Devices[i].name + " is a HardwareTracker. Running checks...");
                List<InputFeatureUsage> Features = new List<InputFeatureUsage>();
                Devices[i].TryGetFeatureUsages(Features);

                Assert.IsTrue(ContainsFeatureWithName(Features, "DevicePosition"), "A DevicePosition usage must exist.");
                Assert.IsTrue(ContainsFeatureWithName(Features, "DeviceRotation"), "A DeviceRotation usage must exist.");
                HardwareTrackerCount++;
            }
            else {
                Debug.Log(Devices[i].name + " is NOT a HardwareTracker. Skipping to next device...");
            }
        }

        Debug.Log(HardwareTrackerCount + " HardwareTrackers found.");
    }

    [UnityTest]
    [Description("This test verifies that each tracked device contatins the minimum set of features.")]
    public IEnumerator TrackingUsagesDeviceDefinition()
    {
        yield return WaitForFrames();

        List<InputDevice> Devices = new List<InputDevice>();
        InputDevices.GetDevices(Devices);

        Debug.Log(Devices.Count + " devices Found.");
        Assert.AreNotEqual(0, Devices.Count, "No devices found. This test applies to devices reported by this Input Provider. Please complete a full device setup before rerunning this test.");

        for (int i = 0; i < Devices.Count; i++)
        {
            Debug.Log("Testing device: " + Devices[i].name);
            List<InputFeatureUsage> Features = new List<InputFeatureUsage>();
            Devices[i].TryGetFeatureUsages(Features);

            if ((0 != (Devices[i].characteristics & InputDeviceCharacteristics.TrackedDevice))
                || ContainsFeatureWithName(Features, "IsTracked")
                || ContainsFeatureWithName(Features, "TrackingState")
                || ContainsFeatureWithName(Features, "DevicePosition")
                || ContainsFeatureWithName(Features, "DeviceRotation")
                || ContainsFeatureWithName(Features, "DeviceVelocity")
                || ContainsFeatureWithName(Features, "DeviceAngularVelocity")
                || ContainsFeatureWithName(Features, "DeviceAcceleration")
                || ContainsFeatureWithName(Features, "DeviceAngularAcceleration")
                )
                Assert.IsTrue((0 != (Devices[i].characteristics & InputDeviceCharacteristics.TrackedDevice))
                && ContainsFeatureWithName(Features, "IsTracked")
                && ContainsFeatureWithName(Features, "TrackingState")
                && (ContainsFeatureWithName(Features, "DevicePosition")
                || ContainsFeatureWithName(Features, "DeviceRotation")
                || ContainsFeatureWithName(Features, "DeviceVelocity")
                || ContainsFeatureWithName(Features, "DeviceAngularVelocity")
                || ContainsFeatureWithName(Features, "DeviceAcceleration")
                || ContainsFeatureWithName(Features, "DeviceAngularAcceleration")),
                "At a minimum, a tracked device must have the IsTracked, TrackingState, and one the Device___ usages."
                );
        }
    }

    [UnityTest]
    [Description("This test verifies that haptics capabilities adhere to correct limits.")]
    public IEnumerator HapticCapabilitiesSanityCheck()
    {
        yield return WaitForFrames();

        List<InputDevice> Devices = new List<InputDevice>();
        InputDevices.GetDevices(Devices);

        Debug.Log(Devices.Count + " devices Found.");
        Assert.AreNotEqual(0, Devices.Count, "No devices found. This test applies to devices reported by this Input Provider. Please complete a full device setup before rerunning this test.");

        for (int i = 0; i < Devices.Count; i++)
        {
            Debug.Log("Testing device: " + Devices[i].name);
            HapticCapabilities hapticCapabilities;

            if (!Devices[i].TryGetHapticCapabilities(out hapticCapabilities))
                continue;

            if (hapticCapabilities.supportsBuffer) {
                Assert.IsTrue(hapticCapabilities.bufferFrequencyHz > 0, "Supports buffer is true, HapticCapabilities.bufferFrequencyHz is zero");
                Assert.IsTrue(hapticCapabilities.bufferOptimalSize > 0, "Supports buffer is true, HapticCapabilities.bufferOptimalSize is zero");
                Assert.IsTrue(hapticCapabilities.bufferOptimalSize <= hapticCapabilities.bufferMaxSize, "Error! HapticCapabilities.bufferOptimalSize > HapticCapabilities.bufferMaxSize!");
                Assert.IsTrue(hapticCapabilities.bufferOptimalSize <= 4096, "HapticCapabilities.bufferOptimalSize = " + hapticCapabilities.bufferOptimalSize + " detected as greater than kUnityXRMaxHapticBufferSize."); // kUnityXRMaxHapticBufferSize
            }
            else
            {
                Assert.AreEqual(0, hapticCapabilities.bufferFrequencyHz, "Supports buffer is false, but HapticCapabilities.bufferFrequencyHz != 0");
                Assert.AreEqual(0, hapticCapabilities.bufferOptimalSize, "Supports buffer is false, but HapticCapabilities.bufferOptimalSize != 0");
                Assert.AreEqual(0, hapticCapabilities.bufferMaxSize, "Supports buffer is false, but HapticCapabilities.bufferMaxSize != 0");
            }
        }
    }

    [UnityTest]
    [Description("This test verifies that there are no repeated features in a device's features list.")]
    public IEnumerator UsagesNoRepeats()
    {
        yield return WaitForFrames();

        List<InputDevice> Devices = new List<InputDevice>();
        InputDevices.GetDevices(Devices);

        Debug.Log(Devices.Count + " devices Found.");
        Assert.AreNotEqual(0, Devices.Count, "No devices found. This test applies to devices reported by this Input Provider. Please complete a full device setup before rerunning this test.");

        for (int i = 0; i < Devices.Count; i++)
        {
            Debug.Log("Testing device: " + Devices[i].name);
            List<InputFeatureUsage> Features = new List<InputFeatureUsage>();
            Devices[i].TryGetFeatureUsages(Features);

            for (int j = 0; j < Features.Count - 1; j++)
            {
                for (int k = j + 1; k < Features.Count; k++)
                {
                    Assert.AreNotEqual(Features[j].name, Features[k].name, "The feature usage " + Features[j].name + " is duplicated in this device.");
                }
            }
        }
    }

    [UnityTest]
    [Description("This test verifies that all features are backed by the correct values types.")]
    public IEnumerator UsagesCorrectBackingValues()
    {
        yield return WaitForFrames();

        List<InputDevice> Devices = new List<InputDevice>();
        InputDevices.GetDevices(Devices);

        Debug.Log(Devices.Count + " devices Found.");
        Assert.AreNotEqual(0, Devices.Count, "No devices found. This test applies to devices reported by this Input Provider. Please complete a full device setup before rerunning this test.");

        for (int i = 0; i < Devices.Count; i++)
        {
            Debug.Log("Testing device: " + Devices[i].name);
            List<InputFeatureUsage> Features = new List<InputFeatureUsage>();
            Devices[i].TryGetFeatureUsages(Features);

            for (int j = 0; j < Features.Count; j++)
            {
                switch (Features[j].name)
                {
                    case "TrackingState":
                        Assert.IsTrue(Features[j].type == typeof(uint), Features[j].name + " should be of type uint, but is observed as type " + Features[j].type);
                        break;
                    case "IsTracked":
                    case "PrimaryButton":
                    case "PrimaryTouch":
                    case "SecondaryButton":
                    case "SecondaryTouch":
                    case "GripButton":
                    case "TriggerButton":
                    case "MenuButton":
                    case "Primary2DAxisClick":
                    case "Primary2DAxisTouch":
                    case "Thumbrest":
                        Assert.IsTrue(Features[j].type == typeof(bool), Features[j].name + " should be of type bool, but is observed as type " + Features[j].type);
                        break;
                    case "Trigger":
                    case "Grip":
                    case "IndexTouch":
                    case "ThumbTouch":
                    case "IndexFinger":
                    case "MiddleFinger":
                    case "RingFinger":
                    case "PinkyFinger":
                    case "BatteryLevel":
                        Assert.IsTrue(Features[j].type == typeof(float), Features[j].name + " should be of type float, but is observed as type " + Features[j].type);
                        break;
                    case "Primary2DAxis":
                    case "Secondary2DAxis":
                        Assert.IsTrue(Features[j].type == typeof(Vector2), Features[j].name + " should be of type Vector2, but is observed as type " + Features[j].type);
                        break;
                    case "DevicePosition":
                    case "DeviceVelocity":
                    case "DeviceAcceleration":
                    case "DeviceAngularVelocity":
                    case "DeviceAngularAcceleration":
                    case "ColorCameraPosition":
                    case "ColorCameraVelocity":
                    case "ColorCameraAcceleration":
                    case "ColorCameraAngularVelocity":
                    case "ColorCameraAngularAcceleration":
                    case "CenterEyePosition":
                    case "CenterEyeVelocity":
                    case "CenterEyeAcceleration":
                    case "CenterEyeAngularVelocity":
                    case "CenterEyeAngularAcceleration":
                    case "LeftEyePosition":
                    case "LeftEyeVelocity":
                    case "LeftEyeAcceleration":
                    case "LeftEyeAngularVelocity":
                    case "LeftEyeAngularAcceleration":
                    case "RightEyePosition":
                    case "RightEyeVelocity":
                    case "RightEyeAcceleration":
                    case "RightEyeAngularVelocity":
                    case "RightEyeAngularAcceleration":
                        Assert.IsTrue(Features[j].type == typeof(Vector3), Features[j].name + " should be of type Vector3, but is observed as type " + Features[j].type);
                        break;
                    case "DeviceRotation":
                    case "ColorCameraRotation":
                    case "CenterEyeRotation":
                    case "LeftEyeRotation":
                    case "RightEyeRotation":
                        Assert.IsTrue(Features[j].type == typeof(Quaternion), Features[j].name + " should be of type Quaternion, but is observed as type " + Features[j].type);
                        break;
                    default:
                        FieldInfo[] fields = typeof(CommonUsages).GetFields();
                        bool FieldIsInCommonUsages = false;
                        for (int k = 0; k < fields.Length; k++)
                        {
                            if (fields[k].Name == Features[j].name)
                            {
                                FieldIsInCommonUsages = true;
                                break;
                            }
                        }
                        if (FieldIsInCommonUsages)
                            Assert.IsTrue(false, "Error: " + Features[j].name + " is in Common Usages.  Its backing value needs to be added to this test.");
                        else
                            Debug.Log("Provider specific feature detected: \"" + Features[j].name + ".\"  This test is unable to verify provider-specific usage backing values.");
                        break;
                }
            }
        }
    }

    [UnityTest]
    [Description("This test verifies that XRInputSubsystem.TryGetDevices works properly when compared to InputDevices.GetDevices.")]
    public IEnumerator GetDevicesFromSubsystem()
    {
        yield return WaitForFrames();

        List<InputDevice> allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);

        Debug.Log(allDevices.Count + " devices Found.");       
        Assert.AreNotEqual(0, allDevices.Count, "No devices found. This test applies to devices reported by this Input Provider. Please complete a full device setup before rerunning this test.");

        List<XRInputSubsystem> InputSubsystemInstances = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances<XRInputSubsystem>(InputSubsystemInstances);

        List<InputDevice> DevicesForCurrentSubsystem = new List<InputDevice>();
        for (int i = 0; i < InputSubsystemInstances.Count; i++)
        {
            DevicesForCurrentSubsystem.Clear();
            XRInputSubsystem CurrentSubsystem = InputSubsystemInstances[i];
            CurrentSubsystem.TryGetInputDevices(DevicesForCurrentSubsystem);

            for (int j = DevicesForCurrentSubsystem.Count - 1; j >= 0; j--)
            {
                InputDevice CurrentDevice = DevicesForCurrentSubsystem[j];

                Assert.IsTrue(allDevices.Contains(CurrentDevice), "Device \"" + CurrentDevice.name + "\" must be reported by InputDevices.");
                Assert.AreEqual(CurrentDevice.subsystem, CurrentSubsystem, "Device \"" + CurrentDevice.name + "\" must report as from the same subsystem that it was pulled from, which was \"" + CurrentSubsystem.SubsystemDescriptor.id + "\"");
                allDevices.Remove(CurrentDevice);
                DevicesForCurrentSubsystem.Remove(CurrentDevice);
            }

            Assert.AreEqual(0, DevicesForCurrentSubsystem.Count, "\"" + CurrentSubsystem.SubsystemDescriptor.id + "\" must not have devices that are not reported by InputDevices.GetDevices");
        }

        Assert.AreEqual(0, allDevices.Count, "InputDevices.GetDevices is reporting devices that are somehow not reported by a XRInputSubsystem");
    }
}
