
using ObjCRuntime;

[assembly: LinkWith("LetsMove.framework",
    target: LinkTarget.x86_64,
    SmartLink = true,
    ForceLoad = true,
    LinkerFlags = "-ObjC",
Frameworks = "Foundation CoreGraphics QuartzCore Cocoa")]
