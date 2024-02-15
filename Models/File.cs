using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Barrista.Models;
namespace Barrista.Models
{
    public class File : AbstractItemExtended
    {
        /// <summary>
           /// Specifies the ID and version of a file to use for indexing and retrieval.
           /// </summary>
        public string IdWithVersion { get; set; }

        /// <summary>
           /// Specifies the file's workflow status.
           /// </summary>
        public string FileStatus { get; set; }

        /// <summary>
           /// Specifies the file's content type (such as .txt, .jpg, and so on).
           /// </summary>
        public string FileContentType { get; set; }

        /// <summary>
           /// Specifies the file's encryption type and level.
           /// Note: This parameter is only a label. All encryption and decryption is the responsibility of the client.
           /// </summary>
        public string Encryption { get; set; }

        /// <summary>
           /// True if the file is inheriting from the folder permissions; otherwise, false.
           /// </summary>
        public bool InheritFromFolderPermissions { get; set; }

        /// <summary>
           /// Specifies the file's permissions collection.
           /// </summary>
        public List<FilePermissionWithIdentities> Permissions { get; set; }

        /// <summary>
           /// Specifies a friendly version description.
           /// </summary>
        public string VersionDescription { get; set; }

        /// <summary>
           /// Specifies the file's attachments collection.
           /// </summary>
        public List<FileAttachment> Attachments { get; set; } = new List<FileAttachment>();
    }
    public class AbstractItemExtended : AbstractItem
    {
        /// <summary>
           /// Specifies the item's space ID.
           /// </summary>
        public int SpaceId { get; set; }

        /// <summary>
           /// True if a file is locked; otherwise, false.
           /// Files can be locked only when a user performs a checkout operation.
           /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
           /// True if a file is hidden; otherwise, false.
           /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
           /// True if a file is a temporary file; otherwise, false.
           /// </summary>
        public bool IsTemporary { get; set; }

        /// <summary>
           /// Specifies the ID of the user who locked the file.
           /// </summary>
        public string LockedByUser { get; set; }

        /// <summary>
           /// Specifies the file's checksum.
           /// </summary>
        public string Checksum { get; set; }

        /// <summary>
           /// Specifies the file's version.
           /// </summary>
        public Version Version { get; set; }

        /// <summary>
           /// Specifies the file's published version.
           /// </summary>
        /// 

        public Version PublishedVersion { get; set; }

        /// <summary>
           /// True if the file is published; otherwise, false.
           /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
           /// Specifies the date and time when the file was last modified. 
           /// </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
           /// Specifies the date and time when the file was published.
           /// </summary>
        public DateTime? PublishedDateTime { get; set; }

        /// <summary>
           /// Specifies the ID of the user who last modified the file.
           /// </summary>
        public User ModifiedBy { get; set; }

        /// <summary>
           /// Specifies the ID of the user who published the file.
           /// </summary>
        public string PublishedBy { get; set; }

        /// <summary>
           /// Specifies the ID of the file's workflow state.
           /// </summary>
        public Guid WorkflowStateId { get; set; }

        /// <summary>
           /// Specifies the ID of the folder that contains the file. 
           /// </summary>
        public string FolderId { get; set; }

        /// <summary>
           /// Specifies the file's size in bytes.
           /// </summary>
        public long Size { get; set; }
    }
    public class FileAttachment
    {
        /// <summary>
           /// Specifies the ID of the file's attachment.
           /// </summary>
        public string AttachmentId { get; set; }

        /// <summary>
           /// Specifies the name of the file's attachment.
           /// </summary>
        public string AttachmentName { get; set; }

        /// <summary>
           /// Specifies the attachment's content type (such as .txt, .jpg, .btw, and so on). 
           /// Use this parameter to help users understand how the attachment can be processed.
           /// </summary>
        public string ContentType { get; set; }

        /// <summary>
           /// Specifies the attachment's size in bytes.
           /// </summary>
        public long Size { get; set; }
    }
    public class FilePermissionWithIdentities
    {
        /// <summary>
            /// Specifies a file permission.
            /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public FilePermission Permission { get; set; }

        /// <summary>
            /// Specifies the file permission's identities.
            /// </summary>
        public Dictionary<IdentityType, List<Identity>> GrantedTo { get; set; }
    }
    public class AbstractItem : AbstractItemBase
    {
        /// <summary>
           /// Specifies the date and time when the item was created. 
           /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
           /// Specifies the ID of the user who created the item.
           /// </summary>
        public User CreatedBy { get; set; }

        /// <summary>
           /// Specifies the item's metadata collection.
           /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
           /// True if the item is marked for deletion; otherwise, false.
           /// The item is deleted after a purge action is initiated.
           /// </summary>
        public bool IsDeleted { get; set; }
    }
    public enum FilePermission
    {
        Uninitialized = 0,
        Read,
        Write,
        Delete,
        Purge
    }
    public class AbstractItemBase
    {
        /// <summary>
           /// Specifies the item's ID. 
           /// </summary>
        public string ItemId { get; set; }

