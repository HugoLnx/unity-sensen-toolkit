using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.iOS;
using System;

namespace SensenToolkit
{
    public enum Vector2WizardErrorCode
    {
        None,
        Canceled,
        DifferentDevice,
        DuplicateBinding,
    }
    public struct Vector2CompositionPartListeningResult
    {
        public string PartName;
        public KeyListeningResult ListeningResult;
    }
    public struct Vector2ListeningWizardResult
    {
        public BindingPlus NewBinding;
        public List<Vector2CompositionPartListeningResult> RawResults;
        public bool IsSingleBinding;
        public Vector2CompositionPartListeningResult? SingleCompositePartResult => IsSingleBinding ? RawResults[^1] : null;
        public bool IsSuccess;
    }

    public class Vector2ListeningWizard
    {
        private struct TryCompositePartListeningResult
        {
            public string PartName;
            public KeyListeningResult ListeningResult;
            public Vector2WizardErrorCode ErrorCode;
            public bool ShouldTryAgain;
            public bool IsVector2CompositePart;
            public bool IsSuccess => ErrorCode == Vector2WizardErrorCode.None
                && ListeningResult.HasListened;
        }

        private InputAction _action;
        private KeyListener _keyListener;
        private KeyRebindingOverlay _overlay;
        private RebindingMetadataProcessor _rebindingProcessor;
        private Func<string> _getActionHumanName;
        private Func<string, string> _getPartHumanName;
        private string _commonDeviceId;
        private HashSet<string> _usedPaths = new();
        private bool _alreadyUsed = false;

        public Vector2ListeningWizard(
            InputAction action,
            KeyListener keyListener,
            KeyRebindingOverlay overlay,
            RebindingMetadataProcessor rebindingProcessor,
            System.Func<string> getActionName,
            System.Func<string, string> getPartHumanName
        )
        {
            _action = action;
            _keyListener = keyListener;
            _overlay = overlay;
            _rebindingProcessor = rebindingProcessor;
            _getActionHumanName = getActionName;
            _getPartHumanName = getPartHumanName;
        }

        public async UniTask<Vector2ListeningWizardResult> CaptureComposite()
        {
            if (_alreadyUsed)
            {
                throw new System.InvalidOperationException("This Vector2ListeningWizard instance has already been used to capture a composite. Please create a new instance for each capture.");
            }
            _alreadyUsed = true;

            List<TryCompositePartListeningResult> allTryResults = new();
            foreach (string partName in InputConstants.Vector2CompositeNames)
            {
                _overlay.ShowListening($"{_getActionHumanName()}/{_getPartHumanName(partName)}");

                TryCompositePartListeningResult tryResult = await StartRebindingLoopOfPart();
                tryResult.PartName = partName;
                allTryResults.Add(tryResult);

                if (!tryResult.IsSuccess || tryResult.IsVector2CompositePart) break;

                KeyListeningResult listeningResult = tryResult.ListeningResult;
                _usedPaths.Add(listeningResult.NewPath);
            }

            TryCompositePartListeningResult latestTryResult = allTryResults[^1];

            // TODO: Instead of returning AllResults, return a single BindingPlus for the composite
            Vector2ListeningWizardResult finalResult = new()
            {
                NewBinding = CreateBindingFromTryResults(allTryResults),
                RawResults = allTryResults.ConvertAll(r => new Vector2CompositionPartListeningResult
                {
                    PartName = r.PartName,
                    ListeningResult = r.ListeningResult
                }),
                IsSingleBinding = latestTryResult.IsVector2CompositePart,
                IsSuccess = latestTryResult.IsSuccess
            };

            _overlay.Hide(finalResult.IsSuccess ? 0f : 0.15f);

            return finalResult;
        }

