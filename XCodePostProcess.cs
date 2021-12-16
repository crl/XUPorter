using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.XCodeEditor;
#endif
using System.IO;

public static class XCodePostProcess
{

#if UNITY_EDITOR
	[PostProcessBuild(999)]
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
		if (target != BuildTarget.iOS) {
			Debug.LogWarning("Target is not iPhone. XCodePostProcess will not run");
			return;
		}
		//得到xcode工程的路径
		string path = Path.GetFullPath(pathToBuiltProject);

		// Create a new project object from build target
		XCProject project = new XCProject( pathToBuiltProject );

		// Find and run through all projmods files to patch the project.
		// Please pay attention that ALL projmods files in your project folder will be excuted!
		//在这里面把frameworks添加在你的xcode工程里面
		string[] files = Directory.GetFiles( Application.dataPath, "*.projmods", SearchOption.AllDirectories );
		foreach( string file in files ) {
			UnityEngine.Debug.Log("ProjMod File: "+file);
			project.ApplyMod( file );
		}

		//增加一个编译标记。。没有的话sharesdk会报错。。
		// project.AddOtherLinkerFlags("-licucore");
		project.AddOtherLinkerFlags("-ObjC");

		//TODO implement generic settings as a module option
		project.overwriteBuildSetting("CODE_SIGN_IDENTITY[sdk=iphoneos*]", "iPhone Distribution", "Release");

		project.overwriteBuildSetting("ENABLE_BITCODE", "NO", "Release");
		project.overwriteBuildSetting("ENABLE_BITCODE", "NO", "Debug");


		EditorPlist(path);

		//编辑代码文件
		EditorCode(path);

		// Finally save the xcode project
		project.Save();

	}

	private static void EditorPlist(string filePath)
	{
		XCPlist list = new XCPlist(filePath);
		string bundle = "com.sdo.qyn";

		string PlistAdd = @"<key>NSAppTransportSecurity</key>
                            <dict>
                                <key>NSAllowsArbitraryLoads</key>
                                <true/>
                            </dict>";

		//在plist里面增加一行
		list.AddKey(PlistAdd);

		string _temp = @"<key>URL types</key>
						 <array>
							 <dict>
								 <key>CFBundleURLName</key>
								 <string>com.sdo.qyn</string>
								 <key>CFBundleURLSchemes</key>
								 <array>
								 <string>123</string>
								 </array>
							 </dict>
							 <dict>
								 <key>CFBundleURLSchemes</key>
								 <array>
								 <string>123</string>
								 </array>
							 </dict>
							 <dict>
								 <key>CFBundleURLSchemes</key>
								 <array>
								 <string>123</string>
								 </array>
							 </dict>
							 <dict>
								 <key>CFBundleURLSchemes</key>
								 <array>
								 <string>123</string>
								 </array>
							 </dict>
							 <dict>
								 <key>CFBundleURLSchemes</key>
								 <array>
								 <string>123</string>
								 </array>
							 </dict>
						 </array>";

		list.AddKey(_temp);

		string _temp2 = @"<key>LSApplicationQueriesSchemes</key>
						  <array>
							  <string>weixin</string>
							  <string>weichat</string>
							  <string>sinaweibohd</string>
							  <string>sinaweibo</string>
							  <string>sinaweibosso</string>
							  <string>sinaweibohdsso</string>
							  <string>weibosdk</string>
							  <string>weibosdk2.5</string>
							  <string>yixin</string>
							  <string>yixinfav</string>
							  <string>yixinoauth</string>
							  <string>yixinopenapi</string>
						  </array>";

		list.AddKey(_temp2);

		//在plist里面替换一行
		list.ReplaceKey("<string>com.sdo.${PRODUCT_NAME}</string>","<string>"+bundle+"</string>");

		//保存
		list.Save();
	}

	private static void EditorCode(string filePath)
	{
		//读取UnityAppController.mm文件
		XClass UnityAppController = new XClass(filePath + "/Classes/UnityAppController.mm");

		//在指定代码后面增加一行代码
		UnityAppController.WriteBelow("#include \"PluginBase/AppDelegateListener.h\"", "#import <ShareSDK/ShareSDK.h>");

		//在指定代码中替换一行
		UnityAppController.Replace("return YES;", "return [ShareSDK handleOpenURL:url sourceApplication:sourceApplication annotation:annotation wxDelegate:nil];");

		//在指定代码后面增加一行
		UnityAppController.WriteBelow("UnityCleanup();\n}", "- (BOOL)application:(UIApplication *)application handleOpenURL:(NSURL *)url\r{\r    return [ShareSDK handleOpenURL:url wxDelegate:nil];\r}");
	}
#endif

	public static void Log(string message)
	{
		UnityEngine.Debug.Log("PostProcess: "+message);
	}
}
