# VR Button Stiffness

A Unity VR user study on **pseudo-haptic button stiffness**: can virtual buttons feel soft, normal or hard without any physical feedback? Participants press a 3×3 grid of virtual buttons while playing a Simon Says memory game. Each group of rounds changes how far the hand must travel to push a button down, and every press is logged to CSV for analysis.

**Engine:** Unity 6 (6000.2.12f1) · **Render pipeline:** URP · **XR:** OpenXR + XR Interaction Toolkit 3 + XR Hands · **Target:** Meta Quest (Android)

Built for the Virtual Reality course at Instituto Superior Técnico (2025/26) with Francisco Salgueiro and Sofia Paiva.

## How the study works

1. **Tutorial.** Participants practise pressing the buttons and playing the game before data collection starts.
2. **Simon Says.** A sequence of buttons flashes and the participant repeats it by pressing them in order. Sequences start at 3 buttons and grow by one each round, with no repeats inside a round.
3. **Stiffness groups.** Rounds run in groups labelled A, B and C, one per stiffness level (Soft, Normal, Hard). The order can be shuffled to counterbalance learning effects. A **Next** button moves on to the next group, and the experiment ends with an "Experiment Completed!" screen.
4. **Mistakes.** A wrong press flashes every button, then the same sequence is replayed.

## Pseudo-haptic stiffness

There's no force feedback in a headset, so stiffness comes from the **control/display ratio**. `ButtonFollowVisual` follows the poke interactor along the button's press axis and divides the hand's movement by a `pushResistance` factor:

| Level | Resistance | Effect |
| --- | --- | --- |
| Soft | 1.0 | The button moves 1:1 with the finger. |
| Normal | 2.0 | The finger must travel twice as far. |
| Hard | 3.0 | The finger must travel three times as far. |

The button visually lags behind the finger, so it reads as heavier. Pokes from an angle above 45° are ignored, and the button springs back once the finger leaves.

## Data logging

`XRButtonLogger` writes one row per press to `buttonLog_XR_<timestamp>.csv` in the app's persistent data folder (`Application.persistentDataPath`, which is the app's files folder on Quest):

```
Trial,ButtonId,TimeToGoal,MaxDepthReached,PushResistance,DateTime
```

`TimeToGoal` is the time from the finger first touching the button to the press registering, and `MaxDepthReached` is how far the button went down.

## Project structure

| Path | Purpose |
| --- | --- |
| `Assets/Scenes/Tutorial.unity` | Practice scene (first in the build). |
| `Assets/Scenes/SoftButton.unity` | Main experiment scene with the 9-button grid and all three stiffness groups. |
| `Assets/Scenes/NormalButton.unity`, `StiffButton.unity` | Earlier per-stiffness prototype scenes. |
| `Assets/Prefab/NewButton.prefab` | Pokeable button with visual follow, stiffness and logging. |
| `Assets/Scripts/SimonGameManager9.cs` | Game flow: sequences, rounds, groups, errors and UI. |
| `Assets/Scripts/SimonButton.cs` | Per-button lighting, press handling and stiffness level. |
| `Assets/Scripts/ButtonFollowVisual.cs` | Poke-following button visual with the resistance factor. |
| `Assets/Scripts/NewButtonLogger.cs` | CSV logger (`XRButtonLogger`). |
| `Assets/Scripts/SceneChanger.cs` | Loads the next scene from the Next button. |
| `Assets/Scripts/MotionGainButton.cs`, `ButtonPressLogger.cs`, `VRLogger.cs`, `GrabLogger.cs` | Motion-gain and logging scripts from the first prototype. |

## Running it

1. Install **Unity 6000.2.12f1** with **Android Build Support** (including OpenJDK and the Android SDK/NDK) through Unity Hub.
2. Clone the repository and open the folder in Unity Hub (**Add → Add project from disk**). The first import downloads the packages and can take a few minutes.
3. To run on a **Meta Quest**: enable developer mode on the headset and connect it over USB. Then open **File → Build Profiles**, switch to **Android**, select the headset under **Run Device** and click **Build And Run**.
4. To try it **without a headset**, import the **XR Interaction Simulator** sample from the XR Interaction Toolkit package (**Window → Package Manager → XR Interaction Toolkit → Samples**) and press Play in the editor.

Buttons are pressed by poking them with your index finger (hand tracking) or the tip of a controller.

## Limitations

- Stiffness is only visual. The finger can still pass straight through the button, so the effect depends on how much participants trust what they see.
- Press timing is measured from first contact, so slow approaches and hesitations count towards `TimeToGoal`.
- Each run writes its own CSV on the device, so data has to be pulled off the headset (for example with `adb pull`) and merged by hand.
