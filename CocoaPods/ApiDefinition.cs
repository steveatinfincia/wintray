using System;

using AppKit;
using Foundation;
using ObjCRuntime;
using CoreGraphics;

namespace FontAwesomeIconFactory {
    using NIKImage = NSImage;
    using NIKEdgeInsets = NSEdgeInsets;
    using NIKColor = NSColor;

    [BaseType(typeof(NSImageView))]
    interface NIKFontAwesomeImageView {
        [Export("icon")]
        NIKFontAwesomeIcon Icon { get; set; }

        [Export("iconHex")]
        NSString IconHex { get; set; }
    }

    [BaseType(typeof(NSObject))]
    interface NIKFontAwesomeIconFactory {
        [Export("createImageForIcon:")]
        NIKImage CreateImageForIcon(NIKFontAwesomeIcon icon);

        [Static, Export("bevelButtonIconFactory")]
        NIKFontAwesomeIconFactory BevelButtonIconFactory();

        [Static, Export("bushButtonIconFactory")]
        NIKFontAwesomeIconFactory BushButtonIconFactory();

        [Static, Export("toolbarItemIconFactory")]
        NIKFontAwesomeIconFactory ToolbarItemIconFactory();
    }

    [BaseType(typeof(NSObject))]
    interface NIKFontAwesomePathFactory {
        [Export("createPathForIcon:height:maxWidth:")]
        CGPath CreatePathForIcon(NIKFontAwesomeIcon icon, float height, float width);
    }

    [BaseType(typeof(NSObject))]
    [Model]
    [Protocol]
    interface NIKFontAwesomeIconTraits {
        [Export("size")]
        float Size { get; set; }

        [Export("edgeInsets")]
        NIKEdgeInsets EdgeInsets { get; set; }

        [Export("padded")]
        bool Padded { get; }

        [Export("square")]
        bool Square { get; }

        [Export("colors")]
        NSArray<NIKColor> Colors { get; set; }

        [Export("strokeColor")]
        NIKColor StrokeColor { get; set; }

        [Export("strokeWidth")]
        float StrokeWidth { get; set; }

    }

    [BaseType(typeof(NSObject))]
    [Model]
    [Protocol]
    interface NIKFontAwesomePathRenderer {

        [Export("path")]
        CGPath Path { get; set; }

        [Export("offset")]
        CGPoint Offset { get; set; }

        [Export("colors")]
        NSArray<NIKColor> Colors { get; set; }

        [Export("strokeColor")]
        CGColor StrokeColor { get; set; }

        [Export("strokeWidth")]
        float StrokeWidth { get; set; }

        [Export("renderInContext:")]
        CGPath RenderInContext(CGContext context);
    }
}