        /// <summary>
           /// Specifies the item's tenant ID. 
           /// </summary>
        public string TenantId { get; set; }

        /// <summary>
           /// Specifies the item's name.
           /// </summary>
        public string Name { get; set; }

        /// <summary>
           /// Specifies the item's type.
           /// </summary>
        public virtual string Type { get; set; }
    }

    public class FileAddRequest : FileAddRequestBase
    {
        /// <summary>
        /// Specifies the file's name.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Specifies the ID of the file's parent folder.
        /// </summary>
        public string FolderId { get; set; }
    }
    public class FileAddRequestBase
    {
        /// <summary>
        /// Specifies a comment for the file.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Specifies the file's content type (such as .txt, .jpg, and so on).
        /// </summary>
        public string FileContentType { get; set; }

        /// <summary>
        /// Specifies the file's encryption type and level.
        /// Note: This parameter is only a label. All encryption and decryption is the responsibility of the client.
        /// </summary>
        public string Encryption { get; set; }

        /// <summary>
        /// True if inheriting permissions from the folder permissions. If InheritFromFolderPermissions
        /// is true, the contents of the Permissions property are ignored.
        /// </summary>
        public bool InheritFromFolderPermissions { get; set; } = true;

        /// <summary>
        /// Specifies the file's metadata collection.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Specifies the file's permissions collection.
        /// </summary>
        public List<FilePermissionWithIdentities> Permissions { get; set; }

        /// <summary>
        /// True if the file should be hidden. When a file is hidden, it is excluded from
        /// query responses unless it is specifically requested.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Specifies a friendly version description.
        /// </summary>
        public string VersionDescription { get; set; }
    }
    public class FileChange
    {
        /// <summary>
        /// Specifies the ID of the file change history.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Specifies the ID of the file's space.
        /// </summary>
        public int SpaceId { get; set; }

        /// <summary>
        /// Specifies the file's ID.
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Specifies the ID of the file's parent folder.
        /// </summary>
        public string FolderId { get; set; }

        /// <summary>
        /// Specifies the file's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the date and time when the file was created.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Specifies the name of the user who created the file.
        /// </summary>
        public User CreatedBy { get; set; }

        /// <summary>
        /// Specifies the file's metadata collection.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// True if the file is locked for checkout; otherwise, false.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// True if a file is hidden; otherwise, false.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// True if a file is a temporary file; otherwise, false.
        /// </summary>
        public bool IsTemporary { get; set; }

        /// <summary>
        /// Specifies the name of the user who locked the file for checkout.
        /// </summary>
        public User LockedByUser { get; set; }

        /// <summary>
        /// Specifies the file's version.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Specifies the file's published version.
        /// </summary>
        public Version PublishedVersion { get; set; }

        /// <summary>
        /// Specifies the date and time when the file was modified.
        /// </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Specifies the date and time when the file was published.
        /// </summary>
        public DateTime? PublishedDateTime { get; set; }

        /// <summary>
        /// Specifies the file's file change action.
        /// </summary>
        public FileChangeAction Action { get; set; }

        /// <summary>
        /// Specifies the ID of the user who made the change to the file.
        /// </summary>
        public string ActionUserId { get; set; }

        /// <summary>
        /// Specifies a comment for the file.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Specifies the name of the user who last modified the file.
        /// </summary>
        public User ModifiedBy { get; set; }

        /// <summary>
        /// Specifies the name of the user who published the file.
        /// </summary>
        public User PublishedBy { get; set; }

        /// <summary>
        /// Specifies the file's size.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// True if the file is marked for deletion; otherwise, false.
        /// The file can permanently be deleted by using the Purge action.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Specifies the ID of the file's workflow state.
        /// </summary>
        public Guid StateId { get; set; }

        /// <summary>
        /// Specifies the name of the file's workflow state.
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// Specifies the ID of the file's workflow.
        /// </summary>
        public Guid WorkflowId { get; set; }

        /// <summary>
        /// Specifies the name of the file's workflow.
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// True if inheriting permissions from the folder permissions; otherwise, false.
        /// </summary>
        public bool InheritFromFolderPermissions { get; set; }

        /// <summary>
        /// Specifies a friendly version description.
        /// </summary>
        public string VersionDescription { get; set; }

        /// <summary>
        /// Specifies the file's attachments collection.
        /// </summary>
        public List<FileAttachment> Attachments { get; set; }

        /// <summary>
        /// Specifies the file's checksum at this history point.
        /// </summary>
        public string Checksum { get; set; }
    }
    public enum FileChangeAction
    {
        Uninitialized = 0,
        Add,
        Rename,
        Move,
        Copy,
        Delete,
        Undelete,
        CheckOut,
        CheckIn,
        Revert,
        AddComment,
        EditComment,
        AddMetadata,
        UpdateMetadata,
        DeleteMetadata,
        DeleteAllMetadata,
        ChangeState,
        Transition,
        Publish
    }
    public class Version
    {
        public int Major { get; set; }
        public int Minor { get; set; }
    }
}
