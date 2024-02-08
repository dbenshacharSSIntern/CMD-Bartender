using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace BasicAuthLogon.Models
{
   public abstract class SpacePolicyBase
   {
      public bool EnabledForSpace { get; set; }
   }

   public class SpacePolicy : SpacePolicyBase
   {
      public bool Readonly { get; set; }
   }

   public class VersionContentSpacePolicy : SpacePolicy
   {
   }

   public class DeleteFilesOlderThanDaysCountSpacePolicy : SpacePolicy
   {
      public int DaysCount { get; set; } = 0;
   }

   public class Space
   {
      /// <summary>
      /// The space's id.
      /// </summary>
      public int SpaceId { get; set; }

      /// <summary>
      /// The space's name.
      /// </summary>
      public string Name { get; set; } = string.Empty;

      /// <summary>
      /// The space's friendly display name, usually used for UI.
      /// </summary>
      public string DisplayName { get; set; } = string.Empty;

      /// <summary>
      /// True if the space is locked for use, else false.
      /// </summary>
      public bool IsLocked { get; set; }

      /// <summary>
      /// True if a space is hidden, else false.
      /// </summary>
      public bool IsHidden { get; set; }

      /// <summary>
      /// The file's created date time.
      /// </summary>
      public DateTime? CreatedDateTime { get; set; }

      /// <summary>
      /// The user that created the space.
      /// </summary>
      public User? CreatedBy { get; set; }

      /// <summary>
      /// The space's root folder id.
      /// </summary>
      public string? RootFolderId { get; set; }

      /// <summary>
      /// The space's bytes used.
      /// </summary>
      public long BytesUsed { get; set; }

      /// <summary>
      /// The space's maximum bytes allowed.
      /// </summary>
      public long MaxBytes { get; set; }

      /// <summary>
      /// The space's metadata.
      /// </summary>
      public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

      /// <summary>
      /// True if the space enforces versioning, else false.
      /// </summary>
      public VersionContentSpacePolicy? VersionContent { get; set; }

      /// <summary>
      /// If value is greater than zero, then files should be deleted if older than the value count of days.
      /// </summary>
      public DeleteFilesOlderThanDaysCountSpacePolicy? DeleteFilesOlderThanDaysCount { get; set; }

      public Space()
      { }

      public Space(Space space)
      {
         SpaceId = space.SpaceId;
         Name = space.Name;
         DisplayName = space.DisplayName;
         IsLocked = space.IsLocked;
         CreatedDateTime = space.CreatedDateTime;
         CreatedBy = space.CreatedBy;
         RootFolderId = space.RootFolderId;
         Metadata = space.Metadata;
         VersionContent = space.VersionContent;
         DeleteFilesOlderThanDaysCount = space.DeleteFilesOlderThanDaysCount;
         MaxBytes = space.MaxBytes;
         BytesUsed = space.BytesUsed;
      }
   }

   public class SpacesCollection
   {
      public List<Space> Spaces { get; set; } = new List<Space>();

      public SpacesCollection()
      { }

      public SpacesCollection(IEnumerable<Space> spaces)
      {
         foreach (Space space in spaces)
         {
            Spaces.Add(new Space(space));
         }
      }
   }
}