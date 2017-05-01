using System;
using ObjCRuntime;

[assembly: LinkWith ("libDirectoryWatcher.a", LinkTarget.Simulator | LinkTarget.Arm64, ForceLoad = true, SmartLink=true)]