        private BindingPlus CreateBindingFromTryResults(List<TryCompositePartListeningResult> allTryResults)
        {
            if (allTryResults.Count == 0) return null;
            TryCompositePartListeningResult latestTryResult = allTryResults[^1];
            if (!latestTryResult.IsSuccess) return null;

            bool shouldUseSingleBinding = latestTryResult.IsVector2CompositePart;
            if (shouldUseSingleBinding)
            {
                KeyListeningResult keyResult = latestTryResult.ListeningResult;
                keyResult.NewPath = BindingPathComponents
                    .FromFullPath(keyResult.NewPath)
                    .SetControlPart(null)
                    .AsString;
                RebindingMetadata rebindingMetadata = _rebindingProcessor.ProcessKeyListeningResult(keyResult);
                return BindingPlus.Build(_action, rebindingMetadata.NewBinding);
            }

            List<InputBinding> rawNewBindingParts = new();

            foreach (TryCompositePartListeningResult partResult in allTryResults)
            {
                KeyListeningResult listeningResult = partResult.ListeningResult;
                RebindingMetadata rebindingMetadata = _rebindingProcessor.ProcessKeyListeningResult(listeningResult);
                InputBinding binding = rebindingMetadata.NewBinding;
                binding.name = partResult.PartName;
                binding.isPartOfComposite = true;
                rawNewBindingParts.Add(binding);
            }

            InputBinding rawHeadBinding = new()
            {
                path = "2DVector",
                isComposite = true,
            };

            return BindingPlus.Build(
                _action,
                rawHeadBinding,
                compositeChildren: rawNewBindingParts.ConvertAll(b => BindingPlus.Build(_action, b))
            );
        }

        private async UniTask<TryCompositePartListeningResult> StartRebindingLoopOfPart()
        {
            TryCompositePartListeningResult? tryResult = null;
            string lastPath = null;
            float lastPathTimestamp = -99999f;
            while (!tryResult.HasValue || tryResult?.ShouldTryAgain == true)
            {
                ShowErrorIfAny(tryResult?.ErrorCode);

                tryResult = await TryListenKeyForPart();
                if (!tryResult.Value.IsSuccess) continue;

                await UniTask.Delay(50, ignoreTimeScale: true);
                string newPath = tryResult.Value.ListeningResult.NewPath;
                float timeDiff = Time.unscaledTime - lastPathTimestamp;
                bool isSameAsLastKeyTooSoon = newPath == lastPath && timeDiff < 1.5f;
                if (isSameAsLastKeyTooSoon)
                {
                    tryResult = null; // Ignore and force retry
                    continue;
                }

                lastPath = newPath;
                lastPathTimestamp = Time.unscaledTime;
            }

            if (tryResult?.IsSuccess != true)
            {
                ShowErrorIfAny(tryResult?.ErrorCode);
            }

            return tryResult.Value;
        }

        private void ShowErrorIfAny(Vector2WizardErrorCode? errorCode)
        {
            if (errorCode == null || errorCode == Vector2WizardErrorCode.None) return;
            // TODO: Use localized messages
            string message = errorCode switch
            {
                Vector2WizardErrorCode.Canceled => "Action rebinding canceled.",
                Vector2WizardErrorCode.DifferentDevice => "Key/button doesn't belong to the same device as the previous key/button.",
                Vector2WizardErrorCode.DuplicateBinding => "This key/button was used already.",
                _ => "An unknown error occurred."
            };
            _overlay.ShowError(message);
        }

        private async UniTask<TryCompositePartListeningResult> TryListenKeyForPart()
        {
            KeyListeningResult rawResult = await _keyListener.ListenToKey(action: _action);

            TryCompositePartListeningResult tryResult = new()
            {
                ListeningResult = rawResult,
                ErrorCode = Vector2WizardErrorCode.None,
                ShouldTryAgain = false
            };

            if (!rawResult.HasListened)
            {
                tryResult.ErrorCode = Vector2WizardErrorCode.Canceled;
                tryResult.ShouldTryAgain = false;
                return tryResult;
            }

            RebindingMetadata rebindingMetadata = _rebindingProcessor.ProcessKeyListeningResult(rawResult);
            InputBinding newBinding = rebindingMetadata.NewBinding;
            newBinding.isPartOfComposite = true;
            rebindingMetadata.NewBinding = newBinding;
            var bindingMetadata = BindingPlus.Build(_action, newBinding);

            _overlay.UpdateKeyName(bindingMetadata.DisplayString);

            BindingPathComponents path = bindingMetadata.Path;

            if (_commonDeviceId == null)
            {
                _commonDeviceId = bindingMetadata.DeviceId;
            }

            if (bindingMetadata.DeviceId != _commonDeviceId)
            {
                tryResult.ErrorCode = Vector2WizardErrorCode.DifferentDevice;
                tryResult.ShouldTryAgain = true;
                return tryResult;
            }

            if (_usedPaths.Contains(path.AsString))
            {
                tryResult.ErrorCode = Vector2WizardErrorCode.DuplicateBinding;
                tryResult.ShouldTryAgain = true;
                return tryResult;
            }

            if (path.IsVector2CompositePart)
            {
                tryResult.IsVector2CompositePart = true;
            }

            return tryResult;
        }
    }
}
