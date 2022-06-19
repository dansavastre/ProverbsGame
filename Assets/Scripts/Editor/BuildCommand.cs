using UnityEditor;
using System.Linq;
using System;
using System.IO;

static class BuildCommand
{
    // various strings for important attributes of the build commands
    private const string KEYSTORE_PASS  = "KEYSTORE_PASS";
    private const string KEY_ALIAS_PASS = "KEY_ALIAS_PASS";
    private const string KEY_ALIAS_NAME = "KEY_ALIAS_NAME";
    private const string KEYSTORE       = "keystore.keystore";
    private const string BUILD_OPTIONS_ENV_VAR = "BuildOptions";
    private const string ANDROID_BUNDLE_VERSION_CODE = "VERSION_BUILD_VAR";
    private const string ANDROID_APP_BUNDLE = "BUILD_APP_BUNDLE";
    private const string SCRIPTING_BACKEND_ENV_VAR = "SCRIPTING_BACKEND";
    private const string VERSION_NUMBER_VAR = "VERSION_NUMBER_VAR";
    private const string VERSION_iOS = "VERSION_BUILD_VAR";
    
    /// <summary>
    /// Method that gets a certain command line argument.
    /// </summary>
    /// <param name="name">string denoting the name of the argument</param>
    /// <returns>null</returns>
    static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(name))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    /// <summary>
    /// Method for getting the enabled scenes.
    /// </summary>
    /// <returns>a list of strings denoting the names of the enabled scenes</returns>
    static string[] GetEnabledScenes()
    {
        return (
            from scene in EditorBuildSettings.scenes
            where scene.enabled
            where !string.IsNullOrEmpty(scene.path)
            select scene.path
        ).ToArray();
    }

    /// <summary>
    /// Method for getting the build target.
    /// </summary>
    /// <returns>the build target</returns>
    static BuildTarget GetBuildTarget()
    {
        string buildTargetName = GetArgument("customBuildTarget");
        Console.WriteLine(":: Received customBuildTarget " + buildTargetName);

        if (buildTargetName.ToLower() == "android")
        {
#if !UNITY_5_6_OR_NEWER
			// https://issuetracker.unity3d.com/issues/buildoptions-dot-acceptexternalmodificationstoplayer-causes-unityexception-unknown-project-type-0
			// Fixed in Unity 5.6.0
			// side effect to fix android build system:
			EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
#endif
        }

        if (buildTargetName.TryConvertToEnum(out BuildTarget target))
            return target;

        Console.WriteLine($":: {nameof(buildTargetName)} \"{buildTargetName}\" not defined on enum {nameof(BuildTarget)}, using {nameof(BuildTarget.NoTarget)} enum to build");

        return BuildTarget.NoTarget;
    }

    /// <summary>
    /// Method for getting the build path.
    /// </summary>
    /// <returns>string denoting the build path</returns>
    /// <exception cref="Exception">exception to be thrown if the build path argumment is missing</exception>
    static string GetBuildPath()
    {
        string buildPath = GetArgument("customBuildPath");
        Console.WriteLine(":: Received customBuildPath " + buildPath);
        if (buildPath == "")
        {
            throw new Exception("customBuildPath argument is missing");
        }
        return buildPath;
    }

    /// <summary>
    /// Method for getting the name of the build.
    /// </summary>
    /// <returns>string denoting the name of the build</returns>
    /// <exception cref="Exception">exception to be thrown if the build name is missing</exception>
    static string GetBuildName()
    {
        string buildName = GetArgument("customBuildName");
        Console.WriteLine(":: Received customBuildName " + buildName);
        if (buildName == "")
        {
            throw new Exception("customBuildName argument is missing");
        }
        return buildName;
    }

    /// <summary>
    /// Method for getting the fixed build path.
    /// </summary>
    /// <param name="buildTarget">the build target</param>
    /// <param name="buildPath">the build path</param>
    /// <param name="buildName">the build name</param>
    /// <returns>string denoting the fixed build path</returns>
    static string GetFixedBuildPath(BuildTarget buildTarget, string buildPath, string buildName)
    {
        if (buildTarget.ToString().ToLower().Contains("windows")) {
            buildName += ".exe";
        } else if (buildTarget == BuildTarget.Android) {
#if UNITY_2018_3_OR_NEWER
            buildName += EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk";
#else
            buildName += ".apk";
#endif
        }
        return buildPath + buildName;
    }

    /// <summary>
    /// Method for getting the build options.
    /// </summary>
    /// <returns>the build options</returns>
    static BuildOptions GetBuildOptions()
    {
        if (TryGetEnv(BUILD_OPTIONS_ENV_VAR, out string envVar)) {
            string[] allOptionVars = envVar.Split(',');
            BuildOptions allOptions = BuildOptions.None;
            BuildOptions option;
            string optionVar;
            int length = allOptionVars.Length;

            Console.WriteLine($":: Detecting {BUILD_OPTIONS_ENV_VAR} env var with {length} elements ({envVar})");

            for (int i = 0; i < length; i++) {
                optionVar = allOptionVars[i];

                if (optionVar.TryConvertToEnum(out option)) {
                    allOptions |= option;
                }
                else {
                    Console.WriteLine($":: Cannot convert {optionVar} to {nameof(BuildOptions)} enum, skipping it.");
                }
            }

            return allOptions;
        }

        return BuildOptions.None;
    }

    /// <summary>
    /// https://stackoverflow.com/questions/1082532/how-to-tryparse-for-enum-value
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="strEnumValue"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    static bool TryConvertToEnum<TEnum>(this string strEnumValue, out TEnum value)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
        {
            value = default;
            return false;
        }

        value = (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
        return true;
    }

    /// <summary>
    /// Method for trying to get the environment variable.
    /// </summary>
    /// <param name="key">string denoting the key of the environment variable</param>
    /// <param name="value">string denoting the value of the environment variable</param>
    /// <returns>whether or not the searched value is null or empty</returns>
    static bool TryGetEnv(string key, out string value)
    {
        value = Environment.GetEnvironmentVariable(key);
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Method for setting the scripting backend from the environment variables.
    /// </summary>
    /// <param name="platform">the build target</param>
    /// <exception cref="Exception">exception to be thrown if the scripting backend could not be found</exception>
    static void SetScriptingBackendFromEnv(BuildTarget platform) {
        var targetGroup = BuildPipeline.GetBuildTargetGroup(platform);
        if (TryGetEnv(SCRIPTING_BACKEND_ENV_VAR, out string scriptingBackend)) {
            if (scriptingBackend.TryConvertToEnum(out ScriptingImplementation backend)) {
                Console.WriteLine($":: Setting ScriptingBackend to {backend}");
                PlayerSettings.SetScriptingBackend(targetGroup, backend);
            } else {
                string possibleValues = string.Join(", ", Enum.GetValues(typeof(ScriptingImplementation)).Cast<ScriptingImplementation>());
                throw new Exception($"Could not find '{scriptingBackend}' in ScriptingImplementation enum. Possible values are: {possibleValues}");
            }
        } else {
            var defaultBackend = PlayerSettings.GetDefaultScriptingBackend(targetGroup);
            Console.WriteLine($":: Using project's configured ScriptingBackend (should be {defaultBackend} for targetGroup {targetGroup}");
        }
    }

    /// <summary>
    /// Method for performing the build.
    /// </summary>
    /// <exception cref="Exception">exception to be thrown if the build failed</exception>
    static void PerformBuild()
    {
        var buildTarget = GetBuildTarget();

        Console.WriteLine(":: Performing build");
        if (TryGetEnv(VERSION_NUMBER_VAR, out var bundleVersionNumber))
        {
            if (buildTarget == BuildTarget.iOS)
            {
                bundleVersionNumber = GetIosVersion();
            }
            Console.WriteLine($":: Setting bundleVersionNumber to '{bundleVersionNumber}' (Length: {bundleVersionNumber.Length})");
            PlayerSettings.bundleVersion = bundleVersionNumber;
        }

        if (buildTarget == BuildTarget.Android) {
            HandleAndroidAppBundle();
            HandleAndroidBundleVersionCode();
            HandleAndroidKeystore();
        }

        var buildPath      = GetBuildPath();
        var buildName      = GetBuildName();
        var buildOptions   = GetBuildOptions();
        var fixedBuildPath = GetFixedBuildPath(buildTarget, buildPath, buildName);

        SetScriptingBackendFromEnv(buildTarget);

        var buildReport = BuildPipeline.BuildPlayer(GetEnabledScenes(), fixedBuildPath, buildTarget, buildOptions);

        if (buildReport.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception($"Build ended with {buildReport.summary.result} status");

        Console.WriteLine(":: Done with build");
    }

    /// <summary>
    /// Handles the Android app bundle.
    /// </summary>
    private static void HandleAndroidAppBundle()
    {
        if (TryGetEnv(ANDROID_APP_BUNDLE, out string value))
        {
#if UNITY_2018_3_OR_NEWER
            if (bool.TryParse(value, out bool buildAppBundle))
            {
                EditorUserBuildSettings.buildAppBundle = buildAppBundle;
                Console.WriteLine($":: {ANDROID_APP_BUNDLE} env var detected, set buildAppBundle to {value}.");
            }
            else
            {
                Console.WriteLine($":: {ANDROID_APP_BUNDLE} env var detected but the value \"{value}\" is not a boolean.");
            }
#else
            Console.WriteLine($":: {ANDROID_APP_BUNDLE} env var detected but does not work with lower Unity version than 2018.3");
#endif
        }
    }

    /// <summary>
    /// Handles the Android bundle version code.
    /// </summary>
    private static void HandleAndroidBundleVersionCode()
    {
        if (TryGetEnv(ANDROID_BUNDLE_VERSION_CODE, out string value))
        {
            if (int.TryParse(value, out int version))
            {
                PlayerSettings.Android.bundleVersionCode = version;
                Console.WriteLine($":: {ANDROID_BUNDLE_VERSION_CODE} env var detected, set the bundle version code to {value}.");
            }
            else
                Console.WriteLine($":: {ANDROID_BUNDLE_VERSION_CODE} env var detected but the version value \"{value}\" is not an integer.");
        }
    }

    /// <summary>
    /// Method for getting the iOS version.
    /// </summary>
    /// <returns>string denoting the iOS version</returns>
    /// <exception cref="ArgumentNullException">exception to be thrown if the version could not be extracted</exception>
    private static string GetIosVersion()
    {
        if (TryGetEnv(VERSION_iOS, out string value))
        {
            if (int.TryParse(value, out int version))
            {
                Console.WriteLine($":: {VERSION_iOS} env var detected, set the version to {value}.");
                return version.ToString();
            }
            else
                Console.WriteLine($":: {VERSION_iOS} env var detected but the version value \"{value}\" is not an integer.");
        }

        throw new ArgumentNullException(nameof(value), $":: Error finding {VERSION_iOS} env var");
    }

    /// <summary>
    /// Handles the Android keystore.
    /// </summary>
    private static void HandleAndroidKeystore()
    {
#if UNITY_2019_1_OR_NEWER
        PlayerSettings.Android.useCustomKeystore = false;
#endif

        if (!File.Exists(KEYSTORE)) {
            Console.WriteLine($":: {KEYSTORE} not found, skipping setup, using Unity's default keystore");
            return;    
        }

        PlayerSettings.Android.keystoreName = KEYSTORE;

        string keystorePass;
        string keystoreAliasPass;

        if (TryGetEnv(KEY_ALIAS_NAME, out string keyaliasName)) {
            PlayerSettings.Android.keyaliasName = keyaliasName;
            Console.WriteLine($":: using ${KEY_ALIAS_NAME} env var on PlayerSettings");
        } else {
            Console.WriteLine($":: ${KEY_ALIAS_NAME} env var not set, using Project's PlayerSettings");
        }

        if (!TryGetEnv(KEYSTORE_PASS, out keystorePass)) {
            Console.WriteLine($":: ${KEYSTORE_PASS} env var not set, skipping setup, using Unity's default keystore");
            return;
        }

        if (!TryGetEnv(KEY_ALIAS_PASS, out keystoreAliasPass)) {
            Console.WriteLine($":: ${KEY_ALIAS_PASS} env var not set, skipping setup, using Unity's default keystore");
            return;
        }
#if UNITY_2019_1_OR_NEWER
        PlayerSettings.Android.useCustomKeystore = true;
#endif
        PlayerSettings.Android.keystorePass = keystorePass;
        PlayerSettings.Android.keyaliasPass = keystoreAliasPass;
    }
}
