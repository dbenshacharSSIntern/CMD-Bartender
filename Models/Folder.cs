using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barrista.Models
{
    public class Folder
    {
        /// <summary>
        /// Specifies the ID of the folder's space.
        /// </summary>
        public int SpaceId { get; set; }
        /// <summary>
        /// Specifies the folder's ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Specifies the folder's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the folder's path.
        /// </summary>
        public string UncPath { get; set; }

        /// <summary>
        /// True if the folder is locked for use; otherwise, false.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Specifies the date and time when the folder was created.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Specifies the name of the user who created the folder.
        /// </summary>
        public User CreatedBy { get; set; }

        /// <summary>
        /// Specifies the folder's metadata collection.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// True if inheriting permissions from the parent folder; otherwise, false.
        /// </summary>
        public bool InheritPermissionsFromParent { get; set; }

        /// <summary>
        /// True if the folder is marked for deletion; otherwise, false.
        /// Note: A Purge action will delete the folder permanently.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Specifies the ID of the folder's parent folder.
        /// </summary>
        public string ParentFolderId { get; set; }

        /// <summary>
        /// True if a folder is hidden; otherwise, false.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Specifies the folder's subfolders.
        /// </summary>
        public List<Folder> Folders { get; set; } = new List<Folder>();
    }
    public class FolderCreateRequestBase
    {
        /// <summary>
        /// True if the folder inherits permissions from the parent folder; otherwise, false.
        /// </summary>
        public bool InheritPermissionsFromParent { get; set; } = true;

        /// <summary>
        /// Specifies the new folder's metadata collection.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Specifies the new folder's permissions collection.
        /// </summary>
        public List<FolderPermissionWithIdentities> Permissions { get; set; }
    }


    public class FolderCreateRequest : FolderCreateRequestBase
    {
        /// <summary>
        /// Specifies the new folder's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the ID of the new folder's parent folder.
        /// </summary>
        public string ParentFolderId { get; set; }

        /// <summary>
        /// True if the folder is hidden; otherwise, false.
        /// </summary>
        public bool IsHidden { get; set; }
    }
    public class FolderPermissionWithIdentities
    {
        /// <summary>
        /// Specifies a folder permission.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public FolderPermission Permission { get; set; }

        /// <summary>
        /// Specifies the collection of the folder permission identities.
        /// </summary>
        public Dictionary<IdentityType, List<Identity>> GrantedTo { get; set; }
    }

    public enum FolderPermission
    {
        Uninitialized = 0,
        Read,
        Write,
        Delete,
        List,
        Purge
    }
    public class Identity
    {
        /// <summary>
        /// Specifies the ID of the identity type that is being referenced, such as UserID, GroupID, or RoleID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Specifies the display name for the identity. This parameter is primarily used to provide context if
        /// a permission-editing UI is displayed.
        /// </summary>
        public string DisplayName { get; set; }
    }
    public enum IdentityType
    {
        Uninitialized = 0,
        User,
        Group,
        Role
    }
    public class FolderHierarchy
    {
        /// <summary>
        /// Specifies the folder's subfolders.
        /// </summary>
        public List<FolderMin> Folders { get; set; } = new List<FolderMin>();
    }

    public class FolderMin
    {
        /// <summary>
        /// Specifies the folder's ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Specifies the folder's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the folder's path.
        /// </summary>
        public string UncPath { get; set; }

        public bool IsHidden { get; set; }
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Specifies the folder's subfolders.
        /// </summary>
        public List<FolderMin> Folders { get; set; } = new List<FolderMin>();

        /// <summary>
        /// Provides a utility method to allow for enumeration over the folder hierarchy.
        /// </summary>
        public static IEnumerable<FolderMin> Traverse(FolderMin root)
        {
            var stack = new Stack<FolderMin>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                foreach (var child in current.Folders)
                    stack.Push(child);
            }
        }
    }

    public class FolderNoChildren
    {
        /// <summary>
        /// Specifies the ID of the folder's space.
        /// </summary>
        public int SpaceId { get; set; }

        /// <summary>
        /// Specifies the folder's ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Specifies the folder's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the folder's path.
        /// </summary>
        public string UncPath { get; set; }

        /// <summary>
        /// True if the folder is locked for use; otherwise, false.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Specifies the date and time when the folder was created.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Specifies the name of the user who created the folder.
        /// </summary>
        public User CreatedBy { get; set; }

        /// <summary>
        /// Specifies the folder's metadata collection.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// True if inheriting permissions from the parent folder; otherwise, false.
        /// </summary>
        public bool InheritPermissionsFromParent { get; set; }

        /// <summary>
        /// True if the folder is marked for deletion; otherwise, false.
        /// Note: A Purge action will delete the folder permanently.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Specifies the ID of the folder's parent folder.
        /// </summary>
        public string ParentFolderId { get; set; }
    }
}
