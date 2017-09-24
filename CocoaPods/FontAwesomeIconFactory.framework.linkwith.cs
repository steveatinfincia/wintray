
using ObjCRuntime;

[assembly: LinkWith("FontAwesomeIconFactory.framework",
    target: LinkTarget.x86_64,
    SmartLink = true,
    ForceLoad = true,
    LinkerFlags = "-ObjC",
Frameworks = "Foundation CoreGraphics QuartzCore Cocoa")]
