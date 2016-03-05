﻿#region Blobber!
// Blobber - Merges or embed referenced assemblies
// https://github.com/picrap/Blobber
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace Blobber
{
    using System.Diagnostics;
    using System.IO;
    using StitcherBoy.Project;
    using WildcardMatch;

    [DebuggerDisplay("{Literal}")]
    public class BlobDirective
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="BlobDirective"/> works on private assemblies (copy to local).
        /// </summary>
        /// <value>
        ///   <c>true</c> if private; otherwise, <c>false</c>.
        /// </value>
        public bool Private { get; }

        /// <summary>
        /// Gets the name if assemblies to be matched (wildcard works).
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public BlobAction Action { get; }

        /// <summary>
        /// Gets the literal.
        /// </summary>
        /// <value>
        /// The literal.
        /// </value>
        public string Literal => $"{(Private ? "-" : "+")}{Name}: {Action}";

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobDirective"/> class.
        /// </summary>
        /// <param name="private">if set to <c>true</c> [private].</param>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        public BlobDirective(bool @private, string name, BlobAction action)
        {
            Private = @private;
            Name = name;
            Action = action;
        }

        /// <summary>
        /// If this directive matches the specified reference, then an action is returned.
        /// </summary>
        /// <param name="assemblyReference">The assembly reference.</param>
        /// <returns></returns>
        public BlobAction? Matches(AssemblyReference assemblyReference)
        {
            if (assemblyReference.IsPrivate != Private)
                return null;
            var assemblyFileName = Path.GetFileName(assemblyReference.Path);
            if (Name.WildcardMatch(assemblyFileName))
                return Action;
            return null;
        }
    }
}