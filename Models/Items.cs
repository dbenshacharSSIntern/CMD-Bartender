using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barrista.Models
{
    public class Items
    {
        /// <summary>
        /// Specifies the item's folder.
        /// </summary>
        public FolderNoChildren Folder { get; set; }

        /// <summary>
        /// Specifies the item's files collection.
        /// </summary>
        public List<File> Files { get; set; } = new List<File>();

        /// <summary>
        /// Specifies the item's subfolders collection.
        /// </summary>
        public List<FolderNoChildren> Subfolders { get; set; } = new List<FolderNoChildren>();

        /// <summary>
        /// Specifies the next items request.
        /// </summary>
        public ItemsRequest NextItemsRequest { get; set; }

        /// <summary>
        /// True if there are more items to get; otherwise, false.
        /// </summary>
        public bool MoreItemsToGet { get; set; }
    }
    public class ItemsRequest
    {
        /// <summary>
        /// Specifies the folders limit value to use in the next GetItems request.
        /// </summary>
        public int? FoldersLimit { get; set; }

        /// <summary>
        /// Specifies the files limit value to use in the next GetItems request.
        /// </summary>
        public int? FilesLimit { get; set; }

        /// <summary>
        /// Specifies the folders skip value to use in the next GetItems request.
        /// </summary>
        public long FoldersSkip { get; set; }

        /// <summary>
        /// Specifies the files skip value to use in the next GetItems request.
        /// </summary>
        public long FilesSkip { get; set; }

        /// <summary>
        /// True if hidden folders and files should be included; otherwise, false.
        /// </summary>
        public bool IncludeHidden { get; set; }

        /// <summary>
        /// True if deleted folders and files should be included; otherwise, false.
        /// </summary>
        public bool IncludeDeleted { get; set; }
    }
}
