﻿#region Blobber!
// Blobber - Merges or embed referenced assemblies
// https://github.com/picrap/Blobber
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace Blobber
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using Microsoft.Build.Evaluation;
    using StitcherBoy.Project;
    using StitcherBoy.Weaving;

    public partial class BlobberStitcher : SingleStitcher
    {
        protected override bool Process(StitcherContext context)
        {
            bool processed = false;
            var directives = LoadDirectives(context);
            foreach (var reference in context.Project.References)
            {
                var action = GetAction(reference, directives);
                switch (action)
                {
                    case BlobAction.Embed:
                        Embed(context.Module, reference);
                        processed = true;
                        break;
                    case BlobAction.Merge:
                        Merge(context.Module, reference);
                        processed = true;
                        break;
                }
            }

            if (processed)
                EmbedLoader(context.Module, context.TaskAssemblyPath);
            return processed;
        }

        private void EmbedLoader(ModuleDefMD2 moduleDef, string taskAssemblyPath)
        {
            var assemblyLoaderTypeName = typeof(Loader).FullName;
            // import Loader type from this assembly
            var thisModuleDef = ModuleDefMD.Load(taskAssemblyPath);
            var loaderType = thisModuleDef.Find(assemblyLoaderTypeName, true);
            thisModuleDef.Types.Remove(loaderType);
            loaderType.Name = "⌂";
            loaderType.Namespace = null;
            moduleDef.Types.Add(loaderType);
            // ensure it is called from module cctor
            var moduleType = moduleDef.Find("<Module>", true);
            var cctor = moduleType.FindOrCreateStaticConstructor();
            var loaderInitializeMethod = loaderType.FindMethod(nameof(Loader.Initialize));
            cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, moduleDef.Import(loaderInitializeMethod)));
        }


        /// <summary>
        /// Gets the action.
        /// </summary>
        /// <param name="assemblyReference">The assembly reference.</param>
        /// <param name="directives">The directives.</param>
        /// <returns></returns>
        private static BlobAction GetAction(AssemblyReference assemblyReference, IList<BlobDirective> directives)
        {
            var action = BlobAction.None;
            foreach (var directive in directives)
            {
                var directiveAction = directive.Matches(assemblyReference);
                if (directiveAction.HasValue)
                    action = directiveAction.Value;
            }
            return action;
        }

        private IList<BlobDirective> LoadDirectives(StitcherContext context)
        {
            var directivesFile = context.Project.Project.Items.SingleOrDefault(i => string.Equals(i.EvaluatedInclude, "Blobber", StringComparison.OrdinalIgnoreCase));
            var directives = LoadDirectives(Path.GetDirectoryName(context.ProjectPath), directivesFile);
            return directives;
        }

        private IList<BlobDirective> LoadDirectives(string projectDirectory, ProjectItem item)
        {
            var directives = new List<BlobDirective>();
            directives.Add(new BlobDirective(true, "*", BlobAction.Embed));
            if (item != null)
            {
                var itemPath = Path.Combine(projectDirectory, item.EvaluatedInclude);
                using (var itemReader = File.OpenText(itemPath))
                {
                    for (;;)
                    {
                        var line = itemReader.ReadLine();
                        if (line == null)
                            break;
                        if (line.StartsWith("#"))
                            continue;

                        var parts = line.Split(new[] { ':' }, 2);
                        if (parts.Length == 1)
                            continue;

                        bool isPrivate = true;
                        var name = parts[0].Trim();
                        if (name.StartsWith("+"))
                        {
                            isPrivate = false;
                            name = name.Substring(1);
                        }
                        else if (name.StartsWith("-"))
                        {
                            isPrivate = false;
                            name = name.Substring(1);
                        }
                        var action = Parse(parts[1].Trim());
                        if (!action.HasValue)
                        {
                            Logging.WriteWarning("Error in Blobber line: {0}", line);
                            continue;
                        }
                        directives.Add(new BlobDirective(isPrivate, name, action.Value));
                    }
                }
            }
            return directives;
        }

        private static BlobAction? Parse(string literal)
        {
            BlobAction action;
            if (Enum.TryParse(literal, true, out action))
                return action;
            return null;
        }
    }
}
