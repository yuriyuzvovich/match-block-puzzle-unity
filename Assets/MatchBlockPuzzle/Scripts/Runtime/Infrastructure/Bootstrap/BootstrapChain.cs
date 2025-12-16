using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MatchPuzzle.Core.Interfaces;

namespace MatchPuzzle.Infrastructure.Bootstrap
{
    /// <summary>
    /// Executes bootstrap steps in dependency order across PreRun, Run, and AfterRun phases.
    /// </summary>
    public sealed class BootstrapChain
    {
        private readonly List<IBootstrapStep> _steps = new List<IBootstrapStep>();
        private readonly List<IBootstrapStep> _orderedSteps = new List<IBootstrapStep>();

        public BootstrapChain AddStep(IBootstrapStep step)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));

            foreach (var existing in _steps)
            {
                if (existing.Id == step.Id)
                {
                    throw new InvalidOperationException($"Bootstrap step with id '{step.Id}' is already registered.");
                }
            }

            _steps.Add(step);
            return this;
        }

        public async UniTask RunAsync(ServiceContainer services, CancellationToken cancellationToken)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            _orderedSteps.Clear();
            _orderedSteps.AddRange(TopologicallySort(_steps));

            // PreRun
            foreach (var step in _orderedSteps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await step.PreRunAsync(services, cancellationToken);
            }

            // Run
            foreach (var step in _orderedSteps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await step.RunAsync(services, cancellationToken);
            }

            // AfterRun
            foreach (var step in _orderedSteps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await step.PostRunAsync(services, cancellationToken);
            }
        }

        public void Cleanup(ServiceContainer services)
        {
            for (int i = _orderedSteps.Count - 1; i >= 0; i--)
            {
                if (_orderedSteps[i] is ICleanupStep cleanupStep)
                {
                    cleanupStep.Cleanup(services);
                }
            }
        }

        private static IEnumerable<IBootstrapStep> TopologicallySort(IReadOnlyList<IBootstrapStep> steps)
        {
            var ordered = new List<IBootstrapStep>();
            var visitState = new Dictionary<string, int>(StringComparer.Ordinal);
            var stepLookup = new Dictionary<string, IBootstrapStep>(StringComparer.Ordinal);

            foreach (var step in steps)
            {
                stepLookup[step.Id] = step;
            }

            // Perform DFS for each step to ensure all are visited
            foreach (var step in steps)
            {
                Visit(step, stepLookup, visitState, ordered);
            }

            return ordered;
        }

        // See: https://en.wikipedia.org/wiki/Topological_sorting#Depth-first_search
        // - If a step is unvisited, we start visiting it (mark as 1)
        // - We recursively visit all its dependencies
        // - If we encounter a step that is already being visited (marked as 1), we have a cycle
        // - Once all dependencies are visited, we mark the step as visited (mark as 2) and add it to the ordered list
        // This ensures that all dependencies are processed before the step itself
        // and detects cycles in the dependency graph.
        private static void Visit(
            IBootstrapStep step,
            Dictionary<string, IBootstrapStep> lookup,
            Dictionary<string, int> visitState,
            List<IBootstrapStep> orderedSteps)
        {
            if (visitState.TryGetValue(step.Id, out var state))
            {
                if (state == 1)
                {
                    throw new InvalidOperationException($"Detected cycle while ordering bootstrap steps at '{step.Id}'.");
                }

                if (state == 2)
                {
                    return;
                }
            }

            visitState[step.Id] = 1; // visiting

            if (step.DependsOn != null)
            {
                foreach (var dependencyId in step.DependsOn)
                {
                    if (!lookup.TryGetValue(dependencyId, out var dependency))
                    {
                        throw new InvalidOperationException($"Step '{step.Id}' depends on missing step '{dependencyId}'.");
                    }

                    Visit(dependency, lookup, visitState, orderedSteps);
                }
            }

            visitState[step.Id] = 2; // visited
            orderedSteps.Add(step);
        }
    }
}
