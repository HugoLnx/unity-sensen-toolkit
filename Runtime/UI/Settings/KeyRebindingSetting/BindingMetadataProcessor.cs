using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    // public class BindingMetadata
    // {
    //     public InputBinding Binding;
    //     public BindingPathComponents Path;
    //     public bool IsComposite => Binding.isComposite;
    //     public List<BindingMetadata> CompositeParts;
    //     public BindingMetadata ParentComposite;

    //     private List<string> _groups;
    //     public IReadOnlyList<string> Groups => _groups ??= BindingGroups
    //         .Split(IsComposite ? CompositeParts[0].Binding.groups : Binding.groups)
    //         .ToList();

    //     private HashSet<string> _groupsSet;
    //     public HashSet<string> GroupsSet => _groupsSet ??= new HashSet<string>(Groups);


    //     public IEnumerable<BindingMetadata> EnumerateAllBindings()
    //     {
    //         yield return this;
    //         if (IsComposite && CompositeParts != null)
    //         {
    //             foreach (BindingMetadata part in CompositeParts)
    //             {
    //                 yield return part;
    //             }
    //         }
    //     }
    // }

    // public class BindingMetadataProcessor
    // {
    //     private const string DEVICE_SHORTNAME_PREFIX = BindingPlus.DEVICE_SHORTNAME_PREFIX;
    //     private const string DEVICE_ID_PREFIX = BindingPlus.DEVICE_ID_PREFIX;
    //     private const string CUSTOM_BINDING_GROUP = BindingPlus.CUSTOM_BINDING_GROUP;
    //     private readonly InputToolkitService _inputToolkit;
    //     private readonly InputAction _originalAction;
    //     private HashSet<string> _defaultBindingPaths;
    //     private HashSet<string> DefaultBindingPaths => _defaultBindingPaths
    //         ??= new(_originalAction.bindings.Select(b => b.effectivePath));

    //     public BindingMetadataProcessor(
    //         InputToolkitService inputToolkit,
    //         InputAction originalAction
    //     )
    //     {
    //         _inputToolkit = inputToolkit;
    //         _originalAction = originalAction;
    //     }

    //     // public BindingPlusCollection ProcessAllBindings(IReadOnlyList<InputBinding> bindings)
    //     // {
    //     //     for (int i = 0; i < bindings.Count; i++)
    //     //     {
    //     //         InputBinding binding = bindings[i];
    //     //         if (binding.isPartOfComposite)
    //     //         {
    //     //             Debug.LogError($"[KeyRebindingSetting:{_originalAction.name}] Skipping composite part binding: {binding.ToDisplayString()}");
    //     //             continue;
    //     //         }
    //     //         if (binding.isComposite)
    //     //         {
    //     //             List<BindingMetadata> compositeParts = new();
    //     //             i++;
    //     //             while (i < bindings.Count && bindings[i].isPartOfComposite)
    //     //             {
    //     //                 BindingMetadata bindingPart = ProcessSingleBinding(bindings[i]);
    //     //                 compositeParts.Add(bindingPart);
    //     //                 i++;
    //     //             }
    //     //             i--;

    //     //             var compositeBinding = new BindingMetadata
    //     //             {
    //     //                 Binding = binding,
    //     //                 CompositeParts = compositeParts,
    //     //                 Path = compositeParts[0].Path.Clone(),
    //     //             };

    //     //             foreach (BindingMetadata part in compositeParts)
    //     //             {
    //     //                 part.ParentComposite = compositeBinding;
    //     //                 if (!part.Path.MatchesControl(compositeBinding.Path.Control))
    //     //                 {
    //     //                     compositeBinding.Path.SetControlPart(null);
    //     //                     compositeBinding.Path.SetControl(null);
    //     //                     break;
    //     //                 }
    //     //             }

    //     //             yield return compositeBinding;
    //     //         }
    //     //         else
    //     //         {
    //     //             yield return ProcessSingleBinding(binding);
    //     //         }
    //     //     }
    //     // }

    //     // public BindingMetadata ProcessSingleBinding(InputBinding binding)
    //     // {
    //     //     string path = binding.effectivePath;
    //     //     return new BindingMetadata
    //     //     {
    //     //         Binding = binding,
    //     //         Path = BindingPathComponents.FromFullPath(path),
    //     //     };
    //     // }
    // }
}
