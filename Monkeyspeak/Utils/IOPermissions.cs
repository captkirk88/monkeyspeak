using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Utils
{
    public static class IOPermissions
    {
        private static WindowsIdentity _currentUser;
        private static WindowsPrincipal _currentPrincipal;

        static IOPermissions()
        {
            _currentUser = WindowsIdentity.GetCurrent();
            _currentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        }

        public static bool HasAccess(string directory)
        {
            if (directory == null) return false;
            if (!Directory.Exists(directory)) return false;
            // Get the collection of authorization rules that apply to the directory.
            AuthorizationRuleCollection acl = new DirectoryInfo(directory).GetAccessControl()
                .GetAccessRules(true, true, typeof(SecurityIdentifier));
            return HasFileOrDirectoryAccess(acl);
        }

        public static bool HasAccess(DirectoryInfo directory)
        {
            if (directory == null) return false;
            // Get the collection of authorization rules that apply to the directory.
            AuthorizationRuleCollection acl = directory.GetAccessControl()
                .GetAccessRules(true, true, typeof(SecurityIdentifier));
            return HasFileOrDirectoryAccess(acl);
        }

        public static bool HasAccess(FileInfo file)
        {
            if (file == null) return false;
            // Get the collection of authorization rules that apply to the file.
            AuthorizationRuleCollection acl = file.GetAccessControl()
                .GetAccessRules(true, true, typeof(SecurityIdentifier));
            return HasFileOrDirectoryAccess(acl);
        }

        private static bool HasFileOrDirectoryAccess(AuthorizationRuleCollection acl)
        {
            for (int i = 0; i < acl.Count; i++)
            {
                var currentRule = (FileSystemAccessRule)acl[i];
                // If the current rule applies to the current user.
                if (_currentUser.User.Equals(currentRule.IdentityReference) ||
                    _currentPrincipal.IsInRole(
                                    (SecurityIdentifier)currentRule.IdentityReference))
                {
                    if (currentRule.AccessControlType.Equals(AccessControlType.Deny))
                    {
                        return false;
                    }
                    else if (currentRule.AccessControlType
                                                    .Equals(AccessControlType.Allow))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}