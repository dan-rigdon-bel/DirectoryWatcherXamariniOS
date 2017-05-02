using System;
using ObjCRuntime;

[assembly: LinkWith ("libDirectoryWatcher.a", LinkTarget.Simulator64 | LinkTarget.Arm64, ForceLoad = true, SmartLink=true)]
