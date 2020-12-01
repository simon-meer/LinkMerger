using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Zomg.Editor
{
    [InitializeOnLoad]
    public class LinkMerger
    {
        private const string MARKER_TEXT = "AUTO-GENERATED";

        static LinkMerger()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(
                async buildPlayerOptions =>
                {
                    await PatchAll();
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
                });
        }

        private static async Task PatchAll()
        {
            // Collect link files in dependencies
            var linkFiles = await Collect();

            // Merge them into the project link.xml
            Merge(linkFiles);
        }

        private static async Task<List<(PackageInfo, string)>> Collect()
        {
            var request = Client.List(true, true);
            while (!request.IsCompleted)
            {
                await Task.Yield();
            }

            var result = new List<(PackageInfo, string)>();

            if (request.Status == StatusCode.Success)
            {
                foreach (var dependency in request.Result)
                {
                    if (dependency.status != PackageStatus.Available) continue;

                    var linkFile = Path.Combine(dependency.resolvedPath, "link.xml");
                    if (File.Exists(linkFile))
                    {
                        result.Add((dependency, linkFile));
                    }
                }
            }

            return result;
        }

        private static void Merge(List<(PackageInfo Package, string LinkFile)> linkFiles)
        {
            var baseFile = Path.Combine(Application.dataPath, "link.xml");

            // Initialize file if needed
            if (!File.Exists(baseFile))
            {
                File.WriteAllText(baseFile, "<linker></linker>");
            }

            try
            {
                var baseDocument = XDocument.Parse(File.ReadAllText(baseFile));
                var root = baseDocument.Root;

                var markerNode = root.Nodes().OfType<XComment>().FirstOrDefault(comment => comment.Value == MARKER_TEXT);
                if (markerNode != null)
                {
                    // Remove all existing elements
                    markerNode.NodesAfterSelf().ToList().ForEach(n => n.Remove());
                }
                else
                {
                    markerNode = new XComment(MARKER_TEXT);
                    root.Add(markerNode);
                }

                // Merge all link.xml in dependencies into the base link.xml
                foreach (var file in linkFiles)
                {
                    root.Add(new XComment(file.Package.name));
                    var subDocument = XDocument.Parse(File.ReadAllText(file.LinkFile));

                    root.Add(subDocument.Root.Nodes());
                }

                // Save changes
                baseDocument.Save(baseFile);
            }
            catch (Exception e)
            {
                Debug.LogError("Something went wrong while creating link.xml");
                Debug.LogException(e);
            }
        }
    }
}