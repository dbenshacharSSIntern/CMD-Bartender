using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barrista.Models
{
   public class User
   {
      /// <summary>
      /// The user's id.
      /// </summary>
      public string Id { get; set; } = String.Empty;

      /// <summary>
      /// The user's display name.
      /// </summary>
      public string DisplayName { get; set; } = String.Empty;
   }
}
