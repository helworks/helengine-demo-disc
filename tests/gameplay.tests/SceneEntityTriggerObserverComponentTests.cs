using System.Runtime.CompilerServices;
using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies gameplay-side trigger observation resolves enter, stay, and exit state deterministically.
    /// </summary>
    public sealed class SceneEntityTriggerObserverComponentTests {
        [Fact]
        public void Resolve_observed_state_sets_enter_and_inside_for_matching_enter_event() {
            Entity triggerEntity = CreateOpaqueEntity();
            Entity targetEntity = CreateOpaqueEntity();

            helengine.SceneEntityTriggerObserverComponent.ResolveObservedState(
                new[] {
                    new TriggerEvent3D(TriggerEventKind3D.Enter, triggerEntity, targetEntity)
                },
                triggerEntity,
                targetEntity,
                false,
                out bool isTriggered,
                out bool wasEnteredThisFrame,
                out bool wasExitedThisFrame);

            Assert.True(isTriggered);
            Assert.True(wasEnteredThisFrame);
            Assert.False(wasExitedThisFrame);
        }

        [Fact]
        public void Resolve_observed_state_ignores_unrelated_pairs_and_clears_inside_for_matching_exit_event() {
            Entity triggerEntity = CreateOpaqueEntity();
            Entity targetEntity = CreateOpaqueEntity();
            Entity otherTriggerEntity = CreateOpaqueEntity();
            Entity otherTargetEntity = CreateOpaqueEntity();

            helengine.SceneEntityTriggerObserverComponent.ResolveObservedState(
                new[] {
                    new TriggerEvent3D(TriggerEventKind3D.Stay, otherTriggerEntity, otherTargetEntity),
                    new TriggerEvent3D(TriggerEventKind3D.Exit, triggerEntity, targetEntity)
                },
                triggerEntity,
                targetEntity,
                true,
                out bool isTriggered,
                out bool wasEnteredThisFrame,
                out bool wasExitedThisFrame);

            Assert.False(isTriggered);
            Assert.False(wasEnteredThisFrame);
            Assert.True(wasExitedThisFrame);
        }

        static Entity CreateOpaqueEntity() {
            return (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        }
    }
}
