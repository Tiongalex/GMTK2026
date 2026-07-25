# Unity Manual Integration Guide - Talisman Minigame

Follow these step-by-step instructions in the Unity Editor to wire up the new weathered textures and configurables to your UI.

---

### Step 1: Configure Texture Import Settings
1. In the **Project Window**, navigate to `Assets/TalismanTextures/`.
2. Select the three new textures:
   - `TalismanBase.jpg`
   - `TalismanMarkings.png`
   - `MagicSpark.png`
3. In the **Inspector Window**, change their **Texture Type** to **Sprite (2D and UI)**.
4. Click **Apply** at the bottom of the Inspector.

---

### Step 2: Update the Talisman Background Paper
1. In the **Hierarchy Window**, locate your Talisman canvas. Select the background panel (the RectTransform referenced in `talismanPaper` on your script).
2. On its **Image** component:
   - Drag `TalismanBase` (from `Assets/TalismanTextures/`) into the **Source Image** slot.
   - Set **Color** to pure white (`#FFFFFF`, opacity `255`).
   - Check the **Preserve Aspect** box to prevent stretching.

---

### Step 3: Add the Mystic Calligraphy (Markings) Layer
1. Right-click the background panel GameObject in the Hierarchy and select **UI -> Image** to create a child image.
2. Name this new child object `TalismanMarkings`.
3. Select `TalismanMarkings` and configure its **Image** component:
   - Drag `TalismanMarkings` (from `Assets/TalismanTextures/`) into the **Source Image** slot.
   - Check **Preserve Aspect**.
4. Adjust its **RectTransform** to fit nicely over the yellow base paper (e.g., set anchors to stretch, or adjust width/height to match the design).

---

### Step 4: Stylize the Magic Spark (Target Point)
1. In the Hierarchy, locate and select the `movingTargetPoint` GameObject.
2. On its **Image** component (or the child `targetPointVisual` if separate):
   - Drag `MagicSpark` (from `Assets/TalismanTextures/`) into the **Source Image** slot.

---

### Step 5: Wire up references on the TalismanMiniGame Script
1. Select the GameObject containing the `TalismanMiniGame` script.
2. In the script component inspector, configure the new properties:
   - Drag the `TalismanMarkings` GameObject you created in Step 3 into the **Talisman Markings** slot.
   - Configure **Tracking Color** (e.g., a glowing Cyan/Blue like `#00C8FF`).
   - Configure **Searching Color** (e.g., a warning Red like `#FF3333`).
   - Adjust the **Visual Juice Settings** (Pulse Speed, Min/Max Scale, Min/Max Alpha) to your liking.

---

### Step 6: Test
- Enter Play Mode in Unity.
- Trigger the talisman seal minigame.
- Check that the spark pulses when tracking and that the red brush calligraphy glows brighter as you progress!
