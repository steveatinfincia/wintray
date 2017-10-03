using System;
namespace Common {

    public interface Platform {
        bool IsJavaInstalled();
    }

    public partial class PlatformHelpers {
        public PlatformHelpers() {

        }
    }

    public partial class PlatformHelpers: Platform {
        public bool IsJavaInstalled() {
            return false;
        }
    }
}
