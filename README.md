# XR Plugin Automated Input Tests

This package contains automated tests meant to exercise a XR Plugin Input Provider.  Active devices must be hooked up to the machine running these tests. These tests do not guarantee complete compliance.  Make sure to run the manual tests as well for full coverage.

How to add this to your project:
- Open the package manager window
- Press the "+" button in the upper left corner of package manager, and select to add a package from a git URL.
- In the text box that appears, paste in this URL: "https://github.com/Unity-Technologies/com.unity.xr.certinputauto.git"
- Open the manifest.json for your project and add this package to the "testables" list.  If no testables list exists, add the following as a top level entry (at the same level as the "dependencies" list)"
"testables": [
  "com.unity.xr.certinputauto"
]

The VerifyConfiguration test is optional.  It allows you to smoke test that your XR system is set up correctly.  To enable it, create a file in your project at "Assets/Resources/XRInputProviderAutomatedTestConfig.json" of the following format:

```
{
  "FramesToDelayForTests": 0,
  "DeviceNames": [
    "Mock Head Mounted Display",
    "Mock Controller - Left",
    "Mock Controller - Right"
  ]
}

```
