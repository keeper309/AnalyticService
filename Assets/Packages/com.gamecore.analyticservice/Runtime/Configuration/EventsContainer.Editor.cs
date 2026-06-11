using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameCore.AnalyticService
{
    public partial class EventsContainer
    {
#if UNITY_EDITOR

        private void SaveChanges()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static IEnumerable<Type> GetNonAbstractDerivedTypes(Type interfaceType)
        {
            IEnumerable<Type> allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => interfaceType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

            return allTypes;
        }

        //todo: inspector button
        private void CreateCategory()
        {
            string destination = EditorUtility.SaveFilePanelInProject(
                "Create Category",
                "ParametersCategory",
                "asset",
                ""
            );

            ParametersCategory category = CreateInstance<ParametersCategory>();
            if (!string.IsNullOrEmpty(destination))
            {
                AssetDatabase.CreateAsset(category, destination);

                categories.Add(category);
                SaveChanges();
            }
            else
            {
                DestroyImmediate(category);
            }
        }

        //todo: inspector button
        private void AddAll()
        {
            usedEvents = eventProfiles.Select(p => new EventReference(p.EventId)).ToList();
            SaveChanges();
        }

        //todo: inspector button
        private void RemoveAll()
        {
            usedEvents.Clear();
            SaveChanges();
        }

        [SerializeField]
        private TextAsset eventsConstantsFileAsset;

        [SerializeField]
        private TextAsset funnelsConstantsFileAsset;

        //todo: inspector button
        private void GenerateEventIdsConstants()
        {
            string eventsConstantsFilePath = AssetDatabase.GetAssetPath(eventsConstantsFileAsset);
            string path = GenerateConstantsClass(
                eventsConstantsFilePath,
                "EventsIds",
                eventsConstantsFileAsset,
                eventProfiles.Select(p => p.EventId).ToArray()
            );

            if (eventsConstantsFileAsset != null)
                return;
            eventsConstantsFileAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        //todo: inspector button
        private void GenerateFunnelIdsConstants()
        {
            string funnelsConstantsFilePath = AssetDatabase.GetAssetPath(funnelsConstantsFileAsset);
            string path = GenerateConstantsClass(
                funnelsConstantsFilePath,
                "FunnelsIds",
                funnelsConstantsFileAsset,
                funnelProfiles.Select(p => p.funnelId).ToArray()
            );

            if (funnelsConstantsFileAsset != null)
                return;
            funnelsConstantsFileAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static string GenerateConstantsClass(
            string path,
            string className,
            TextAsset asset,
            IEnumerable<string> constants
        )
        {
            ConstantsClassCodeGenerator classCodeGenerator = new();
            string code = classCodeGenerator.Generate(className, constants);

            if (string.IsNullOrEmpty(path) || !asset)
            {
                path = EditorUtility.SaveFilePanelInProject("Create Constants Class", className, "cs", "");
                if (string.IsNullOrEmpty(path))
                {
                    return path;
                }
            }

            File.WriteAllText(path, code);
            AssetDatabase.ImportAsset(path);

            return path;
        }

        //todo: inspector button
        private void ImportEvents()
        {
            string path = EditorUtility.OpenFilePanel("Import Events", "", "zip");
            if (!string.IsNullOrEmpty(path))
            {
                ZipEventsImporter importer = new();
                ZipEventsContainer container = importer.Import(path);
                ImportEvents(container);
            }
        }

        private void ImportEvents(ZipEventsContainer container)
        {
            usedEvents.Clear();
            eventProfiles.Clear();
            funnelProfiles.Clear();

            foreach (ZipEventsContainer.ZipEvent @event in container.Events.Values)
            {
                string[] allParameters = container.Parameters.TryGetValue(
                    @event.Id,
                    out ZipEventsContainer.ZipParameters p
                )
                    ? p.Parameters.ToArray()
                    : null;

                string[] op = allParameters?.Where(container.ObservableParameters.Contains).ToArray();

                string[] parameters = allParameters?.Except(op).ToArray();

                Parameter[] observableParameters = op?.Select(parameterId => new Parameter(parameterId)).ToArray();

                AnalyticsProviderId[] providers = GetAvailableProviders().Select(id => new AnalyticsProviderId(id)).ToArray();

                string[] attachedEvents = @event.AttachedEvents.ToArray();

                Dictionary<string, string> customAttributes = new()
                {
                    { "adjust-token", @event.AdjustToken }
                };

                eventProfiles.Add(
                    new EventProfile(
                        @event.Id,
                        @event.Type,
                        providers,
                        observableParameters,
                        null,
                        parameters,
                        customAttributes,
                        attachedEvents
                    )
                );
            }

            foreach (string funnel in container.Funnels)
            {
                string[] steps = container.FunnelSteps.TryGetValue(funnel, out ZipEventsContainer.ZipFunnelSteps s)
                    ? s.Steps.ToArray()
                    : null;

                string[] indexes = container.FunnelSteps.TryGetValue(funnel, out ZipEventsContainer.ZipFunnelSteps i)
                    ? i.Indexes.ToArray()
                    : null;

                funnelProfiles.Add(new FunnelProfile(funnel, steps, indexes));
            }
            SaveChanges();
            ValidateImplementation();
        }

        //todo: inspector button
        private void ImportDefaultEvents()
        {
            string guid = AssetDatabase.FindAssets("DefaultEventsZipContainer").FirstOrDefault();
            string path = AssetDatabase.GUIDToAssetPath(guid);

            ZipEventsImporter importer = new();
            ZipEventsContainer container = importer.Import(path);
            ImportEvents(container);
        }

#endif
    }
}
