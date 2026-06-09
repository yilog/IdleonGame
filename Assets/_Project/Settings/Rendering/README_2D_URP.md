# Unity 2D URP Setup

This project targets Unity 2021.3 LTS with Universal Render Pipeline 12.x and the 2D Renderer.

## Package

`Packages/manifest.json` includes:

- `com.unity.render-pipelines.universal`: `12.1.15`

## First Unity open

1. Open `E:\IdleonGame` with Unity 2021.3.37f1.
2. Let Unity restore packages.
3. Run menu item `IdleonGame > Setup > Create 2D URP Rendering Assets`.
4. Confirm these files exist:
   - `Assets/_Project/Settings/Rendering/IdleonGame_URP_2D.asset`
   - `Assets/_Project/Settings/Rendering/IdleonGame_2DRenderer.asset`

## 2D map convention

- Use Tilemap Renderer for Tilemap layers.
- Use Sprite Renderer for characters, enemies, hit effects, and portals.
- Use 2D Light components only after the 2D Renderer asset has been created and assigned.
- Keep all rendering pipeline assets under `Assets/_Project/Settings/Rendering/`.
